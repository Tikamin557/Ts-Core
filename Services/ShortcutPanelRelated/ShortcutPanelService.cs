using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using Ts_Core.Models;
using Ts_Core.Services.DebugSupport;
using Ts_Core.Services.ScreenshotRelated;

namespace Ts_Core.Services.ShortcutPanelRelated
{
    /// <summary>
    /// ショートカットパネルの表示と入力を管理します。
    /// </summary>
    internal static class ShortcutPanelService
    {
        private static IModHelper helper = null!;

        private static IMonitor monitor = null!;

        private const string ShortcutPanelConfigFile =
            "ShortcutPanelConfig.json";

        private static Texture2D missingShortcutIcon =
            null!;

        private static Texture2D gmcmIcon =
            null!;

        //----------------------------------------
        // 表示状態
        //----------------------------------------

        private static bool isOpen;

        /// <summary>
        /// スクリーンショット撮影中だけ一時的に非表示にするか。
        /// </summary>
        private static bool screenshotHidden;

        // Androidのタッチ操作を管理します。スロットのタップは指を離した時に確定し、
        // ショートカットを先に実行せず、通常タップと長押しを判別できるようにします。
        private static int androidPressedSlotIndex = -1;
        private static DateTime androidSlotPressedAt;
        private const int AndroidLongPressMilliseconds = 650;

        // Androidで家具を配置中の描画を補正します。
        // Androidでは家具を持っている間、HUDのSpriteBatchからDateTimeScaleが外れます。
        // 描画時だけ通常のHUDと同じDateTimeScale変換を一時的に再現し、
        // どのHUD倍率でもパネルが同じ位置・大きさで描画されるようにします。
        private static bool useFurnitureDateTimeDrawTransform;


        /// <summary>
        /// スクリーンショット撮影中だけショートカットパネルを一時的に非表示にするか設定します。
        /// </summary>
        internal static void SetScreenshotHidden(bool hidden)
        {
            screenshotHidden = hidden;
        }

        //----------------------------------------
        // 表示範囲
        //----------------------------------------

        private static Rectangle tabBounds;

        private static Rectangle panelBounds;

        private static readonly Rectangle[] slotBounds =
            new Rectangle[SlotCount];

        /// <summary>
        /// Shortcut Panelの各スロット。
        /// </summary>
        private static readonly ShortcutPanelSlot[] slots =
            new ShortcutPanelSlot[SlotCount];

        //----------------------------------------
        // レイアウト
        //----------------------------------------

        /// <summary>
        /// スロットの列数。
        /// </summary>
        private const int SlotColumns = 5;

        /// <summary>
        /// スロットの行数。
        /// </summary>
        private const int SlotRows = 2;

        /// <summary>
        /// スロット総数。
        /// </summary>
        private const int SlotCount =
            SlotColumns * SlotRows;

        /// <summary>
        /// 1スロットのサイズ。
        /// </summary>
        private const int SlotSize = 63;

        /// <summary>
        /// スロット間の余白。
        /// </summary>
        private const int SlotSpacing = 8;

        /// <summary>
        /// パネル内側の余白。
        /// </summary>
        private const int PanelPadding = 16;

        /// <summary>
        /// 開閉タブの幅。
        /// </summary>
        private const int TabWidth = 44;

        /// <summary>
        /// 開閉タブの高さ。
        /// 所持金表示の背景と同じ高さ。
        /// </summary>
        private const int TabHeight = 60;

        /// <summary>
        /// パネルの幅。
        /// </summary>
        private const int PanelWidth =
            PanelPadding * 2
            + SlotSize * SlotColumns
            + SlotSpacing * (SlotColumns - 1);

        /// <summary>
        /// パネルの高さ。
        /// </summary>
        private const int PanelHeight =
            PanelPadding * 2
            + SlotSize * SlotRows
            + SlotSpacing * (SlotRows - 1);

        //----------------------------------------
        // 初期化
        //----------------------------------------

        /// <summary>
        /// Shortcut Panelを初期化します。
        /// </summary>
        internal static void Initialize(
            IModHelper modHelper,
            IMonitor modMonitor)
        {
            helper =
                modHelper;

            monitor =
                modMonitor;

            //----------------------------------------
            // アセット
            //----------------------------------------

            missingShortcutIcon =
                helper.ModContent.Load<Texture2D>(
                    "assets/ShortcutPanel/MissingShortcut.png");

            gmcmIcon =
                helper.ModContent.Load<Texture2D>(
                    "assets/ShortcutPanel/GmcmSelectMod.png");

            //----------------------------------------
            // スロット初期化
            //----------------------------------------

            for (int i = 0;
                 i < slots.Length;
                 i++)
            {
                slots[i] =
                    new ShortcutPanelSlot();
            }

            //----------------------------------------
            // 保存済みスロットを復元
            //----------------------------------------

            LoadSlots();

            //----------------------------------------
            // TsCore内蔵ショートカット
            //----------------------------------------

            RegisterBuiltInShortcuts();

            //----------------------------------------
            // イベント
            //----------------------------------------

            helper.Events.Display.RenderedHud
                += OnRenderedHud;

            helper.Events.Input.ButtonPressed
                += OnButtonPressed;

            helper.Events.Input.ButtonReleased
                += OnButtonReleased;

        }

        /// <summary>
        /// TsCore内蔵のショートカットを現在のi18nで登録し直します。
        /// 同じIDはRegistry側で上書きされるため、保存済みスロットの割り当ては維持されます。
        /// </summary>
        internal static void RegisterBuiltInShortcuts()
        {
            ShortcutPanelRegistry.Register(
                new ShortcutPanelEntry(
                    "TsCore/TimeSkip",
                    helper.Translation.Get(
                        "shortcutPanel.TimeSkip"),
                    () => helper.ModContent.Load<Texture2D>(
                        "assets/ShortcutPanel/TimeSkip.png"),
                    null,
                    () =>
                    {
                        TimeSkipService.TryStartTimeSkip();
                    }));

            ShortcutPanelRegistry.Register(
                new ShortcutPanelEntry(
                    "TsCore/TimeSkipDuration",
                    helper.Translation.Get(
                        "shortcutPanel.TimeSkipDuration"),
                    () => helper.ModContent.Load<Texture2D>(
                        "assets/ShortcutPanel/TimeSkipDuration.png"),
                    null,
                    () =>
                    {
                        TimeSkipService.TryStartDurationTimeSkip();
                    }));

            ShortcutPanelRegistry.Register(
                new ShortcutPanelEntry(
                    "TsCore/OpenGmcmMenu",
                    helper.Translation.Get(
                        "shortcutPanel.OpenGmcmMenu"),
                    () => helper.ModContent.Load<Texture2D>(
                        "assets/ShortcutPanel/GmcmTopMenu.png"),
                    null,
                    () =>
                    {
                        if (!ShortcutPanelGmcmService.IsInstalled(helper))
                        {
                            Game1.drawObjectDialogue(
                                helper.Translation.Get(
                                    "shortcutPanel.SelectGmcmMod.notInstalled")
                                    .ToString());
                            return;
                        }

                        ShortcutPanelGmcmService.TryOpenListMenu(
                            helper,
                            monitor);
                    },
                    isAvailable: () =>
                        ShortcutPanelGmcmService.IsInstalled(
                            helper),
                    unavailableAction: () =>
                    {
                        Game1.drawObjectDialogue(
                            helper.Translation.Get(
                                "shortcutPanel.SelectGmcmMod.notInstalled")
                                .ToString());
                    }));

            // SMAPIコンソールはAndroidでのみ使用できます。
            // PCでも機能の存在を案内するため一覧には表示しますが、
            // 薄く表示して選択できない状態にします。
            ShortcutPanelRegistry.Register(
                new ShortcutPanelEntry(
                    "TsCore/OpenSmapiConsole",
                    helper.Translation.Get(
                        "shortcutPanel.OpenSmapiConsole"),
                    () => helper.ModContent.Load<Texture2D>(
                        "assets/ShortcutPanel/SmapiConsole.png"),
                    iconSourceRect: null,
                    action: () =>
                    {
                        Game1.activeClickableMenu =
                            new ShortcutPanelSmapiConsoleMenu(
                                helper.Translation);
                    },
                    isAvailable: () =>
                        IsAndroidEnvironment(),
                    unavailableAction: null,
                    unavailableHoverTextProvider: () =>
                        helper.Translation.Get(
                            "shortcutPanel.SmapiConsole.androidOnly"),
                    playUnavailableSound: false));

            ShortcutPanelRegistry.Register(
                new ShortcutPanelEntry(
                    "TsCore/ScreenshotFullMap",
                    helper.Translation.Get(
                        "shortcutPanel.ScreenshotFullMap"),
                    () => helper.ModContent.Load<Texture2D>(
                        "assets/ShortcutPanel/ScreenshotFullMap.png"),
                    null,
                    () =>
                    {
                        ScreenshotService.TakeFullMapScreenshot();
                    }));

            ShortcutPanelRegistry.Register(
                new ShortcutPanelEntry(
                    "TsCore/ScreenshotCurrentScreen",
                    helper.Translation.Get(
                        "shortcutPanel.ScreenshotCurrentScreen"),
                    () => helper.ModContent.Load<Texture2D>(
                        "assets/ShortcutPanel/ScreenshotCurrentScreen.png"),
                    null,
                    () =>
                    {
                        ScreenshotService.RequestCurrentScreenScreenshot();
                    }));
        }

