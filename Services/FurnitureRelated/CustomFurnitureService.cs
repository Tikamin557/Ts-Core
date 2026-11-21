using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Ts_Core.Models.ContentPatcherRelated;
using Ts_Core.Services.ContentPatcherRelated;

namespace Ts_Core.Services.FurnitureRelated
{
    /// <summary>
    /// 登録された家具をバニラTVとして生成し、番組とオーバーレイの描画だけを拡張します。
    /// 独自の家具クラスやセーブ形式は使用しません。
    /// </summary>
    internal static class CustomFurnitureService
    {
        private static IModHelper helper = null!;
        private static IMonitor monitor = null!;
        private static Dictionary<string, CustomFurnitureModel>? definitions;
        private static readonly HashSet<string> warnings = new(StringComparer.Ordinal);
        private static bool drawPatched;
        private static bool initialized;
        private const string RotationVariantKey = "Tikamin557.TsCore/RotationVariant";
        private static readonly ConditionalWeakTable<Furniture, DrawState> drawStates = new();

        /// <summary>TV.drawからFurniture.drawが呼ばれる場合の二重オフセットを防ぎます。</summary>
        private sealed class DrawState
        {
            internal int Depth;
        }

        //----------------------------------------
        // 初期化・データ取得
        //----------------------------------------

        internal static void Initialize(IModHelper modHelper, IMonitor modMonitor, Harmony harmony)
        {
            helper = modHelper;
            monitor = modMonitor;
            helper.Events.Content.AssetRequested += CustomFurnitureDataService.OnAssetRequested;
            helper.Events.Content.AssetsInvalidated += (_, e) =>
            {
                if (e.NamesWithoutLocale.Any(n => n.IsEquivalentTo(CustomFurnitureDataService.AssetName) || n.IsEquivalentTo("Data/Furniture")))
                {
                    definitions = null;
                    warnings.Clear();
                }
            };
            helper.Events.GameLoop.ReturnedToTitle += (_, _) => { definitions = null; warnings.Clear(); };

            try
            {
                harmony.Patch(AccessTools.Method(typeof(TV), nameof(TV.draw), new[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) }),
                    transpiler: new HarmonyMethod(typeof(CustomFurnitureService), nameof(ReplaceScreenDrawCalls)));
                if (!drawPatched)
                    return;
                harmony.Patch(AccessTools.Method(typeof(Furniture), nameof(Furniture.GetFurnitureInstance)),
                    postfix: new HarmonyMethod(typeof(CustomFurnitureService), nameof(AfterCreateFurniture)) { priority = Priority.Last });
                // 家具の説明文を定義から上書きします。未指定の家具はバニラのままです。
                var descriptionMethod = AccessTools.Method(typeof(Furniture), nameof(Furniture.getDescription));
                if (descriptionMethod != null)
                    harmony.Patch(descriptionMethod,
                        postfix: new HarmonyMethod(typeof(CustomFurnitureService), nameof(AfterGetDescription)));
                else
                    monitor.Log("CustomFurniture: Furniture.getDescription was not found; description overrides are unavailable.", LogLevel.Warn);
                // 家具の衝突タイルに指定されたActionを、マップの操作処理から実行します。
                var locationAction = AccessTools.Method(typeof(GameLocation), nameof(GameLocation.checkAction),
                    new[] { typeof(xTile.Dimensions.Location), typeof(xTile.Dimensions.Rectangle), typeof(Farmer) });
                if (locationAction != null)
                    harmony.Patch(locationAction,
                        prefix: new HarmonyMethod(typeof(CustomFurnitureService), nameof(BeforeLocationCheckAction)));
                else
                    monitor.Log("CustomFurniture: GameLocation.checkAction was not found; TileProperties actions are unavailable.", LogLevel.Warn);
                var actionableTile = AccessTools.Method(typeof(GameLocation), nameof(GameLocation.isActionableTile),
                    new[] { typeof(int), typeof(int), typeof(Farmer) });
                if (actionableTile != null)
                    harmony.Patch(actionableTile,
                        postfix: new HarmonyMethod(typeof(CustomFurnitureService), nameof(AfterIsActionableTile)));
                // TVの一部タイルをテーブルとして使用します。保持はバニラのheldObjectを使用します。
                var isTable = AccessTools.Method(typeof(Furniture), nameof(Furniture.IsTable));
                if (isTable != null)
                    harmony.Patch(isTable, postfix: new HarmonyMethod(typeof(CustomFurnitureService), nameof(AfterIsTable)));
                var tvAction = AccessTools.Method(typeof(TV), nameof(TV.checkForAction));
                if (tvAction != null)
                    harmony.Patch(tvAction, prefix: new HarmonyMethod(typeof(CustomFurnitureService), nameof(BeforeTvAction)));
                // 家具のheldObject描画座標を指定タイルの中央へ移動します。
                var furnitureDraw = AccessTools.Method(typeof(Furniture), nameof(Furniture.draw),
                    new[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) });
                if (furnitureDraw != null)
                    harmony.Patch(furnitureDraw, transpiler: new HarmonyMethod(typeof(CustomFurnitureService), nameof(ReplaceHeldObjectCenter)));
                // バニラの回転処理を実行せず、表示バリエーションのみを切り替えます。
                harmony.Patch(AccessTools.Method(typeof(Furniture), nameof(Furniture.rotate)),
                    prefix: new HarmonyMethod(typeof(CustomFurnitureService), nameof(BeforeRotate)));
                var tvRotate = AccessTools.Method(typeof(TV), nameof(Furniture.rotate));
                if (tvRotate != null && tvRotate.DeclaringType == typeof(TV))
                    harmony.Patch(tvRotate, prefix: new HarmonyMethod(typeof(CustomFurnitureService), nameof(BeforeRotate)));

