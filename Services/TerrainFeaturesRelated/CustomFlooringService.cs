using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Audio;
using StardewValley.GameData.FloorsAndPaths;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;
using System.Reflection.Emit;
using Ts_Core.Models.ContentPatcherRelated;
using Ts_Core.Services.ContentPatcherRelated;

namespace Ts_Core.Services.TerrainFeaturesRelated
{
    /// <summary>
    /// 通常のFlooringを利用し、設置前に左クリックで次の画像を選択します。設定キーで前の画像への切り替えと収納も行います。
    /// 設置後はVariantの固定IDをFlooring.modDataへ保存し、隣接状態に依存せず固定描画します。
    /// </summary>
    internal static class CustomFlooringService
    {
        private const string VariantKey = "Tikamin557.TsCore/FlooringVariant";
        private const string RandomPlacementKey = "Tikamin557.TsCore/FlooringRandomPlacement";
        private const string AnimationOffsetKey = "Tikamin557.TsCore/FlooringAnimationOffset";
        private const string SilentFootstepCue = "Tikamin557.TsCore_SilentFootstep";
        private static ISoundBank? registeredSoundBank;
        private static SoundEffect? silentFootstepEffect;
        private static readonly Dictionary<string, int> SelectedVariants = new(StringComparer.OrdinalIgnoreCase);
        private static readonly System.Reflection.MethodInfo RadialDebrisMethod = AccessTools.Method(typeof(Game1), nameof(Game1.createRadialDebris),
            new[] { typeof(GameLocation), typeof(int), typeof(int), typeof(int), typeof(int), typeof(bool), typeof(int), typeof(bool), typeof(Color?) });
        private static readonly System.Reflection.MethodInfo CustomRadialDebrisMethod = AccessTools.Method(typeof(CustomFlooringService), nameof(CreateRemovalDebris));
        private static readonly System.Reflection.MethodInfo PlaySoundMethod = AccessTools.Method(typeof(GameLocation), nameof(GameLocation.playSound),
            new[] { typeof(string), typeof(Vector2?), typeof(int?), typeof(SoundContext) });
        private static readonly System.Reflection.MethodInfo PlacementSoundWrapper = AccessTools.Method(typeof(CustomFlooringService), nameof(PlayPlacementSound));
        private static readonly System.Reflection.MethodInfo RemovalSoundWrapper = AccessTools.Method(typeof(CustomFlooringService), nameof(PlayRemovalSound));
        // アセットのインスタンスが変わった時だけ検索辞書を再構築します。
        private static Dictionary<string, CustomFlooringModel>? cachedDefinitions;
        private static readonly Dictionary<string, CachedFlooring> FlooringCache = new(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<string, FloorPathData>? cachedFloorPaths;
        private static readonly Dictionary<string, string> FloorIdByItemId = new(StringComparer.OrdinalIgnoreCase);

        private sealed class CachedFlooring
        {
            internal CustomFlooringModel Definition = null!;
            internal Dictionary<string, CustomFlooringVariant> VariantsById = new(StringComparer.OrdinalIgnoreCase);
        }

        private static readonly Dictionary<CustomFlooringVariant, (long Cycle, long GroupOffset)> AnimationCache = new();
        private static IModHelper helper = null!;
        private static IMonitor monitor = null!;
        private static bool showFlooringMarkers;
        private static Rectangle? identificationPreviewAnchor;
        private static Texture2D? markerPixel;

        //----------------------------------------
        // 初期化
        //----------------------------------------

        internal static void Initialize(IModHelper modHelper, IMonitor modMonitor, Harmony harmony)
        {
            helper = modHelper;
            monitor = modMonitor;
            helper.Events.Input.ButtonPressed += OnButtonPressed;
            helper.Events.Display.RenderedWorld += OnRenderedWorld;
            helper.Events.Display.RenderedHud += OnRenderedHud;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            helper.Events.GameLoop.ReturnedToTitle += (_, _) =>
            {
                SelectedVariants.Clear();
                showFlooringMarkers = false;
                markerPixel?.Dispose();
                markerPixel = null;
                ClearDefinitionCache();
                ClearFloorPathCache();
            };
            helper.Events.Content.AssetsInvalidated += OnAssetsInvalidated;
            harmony.Patch(
                AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.placementAction),
                    new[] { typeof(GameLocation), typeof(int), typeof(int), typeof(Farmer) }),
                prefix: new HarmonyMethod(typeof(CustomFlooringService), nameof(BeforePlacement)),
                postfix: new HarmonyMethod(typeof(CustomFlooringService), nameof(AfterPlacement)),
                transpiler: new HarmonyMethod(typeof(CustomFlooringService), nameof(ReplacePlacementSoundCall)));
            harmony.Patch(
                AccessTools.Method(typeof(Flooring), nameof(Flooring.performToolAction),
                    new[] { typeof(Tool), typeof(int), typeof(Vector2) }),
                transpiler: new HarmonyMethod(typeof(CustomFlooringService), nameof(ReplaceRemovalCalls)));
            harmony.Patch(
                AccessTools.Method(typeof(Flooring), nameof(Flooring.getFootstepSound)),
                postfix: new HarmonyMethod(typeof(CustomFlooringService), nameof(AfterGetFootstepSound)));
            harmony.Patch(
                AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.drawPlacementBounds),
                    new[] { typeof(SpriteBatch), typeof(GameLocation) }),
                prefix: new HarmonyMethod(typeof(CustomFlooringService), nameof(BeforeDrawPlacementBounds)));
            harmony.Patch(
                AccessTools.Method(typeof(Flooring), nameof(Flooring.draw), new[] { typeof(SpriteBatch) }),
                prefix: new HarmonyMethod(typeof(CustomFlooringService), nameof(BeforeDraw)));
        }

        /// <summary>
        /// 無音指定の足音用に、実際に再生可能な無音キューを登録します。
        /// ゲームのSoundBankが初期化・再生成された場合にも登録を試みます。
        /// </summary>
        private static void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            if (Game1.soundBank is not SoundBankWrapper bank || ReferenceEquals(registeredSoundBank, bank))
                return;

            try
            {
                if (!bank.Exists(SilentFootstepCue))
                {
                    // 16bit PCM / 22050Hz / モノラル、約0.1秒の無音。
                    silentFootstepEffect ??= new SoundEffect(new byte[4410], 22050, AudioChannels.Mono);
                    bank.AddCue(new CueDefinition(SilentFootstepCue, silentFootstepEffect, 1, false, false));
                }
                registeredSoundBank = bank;
            }
            catch (Exception ex)
            {
                monitor.Log($"Failed to register silent flooring footstep cue: {ex.Message}", LogLevel.Warn);
            }
        }

        //----------------------------------------
        // 定義と選択
        //----------------------------------------

        /// <summary>Content Patcher等によるアセットの無効化時に、対応する検索情報を破棄します。</summary>
        private static void OnAssetsInvalidated(object? sender, AssetsInvalidatedEventArgs e)
        {
            foreach (var name in e.NamesWithoutLocale)
            {
                if (name.IsEquivalentTo(CustomFlooringDataService.AssetName))
                    ClearDefinitionCache();
                if (name.IsEquivalentTo("Data/FloorsAndPaths"))
                    ClearFloorPathCache();
            }
        }

        private static void ClearDefinitionCache()
        {
            cachedDefinitions = null;
            FlooringCache.Clear();
            AnimationCache.Clear();
        }

        private static void ClearFloorPathCache()
        {
            cachedFloorPaths = null;
            FloorIdByItemId.Clear();
        }

        /// <summary>定義の読み込み時に一度だけID検証・Variant検索辞書・アニメーション周期を準備します。</summary>
        private static bool TryGetCachedFlooring(string floorId, out CachedFlooring cached)
        {
            cached = null!;
            try
            {
                var all = Game1.content.Load<Dictionary<string, CustomFlooringModel>>(CustomFlooringDataService.AssetName);
                if (!ReferenceEquals(cachedDefinitions, all))
                {
                    ClearDefinitionCache();
                    cachedDefinitions = all;
                    foreach (var entry in all)
                    {
                        var definition = entry.Value;
                        if (definition?.Variants == null || definition.Variants.Count == 0)
                            continue;
                        var byId = new Dictionary<string, CustomFlooringVariant>(StringComparer.OrdinalIgnoreCase);
                        bool valid = true;
                        foreach (var variant in definition.Variants)
                        {
                            if (variant == null || string.IsNullOrWhiteSpace(variant.Id) || !byId.TryAdd(variant.Id, variant))
                            {
                                valid = false;
                                break;
                            }
                        }
                        if (!valid)
                        {
                            monitor.Log($"Custom flooring '{entry.Key}' has missing or duplicate variant IDs.", LogLevel.Warn);
                            continue;
                        }
                        FlooringCache[entry.Key] = new CachedFlooring { Definition = definition, VariantsById = byId };
                        foreach (var variant in definition.Variants)
                        {
                            long cycle = 0;
                            if (variant.Animation != null)
                                foreach (var frame in variant.Animation)
                                    cycle += Math.Max(1, frame.Duration);
                            long offset = 0;
                            if (cycle > 0 && !string.IsNullOrWhiteSpace(variant.AnimationId))
                            {
                                uint hash = 2166136261;
                                foreach (char c in variant.AnimationId.ToUpperInvariant())
                                    hash = (hash ^ c) * 16777619;
                                offset = hash % cycle;
                            }
                            AnimationCache[variant] = (cycle, offset);
                        }
                    }
                }
                return FlooringCache.TryGetValue(floorId, out cached!);
            }
            catch (Exception ex)
            {
                monitor.Log($"Failed to load custom flooring definitions: {ex.Message}", LogLevel.Warn);
                return false;
            }
        }

        private static bool TryGetDefinition(string floorId, out CustomFlooringModel definition)
        {
            definition = null!;
            if (!TryGetCachedFlooring(floorId, out var cached))
                return false;
            definition = cached.Definition;
            return true;
        }

        private static bool TryGetFloorId(StardewValley.Object item, out string floorId)
        {
            floorId = "";
            if (!item.IsFloorPathItem())
                return false;
            try
            {
                var data = Game1.content.Load<Dictionary<string, FloorPathData>>("Data/FloorsAndPaths");
                if (!ReferenceEquals(cachedFloorPaths, data))
                {
                    ClearFloorPathCache();
                    cachedFloorPaths = data;
                    foreach (var entry in data)
                    {
                        if (!string.IsNullOrEmpty(entry.Value?.ItemId))
                            FloorIdByItemId.TryAdd(entry.Value.ItemId, entry.Key);
                    }
                }
                return FloorIdByItemId.TryGetValue(item.ItemId, out floorId!);
            }
            catch (Exception ex)
            {
                monitor.Log($"Failed to resolve flooring item: {ex.Message}", LogLevel.Warn);
                return false;
            }
        }

        private static void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady || Game1.activeClickableMenu != null)
                return;

            if (ModEntry.Config.CustomFlooringIdentifyKey.JustPressed())
            {
                showFlooringMarkers = !showFlooringMarkers;
                helper.Input.Suppress(e.Button);
                return;
            }

            Farmer player = Game1.player;
            // 収納中はCurrentItemがnullになるため、選択スロットから床材を判定します。
            int slot = player.CurrentToolIndex;
            if (slot < 0 || slot >= player.Items.Count
                || player.Items[slot] is not StardewValley.Object item
                || !TryGetFloorId(item, out string floorId)
                || !TryGetDefinition(floorId, out var definition))
                return;

            // バニラの収納状態を切り替えます。選択スロットやアイテムは変更しません。
            if (ModEntry.Config.CustomFlooringStowKey.JustPressed())
            {
                bool stow = !player.netItemStowed.Value;
                player.netItemStowed.Value = stow;
                player.UpdateItemStow();
                Game1.playSound(stow ? "stoneStep" : "toolSwap");
                helper.Input.Suppress(e.Button);
                return;
            }

            // 収納中の通常クリックはバニラに任せ、テクスチャは変更しません。
            if (player.netItemStowed.Value)
                return;

            // BuilderモードではHUDのスロットをクリックしてVariantを直接選択します。
            // この判定を通常の左クリック変更より先に行い、ワールドへのクリックも抑制します。
            if (e.Button == SButton.MouseLeft && definition.VariantPreviewEnabled
                && IsSelectablePreview(definition)
                && TryGetSelectableLayout(definition, floorId, out var layout))
            {
                int mouseX = Game1.getMouseX(true);
                int mouseY = Game1.getMouseY(true);
                if (layout.Bounds.Contains(mouseX, mouseY))
                {
                    int[] ordered = GetSelectablePreviewSlots(definition);
                    for (int i = 0; i < ordered.Length; i++)
                    {
                        if (!GetSelectableSlot(layout, i).Contains(mouseX, mouseY))
                            continue;

                        int index = ordered[i];
                        if (index < 0)
                            break;

                        if (SelectedVariants.GetValueOrDefault(floorId) != index)
                        {
                            SelectedVariants[floorId] = index;
                            if (!string.IsNullOrWhiteSpace(definition.VariantChangeSound))
                                Game1.currentLocation.playSound(definition.VariantChangeSound);
                        }
                        break;
                    }
                    helper.Input.Suppress(e.Button);
                    return;
                }
            }

            // Carouselのプレビュー内では、クリックしたスロットのVariantを直接選択します。
            // 通常のワールド左クリックによる切り替えは維持します。
            if (e.Button == SButton.MouseLeft && definition.VariantPreviewEnabled
                && !IsSelectablePreview(definition) && definition.Variants.Count > 0)
            {
                int previewCount = definition.VariantPreviewCount is 3 or 5 or 7 or 9
                    ? definition.VariantPreviewCount : 5;
                float previewScale = 1f + Math.Clamp(definition.VariantPreviewScale, 0, 5) * 0.2f;
                int previewSlot = (int)MathF.Round(64 * previewScale);
                int previewGap = (int)MathF.Round(4 * previewScale);
                int previewPadding = (int)MathF.Round(8 * previewScale);
                int previewWidth = previewPadding * 2 + previewCount * previewSlot + (previewCount - 1) * previewGap;
                int previewHeight = previewPadding * 2 + previewSlot;
                int previewX = Math.Clamp(definition.VariantPreviewOffsetX * 10, 0,
                    Math.Max(0, Game1.uiViewport.Width - previewWidth));
                Rectangle available = GetPreviewAvailableArea();
                int previewY = Math.Clamp(available.Top + definition.VariantPreviewOffsetY * 10,
                    available.Top, Math.Max(available.Top, available.Bottom - previewHeight
                        - GetPreviewFooterHeight(definition, floorId, previewScale)));
                int mouseX = Game1.getMouseX(true);
                int mouseY = Game1.getMouseY(true);
                if (new Rectangle(previewX, previewY, previewWidth, previewHeight).Contains(mouseX, mouseY))
                {
                    int selected = SelectedVariants.GetValueOrDefault(floorId) % definition.Variants.Count;
                    int middle = previewCount / 2;
                    for (int i = 0; i < previewCount; i++)
                    {
                        Rectangle previewSlotBounds = new(
                            previewX + previewPadding + i * (previewSlot + previewGap),
                            previewY + previewPadding, previewSlot, previewSlot);
                        if (!previewSlotBounds.Contains(mouseX, mouseY))
                            continue;

                        int index = (selected + i - middle + definition.Variants.Count * previewCount)
                            % definition.Variants.Count;
                        if (selected != index)
                        {
                            SelectedVariants[floorId] = index;
                            if (!string.IsNullOrWhiteSpace(definition.VariantChangeSound))
                                Game1.currentLocation.playSound(definition.VariantChangeSound);
                        }
                        break;
                    }
                    helper.Input.Suppress(e.Button);
                    return;
                }
            }

            int direction;
            if (ModEntry.Config.CustomFlooringPreviousVariantKey.JustPressed())
                direction = -1;
            else if (e.Button == SButton.MouseLeft)
            {
                // UI座標でツールバーの各スロットを判定し、アイテム選択のクリックを抑制しません。
                // TsCore独自HUDはonScreenMenusに含まれないため、パネル自身の領域判定を利用します。
                if (Ts_Core.Services.ShortcutPanelRelated.ShortcutPanelService.ContainsScreenPoint(e.Cursor.ScreenPixels))
                    return;

                int mouseX = Game1.getMouseX(true);
                int mouseY = Game1.getMouseY(true);
                if (Game1.onScreenMenus.Any(menu => menu is Toolbar toolbar
                    ? toolbar.buttons.Any(button => button.containsPoint(mouseX, mouseY))
                    : menu.isWithinBounds(mouseX, mouseY)))
                    return;

                direction = 1;
            }
            else
                return;

            int current = SelectedVariants.GetValueOrDefault(floorId);
            if (IsSelectablePreview(definition))
            {
                // 空欄を除いたBuilderの表示順で前後へ移動します。
                int[] ordered = GetSelectableVariantIndices(definition);
                int position = Array.IndexOf(ordered, current);
                int nextPosition = (position + direction + ordered.Length) % ordered.Length;
                SelectedVariants[floorId] = ordered[nextPosition];
            }
            else
                SelectedVariants[floorId] = (current + direction + definition.Variants.Count) % definition.Variants.Count;
            if (!string.IsNullOrWhiteSpace(definition.VariantChangeSound))
                Game1.currentLocation.playSound(definition.VariantChangeSound);
            helper.Input.Suppress(e.Button);
        }

        //----------------------------------------
        // Builder Variantプレビュー
        //----------------------------------------

        /// <summary>選択モードかどうかを判定します。不明な値は従来モードとして扱います。</summary>
        private static bool IsSelectablePreview(CustomFlooringModel definition)
            => string.Equals(definition.VariantPreviewMode, "Builder", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// ツールバーの上下位置から、カスタム床材HUDを配置できる領域を求めます。
        /// 画面下部・上部へのツールバー移動の両方に対応します。
        /// </summary>
        private static Rectangle GetPreviewAvailableArea()
        {
            int top = 0;
            int bottom = Game1.uiViewport.Height;
            Toolbar? toolbar = Game1.onScreenMenus.OfType<Toolbar>().FirstOrDefault();
            if (toolbar != null && toolbar.buttons.Count > 0)
            {
                int toolbarTop = toolbar.buttons.Min(button => button.bounds.Top);
                int toolbarBottom = toolbar.buttons.Max(button => button.bounds.Bottom);
                if (toolbarTop < Game1.uiViewport.Height / 2)
                    top = Math.Min(bottom, toolbarBottom + 8);
                else
                    bottom = Math.Max(top, toolbarTop - 8);
            }
            return new Rectangle(0, top, Game1.uiViewport.Width, Math.Max(0, bottom - top));
        }

        /// <summary>名前欄と識別表示を含めた、プレビュー下部の必要高さを計算します。</summary>
        private static int GetPreviewFooterHeight(CustomFlooringModel definition, string floorId, float scale)
        {
            int result = 0;
            if (definition.VariantPreviewShowName)
            {
                int selected = SelectedVariants.GetValueOrDefault(floorId) % definition.Variants.Count;
                string? name = definition.Variants[selected].DisplayName;
                if (string.IsNullOrWhiteSpace(name))
                    name = helper.Translation.Get("customFlooring.variantPreview.noDisplayName");
                result += (int)MathF.Round(4 * scale)
                    + (int)MathF.Ceiling(Game1.smallFont.MeasureString(name).Y * scale)
                    + (int)MathF.Round(24 * scale);
            }
            if (showFlooringMarkers)
            {
                string message = helper.Translation.Get("customFlooring.identification.active");
                result += 4 + (int)MathF.Ceiling(Game1.smallFont.MeasureString(message).Y) + 24;
            }
            return result;
        }

        private readonly record struct SelectableLayout(Rectangle Bounds, int Columns, int SlotSize, int Gap, int Padding, float Scale);

        /// <summary>描画とクリック判定で同一の位置・大きさを使用します。</summary>
        private static bool TryGetSelectableLayout(CustomFlooringModel definition, string floorId, out SelectableLayout layout)
        {
            layout = default;
            if (definition.Variants.Count == 0)
                return false;

            int slotCount = GetSelectablePreviewSlots(definition).Length;
            int columns = Math.Min(Math.Clamp(definition.VariantPreviewColumns, 1, 10), slotCount);
            int rows = (slotCount + columns - 1) / columns;
            float scale = 1f + Math.Clamp(definition.VariantPreviewScale, 0, 5) * 0.2f;
            int slotSize, gap, padding, width, height;

            Rectangle available = GetPreviewAvailableArea();
            // 一覧だけでなく、名前欄と識別表示の高さも含めて縮小します。
            do
            {
                slotSize = Math.Max(1, (int)MathF.Round(64 * scale));
                gap = Math.Max(1, (int)MathF.Round(4 * scale));
                padding = Math.Max(1, (int)MathF.Round(8 * scale));
                width = padding * 2 + columns * slotSize + (columns - 1) * gap;
                height = padding * 2 + rows * slotSize + (rows - 1) * gap;
                if ((width <= available.Width && height + GetPreviewFooterHeight(definition, floorId, scale) <= available.Height)
                    || scale <= 0.10f)
                    break;
                scale = Math.Max(0.10f, scale - 0.05f);
            } while (true);

            int x = Math.Clamp(definition.VariantPreviewOffsetX * 10, 0, Math.Max(0, available.Width - width));
            int y = Math.Clamp(available.Top + definition.VariantPreviewOffsetY * 10,
                available.Top, Math.Max(available.Top, available.Bottom - height - GetPreviewFooterHeight(definition, floorId, scale)));
            layout = new(new Rectangle(x, y, width, height), columns, slotSize, gap, padding, scale);
            return true;
        }

        private static Rectangle GetSelectableSlot(SelectableLayout layout, int index)
            => new(layout.Bounds.X + layout.Padding + (index % layout.Columns) * (layout.SlotSize + layout.Gap),
                layout.Bounds.Y + layout.Padding + (index / layout.Columns) * (layout.SlotSize + layout.Gap),
                layout.SlotSize, layout.SlotSize);

        /// <summary>番号指定のあるVariantを昇順に並べ、未指定は元の定義順で後ろに並べます。</summary>
        private static int[] GetSelectableVariantIndices(CustomFlooringModel definition)
            => Enumerable.Range(0, definition.Variants.Count)
                .OrderBy(i => definition.Variants[i].SelectablePreviewOrder.HasValue ? 0 : 1)
                .ThenBy(i => definition.Variants[i].SelectablePreviewOrder ?? 0)
                .ThenBy(i => i)
                .ToArray();

        /// <summary>
        /// 指定した1始まりの位置を空欄にした、表示用のスロット一覧を作ります。
        /// -1は空欄を表し、Variantの内部順序や選択インデックスには影響しません。
        /// </summary>
        private static int[] GetSelectablePreviewSlots(CustomFlooringModel definition)
        {
            int[] ordered = GetSelectableVariantIndices(definition);
            if (string.IsNullOrWhiteSpace(definition.SelectablePreviewEmptyOrders))
                return ordered;

            var emptyPositions = new HashSet<int>();
            foreach (string part in definition.SelectablePreviewEmptyOrders.Split(','))
            {
                if (int.TryParse(part.Trim(), out int position) && position > 0)
                    emptyPositions.Add(position);
            }

            if (emptyPositions.Count == 0)
                return ordered;

            // 不正な巨大番号によって極端に大きなUIが生成されるのを防ぎます。
            int maxPosition = Math.Min(emptyPositions.Max(), ordered.Length + 100);
            emptyPositions.RemoveWhere(position => position > maxPosition);

            var slots = new List<int>(ordered.Length + emptyPositions.Count);
            int nextVariant = 0;
            for (int position = 1; nextVariant < ordered.Length || position <= maxPosition; position++)
            {
                if (emptyPositions.Contains(position))
                    slots.Add(-1);
                else if (nextVariant < ordered.Length)
                    slots.Add(ordered[nextVariant++]);
            }

            return slots.ToArray();
        }

        /// <summary>全Variantを一覧表示し、選択中のVariantを赤枠で示します。</summary>
        private static void DrawSelectablePreview(SpriteBatch spriteBatch, CustomFlooringModel definition, string floorId)
        {
            if (!TryGetSelectableLayout(definition, floorId, out var layout))
                return;

            Rectangle panel = layout.Bounds;
            identificationPreviewAnchor = panel;
            IClickableMenu.drawTextureBox(spriteBatch, Game1.menuTexture,
                new Rectangle(0, 256, 60, 60), panel.X, panel.Y, panel.Width, panel.Height,
                Color.White, 1f, drawShadow: false);

            int selected = SelectedVariants.GetValueOrDefault(floorId) % definition.Variants.Count;
            int[] ordered = GetSelectablePreviewSlots(definition);
            for (int i = 0; i < ordered.Length; i++)
            {
                int index = ordered[i];
                if (index < 0)
                    continue; // 空欄は背景やクリック対象を描画しません。

                var variant = definition.Variants[index];
                Rectangle slot = GetSelectableSlot(layout, i);
                spriteBatch.Draw(Game1.staminaRect, slot, new Color(235, 226, 218));
                int slotBorder = Math.Max(1, (int)MathF.Round(layout.Scale));
                Color edge = new(201, 190, 184);
                spriteBatch.Draw(Game1.staminaRect, new Rectangle(slot.X, slot.Y, slot.Width, slotBorder), edge);
                spriteBatch.Draw(Game1.staminaRect, new Rectangle(slot.X, slot.Bottom - slotBorder, slot.Width, slotBorder), edge);
                spriteBatch.Draw(Game1.staminaRect, new Rectangle(slot.X, slot.Y, slotBorder, slot.Height), edge);
                spriteBatch.Draw(Game1.staminaRect, new Rectangle(slot.Right - slotBorder, slot.Y, slotBorder, slot.Height), edge);

                var randomImage = GetRandomTexture(variant, floorId);
                Texture2D texture = Game1.content.Load<Texture2D>(randomImage.Texture);
                Rectangle source = randomImage.Source;
                if (source.X >= 0 && source.Y >= 0 && source.Right <= texture.Width && source.Bottom <= texture.Height)
                {
                    int inset = Math.Max(1, slot.Width / 8);
                    spriteBatch.Draw(texture,
                        new Rectangle(slot.X + inset, slot.Y + inset, slot.Width - inset * 2, slot.Height - inset * 2),
                        source, Color.White);
                }

                if (index == selected)
                {
                    int border = Math.Max(2, (int)MathF.Round(4 * layout.Scale));
                    Color red = new(225, 40, 35);
                    spriteBatch.Draw(Game1.staminaRect, new Rectangle(slot.X, slot.Y, slot.Width, border), red);
                    spriteBatch.Draw(Game1.staminaRect, new Rectangle(slot.X, slot.Bottom - border, slot.Width, border), red);
                    spriteBatch.Draw(Game1.staminaRect, new Rectangle(slot.X, slot.Y, border, slot.Height), red);
                    spriteBatch.Draw(Game1.staminaRect, new Rectangle(slot.Right - border, slot.Y, border, slot.Height), red);
                }
            }

            if (definition.VariantPreviewShowName)
            {
                string? name = definition.Variants[selected].DisplayName;
                if (string.IsNullOrWhiteSpace(name))
                    name = helper.Translation.Get("customFlooring.variantPreview.noDisplayName");

                Vector2 measured = Game1.smallFont.MeasureString(name) * layout.Scale;
                int namePadding = (int)MathF.Round(12 * layout.Scale);
                int nameWidth = Math.Max(panel.Width, (int)MathF.Ceiling(measured.X) + namePadding * 2);
                int nameHeight = (int)MathF.Ceiling(measured.Y) + namePadding * 2;
                int nameX = Math.Clamp(panel.X, 0, Math.Max(0, Game1.uiViewport.Width - nameWidth));
                int nameY = panel.Bottom + (int)MathF.Round(4 * layout.Scale);

                identificationPreviewAnchor = new Rectangle(nameX, nameY, nameWidth, nameHeight);
                IClickableMenu.drawTextureBox(spriteBatch, Game1.menuTexture,
                    new Rectangle(0, 256, 60, 60), nameX, nameY, nameWidth, nameHeight,
                    Color.White, 1f, drawShadow: false);
                spriteBatch.DrawString(Game1.smallFont, name,
                    new Vector2(nameX + (nameWidth - measured.X) / 2,
                        nameY + (nameHeight - measured.Y) / 2 + MathF.Round(3 * layout.Scale)),
                    Game1.textColor, 0f, Vector2.Zero, layout.Scale, SpriteEffects.None, 1f);
            }

        }

        //----------------------------------------
        // 設置前プレビュー
        //----------------------------------------

        /// <summary>
        /// Custom Flooringでハイライト無効の場合だけ、バニラの緑色の配置可否表示を描画しません。
        /// 通常アイテムとハイライト有効の床材ではバニラ処理を維持します。
        /// </summary>
        private static bool BeforeDrawPlacementBounds(StardewValley.Object __instance)
        {
            if (!TryGetFloorId(__instance, out string floorId)
                || !TryGetDefinition(floorId, out var definition))
                return true;

            return IsDefaultPlacementHighlight(definition);
        }


        /// <summary>未指定・不明な設定値では従来の表示に戻します。</summary>
        private static bool IsDefaultPlacementHighlight(CustomFlooringModel definition)
        {
            return !string.Equals(definition.PlacementHighlight, "None", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(definition.PlacementHighlight, "Outline", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// バニラの設置プレビューと同じ判定を使用し、配置できない場合は赤い輪郭線にします。
        /// </summary>
        private static bool CanPlaceAtPreviewTile(StardewValley.Object item, Vector2 tile)
        {
            GameLocation location = Game1.currentLocation;
            Farmer player = Game1.player;
            int x = (int)tile.X * Game1.tileSize;
            int y = (int)tile.Y * Game1.tileSize;

            if (Utility.playerCanPlaceItemHere(location, item, x, y, player, false))
                return true;

            return Utility.isThereAnObjectHereWhichAcceptsThisItem(location, item, x, y)
                && Utility.withinRadiusOfPlayer(x, y, 1, player);
        }

        /// <summary>Outline指定時は、設置可否に応じた色でタイルの四辺に細い枠線を描きます。</summary>
        private static void DrawPlacementOutline(SpriteBatch spriteBatch, Vector2 position, bool canPlace)
        {
            int x = (int)position.X;
            int y = (int)position.Y;
            int size = Game1.tileSize;
            Color color = canPlace ? Color.LimeGreen : Color.Red;
            spriteBatch.Draw(Game1.staminaRect, new Rectangle(x, y, size, 2), color);
            spriteBatch.Draw(Game1.staminaRect, new Rectangle(x, y + size - 2, size, 2), color);
            spriteBatch.Draw(Game1.staminaRect, new Rectangle(x, y, 2, size), color);
            spriteBatch.Draw(Game1.staminaRect, new Rectangle(x + size - 2, y, 2, size), color);
        }

        //----------------------------------------
        // アニメーション
        //----------------------------------------

        /// <summary>
        /// 共通のゲーム内経過時間を基準にフレームを選びます。
        /// AnimationIdを指定した場合はIDに応じた固定の位相差を与えます。
        /// </summary>
        private static Rectangle GetVariantSource(CustomFlooringVariant variant, long placementOffset = 0)
        {
            return GetAnimatedSource(variant, variant.Animation, variant.X, variant.Y, placementOffset);
        }

        /// <summary>通常Variantとランダム候補に共通のフレーム選択処理です。</summary>
        private static Rectangle GetAnimatedSource(CustomFlooringVariant variant,
            List<CustomFlooringAnimationFrame>? frames, int x, int y, long placementOffset = 0)
        {
            if (frames == null || frames.Count == 0)
                return new Rectangle(x, y, 16, 16);

            var timing = GetAnimationTiming(variant, frames);
            if (timing.Cycle <= 0)
                return new Rectangle(x, y, 16, 16);
            long cycle = timing.Cycle;
            long offset = timing.GroupOffset;

            long elapsed = (long)(Game1.currentGameTime?.TotalGameTime.TotalMilliseconds ?? 0);
            long phase = (elapsed + offset + placementOffset % cycle) % cycle;
            foreach (var frame in frames)
            {
                int duration = Math.Max(1, frame.Duration);
                if (phase < duration)
                    return new Rectangle(frame.X, frame.Y, 16, 16);
                phase -= duration;
            }

            return new Rectangle(x, y, 16, 16);
        }

        /// <summary>ランダム候補の周期も、VariantのAnimationIdによる位相差を引き継ぎます。</summary>
        private static (long Cycle, long GroupOffset) GetAnimationTiming(CustomFlooringVariant variant,
            List<CustomFlooringAnimationFrame> frames)
        {
            if (ReferenceEquals(frames, variant.Animation) && AnimationCache.TryGetValue(variant, out var cached))
                return cached;
            long cycle = 0;
            foreach (var frame in frames)
                cycle += Math.Max(1, frame.Duration);
            long offset = 0;
            if (cycle > 0 && !string.IsNullOrWhiteSpace(variant.AnimationId))
            {
                uint hash = 2166136261;
                foreach (char c in variant.AnimationId.ToUpperInvariant())
                    hash = (hash ^ c) * 16777619;
                offset = hash % cycle;
            }
            return (cycle, offset);
        }

        /// <summary>
        /// 設置時のフレームを開始フレームとして固定するため、共通時間に対する位相差を求めます。
        /// AnimationIdの位相差とは独立して床材ごとに保存します。
        /// </summary>
        private static long GetPlacementAnimationOffset(CustomFlooringVariant variant,
            List<CustomFlooringAnimationFrame>? frames)
        {
            if (frames == null || frames.Count == 0)
                return 0;

            var timing = GetAnimationTiming(variant, frames);
            if (timing.Cycle <= 0)
                return 0;
            long cycle = timing.Cycle;
            long elapsed = (long)(Game1.currentGameTime?.TotalGameTime.TotalMilliseconds ?? 0);
            long groupOffset = timing.GroupOffset;
            return (cycle - (elapsed + groupOffset) % cycle) % cycle;
        }

        /// <summary>選択した画像を設置候補タイルに表示します。</summary>
        private static void OnRenderedWorld(object? sender, RenderedWorldEventArgs e)
        {
            // オブジェクトやプレイヤーの描画後に、識別マークを最前面へ重ねます。
            if (showFlooringMarkers && Context.IsWorldReady && Game1.activeClickableMenu == null)
                DrawFlooringMarkers(e.SpriteBatch);

            if (!Context.IsWorldReady || Game1.activeClickableMenu != null
                || Game1.player?.CurrentItem is not StardewValley.Object item
                || !TryGetFloorId(item, out string floorId)
                || !TryGetDefinition(floorId, out var definition))
                return;

            int index = SelectedVariants.GetValueOrDefault(floorId) % definition.Variants.Count;
            var variant = definition.Variants[index];
            try
            {
                var randomImage = GetRandomTexture(variant, floorId);
                Texture2D texture = Game1.content.Load<Texture2D>(randomImage.Texture);
                Rectangle source = randomImage.Source;
                if (source.X < 0 || source.Y < 0 || source.Right > texture.Width || source.Bottom > texture.Height)
                    return;

                Vector2 tile = Game1.currentCursorTile;
                Vector2 position = Game1.GlobalToLocal(Game1.viewport, tile * Game1.tileSize);
                e.SpriteBatch.Draw(texture, position, source, (IsDefaultPlacementHighlight(definition) ? Color.White * 0.65f : Color.White), 0f,
                    Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, 0.00001f);
                if (string.Equals(definition.PlacementHighlight, "Outline", StringComparison.OrdinalIgnoreCase))
                    DrawPlacementOutline(e.SpriteBatch, position, CanPlaceAtPreviewTile(item, tile));
            }
            catch (Exception)
            {
                // 無効なテクスチャ定義ではプレビューだけを省略します。
            }
        }

        //----------------------------------------
        // 設置前Variant確認HUD
        //----------------------------------------

        /// <summary>
        /// 頭上に掲げたCustomFlooringの現在Variantと前後の候補を、
        /// インベントリに近いスロット表示で左上へ描画します。表示専用で入力を奪いません。
        /// </summary>
        private static void OnRenderedHud(object? sender, RenderedHudEventArgs e)
        {
            if (!Context.IsWorldReady || Game1.activeClickableMenu != null)
                return;

            identificationPreviewAnchor = null;
            if (Game1.player?.CurrentItem is not StardewValley.Object item
                || Game1.player.netItemStowed.Value
                || !TryGetFloorId(item, out string floorId)
                || !TryGetDefinition(floorId, out var definition)
                || !definition.VariantPreviewEnabled || definition.Variants.Count == 0)
            {
                DrawIdentificationStatus(e.SpriteBatch);
                return;
            }

            if (IsSelectablePreview(definition))
            {
                try
                {
                    DrawSelectablePreview(e.SpriteBatch, definition, floorId);
                }
                catch (Exception)
                {
                    // 不正な画像などがあっても、ゲーム本体のHUD描画は継続します。
                }
                DrawIdentificationStatus(e.SpriteBatch);
                return;
            }

            int count = definition.VariantPreviewCount is 3 or 5 or 7 or 9
                ? definition.VariantPreviewCount : 5;
            float scale = 1f + Math.Clamp(definition.VariantPreviewScale, 0, 5) * 0.2f;
            int slotSize = (int)MathF.Round(64 * scale);
            int gap = (int)MathF.Round(4 * scale);
            int padding = (int)MathF.Round(8 * scale);
            int width = padding * 2 + count * slotSize + (count - 1) * gap;
            int height = padding * 2 + slotSize;
            int x = definition.VariantPreviewOffsetX * 10;
            int y = definition.VariantPreviewOffsetY * 10;

            // 画面端で見切れないよう、負の補正や大きすぎる拡大を安全に丸めます。
            x = Math.Clamp(x, 0, Math.Max(0, Game1.uiViewport.Width - width));
            Rectangle available = GetPreviewAvailableArea();
            y = Math.Clamp(available.Top + y, available.Top,
                Math.Max(available.Top, available.Bottom - height - GetPreviewFooterHeight(definition, floorId, scale)));

            identificationPreviewAnchor = new Rectangle(x, y, width, height);
            try
            {
                // 外枠だけはゲームのUIテクスチャを使用し、UI差し替えModにも追従します。
                // スロット背景はFrameFix v4の単色描画を維持して、突起の原因を切り分けます。
                IClickableMenu.drawTextureBox(e.SpriteBatch, Game1.menuTexture,
                    new Rectangle(0, 256, 60, 60), x, y, width, height,
                    Color.White, 1f, drawShadow: false);

                int selected = SelectedVariants.GetValueOrDefault(floorId) % definition.Variants.Count;
                int middle = count / 2;
                for (int i = 0; i < count; i++)
                {
                    int index = (selected + i - middle + definition.Variants.Count * count)
                        % definition.Variants.Count;
                    var variant = definition.Variants[index];
                    Rectangle slot = new(x + padding + i * (slotSize + gap), y + padding, slotSize, slotSize);
                    // バニラのスロット画像には外周の突起が含まれるため、枠と重ねず平坦な背景を描きます。
                    // 外側のウィンドウ枠と中央の選択枠は従来どおり維持します。
                    e.SpriteBatch.Draw(Game1.staminaRect, slot, new Color(235, 226, 218));
                    int slotBorder = Math.Max(1, (int)MathF.Round(scale));
                    Color slotEdge = new(201, 190, 184);
                    e.SpriteBatch.Draw(Game1.staminaRect, new Rectangle(slot.X, slot.Y, slot.Width, slotBorder), slotEdge);
                    e.SpriteBatch.Draw(Game1.staminaRect, new Rectangle(slot.X, slot.Bottom - slotBorder, slot.Width, slotBorder), slotEdge);
                    e.SpriteBatch.Draw(Game1.staminaRect, new Rectangle(slot.X, slot.Y, slotBorder, slot.Height), slotEdge);
                    e.SpriteBatch.Draw(Game1.staminaRect, new Rectangle(slot.Right - slotBorder, slot.Y, slotBorder, slot.Height), slotEdge);

                    var randomImage = GetRandomTexture(variant, floorId);
                    Texture2D texture = Game1.content.Load<Texture2D>(randomImage.Texture);
                    Rectangle source = randomImage.Source;
                    if (source.X >= 0 && source.Y >= 0 && source.Right <= texture.Width && source.Bottom <= texture.Height)
                    {
                        int inset = Math.Max(4, slotSize / 8);
                        Rectangle icon = new(slot.X + inset, slot.Y + inset,
                            slotSize - inset * 2, slotSize - inset * 2);
                        e.SpriteBatch.Draw(texture, icon, source, Color.White);
                    }

                    if (i == middle)
                    {
                        int border = Math.Max(2, (int)MathF.Round(4 * scale));
                        Color red = new(225, 40, 35);
                        e.SpriteBatch.Draw(Game1.staminaRect, new Rectangle(slot.X, slot.Y, slot.Width, border), red);
                        e.SpriteBatch.Draw(Game1.staminaRect, new Rectangle(slot.X, slot.Bottom - border, slot.Width, border), red);
                        e.SpriteBatch.Draw(Game1.staminaRect, new Rectangle(slot.X, slot.Y, border, slot.Height), red);
                        e.SpriteBatch.Draw(Game1.staminaRect, new Rectangle(slot.Right - border, slot.Y, border, slot.Height), red);
                    }
                }

                if (definition.VariantPreviewShowName)
                {
                    // 選択中のVariant名を独立したバニラUI枠に表示します。
                    // プレビュー本体の位置・サイズ・描画は変更しません。
                    string? name = definition.Variants[selected].DisplayName;
                    if (string.IsNullOrWhiteSpace(name))
                        name = helper.Translation.Get("customFlooring.variantPreview.noDisplayName");

                    float textScale = scale;
                    Vector2 measured = Game1.smallFont.MeasureString(name) * textScale;
                    int namePadding = (int)MathF.Round(12 * scale);
                    int nameWidth = Math.Max(width, (int)MathF.Ceiling(measured.X) + namePadding * 2);
                    int nameHeight = (int)MathF.Ceiling(measured.Y) + namePadding * 2;
                    int nameX = Math.Clamp(x, 0, Math.Max(0, Game1.uiViewport.Width - nameWidth));
                    int nameY = y + height + (int)MathF.Round(4 * scale);

                    identificationPreviewAnchor = new Rectangle(nameX, nameY, nameWidth, nameHeight);
                    IClickableMenu.drawTextureBox(e.SpriteBatch, Game1.menuTexture,
                        new Rectangle(0, 256, 60, 60), nameX, nameY, nameWidth, nameHeight,
                        Color.White, 1f, drawShadow: false);
                    e.SpriteBatch.DrawString(Game1.smallFont, name,
                        new Vector2(nameX + (nameWidth - measured.X) / 2,
                            nameY + (nameHeight - measured.Y) / 2 + MathF.Round(3 * scale)),
                        Game1.textColor, 0f, Vector2.Zero, textScale,
                        SpriteEffects.None, 1f);
                }

            }
            catch (Exception)
            {
                // 無効なテクスチャ等ではHUDだけを省略し、床材の通常操作は維持します。
            }

            DrawIdentificationStatus(e.SpriteBatch);
        }

        /// <summary>
        /// 識別表示中の案内をHUDに表示します。プレビューがある場合はその下に配置します。
        /// </summary>
        private static void DrawIdentificationStatus(SpriteBatch spriteBatch)
        {
            if (!showFlooringMarkers)
                return;

            string message = helper.Translation.Get("customFlooring.identification.active");
            Vector2 textSize = Game1.smallFont.MeasureString(message);
            const int padding = 12;
            int width = (int)MathF.Ceiling(textSize.X) + padding * 2;
            int height = (int)MathF.Ceiling(textSize.Y) + padding * 2;
            int x = identificationPreviewAnchor?.X ?? 10;
            int y = identificationPreviewAnchor is Rectangle anchor ? anchor.Bottom + 4 : 10;
            x = Math.Clamp(x, 0, Math.Max(0, Game1.uiViewport.Width - width));
            Rectangle available = GetPreviewAvailableArea();
            y = Math.Clamp(y, available.Top, Math.Max(available.Top, available.Bottom - height));

            IClickableMenu.drawTextureBox(spriteBatch, Game1.menuTexture,
                new Rectangle(0, 256, 60, 60), x, y, width, height,
                Color.White, 1f, drawShadow: false);
            spriteBatch.DrawString(Game1.smallFont, message,
                new Vector2(x + padding, y + padding + 3), Game1.textColor);
        }

        //----------------------------------------
        // 設置済みカスタム床材の識別表示
        //----------------------------------------

        /// <summary>
        /// 現在のマップにあるT's Core製の床材だけに赤い四角と×印を重ねます。
        /// 表示専用のため、地形や床材のデータは変更しません。
        /// </summary>
        private static void DrawFlooringMarkers(SpriteBatch spriteBatch)
        {
            GameLocation location = Game1.currentLocation;
            if (location == null)
                return;

            markerPixel ??= CreateMarkerPixel();
            foreach (var entry in location.terrainFeatures.Pairs)
            {
                if (entry.Value is not Flooring floor
                    || !floor.modData.ContainsKey(VariantKey)
                    || !TryGetDefinition(floor.whichFloor.Value, out CustomFlooringModel definition))
                    continue;

                Vector2 position = Game1.GlobalToLocal(Game1.viewport, entry.Key * Game1.tileSize);
                int size = Game1.tileSize;
                if (position.X + size < 0 || position.Y + size < 0
                    || position.X > Game1.viewport.Width || position.Y > Game1.viewport.Height)
                    continue;

                // ツールの選択枠と重ならないよう、マーク全体をタイルの内側に収めます。
                int margin = Math.Max(4, size / 8);
                int innerSize = size - margin * 2;
                Vector2 topLeft = position + new Vector2(margin, margin);
                if (definition.IdentificationMarker is { } customMarker && !string.IsNullOrWhiteSpace(customMarker.Texture))
                {
                    try
                    {
                        Texture2D texture = Game1.content.Load<Texture2D>(customMarker.Texture);
                        Rectangle source = new(customMarker.X, customMarker.Y, 16, 16);
                        if (source.X >= 0 && source.Y >= 0 && source.Right <= texture.Width && source.Bottom <= texture.Height)
                        {
                            spriteBatch.Draw(texture, new Rectangle((int)topLeft.X, (int)topLeft.Y, innerSize, innerSize), source, Color.White);
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        monitor.Log($"Could not draw identification marker '{customMarker.Texture}': {ex.Message}", LogLevel.Warn);
                    }
                }
                spriteBatch.Draw(markerPixel, new Rectangle((int)topLeft.X, (int)topLeft.Y, innerSize, innerSize), Color.Black * 0.30f);
                int thickness = Math.Max(2, size / 32) + 1;
                DrawMarkerLine(spriteBatch, topLeft, topLeft + new Vector2(innerSize, 0), thickness);
                DrawMarkerLine(spriteBatch, topLeft + new Vector2(innerSize, 0), topLeft + new Vector2(innerSize, innerSize), thickness);
                DrawMarkerLine(spriteBatch, topLeft + new Vector2(innerSize, innerSize), topLeft + new Vector2(0, innerSize), thickness);
                DrawMarkerLine(spriteBatch, topLeft + new Vector2(0, innerSize), topLeft, thickness);
                int inset = Math.Max(4, innerSize / 6);
                DrawMarkerLine(spriteBatch, topLeft + new Vector2(inset, inset), topLeft + new Vector2(innerSize - inset, innerSize - inset), thickness);
                DrawMarkerLine(spriteBatch, topLeft + new Vector2(innerSize - inset, inset), topLeft + new Vector2(inset, innerSize - inset), thickness);
            }
        }

        private static Texture2D CreateMarkerPixel()
        {
            Texture2D pixel = new(Game1.graphics.GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });
            return pixel;
        }

        private static void DrawMarkerLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, int thickness)
        {
            Vector2 delta = end - start;
            spriteBatch.Draw(markerPixel!, start, null, Color.Red * 0.85f,
                (float)Math.Atan2(delta.Y, delta.X), Vector2.Zero,
                new Vector2(delta.Length(), thickness), SpriteEffects.None, 0f);
        }

        //----------------------------------------
        // 設置
        //----------------------------------------

        private sealed class PlacementState
        {
            internal Vector2 Tile;
            internal Flooring? Before;
            internal string FloorId = "";
            internal bool Replacing;
        }

        /// <summary>
        /// 既存のカスタム床材が張り替えを許可している場合のみ、設置判定の間だけ取り外します。
        /// 設置する側はバニラ・カスタムのどちらの床材でも対象です。
        /// </summary>
        private static void BeforePlacement(StardewValley.Object __instance, GameLocation location,
            int x, int y, ref PlacementState? __state)
        {
            __state = null;
            if (!TryGetFloorId(__instance, out string floorId))
                return;

            Vector2 tile = new(x / Game1.tileSize, y / Game1.tileSize);
            location.terrainFeatures.TryGetValue(tile, out TerrainFeature? existing);
            var before = existing as Flooring;
            bool replacing = before != null
                && TryGetDefinition(before.whichFloor.Value, out var existingDefinition)
                && existingDefinition.AllowFlooringReplacement;

            __state = new PlacementState
            {
                Tile = tile,
                Before = before,
                FloorId = floorId,
                Replacing = replacing
            };

            if (replacing)
                location.terrainFeatures.Remove(tile);
        }

        /// <summary>
        /// 設置が失敗したら元の床材を戻し、成功した場合だけ旧床材をアイテム化します。
        /// Variantの保存は従来どおり、新しく設置されたカスタム床材だけに適用します。
        /// </summary>
        private static void AfterPlacement(GameLocation location, bool __result, PlacementState? __state)
        {
            if (__state == null)
                return;

            if (!__result)
            {
                if (__state.Replacing && __state.Before != null
                    && !location.terrainFeatures.ContainsKey(__state.Tile))
                    location.terrainFeatures.Add(__state.Tile, __state.Before);
                return;
            }

            bool placedNewFloor = location.terrainFeatures.TryGetValue(__state.Tile, out var placedFeature)
                && placedFeature is Flooring placedFloor
                && !ReferenceEquals(placedFloor, __state.Before);
            if (__state.Replacing && !placedNewFloor)
            {
                if (__state.Before != null && !location.terrainFeatures.ContainsKey(__state.Tile))
                    location.terrainFeatures.Add(__state.Tile, __state.Before);
                return;
            }

            if (__state.Replacing && __state.Before != null)
            {
                try
                {
                    var paths = Game1.content.Load<Dictionary<string, FloorPathData>>("Data/FloorsAndPaths");
                    if (paths.TryGetValue(__state.Before.whichFloor.Value, out var oldPath)
                        && !string.IsNullOrEmpty(oldPath.ItemId))
                    {
                        Item recovered = ItemRegistry.Create("(O)" + oldPath.ItemId);
                        Game1.createItemDebris(recovered,
                            __state.Tile * Game1.tileSize + new Vector2(Game1.tileSize / 2),
                            -1, location);
                    }
                }
                catch (Exception ex)
                {
                    monitor.Log($"Failed to recover replaced flooring: {ex.Message}", LogLevel.Warn);
                }
            }

            if (!location.terrainFeatures.TryGetValue(__state.Tile, out var feature)
                || feature is not Flooring floor || ReferenceEquals(floor, __state.Before)
                || !TryGetDefinition(__state.FloorId, out var definition))
                return;

            int index = SelectedVariants.GetValueOrDefault(__state.FloorId) % definition.Variants.Count;
            var variant = definition.Variants[index];
            floor.modData[VariantKey] = variant.Id;
            // 個別抽選では設置するたびに新しい値を保存し、同じ位置への張り替えでも再抽選します。
            // 保存することでロード後の表示・足音は変化しません。
            if (!variant.RandomTextureShared && variant.RandomTextures is { Count: > 0 })
                floor.modData[RandomPlacementKey] = Guid.NewGuid().ToString("N");
            else
                floor.modData.Remove(RandomPlacementKey);
            // ランダム候補のアニメーションも、通常Variantと同じ同期設定を適用します。
            var selectedImage = GetRandomTexture(variant, __state.FloorId, location.NameOrUniqueName, __state.Tile, floor);
            if (!variant.AnimationSync && selectedImage.Animation?.Count > 0)
                floor.modData[AnimationOffsetKey] = GetPlacementAnimationOffset(variant, selectedImage.Animation).ToString(System.Globalization.CultureInfo.InvariantCulture);
            else
                floor.modData.Remove(AnimationOffsetKey);
        }

        //----------------------------------------
        // Variantごとの効果音
        //----------------------------------------

        /// <summary>未指定の音はバニラの定義を引き継ぎ、明示的なnullは無音にします。</summary>
        private static string? GetVariantSound(Flooring floor, Func<CustomFlooringVariant, bool> isSpecified,
            Func<CustomFlooringVariant, string?> getSound, string? vanillaSound)
        {
            if (!floor.modData.TryGetValue(VariantKey, out string? saved)
                || !TryGetDefinition(floor.whichFloor.Value, out var definition))
                return vanillaSound;

            if (!TryGetCachedFlooring(floor.whichFloor.Value, out var cached)
                || !cached.VariantsById.TryGetValue(saved, out var variant))
                return vanillaSound;
            return isSpecified(variant) ? getSound(variant) : vanillaSound;
        }

        /// <summary>設置音はFlooringが生成される前なので、設置前に選択されたVariantを参照します。</summary>
        private static void PlayPlacementSound(GameLocation location, string sound, Vector2? position,
            int? pitch, SoundContext context, StardewValley.Object item)
        {
            string? selected = sound;
            if (TryGetFloorId(item, out string floorId) && TryGetDefinition(floorId, out var definition))
            {
                int index = SelectedVariants.GetValueOrDefault(floorId) % definition.Variants.Count;
                var variant = definition.Variants[index];
                if (variant.HasPlacementSound)
                    selected = variant.PlacementSound;
            }
            if (!string.IsNullOrEmpty(selected))
                location.playSound(selected, position, pitch, context);
        }

        /// <summary>撤去音は設置済みFlooringの保存済みVariantを参照します。</summary>
        private static void PlayRemovalSound(GameLocation location, string sound, Vector2? position,
            int? pitch, SoundContext context, Flooring floor)
        {
            string? selected = GetVariantSound(floor, v => v.HasRemovalSound, v => v.RemovalSound, sound);
            if (!string.IsNullOrEmpty(selected))
                location.playSound(selected, position, pitch, context);
        }

        /// <summary>歩行音は設置済みFlooringの保存済みVariantを参照します。</summary>
        private static void AfterGetFootstepSound(Flooring __instance, ref string __result)
        {
            string? sound = GetVariantSound(__instance, v => v.HasFootstepSound, v => v.FootstepSound, __result);
            if (__instance.modData.TryGetValue(VariantKey, out string? saved)
                && TryGetCachedFlooring(__instance.whichFloor.Value, out var cached)
                && cached.VariantsById.TryGetValue(saved, out var variant))
            {
                var randomTexture = GetRandomTexture(variant, __instance.whichFloor.Value,
                    __instance.Location?.NameOrUniqueName, __instance.Tile, __instance);
                if (randomTexture.Selected?.HasFootstepSound == true)
                    sound = randomTexture.Selected.FootstepSound;
            }
            __result = sound ?? SilentFootstepCue;
        }

        /// <summary>Flooringアイテムの設置音だけを差し替えます。</summary>
        private static IEnumerable<CodeInstruction> ReplacePlacementSoundCall(IEnumerable<CodeInstruction> instructions)
        {
            bool inFloorPathSound = false;
            foreach (var instruction in instructions)
            {
                if (instruction.LoadsField(AccessTools.Field(typeof(FloorPathData), nameof(FloorPathData.PlacementSound))))
                    inFloorPathSound = true;
                if (inFloorPathSound && instruction.Calls(PlaySoundMethod))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    var replacement = new CodeInstruction(OpCodes.Call, PlacementSoundWrapper);
                    replacement.labels.AddRange(instruction.labels);
                    replacement.blocks.AddRange(instruction.blocks);
                    yield return replacement;
                    inFloorPathSound = false;
                }
                else
                    yield return instruction;
            }
        }

//----------------------------------------
        // 撤去時の音と破片
        //----------------------------------------

        /// <summary>
        /// バニラのFlooring撤去処理の破片生成呼び出しだけを差し替えます。
        /// アイテム回収・撤去音・床材削除は元の処理を維持します。
        /// </summary>
        private static IEnumerable<CodeInstruction> ReplaceRemovalCalls(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var instruction in instructions)
            {
                if (instruction.Calls(PlaySoundMethod))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    var replacement = new CodeInstruction(OpCodes.Call, RemovalSoundWrapper);
                    replacement.labels.AddRange(instruction.labels);
                    replacement.blocks.AddRange(instruction.blocks);
                    yield return replacement;
                }
                else if (instruction.Calls(RadialDebrisMethod))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    var replacement = new CodeInstruction(OpCodes.Call, CustomRadialDebrisMethod);
                    replacement.labels.AddRange(instruction.labels);
                    replacement.blocks.AddRange(instruction.blocks);
                    yield return replacement;
                }
                else
                {
                    yield return instruction;
                }
            }
        }

        /// <summary>対象のカスタム床材だけ破片を抑制し、それ以外はバニラの破片を生成します。</summary>
        private static void CreateRemovalDebris(GameLocation location, int debrisType, int x, int y,
            int numberOfChunks, bool resource, int groundLevel, bool item, Color? color, Flooring flooring)
        {
            if (TryGetDefinition(flooring.whichFloor.Value, out var definition) && !definition.ShowRemovalDebris)
                return;

            Game1.createRadialDebris(location, debrisType, x, y, numberOfChunks, resource, groundLevel, item, color);
        }

        //----------------------------------------
        // ランダムテクスチャ
        //----------------------------------------

        /// <summary>
        /// 現在の変更周期を求めます。同じ周期内では結果が変わらず、ロード後も再現されます。
        /// </summary>
        private static long GetRandomTexturePeriod(CustomFlooringVariant variant)
        {
            long day = Convert.ToInt64(Game1.stats.DaysPlayed);
            return variant.RandomTextureChange?.Trim().ToLowerInvariant() switch
            {
                "ampm" => day * 2 + (Game1.timeOfDay >= 1200 ? 1 : 0),
                "daynight" => day * 2 + (Game1.timeOfDay >= 1800 ? 1 : 0),
                // 毎週月曜日（各季節の1・8・15・22日）に切り替えます。
                "weekly" => GetCalendarSeasonIndex() * 4 + (Game1.dayOfMonth - 1) / 7,
                // 季節が切り替わったときだけ再抽選します。
                "seasonal" => GetCalendarSeasonIndex(),
                _ => day
            };
        }

        /// <summary>
        /// ゲーム内カレンダーの年と季節から、季節ごとに固有の番号を求めます。
        /// </summary>
        private static long GetCalendarSeasonIndex()
        {
            int seasonIndex = Game1.currentSeason switch
            {
                "spring" => 0,
                "summer" => 1,
                "fall" => 2,
                "winter" => 3,
                _ => 0
            };
            return (Game1.year - 1L) * 4 + seasonIndex;
        }

        /// <summary>
        /// 保存済みのVariant IDと設置位置から安定した抽選結果を算出します。
        /// 個別抽選時のプレビューには位置情報を使わず、代表画像を表示します。
        /// </summary>
        private static (string Texture, Rectangle Source, List<CustomFlooringAnimationFrame>? Animation, CustomFlooringRandomTexture? Selected) GetRandomTexture(
            CustomFlooringVariant variant, string floorId, string? locationName = null, Vector2? tile = null, Flooring? flooring = null)
        {
            if (variant.RandomTextures is not { Count: > 0 } candidates)
                return (variant.Texture, GetVariantSource(variant), variant.Animation, null);

            // GetHashCodeはプロセスごとに値が変わるため使用しません。
            string seed = floorId + "|" + variant.Id + "|" + GetRandomTexturePeriod(variant);
            if (!variant.RandomTextureShared && tile.HasValue)
                {
                seed += "|" + locationName + "|" + tile.Value.X + "|" + tile.Value.Y;
                if (flooring != null && flooring.modData.TryGetValue(RandomPlacementKey, out string? placementId))
                    seed += "|" + placementId;
            }
            uint hash = 2166136261;
            foreach (char ch in seed)
            {
                hash ^= ch;
                hash *= 16777619;
            }
            // 0以下のWeightは抽選対象から除外し、全候補が無効なら従来の均等抽選に戻します。
            long totalWeight = 0;
            foreach (var candidate in candidates)
            {
                if (candidate.Weight > 0)
                    totalWeight += candidate.Weight;
            }

            CustomFlooringRandomTexture chosen;
            if (totalWeight == 0)
            {
                chosen = candidates[(int)(hash % (uint)candidates.Count)];
            }
            else
            {
                long roll = hash % totalWeight;
                chosen = candidates[0];
                foreach (var candidate in candidates)
                {
                    if (candidate.Weight <= 0)
                        continue;

                    if (roll < candidate.Weight)
                    {
                        chosen = candidate;
                        break;
                    }
                    roll -= candidate.Weight;
                }
            }
            return (chosen.Texture, GetAnimatedSource(variant, chosen.Animation, chosen.X, chosen.Y), chosen.Animation, chosen);
        }

        //----------------------------------------
        // 設置済み床材の描画
        //----------------------------------------

        private static bool BeforeDraw(Flooring __instance, SpriteBatch spriteBatch)
        {
            if (!__instance.modData.TryGetValue(VariantKey, out string? saved))
                return true;
            if (!TryGetDefinition(__instance.whichFloor.Value, out var definition))
                return true;

            if (!TryGetCachedFlooring(__instance.whichFloor.Value, out var cached)
                || !cached.VariantsById.TryGetValue(saved, out var variant))
                return true;
            try
            {
                var randomImage = GetRandomTexture(variant, __instance.whichFloor.Value,
                    __instance.Location?.NameOrUniqueName, __instance.Tile, __instance);
                Texture2D texture = Game1.content.Load<Texture2D>(randomImage.Texture);
                long placementOffset = 0;
                if (!variant.AnimationSync && __instance.modData.TryGetValue(AnimationOffsetKey, out string? savedOffset))
                    long.TryParse(savedOffset, System.Globalization.NumberStyles.Integer,
                        System.Globalization.CultureInfo.InvariantCulture, out placementOffset);
                // ランダム候補の非同期アニメーションは、同時に設置した床材も個別の位相にします。
                // タイル座標由来の固定オフセットなので、セーブ・ロード後も同じ位相を維持します。
                if (!variant.AnimationSync && variant.RandomTextures is { Count: > 0 }
                    && randomImage.Animation is { Count: > 0 })
                {
                    long cycle = GetAnimationTiming(variant, randomImage.Animation).Cycle;
                    if (cycle > 0)
                    {
                        string seed = __instance.whichFloor.Value + "|" + variant.Id + "|"
                            + __instance.Location?.NameOrUniqueName + "|"
                            + __instance.Tile.X + "|" + __instance.Tile.Y;
                        uint hash = 2166136261;
                        foreach (char ch in seed)
                            hash = (hash ^ ch) * 16777619;
                        placementOffset += hash % cycle;
                    }
                }
                Rectangle source = variant.RandomTextures is { Count: > 0 }
                    ? GetAnimatedSource(variant, randomImage.Animation, randomImage.Source.X, randomImage.Source.Y, placementOffset)
                    : GetVariantSource(variant, placementOffset);
                if (source.X < 0 || source.Y < 0 || source.Right > texture.Width || source.Bottom > texture.Height)
                    return true;
                Vector2 position = Game1.GlobalToLocal(Game1.viewport, __instance.Tile * Game1.tileSize);
                spriteBatch.Draw(texture, position, source, Color.White, 0f, Vector2.Zero,
                    Game1.pixelZoom, SpriteEffects.None, 0.00001f);
                return false;
            }
            catch (Exception ex)
            {
                monitor.Log($"Custom flooring texture '{variant.Texture}' could not be drawn: {ex.Message}", LogLevel.Warn);
                return true;
            }
        }
    }
}
