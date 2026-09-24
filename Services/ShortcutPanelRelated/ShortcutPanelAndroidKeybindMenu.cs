using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;

namespace Ts_Core.Services.ShortcutPanelRelated
{
    /// <summary>
    /// Android用の単一キー選択メニューです。
    /// 物理キーボード入力を待つ代わりに、画面上の一覧からSButtonを選択します。
    /// </summary>
    internal sealed class ShortcutPanelAndroidKeybindMenu : IClickableMenu
    {
        //----------------------------------------
        // 基本情報
        //----------------------------------------

        private readonly ITranslationHelper translation;
        //----------------------------------------
        // キー選択完了時の処理
        //----------------------------------------

        private readonly Action<KeybindList> onSelected;
        //----------------------------------------
        // 登録可能なキーと画面上のボタン
        //----------------------------------------

        private readonly List<(SButton Button, string Label)> buttons = new();
        private readonly List<Rectangle> visibleBounds = new();

        //----------------------------------------
        // ページ操作ボタンの表示範囲
        //----------------------------------------

        private Rectangle previousBounds;
        private Rectangle nextBounds;
        private Rectangle cancelBounds;
        //----------------------------------------
        // 現在表示しているページ
        //----------------------------------------

        private int page;

        //----------------------------------------
        // キー一覧とメニューのレイアウト設定
        //----------------------------------------

        private const int Columns = 4;
        private const int Rows = 5;
        private const int PerPage = Columns * Rows;
        private const int Padding = 24;
        private const int Gap = 8;
        private const int ButtonHeight = 58;
        private const int NavigationHeight = 58;
        private const int MenuWidth = 760;
        private const int MenuHeight = 520;

        /// <summary>
        /// Androidでショートカットへ登録するキーを選ぶための専用メニューを初期化します。
        /// </summary>
        internal ShortcutPanelAndroidKeybindMenu(
            ITranslationHelper translation,
            Action<KeybindList> onSelected)
        {
            this.translation = translation;
            this.onSelected = onSelected;

            AddKeyboardButtons();

            int width = Math.Min(MenuWidth, Game1.uiViewport.Width - 32);
            int height = Math.Min(MenuHeight, Game1.uiViewport.Height - 32);
            initialize(
                Game1.uiViewport.Width / 2 - width / 2,
                Game1.uiViewport.Height / 2 - height / 2,
                width,
                height);

            UpdateBounds();
        }

        /// <summary>
        /// Androidで登録可能な文字キー・数字キー・Fキー・特殊キーなどを一覧へ追加します。
        /// </summary>
        private void AddKeyboardButtons()
        {
            string[] names =
            {
                "A","B","C","D","E","F","G","H","I","J","K","L","M",
                "N","O","P","Q","R","S","T","U","V","W","X","Y","Z",
                "D0","D1","D2","D3","D4","D5","D6","D7","D8","D9",
                "F1","F2","F3","F4","F5","F6","F7","F8","F9","F10","F11","F12",
                "Space","Enter","Escape","Tab","Back","Delete","Home","End","PageUp","PageDown",
                "Up","Down","Left","Right",
                "LeftShift","RightShift","LeftControl","RightControl","LeftAlt","RightAlt"
            };

            foreach (string name in names)
            {
                if (!Enum.TryParse(name, ignoreCase: false, out SButton button))
                    continue;

                string label = name.StartsWith("D")
                    && name.Length == 2
                    && char.IsDigit(name[1])
                        ? name[1].ToString()
                        : name;

                buttons.Add((button, label));
            }
        }

        private int PageCount => Math.Max(1, (buttons.Count + PerPage - 1) / PerPage);

        /// <summary>
        /// 現在のページに表示するキーと、前へ・キャンセル・次へボタンの位置を計算します。
        /// </summary>
        private void UpdateBounds()
        {
            visibleBounds.Clear();

            int contentTop = yPositionOnScreen + 82;
            int contentBottom = yPositionOnScreen + height - Padding - NavigationHeight - Gap;
            int availableHeight = Math.Max(1, contentBottom - contentTop);
            int rowHeight = Math.Min(ButtonHeight, (availableHeight - Gap * (Rows - 1)) / Rows);
            int buttonWidth = (width - Padding * 2 - Gap * (Columns - 1)) / Columns;

            for (int i = 0; i < PerPage; i++)
            {
                int column = i % Columns;
                int row = i / Columns;
                visibleBounds.Add(new Rectangle(
                    xPositionOnScreen + Padding + column * (buttonWidth + Gap),
                    contentTop + row * (rowHeight + Gap),
                    buttonWidth,
                    rowHeight));
            }

            int navY = yPositionOnScreen + height - Padding - NavigationHeight;
            int navWidth = (width - Padding * 2 - Gap * 2) / 3;

            previousBounds = new Rectangle(xPositionOnScreen + Padding, navY, navWidth, NavigationHeight);
            cancelBounds = new Rectangle(previousBounds.Right + Gap, navY, navWidth, NavigationHeight);
            nextBounds = new Rectangle(cancelBounds.Right + Gap, navY, navWidth, NavigationHeight);
        }

