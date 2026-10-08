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

        private Rectangle upBounds;

        private Rectangle downBounds;

        private Rectangle cancelBounds;

        private int scrollIndex;

        private int visibleEntries;

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

        private const int ScrollButtonWidth = 64;

        private const int ScrollButtonGap = 12;

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
                // CJB Warpの個別Entryは保存済みSlotの復元・実行用です。
                // Mod機能一覧にはSelectorだけを表示し、
                // 宛先は専用の選択メニューで選びます。
                if (entry.Id.StartsWith(
                    "TsCore/CjbWarp/",
                    StringComparison.Ordinal))
                {
                    continue;
                }

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

            int fixedHeight =
                Padding
                + TitleHeight
                + Padding
                + CancelHeight
                + Padding;

            int maxListHeight =
                Math.Max(
                    EntryHeight,
                    Game1.uiViewport.Height
                    - ScreenMargin * 2
                    - fixedHeight);

            visibleEntries =
                Math.Max(
                    1,
                    Math.Min(
                        entries.Count,
                        (maxListHeight + EntrySpacing)
                        / (EntryHeight + EntrySpacing)));

            int entriesHeight =
                visibleEntries
                * EntryHeight;

            if (visibleEntries > 1)
            {
                entriesHeight +=
                    (visibleEntries - 1)
                    * EntrySpacing;
            }

            int menuHeight =
                fixedHeight
                + entriesHeight;

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

            bool needsScroll =
                entries.Count > visibleEntries;

            int entryWidth =
                width
                - Padding * 2
                - (needsScroll
                    ? ScrollButtonWidth + ScrollButtonGap
                    : 0);

            for (int i = 0;
                 i < visibleEntries;
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
            // Scroll
            //----------------------------------------

            if (needsScroll)
            {
                int scrollX =
                    entryX
                    + entryWidth
                    + ScrollButtonGap;

                upBounds =
                    new Rectangle(
                        scrollX,
                        entryBounds[0].Y,
                        ScrollButtonWidth,
                        EntryHeight);

                downBounds =
                    new Rectangle(
                        scrollX,
                        entryBounds[^1].Bottom
                        - EntryHeight,
                        ScrollButtonWidth,
                        EntryHeight);
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
            // Scroll
            //----------------------------------------

            if (entries.Count > visibleEntries
                && upBounds.Contains(x, y))
            {
                Scroll(-1);
                return;
            }

            if (entries.Count > visibleEntries
                && downBounds.Contains(x, y))
            {
                Scroll(1);
                return;
            }

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

                int entryIndex =
                    scrollIndex + i;

                if (entryIndex >= entries.Count)
                {
                    break;
                }

                ShortcutPanelEntry entry =
                    entries[entryIndex];

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
                // 通常の選択ではこのメニューを閉じます。
                // Callback側で次の選択メニューへ切り替えた場合は、
                // 新しく開いたメニューを閉じないようにします。
                //----------------------------------------

                if (Game1.activeClickableMenu == this)
                {
                    exitThisMenu();
                }

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
        // Scroll
        //----------------------------------------

        /// <summary>
        /// マウスホイールによるMod機能一覧のスクロールを処理します。
        /// </summary>
        public override void receiveScrollWheelAction(
            int direction)
        {
            base.receiveScrollWheelAction(
                direction);

            Scroll(
                direction > 0
                    ? -1
                    : 1);
        }

        /// <summary>
        /// Mod機能一覧の表示開始位置を指定量だけ移動します。
        /// </summary>
        private void Scroll(
            int amount)
        {
            int maxScroll =
                Math.Max(
                    0,
                    entries.Count
                    - visibleEntries);

            int next =
                Math.Clamp(
                    scrollIndex + amount,
                    0,
                    maxScroll);

            if (next == scrollIndex)
            {
                return;
            }

            scrollIndex = next;

            Game1.playSound(
                "shiny4");
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
                 i < entryBounds.Count;
                 i++)
            {
                int entryIndex =
                    scrollIndex + i;

                if (entryIndex >= entries.Count)
                {
                    break;
                }

                Rectangle bounds =
                    entryBounds[i];

                ShortcutPanelEntry entry =
                    entries[entryIndex];

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
            // Scroll
            //----------------------------------------

            if (entries.Count > visibleEntries)
            {
                DrawScrollButton(
                    b,
                    upBounds,
                    pointsUp: true,
                    enabled: scrollIndex > 0);

                DrawScrollButton(
                    b,
                    downBounds,
                    pointsUp: false,
                    enabled: scrollIndex < entries.Count - visibleEntries);
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
                 i < entryBounds.Count;
                 i++)
            {
                int entryIndex =
                    scrollIndex + i;

                if (entryIndex >= entries.Count)
                {
                    break;
                }

                ShortcutPanelEntry entry =
                    entries[entryIndex];

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
        /// <summary>
        /// Mod機能一覧の上下スクロールボタンを描画します。
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
                enabled
                    ? Color.White
                    : Color.White * 0.45f,
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

            for (int y = 0;
                 y < arrowHeight;
                 y++)
            {
                float progress =
                    y / (float)(arrowHeight - 1);

                int lineWidth =
                    pointsUp
                        ? Math.Max(1, (int)(arrowWidth * progress))
                        : Math.Max(1, (int)(arrowWidth * (1f - progress)));

                b.Draw(
                    Game1.staminaRect,
                    new Rectangle(
                        centerX - lineWidth / 2,
                        centerY - arrowHeight / 2 + y,
                        lineWidth,
                        1),
                    arrowColor);
            }
        }

    }
}
