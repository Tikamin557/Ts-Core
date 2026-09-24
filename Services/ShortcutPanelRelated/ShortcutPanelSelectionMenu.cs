using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace Ts_Core.Services.ShortcutPanelRelated
{
    /// <summary>
    /// Shortcut Panelへ登録する
    /// Mod機能を選択するメニューです。
    /// </summary>
    internal sealed class ShortcutPanelSelectionMenu
        : IClickableMenu
    {
        //----------------------------------------
        // Translation
        //----------------------------------------

        private readonly ITranslationHelper
            translation;

        //----------------------------------------
        // Entry
        //----------------------------------------

        private readonly List<ShortcutPanelEntry>
            entries = new();

        //----------------------------------------
        // Bounds
        //----------------------------------------

        private readonly List<Rectangle>
            entryBounds = new();

        private Rectangle cancelBounds;

        //----------------------------------------
        // Callback
        //----------------------------------------

        private readonly Action<string>
            onSelected;

        //----------------------------------------
        // Layout
        //----------------------------------------

        /// <summary>
        /// メニューの最小幅。
        /// </summary>
        private const int MinMenuWidth = 520;

        /// <summary>
        /// メニューと画面端の最低余白。
        /// </summary>
        private const int ScreenMargin = 32;

        /// <summary>
        /// Entry内のアイコンより右側にある
        /// テキスト開始位置。
        /// </summary>
        private const int EntryTextOffset = 76;

        /// <summary>
        /// Entry内のテキスト右側の余白。
        /// </summary>
        private const int EntryTextRightPadding = 16;

        private const int EntryHeight = 72;

        private const int EntrySpacing = 8;

        private const int Padding = 24;

        private const int TitleHeight = 60;

        private const int CancelHeight = 64;

        //----------------------------------------
        // Constructor
        //----------------------------------------

        /// <summary>
        /// ショートカットへ登録するT's Coreまたは外部Modの機能を選択するメニューを初期化します。
        /// </summary>
        internal ShortcutPanelSelectionMenu(
            ITranslationHelper translation,
            Action<string> onSelected)
        {
            this.translation =
                translation;

            this.onSelected =
                onSelected;

            //----------------------------------------
            // 登録済みShortcutを取得
            //----------------------------------------

            foreach (ShortcutPanelEntry entry
                     in ShortcutPanelRegistry.GetAll())
            {
                entries.Add(
                    entry);
            }

            //----------------------------------------
            // 必要なメニュー幅を計算
            //----------------------------------------

            float longestTextWidth =
                0f;

            foreach (ShortcutPanelEntry entry
                     in entries)
            {
                float textWidth =
                    Game1.smallFont.MeasureString(
                        entry.DisplayName).X;

                if (textWidth
                    > longestTextWidth)
                {
                    longestTextWidth =
                        textWidth;
                }
            }

            int requiredMenuWidth =
                Padding * 2
                + EntryTextOffset
                + (int)Math.Ceiling(
                    longestTextWidth)
                + EntryTextRightPadding;

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
            // メニューサイズ
            //----------------------------------------

            int entriesHeight =
                entries.Count
                * EntryHeight;

            if (entries.Count > 1)
            {
                entriesHeight +=
                    (entries.Count - 1)
                    * EntrySpacing;
            }

            int menuHeight =
                Padding
                + TitleHeight
                + entriesHeight
                + Padding
                + CancelHeight
                + Padding;

            //----------------------------------------
            // 画面中央
            //----------------------------------------

            int x =
                Game1.uiViewport.Width / 2
                - menuWidth / 2;

            int y =
                Game1.uiViewport.Height / 2
                - menuHeight / 2;

            initialize(
                x,
                y,
                menuWidth,
                menuHeight);

            //----------------------------------------
            // Entry Bounds
            //----------------------------------------

            int entryX =
                xPositionOnScreen
                + Padding;

            int entryY =
                yPositionOnScreen
                + Padding
                + TitleHeight;

            int entryWidth =
                width
                - Padding * 2;

            for (int i = 0;
                 i < entries.Count;
                 i++)
            {
                entryBounds.Add(
                    new Rectangle(
                        entryX,
                        entryY,
                        entryWidth,
                        EntryHeight));

                entryY +=
                    EntryHeight
                    + EntrySpacing;
            }

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
        // Click
        //----------------------------------------

        /// <summary>
        /// Mod機能一覧の項目やキャンセル操作のクリックを処理します。
        /// </summary>
        public override void receiveLeftClick(
            int x,
            int y,
            bool playSound = true)
        {
            //----------------------------------------
            // Shortcut
            //----------------------------------------

            for (int i = 0;
                 i < entryBounds.Count;
                 i++)
            {
                if (!entryBounds[i].Contains(
                    x,
                    y))
                {
                    continue;
                }

                ShortcutPanelEntry entry =
                    entries[i];

                if (!entry.IsAvailable())
                {
                    if (entry.PlayUnavailableSound)
                    {
                        Game1.playSound(
                            "cancel");
                    }

                    entry.ExecuteUnavailableAction();
                    return;
                }

                Game1.playSound(
                    "smallSelect");

                //----------------------------------------
                // 選択結果を返す
                //----------------------------------------

                onSelected(
                    entry.Id);

                //----------------------------------------
                // メニューを閉じる
                //----------------------------------------

                exitThisMenu();

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
        /// 登録可能なMod機能一覧とキャンセル操作を描画します。
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

            string title =
                translation.Get(
                    "shortcutPanel.SelectModFunction.title");

            Vector2 titleSize =
                Game1.dialogueFont.MeasureString(
                    title);

            Vector2 titlePosition =
                new Vector2(
                    xPositionOnScreen
                    + width / 2f
                    - titleSize.X / 2f,
                    yPositionOnScreen
                    + Padding);

            b.DrawString(
                Game1.dialogueFont,
                title,
                titlePosition,
                Game1.textColor);

            //----------------------------------------
            // Shortcut Entries
            //----------------------------------------

            for (int i = 0;
                 i < entries.Count;
                 i++)
            {
                Rectangle bounds =
                    entryBounds[i];

                ShortcutPanelEntry entry =
                    entries[i];

                float entryAlpha =
                    entry.IsAvailable()
                        ? 1f
                        : 0.45f;

                //----------------------------------------
                // Button
                //----------------------------------------

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
                    Color.White * entryAlpha,
                    1f,
                    drawShadow: false);

                //----------------------------------------
                // Icon
                //----------------------------------------

                Texture2D? texture =
                    entry.GetIconTexture();

                const int iconSize = 48;

                if (texture != null)
                {
                    Rectangle iconBounds =
                        new Rectangle(
                            bounds.X + 12,
                            bounds.Center.Y
                            - iconSize / 2,
                            iconSize,
                            iconSize);

                    b.Draw(
                        texture,
                        iconBounds,
                        entry.IconSourceRect,
                        Color.White * entryAlpha);
                }

                //----------------------------------------
                // Display Name
                //----------------------------------------

                Vector2 textSize =
                    Game1.smallFont.MeasureString(
                        entry.DisplayName);

                float maxTextWidth =
                    bounds.Width
                    - EntryTextOffset
                    - EntryTextRightPadding;

                float textScale =
                    1f;

                //----------------------------------------
                // 最大幅でも収まらない場合のみ
                // テキストを縮小
                //----------------------------------------

                if (textSize.X > maxTextWidth
                    && textSize.X > 0)
                {
                    textScale =
                        maxTextWidth
                        / textSize.X;
                }

                Vector2 textPosition =
                    new Vector2(
                        bounds.X
                        + EntryTextOffset,
                        bounds.Center.Y
                        - textSize.Y
                        * textScale / 2f);

                b.DrawString(
                    Game1.smallFont,
                    entry.DisplayName,
                    textPosition,
                    Game1.textColor * entryAlpha,
                    0f,
                    Vector2.Zero,
                    textScale,
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

            //----------------------------------------
            // 使用不可項目のHover
            //----------------------------------------

            Point mousePosition =
                Game1.getMousePosition();

            for (int i = 0;
                 i < entries.Count;
                 i++)
            {
                ShortcutPanelEntry entry =
                    entries[i];

                if (entry.IsAvailable()
                    || !entryBounds[i].Contains(mousePosition))
                {
                    continue;
                }

                string? hoverText =
                    entry.GetUnavailableHoverText();

                if (!string.IsNullOrWhiteSpace(hoverText))
                {
                    IClickableMenu.drawHoverText(
                        b,
                        hoverText,
                        Game1.smallFont);
                }

                break;
            }

            //----------------------------------------
            // Mouse
            //----------------------------------------

            drawMouse(
                b);
        }
    }
}