        /// <summary>
        /// タップされたキーやページ移動ボタンを判定し、登録またはページ切り替えを行います。
        /// </summary>
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            int start = page * PerPage;
            for (int i = 0; i < visibleBounds.Count; i++)
            {
                int index = start + i;
                if (index >= buttons.Count)
                    break;

                if (!visibleBounds[i].Contains(x, y))
                    continue;

                var selected = buttons[index];
                onSelected(new KeybindList(new Keybind(selected.Button)));
                Game1.playSound("smallSelect");
                exitThisMenu();
                return;
            }

            if (previousBounds.Contains(x, y) && page > 0)
            {
                page--;
                Game1.playSound("shwip");
                return;
            }

            if (nextBounds.Contains(x, y) && page < PageCount - 1)
            {
                page++;
                Game1.playSound("shwip");
                return;
            }

            if (cancelBounds.Contains(x, y))
            {
                Game1.playSound("bigDeSelect");
                exitThisMenu();
            }
        }

        /// <summary>
        /// Android用キー選択メニューのタイトル・キー一覧・ページ操作ボタンを描画します。
        /// </summary>
        public override void draw(SpriteBatch b)
        {
            b.Draw(Game1.fadeToBlackRect,
                new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height),
                Color.Black * 0.5f);

            IClickableMenu.drawTextureBox(
                b, Game1.menuTexture, new Rectangle(0, 256, 60, 60),
                xPositionOnScreen, yPositionOnScreen, width, height,
                Color.White, 1f, drawShadow: true);

            string title = translation.Get("shortcutPanel.Keybind.androidTitle");
            Vector2 titleSize = Game1.dialogueFont.MeasureString(title);
            float titleScale = Math.Min(1f, (width - Padding * 2) / Math.Max(1f, titleSize.X));
            b.DrawString(Game1.dialogueFont, title,
                new Vector2(xPositionOnScreen + width / 2f - titleSize.X * titleScale / 2f,
                    yPositionOnScreen + 28),
                Game1.textColor, 0f, Vector2.Zero, titleScale, SpriteEffects.None, 0f);

            int start = page * PerPage;
            for (int i = 0; i < visibleBounds.Count; i++)
            {
                int index = start + i;
                if (index >= buttons.Count)
                    break;

                Rectangle bounds = visibleBounds[i];
                IClickableMenu.drawTextureBox(
                    b, Game1.menuTexture, new Rectangle(0, 256, 60, 60),
                    bounds.X, bounds.Y, bounds.Width, bounds.Height,
                    Color.White, 1f, drawShadow: false);

                string label = buttons[index].Label;
                Vector2 size = Game1.smallFont.MeasureString(label);
                float scale = Math.Min(1f, (bounds.Width - 12f) / Math.Max(1f, size.X));
                b.DrawString(Game1.smallFont, label,
                    new Vector2(bounds.Center.X - size.X * scale / 2f,
                        bounds.Center.Y - size.Y * scale / 2f),
                    Game1.textColor, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            }

            DrawNavButton(b, previousBounds, "◀", page > 0);
            DrawNavButton(b, cancelBounds, translation.Get("shortcutPanel.cancel"), true);
            DrawNavButton(b, nextBounds, "▶", page < PageCount - 1);

            string pageText = $"{page + 1} / {PageCount}";
            Vector2 pageSize = Game1.tinyFont.MeasureString(pageText);
            b.DrawString(Game1.tinyFont, pageText,
                new Vector2(xPositionOnScreen + width - Padding - pageSize.X,
                    yPositionOnScreen + 12), Game1.textColor);

            drawMouse(b);
        }

        /// <summary>
        /// 前へ・キャンセル・次へなどのナビゲーションボタンを描画します。
        /// </summary>
        private static void DrawNavButton(SpriteBatch b, Rectangle bounds, string text, bool enabled)
        {
            Color color = enabled ? Color.White : Color.White * 0.45f;
            IClickableMenu.drawTextureBox(
                b, Game1.menuTexture, new Rectangle(0, 256, 60, 60),
                bounds.X, bounds.Y, bounds.Width, bounds.Height,
                color, 1f, drawShadow: false);

            Vector2 size = Game1.smallFont.MeasureString(text);
            float scale = Math.Min(1f, (bounds.Width - 12f) / Math.Max(1f, size.X));
            b.DrawString(Game1.smallFont, text,
                new Vector2(bounds.Center.X - size.X * scale / 2f,
                    bounds.Center.Y - size.Y * scale / 2f),
                Game1.textColor * (enabled ? 1f : 0.45f),
                0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }
    }
}
