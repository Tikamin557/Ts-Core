using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace Ts_Core.Services.ShortcutPanelRelated
{
    /// <summary>
    /// Shortcut Panelへ登録するGMCMのModを選択するメニューです。
    /// </summary>
    internal sealed class ShortcutPanelGmcmSelectionMenu : IClickableMenu
    {
        private readonly ITranslationHelper translation;
        private readonly List<IManifest> entries;
        private readonly Action<string> onSelected;
        private readonly bool isGmcmInstalled;

        private readonly List<Rectangle> entryBounds = new();
        private Rectangle upBounds;
        private Rectangle downBounds;
        private Rectangle cancelBounds;

        private int scrollIndex;

        private const int MinMenuWidth = 520;
        private const int ScreenMargin = 32;
        private const int Padding = 24;
        private const int TitleHeight = 60;
        private const int EntryHeight = 64;
        private const int EntrySpacing = 8;
        private const int VisibleEntries = 6;
        private const int ScrollButtonWidth = 64;
        private const int CancelHeight = 64;

        /// <summary>
        /// ショートカットへ登録するGMCM対応Modを選択するメニューを初期化します。
        /// </summary>
        internal ShortcutPanelGmcmSelectionMenu(
            ITranslationHelper translation,
            IReadOnlyList<IManifest> manifests,
            Action<string> onSelected,
            bool isGmcmInstalled)
        {
            this.translation = translation;
            this.onSelected = onSelected;
            this.isGmcmInstalled = isGmcmInstalled;
            entries = new List<IManifest>(manifests);

            string title = translation.Get("shortcutPanel.SelectGmcmMod.title");
            float longestTextWidth = Game1.dialogueFont.MeasureString(title).X;

            foreach (IManifest entry in entries)
            {
                float textWidth = Game1.smallFont.MeasureString(entry.Name).X;
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
                VisibleEntries * EntryHeight +
                (VisibleEntries - 1) * EntrySpacing;

            int menuHeight =
                Padding + TitleHeight + listHeight +
                Padding + CancelHeight + Padding;

            int x = Game1.uiViewport.Width / 2 - menuWidth / 2;
            int y = Game1.uiViewport.Height / 2 - menuHeight / 2;

            initialize(x, y, menuWidth, menuHeight);

            int listX = xPositionOnScreen + Padding;
            int listY = yPositionOnScreen + Padding + TitleHeight;
            int listWidth = width - Padding * 2 - ScrollButtonWidth - 12;

            for (int i = 0; i < VisibleEntries; i++)
            {
                entryBounds.Add(new Rectangle(
                    listX,
                    listY + i * (EntryHeight + EntrySpacing),
                    listWidth,
                    EntryHeight));
            }

            int scrollX = listX + listWidth + 12;

            upBounds = new Rectangle(
                scrollX,
                listY,
                ScrollButtonWidth,
                EntryHeight);

            downBounds = new Rectangle(
                scrollX,
                listY + listHeight - EntryHeight,
                ScrollButtonWidth,
                EntryHeight);

            cancelBounds = new Rectangle(
                xPositionOnScreen + Padding,
                yPositionOnScreen + height - Padding - CancelHeight,
                width - Padding * 2,
                CancelHeight);
        }

        /// <summary>
        /// 一覧項目・スクロールボタン・キャンセルボタンのクリックを処理します。
        /// </summary>
        public override void receiveLeftClick(
            int x,
            int y,
            bool playSound = true)
        {
            if (isGmcmInstalled
                && upBounds.Contains(x, y))
            {
                Scroll(-1);
                return;
            }

            if (isGmcmInstalled
                && downBounds.Contains(x, y))
            {
                Scroll(1);
                return;
            }

            if (isGmcmInstalled)
            {
                for (int i = 0; i < entryBounds.Count; i++)
            {
                int entryIndex = scrollIndex + i;
                if (entryIndex >= entries.Count)
                    break;

                if (!entryBounds[i].Contains(x, y))
                    continue;

                Game1.playSound("smallSelect");
                onSelected(entries[entryIndex].UniqueID);
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

        /// <summary>
        /// マウスホイールによるGMCM対象Mod一覧のスクロールを処理します。
        /// </summary>
        public override void receiveScrollWheelAction(int direction)
        {
            base.receiveScrollWheelAction(direction);

            if (!isGmcmInstalled)
                return;

            Scroll(direction > 0 ? -1 : 1);
        }

        /// <summary>
        /// GMCM対象Mod一覧の表示開始位置を指定量だけ移動します。
        /// </summary>
        private void Scroll(int amount)
        {
            int maxScroll = Math.Max(0, entries.Count - VisibleEntries);
            int next = Math.Clamp(scrollIndex + amount, 0, maxScroll);

            if (next == scrollIndex)
                return;

            scrollIndex = next;
            Game1.playSound("shiny4");
        }

        /// <summary>
        /// GMCM対象Modの一覧・スクロール操作・キャンセルボタンを描画します。
        /// </summary>
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

            string title = translation.Get("shortcutPanel.SelectGmcmMod.title");
            Vector2 titleSize = Game1.dialogueFont.MeasureString(title);
            b.DrawString(
                Game1.dialogueFont,
                title,
                new Vector2(
                    xPositionOnScreen + width / 2f - titleSize.X / 2f,
                    yPositionOnScreen + Padding),
                Game1.textColor);

            if (!isGmcmInstalled)
            {
                string message =
                    translation.Get(
                        "shortcutPanel.SelectGmcmMod.notInstalled");

                Vector2 messageSize =
                    Game1.smallFont.MeasureString(
                        message);

                float messageCenterY =
                    entryBounds[0].Top
                    + (
                        entryBounds[^1].Bottom
                        - entryBounds[0].Top)
                    / 2f;

                b.DrawString(
                    Game1.smallFont,
                    message,
                    new Vector2(
                        xPositionOnScreen
                        + width / 2f
                        - messageSize.X / 2f,
                        messageCenterY
                        - messageSize.Y / 2f),
                    Game1.textColor);
            }
            else
            {
                for (int i = 0; i < entryBounds.Count; i++)
                {
                    int entryIndex = scrollIndex + i;
                    if (entryIndex >= entries.Count)
                        break;

                    Rectangle bounds = entryBounds[i];
                    IManifest entry = entries[entryIndex];

                    IClickableMenu.drawTextureBox(
                        b,
                        Game1.menuTexture,
                        new Rectangle(0, 256, 60, 60),
                        bounds.X,
                        bounds.Y,
                        bounds.Width,
                        bounds.Height,
                        Color.White,
                        1f,
                        drawShadow: false);

                    Vector2 size = Game1.smallFont.MeasureString(entry.Name);
                    float maxWidth = bounds.Width - 24;
                    float scale = size.X > maxWidth && size.X > 0f
                        ? maxWidth / size.X
                        : 1f;

                    b.DrawString(
                        Game1.smallFont,
                        entry.Name,
                        new Vector2(
                            bounds.X + 12,
                            bounds.Center.Y - size.Y * scale / 2f),
                        Game1.textColor,
                        0f,
                        Vector2.Zero,
                        scale,
                        SpriteEffects.None,
                        0f);
                }
            }

            DrawScrollButton(
                b,
                upBounds,
                pointsUp: true,
                enabled: isGmcmInstalled
                    && scrollIndex > 0);

            DrawScrollButton(
                b,
                downBounds,
                pointsUp: false,
                enabled: isGmcmInstalled
                    && scrollIndex < Math.Max(
                        0,
                        entries.Count - VisibleEntries));

            DrawButton(
                b,
                cancelBounds,
                translation.Get("shortcutPanel.cancel"),
                true);

            drawMouse(b);
        }

        /// <summary>
        /// スクロール用の上下ボタンを描画します。
        /// 三角形はフォントを使わず直接描画します。
        /// </summary>
        private static void DrawScrollButton(
            SpriteBatch b,
            Rectangle bounds,
            bool pointsUp,
            bool enabled)
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

            Color arrowColor =
                enabled
                    ? Game1.textColor
                    : Game1.textColor * 0.45f;

            for (int y = 0; y < arrowHeight; y++)
            {
                float progress =
                    y / (float)(arrowHeight - 1);

                int lineWidth;

                if (pointsUp)
                {
                    lineWidth =
                        Math.Max(
                            1,
                            (int)(
                                arrowWidth
                                * progress));
                }
                else
                {
                    lineWidth =
                        Math.Max(
                            1,
                            (int)(
                                arrowWidth
                                * (1f - progress)));
                }

                int drawX =
                    centerX - lineWidth / 2;

                int drawY =
                    centerY
                    - arrowHeight / 2
                    + y;

                b.Draw(
                    Game1.staminaRect,
                    new Rectangle(
                        drawX,
                        drawY,
                        lineWidth,
                        1),
                    arrowColor);
            }
        }

        /// <summary>
        /// 選択メニュー内で使用するボタンを描画します。
        /// </summary>
        private static void DrawButton(
            SpriteBatch b,
            Rectangle bounds,
            string text,
            bool enabled)
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
            b.DrawString(
                Game1.smallFont,
                text,
                new Vector2(
                    bounds.Center.X - size.X / 2f,
                    bounds.Center.Y - size.Y / 2f),
                enabled ? Game1.textColor : Game1.textColor * 0.45f);
        }
    }
}
