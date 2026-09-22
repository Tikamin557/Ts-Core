using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using Ts_Core.Services.DebugSupport;
using Ts_Core.Models;

namespace Ts_Core.Services.ShortcutPanelRelated
{
    /// <summary>
    /// ショートカットパネルの表示と入力を管理します。
    /// </summary>
    internal static class ShortcutPanelService
    {
        private static IModHelper helper = null!;

        private static Texture2D missingShortcutIcon =
            null!;

        //----------------------------------------
        // 表示状態
        //----------------------------------------

        private static bool isOpen;

        //----------------------------------------
        // Bounds
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
            IModHelper modHelper)
        {
            helper =
                modHelper;

            //----------------------------------------
            // Assets
            //----------------------------------------

            missingShortcutIcon =
                helper.ModContent.Load<Texture2D>(
                    "assets/ShortcutPanel/MissingShortcut.png");

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
            // TsCore Shortcut
            //----------------------------------------

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

            //----------------------------------------
            // Events
            //----------------------------------------

            helper.Events.Display.RenderedHud
                += OnRenderedHud;

            helper.Events.Input.ButtonPressed
                += OnButtonPressed;

            helper.Events.Input.ButtonReleased
                += OnButtonReleased;
        }

        //----------------------------------------
        // Slot Config
        //----------------------------------------

        /// <summary>
        /// config.jsonから
        /// Shortcut Panelのスロットを復元します。
        /// </summary>
        private static void LoadSlots()
        {
            List<ShortcutPanelSlotConfig> configs =
                ModEntry.Config.ShortcutPanelSlots;

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
                // Mod Action
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
                // Keybind
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
        /// config.jsonへ保存します。
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
                // Mod Action
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
                // Keybind
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
                // None
                //----------------------------------------

                else
                {
                    config.Type =
                        "None";
                }

                configs.Add(
                    config);
            }

            ModEntry.Config.ShortcutPanelSlots =
                configs;