        //----------------------------------------
        // スロット設定
        //----------------------------------------

        /// <summary>
        /// ShortcutPanelConfig.jsonから
        /// Shortcut Panelのスロットを復元します。
        /// </summary>
        private static void LoadSlots()
        {
            ShortcutPanelConfig shortcutConfig =
                helper.Data.ReadJsonFile<ShortcutPanelConfig>(
                    ShortcutPanelConfigFile)
                ?? new ShortcutPanelConfig();

            List<ShortcutPanelSlotConfig> configs =
                shortcutConfig.Slots;

            int count =
                Math.Min(
                    slots.Length,
                    configs.Count);

            for (int i = 0;
                 i < count;
                 i++)
            {
                ShortcutPanelSlotConfig config =
                    configs[i];

                //----------------------------------------
                // Mod機能
                //----------------------------------------

                if (string.Equals(
                    config.Type,
                    "ModAction",
                    StringComparison.OrdinalIgnoreCase))
                {
                    if (!string.IsNullOrWhiteSpace(
                        config.ShortcutId))
                    {
                        slots[i].SetModAction(
                            config.ShortcutId);
                    }

                    continue;
                }

                //----------------------------------------
                // GMCM
                //----------------------------------------

                if (string.Equals(
                    config.Type,
                    "Gmcm",
                    StringComparison.OrdinalIgnoreCase))
                {
                    if (!string.IsNullOrWhiteSpace(
                        config.GmcmModId))
                    {
                        slots[i].SetGmcm(
                            config.GmcmModId);
                    }

                    continue;
                }

                //----------------------------------------
                // キー設定
                //----------------------------------------

                if (string.Equals(
                    config.Type,
                    "Keybind",
                    StringComparison.OrdinalIgnoreCase))
                {
                    if (config.Keybind != null
                        && config.Keybind.IsBound)
                    {
                        slots[i].SetKeybind(
                            config.Keybind);
                    }
                }
            }
        }

        /// <summary>
        /// Shortcut Panelの現在のスロット状態を
        /// ShortcutPanelConfig.jsonへ保存します。
        /// </summary>
        private static void SaveSlots()
        {
            List<ShortcutPanelSlotConfig> configs =
                new();

            foreach (ShortcutPanelSlot slot
                     in slots)
            {
                ShortcutPanelSlotConfig config =
                    new();

                //----------------------------------------
                // Mod機能
                //----------------------------------------

                if (slot.Type
                    == ShortcutPanelSlotType.ModAction)
                {
                    config.Type =
                        "ModAction";

                    config.ShortcutId =
                        slot.ShortcutId;
                }

                //----------------------------------------
                // GMCM
                //----------------------------------------

                else if (slot.Type
                    == ShortcutPanelSlotType.Gmcm)
                {
                    config.Type =
                        "Gmcm";

                    config.GmcmModId =
                        slot.GmcmModId;
                }

                //----------------------------------------
                // キー設定
                //----------------------------------------

                else if (slot.Type
                    == ShortcutPanelSlotType.Keybind)
                {
                    config.Type =
                        "Keybind";

                    config.Keybind =
                        slot.Keybind;
                }

                //----------------------------------------
                // 未登録
                //----------------------------------------

                else
                {
                    config.Type =
                        "None";
                }

                configs.Add(
                    config);
            }

            ShortcutPanelConfig shortcutConfig =
                new()
                {
                    Slots = configs
                };

            helper.Data.WriteJsonFile(
                ShortcutPanelConfigFile,
                shortcutConfig);
        }

        //----------------------------------------
        // 表示範囲更新
        //----------------------------------------

        /// <summary>
        /// 所持金UIを基準に
        /// パネルの表示位置を更新します。
        /// </summary>
        private static void UpdateBounds()
        {
            if (Game1.dayTimeMoneyBox == null)
                return;

            Vector2 position =
                Game1.dayTimeMoneyBox.position;

            float panelScale =
                GetPanelScale();

            float tabScale =
                GetTabScale();

            int tabWidth =
                ScaleLayoutValue(
                    TabWidth,
                    tabScale);

            int tabHeight =
                ScaleLayoutValue(
                    TabHeight,
                    tabScale);

            int slotSize =
                ScaleLayoutValue(
                    SlotSize,
                    panelScale);

            int slotSpacing =
                ScaleLayoutValue(
                    SlotSpacing,
                    panelScale);

            int panelPadding =
                ScaleLayoutValue(
                    PanelPadding,
                    panelScale);

            int panelWidth =
                panelPadding * 2
                + SlotColumns * slotSize
                + (SlotColumns - 1) * slotSpacing;

            int panelHeight =
                panelPadding * 2
                + SlotRows * slotSize
                + (SlotRows - 1) * slotSpacing;

            float inverse = 1f;
            int tabRight;
            int centerY;

            if (IsAndroidEnvironment())
            {
                float dateTimeScale =
                    GetDateTimeScale();

                int rawMenuEdgePadding =
                    GetMenuEdgePadding();

                int physicalDisplayWidth =
                    GetPhysicalDisplayWidth();

                float convertedMenuEdgePadding =
                    rawMenuEdgePadding > 0
                    && physicalDisplayWidth > 0
                        ? rawMenuEdgePadding
                            * (Game1.uiViewport.Width / (float)physicalDisplayWidth)
                        : rawMenuEdgePadding;

                // Androidのノッチ調整値をUI座標へ変換し、
                // DateTimeScaleを反映した最終画面座標で所持金UIに追従させます。
                float scaledMenuEdgePadding =
                    convertedMenuEdgePadding * dateTimeScale;

                int desiredBaseTabRight =
                    (int)MathF.Round(
                        Game1.uiViewport.Width
                        - scaledMenuEdgePadding
                        - 268f * dateTimeScale
                        - 4f);

                int desiredCenterY =
                    12
                    + 180
                    + TabHeight / 2;

                bool furniturePlacementMode =
                    IsAndroidFurniturePlacementMode()
                    && !useFurnitureDateTimeDrawTransform;

                inverse =
                    !furniturePlacementMode
                    && dateTimeScale > 0f
                        ? 1f / dateTimeScale
                        : 1f;

                tabRight =
                    (int)MathF.Round(
                        desiredBaseTabRight * inverse);

                centerY =
                    furniturePlacementMode
                        ? (int)MathF.Round(
                            desiredCenterY * dateTimeScale)
                        : desiredCenterY;
            }
            else
            {
                // PC版はAndroid対応前に確認済みだった
                // DayTimeMoneyBox.position基準の位置計算をそのまま使用します。
                // 100%時: tabX = position.X + 28 - TabWidth - 4
                //         tabY = position.Y + 180
                tabRight =
                    (int)position.X
                    + 28
                    - 4;

                centerY =
                    (int)position.Y
                    + 180
                    + TabHeight / 2;
            }

            int logicalTabWidth =
                Math.Max(1,
                    (int)MathF.Round(
                        tabWidth * inverse));

            int logicalTabHeight =
                Math.Max(1,
                    (int)MathF.Round(
                        tabHeight * inverse));

            tabBounds =
                new Rectangle(
                    tabRight - logicalTabWidth,
                    centerY - logicalTabHeight / 2,
                    logicalTabWidth,
                    logicalTabHeight);

            int logicalPanelWidth =
                Math.Max(1,
                    (int)MathF.Round(
                        panelWidth * inverse));

            int logicalPanelHeight =
                Math.Max(1,
                    (int)MathF.Round(
                        panelHeight * inverse));

            int logicalGap =
                Math.Max(1,
                    (int)MathF.Round(
                        4f * inverse));

            int panelRight =
                tabBounds.Left
                - logicalGap;

            panelBounds =
                new Rectangle(
                    panelRight - logicalPanelWidth,
                    centerY - logicalPanelHeight / 2,
                    logicalPanelWidth,
                    logicalPanelHeight);

            int logicalPadding =
                Math.Max(1,
                    (int)MathF.Round(
                        panelPadding * inverse));

            int logicalSlotSize =
                Math.Max(1,
                    (int)MathF.Round(
                        slotSize * inverse));

            int logicalSlotSpacing =
                Math.Max(1,
                    (int)MathF.Round(
                        slotSpacing * inverse));

            for (int i = 0; i < SlotCount; i++)
            {
                int column =
                    i % SlotColumns;

                int row =
                    i / SlotColumns;

                int x =
                    panelBounds.X
                    + logicalPadding
                    + column
                    * (logicalSlotSize + logicalSlotSpacing);

                int y =
                    panelBounds.Y
                    + logicalPadding
                    + row
                    * (logicalSlotSize + logicalSlotSpacing);

                slotBounds[i] =
                    new Rectangle(
                        x,
                        y,
                        logicalSlotSize,
                        logicalSlotSize);
            }
        }

