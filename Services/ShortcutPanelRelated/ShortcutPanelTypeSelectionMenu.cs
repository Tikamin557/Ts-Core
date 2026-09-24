using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace Ts_Core.Services.ShortcutPanelRelated
{
    /// <summary>
    /// Shortcut Panelへ何を登録するか
    /// 選択するメニューです。
    /// </summary>
    internal sealed class ShortcutPanelTypeSelectionMenu
        : IClickableMenu
    {
        //----------------------------------------
        // Translation
        //----------------------------------------

        private readonly ITranslationHelper
            translation;

        //----------------------------------------
        // Callback
        //----------------------------------------

        private readonly Action
            onKeybindSelected;

        private readonly Action
            onModActionSelected;

        private readonly Action
            onGmcmSelected;

        private readonly bool
            isGmcmInstalled;

        //----------------------------------------
        // Bounds
        //----------------------------------------

        private Rectangle keybindBounds;

        private Rectangle modActionBounds;

        private Rectangle gmcmBounds;

        private Rectangle cancelBounds;

        //----------------------------------------
        // Layout
        //----------------------------------------

        /// <summary>
        /// メニューの最小幅。
        /// </summary>
        private const int MinMenuWidth = 440;

        /// <summary>
        /// タイトル左右の余白。
        /// </summary>
        private const int TitleHorizontalPadding = 32;

        /// <summary>
        /// メニューと画面端の最低余白。
        /// </summary>
        private const int ScreenMargin = 32;

        private const int MenuHeight = 436;

        private const int Padding = 24;

        private const int TitleHeight = 72;

        private const int ButtonHeight = 64;

        private const int ButtonSpacing = 12;

        //----------------------------------------
        // Constructor
        //----------------------------------------

        /// <summary>
        /// 空きスロットへ登録する種類（キー・Mod機能・GMCM設定）を選ぶメニューを初期化します。
        /// </summary>
        internal ShortcutPanelTypeSelectionMenu(
            ITranslationHelper translation,
            Action onKeybindSelected,
            Action onModActionSelected,
            Action onGmcmSelected,
            bool isGmcmInstalled)
        {
            this.translation =
                translation;

            this.onKeybindSelected =
                onKeybindSelected;

            this.onModActionSelected =
                onModActionSelected;

            this.onGmcmSelected =
                onGmcmSelected;

            this.isGmcmInstalled =
                isGmcmInstalled;

            //----------------------------------------
            // タイトル
            //----------------------------------------

            string titleText =
                translation.Get(
                    "shortcutPanel.TypeSelection.title");

            Vector2 titleSize =
                Game1.dialogueFont.MeasureString(
                    titleText);

            //----------------------------------------
            // 必要なメニュー幅
            //----------------------------------------

            int requiredMenuWidth =
                (int)Math.Ceiling(
                    titleSize.X)
                + TitleHorizontalPadding * 2;

            int maxMenuWidth =
                Math.Max(
                    MinMenuWidth,
                    Game1.uiViewport.Width
                    - ScreenMargin * 2);

            int menuWidth =
                Math.Min(
                    Math.Max(
                        MinMenuWidth,
                        requiredMenuWidth),
                    maxMenuWidth);

            //----------------------------------------
            // 画面中央
            //----------------------------------------

            int x =
                Game1.uiViewport.Width / 2
                - menuWidth / 2;

            int y =
                Game1.uiViewport.Height / 2
                - MenuHeight / 2;

            initialize(
                x,
                y,
                menuWidth,
                MenuHeight);

            //----------------------------------------
            // Button Bounds
            //----------------------------------------

            int buttonX =
                xPositionOnScreen
                + Padding;

            int buttonWidth =
                width
                - Padding * 2;

            int buttonY =
                yPositionOnScreen
                + Padding
                + TitleHeight;

            //----------------------------------------
            // Keybind
            //----------------------------------------

            keybindBounds =
                new Rectangle(
                    buttonX,
                    buttonY,
                    buttonWidth,
                    ButtonHeight);

            buttonY +=
                ButtonHeight
                + ButtonSpacing;

            //----------------------------------------
            // Mod Action
            //----------------------------------------

            modActionBounds =
                new Rectangle(
                    buttonX,
                    buttonY,
                    buttonWidth,
                    ButtonHeight);

            buttonY +=
                ButtonHeight
                + ButtonSpacing;

            //----------------------------------------
            // GMCM
            //----------------------------------------

            gmcmBounds =
                new Rectangle(
                    buttonX,
                    buttonY,
                    buttonWidth,
                    ButtonHeight);

            buttonY +=
                ButtonHeight
                + ButtonSpacing;

            //----------------------------------------
            // Cancel
            //----------------------------------------

            cancelBounds =
                new Rectangle(
                    buttonX,
                    buttonY,
                    buttonWidth,
                    ButtonHeight);
        }

        //----------------------------------------
        // Click
        //----------------------------------------

        /// <summary>
        /// 登録種類の選択またはキャンセルのクリックを処理し、次の登録画面へ進みます。
        /// </summary>
        public override void receiveLeftClick(
            int x,
            int y,
            bool playSound = true)
        {
            //----------------------------------------
            // Keybind
            //----------------------------------------

            if (keybindBounds.Contains(
                x,
                y))
            {
                Game1.playSound(
                    "smallSelect");

                exitThisMenu();

                onKeybindSelected();

                return;
            }

            //----------------------------------------
            // Mod Action
            //----------------------------------------

            if (modActionBounds.Contains(
                x,
                y))
            {
                Game1.playSound(
                    "smallSelect");

                exitThisMenu();

                onModActionSelected();

                return;
            }

            //----------------------------------------
            // GMCM
            //----------------------------------------

            if (gmcmBounds.Contains(
                x,
                y))
            {
                Game1.playSound(
                    "smallSelect");

                exitThisMenu();

                onGmcmSelected();

                return;
            }

            //----------------------------------------
            // Cancel
            //----------------------------------------

            if (cancelBounds.Contains(
                x,
                y))
            {
                Game1.playSound(
                    "bigDeSelect");

                exitThisMenu();
            }
        }

        //----------------------------------------
        // Draw
        //----------------------------------------

        /// <summary>
        /// ショートカット種類の選択肢と説明を描画します。
        /// </summary>
        public override void draw(
            SpriteBatch b)
        {
            //----------------------------------------
            // 背景を暗くする
            //----------------------------------------

            b.Draw(
                Game1.fadeToBlackRect,
                new Rectangle(
                    0,
                    0,
                    Game1.uiViewport.Width,
                    Game1.uiViewport.Height),
                Color.Black * 0.5f);

            //----------------------------------------
            // メニュー背景
            //----------------------------------------

            IClickableMenu.drawTextureBox(
                b,
                Game1.menuTexture,
                new Rectangle(
                    0,
                    256,
                    60,
                    60),
                xPositionOnScreen,
                yPositionOnScreen,
                width,
                height,
                Color.White,
                1f,
                drawShadow: true);

            //----------------------------------------
            // Title
            //----------------------------------------

            DrawCenteredText(
                b,
                translation.Get(
                    "shortcutPanel.TypeSelection.title"),
                yPositionOnScreen
                + Padding,
                Game1.dialogueFont);

            //----------------------------------------
            // Keybind
            //----------------------------------------

            DrawButton(
                b,
                keybindBounds,
                translation.Get(
                    "shortcutPanel.TypeSelection.key"));

            //----------------------------------------
            // Mod Action
            //----------------------------------------

            DrawButton(
                b,
                modActionBounds,
                translation.Get(
                    "shortcutPanel.TypeSelection.modFunction"));

            //----------------------------------------
            // GMCM
            //----------------------------------------

            DrawButton(
                b,
                gmcmBounds,
                translation.Get(
                    "shortcutPanel.TypeSelection.gmcm"),
                isGmcmInstalled);

            //----------------------------------------
            // Cancel
            //----------------------------------------

            DrawButton(
                b,
                cancelBounds,
                translation.Get(
                    "shortcutPanel.cancel"));

            //----------------------------------------
            // Mouse
            //----------------------------------------

            drawMouse(
                b);
        }

        //----------------------------------------
        // Draw Button
        //----------------------------------------

        /// <summary>
        /// 種類選択メニューで使用するボタンを描画します。
        /// </summary>
        private static void DrawButton(
            SpriteBatch b,
            Rectangle bounds,
            string text,
            bool enabled = true)
        {
            IClickableMenu.drawTextureBox(
                b,
                Game1.menuTexture,
                new Rectangle(
                    0,
                    256,
                    60,
                    60),
                bounds.X,
                bounds.Y,
                bounds.Width,
                bounds.Height,
                enabled
                    ? Color.White
                    : Color.White * 0.45f,
                1f,
                drawShadow: false);

            Vector2 textSize =
                Game1.smallFont.MeasureString(
                    text);

            Vector2 textPosition =
                new Vector2(
                    bounds.Center.X
                    - textSize.X / 2f,
                    bounds.Center.Y
                    - textSize.Y / 2f);

            b.DrawString(
                Game1.smallFont,
                text,
                textPosition,
                enabled
                    ? Game1.textColor
                    : Game1.textColor * 0.45f);
        }

        //----------------------------------------
        // Draw Centered Text
        //----------------------------------------

        /// <summary>
        /// 指定範囲の中央へ文字列を描画します。
        /// </summary>
        private static void DrawCenteredText(
            SpriteBatch b,
            string text,
            int y,
            SpriteFont font)
        {
            Vector2 textSize =
                font.MeasureString(
                    text);

            Vector2 position =
                new Vector2(
                    Game1.uiViewport.Width / 2f
                    - textSize.X / 2f,
                    y);

            b.DrawString(
                font,
                text,
                position,
                Game1.textColor);
        }
    }
}
