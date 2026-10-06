using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System.Collections;
using System.Reflection;
using Ts_Core.Interfaces;
using Ts_Core.Models.ContentPatcherRelated;
using xTile;
using xTile.Tiles;

namespace Ts_Core.Services.ContentPatcherRelated
{
    /// <summary>
    /// Data Asset TsCore/PositionPickersに登録された定義を使用して、
    /// Content Patcher Content PackのGMCMへ位置選択ボタンを追加し、Map上で座標を視覚的に選択します。
    /// </summary>
    internal static class ContentPatcherPositionPickerService
    {
        //----------------------------------------
        // UI
        //----------------------------------------

        private const int ButtonWidth = 320;
        private const int ButtonHeight = 60;
        private const float PreviewAlpha = 0.65f;

        private static Rectangle ButtonBounds;
        private static long ButtonLastDrawTick;
        private static string? ButtonContentPackId;
        private static PositionPickerDefinition? ButtonDefinition;

        private static string? ButtonHoverText;
        private static long ButtonHoverDrawTick;

        //----------------------------------------
        // サービス状態
        //----------------------------------------

        private static IModHelper? Helper;
        private static IMonitor? Monitor;
        private static IGenericModConfigMenuApi? GmcmApi;
        private static bool Initialized;

        private static bool PickerStartPending;
        private static long PickerStartAfterTick;
        private static PositionPickerDefinition? PendingDefinition;

        private static PositionPickerDefinition? ActiveDefinition;
        private static Vector2 CursorTile;
        private static Map? PreviewMap;

        //----------------------------------------
        // 初期化
        //----------------------------------------

        internal static void Initialize(
            IModHelper helper,
            IMonitor monitor)
        {
            if (Initialized)
                return;

            Initialized = true;
            Helper = helper;
            Monitor = monitor;

            helper.Events.Input.ButtonPressed += OnButtonPressed;
            helper.Events.Input.CursorMoved += OnCursorMoved;
            helper.Events.Display.RenderedActiveMenu += OnRenderedActiveMenu;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            helper.Events.GameLoop.ReturnedToTitle += OnReturnedToTitle;
        }

        //----------------------------------------
        // GMCM登録
        //----------------------------------------

        /// <summary>GMCM再登録中に挿入するPosition Pickerの情報です。</summary>
        private static IManifest? RegisteringManifest;
        private static readonly List<(string Id, PositionPickerDefinition Definition)> PendingGmcmDefinitions = new();
        private static bool IsAddingPickerOption;
        private static bool ContentPatcherAddFieldPatched;

        /// <summary>
        /// Content PatcherがGMCM項目を登録する直前に、
        /// 対象Content PackのPosition Picker定義を準備します。
        /// </summary>
        internal static void BeginConfigMenuRegistration(
            object rawContentPack,
            object currentConfig,
            IModHelper helper,
            IMonitor monitor,
            Action saveAndApply,
            object contentPackMenu)
        {
            ClearConfigMenuRegistration();

            if (rawContentPack is not IContentPack contentPack)
                return;

            GmcmApi ??=
                helper.ModRegistry.GetApi<IGenericModConfigMenuApi>(
                    "spacechase0.GenericModConfigMenu");

            if (GmcmApi == null)
                return;

            EnsureContentPatcherAddFieldPatched(contentPackMenu, monitor);

            Dictionary<string, PositionPickerModel>? definitions =
                LoadDefinitions(helper, monitor);

            if (definitions == null)
                return;

