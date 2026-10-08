using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace Ts_Core.Services.ShortcutPanelRelated
{
    /// <summary>
    /// Shortcut Panelへ登録するCJB Cheats Menuのワープ先を選択するメニューです。
    /// CJB Cheats MenuのWarpタブと同様に、セクション見出しごとにワープ先をまとめて表示します。
    /// </summary>
    internal sealed class ShortcutPanelCjbWarpSelectionMenu : IClickableMenu
    {
        private readonly ITranslationHelper translation;
        private readonly List<DisplayRow> rows = new();
        private readonly Action<string> onSelected;
        private readonly bool isCjbInstalled;

        private readonly List<Rectangle> rowBounds = new();
        private Rectangle upBounds;
        private Rectangle downBounds;
        private Rectangle cancelBounds;

        private int scrollIndex;

        private const int MinMenuWidth = 520;
        private const int ScreenMargin = 32;
        private const int Padding = 24;
        private const int TitleHeight = 60;
        private const int RowHeight = 64;
        private const int RowSpacing = 8;
        private const int VisibleRows = 6;
        private const int ScrollButtonWidth = 64;
        private const int CancelHeight = 64;

        /// <summary>
        /// ショートカットへ登録するCJB Cheats Menuのワープ先を選択するメニューを初期化します。
        /// </summary>
        internal ShortcutPanelCjbWarpSelectionMenu(
            ITranslationHelper translation,
            IReadOnlyList<CjbWarpEntry> warps,
            Action<string> onSelected,
            bool isCjbInstalled)
        {
            this.translation = translation;
            this.onSelected = onSelected;
            this.isCjbInstalled = isCjbInstalled;

            BuildDisplayRows(warps);

            string title = translation.Get("shortcutPanel.CjbWarp.title");
            float longestTextWidth = Game1.dialogueFont.MeasureString(title).X;

            foreach (DisplayRow row in rows)
            {
                SpriteFont font = row.IsSection ? Game1.dialogueFont : Game1.smallFont;
                float textWidth = font.MeasureString(row.Text).X;
                if (textWidth > longestTextWidth)
                    longestTextWidth = textWidth;
            }

            int requiredWidth =
                Padding * 2 +
                (int)Math.Ceiling(longestTextWidth) +
                ScrollButtonWidth + 32;

            int maxWidth = Math.Max(
                MinMenuWidth,
                Game1.uiViewport.Width - ScreenMargin * 2);

            int menuWidth = Math.Min(
                Math.Max(MinMenuWidth, requiredWidth),
                maxWidth);

            int listHeight =
                VisibleRows * RowHeight +
                (VisibleRows - 1) * RowSpacing;

            int menuHeight =
                Padding + TitleHeight + listHeight +
                Padding + CancelHeight + Padding;

            int x = Game1.uiViewport.Width / 2 - menuWidth / 2;
            int y = Game1.uiViewport.Height / 2 - menuHeight / 2;

            initialize(x, y, menuWidth, menuHeight);

            int listX = xPositionOnScreen + Padding;
            int listY = yPositionOnScreen + Padding + TitleHeight;
            int listWidth = width - Padding * 2 - ScrollButtonWidth - 12;

            for (int i = 0; i < VisibleRows; i++)
            {
                rowBounds.Add(new Rectangle(
                    listX,
                    listY + i * (RowHeight + RowSpacing),
                    listWidth,
                    RowHeight));
            }

            int scrollX = listX + listWidth + 12;

            upBounds = new Rectangle(
                scrollX,
                listY,
                ScrollButtonWidth,
                RowHeight);

            downBounds = new Rectangle(
                scrollX,
                listY + listHeight - RowHeight,
                ScrollButtonWidth,
                RowHeight);

            cancelBounds = new Rectangle(
                xPositionOnScreen + Padding,
                yPositionOnScreen + height - Padding - CancelHeight,
                width - Padding * 2,
                CancelHeight);
        }

        /// <summary>CJBのセクション順を維持したまま、見出しとワープ先の表示行を作成します。</summary>
        private void BuildDisplayRows(IReadOnlyList<CjbWarpEntry> warps)
        {
            string? currentSectionId = null;

            foreach (CjbWarpEntry warp in warps)
            {
                if (!string.Equals(currentSectionId, warp.SectionId, StringComparison.Ordinal))
                {
                    currentSectionId = warp.SectionId;
                    rows.Add(new DisplayRow($"{warp.SectionName}:", null, true));
                }

                rows.Add(new DisplayRow(warp.DisplayName, warp.Id, false));
            }
        }

        /// <summary>一覧項目・スクロールボタン・キャンセルボタンのクリックを処理します。</summary>
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (isCjbInstalled && upBounds.Contains(x, y))
            {
                Scroll(-1);
                return;
            }

            if (isCjbInstalled && downBounds.Contains(x, y))
            {
                Scroll(1);
                return;
            }

            if (isCjbInstalled)
            {
                for (int i = 0; i < rowBounds.Count; i++)
                {
                    int rowIndex = scrollIndex + i;
                    if (rowIndex >= rows.Count)
                        break;

                    DisplayRow row = rows[rowIndex];
                    if (row.IsSection || row.WarpId is null || !rowBounds[i].Contains(x, y))
                        continue;

                    Game1.playSound("smallSelect");
                    onSelected(row.WarpId);
                    exitThisMenu();
                    return;
                }
            }

            if (cancelBounds.Contains(x, y))
            {
                Game1.playSound("bigDeSelect");
                exitThisMenu();
            }
        }

        /// <summary>マウスホイールによる一覧のスクロールを処理します。</summary>
        public override void receiveScrollWheelAction(int direction)
        {
            base.receiveScrollWheelAction(direction);

            if (!isCjbInstalled)
                return;

            Scroll(direction > 0 ? -1 : 1);
        }

        /// <summary>一覧の表示開始位置を指定量だけ移動します。</summary>
        private void Scroll(int amount)
        {
            int maxScroll = Math.Max(0, rows.Count - VisibleRows);
            int next = Math.Clamp(scrollIndex + amount, 0, maxScroll);

            if (next == scrollIndex)
                return;

            scrollIndex = next;
            Game1.playSound("shiny4");
        }

        /// <summary>CJB Cheats Menuのセクション・ワープ先一覧・スクロール操作・キャンセルボタンを描画します。</summary>
        public override void draw(SpriteBatch b)
        {
            b.Draw(
                Game1.fadeToBlackRect,
                new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height),
                Color.Black * 0.5f);

            IClickableMenu.drawTextureBox(
                b,
                Game1.menuTexture,
                new Rectangle(0, 256, 60, 60),
                xPositionOnScreen,
                yPositionOnScreen,
                width,
                height,
                Color.White,
                1f,
                drawShadow: true);

            string title = translation.Get("shortcutPanel.CjbWarp.title");
            Vector2 titleSize = Game1.dialogueFont.MeasureString(title);
            b.DrawString(
                Game1.dialogueFont,
                title,
                new Vector2(
                    xPositionOnScreen + width / 2f - titleSize.X / 2f,
                    yPositionOnScreen + Padding),
                Game1.textColor);

            if (!isCjbInstalled)
            {
                string message = translation.Get("shortcutPanel.CjbWarp.notInstalled");
                Vector2 messageSize = Game1.smallFont.MeasureString(message);
                float messageCenterY = rowBounds[0].Top + (rowBounds[^1].Bottom - rowBounds[0].Top) / 2f;

                b.DrawString(
                    Game1.smallFont,
                    message,
                    new Vector2(
                        xPositionOnScreen + width / 2f - messageSize.X / 2f,
                        messageCenterY - messageSize.Y / 2f),
                    Game1.textColor);
            }
            else
            {
                for (int i = 0; i < rowBounds.Count; i++)
                {
                    int rowIndex = scrollIndex + i;
                    if (rowIndex >= rows.Count)
                        break;

                    Rectangle bounds = rowBounds[i];
                    DisplayRow row = rows[rowIndex];

                    if (row.IsSection)
                        DrawSectionHeader(b, bounds, row.Text);
                    else
                        DrawButton(b, bounds, row.Text, true);
                }
            }

            DrawScrollButton(
                b,
                upBounds,
                pointsUp: true,
                enabled: isCjbInstalled && scrollIndex > 0);

            DrawScrollButton(
                b,
                downBounds,
                pointsUp: false,
                enabled: isCjbInstalled && scrollIndex < Math.Max(0, rows.Count - VisibleRows));

            DrawButton(b, cancelBounds, translation.Get("shortcutPanel.cancel"), true);
            drawMouse(b);
        }

        /// <summary>CJB Cheats MenuのWarpタブ風に、ボタン枠を付けずセクション見出しを描画します。</summary>
        private static void DrawSectionHeader(SpriteBatch b, Rectangle bounds, string text)
        {
            Vector2 size = Game1.dialogueFont.MeasureString(text);
            float maxWidth = bounds.Width - 12;
            float scale = size.X > maxWidth && size.X > 0f ? maxWidth / size.X : 1f;

            b.DrawString(
                Game1.dialogueFont,
                text,
                new Vector2(
                    bounds.X + 6,
                    bounds.Center.Y - size.Y * scale / 2f),
                Game1.textColor,
                0f,
                Vector2.Zero,
                scale,
                SpriteEffects.None,
                0f);
        }

        /// <summary>スクロール用の上下ボタンを描画します。三角形はフォントを使わず直接描画します。</summary>
        private static void DrawScrollButton(SpriteBatch b, Rectangle bounds, bool pointsUp, bool enabled)
        {
            IClickableMenu.drawTextureBox(
                b,
                Game1.menuTexture,
                new Rectangle(0, 256, 60, 60),
                bounds.X,
                bounds.Y,
                bounds.Width,
                bounds.Height,
                enabled ? Color.White : Color.White * 0.45f,
                1f,
                drawShadow: false);

            const int arrowWidth = 20;
            const int arrowHeight = 12;
            int centerX = bounds.Center.X;
            int centerY = bounds.Center.Y;
            Color arrowColor = enabled ? Game1.textColor : Game1.textColor * 0.45f;

            for (int y = 0; y < arrowHeight; y++)
            {
                float progress = y / (float)(arrowHeight - 1);
                int lineWidth = pointsUp
                    ? Math.Max(1, (int)(arrowWidth * progress))
                    : Math.Max(1, (int)(arrowWidth * (1f - progress)));

                b.Draw(
                    Game1.staminaRect,
                    new Rectangle(centerX - lineWidth / 2, centerY - arrowHeight / 2 + y, lineWidth, 1),
                    arrowColor);
            }
        }

        /// <summary>選択メニュー内で使用するボタンを描画します。</summary>
        private static void DrawButton(SpriteBatch b, Rectangle bounds, string text, bool enabled)
        {
            IClickableMenu.drawTextureBox(
                b,
                Game1.menuTexture,
                new Rectangle(0, 256, 60, 60),
                bounds.X,
                bounds.Y,
                bounds.Width,
                bounds.Height,
                enabled ? Color.White : Color.White * 0.45f,
                1f,
                drawShadow: false);

            Vector2 size = Game1.smallFont.MeasureString(text);
            float maxWidth = bounds.Width - 24;
            float scale = size.X > maxWidth && size.X > 0f ? maxWidth / size.X : 1f;

            b.DrawString(
                Game1.smallFont,
                text,
                new Vector2(bounds.X + 12, bounds.Center.Y - size.Y * scale / 2f),
                enabled ? Game1.textColor : Game1.textColor * 0.45f,
                0f,
                Vector2.Zero,
                scale,
                SpriteEffects.None,
                0f);
        }

        /// <summary>選択メニューに表示する1行分の情報です。</summary>
        private sealed record DisplayRow(string Text, string? WarpId, bool IsSection);
    }
}