                // 家具本体の描画中だけスプライトの参照領域を変更します。
                harmony.Patch(AccessTools.Method(typeof(Furniture), nameof(Furniture.draw),
                    new[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) }),
                    prefix: new HarmonyMethod(typeof(CustomFurnitureService), nameof(BeforeDraw)),
                    postfix: new HarmonyMethod(typeof(CustomFurnitureService), nameof(AfterDraw)));
                harmony.Patch(AccessTools.Method(typeof(TV), nameof(TV.draw),
                    new[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) }),
                    prefix: new HarmonyMethod(typeof(CustomFurnitureService), nameof(BeforeDraw)),
                    postfix: new HarmonyMethod(typeof(CustomFurnitureService), nameof(AfterDraw)));
                initialized = true;
            }
            catch (Exception ex)
            {
                monitor.Log($"CustomFurniture initialization failed: {ex}", LogLevel.Error);
            }
        }

        private static void Warn(string id, string message)
        {
            if (warnings.Add(id + ":" + message))
                monitor.Log($"CustomFurniture '{id}': {message}", LogLevel.Warn);
        }

        private static bool TryGetDefinition(string id, out CustomFurnitureModel model)
        {
            model = null!;
            if (!initialized || !drawPatched)
                return false;
            try
            {
                definitions ??= helper.GameContent.Load<Dictionary<string, CustomFurnitureModel>>(CustomFurnitureDataService.AssetName);
                if (!definitions.TryGetValue(id, out var value) || value == null)
                    return false;
                var area = value.ScreenArea;
                bool isTv = string.Equals(value.Type, "TV", StringComparison.OrdinalIgnoreCase);
                bool isFurniture = string.Equals(value.Type, "Furniture", StringComparison.OrdinalIgnoreCase);
                if ((!isTv && !isFurniture) || (isTv && (area == null ||
                    area.X < 0 || area.Y < 0 || area.Width <= 0 || area.Height <= 0 ||
                    !(string.Equals(value.ScreenFit, "Stretch", StringComparison.OrdinalIgnoreCase) ||
                      string.Equals(value.ScreenFit, "Contain", StringComparison.OrdinalIgnoreCase) ||
                      string.Equals(value.ScreenFit, "Cover", StringComparison.OrdinalIgnoreCase)))))
                {
                    Warn(id, "Invalid Type, ScreenArea or ScreenFit.");
                    return false;
                }
                if (value.RotationVariants is { Enabled: true } variants && (variants.Count < 2 || variants.Count > 64))
                {
                    Warn(id, "RotationVariants.Count must be between 2 and 64.");
                    return false;
                }
                if (value.RotationVariants is { Enabled: true, Columns: { } columns } columnVariants &&
                    (columns < 1 || columns > columnVariants.Count))
                {
                    Warn(id, "RotationVariants.Columns must be between 1 and Count.");
                    return false;
                }
                if (value.RotationVariants is { Enabled: true, ScreenAreas: { } screenAreas } screenVariants)
                {
                    foreach (var entry in screenAreas)
                    {
                        if (!int.TryParse(entry.Key, System.Globalization.NumberStyles.None,
                            System.Globalization.CultureInfo.InvariantCulture, out int number) ||
                            number < 1 || number > screenVariants.Count ||
                            entry.Value == null || entry.Value.X < 0 || entry.Value.Y < 0 ||
                            entry.Value.Width <= 0 || entry.Value.Height <= 0)
                        {
                            Warn(id, "RotationVariants.ScreenAreas contains an invalid number or screen area.");
                            return false;
                        }
                    }
                }
                model = value;
                return true;
            }
            catch (Exception ex)
            {
                Warn(id, $"Could not load definitions: {ex.Message}");
                return false;
            }
        }

        /// <summary>定義がある場合に限り、家具のツールチップ説明文をバニラの文字幅処理で折り返します。</summary>
        private static void AfterGetDescription(Furniture __instance, ref string __result)
        {
            if (TryGetDefinition(__instance.ItemId, out var model) && model.Description != null)
                __result = Game1.parseText(model.Description, Game1.smallFont, 380);
        }

        //----------------------------------------
        // 衝突タイルのAction
        //----------------------------------------

        /// <summary>タイルごとの操作設定を取得します。</summary>
        private static bool TryGetInteraction(Furniture furniture, int tileX, int tileY, out string interaction)
        {
            interaction = string.Empty;
            if (!TryGetDefinition(furniture.ItemId, out var model) || model.TileProperties == null)
                return false;
            var bounds = furniture.boundingBox.Value;
            if (!bounds.Contains(tileX * 64 + 32, tileY * 64 + 32))
                return false;
            int relativeX = tileX - bounds.Left / 64;
            int relativeY = tileY - bounds.Top / 64;
            if (!model.TileProperties.TryGetValue($"{relativeX}, {relativeY}", out var property) &&
                !model.TileProperties.TryGetValue($"{relativeX},{relativeY}", out property))
                return false;
            interaction = property?.Interaction ?? string.Empty;
            return interaction.Equals("Table", StringComparison.OrdinalIgnoreCase) ||
                interaction.Equals("TV", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>テーブルとして設定されたTVだけに、バニラのアイテム保持機能を許可します。</summary>
        private static void AfterIsTable(Furniture __instance, ref bool __result)
        {
            if (__result || __instance is not TV || !TryGetDefinition(__instance.ItemId, out var model) || model.TileProperties == null)
                return;
            __result = model.TileProperties.Values.Any(p => string.Equals(p?.Interaction, "Table", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>左タイルではTVメニューを開かず、設置済みアイテムを回収します。</summary>
        private static bool BeforeTvAction(TV __instance, Farmer who, bool justCheckingForActivity, ref bool __result)
        {
            var tile = Game1.currentCursorTile;
            if (!TryGetInteraction(__instance, (int)tile.X, (int)tile.Y, out string interaction) ||
                !interaction.Equals("Table", StringComparison.OrdinalIgnoreCase))
                return true;
            __result = justCheckingForActivity || (__instance.heldObject.Value != null && __instance.clicked(who));
            return false;
        }

        /// <summary>バニラのheldObject描画だけを左側のテーブルタイルに合わせます。</summary>
        private static IEnumerable<CodeInstruction> ReplaceHeldObjectCenter(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            var center = AccessTools.PropertyGetter(typeof(NetRectangle), nameof(NetRectangle.Center));
            var replacement = AccessTools.Method(typeof(CustomFurnitureService), nameof(GetHeldObjectCenter));
            var heldField = AccessTools.Field(typeof(StardewValley.Object), nameof(StardewValley.Object.heldObject));
            int firstHeld = codes.FindIndex(c => c.LoadsField(heldField));
            if (firstHeld < 0 || center == null || replacement == null)
                return codes;
            var result = new List<CodeInstruction>(codes.Count);
            for (int i = 0; i < codes.Count; i++)
            {
                var code = codes[i];
                if (i > firstHeld && code.Calls(center))
                {
                    result.Add(new CodeInstruction(OpCodes.Ldarg_0));
                    result.Add(new CodeInstruction(OpCodes.Call, replacement) { labels = code.labels, blocks = code.blocks });
                }
                else
                    result.Add(code);
            }
            return result;
        }

        private static Point GetHeldObjectCenter(NetRectangle bounds, Furniture furniture)
        {
            var center = bounds.Center;
            if (furniture is not TV || !TryGetDefinition(furniture.ItemId, out var model) || model.TileProperties == null)
                return center;
            foreach (var entry in model.TileProperties)
            {
                if (!string.Equals(entry.Value?.Interaction, "Table", StringComparison.OrdinalIgnoreCase))
                    continue;
                var parts = entry.Key.Split(',');
                if (parts.Length == 2 && int.TryParse(parts[0].Trim(), out int x) && int.TryParse(parts[1].Trim(), out int y))
                {
                    int offsetX = 0;
                    int offsetY = 0;
                    string? itemOffset = entry.Value?.ItemOffset;
                    if (!string.IsNullOrWhiteSpace(itemOffset))
                    {
                        var offsetParts = itemOffset.Split(',');
                        if (offsetParts.Length == 2 &&
                            int.TryParse(offsetParts[0].Trim(), out int parsedX) &&
                            int.TryParse(offsetParts[1].Trim(), out int parsedY))
                        {
                            offsetX = parsedX;
                            offsetY = parsedY;
                        }
                    }
                    return new Point(bounds.X + x * 64 + 32 + offsetX * 4, bounds.Y + y * 64 + 32 + offsetY * 4);
                }
            }
            return center;
        }

        /// <summary>クリックしたタイルに対応する家具のActionを取得します。</summary>
        private static bool TryGetTileAction(GameLocation location, int tileX, int tileY, out string action)
        {
            action = string.Empty;
            foreach (Furniture furniture in location.furniture)
            {
                if (!TryGetDefinition(furniture.ItemId, out var model) || model.TileProperties == null)
                    continue;
                var bounds = furniture.boundingBox.Value;
                int pixelX = tileX * 64 + 32;
                int pixelY = tileY * 64 + 32;
                if (!bounds.Contains(pixelX, pixelY))
                    continue;
                int relativeX = tileX - bounds.Left / 64;
                int relativeY = tileY - bounds.Top / 64;
                string key = $"{relativeX}, {relativeY}";
                if (model.TileProperties.TryGetValue(key, out var property) &&
                    !string.IsNullOrWhiteSpace(property?.Action))
                {
                    action = property.Action;
                    return true;
                }
                // スペースを省いた座標表記も受け付けます。
                key = $"{relativeX},{relativeY}";
                if (model.TileProperties.TryGetValue(key, out property) &&
                    !string.IsNullOrWhiteSpace(property?.Action))
                {
                    action = property.Action;
                    return true;
                }
            }
            return false;
        }

        /// <summary>明示されたActionが成功した場合のみ、通常の家具操作を置き換えます。</summary>
        private static bool BeforeLocationCheckAction(GameLocation __instance,
            xTile.Dimensions.Location tileLocation, Farmer who, ref bool __result)
        {
            // TVの左側にアイテムを置く場合、右側のTV動作と分離します。
            foreach (Furniture furniture in __instance.furniture)
            {
                if (furniture is not TV || !TryGetInteraction(furniture, tileLocation.X, tileLocation.Y, out string interaction) ||
                    !interaction.Equals("Table", StringComparison.OrdinalIgnoreCase))
                    continue;
                if (who.CurrentItem != null && furniture.performObjectDropInAction(who.CurrentItem, false, who))
                {
                    __result = true;
                    return false;
                }
                if (furniture.heldObject.Value != null)
                {
                    __result = furniture.clicked(who);
                    return false;
                }
                __result = false;
                return false;
            }
            if (!TryGetTileAction(__instance, tileLocation.X, tileLocation.Y, out string action))
                return true;
            if (!__instance.performAction(action, who, tileLocation))
                return true;
            __result = true;
            return false;
        }

        /// <summary>Actionが設定された家具の衝突タイルを操作可能として扱います。</summary>
        private static void AfterIsActionableTile(GameLocation __instance, int xTile, int yTile,
            Farmer who, ref bool __result)
        {
            if (!__result && (TryGetTileAction(__instance, xTile, yTile, out _) ||
                __instance.furniture.Any(f => f is TV && TryGetInteraction(f, xTile, yTile, out _))))
                __result = true;
        }

        private static bool FitsFurniture(Furniture furniture, CustomFurnitureModel model)
        {
            if (model.ScreenArea == null)
                return true;
            var rect = furniture.sourceRect.Value;
            if (!IsScreenAreaInside(model.ScreenArea, rect))
            {
                Warn(furniture.ItemId, "ScreenArea is outside the furniture sprite.");
                return false;
            }
            if (model.RotationVariants is { Enabled: true, ScreenAreas: { } screenAreas })
            {
                foreach (var area in screenAreas.Values)
                {
                    if (!IsScreenAreaInside(area, rect))
                    {
                        Warn(furniture.ItemId, "RotationVariants.ScreenAreas contains an area outside the furniture sprite.");
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>各バリエーションの画面領域を共通スプライトの寸法内に制限します。</summary>
        private static bool IsScreenAreaInside(CustomFurnitureScreenArea area, Rectangle rect)
        {
            return area.X >= 0 && area.Y >= 0 && area.Width > 0 && area.Height > 0 &&
                (long)area.X + area.Width <= rect.Width && (long)area.Y + area.Height <= rect.Height;
        }

        /// <summary>選択中の番号に専用画面領域があれば優先し、なければ共通設定を使用します。</summary>
        private static CustomFurnitureScreenArea GetScreenArea(TV tv, CustomFurnitureModel model)
        {
            if (model.RotationVariants is { Enabled: true, ScreenAreas: { } screenAreas } variants)
            {
                string number = (GetVariant(tv, variants.Count) + 1).ToString(System.Globalization.CultureInfo.InvariantCulture);
                if (screenAreas.TryGetValue(number, out var area) && area != null)
                    return area;
            }
            return model.ScreenArea!;
        }

        //----------------------------------------
        // 家具生成
        //----------------------------------------

        /// <summary>通常家具として生成された対象だけをTVへ置き換えます。</summary>
        private static void AfterCreateFurniture(string itemId, Vector2? position, ref Furniture __result)
        {
            if (__result == null || __result.GetType() != typeof(Furniture) || !TryGetDefinition(itemId, out var model) ||
                !string.Equals(model.Type, "TV", StringComparison.OrdinalIgnoreCase) || !FitsFurniture(__result, model))
                return;
            try
            {
                // TVの通常回転は利用しないため、基準スプライトは回転数1の家具に限定します。
                if (__result.rotations.Value != 1)
                {
                    Warn(itemId, "Only furniture with one rotation is supported.");
                    return;
                }
                __result = new TV(itemId, position ?? Vector2.Zero);
            }
            catch (Exception ex)
            {
                Warn(itemId, $"Could not create TV: {ex.Message}");
            }
        }

        //----------------------------------------
        // 回転操作によるスプライト切り替え
        //----------------------------------------

        /// <summary>対象家具の回転を表示バリエーションの変更に置き換えます。</summary>
        private static bool BeforeRotate(Furniture __instance)
        {
            if (!TryGetDefinition(__instance.ItemId, out var model) || model.RotationVariants is not { Enabled: true } variants)
                return true;
            int current = GetVariant(__instance, variants.Count);
            __instance.modData[RotationVariantKey] = ((current + 1) % variants.Count).ToString(System.Globalization.CultureInfo.InvariantCulture);
            return false;
        }

        private static int GetVariant(Furniture furniture, int count)
        {
            if (furniture.modData.TryGetValue(RotationVariantKey, out string? stored) &&
                int.TryParse(stored, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out int index) &&
                index >= 0 && index < count)
                return index;
            return 0;
        }

        /// <summary>描画中のみ、Columnsで指定された列数に従って参照領域を選択します。</summary>
        private static void BeforeDraw(Furniture __instance, out Rectangle? __state)
        {
            __state = null;
            if (!TryGetDefinition(__instance.ItemId, out var model) || model.RotationVariants is not { Enabled: true } variants)
                return;

            DrawState state = drawStates.GetValue(__instance, _ => new DrawState());
            // TV.drawとFurniture.drawが入れ子になっても、画像の移動は最外層の1回だけ行います。
            if (state.Depth++ > 0)
                return;

            int index = GetVariant(__instance, variants.Count);
            if (index == 0)
                return;
            var rect = __instance.sourceRect.Value;
            __state = rect;
            int columns = variants.Columns ?? variants.Count;
            rect.X += rect.Width * (index % columns);
            rect.Y += rect.Height * (index / columns);
            __instance.sourceRect.Value = rect;
        }

        /// <summary>描画が終了したら元の参照領域に戻し、入れ子の深さを復元します。</summary>
        private static void AfterDraw(Furniture __instance, Rectangle? __state)
        {
            if (__state.HasValue)
                __instance.sourceRect.Value = __state.Value;
            if (drawStates.TryGetValue(__instance, out DrawState? state) && state.Depth > 0)
                state.Depth--;
        }

        //----------------------------------------
        // 番組描画
        //----------------------------------------

        /// <summary>TV.draw内の2つのアニメーション描画呼び出しだけを置き換えます。</summary>
        private static IEnumerable<CodeInstruction> ReplaceScreenDrawCalls(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            var original = AccessTools.Method(typeof(TemporaryAnimatedSprite), nameof(TemporaryAnimatedSprite.draw),
                new[] { typeof(SpriteBatch), typeof(bool), typeof(int), typeof(int), typeof(float) });
            var replacement = AccessTools.Method(typeof(CustomFurnitureService), nameof(DrawScreenSprite));
            drawPatched = codes.Count(c => c.Calls(original)) == 2;
            if (!drawPatched)
            {
                monitor.Log("CustomFurniture: TV.draw screen calls could not be identified; patch skipped.", LogLevel.Warn);
                return codes;
            }
            var result = new List<CodeInstruction>();
            foreach (var code in codes)
            {
                if (code.Calls(original))
                {
                    var loadTv = new CodeInstruction(OpCodes.Ldarg_0);
                    loadTv.labels.AddRange(code.labels);
                    loadTv.blocks.AddRange(code.blocks);
                    result.Add(loadTv);
                    result.Add(new CodeInstruction(OpCodes.Call, replacement));
                }
                else
                    result.Add(code);
            }
            return result;
        }

        /// <summary>
        /// 番組の基準座標（42x28）から画面領域へ変換します。
        /// バニラによるフレーム更新、番組進行、描画深度は維持します。
        /// </summary>
        private static void DrawScreenSprite(TemporaryAnimatedSprite sprite, SpriteBatch batch, bool localPosition,
            int xOffset, int yOffset, float extraAlpha, TV tv)
        {
            if (!TryGetDefinition(tv.ItemId, out var model) ||
                !string.Equals(model.Type, "TV", StringComparison.OrdinalIgnoreCase) || !FitsFurniture(tv, model))
            {
                sprite.draw(batch, localPosition, xOffset, yOffset, extraAlpha);
                return;
            }
            if (tv.isTemporarilyInvisible || sprite.alpha <= 0.000001f || sprite.currentParentTileIndex < 0 ||
                sprite.delayBeforeAnimationStart > 0 || sprite.ticksBeforeAnimationStart > 0 || sprite.Texture == null)
                return;

            var source = sprite.sourceRect;
            if (source.Width <= 0 || source.Height <= 0 || !float.IsFinite(sprite.scale) || sprite.scale <= 0)
                return;
            var area = GetScreenArea(tv, model);
            var box = tv.boundingBox.Value;
            var furnitureRect = tv.sourceRect.Value;
            var target = new Vector2(box.X + area.X * 4, box.Bottom - furnitureRect.Height * 4 + area.Y * 4);
            var size = new Vector2(area.Width * 4, area.Height * 4);
            float sx = size.X / 42f;
            float sy = size.Y / 28f;
            if (string.Equals(model.ScreenFit, "Contain", StringComparison.OrdinalIgnoreCase))
                sx = sy = Math.Min(sx, sy);
            else if (string.Equals(model.ScreenFit, "Cover", StringComparison.OrdinalIgnoreCase))
                sx = sy = Math.Max(sx, sy);
            var origin = target + (size - new Vector2(42 * sx, 28 * sy)) / 2;
            float vanillaScale = tv.getScreenSizeModifier();
            if (!float.IsFinite(vanillaScale) || vanillaScale <= 0)
                return;
            var offset = (sprite.Position - tv.getScreenPosition() + new Vector2(xOffset, yOffset)) / vanillaScale;
            var scale = new Vector2(sx, sy) * (sprite.scale / vanillaScale);
            var start = origin + offset * new Vector2(sx, sy);
            var end = start + new Vector2(source.Width * scale.X, source.Height * scale.Y);
            var clipMin = Vector2.Max(target, origin);
            var clipMax = Vector2.Min(target + size, origin + new Vector2(42 * sx, 28 * sy));
            var min = Vector2.Max(start, clipMin);
            var max = Vector2.Min(end, clipMax);
            if (max.X <= min.X || max.Y <= min.Y)
                return;

            // 端の部分ピクセルも切り取るため、各軸を最大3区間に分割します。
            // SpriteBatchの再開やScissorRectangleの変更は行いません。
            var xs = ClipAxis((min.X - start.X) / scale.X, (max.X - start.X) / scale.X, source.Width);
            var ys = ClipAxis((min.Y - start.Y) / scale.Y, (max.Y - start.Y) / scale.Y, source.Height);
            var effects = sprite.flipped ? SpriteEffects.FlipHorizontally : sprite.verticalFlipped ? SpriteEffects.FlipVertically : SpriteEffects.None;
            foreach (var x in xs)
            foreach (var y in ys)
            {
                int ix = sprite.flipped ? source.Width - x.Index - x.Count : x.Index;
                int iy = !sprite.flipped && sprite.verticalFlipped ? source.Height - y.Index - y.Count : y.Index;
                var clipped = new Rectangle(source.X + ix, source.Y + iy, x.Count, y.Count);
                var pos = Vector2.Max(min, start + new Vector2(x.Start * scale.X, y.Start * scale.Y));
                var finish = Vector2.Min(max, start + new Vector2(x.End * scale.X, y.End * scale.Y));
                var drawScale = (finish - pos) / new Vector2(x.Count, y.Count);
                if (drawScale.X <= 0 || drawScale.Y <= 0)
                    continue;
                batch.Draw(sprite.Texture, Game1.GlobalToLocal(Game1.viewport, pos), clipped,
                    sprite.color * sprite.alpha * extraAlpha, 0f, Vector2.Zero, drawScale, effects,
                    sprite.layerDepth + sprite.layerDepthOffset);
            }
        }

        private readonly record struct ClipSpan(int Index, int Count, float Start, float End);

        /// <summary>切り取り後の範囲を先頭・中央・末尾の描画区間へ変換します。</summary>
        private static List<ClipSpan> ClipAxis(float start, float end, int length)
        {
            start = Math.Clamp(start, 0, length);
            end = Math.Clamp(end, 0, length);
            var result = new List<ClipSpan>(3);
            if (end <= start)
                return result;
            int first = (int)Math.Floor(start);
            int last = (int)Math.Ceiling(end) - 1;
            if (first == last)
            {
                result.Add(new(first, 1, start, end));
                return result;
            }
            int middleStart = (int)Math.Ceiling(start);
            int middleEnd = (int)Math.Floor(end);
            if (middleStart > start)
                result.Add(new(first, 1, start, middleStart));
            if (middleEnd > middleStart)
                result.Add(new(middleStart, middleEnd - middleStart, middleStart, middleEnd));
            if (end > middleEnd)
                result.Add(new(middleEnd, 1, middleEnd, end));
            return result;
        }
    }
}
