using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;

namespace Ts_Core.Services.ShortcutPanelRelated
{
    /// <summary>
    /// Shortcut Panelへ登録するキーの
    /// 入力を待機するメニューです。
    /// </summary>
    internal sealed class ShortcutPanelKeybindMenu
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

        private readonly Action<KeybindList>
            onSelected;

        //----------------------------------------
        // Keybind Input
        //----------------------------------------

        private readonly List<SButton>
            pressedButtons = new();

        /// <summary>
        /// 現在押されているキー。
        /// </summary>
        private readonly HashSet<SButton>
            heldButtons = new();

        //----------------------------------------
        // Bounds
        //----------------------------------------

        private Rectangle cancelBounds;

        //----------------------------------------
        // Layout
        //----------------------------------------

        /// <summary>
        /// メニューの最小幅。
        /// </summary>
        private const int MinMenuWidth = 480;

        /// <summary>
        /// タイトル左右の余白。
        /// </summary>
        private const int TitleHorizontalPadding = 32;

        /// <summary>
        /// メニューと画面端の最低余白。
        /// </summary>
        private const int ScreenMargin = 32;

        private const int MenuHeight = 260;

        private const int Padding = 24;

        private const int CancelHeight = 64;

        //----------------------------------------
        // Constructor
        //----------------------------------------

        /// <summary>
        /// PCでショートカットへ登録するキー入力を待ち受けるメニューを初期化します。
        /// </summary>
        internal ShortcutPanelKeybindMenu(
            ITranslationHelper translation,
            Action<KeybindList> onSelected)
        {
            this.translation =
                translation;

            this.onSelected =
                onSelected;

            //----------------------------------------
            // タイトル
            //----------------------------------------

            string titleText =
                translation.Get(
                    "shortcutPanel.Keybind.title");

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
            // Cancel
            //----------------------------------------

            cancelBounds =
                new Rectangle(
                    xPositionOnScreen
                    + Padding,
                    yPositionOnScreen
                    + height
                    - Padding
                    - CancelHeight,
                    width
                    - Padding * 2,
                    CancelHeight);
        }

        //----------------------------------------
        // Key Input
        //----------------------------------------

        /// <summary>
        /// キー入力を受け取り、登録候補となるキーの組み合わせへ追加します。
        /// </summary>
        internal void ReceiveButton(
            SButton button)
        {
            //----------------------------------------
            // マウス入力は登録しない
            //----------------------------------------

            if (button == SButton.MouseLeft
                || button == SButton.MouseRight)
            {
                return;
            }

            //----------------------------------------
            // 今回のKeybindに追加
            //----------------------------------------

            if (!pressedButtons.Contains(
                button))
            {
                pressedButtons.Add(
                    button);
            }

            //----------------------------------------
            // 現在押されているキーとして記録
            //----------------------------------------

            heldButtons.Add(
                button);
        }

        //----------------------------------------
        // Key Released
        //----------------------------------------

        /// <summary>
        /// 押していたキーが離されたことを受け取り、キー登録を確定できる状態か処理します。
        /// </summary>
        internal void ReceiveButtonReleased(
            SButton button)
        {
            //----------------------------------------
            // この登録操作で押されていないキーは
            // 無視する
            //----------------------------------------

            if (!pressedButtons.Contains(
                button))
            {
                return;
            }

            //----------------------------------------
            // 現在押されているキーから削除
            //----------------------------------------

            heldButtons.Remove(
                button);

            //----------------------------------------
            // まだ押されているキーがある場合は
            // 確定しない
            //----------------------------------------

            if (heldButtons.Count > 0)
                return;

            //----------------------------------------
            // まだ何も登録されていない場合
            //----------------------------------------

            if (pressedButtons.Count == 0)
                return;

            //----------------------------------------
            // 入力された全キーから
            // Keybindを作成
            //----------------------------------------

            Keybind keybind =
                new Keybind(
                    pressedButtons.ToArray());

            KeybindList keybindList =
                new KeybindList(
                    keybind);

            Game1.playSound(
                "smallSelect");

            onSelected(
                keybindList);

            exitThisMenu();
        }

        //----------------------------------------
        // Click
        //----------------------------------------

        /// <summary>
        /// キー登録画面のキャンセルなど、マウスクリック操作を処理します。
        /// </summary>
        public override void receiveLeftClick(
            int x,
            int y,
            bool playSound = true)
        {
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
        /// キー入力待機画面と現在入力されているキー情報を描画します。
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
            // Message
            //----------------------------------------

            string text =
                translation.Get(
                    "shortcutPanel.Keybind.title");

            Vector2 textSize =
                Game1.dialogueFont.MeasureString(
                    text);

            Vector2 textPosition =
                new Vector2(
                    xPositionOnScreen
                    + width / 2f
                    - textSize.X / 2f,
                    yPositionOnScreen
                    + 48);

            b.DrawString(
                Game1.dialogueFont,
                text,
                textPosition,
                Game1.textColor);

            //----------------------------------------
            // 現在入力中のKeybind
            //----------------------------------------

            if (pressedButtons.Count > 0)
            {
                string keybindText =
                    string.Join(
                        " + ",
                        pressedButtons);

                Vector2 keybindSize =
                    Game1.smallFont.MeasureString(
                        keybindText);

                float maxWidth =
                    width - Padding * 2;

                float scale =
                    1f;

                if (keybindSize.X > maxWidth
                    && keybindSize.X > 0)
                {
                    scale =
                        maxWidth / keybindSize.X;
                }

                Vector2 keybindPosition =
                    new Vector2(
                        xPositionOnScreen
                        + width / 2f
                        - keybindSize.X
                        * scale / 2f,
                        yPositionOnScreen
                        + 115);

                b.DrawString(
                    Game1.smallFont,
                    keybindText,
                    keybindPosition,
                    Game1.textColor,
                    0f,
                    Vector2.Zero,
                    scale,
                    SpriteEffects.None,
                    0f);
            }

            //----------------------------------------
            // Cancel
            //----------------------------------------

            IClickableMenu.drawTextureBox(
                b,
                Game1.menuTexture,
                new Rectangle(
                    0,
                    256,
                    60,
                    60),
                cancelBounds.X,
                cancelBounds.Y,
                cancelBounds.Width,
                cancelBounds.Height,
                Color.White,
                1f,
                drawShadow: false);

            string cancelText =
                translation.Get(
                    "shortcutPanel.cancel");

            Vector2 cancelSize =
                Game1.smallFont.MeasureString(
                    cancelText);

            Vector2 cancelPosition =
                new Vector2(
                    cancelBounds.Center.X
                    - cancelSize.X / 2f,
                    cancelBounds.Center.Y
                    - cancelSize.Y / 2f);

            b.DrawString(
                Game1.smallFont,
                cancelText,
                cancelPosition,
                Game1.textColor);

            drawMouse(
                b);
        }
    }
}