        /// <summary>
        /// 現在のショートカットパネル位置計算に関する診断情報を返します。
        /// 実際の描画と同じUpdateBoundsを実行した後の値を使用します。
        /// </summary>
        internal static string BuildPositionDiagnosticReport()
        {
            if (!Context.IsWorldReady)
            {
                return "Shortcut Panel position diagnostic is unavailable before a save is loaded.";
            }

            UpdateBounds();

            float dateTimeScale = GetDateTimeScale();
            float panelScale = GetPanelScale();
            float tabScale = GetTabScale();
            int xEdge = GetMenuEdgePadding();
            int physicalWidth = GetPhysicalDisplayWidth();
            int physicalHeight = 0;

            try
            {
                physicalHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            }
            catch
            {
                // 診断情報なので取得失敗時は0のまま出力します。
            }

            bool isAndroid =
                IsAndroidEnvironment();

            float convertedXEdge =
                xEdge > 0 && physicalWidth > 0
                    ? xEdge * (Game1.uiViewport.Width / (float)physicalWidth)
                    : xEdge;

            float scaledXEdge = convertedXEdge * dateTimeScale;

            int baseTabRight =
                isAndroid
                    ? (int)MathF.Round(
                        Game1.uiViewport.Width
                        - scaledXEdge
                        - 268f * dateTimeScale
                        - 4f)
                    : (int)(Game1.dayTimeMoneyBox?.position.X ?? 0f)
                        + 28
                        - 4;

            bool furniturePlacementMode =
                isAndroid
                && IsAndroidFurniturePlacementMode()
                && !useFurnitureDateTimeDrawTransform;

            float inverse =
                isAndroid
                && !furniturePlacementMode
                && dateTimeScale > 0f
                    ? 1f / dateTimeScale
                    : 1f;

            Vector2 moneyPosition =
                Game1.dayTimeMoneyBox?.position
                ?? Vector2.Zero;

            Rectangle graphicsViewport =
                Game1.graphics.GraphicsDevice.Viewport.Bounds;

            return
                "========== T's Core Shortcut Panel Position Diagnostic ==========\n"
                + $"Platform: {(isAndroid ? "Android" : "PC/Other")}\n"
                + $"WorldReady: {Context.IsWorldReady}\n"
                + $"PanelOpen: {isOpen}\n"
                + $"FurniturePlacementMode: {furniturePlacementMode}\n"
                + "\n[Shortcut Panel Config]\n"
                + $"Enabled: {ModEntry.Config.ShortcutPanelEnabled}\n"
                + $"PanelScaleSetting: {ModEntry.Config.ShortcutPanelScale}%\n"
                + $"TabScaleSetting: {ModEntry.Config.ShortcutPanelTabScale}%\n"
                + $"PanelOpacity: {ModEntry.Config.ShortcutPanelOpacity}%\n"
                + $"TabOpacity: {ModEntry.Config.ShortcutPanelTabOpacity}%\n"
                + $"EffectivePanelScale: {panelScale:0.###}\n"
                + $"EffectiveTabScale: {tabScale:0.###}\n"
                + "\n[Display / Viewport]\n"
                + $"PhysicalDisplay: {physicalWidth}x{physicalHeight}\n"
                + $"GraphicsViewport: X={graphicsViewport.X}, Y={graphicsViewport.Y}, Width={graphicsViewport.Width}, Height={graphicsViewport.Height}\n"
                + $"GameViewport: X={Game1.viewport.X}, Y={Game1.viewport.Y}, Width={Game1.viewport.Width}, Height={Game1.viewport.Height}\n"
                + $"UiViewport: X={Game1.uiViewport.X}, Y={Game1.uiViewport.Y}, Width={Game1.uiViewport.Width}, Height={Game1.uiViewport.Height}\n"
                + "\n[Date / Money UI]\n"
                + $"DateTimeScale: {dateTimeScale:0.###}\n"
                + $"MoneyBoxPosition: X={moneyPosition.X:0.###}, Y={moneyPosition.Y:0.###}\n"
                + (isAndroid
                    ? "\n[Android Menu Edge / Position Calculation]\n"
                        + $"xEdgeRaw: {xEdge}\n"
                        + $"ConvertedXEdge: {convertedXEdge:0.###}\n"
                        + $"ScaledXEdge: {scaledXEdge:0.###}\n"
                        + $"InverseDateTimeScale: {inverse:0.###}\n"
                        + $"BaseTabRightScreen: {baseTabRight}\n"
                        + $"LogicalTabRight: {tabBounds.Right}\n"
                        + $"TabRightAfterDateTimeScale: {(tabBounds.Right * dateTimeScale):0.###}\n"
                    : "\n[PC Position Calculation]\n"
                        + $"BaseTabRight: {baseTabRight}\n"
                        + $"ExpectedBaseTabY: {(int)moneyPosition.Y + 180}\n"
                        + $"LogicalTabRight: {tabBounds.Right}\n")
                + "\n[Final Bounds]\n"
                + $"TabBounds: X={tabBounds.X}, Y={tabBounds.Y}, Width={tabBounds.Width}, Height={tabBounds.Height}, Right={tabBounds.Right}, Bottom={tabBounds.Bottom}\n"
                + $"PanelBounds: X={panelBounds.X}, Y={panelBounds.Y}, Width={panelBounds.Width}, Height={panelBounds.Height}, Right={panelBounds.Right}, Bottom={panelBounds.Bottom}\n"
                + $"Slot0Bounds: X={slotBounds[0].X}, Y={slotBounds[0].Y}, Width={slotBounds[0].Width}, Height={slotBounds[0].Height}\n"
                + $"Slot9Bounds: X={slotBounds[9].X}, Y={slotBounds[9].Y}, Width={slotBounds[9].Width}, Height={slotBounds[9].Height}\n"
                + "===============================================================\n";
        }

        /// <summary>
        /// Android版の「メニュー調整（ノッチスマホ用）」で使用される
        /// 左右の安全領域（Game1.xEdge）を取得します。
        /// PC版にはこのフィールドが存在しないため、取得できない場合は0を返します。
        /// </summary>
        private static int GetMenuEdgePadding()
        {
            try
            {
                Type game1Type =
                    typeof(Game1);

                var field =
                    game1Type.GetField(
                        "xEdge",
                        System.Reflection.BindingFlags.Public
                        | System.Reflection.BindingFlags.NonPublic
                        | System.Reflection.BindingFlags.Static);

                if (field?.GetValue(null)
                    is int value
                    && value > 0)
                {
                    return value;
                }
            }
            catch
            {
                // PC版や取得できない環境では補正しません。
            }

            return 0;
        }