            foreach ((string id, PositionPickerModel model) in definitions)
            {
                if (!string.Equals(
                        model.ContentPackId,
                        contentPack.Manifest.UniqueID,
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                PositionPickerDefinition? definition =
                    CreateDefinition(
                        contentPack,
                        currentConfig,
                        id,
                        model,
                        monitor,
                        saveAndApply);

                if (definition != null)
                    PendingGmcmDefinitions.Add((id, definition));
            }

            if (PendingGmcmDefinitions.Count > 0)
                RegisteringManifest = contentPack.Manifest;
        }

        /// <summary>
        /// Content PatcherのGMCM登録完了後、AfterFieldを見つけられなかったPickerを
        /// 従来どおりメニュー末尾へ追加します。
        /// </summary>
        internal static void CompleteConfigMenuRegistration(
            IModHelper helper,
            IMonitor monitor)
        {
            try
            {
                foreach ((string id, PositionPickerDefinition definition)
                    in PendingGmcmDefinitions.ToArray())
                {
                    AddGmcmOption(id, definition, helper);

                    if (!string.IsNullOrWhiteSpace(definition.AfterField))
                    {
                        monitor.Log(
                            $"Could not find GMCM field '{definition.AfterField}' for Position Picker '{id}'. " +
                            "The button was added at the end of the menu.",
                            LogLevel.Trace);
                    }
                }
            }
            finally
            {
                ClearConfigMenuRegistration();
            }
        }

        /// <summary>Content PatcherがConfigSchemaの各項目をGMCMへ登録するAddFieldを監視できるようにします。</summary>
        private static void EnsureContentPatcherAddFieldPatched(
            object contentPackMenu,
            IMonitor monitor)
        {
            if (ContentPatcherAddFieldPatched)
                return;

            MethodInfo? addFieldMethod =
                contentPackMenu.GetType().GetMethod(
                    "AddField",
                    BindingFlags.Instance
                    | BindingFlags.NonPublic);

            if (addFieldMethod == null)
            {
                monitor.Log(
                    "Could not hook Content Patcher GMCM AddField(). " +
                    "Position Picker buttons will fall back to the end of the menu.",
                    LogLevel.Trace);
                return;
            }

            ParameterInfo[] parameters = addFieldMethod.GetParameters();

            if (parameters.Length != 3
                || parameters[1].ParameterType != typeof(string))
            {
                monitor.Log(
                    "Content Patcher GMCM AddField() has an unexpected signature. " +
                    "Position Picker buttons will fall back to the end of the menu.",
                    LogLevel.Trace);
                return;
            }

            MethodInfo postfix =
                typeof(ContentPatcherPositionPickerService)
                    .GetMethod(
                        nameof(AfterContentPatcherFieldAdded),
                        BindingFlags.Static | BindingFlags.NonPublic)
                ?? throw new InvalidOperationException(
                    "Could not access Position Picker Content Patcher AddField postfix.");

            Harmony harmony =
                new("Tikamin557.TsCore.PositionPicker.ConfigMenuInsertion");

            harmony.Patch(
                addFieldMethod,
                postfix: new HarmonyMethod(postfix));

            ContentPatcherAddFieldPatched = true;
        }

        /// <summary>Content PatcherがConfigSchemaの1項目をGMCMへ追加した直後に呼ばれます。</summary>
        private static void AfterContentPatcherFieldAdded(string __1)
        {
            ContentPatcherPsrRoomPresetButtonService.AfterContentPatcherFieldAdded(__1);

            if (IsAddingPickerOption
                || RegisteringManifest == null
                || PendingGmcmDefinitions.Count == 0
                || Helper == null)
            {
                return;
            }

            foreach ((string id, PositionPickerDefinition definition)
                in PendingGmcmDefinitions.ToArray())
            {
                if (!string.Equals(
                        definition.AfterField,
                        __1,
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                AddGmcmOption(id, definition, Helper);
                PendingGmcmDefinitions.Remove((id, definition));
            }

        }

        /// <summary>Position PickerボタンをGMCMへ追加します。</summary>
        private static void AddGmcmOption(
            string id,
            PositionPickerDefinition definition,
            IModHelper helper)
        {
            if (GmcmApi == null || RegisteringManifest == null)
                return;

            string pickerFieldId = $"TsCore.PositionPicker.{id}";

            try
            {
                IsAddingPickerOption = true;

                GmcmApi.AddComplexOption(
                    RegisteringManifest,
                    name: () => GetGmcmText(
                        definition.GmcmName,
                        "positionPicker.name",
                        helper),
                    draw: (spriteBatch, position) =>
                        DrawGmcmButton(
                            spriteBatch,
                            position,
                            definition),
                    tooltip: () => GetGmcmText(
                        definition.GmcmDescription,
                        "positionPicker.description",
                        helper),
                    height: () => ButtonHeight,
                    fieldId: pickerFieldId);
            }
            finally
            {
                IsAddingPickerOption = false;
            }
        }

        /// <summary>GMCM再登録用の一時状態をクリアします。</summary>
        private static void ClearConfigMenuRegistration()
        {
            RegisteringManifest = null;
            PendingGmcmDefinitions.Clear();
            IsAddingPickerOption = false;
        }

        /// <summary>
        /// 指定Content Pack用のPosition Picker定義がData Assetに存在するか確認します。
        /// </summary>
        internal static bool HasDefinitionsForContentPack(
            string contentPackId,
            IModHelper helper,
            IMonitor monitor)
        {
            Dictionary<string, PositionPickerModel>? definitions =
                LoadDefinitions(helper, monitor);

            return definitions?.Values.Any(model =>
                string.Equals(
                    model.ContentPackId,
                    contentPackId,
                    StringComparison.OrdinalIgnoreCase))
                == true;
        }

        /// <summary>Position Picker Data Assetを読み込みます。</summary>
        private static Dictionary<string, PositionPickerModel>? LoadDefinitions(
            IModHelper helper,
            IMonitor monitor)
        {
            try
            {
                return helper.GameContent.Load<
                    Dictionary<string, PositionPickerModel>>(
                        PositionPickerDataService.AssetName);
            }
            catch (Exception ex)
            {
                monitor.Log(
                    $"Failed to load '{PositionPickerDataService.AssetName}'.\n{ex}",
                    LogLevel.Warn);

                return null;
            }
        }

        //----------------------------------------
        // GMCMボタン
        //----------------------------------------

        private static void DrawGmcmButton(
            SpriteBatch spriteBatch,
            Vector2 position,
            PositionPickerDefinition definition)
        {
            ButtonBounds = new Rectangle(
                (int)position.X,
                (int)position.Y,
                ButtonWidth,
                ButtonHeight);

            ButtonLastDrawTick = Game1.ticks;
            ButtonContentPackId = definition.ContentPackId;
            ButtonDefinition = definition;

            bool available =
                Context.IsWorldReady
                && IsTargetLocation(definition);

            Point mouse = Game1.getMousePosition();
            bool hover = ButtonBounds.Contains(mouse);

            float alpha = available ? (hover ? 1f : 0.9f) : 0.45f;

            IClickableMenu.drawTextureBox(
                spriteBatch,
                ButtonBounds.X,
                ButtonBounds.Y,
                ButtonBounds.Width,
                ButtonBounds.Height,
                available ? Color.White * alpha : Color.White);

            // 無効時にボタン全体を半透明化すると、drawTextureBoxの影が
            // ボタン本体越しに透けて見えるため、内側だけを暗くします。
            if (!available)
            {
                spriteBatch.Draw(
                    Game1.fadeToBlackRect,
                    new Rectangle(
                        ButtonBounds.X + 4,
                        ButtonBounds.Y + 4,
                        ButtonBounds.Width - 8,
                        ButtonBounds.Height - 8),
                    Color.White * 0.45f);
            }

            string text = Helper != null
                ? GetGmcmText(
                    definition.GmcmButton,
                    "positionPicker.button",
                    Helper)
                : "Select Position on Map";

            Vector2 textSize =
                Game1.smallFont.MeasureString(text);

            spriteBatch.DrawString(
                Game1.smallFont,
                text,
                new Vector2(
                    ButtonBounds.Center.X - textSize.X / 2f,
                    ButtonBounds.Center.Y - textSize.Y / 2f),
                Game1.textColor * alpha);

            // 左側の項目名はGMCM標準Tooltipに任せ、
            // ボタン上だけ現在の状態に応じたHover Textを最前面へ表示します。
            if (hover && Helper != null)
            {
                ButtonHoverText = GetButtonTooltip(
                    definition,
                    Helper);
                ButtonHoverDrawTick = Game1.ticks;
            }
        }

        private static string GetButtonTooltip(
            PositionPickerDefinition definition,
            IModHelper helper)
        {
            if (!Context.IsWorldReady)
            {
                return GetGmcmText(
                    definition.GmcmWorldRequired,
                    "positionPicker.worldRequired",
                    helper);
            }

            if (!IsTargetLocation(definition))
            {
                if (!string.IsNullOrWhiteSpace(
                        definition.GmcmLocationRequired))
                {
                    return definition.GmcmLocationRequired;
                }

                return helper.Translation.Get(
                    "positionPicker.locationRequired",
                    new
                    {
                        Location = definition.Location
                    });
            }

            return GetGmcmText(
                definition.GmcmDescription,
                "positionPicker.description",
                helper);
        }

        /// <summary>Content Pack側のGMCM表示文字列を取得し、未指定時はTsCore標準の翻訳へフォールバックします。</summary>
        private static string GetGmcmText(
            string customText,
            string fallbackKey,
            IModHelper helper)
        {
            return !string.IsNullOrWhiteSpace(customText)
                ? customText
                : helper.Translation.Get(fallbackKey);
        }

        /// <summary>
        /// GMCMの通常Tooltipより後に、Position Pickerボタン用Hover Textを描画します。
        /// </summary>
        private static void OnRenderedActiveMenu(
            object? sender,
            RenderedActiveMenuEventArgs e)
        {
            if (ButtonHoverDrawTick != Game1.ticks
                || string.IsNullOrWhiteSpace(ButtonHoverText)
                || GmcmApi == null
                || string.IsNullOrWhiteSpace(ButtonContentPackId))
            {
                return;
            }

            string text = ButtonHoverText;
            ButtonHoverText = null;

            if (!GmcmApi.TryGetCurrentMenu(
                    out IManifest currentMod,
                    out _)
                || !string.Equals(
                    currentMod.UniqueID,
                    ButtonContentPackId,
                    StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            DrawButtonHoverText(
                e.SpriteBatch,
                text);
        }

        /// <summary>
        /// Position Pickerボタン用Hover Textを画面内に収まる位置へ描画します。
        /// </summary>
        private static void DrawButtonHoverText(
            SpriteBatch spriteBatch,
            string text)
        {
            const int padding = 16;
            const int screenMargin = 8;
            const int cursorOffset = 32;
            const int maxTextWidth = 520;

            string wrappedText =
                Game1.parseText(
                    text,
                    Game1.smallFont,
                    maxTextWidth);

            Vector2 textSize =
                Game1.smallFont.MeasureString(
                    wrappedText);

            int boxWidth =
                (int)MathF.Ceiling(textSize.X)
                + padding * 2;

            int boxHeight =
                (int)MathF.Ceiling(textSize.Y)
                + padding * 2;

            Point mousePosition =
                Game1.getMousePosition();

            int hoverX =
                mousePosition.X
                - boxWidth
                - cursorOffset;

            int hoverY =
                mousePosition.Y
                + cursorOffset;

            if (hoverX < screenMargin)
            {
                hoverX =
                    mousePosition.X
                    + cursorOffset;
            }

            hoverX = Math.Max(screenMargin, hoverX);
            hoverY = Math.Max(screenMargin, hoverY);

            IClickableMenu.drawTextureBox(
                spriteBatch,
                Game1.menuTexture,
                new Rectangle(0, 256, 60, 60),
                hoverX,
                hoverY,
                boxWidth,
                boxHeight,
                Color.White,
                1f,
                drawShadow: true);

            spriteBatch.DrawString(
                Game1.smallFont,
                wrappedText,
                new Vector2(
                    hoverX + padding,
                    hoverY + padding),
                Game1.textColor);
        }

        //----------------------------------------
        // 入力
        //----------------------------------------

        private static void OnButtonPressed(
            object? sender,
            ButtonPressedEventArgs e)
        {
            if (Helper == null)
                return;

            //----------------------------------------
            // Position Picker実行中
            //----------------------------------------

            if (ActiveDefinition != null)
            {
                // 配置中の入力はPositionPickerMenu側で処理します。
                return;
            }

            //----------------------------------------
            // GMCMボタン
            //----------------------------------------

            if (e.Button != SButton.MouseLeft
                || ButtonDefinition == null
                || ButtonContentPackId == null
                || Game1.ticks > ButtonLastDrawTick + 1
                || GmcmApi == null)
            {
                return;
            }

            if (!GmcmApi.TryGetCurrentMenu(
                    out IManifest currentMod,
                    out _)
                || !string.Equals(
                    currentMod.UniqueID,
                    ButtonContentPackId,
                    StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            Vector2 cursorPosition =
                Utility.ModifyCoordinatesForUIScale(
                    e.Cursor.ScreenPixels);

            if (!ButtonBounds.Contains(
                    cursorPosition.ToPoint()))
            {
                return;
            }

            if (!Context.IsWorldReady
                || !IsTargetLocation(ButtonDefinition))
            {
                Game1.playSound("cancel");
                return;
            }

            Helper.Input.Suppress(e.Button);
            Game1.playSound("smallSelect");

            PendingDefinition = ButtonDefinition;
            PickerStartPending = true;
            PickerStartAfterTick = Game1.ticks + 2;

            Game1.activeClickableMenu?.exitThisMenu();
        }

        private static void OnCursorMoved(
            object? sender,
            CursorMovedEventArgs e)
        {
            if (ActiveDefinition == null)
                return;

            CursorTile = e.NewPosition.GrabTile;
        }

        //----------------------------------------
        // Picker開始待ち
        //----------------------------------------

        private static void OnUpdateTicked(
            object? sender,
            UpdateTickedEventArgs e)
        {
            if (!PickerStartPending
                || Game1.ticks < PickerStartAfterTick)
            {
                return;
            }

            // Androidを含め、GMCMの終了処理が完了するまで待機します。
            if (TitleMenu.subMenu != null)
                return;

            PickerStartPending = false;

            if (PendingDefinition == null
                || !Context.IsWorldReady
                || !IsTargetLocation(PendingDefinition))
            {
                PendingDefinition = null;
                return;
            }

            StartPicker(PendingDefinition);
            PendingDefinition = null;
        }

        private static void StartPicker(
            PositionPickerDefinition definition)
        {
            if (Helper == null
                || Monitor == null)
            {
                return;
            }

            try
            {
                PreviewMap =
                    Helper.GameContent.Load<Map>(
                        definition.PreviewMap);

                ActiveDefinition = definition;
                CursorTile = Helper.Input.GetCursorPosition().GrabTile;

                // バニラの建築配置と同様、通常ゲーム操作へ戻すのではなく
                // 配置専用メニューを開いたままMap上の位置を選択します。
                Game1.activeClickableMenu = new PositionPickerMenu();

                Game1.playSound("smallSelect");
            }
            catch (Exception ex)
            {
                PreviewMap = null;
                ActiveDefinition = null;

                Monitor.Log(
                    $"Failed to start Position Picker for '{definition.ContentPackId}'.\n{ex}",
                    LogLevel.Warn);

                Game1.playSound("cancel");
            }
        }

        //----------------------------------------
        // プレビュー描画
        //----------------------------------------

        private static void DrawPickerPreview(
            SpriteBatch spriteBatch)
        {
            if (ActiveDefinition == null
                || PreviewMap == null
                || !Context.IsWorldReady
                || !IsTargetLocation(ActiveDefinition))
            {
                return;
            }

            UpdateCursorTileFromMouse();

            int originX =
                (int)CursorTile.X
                - ActiveDefinition.AnchorX;

            int originY =
                (int)CursorTile.Y
                - ActiveDefinition.AnchorY;

            foreach (xTile.Layers.Layer layer
                in PreviewMap.Layers)
            {
                for (int y = 0; y < layer.LayerHeight; y++)
                {
                    for (int x = 0; x < layer.LayerWidth; x++)
                    {
                        Tile? tile = layer.Tiles[x, y];

                        if (tile == null)
                            continue;

                        DrawPreviewTile(
                            spriteBatch,
                            tile,
                            originX + x,
                            originY + y);
                    }
                }
            }

            DrawPickerGuide(
                spriteBatch,
                originX,
                originY);
        }

        private static void DrawPreviewTile(
            SpriteBatch spriteBatch,
            Tile tile,
            int tileX,
            int tileY)
        {
            if (Helper == null)
                return;

            StaticTile? staticTile =
                tile switch
                {
                    StaticTile value => value,
                    AnimatedTile value
                        when value.TileFrames.Length > 0
                        => value.TileFrames[0],
                    _ => null
                };

            if (staticTile?.TileSheet == null)
                return;

            string imageSource =
                staticTile.TileSheet.ImageSource
                    .Replace('\\', '/');

            Texture2D? texture =
                TryLoadTileSheetTexture(
                    imageSource);

            if (texture == null)
                return;

            // xTile自身がTileIndexから求めた元Texture上の範囲を使用します。
            // SheetSizeとTexture全体サイズは必ずしも単純対応しないため、
            // TsCore側では列数・行数からsource位置を再計算しません。
            var tileImageBounds =
                staticTile.TileSheet.GetTileImageBounds(
                    staticTile.TileIndex);

            Rectangle sourceRect =
                new Rectangle(
                    tileImageBounds.X,
                    tileImageBounds.Y,
                    tileImageBounds.Width,
                    tileImageBounds.Height);

            Vector2 screenPosition =
                Game1.GlobalToLocal(
                    Game1.viewport,
                    new Vector2(
                        tileX * Game1.tileSize,
                        tileY * Game1.tileSize));

            // IClickableMenu.draw()はUI Scaleが適用されたSpriteBatch上で描画されます。
            // 一方、GlobalToLocal()の結果はゲーム世界の画面ピクセル座標なので、
            // そのまま描画するとUI Scale分だけ位置・サイズが縮小/拡大されます。
            // UI座標へ変換してから描画し、実際のMapタイル境界と一致させます。
            Vector2 uiScreenPosition =
                Utility.ModifyCoordinatesForUIScale(
                    screenPosition);

            Vector2 uiTileSize =
                Utility.ModifyCoordinatesForUIScale(
                    new Vector2(
                        Game1.tileSize,
                        Game1.tileSize));

            // GameContentから取得したMap用Textureは、Stardew Valley側で
            // ゲーム描画用サイズへ拡大済みの場合があります。
            // pixelZoomをさらに掛けず、画面上で1Mapタイルと同じサイズへ固定します。
            Rectangle destinationRect =
                new Rectangle(
                    (int)MathF.Round(uiScreenPosition.X),
                    (int)MathF.Round(uiScreenPosition.Y),
                    (int)MathF.Round(uiTileSize.X),
                    (int)MathF.Round(uiTileSize.Y));

            // TMXTileはTiledの回転・反転情報をxTileのTile Propertiesへ
            // @Rotation / @Flip として保持します。
            // Position Pickerでも同じ情報を反映し、実際のMap表示と向きを一致させます。
            float rotation =
                MathHelper.ToRadians(
                    GetPreviewTileRotation(tile));

            SpriteEffects spriteEffects =
                GetPreviewTileSpriteEffects(tile);

            // 回転時も1タイルの領域内に収まるよう、タイル中央を基準に描画します。
            Vector2 drawPosition =
                new Vector2(
                    destinationRect.Center.X,
                    destinationRect.Center.Y);

            Vector2 sourceOrigin =
                new Vector2(
                    sourceRect.Width / 2f,
                    sourceRect.Height / 2f);

            float scale =
                destinationRect.Width
                / (float)sourceRect.Width;

            spriteBatch.Draw(
                texture,
                drawPosition,
                sourceRect,
                Color.White * PreviewAlpha,
                rotation,
                sourceOrigin,
                scale,
                spriteEffects,
                1f);
        }

        /// <summary>TMXTileが保持しているタイル回転角度を取得します。</summary>
        private static float GetPreviewTileRotation(
            Tile tile)
        {
            if (!tile.Properties.TryGetValue(
                    "@Rotation",
                    out var value)
                || !float.TryParse(
                    value?.ToString(),
                    System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out float rotation))
            {
                return 0f;
            }

            return rotation;
        }

        /// <summary>TMXTileが保持しているタイル反転情報を取得します。</summary>
        private static SpriteEffects GetPreviewTileSpriteEffects(
            Tile tile)
        {
            if (!tile.Properties.TryGetValue(
                    "@Flip",
                    out var value)
                || !int.TryParse(
                    value?.ToString(),
                    System.Globalization.NumberStyles.Integer,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out int effects))
            {
                return SpriteEffects.None;
            }

            return (SpriteEffects)effects;
        }

        private static Texture2D? TryLoadTileSheetTexture(
            string imageSource)
        {
            if (Helper == null)
                return null;

            string[] candidates =
            {
                imageSource,
                "Maps/" + imageSource
            };

            foreach (string assetName in candidates.Distinct(
                StringComparer.OrdinalIgnoreCase))
            {
                try
                {
                    return Helper.GameContent.Load<Texture2D>(
                        assetName);
                }
                catch
                {
                    // 次の候補を試します。
                }
            }

            return null;
        }

        private static void DrawPickerGuide(
            SpriteBatch spriteBatch,
            int originX,
            int originY)
        {
            if (Helper == null
                || ActiveDefinition == null
                || PreviewMap == null)
            {
                return;
            }

            Vector2 topLeft =
                Game1.GlobalToLocal(
                    Game1.viewport,
                    new Vector2(
                        originX * Game1.tileSize,
                        originY * Game1.tileSize));

            int previewWidth =
                PreviewMap.Layers.Max(
                    layer => layer.LayerWidth)
                * Game1.tileSize;

            int previewHeight =
                PreviewMap.Layers.Max(
                    layer => layer.LayerHeight)
                * Game1.tileSize;

            Vector2 uiTopLeft =
                Utility.ModifyCoordinatesForUIScale(
                    topLeft);

            Vector2 uiPreviewSize =
                Utility.ModifyCoordinatesForUIScale(
                    new Vector2(
                        previewWidth,
                        previewHeight));

            Rectangle previewBounds =
                new Rectangle(
                    (int)MathF.Round(uiTopLeft.X),
                    (int)MathF.Round(uiTopLeft.Y),
                    (int)MathF.Round(uiPreviewSize.X),
                    (int)MathF.Round(uiPreviewSize.Y));

            const int border = 3;

            spriteBatch.Draw(
                Game1.fadeToBlackRect,
                new Rectangle(previewBounds.X, previewBounds.Y, previewBounds.Width, border),
                Color.Lime * 0.8f);
            spriteBatch.Draw(
                Game1.fadeToBlackRect,
                new Rectangle(previewBounds.X, previewBounds.Bottom - border, previewBounds.Width, border),
                Color.Lime * 0.8f);
            spriteBatch.Draw(
                Game1.fadeToBlackRect,
                new Rectangle(previewBounds.X, previewBounds.Y, border, previewBounds.Height),
                Color.Lime * 0.8f);
            spriteBatch.Draw(
                Game1.fadeToBlackRect,
                new Rectangle(previewBounds.Right - border, previewBounds.Y, border, previewBounds.Height),
                Color.Lime * 0.8f);

            string text =
                Helper.Translation.Get(
                    "positionPicker.guide",
                    new
                    {
                        X = (int)CursorTile.X,
                        Y = (int)CursorTile.Y
                    });

            Vector2 guidePosition =
                new Vector2(24f, 24f);

            const int guidePaddingX = 12;
            const int guidePaddingY = 8;
            const int noteSpacing = 8;
            const int maxNoteWidth = 640;

            string noteText =
                string.IsNullOrWhiteSpace(ActiveDefinition.PreviewNote)
                    ? ""
                    : Game1.parseText(
                        ActiveDefinition.PreviewNote,
                        Game1.smallFont,
                        maxNoteWidth);

            Vector2 guideSize =
                Game1.smallFont.MeasureString(text);

            Vector2 noteSize =
                string.IsNullOrWhiteSpace(noteText)
                    ? Vector2.Zero
                    : Game1.smallFont.MeasureString(noteText);

            int contentWidth =
                (int)MathF.Ceiling(
                    Math.Max(guideSize.X, noteSize.X));

            int contentHeight =
                (int)MathF.Ceiling(guideSize.Y);

            if (!string.IsNullOrWhiteSpace(noteText))
            {
                contentHeight +=
                    noteSpacing
                    + (int)MathF.Ceiling(noteSize.Y);
            }

            Rectangle guideBackground =
                new Rectangle(
                    (int)guidePosition.X - guidePaddingX,
                    (int)guidePosition.Y - guidePaddingY,
                    contentWidth + guidePaddingX * 2,
                    contentHeight + guidePaddingY * 2);

            // 明るいMap上でも座標・操作説明を読みやすくするため、
            // テキストの背面だけを少し透過した黒で覆います。
            spriteBatch.Draw(
                Game1.staminaRect,
                guideBackground,
                Color.Black * 0.8f);

            spriteBatch.DrawString(
                Game1.smallFont,
                text,
                guidePosition,
                Color.White);

            if (!string.IsNullOrWhiteSpace(noteText))
            {
                spriteBatch.DrawString(
                    Game1.smallFont,
                    noteText,
                    new Vector2(
                        guidePosition.X,
                        guidePosition.Y
                            + guideSize.Y
                            + noteSpacing),
                    Color.White);
            }
        }

        //----------------------------------------
        // 確定 / キャンセル
        //----------------------------------------

        private static void ConfirmPosition()
        {
            if (ActiveDefinition == null
                || Helper == null
                || Monitor == null)
            {
                return;
            }

            PositionPickerDefinition definition =
                ActiveDefinition;

            int x = (int)CursorTile.X;
            int y = (int)CursorTile.Y;

            try
            {
                SetConfigValue(
                    definition.CurrentConfig,
                    definition.XField,
                    x.ToString());

                SetConfigValue(
                    definition.CurrentConfig,
                    definition.YField,
                    y.ToString());

                definition.SaveAndApply();

                Monitor.Log(
                    $"Position Picker updated '{definition.ContentPackId}': " +
                    $"{definition.XField}={x}, {definition.YField}={y}.",
                    LogLevel.Trace);

                Game1.playSound("coin");
            }
            catch (Exception ex)
            {
                Monitor.Log(
                    $"Failed to save Position Picker coordinates for '{definition.ContentPackId}'.\n{ex}",
                    LogLevel.Warn);

                Game1.playSound("cancel");
            }
            finally
            {
                StopPicker();
            }
        }

        private static void CancelPicker()
        {
            Game1.playSound("cancel");
            StopPicker();
        }

        private static void StopPicker()
        {
            if (Game1.activeClickableMenu is PositionPickerMenu menu)
            {
                menu.RestoreGameState();
                Game1.activeClickableMenu = null;
            }

            ActiveDefinition = null;
            PreviewMap = null;
        }

        private static void OnReturnedToTitle(
            object? sender,
            ReturnedToTitleEventArgs e)
        {
            PickerStartPending = false;
            PendingDefinition = null;
            StopPicker();
        }

        //----------------------------------------
        // 配置専用メニュー
        //----------------------------------------

        /// <summary>
        /// バニラの建築配置画面と同じく、通常プレイヤー操作を止めた状態で
        /// ゲーム世界上のタイルを選択するための専用メニューです。
        /// </summary>
        private sealed class PositionPickerMenu : IClickableMenu
        {
            private const int EdgeScrollSize = 64;
            private const int ScrollSpeed = 16;
            private const int ViewportHorizontalMarginTiles = 15;
            private const int ViewportVerticalMarginTiles = 6;

            private readonly bool PreviousDisplayHud;
            private readonly bool PreviousViewportFreeze;
            private bool StateRestored;

            public PositionPickerMenu()
                : base(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height)
            {
                PreviousDisplayHud = Game1.displayHUD;
                PreviousViewportFreeze = Game1.viewportFreeze;

                // バニラの建築配置画面と同様、配置中は通常HUDを隠し、
                // プレイヤー追従によるviewport更新を止めます。
                Game1.displayHUD = false;
                Game1.viewportFreeze = true;
                Game1.displayFarmer = true;

                UpdateCursorTileFromMouse();
            }

            public override void update(GameTime time)
            {
                base.update(time);

                if (ActiveDefinition == null
                    || !Context.IsWorldReady
                    || !IsTargetLocation(ActiveDefinition))
                {
                    CancelPicker();
                    return;
                }

                MoveViewport();
                UpdateCursorTileFromMouse();
            }

            public override void receiveLeftClick(int x, int y, bool playSound = true)
            {
                if (ActiveDefinition == null)
                    return;

                UpdateCursorTileFromMouse();
                ConfirmPosition();
            }

            public override void receiveRightClick(int x, int y, bool playSound = true)
            {
                CancelPicker();
            }

            public override void receiveKeyPress(Keys key)
            {
                if (key == Keys.Escape)
                {
                    CancelPicker();
                    return;
                }

                base.receiveKeyPress(key);
            }

            public override void draw(SpriteBatch b)
            {
                UpdateCursorTileFromMouse();
                DrawPickerPreview(b);

                // 配置プレビュー中はタイルとガイドを見やすくするため、
                // ゲーム内マウスカーソルを描画しません。
            }

            /// <summary>配置モード開始前のHUD・viewport状態へ戻します。</summary>
            public void RestoreGameState()
            {
                if (StateRestored)
                    return;

                StateRestored = true;
                Game1.displayHUD = PreviousDisplayHud;
                Game1.viewportFreeze = PreviousViewportFreeze;
            }

            /// <summary>
            /// 建築配置画面と同様に、画面端または方向キー/WASDでviewportを移動します。
            /// 通常のプレイヤー追従処理には任せず、現在Map全体を移動範囲として扱います。
            /// </summary>
            private static void MoveViewport()
            {
                if (Game1.currentLocation?.Map == null)
                    return;

                int mouseX = Game1.getMouseX();
                int mouseY = Game1.getMouseY();
                KeyboardState keyboard = Keyboard.GetState();

                int moveX = 0;
                int moveY = 0;

                if (mouseX <= EdgeScrollSize
                    || keyboard.IsKeyDown(Keys.Left)
                    || keyboard.IsKeyDown(Keys.A))
                {
                    moveX -= ScrollSpeed;
                }
                else if (mouseX >= Game1.uiViewport.Width - EdgeScrollSize
                    || keyboard.IsKeyDown(Keys.Right)
                    || keyboard.IsKeyDown(Keys.D))
                {
                    moveX += ScrollSpeed;
                }

                if (mouseY <= EdgeScrollSize
                    || keyboard.IsKeyDown(Keys.Up)
                    || keyboard.IsKeyDown(Keys.W))
                {
                    moveY -= ScrollSpeed;
                }
                else if (mouseY >= Game1.uiViewport.Height - EdgeScrollSize
                    || keyboard.IsKeyDown(Keys.Down)
                    || keyboard.IsKeyDown(Keys.S))
                {
                    moveY += ScrollSpeed;
                }

                if (moveX == 0 && moveY == 0)
                    return;

                xTile.Layers.Layer? layer = Game1.currentLocation.Map.Layers.FirstOrDefault();

                if (layer == null)
                    return;

                int mapWidth = layer.LayerWidth * Game1.tileSize;
                int mapHeight = layer.LayerHeight * Game1.tileSize;
                int horizontalViewportMargin =
                    ViewportHorizontalMarginTiles
                    * Game1.tileSize;

                int verticalViewportMargin =
                    ViewportVerticalMarginTiles
                    * Game1.tileSize;

                // Map端のタイルも左上ガイド等に隠れず確認できるよう、
                // 配置モード中だけ通常のMap範囲よりviewportを余分に移動できます。
                // 横方向は15タイル、縦方向は画面がMap外へ行き過ぎないよう6タイルです。
                int minX = -horizontalViewportMargin;
                int minY = -verticalViewportMargin;
                int maxX =
                    Math.Max(
                        0,
                        mapWidth - Game1.viewport.Width)
                    + horizontalViewportMargin;
                int maxY =
                    Math.Max(
                        0,
                        mapHeight - Game1.viewport.Height)
                    + verticalViewportMargin;

                Game1.viewport.X =
                    Math.Clamp(
                        Game1.viewport.X + moveX,
                        minX,
                        maxX);

                Game1.viewport.Y =
                    Math.Clamp(
                        Game1.viewport.Y + moveY,
                        minY,
                        maxY);
            }
        }

        /// <summary>
        /// 現在のマウス画面座標とviewportから、カーソル直下のMapタイルを更新します。
        /// </summary>
        private static void UpdateCursorTileFromMouse()
        {
            if (!Context.IsWorldReady)
                return;

            int worldX = Game1.getMouseX() + Game1.viewport.X;
            int worldY = Game1.getMouseY() + Game1.viewport.Y;

            xTile.Layers.Layer? layer =
                Game1.currentLocation?.Map?.Layers.FirstOrDefault();

            if (layer == null)
                return;

            // viewport自体はMap外まで移動できますが、選択座標は
            // 実際に存在するMapタイルの範囲内に限定します。
            int tileX =
                Math.Clamp(
                    (int)Math.Floor(
                        worldX / (double)Game1.tileSize),
                    0,
                    Math.Max(0, layer.LayerWidth - 1));

            int tileY =
                Math.Clamp(
                    (int)Math.Floor(
                        worldY / (double)Game1.tileSize),
                    0,
                    Math.Max(0, layer.LayerHeight - 1));

            CursorTile = new Vector2(
                tileX,
                tileY);
        }

        //----------------------------------------
        // Config操作
        //----------------------------------------

        private static void SetConfigValue(
            object config,
            string fieldName,
            string value)
        {
            if (config is not IEnumerable enumerable)
            {
                throw new InvalidOperationException(
                    "Could not enumerate Content Patcher Config.");
            }

            foreach (object? item in enumerable)
            {
                if (item == null)
                    continue;

                string? key =
                    ContentPatcherReloadService.GetPropertyValue(
                        item,
                        "Key")
                        ?.ToString();

                if (!string.Equals(
                        key,
                        fieldName,
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                object? field =
                    ContentPatcherReloadService.GetPropertyValue(
                        item,
                        "Value");

                if (field == null)
                    break;

                MethodInfo? setValueMethod =
                    field.GetType()
                        .GetMethods(
                            BindingFlags.Instance
                            | BindingFlags.Public
                            | BindingFlags.NonPublic)
                        .FirstOrDefault(method =>
                            method.Name == "SetValue"
                            && method.GetParameters().Length == 1);

                if (setValueMethod == null)
                    break;

                Type valueType =
                    setValueMethod.GetParameters()[0].ParameterType;

                object setValue;

                if (valueType == typeof(string))
                {
                    setValue = value;
                }
                else if (valueType == typeof(string[]))
                {
                    setValue = new[] { value };
                }
                else
                {
                    // Content Patcher 2.9.1のConfigField.SetValueは
                    // IInvariantSetを受け取ります。CP自身のGMCM連携と同じく、
                    // ContentPatcher.Framework.InvariantSets.FromValue(string)で
                    // 単一値を生成して渡します。
                    Type? invariantSetsType =
                        field.GetType()
                            .Assembly
                            .GetType(
                                "ContentPatcher.Framework.InvariantSets");

                    MethodInfo? fromValueMethod =
                        invariantSetsType?
                            .GetMethods(
                                BindingFlags.Static
                                | BindingFlags.Public
                                | BindingFlags.NonPublic)
                            .FirstOrDefault(method =>
                            {
                                if (method.Name != "FromValue")
                                    return false;

                                ParameterInfo[] parameters =
                                    method.GetParameters();

                                return parameters.Length == 1
                                    && parameters[0].ParameterType == typeof(string)
                                    && valueType.IsAssignableFrom(method.ReturnType);
                            });

                    if (fromValueMethod == null)
                    {
                        throw new InvalidOperationException(
                            $"Could not create Content Patcher config value for '{fieldName}'.");
                    }

                    setValue =
                        fromValueMethod.Invoke(
                            null,
                            new object[]
                            {
                                value
                            })
                        ?? throw new InvalidOperationException(
                            $"Content Patcher returned no config value for '{fieldName}'.");
                }

                setValueMethod.Invoke(
                    field,
                    new[]
                    {
                        setValue
                    });

                return;
            }

            throw new InvalidOperationException(
                $"Could not find Content Patcher config field '{fieldName}'.");
        }

        //----------------------------------------
        // Data Asset定義生成
        //----------------------------------------

        private static PositionPickerDefinition? CreateDefinition(
            IContentPack contentPack,
            object currentConfig,
            string id,
            PositionPickerModel model,
            IMonitor monitor,
            Action saveAndApply)
        {
            if (string.IsNullOrWhiteSpace(model.XField)
                || string.IsNullOrWhiteSpace(model.YField)
                || string.IsNullOrWhiteSpace(model.Location)
                || string.IsNullOrWhiteSpace(model.PreviewMap))
            {
                monitor.Log(
                    $"Ignoring invalid Position Picker '{id}' for '{contentPack.Manifest.UniqueID}'. " +
                    "XField, YField, Location, and PreviewMap are required.",
                    LogLevel.Warn);

                return null;
            }

            string previewMap =
                model.PreviewMap.Replace(
                    "{{ModId}}",
                    contentPack.Manifest.UniqueID,
                    StringComparison.OrdinalIgnoreCase);

            return new PositionPickerDefinition
            {
                ContentPackId = contentPack.Manifest.UniqueID,
                CurrentConfig = currentConfig,
                GmcmName = model.GMCM_Name,
                GmcmDescription = model.GMCM_Description,
                GmcmButton = model.GMCM_Button,
                GmcmWorldRequired = model.GMCM_WorldRequired,
                GmcmLocationRequired = model.GMCM_LocationRequired,
                XField = model.XField,
                YField = model.YField,
                AfterField = model.AfterField,
                Location = model.Location,
                PreviewMap = previewMap,
                PreviewNote = model.PreviewNote,
                AnchorX = model.AnchorX,
                AnchorY = model.AnchorY,
                SaveAndApply = saveAndApply
            };
        }

        //----------------------------------------
        // Location判定
        //----------------------------------------

        private static bool IsTargetLocation(
            PositionPickerDefinition definition)
        {
            if (!Context.IsWorldReady
                || Game1.currentLocation == null)
            {
                return false;
            }

            return string.Equals(
                    Game1.currentLocation.Name,
                    definition.Location,
                    StringComparison.OrdinalIgnoreCase)
                || string.Equals(
                    Game1.currentLocation.NameOrUniqueName,
                    definition.Location,
                    StringComparison.OrdinalIgnoreCase);
        }

        //----------------------------------------
        // 最小モデル
        //----------------------------------------

        private sealed class PositionPickerDefinition
        {
            public string ContentPackId { get; init; } = "";
            public object CurrentConfig { get; init; } = null!;
            public string GmcmName { get; init; } = "";
            public string GmcmDescription { get; init; } = "";
            public string GmcmButton { get; init; } = "";
            public string GmcmWorldRequired { get; init; } = "";
            public string GmcmLocationRequired { get; init; } = "";
            public string XField { get; init; } = "";
            public string YField { get; init; } = "";
            public string AfterField { get; init; } = "";
            public string Location { get; init; } = "";
            public string PreviewMap { get; init; } = "";
            public string PreviewNote { get; init; } = "";
            public int AnchorX { get; init; }
            public int AnchorY { get; init; }
            public Action SaveAndApply { get; set; } = null!;
        }
    }
}
