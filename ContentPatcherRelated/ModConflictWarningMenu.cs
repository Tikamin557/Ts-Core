using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace Ts_Core.Services.ContentPatcherRelated
{
    /// <summary>
    /// タイトル画面でMod競合警告を確認するためのメニューです。
    /// SpriteText/DialogueBoxを使わず、長い警告文も通常フォントで描画します。
    /// </summary>
    internal sealed class ModConflictWarningMenu : IClickableMenu
    {
        private readonly List<string> lines = new();
        private readonly string title;
        private Rectangle textBounds;
        private Rectangle okBounds;
        private Rectangle scrollTrackBounds;
        private bool draggingScrollBar;
        private int dragOffsetY;
        private int scrollLine;
        private int visibleLines;
        private int screenWidth;
        private int screenHeight;
        private const int Margin = 24;
        private const int LineHeight = 36;

        internal ModConflictWarningMenu(string title, string message)
        {
            this.title = title;
            RebuildLayout(message);
        }

        /// <summary>タイトル画面の実際のUI描画サイズに合わせて配置します。</summary>
        private void RebuildLayout(string message)
        {
            RenderTarget2D? uiScreen = Game1.game1.uiScreen;
            if (uiScreen != null && !uiScreen.IsDisposed && !uiScreen.IsContentLost)
            {
                screenWidth = uiScreen.Width;
                screenHeight = uiScreen.Height;
            }
            else
            {
                float scale = Math.Max(0.01f, Game1.options.uiScale);
                screenWidth = (int)MathF.Ceiling(Game1.graphics.GraphicsDevice.Viewport.Width / scale);
                screenHeight = (int)MathF.Ceiling(Game1.graphics.GraphicsDevice.Viewport.Height / scale);
            }

            int menuWidth = Math.Min(920, Math.Max(320, screenWidth - Margin * 2));
            int menuHeight = Math.Min(600, Math.Max(240, screenHeight - Margin * 2));
            initialize((screenWidth - menuWidth) / 2, (screenHeight - menuHeight) / 2, menuWidth, menuHeight);
            textBounds = new Rectangle(xPositionOnScreen + 36, yPositionOnScreen + 100,
                Math.Max(100, width - 108), Math.Max(40, height - 196));
            scrollTrackBounds = new Rectangle(textBounds.Right + 14, textBounds.Y, 12, textBounds.Height);
            okBounds = new Rectangle(xPositionOnScreen + (width - 160) / 2,
                yPositionOnScreen + height - 78, 160, 56);
            visibleLines = Math.Max(1, textBounds.Height / LineHeight);
            lines.Clear();
            foreach (string paragraph in message.Replace("\r", "").Split('\n'))
                WrapParagraph(paragraph);
            scrollLine = 0;
        }

        /// <summary>フォントの実測幅で改行し、長いMod名も表示領域内に収めます。</summary>
        private void WrapParagraph(string paragraph)
        {
            if (string.IsNullOrEmpty(paragraph))
            {
                lines.Add(string.Empty);
                return;
            }

            string current = string.Empty;
            foreach (char character in paragraph)
            {
                string candidate = current + character;
                if (current.Length > 0 && Game1.smallFont.MeasureString(candidate).X > textBounds.Width)
                {
                    lines.Add(current);
                    current = character.ToString();
                }
                else
                    current = candidate;
            }
            if (current.Length > 0)
                lines.Add(current);
        }

        /// <summary>表示範囲に応じてスクロールバーのつまみを計算します。</summary>
        private Rectangle GetScrollThumbBounds()
        {
            int maxScroll = Math.Max(0, lines.Count - visibleLines);
            int thumbHeight = Math.Max(28, (int)((long)scrollTrackBounds.Height * visibleLines / Math.Max(1, lines.Count)));
            thumbHeight = Math.Min(scrollTrackBounds.Height, thumbHeight);
            int travel = scrollTrackBounds.Height - thumbHeight;
            int thumbY = scrollTrackBounds.Y + (maxScroll == 0 ? 0 : (int)Math.Round((double)scrollLine * travel / maxScroll));
            return new Rectangle(scrollTrackBounds.X, thumbY, scrollTrackBounds.Width, thumbHeight);
        }

        /// <summary>マウス位置からスクロール位置を更新します。</summary>
        private void ScrollToMouse(int mouseY)
        {
            int maxScroll = Math.Max(0, lines.Count - visibleLines);
            Rectangle thumb = GetScrollThumbBounds();
            int travel = scrollTrackBounds.Height - thumb.Height;
            if (travel > 0)
                scrollLine = Math.Clamp((int)Math.Round((double)(mouseY - dragOffsetY - scrollTrackBounds.Y) * maxScroll / travel), 0, maxScroll);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (lines.Count > visibleLines && scrollTrackBounds.Contains(x, y))
            {
                Rectangle thumb = GetScrollThumbBounds();
                dragOffsetY = thumb.Contains(x, y) ? y - thumb.Y : thumb.Height / 2;
                draggingScrollBar = true;
                ScrollToMouse(y);
                return;
            }
            if (okBounds.Contains(x, y))
            {
                Game1.playSound("bigDeSelect");
                exitThisMenu();
            }
        }

        public override void leftClickHeld(int x, int y)
        {
            if (draggingScrollBar)
                ScrollToMouse(y);
        }

        public override void releaseLeftClick(int x, int y)
        {
            draggingScrollBar = false;
        }

        public override void receiveScrollWheelAction(int direction)
        {
            scrollLine = Math.Clamp(scrollLine - Math.Sign(direction) * 3,
                0, Math.Max(0, lines.Count - visibleLines));
        }

        public override void draw(SpriteBatch b)
        {
            b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, screenWidth, screenHeight), Color.Black * 0.65f);
            IClickableMenu.drawTextureBox(b, xPositionOnScreen, yPositionOnScreen, width, height, Color.White);
            b.DrawString(Game1.dialogueFont, title,
                new Vector2(xPositionOnScreen + 36, yPositionOnScreen + 28), Game1.textColor);

            for (int i = 0; i < visibleLines && i + scrollLine < lines.Count; i++)
                b.DrawString(Game1.smallFont, lines[i + scrollLine],
                    new Vector2(textBounds.X, textBounds.Y + i * LineHeight), Game1.textColor);

            if (lines.Count > visibleLines)
            {
                // 行数表示の代わりに右側へスクロールバーを描画します。
                b.Draw(Game1.fadeToBlackRect, scrollTrackBounds, Color.Black * 0.22f);
                b.Draw(Game1.fadeToBlackRect, GetScrollThumbBounds(), Color.SaddleBrown * 0.9f);
            }

            IClickableMenu.drawTextureBox(b, okBounds.X, okBounds.Y, okBounds.Width, okBounds.Height, Color.White);
            Vector2 labelSize = Game1.smallFont.MeasureString("OK");
            b.DrawString(Game1.smallFont, "OK", new Vector2(
                okBounds.Center.X - labelSize.X / 2, okBounds.Center.Y - labelSize.Y / 2), Game1.textColor);
            drawMouse(b);
        }
    }
}