        /// <summary>
        /// 端末の物理解像度の横幅を取得します。
        /// 取得できない場合はUI Viewport幅を返し、従来と同じ補正量にフォールバックします。
        /// </summary>
        private static int GetPhysicalDisplayWidth()
        {
            try
            {
                int width =
                    GraphicsAdapter.DefaultAdapter
                        .CurrentDisplayMode.Width;

                if (width > 0)
                    return width;
            }
            catch
            {
                // 取得できない環境ではUI座標系をそのまま使用します。
            }

            return Game1.uiViewport.Width;
        }

        /// <summary>
        /// Android版の日時UI描画倍率を取得します。
        /// PC版にはDateTimeScaleが存在しないため、
        /// Reflectionで取得して存在しない場合は1.0fを返します。
        /// </summary>
        private static float GetDateTimeScale()
        {
            try
            {
                Type game1Type =
                    typeof(Game1);

                var property =
                    game1Type.GetProperty(
                        "DateTimeScale",
                        System.Reflection.BindingFlags.Public
                        | System.Reflection.BindingFlags.NonPublic
                        | System.Reflection.BindingFlags.Static);

                if (property?.GetValue(null)
                    is float propertyValue
                    && propertyValue > 0f)
                {
                    return propertyValue;
                }

                var field =
                    game1Type.GetField(
                        "DateTimeScale",
                        System.Reflection.BindingFlags.Public
                        | System.Reflection.BindingFlags.NonPublic
                        | System.Reflection.BindingFlags.Static);

                if (field?.GetValue(null)
                    is float fieldValue
                    && fieldValue > 0f)
                {
                    return fieldValue;
                }
            }
            catch
            {
                // PC版や取得できない環境では
                // 従来の座標計算を使用します。
            }

            return 1f;
        }

