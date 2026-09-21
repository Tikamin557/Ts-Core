using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using Ts_Core.Services.DebugSupport;

namespace Ts_Core.Services.ShortcutPanelRelated
{
    /// <summary>
    /// ショートカットパネルの表示と入力を管理します。
    /// </summary>
    internal static class ShortcutPanelService
    {
        private static IModHelper helper = null!;

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
            // TsCore Shortcut
            //----------------------------------------

            ShortcutPanelRegistry.Register(
                new ShortcutPanelEntry(
                    "TsCore/TimeSkip",
                    "Time Skip",
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
                    "Time Skip Duration",
                    () => helper.ModContent.Load<Texture2D>(
                        "assets/ShortcutPanel/TimeSkipDuration.png"),
                    null,
                    () =>
                    {
                        TimeSkipService.TryStartDurationTimeSkip();
                    }));

            //----------------------------------------
            // TimeSkip と TimeSkip Duration を
            // スロットへ割り当て
            //----------------------------------------
            /*
                   slots[0].ShortcutId =
                       "TsCore/TimeSkip";

                   slots[1].ShortcutId =
                       "TsCore/TimeSkipDuration";
            */
            //----------------------------------------
            // Events
            //----------------------------------------

            helper.Events.Display.RenderedHud
                += OnRenderedHud;

            helper.Events.Input.ButtonPressed
                += OnButtonPressed;
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
                isOpen =
                    !isOpen;

                helper.Input.Suppress(
                    e.Button);

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

                ShortcutPanelEntry? entry =
                    slots[i].GetEntry();

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

                    if (entry == null)
                        return;

                    //----------------------------------------
                    // Shortcutを解除
                    //----------------------------------------

                    slots[i].Clear();

                    Game1.playSound(
                        "bigDeSelect");

                    return;
                }

                //----------------------------------------
                // 左クリック
                //----------------------------------------

                if (entry == null)
                {
                    //----------------------------------------
                    // 空スロット
                    //----------------------------------------

                    int slotIndex =
                        i;

                    Game1.playSound(
                        "smallSelect");

                    Game1.activeClickableMenu =
                        new ShortcutPanelSelectionMenu(
                            shortcutId =>
                            {
                                slots[slotIndex].ShortcutId =
                                    shortcutId;
                            });

                    return;
                }

                //----------------------------------------
                // Shortcutを実行
                //----------------------------------------

                entry.Execute();

                return;
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
                    Color.White);

                //----------------------------------------
                // Shortcut取得
                //----------------------------------------

                ShortcutPanelEntry? entry =
                    slots[i].GetEntry();

                if (entry == null)
                {
                    continue;
                }

                //----------------------------------------
                // アイコンテクスチャ取得
                //----------------------------------------

                Texture2D? iconTexture =
                    entry.GetIconTexture();

                if (iconTexture == null)
                {
                    continue;
                }

                //----------------------------------------
                // アイコンの描画範囲
                //----------------------------------------

                const int iconSize = 48;

                Rectangle iconBounds =
                    new Rectangle(
                        bounds.Center.X - iconSize / 2,
                        bounds.Center.Y - iconSize / 2,
                        iconSize,
                        iconSize);

                //----------------------------------------
                // アイコン
                //----------------------------------------

                spriteBatch.Draw(
                    iconTexture,
                    iconBounds,
                    entry.IconSourceRect,
                    Color.White);
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