            helper.WriteConfig(
                ModEntry.Config);
        }

        //----------------------------------------
        // Bounds更新
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

            //----------------------------------------
            // 所持金表示の左側にタブを配置
            //----------------------------------------

            int tabX =
                (int)position.X
                + 28
                - TabWidth
                - 4;

            int tabY =
                (int)position.Y
                + 180;

            tabBounds =
                new Rectangle(
                    tabX,
                    tabY,
                    TabWidth,
                    TabHeight);

            //----------------------------------------
            // パネル
            //----------------------------------------

            int panelWidth =
                PanelPadding * 2
                + SlotColumns * SlotSize
                + (SlotColumns - 1) * SlotSpacing;

            int panelHeight =
                PanelPadding * 2
                + SlotRows * SlotSize
                + (SlotRows - 1) * SlotSpacing;

            int panelX =
                tabBounds.Left
                - panelWidth
                - 4;

            int panelY =
                tabBounds.Center.Y
                - panelHeight / 2;

            panelBounds =
                new Rectangle(
                    panelX,
                    panelY,
                    panelWidth,
                    panelHeight);

            //----------------------------------------
            // スロット
            //----------------------------------------

            for (int i = 0; i < SlotCount; i++)
            {
                int column =
                    i % SlotColumns;

                int row =
                    i / SlotColumns;

                int x =
                    panelBounds.X
                    + PanelPadding
                    + column
                    * (SlotSize + SlotSpacing);

                int y =
                    panelBounds.Y
                    + PanelPadding
                    + row
                    * (SlotSize + SlotSpacing);

                slotBounds[i] =
                    new Rectangle(
                        x,
                        y,
                        SlotSize,
                        SlotSize);
            }
        }

        //----------------------------------------
        // Input
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

            //----------------------------------------
            // イベント中は操作しない
            //----------------------------------------

            if (Game1.eventUp)
                return;

            //----------------------------------------
            // Keybind入力待機中
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

                //----------------------------------------
                // 入力を抑制
                //----------------------------------------

                helper.Input.Suppress(
                    e.Button);

                ShortcutPanelSlot slot =
                    slots[i];

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

                    Game1.activeClickableMenu =
                        new ShortcutPanelTypeSelectionMenu(
                            helper.Translation,

                            //----------------------------------------
                            // Key
                            //----------------------------------------

                            onKeybindSelected: () =>
                            {
                                Game1.activeClickableMenu =
                                    new ShortcutPanelKeybindMenu(
                                        helper.Translation,
                                        keybind =>
                                        {
                                            slots[slotIndex]
                                                .SetKeybind(
                                                    keybind);

                                            SaveSlots();
                                        });
                            },

                            //----------------------------------------
                            // Mod Function
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
                            });

                    return;
                }

                //----------------------------------------
                // Mod Action
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
                // Keybind
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
        /// Shortcut Panelのキー登録中に
        /// キーが離された時の処理です。
        /// </summary>
        private static void OnButtonReleased(
            object? sender,
            ButtonReleasedEventArgs e)
        {
            //----------------------------------------
            // Keybind入力待機中のみ処理
            //----------------------------------------

            if (Game1.activeClickableMenu
                is not ShortcutPanelKeybindMenu keybindMenu)
            {
                return;
            }

            //----------------------------------------
            // Keybind入力を更新
            //----------------------------------------

            keybindMenu.ReceiveButtonReleased(
                e.Button);
        }

        //----------------------------------------
        // Draw
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
            // イベント中は表示しない
            //----------------------------------------

            if (Game1.eventUp)
                return;

            if (Game1.dayTimeMoneyBox == null)
                return;

            UpdateBounds();

            SpriteBatch spriteBatch =
                e.SpriteBatch;

            //----------------------------------------
            // パネル
            //----------------------------------------

            if (isOpen)
            {
                DrawPanel(
                    spriteBatch);

                DrawSlots(
                    spriteBatch);
            }

            //----------------------------------------
            // 開閉タブ
            //----------------------------------------

            DrawTab(
                spriteBatch);

            //----------------------------------------
            // Hover Text
            //----------------------------------------

            DrawHoverText(
                spriteBatch);
        }

        //----------------------------------------
        // Hover Text
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

            Point cursor =
                Game1.getMousePosition();

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

                IClickableMenu.drawHoverText(
                    spriteBatch,
                    text,
                    Game1.smallFont);

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
                    IClickableMenu.drawHoverText(
                        spriteBatch,
                        helper.Translation.Get(
                            "shortcutPanel.Hover.register"),
                        Game1.smallFont);

                    return;
                }

                //----------------------------------------
                // Keybind
                //----------------------------------------

                if (slot.Type
                    == ShortcutPanelSlotType.Keybind)
                {
                    IClickableMenu.drawHoverText(
                        spriteBatch,
                        helper.Translation.Get(
                            "shortcutPanel.Hover.remove"),
                        Game1.smallFont);

                    return;
                }

                //----------------------------------------
                // Mod Action
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
                                "shortcutPanel.Hover.remove");
                    }
                    else
                    {
                        text =
                            helper.Translation.Get(
                                "shortcutPanel.Hover.remove");
                    }

                    IClickableMenu.drawHoverText(
                        spriteBatch,
                        text,
                        Game1.smallFont);

                    return;
                }
            }
        }

        //----------------------------------------
        // Panel
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
                Color.White,
                1f,
                drawShadow: false);
        }

        //----------------------------------------
        // Slots
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
                // Slot Background
                //----------------------------------------

                spriteBatch.Draw(
                    Game1.menuTexture,
                    bounds,
                    new Rectangle(
                        128,
                        128,
                        64,
                        64),
                    Color.White);

                ShortcutPanelSlot slot =
                    slots[i];

                //----------------------------------------
                // Mod Action
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

                    const int iconSize = 48;

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
                // Keybind
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
            const int iconSize =
                48;

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
        // Draw Keybind Text
        //----------------------------------------

        private static void DrawKeybindText(
            SpriteBatch spriteBatch,
            Rectangle bounds,
            string text)
        {
            Vector2 textSize =
                Game1.smallFont.MeasureString(
                    text);

            float maxWidth =
                bounds.Width - 10;

            float scale =
                1f;

            if (textSize.X > maxWidth
                && textSize.X > 0)
            {
                scale =
                    maxWidth / textSize.X;
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
                Game1.smallFont,
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
        // Draw Multi Keybind Text
        //----------------------------------------

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

            float maxWidth =
                bounds.Width - 8;

            float maxHeight =
                bounds.Height - 8;

            //----------------------------------------
            // 全体のScaleを計算
            //----------------------------------------

            float scale =
                1f;

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
        // Tab
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
                Color.White,
                1f,
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
        // Arrow
        //----------------------------------------

        /// <summary>
        /// 開閉用の三角矢印を描画します。
        /// </summary>
        private static void DrawArrow(
            SpriteBatch spriteBatch,
            Rectangle bounds,
            bool pointsRight)
        {
            const int arrowWidth = 12;
            const int arrowHeight = 20;

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
                        1),
                    Game1.textColor);
            }
        }
    }
}