        /// <summary>
        /// Android版で家具を持って設置プレビュー状態になっているかを取得します。
        /// Androidではこの状態だけRenderedHudのSpriteBatchにDateTimeScaleが残らないため、
        /// 通常時とは別の座標補正が必要です。PC版ではDateTimeScale自体が存在しないためfalseです。
        /// </summary>
        private static bool IsAndroidEnvironment()
        {
            try
            {
                Type game1Type = typeof(Game1);

                return game1Type.GetProperty(
                           "DateTimeScale",
                           System.Reflection.BindingFlags.Public
                           | System.Reflection.BindingFlags.NonPublic
                           | System.Reflection.BindingFlags.Static) != null
                    || game1Type.GetField(
                           "DateTimeScale",
                           System.Reflection.BindingFlags.Public
                           | System.Reflection.BindingFlags.NonPublic
                           | System.Reflection.BindingFlags.Static) != null;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Android版で家具を持って設置プレビュー状態になっているかを取得します。
        /// </summary>
        private static bool IsAndroidFurniturePlacementMode()
        {
            try
            {
                Type game1Type =
                    typeof(Game1);

                bool hasDateTimeScale =
                    game1Type.GetProperty(
                        "DateTimeScale",
                        System.Reflection.BindingFlags.Public
                        | System.Reflection.BindingFlags.NonPublic
                        | System.Reflection.BindingFlags.Static) != null
                    || game1Type.GetField(
                        "DateTimeScale",
                        System.Reflection.BindingFlags.Public
                        | System.Reflection.BindingFlags.NonPublic
                        | System.Reflection.BindingFlags.Static) != null;

                if (!hasDateTimeScale)
                    return false;

                object? item =
                    Game1.player?.CurrentItem;

                if (item == null)
                    return false;

                Type? type =
                    item.GetType();

                while (type != null)
                {
                    if (type.FullName
                        == "StardewValley.Objects.Furniture")
                    {
                        return true;
                    }

                    type =
                        type.BaseType;
                }
            }
            catch
            {
            }

            return false;
        }

        /// <summary>
        /// Shortcut Panel の描画サイズ補正に使う日時UI倍率を取得します。
        /// Androidの家具設置プレビュー中は RenderedHud の SpriteBatch に
        /// DateTimeScale が残らないため、この状態では 1.0f を返します。
        /// </summary>
        private static float GetShortcutPanelDrawDateTimeScale()
        {
            if (IsAndroidFurniturePlacementMode()
                && !useFurnitureDateTimeDrawTransform)
            {
                return 1f;
            }

            return GetDateTimeScale();
        }

        /// <summary>
        /// レイアウト用の基準値へ倍率を適用します。
        /// </summary>
        private static int ScaleLayoutValue(
            int value,
            float scale)
        {
            return
                Math.Max(
                    1,
                    (int)MathF.Round(
                        value * scale));
        }

        //----------------------------------------
        // 入力
        //----------------------------------------

        /// <summary>
        /// パネルのクリック・タップを処理します。
        /// </summary>
        private static void OnButtonPressed(
            object? sender,
            ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            if (screenshotHidden)
                return;

            //----------------------------------------
            // Shortcut Panelが無効
            //----------------------------------------

            if (!ModEntry.Config.ShortcutPanelEnabled)
                return;

            //----------------------------------------
            // イベント中は操作しない
            //----------------------------------------

            if (Game1.eventUp)
                return;

            //----------------------------------------
            // キー設定入力待機中
            //----------------------------------------

            if (Game1.activeClickableMenu
                is ShortcutPanelKeybindMenu keybindMenu)
            {
                //----------------------------------------
                // 左クリックはメニュー自身へ任せる
                //----------------------------------------

                if (e.Button == SButton.MouseLeft)
                    return;

                //----------------------------------------
                // 入力されたキーを渡す
                //
                // 注意:
                // ここではSuppressしない。
                // SuppressするとSMAPIによって
                // 次のtickでButtonReleasedが発生し、
                // 複合キーを入力する前に
                // 登録が確定してしまう。
                //----------------------------------------

                keybindMenu.ReceiveButton(
                    e.Button);

                return;
            }

            if (Game1.activeClickableMenu != null)
                return;

            if (e.Button != SButton.MouseLeft
                && e.Button != SButton.MouseRight)
            {
                return;
            }

            UpdateBounds();

            //----------------------------------------
            // HUD描画座標へ変換
            //----------------------------------------

            Vector2 cursorPosition =
                Utility.ModifyCoordinatesForUIScale(
                    e.Cursor.ScreenPixels);

            // AndroidではRenderedHudのSpriteBatchにDateTimeScaleが残っており、
            // UpdateBounds()側のRectangleはその逆倍率で保持しています。
            // そのため入力座標も同じ論理座標系へ戻してから判定します。
            float inputDateTimeScale =
                GetDateTimeScale();

            if (!IsAndroidFurniturePlacementMode()
                && inputDateTimeScale > 0f)
            {
                cursorPosition /=
                    inputDateTimeScale;
            }

            Point cursor =
                cursorPosition.ToPoint();

            //----------------------------------------
            // 開閉タブ
            //----------------------------------------

            if (tabBounds.Contains(
                cursor))
            {
                //----------------------------------------
                // タブ上の入力を抑制
                //----------------------------------------

                helper.Input.Suppress(
                    e.Button);

                //----------------------------------------
                // 左クリックのみ開閉
                //----------------------------------------

                if (e.Button
                    != SButton.MouseLeft)
                {
                    return;
                }

                isOpen =
                    !isOpen;

                Game1.playSound(
                    "shwip");

                return;
            }

            //----------------------------------------
            // パネルが閉じている場合
            //----------------------------------------

            if (!isOpen)
                return;

            //----------------------------------------
            // スロット
            //----------------------------------------

            for (int i = 0;
                 i < slotBounds.Length;
                 i++)
            {
                if (!slotBounds[i].Contains(
                    cursor))
                {
                    continue;
                }

                ShortcutPanelSlot slot =
                    slots[i];

                //----------------------------------------
                // Android: タップ / 長押し
                //----------------------------------------

                if (IsAndroidEnvironment()
                    && e.Button == SButton.MouseLeft)
                {
                    // AndroidではここでSuppressすると、SMAPI側から
                    // ButtonReleasedが即座に発生する場合があり、
                    // 長押し時間を計測できない。
                    // そのため押下時はSuppressせず、実際に指が離された
                    // ButtonReleasedまで待ってタップ / 長押しを判定する。
                    androidPressedSlotIndex = i;
                    androidSlotPressedAt = DateTime.UtcNow;
                    return;
                }

                //----------------------------------------
                // PC: 入力を抑制
                //----------------------------------------

                helper.Input.Suppress(
                    e.Button);

                //----------------------------------------
                // 右クリック
                // 登録解除
                //----------------------------------------

                if (e.Button
                    == SButton.MouseRight)
                {
                    //----------------------------------------
                    // 空スロットなら何もしない
                    //----------------------------------------

                    if (!slot.IsAssigned())
                        return;

                    //----------------------------------------
                    // Shortcutを解除
                    //----------------------------------------

                    slot.Clear();

                    SaveSlots();

                    Game1.playSound(
                        "bigDeSelect");

                    return;
                }

                //----------------------------------------
                // 左クリック
                //----------------------------------------

                //----------------------------------------
                // 空スロット
                //----------------------------------------

                if (!slot.IsAssigned())
                {
                    int slotIndex =
                        i;

                    Game1.playSound(
                        "smallSelect");

                    bool isGmcmInstalled =
                        helper.ModRegistry.IsLoaded(
                            "spacechase0.GenericModConfigMenu");

                    Game1.activeClickableMenu =
                        new ShortcutPanelTypeSelectionMenu(
                            helper.Translation,

                            //----------------------------------------
                            // キー設定
                            //----------------------------------------

                            onKeybindSelected: () =>
                            {
                                Action<KeybindList> onKeybindSelected =
                                    keybind =>
                                    {
                                        slots[slotIndex]
                                            .SetKeybind(
                                                keybind);

                                        SaveSlots();
                                    };

                                Game1.activeClickableMenu =
                                    IsAndroidEnvironment()
                                        ? new ShortcutPanelAndroidKeybindMenu(
                                            helper.Translation,
                                            onKeybindSelected)
                                        : new ShortcutPanelKeybindMenu(
                                            helper.Translation,
                                            onKeybindSelected);
                            },

                            //----------------------------------------
                            // Mod機能
                            //----------------------------------------

                            onModActionSelected: () =>
                            {
                                Game1.activeClickableMenu =
                                    new ShortcutPanelSelectionMenu(
                                        helper.Translation,
                                        shortcutId =>
                                        {
                                            slots[slotIndex]
                                                .SetModAction(
                                                    shortcutId);

                                            SaveSlots();
                                        });
                            },

                            //----------------------------------------
                            // GMCM
                            //----------------------------------------

                            onGmcmSelected: () =>
                            {
                                IReadOnlyList<IManifest> gmcmMods =
                                    isGmcmInstalled
                                        ? ShortcutPanelGmcmService
                                            .GetEditableMods(
                                                helper,
                                                monitor)
                                        : Array.Empty<IManifest>();

                                Game1.activeClickableMenu =
                                    new ShortcutPanelGmcmSelectionMenu(
                                        helper.Translation,
                                        gmcmMods,
                                        modId =>
                                        {
                                            slots[slotIndex]
                                                .SetGmcm(
                                                    modId);

                                            SaveSlots();
                                        },
                                        isGmcmInstalled);
                            },
                            isGmcmInstalled);

                    return;
                }

                //----------------------------------------
                // Mod機能
                //----------------------------------------

                if (slot.Type
                    == ShortcutPanelSlotType.ModAction)
                {
                    ShortcutPanelEntry? entry =
                        slot.GetEntry();

                    entry?.Execute();

                    return;
                }

                //----------------------------------------
                // GMCM
                //----------------------------------------

                if (slot.Type
                    == ShortcutPanelSlotType.Gmcm)
                {
                    ShortcutPanelGmcmService
                        .TryOpenModMenu(
                            helper,
                            monitor,
                            slot.GmcmModId);

                    return;
                }

                //----------------------------------------
                // キー設定
                //----------------------------------------

                if (slot.Type
                    == ShortcutPanelSlotType.Keybind)
                {
                    slot.ExecuteKeybind(
                        helper.Input);

                    return;
                }
            }

            //----------------------------------------
            // パネル背景上のクリックも
            // ゲーム側へ渡さない
            //----------------------------------------

            if (panelBounds.Contains(
                cursor))
            {
                helper.Input.Suppress(
                    e.Button);
            }
        }

        /// <summary>
        /// Androidでスロットから指を離した時のタップ / 長押し処理です。
        /// </summary>
        private static void CompleteAndroidSlotTouch(int slotIndex, bool longPress)
        {
            if (slotIndex < 0 || slotIndex >= slots.Length)
                return;

            ShortcutPanelSlot slot = slots[slotIndex];

            if (longPress)
            {
                if (!slot.IsAssigned())
                    return;

                slot.Clear();
                SaveSlots();
                Game1.playSound("bigDeSelect");
                return;
            }

            if (!slot.IsAssigned())
            {
                Game1.playSound("smallSelect");

                bool isGmcmInstalled =
                    helper.ModRegistry.IsLoaded("spacechase0.GenericModConfigMenu");

                Game1.activeClickableMenu =
                    new ShortcutPanelTypeSelectionMenu(
                        helper.Translation,
                        onKeybindSelected: () =>
                        {
                            Action<KeybindList> onKeybindSelected =
                                keybind =>
                                {
                                    slots[slotIndex].SetKeybind(keybind);
                                    SaveSlots();
                                };

                            Game1.activeClickableMenu =
                                new ShortcutPanelAndroidKeybindMenu(
                                    helper.Translation,
                                    onKeybindSelected);
                        },
                        onModActionSelected: () =>
                        {
                            Game1.activeClickableMenu =
                                new ShortcutPanelSelectionMenu(
                                    helper.Translation,
                                    shortcutId =>
                                    {
                                        slots[slotIndex].SetModAction(shortcutId);
                                        SaveSlots();
                                    });
                        },
                        onGmcmSelected: () =>
                        {
                            IReadOnlyList<IManifest> gmcmMods =
                                isGmcmInstalled
                                    ? ShortcutPanelGmcmService.GetEditableMods(helper, monitor)
                                    : Array.Empty<IManifest>();

                            Game1.activeClickableMenu =
                                new ShortcutPanelGmcmSelectionMenu(
                                    helper.Translation,
                                    gmcmMods,
                                    modId =>
                                    {
                                        slots[slotIndex].SetGmcm(modId);
                                        SaveSlots();
                                    },
                                    isGmcmInstalled);
                        },
                        isGmcmInstalled);
                return;
            }

            if (slot.Type == ShortcutPanelSlotType.ModAction)
            {
                slot.GetEntry()?.Execute();
                return;
            }

            if (slot.Type == ShortcutPanelSlotType.Gmcm)
            {
                ShortcutPanelGmcmService.TryOpenModMenu(
                    helper, monitor, slot.GmcmModId);
                return;
            }

            if (slot.Type == ShortcutPanelSlotType.Keybind)
                slot.ExecuteKeybind(helper.Input);
        }

        /// <summary>
        /// Shortcut Panelのキー登録中に
        /// キーが離された時の処理です。
        /// </summary>
        private static void OnButtonReleased(
            object? sender,
            ButtonReleasedEventArgs e)
        {
            //----------------------------------------
            // キー設定入力待機中のみ処理
            //----------------------------------------

            if (Game1.activeClickableMenu
                is ShortcutPanelKeybindMenu keybindMenu)
            {
                keybindMenu.ReceiveButtonReleased(
                    e.Button);
                return;
            }

            if (!IsAndroidEnvironment()
                || e.Button != SButton.MouseLeft
                || androidPressedSlotIndex < 0)
            {
                return;
            }

            int slotIndex = androidPressedSlotIndex;
            androidPressedSlotIndex = -1;

            if (Game1.activeClickableMenu != null
                || !Context.IsWorldReady
                || Game1.eventUp
                || !ModEntry.Config.ShortcutPanelEnabled)
            {
                return;
            }

            UpdateBounds();

            Vector2 cursorPosition =
                Utility.ModifyCoordinatesForUIScale(
                    e.Cursor.ScreenPixels);

            float inputDateTimeScale = GetDateTimeScale();
            if (!IsAndroidFurniturePlacementMode()
                && inputDateTimeScale > 0f)
            {
                cursorPosition /= inputDateTimeScale;
            }

            Point cursor = cursorPosition.ToPoint();
            if (!slotBounds[slotIndex].Contains(cursor))
                return;

            bool longPress =
                (DateTime.UtcNow - androidSlotPressedAt).TotalMilliseconds
                >= AndroidLongPressMilliseconds;

            CompleteAndroidSlotTouch(slotIndex, longPress);
        }

        //----------------------------------------
        // 表示倍率
        //----------------------------------------

        /// <summary>
        /// Shortcut Panel本体の表示倍率を取得します。
        /// </summary>
        private static float GetPanelScale()
        {
            return
                Math.Clamp(
                    ModEntry.Config.ShortcutPanelScale,
                    100,
                    200)
                / 100f;
        }

        /// <summary>
        /// Shortcut Panel開閉タブの表示倍率を取得します。
        /// </summary>
        private static float GetTabScale()
        {
            return
                Math.Clamp(
                    ModEntry.Config.ShortcutPanelTabScale,
                    100,
                    200)
                / 100f;
        }

        //----------------------------------------
        // 不透明度
        //----------------------------------------

        /// <summary>
        /// Shortcut Panel本体とスロット背景の
        /// 不透明度を取得します。
        /// </summary>
        private static float GetPanelOpacity()
        {
            return
                Math.Clamp(
                    ModEntry.Config.ShortcutPanelOpacity,
                    0,
                    100)
                / 100f;
        }

        /// <summary>
        /// Shortcut Panel開閉タブ背景の
        /// 不透明度を取得します。
        /// </summary>
        private static float GetTabOpacity()
        {
            return
                Math.Clamp(
                    ModEntry.Config.ShortcutPanelTabOpacity,
                    0,
                    100)
                / 100f;
        }

        //----------------------------------------
        // 描画
        //----------------------------------------

        /// <summary>
        /// Shortcut PanelをHUD上に描画します。
        /// </summary>
        private static void OnRenderedHud(
            object? sender,
            RenderedHudEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            //----------------------------------------
            // スクリーンショット撮影中は一時的に非表示
            //----------------------------------------

            if (screenshotHidden)
                return;

            //----------------------------------------
            // Shortcut Panelが無効
            //----------------------------------------

            if (!ModEntry.Config.ShortcutPanelEnabled)
                return;

            //----------------------------------------
            // イベント中は表示しない
            //----------------------------------------

            if (Game1.eventUp)
                return;

            if (Game1.dayTimeMoneyBox == null)
                return;

            SpriteBatch spriteBatch =
                e.SpriteBatch;

            bool furniturePlacementMode =
                IsAndroidFurniturePlacementMode();

            float dateTimeScale =
                GetDateTimeScale();

            // Androidだけ、家具設置中はHUDのSpriteBatchからDateTimeScaleが
            // 外れます。位置補正だけでは200%時にラスタライズ結果が僅かに
            // 変わるため、Shortcut Panelを描く間だけ通常HUDと同じ変換を
            // 再現します。PC版と通常時には入りません。
            bool recreateDateTimeTransform =
                furniturePlacementMode
                && Math.Abs(dateTimeScale - 1f) > 0.0001f;

            if (recreateDateTimeTransform)
            {
                useFurnitureDateTimeDrawTransform = true;

                try
                {
                    UpdateBounds();

                    spriteBatch.End();
                    spriteBatch.Begin(
                        SpriteSortMode.Deferred,
                        BlendState.AlphaBlend,
                        SamplerState.PointClamp,
                        null,
                        null,
                        null,
                        Matrix.CreateScale(dateTimeScale));

                    DrawShortcutPanelContents(spriteBatch);

                    spriteBatch.End();
                    spriteBatch.Begin(
                        SpriteSortMode.Deferred,
                        BlendState.AlphaBlend,
                        SamplerState.PointClamp,
                        null,
                        null);
                }
                finally
                {
                    useFurnitureDateTimeDrawTransform = false;
                }

                return;
            }

            UpdateBounds();
            DrawShortcutPanelContents(spriteBatch);
        }

        //----------------------------------------
        // ホバーテキスト
        //----------------------------------------

        /// <summary>
        /// Shortcut PanelのHover Textを描画します。
        /// </summary>
        private static void DrawHoverText(
            SpriteBatch spriteBatch)
        {
            //----------------------------------------
            // メニュー表示中はHoverを表示しない
            //----------------------------------------

            if (Game1.activeClickableMenu != null)
                return;

            //----------------------------------------
            // マウス位置
            //----------------------------------------

            Vector2 hoverPosition =
                Game1.getMousePosition().ToVector2();

            // 入力判定と同様に、DateTimeScaleが掛かる前の
            // Shortcut Panel論理座標へ戻します。
            float hoverDateTimeScale =
                GetDateTimeScale();

            if (!IsAndroidFurniturePlacementMode()
                && hoverDateTimeScale > 0f)
            {
                hoverPosition /=
                    hoverDateTimeScale;
            }

            Point cursor =
                hoverPosition.ToPoint();

            //----------------------------------------
            // 開閉タブ
            //----------------------------------------

            if (tabBounds.Contains(
                cursor))
            {
                string text =
                    isOpen
                        ? helper.Translation.Get(
                            "shortcutPanel.Hover.close")
                        : helper.Translation.Get(
                            "shortcutPanel.Hover.open");

                DrawShortcutHoverText(
                    spriteBatch,
                    text,
                    hoverPosition);

                return;
            }

            //----------------------------------------
            // パネルが閉じている場合
            //----------------------------------------

            if (!isOpen)
                return;

            //----------------------------------------
            // スロット
            //----------------------------------------

            for (int i = 0;
                 i < slotBounds.Length;
                 i++)
            {
                if (!slotBounds[i].Contains(
                    cursor))
                {
                    continue;
                }

                ShortcutPanelSlot slot =
                    slots[i];

                //----------------------------------------
                // 未登録
                //----------------------------------------

                if (!slot.IsAssigned())
                {
                    DrawShortcutHoverText(
                        spriteBatch,
                        helper.Translation.Get(
                            IsAndroidEnvironment()
                                ? "shortcutPanel.Hover.register.android"
                                : "shortcutPanel.Hover.register"),
                        hoverPosition);

                    return;
                }

                //----------------------------------------
                // キー設定
                //----------------------------------------

                if (slot.Type
                    == ShortcutPanelSlotType.Keybind)
                {
                    string keybindText =
                        GetKeybindHoverText(
                            slot);

                    string text =
                        keybindText
                        + Environment.NewLine
                        + helper.Translation.Get(
                            "shortcutPanel.Hover.key")
                        + Environment.NewLine
                        + helper.Translation.Get(
                            IsAndroidEnvironment()
                                ? "shortcutPanel.Hover.remove.android"
                                : "shortcutPanel.Hover.remove");

                    DrawShortcutHoverText(
                        spriteBatch,
                        text,
                        hoverPosition);

                    return;
                }

                //----------------------------------------
                // GMCM
                //----------------------------------------

                if (slot.Type
                    == ShortcutPanelSlotType.Gmcm)
                {
                    IManifest? manifest =
                        ShortcutPanelGmcmService
                            .GetManifest(
                                helper,
                                slot.GmcmModId);

                    string text;

                    if (manifest != null)
                    {
                        text =
                            manifest.Name
                            + Environment.NewLine
                            + helper.Translation.Get(
                                "shortcutPanel.Hover.gmcm")
                            + Environment.NewLine
                            + helper.Translation.Get(
                                IsAndroidEnvironment()
                                    ? "shortcutPanel.Hover.remove.android"
                                    : "shortcutPanel.Hover.remove");
                    }
                    else
                    {
                        text =
                            helper.Translation.Get(
                                "shortcutPanel.Hover.gmcm")
                            + Environment.NewLine
                            + helper.Translation.Get(
                                IsAndroidEnvironment()
                                    ? "shortcutPanel.Hover.remove.android"
                                    : "shortcutPanel.Hover.remove");
                    }

                    DrawShortcutHoverText(
                        spriteBatch,
                        text,
                        hoverPosition);

                    return;
                }

                //----------------------------------------
                // Mod機能
                //----------------------------------------

                if (slot.Type
                    == ShortcutPanelSlotType.ModAction)
                {
                    ShortcutPanelEntry? entry =
                        slot.GetEntry();

                    string text;

                    if (entry != null)
                    {
                        text =
                            entry.DisplayName
                            + Environment.NewLine
                            + helper.Translation.Get(
                                "shortcutPanel.Hover.modFunction")
                            + Environment.NewLine
                            + helper.Translation.Get(
                                IsAndroidEnvironment()
                                    ? "shortcutPanel.Hover.remove.android"
                                    : "shortcutPanel.Hover.remove");
                    }
                    else
                    {
                        text =
                            helper.Translation.Get(
                                IsAndroidEnvironment()
                                    ? "shortcutPanel.Hover.remove.android"
                                    : "shortcutPanel.Hover.remove");
                    }

                    DrawShortcutHoverText(
                        spriteBatch,
                        text,
                        hoverPosition);

                    return;
                }
            }
        }

        /// <summary>
        /// Shortcut Panel用のHover Textを、DateTimeScale補正済みの
        /// 座標へ描画します。
        /// </summary>
        private static void DrawShortcutHoverText(
            SpriteBatch spriteBatch,
            string text,
            Vector2 hoverPosition)
        {
            float dateTimeScale =
                GetDateTimeScale();

            if (dateTimeScale <= 0f)
                dateTimeScale = 1f;

            // Android の RenderedHud では SpriteBatch に DateTimeScale が
            // 残っているため、最終画面上でタッチ位置から約32px離れた位置に
            // なるよう、描画前の論理座標へ変換します。
            float inverse =
                1f / dateTimeScale;

            Vector2 textSize =
                Game1.smallFont.MeasureString(
                    text);

            const int padding = 16;
            const int screenMargin = 8;

            int boxWidth =
                (int)MathF.Ceiling(
                    textSize.X)
                + padding * 2;

            int boxHeight =
                (int)MathF.Ceiling(
                    textSize.Y)
                + padding * 2;

            int hoverX =
                (int)MathF.Round(
                    hoverPosition.X
                    + 32f * inverse);

            int hoverY =
                (int)MathF.Round(
                    hoverPosition.Y
                    + 32f * inverse);

            // drawHoverText 自身の画面端補正は DateTimeScale < 1 のとき
            // Android の座標系と一致せず、Hoverが大きく左へ飛ぶことがあります。
            // そのため位置決定と画面内への収め処理をここで行います。
            int logicalViewportWidth =
                (int)MathF.Floor(
                    Game1.uiViewport.Width
                    * inverse);

            int logicalViewportHeight =
                (int)MathF.Floor(
                    Game1.uiViewport.Height
                    * inverse);

            int logicalMargin =
                (int)MathF.Ceiling(
                    screenMargin
                    * inverse);

            int maxX =
                logicalViewportWidth
                - boxWidth
                - logicalMargin;

            int maxY =
                logicalViewportHeight
                - boxHeight
                - logicalMargin;

            hoverX =
                Math.Clamp(
                    hoverX,
                    logicalMargin,
                    Math.Max(
                        logicalMargin,
                        maxX));

            hoverY =
                Math.Clamp(
                    hoverY,
                    logicalMargin,
                    Math.Max(
                        logicalMargin,
                        maxY));

            IClickableMenu.drawTextureBox(
                spriteBatch,
                Game1.menuTexture,
                new Rectangle(
                    0,
                    256,
                    60,
                    60),
                hoverX,
                hoverY,
                boxWidth,
                boxHeight,
                Color.White,
                1f,
                drawShadow: false);

            spriteBatch.DrawString(
                Game1.smallFont,
                text,
                new Vector2(
                    hoverX + padding,
                    hoverY + padding),
                Game1.textColor);
        }

        /// <summary>
        /// キー設定のHover表示用文字列を取得します。
        /// </summary>
        private static string GetKeybindHoverText(
            ShortcutPanelSlot slot)
        {
            Keybind? keybind =
                slot.Keybind?
                    .Keybinds
                    .FirstOrDefault();

            if (keybind == null)
                return string.Empty;

            SButton[] buttons =
                keybind.Buttons
                    .ToArray();

            if (buttons.Length == 0)
                return string.Empty;

            return string.Join(
                " + ",
                buttons.Select(
                    button =>
                        button.ToString()));
        }


        /// <summary>
        /// Shortcut Panel本体・タブ・ホバーを現在のSpriteBatch状態で描画します。
        /// </summary>
        private static void DrawShortcutPanelContents(
            SpriteBatch spriteBatch)
        {
            if (isOpen)
            {
                DrawPanel(spriteBatch);
                DrawSlots(spriteBatch);
            }

            DrawTab(spriteBatch);
            DrawHoverText(spriteBatch);
        }

        //----------------------------------------
        // パネル
        //----------------------------------------

        /// <summary>
        /// パネル背景を描画します。
        /// </summary>
        private static void DrawPanel(
            SpriteBatch spriteBatch)
        {
            IClickableMenu.drawTextureBox(
                spriteBatch,
                Game1.menuTexture,
                new Rectangle(
                    0,
                    256,
                    60,
                    60),
                panelBounds.X,
                panelBounds.Y,
                panelBounds.Width,
                panelBounds.Height,
                Color.White
                    * GetPanelOpacity(),
                GetPanelScale()
                    / GetShortcutPanelDrawDateTimeScale(),
                drawShadow: false);
        }

        //----------------------------------------
        // スロット
        //----------------------------------------

        /// <summary>
        /// ショートカットスロットを描画します。
        /// </summary>
        private static void DrawSlots(
            SpriteBatch spriteBatch)
        {
            for (int i = 0;
                 i < slotBounds.Length;
                 i++)
            {
                Rectangle bounds =
                    slotBounds[i];

                //----------------------------------------
                // スロット背景
                //----------------------------------------

                spriteBatch.Draw(
                    Game1.menuTexture,
                    bounds,
                    new Rectangle(
                        128,
                        128,
                        64,
                        64),
                    Color.White
                        * GetPanelOpacity());

                ShortcutPanelSlot slot =
                    slots[i];

                //----------------------------------------
                // Mod機能
                //----------------------------------------

                if (slot.Type
                    == ShortcutPanelSlotType.ModAction)
                {
                    ShortcutPanelEntry? entry =
                        slot.GetEntry();

                    if (entry == null)
                    {
                        DrawMissingShortcutIcon(
                            spriteBatch,
                            bounds);

                        continue;
                    }

                    Texture2D? iconTexture =
                        entry.GetIconTexture();

                    if (iconTexture == null)
                        continue;

                    int iconSize =
                        Math.Max(1,
                            (int)MathF.Round(
                                ScaleLayoutValue(
                                    48,
                                    GetPanelScale())
                                / GetShortcutPanelDrawDateTimeScale()));

                    Rectangle iconBounds =
                        new Rectangle(
                            bounds.Center.X
                            - iconSize / 2,
                            bounds.Center.Y
                            - iconSize / 2,
                            iconSize,
                            iconSize);

                    spriteBatch.Draw(
                        iconTexture,
                        iconBounds,
                        entry.IconSourceRect,
                        Color.White);

                    continue;
                }

                //----------------------------------------
                // GMCM
                //----------------------------------------

                if (slot.Type
                    == ShortcutPanelSlotType.Gmcm)
                {
                    IManifest? manifest =
                        ShortcutPanelGmcmService
                            .GetManifest(
                                helper,
                                slot.GmcmModId);

                    if (manifest == null)
                    {
                        DrawMissingShortcutIcon(
                            spriteBatch,
                            bounds);

                        continue;
                    }

                    int iconSize =
                        Math.Max(1,
                            (int)MathF.Round(
                                ScaleLayoutValue(
                                    48,
                                    GetPanelScale())
                                / GetShortcutPanelDrawDateTimeScale()));

                    Rectangle iconBounds =
                        new Rectangle(
                            bounds.Center.X
                                - iconSize / 2,
                            bounds.Center.Y
                                - iconSize / 2,
                            iconSize,
                            iconSize);

                    spriteBatch.Draw(
                        gmcmIcon,
                        iconBounds,
                        Color.White);

                    continue;
                }

                //----------------------------------------
                // キー設定
                //----------------------------------------

                if (slot.Type
                    == ShortcutPanelSlotType.Keybind)
                {
                    Keybind? keybind =
                        slot.Keybind?
                            .Keybinds
                            .FirstOrDefault();

                    if (keybind == null)
                        continue;

                    SButton[] buttons =
                        keybind.Buttons
                            .ToArray();

                    if (buttons.Length == 0)
                        continue;

                    //----------------------------------------
                    // 1キー
                    //----------------------------------------

                    if (buttons.Length == 1)
                    {
                        DrawKeybindText(
                            spriteBatch,
                            bounds,
                            buttons[0].ToString());

                        continue;
                    }

                    //----------------------------------------
                    // 複合キー
                    //----------------------------------------

                    DrawMultiKeybindText(
                        spriteBatch,
                        bounds,
                        buttons);

                    continue;
                }
            }
        }

        /// <summary>
        /// 現在利用できないShortcutの
        /// アイコンを描画します。
        /// </summary>
        private static void DrawMissingShortcutIcon(
            SpriteBatch spriteBatch,
            Rectangle bounds)
        {
            int iconSize =
                Math.Max(1,
                    (int)MathF.Round(
                        ScaleLayoutValue(
                            48,
                            GetPanelScale())
                        / GetShortcutPanelDrawDateTimeScale()));

            Rectangle destinationRect =
                new Rectangle(
                    bounds.Center.X
                        - iconSize / 2,
                    bounds.Center.Y
                        - iconSize / 2,
                    iconSize,
                    iconSize);

            spriteBatch.Draw(
                missingShortcutIcon,
                destinationRect,
                Color.White);
        }

        //----------------------------------------
        // 描画 Keybind Text
        //----------------------------------------

        /// <summary>
        /// キー設定スロットに表示するキー名を、スロット内に収まるよう描画します。
        /// </summary>
        private static void DrawKeybindText(
            SpriteBatch spriteBatch,
            Rectangle bounds,
            string text)
        {
            // 単一キーはdialogueFontを使って
            // スロット内でできるだけ大きく表示します。
            Vector2 textSize =
                Game1.dialogueFont.MeasureString(
                    text);

            float panelScale =
                GetPanelScale()
                / GetShortcutPanelDrawDateTimeScale();

            float padding =
                8f * panelScale;

            float maxWidth =
                bounds.Width
                - padding * 2f;

            float maxHeight =
                bounds.Height
                - padding * 2f;

            float scale =
                panelScale;

            if (textSize.X > 0f
                && textSize.Y > 0f)
            {
                scale =
                    Math.Min(
                        panelScale,
                        Math.Min(
                            maxWidth / textSize.X,
                            maxHeight / textSize.Y));
            }

            Vector2 position =
                new Vector2(
                    bounds.Center.X
                    - textSize.X
                    * scale / 2f,
                    bounds.Center.Y
                    - textSize.Y
                    * scale / 2f);

            spriteBatch.DrawString(
                Game1.dialogueFont,
                text,
                position,
                Game1.textColor,
                0f,
                Vector2.Zero,
                scale,
                SpriteEffects.None,
                0f);
        }

        //----------------------------------------
        // 描画 Multi Keybind Text
        //----------------------------------------

        /// <summary>
        /// 複数キーの組み合わせを、スロット内に収まるよう整形して描画します。
        /// </summary>
        private static void DrawMultiKeybindText(
            SpriteBatch spriteBatch,
            Rectangle bounds,
            SButton[] buttons)
        {
            //----------------------------------------
            // 表示行を作成
            //----------------------------------------

            List<string> lines =
                new();

            for (int i = 0;
                 i < buttons.Length;
                 i++)
            {
                if (i > 0)
                {
                    lines.Add(
                        "+");
                }

                lines.Add(
                    buttons[i].ToString());
            }

            //----------------------------------------
            // 使用可能サイズ
            //----------------------------------------

            float panelScale =
                GetPanelScale()
                / GetShortcutPanelDrawDateTimeScale();

            float maxWidth =
                bounds.Width
                - 8f * panelScale;

            float maxHeight =
                bounds.Height
                - 8f * panelScale;

            //----------------------------------------
            // 全体のScaleを計算
            //----------------------------------------

            float scale =
                panelScale;

            float largestWidth =
                0f;

            float totalHeight =
                0f;

            foreach (string line in lines)
            {
                Vector2 size =
                    Game1.smallFont.MeasureString(
                        line);

                if (size.X > largestWidth)
                {
                    largestWidth =
                        size.X;
                }

                totalHeight +=
                    size.Y;
            }

            if (largestWidth > maxWidth
                && largestWidth > 0)
            {
                scale =
                    Math.Min(
                        scale,
                        maxWidth / largestWidth);
            }

            if (totalHeight > maxHeight
                && totalHeight > 0)
            {
                scale =
                    Math.Min(
                        scale,
                        maxHeight / totalHeight);
            }

            //----------------------------------------
            // 縦方向の開始位置
            //----------------------------------------

            float scaledTotalHeight =
                totalHeight * scale;

            float y =
                bounds.Center.Y
                - scaledTotalHeight / 2f;

            //----------------------------------------
            // 各行を中央揃えで描画
            //----------------------------------------

            foreach (string line in lines)
            {
                Vector2 size =
                    Game1.smallFont.MeasureString(
                        line);

                Vector2 position =
                    new Vector2(
                        bounds.Center.X
                        - size.X
                        * scale / 2f,
                        y);

                spriteBatch.DrawString(
                    Game1.smallFont,
                    line,
                    position,
                    Game1.textColor,
                    0f,
                    Vector2.Zero,
                    scale,
                    SpriteEffects.None,
                    0f);

                y +=
                    size.Y * scale;
            }
        }

        //----------------------------------------
        // 開閉タブ
        //----------------------------------------

        /// <summary>
        /// 開閉タブを描画します。
        /// </summary>
        private static void DrawTab(
            SpriteBatch spriteBatch)
        {
            //----------------------------------------
            // タブ背景
            //----------------------------------------

            IClickableMenu.drawTextureBox(
                spriteBatch,
                Game1.menuTexture,
                new Rectangle(
                    0,
                    256,
                    60,
                    60),
                tabBounds.X,
                tabBounds.Y,
                tabBounds.Width,
                tabBounds.Height,
                Color.White
                    * GetTabOpacity(),
                1f / GetShortcutPanelDrawDateTimeScale(),
                drawShadow: false);

            //----------------------------------------
            // 三角矢印
            //----------------------------------------

            DrawArrow(
                spriteBatch,
                tabBounds,
                pointsRight: isOpen);
        }

        //----------------------------------------
        // 矢印
        //----------------------------------------

        /// <summary>
        /// 開閉用の三角矢印を描画します。
        /// </summary>
        private static void DrawArrow(
            SpriteBatch spriteBatch,
            Rectangle bounds,
            bool pointsRight)
        {
            float tabScale =
                GetTabScale();

            float drawDateTimeScale =
                GetShortcutPanelDrawDateTimeScale();

            float inverse =
                drawDateTimeScale > 0f
                    ? 1f / drawDateTimeScale
                    : 1f;

            // AndroidではRenderedHudのSpriteBatchにDateTimeScaleが
            // 掛かったままなので、矢印自体の大きさも逆倍率にして
            // 画面上では時計サイズ100のときと同じ大きさに保ちます。
            int arrowWidth =
                Math.Max(1,
                    (int)MathF.Round(
                        ScaleLayoutValue(
                            12,
                            tabScale)
                        * inverse));

            int arrowHeight =
                Math.Max(2,
                    (int)MathF.Round(
                        ScaleLayoutValue(
                            20,
                            tabScale)
                        * inverse));

            int lineHeight =
                Math.Max(1,
                    (int)MathF.Round(
                        ScaleLayoutValue(
                            1,
                            tabScale)
                        * inverse));

            int centerX =
                bounds.Center.X;

            int centerY =
                bounds.Center.Y;

            //----------------------------------------
            // 横線を積み重ねて
            // 三角形を作る
            //----------------------------------------

            for (int y = 0;
                 y < arrowHeight;
                 y++)
            {
                float progress =
                    y
                    / (float)(arrowHeight - 1);

                float distanceFromCenter =
                    System.Math.Abs(
                        progress * 2f - 1f);

                int lineWidth =
                    System.Math.Max(
                        1,
                        (int)(
                            arrowWidth
                            * (1f - distanceFromCenter)));

                int drawY =
                    centerY
                    - arrowHeight / 2
                    + y;

                int drawX;

                if (pointsRight)
                {
                    drawX =
                        centerX
                        - arrowWidth / 2;
                }
                else
                {
                    drawX =
                        centerX
                        + arrowWidth / 2
                        - lineWidth;
                }

                spriteBatch.Draw(
                    Game1.staminaRect,
                    new Rectangle(
                        drawX,
                        drawY,
                        lineWidth,
                        lineHeight),
                    Game1.textColor);
            }
        }
    }
}
