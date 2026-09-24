using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace Ts_Core.Services.ShortcutPanelRelated
{
    /// <summary>
    /// Android上で現在のSMAPIログをゲーム内表示する読み取り専用ビューアーです。
    /// </summary>
    internal sealed class ShortcutPanelSmapiConsoleMenu : IClickableMenu
    {
        //----------------------------------------
        // 基本情報
        //----------------------------------------

        private readonly ITranslationHelper translation;
        //----------------------------------------
        // ログ本文とスクロール状態
        //----------------------------------------

        private readonly List<string> displayLines = new();
        //----------------------------------------
        // ログ領域・各操作ボタンの表示範囲
        //----------------------------------------

        private Rectangle contentBounds;
        private Rectangle closeBounds;
        private Rectangle upBounds;
        private Rectangle downBounds;
        private Rectangle bottomBounds;
        private Rectangle textSizeMinusBounds;
        private Rectangle textSizePlusBounds;
        private int firstVisibleLine;
        private int visibleLineCount;
        private int lastRefreshTick = -9999;
        private bool dragging;
        private int dragLastY;
        private float dragRemainder;
        private string errorText = string.Empty;
        private float textScale = 0.75f;
        private int layoutWidth;
        private int layoutHeight;
        private readonly bool openedFromTitleMenu;

        private const int Margin = 24;
        private const int HeaderHeight = 64;
        private const int FooterHeight = 68;
        //----------------------------------------
        // 文字倍率・更新間隔などの設定値
        //----------------------------------------

        private const float MinTextScale = 0.5f;
        private const float MaxTextScale = 1.5f;
        private const float TextScaleStep = 0.1f;
        private const int RefreshTicks = 60;
        private const int ScrollLines = 5;

        /// <summary>
        /// Android用のゲーム内SMAPIコンソール画面を初期化し、現在のログを読み込みます。
        /// </summary>
        internal ShortcutPanelSmapiConsoleMenu(ITranslationHelper translation)
        {
            this.translation = translation;
            // Androidのタイトル画面では Game1.uiViewport が実際のUI描画先より
            // 大きい値になることがあります。タイトル画面では、Stardew Valley本体が
            // UIを実際に描き込んでいる Game1.uiScreen のサイズを直接基準にします。
            bool isTitleMenu = Game1.activeClickableMenu is TitleMenu;
            openedFromTitleMenu = isTitleMenu;

            RenderTarget2D? uiScreen = Game1.game1.uiScreen;
            if (isTitleMenu
                && uiScreen != null
                && !uiScreen.IsDisposed
                && !uiScreen.IsContentLost)
            {
                layoutWidth = uiScreen.Width;
                layoutHeight = uiScreen.Height;
            }
            else if (isTitleMenu)
            {
                // uiScreenを取得できない場合だけ従来の計算へフォールバックします。
                float uiScale = Math.Max(0.01f, Game1.options.uiScale);
                Viewport graphicsViewport = Game1.graphics.GraphicsDevice.Viewport;

                layoutWidth = (int)MathF.Ceiling(graphicsViewport.Width / uiScale);
                layoutHeight = (int)MathF.Ceiling(graphicsViewport.Height / uiScale);
            }
            else
            {
                layoutWidth = Game1.uiViewport.Width;
                layoutHeight = Game1.uiViewport.Height;
            }

            int width = Math.Max(320, layoutWidth - Margin * 2);
            int height = Math.Max(240, layoutHeight - Margin * 2);
            initialize(Margin, Margin, width, height);
            RebuildBounds();
            RefreshLog(forceBottom: true);
        }

        /// <summary>
        /// 現在の画面サイズを基準に、ログ領域や各操作ボタンの位置と大きさを再計算します。
        /// </summary>
        private void RebuildBounds()
        {
            closeBounds = new Rectangle(xPositionOnScreen + width - 76, yPositionOnScreen + 8, 60, 50);
            contentBounds = new Rectangle(
                xPositionOnScreen + 18,
                yPositionOnScreen + HeaderHeight,
                width - 36,
                height - HeaderHeight - FooterHeight);

            int buttonWidth = 82;
            int buttonHeight = 52;
            int buttonGap = 8;
            int bottomButtonGap = 28;
            int buttonY = yPositionOnScreen + height - FooterHeight + 8;

            // ▲ / ▼ の2ボタンだけを画面中央に配置します。
            // ▼▼ はその右側へ間隔を空けて配置し、スクロールセットとは別操作だと分かりやすくします。
            int scrollPairWidth = buttonWidth * 2 + buttonGap;
            int scrollPairX = xPositionOnScreen + (width - scrollPairWidth) / 2;
            upBounds = new Rectangle(scrollPairX, buttonY, buttonWidth, buttonHeight);
            downBounds = new Rectangle(upBounds.Right + buttonGap, buttonY, buttonWidth, buttonHeight);
            bottomBounds = new Rectangle(downBounds.Right + bottomButtonGap, buttonY, buttonWidth, buttonHeight);

            int textSizeButtonWidth = 60;
            textSizePlusBounds = new Rectangle(
                xPositionOnScreen + width - 18 - textSizeButtonWidth,
                buttonY,
                textSizeButtonWidth,
                buttonHeight);
            textSizeMinusBounds = new Rectangle(
                textSizePlusBounds.X - 8 - textSizeButtonWidth,
                buttonY,
                textSizeButtonWidth,
                buttonHeight);

            visibleLineCount = Math.Max(1, contentBounds.Height / GetLineHeight());
        }

        /// <summary>
        /// 現在のSMAPIログファイルを再読み込みし、画面表示用の行データを更新します。
        /// </summary>
        private void RefreshLog(bool forceBottom)
        {
            bool wasAtBottom = IsAtBottom();

            if (!ShortcutPanelSmapiLogService.TryReadAll(out string text, out string error))
            {
                errorText = error;
                displayLines.Clear();
                displayLines.Add(translation.Get("shortcutPanel.SmapiConsole.readError").ToString());
                displayLines.Add(error);
                firstVisibleLine = 0;
                return;
            }

            errorText = string.Empty;
            displayLines.Clear();

            float maxWidth = Math.Max(100, (contentBounds.Width - 20) / textScale);
            string normalized = text.Replace("\r\n", "\n").Replace('\r', '\n');
            foreach (string rawLine in normalized.Split('\n'))
                AddWrappedLine(rawLine, maxWidth);

            if (displayLines.Count == 0)
                displayLines.Add(string.Empty);

            if (forceBottom || wasAtBottom)
                ScrollToBottom();
            else
                ClampScroll();
        }

        /// <summary>
        /// 長いログ1行を表示幅に合わせて折り返し、表示用の行一覧へ追加します。
        /// </summary>
        private void AddWrappedLine(string line, float maxWidth)
        {
            if (string.IsNullOrEmpty(line))
            {
                displayLines.Add(string.Empty);
                return;
            }

            string remaining = line;
            while (remaining.Length > 0)
            {
                if (Game1.smallFont.MeasureString(remaining).X <= maxWidth)
                {
                    displayLines.Add(remaining);
                    break;
                }

                int low = 1;
                int high = remaining.Length;
                while (low < high)
                {
                    int mid = (low + high + 1) / 2;
                    if (Game1.smallFont.MeasureString(remaining.Substring(0, mid)).X <= maxWidth)
                        low = mid;
                    else
                        high = mid - 1;
                }

                int take = Math.Max(1, low);
                displayLines.Add(remaining.Substring(0, take));
                remaining = remaining.Substring(take);
            }
        }

        /// <summary>
        /// 現在の文字倍率を反映したログ1行分の高さを取得します。
        /// </summary>
        private int GetLineHeight()
        {
            return Math.Max(12, (int)MathF.Ceiling(Game1.smallFont.LineSpacing * textScale) + 2);
        }

        /// <summary>
        /// ログ文字の表示倍率を変更し、折り返しやスクロール位置を再計算します。
        /// </summary>
        private void ChangeTextScale(float delta)
        {
            bool wasAtBottom = IsAtBottom();
            textScale = Math.Clamp(
                MathF.Round((textScale + delta) * 10f) / 10f,
                MinTextScale,
                MaxTextScale);
            visibleLineCount = Math.Max(1, contentBounds.Height / GetLineHeight());
            RefreshLog(forceBottom: wasAtBottom);
            Game1.playSound("smallSelect");
        }

        private int MaxFirstLine => Math.Max(0, displayLines.Count - visibleLineCount);
        /// <summary>
        /// 現在の表示位置がログの最下部付近かを判定します。
        /// </summary>
        private bool IsAtBottom() => firstVisibleLine >= MaxFirstLine - 1;
        /// <summary>
        /// ログ表示位置を一番下へ移動します。
        /// </summary>
        private void ScrollToBottom() => firstVisibleLine = MaxFirstLine;
        /// <summary>
        /// ログのスクロール位置が有効範囲から外れないよう補正します。
        /// </summary>
        private void ClampScroll() => firstVisibleLine = Math.Clamp(firstVisibleLine, 0, MaxFirstLine);

        /// <summary>
        /// 指定された行数だけログを上下へスクロールします。
        /// </summary>
        private void ScrollBy(int lines)
        {
            firstVisibleLine += lines;
            ClampScroll();
        }

        /// <summary>
        /// 各ボタンのクリックやログ領域のドラッグ開始を処理します。
        /// </summary>
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (closeBounds.Contains(x, y))
            {
                Game1.playSound("bigDeSelect");
                exitThisMenu();
                return;
            }

            if (upBounds.Contains(x, y))
            {
                ScrollBy(-ScrollLines);
                return;
            }

            if (downBounds.Contains(x, y))
            {
                ScrollBy(ScrollLines);
                return;
            }

            if (bottomBounds.Contains(x, y))
            {
                ScrollToBottom();
                Game1.playSound("smallSelect");
                return;
            }

            if (textSizeMinusBounds.Contains(x, y))
            {
                ChangeTextScale(-TextScaleStep);
                return;
            }

            if (textSizePlusBounds.Contains(x, y))
            {
                ChangeTextScale(TextScaleStep);
                return;
            }

            if (contentBounds.Contains(x, y))
            {
                dragging = true;
                dragLastY = y;
                dragRemainder = 0f;
            }
        }

        /// <summary>
        /// ログ領域を押したままドラッグしている間のスクロールを処理します。
        /// </summary>
        public override void leftClickHeld(int x, int y)
        {
            if (!dragging)
                return;

            int delta = dragLastY - y;
            dragLastY = y;
            dragRemainder += delta;

            int lineHeight = GetLineHeight();
            int lines = (int)(dragRemainder / lineHeight);
            if (lines != 0)
            {
                ScrollBy(lines);
                dragRemainder -= lines * lineHeight;
            }
        }

        /// <summary>
        /// ドラッグ操作の終了を処理します。
        /// </summary>
        public override void releaseLeftClick(int x, int y)
        {
            dragging = false;
            dragRemainder = 0f;
        }

        /// <summary>
        /// マウスホイールによるログの上下スクロールを処理します。
        /// </summary>
        public override void receiveScrollWheelAction(int direction)
        {
            ScrollBy(direction > 0 ? -ScrollLines : ScrollLines);
        }

        /// <summary>
        /// 一定間隔でログを再読み込みし、新しいログをゲーム内表示へ反映します。
        /// </summary>
        public override void update(GameTime time)
        {
            base.update(time);

            if (Game1.ticks - lastRefreshTick >= RefreshTicks)
            {
                lastRefreshTick = Game1.ticks;
                RefreshLog(forceBottom: false);
            }
        }

        /// <summary>
        /// SMAPIコンソールの背景・ログ本文・各操作ボタンを描画します。
        /// </summary>
        public override void draw(SpriteBatch b)
        {
            // Androidのタイトル画面から開いた直後は、コンストラクタ時点と実際の描画時点で
            // UI用RenderTarget/Viewportのサイズが切り替わります。
            // 実際に描画する瞬間のViewportを基準に、メニュー全体をここで確定します。
            EnsureTitleDrawLayout();

            // 実際のUI描画先と同じ論理UI座標系で暗幕を描画します。
            b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, layoutWidth, layoutHeight), Color.Black * 0.7f);

            // Stardew Valley標準のメニューボックスで外枠を描画します。
            IClickableMenu.drawTextureBox(
                b, Game1.menuTexture, new Rectangle(0, 256, 60, 60),
                xPositionOnScreen, yPositionOnScreen, width, height,
                Color.White, 1f, drawShadow: true);

            string title = translation.Get("shortcutPanel.SmapiConsole.title");
            b.DrawString(Game1.dialogueFont, title,
                new Vector2(xPositionOnScreen + 20, yPositionOnScreen + 12), Game1.textColor);

            IClickableMenu.drawTextureBox(
                b, Game1.menuTexture, new Rectangle(0, 256, 60, 60),
                closeBounds.X, closeBounds.Y, closeBounds.Width, closeBounds.Height,
                Color.White, 0.8f, drawShadow: false);
            Vector2 closeSize = Game1.smallFont.MeasureString("X");
            const float closeScale = 1.2f;
            b.DrawString(Game1.smallFont, "X",
                new Vector2(
                    closeBounds.Center.X - closeSize.X * closeScale / 2f,
                    closeBounds.Center.Y - closeSize.Y * closeScale / 2f),
                Game1.textColor,
                0f,
                Vector2.Zero,
                closeScale,
                SpriteEffects.None,
                0f);

            b.Draw(Game1.staminaRect, contentBounds, Color.Black * 0.82f);

            int end = Math.Min(displayLines.Count, firstVisibleLine + visibleLineCount);
            int lineHeight = GetLineHeight();
            float y = contentBounds.Y + 4;
            for (int i = firstVisibleLine; i < end; i++)
            {
                b.DrawString(Game1.smallFont, displayLines[i], new Vector2(contentBounds.X + 8, y), Color.White, 0f, Vector2.Zero, textScale, SpriteEffects.None, 0f);
                y += lineHeight;
            }

            DrawScrollButton(b, upBounds, pointsUp: true);
            DrawScrollButton(b, downBounds, pointsUp: false);
            DrawDoubleDownButton(b, bottomBounds);
            DrawTextButton(b, textSizeMinusBounds, "-");
            DrawTextButton(b, textSizePlusBounds, "+");

            string status = errorText.Length > 0
                ? translation.Get("shortcutPanel.SmapiConsole.statusError").ToString()
                : $"{firstVisibleLine + 1}-{Math.Min(displayLines.Count, firstVisibleLine + visibleLineCount)} / {displayLines.Count}";
            b.DrawString(Game1.smallFont, status,
                new Vector2(xPositionOnScreen + 18, yPositionOnScreen + height - FooterHeight + 15), Game1.textColor, 0f, Vector2.Zero, 0.75f, SpriteEffects.None, 0f);

            drawMouse(b);
        }


        /// <summary>
        /// タイトル画面などで実際の描画先Viewportサイズが変化した場合に、画面内へ収まるよう再配置します。
        /// </summary>
        private void EnsureTitleDrawLayout()
        {
            if (!openedFromTitleMenu)
                return;

            Viewport viewport = Game1.graphics.GraphicsDevice.Viewport;
            int targetWidth = viewport.Width;
            int targetHeight = viewport.Height;

            if (targetWidth <= 0 || targetHeight <= 0)
                return;

            int targetMenuWidth = Math.Max(320, targetWidth - Margin * 2);
            int targetMenuHeight = Math.Max(240, targetHeight - Margin * 2);

            bool changed =
                layoutWidth != targetWidth
                || layoutHeight != targetHeight
                || xPositionOnScreen != Margin
                || yPositionOnScreen != Margin
                || width != targetMenuWidth
                || height != targetMenuHeight;

            if (!changed)
                return;

            bool wasAtBottom = IsAtBottom();

            layoutWidth = targetWidth;
            layoutHeight = targetHeight;
            // gameWindowSizeChanged等によって付与されたuiViewport.X/Y分の位置ずれも、
            // 実際のRenderTarget上の座標へここで戻します。
            initialize(Margin, Margin, targetMenuWidth, targetMenuHeight);
            RebuildBounds();
            RefreshLog(forceBottom: wasAtBottom);

        }

        /// <summary>
        /// ログを上下へ移動する三角形のスクロールボタンを描画します。
        /// </summary>
        private static void DrawScrollButton(
            SpriteBatch b,
            Rectangle bounds,
            bool pointsUp)
        {
            IClickableMenu.drawTextureBox(
                b, Game1.menuTexture, new Rectangle(0, 256, 60, 60),
                bounds.X, bounds.Y, bounds.Width, bounds.Height,
                Color.White, 0.8f, drawShadow: false);

            const int arrowWidth = 28;
            const int arrowHeight = 17;
            int centerX = bounds.Center.X;
            int centerY = bounds.Center.Y;

            for (int y = 0; y < arrowHeight; y++)
            {
                float progress = y / (float)(arrowHeight - 1);
                int lineWidth = pointsUp
                    ? Math.Max(1, (int)(arrowWidth * progress))
                    : Math.Max(1, (int)(arrowWidth * (1f - progress)));

                b.Draw(
                    Game1.staminaRect,
                    new Rectangle(
                        centerX - lineWidth / 2,
                        centerY - arrowHeight / 2 + y,
                        lineWidth,
                        1),
                    Game1.textColor);
            }
        }

        /// <summary>
        /// ログの最下部へ一気に移動する二重下向きボタンを描画します。
        /// </summary>
        private static void DrawDoubleDownButton(
            SpriteBatch b,
            Rectangle bounds)
        {
            IClickableMenu.drawTextureBox(
                b, Game1.menuTexture, new Rectangle(0, 256, 60, 60),
                bounds.X, bounds.Y, bounds.Width, bounds.Height,
                Color.White, 0.8f, drawShadow: false);

            const int arrowWidth = 25;
            const int arrowHeight = 13;
            const int arrowGap = 3;
            int centerX = bounds.Center.X;
            int totalHeight = arrowHeight * 2 + arrowGap;
            int startY = bounds.Center.Y - totalHeight / 2;

            for (int arrow = 0; arrow < 2; arrow++)
            {
                int arrowTop = startY + arrow * (arrowHeight + arrowGap);
                for (int y = 0; y < arrowHeight; y++)
                {
                    float progress = y / (float)(arrowHeight - 1);
                    int lineWidth = Math.Max(1, (int)(arrowWidth * (1f - progress)));
                    b.Draw(
                        Game1.staminaRect,
                        new Rectangle(
                            centerX - lineWidth / 2,
                            arrowTop + y,
                            lineWidth,
                            1),
                        Game1.textColor);
                }
            }
        }

        /// <summary>
        /// 文字を中央に表示する汎用ボタンを描画します。
        /// </summary>
        private static void DrawTextButton(
            SpriteBatch b,
            Rectangle bounds,
            string text)
        {
            IClickableMenu.drawTextureBox(
                b, Game1.menuTexture, new Rectangle(0, 256, 60, 60),
                bounds.X, bounds.Y, bounds.Width, bounds.Height,
                Color.White, 0.8f, drawShadow: false);

            Vector2 size = Game1.smallFont.MeasureString(text);
            float maxWidth = bounds.Width - 16;
            float scale = size.X > maxWidth && size.X > 0f
                ? maxWidth / size.X
                : (text == "+" || text == "-" ? 1.25f : 1f);

            b.DrawString(
                Game1.smallFont,
                text,
                new Vector2(
                    bounds.Center.X - size.X * scale / 2f,
                    bounds.Center.Y - size.Y * scale / 2f),
                Game1.textColor,
                0f,
                Vector2.Zero,
                scale,
                SpriteEffects.None,
                0f);
        }
    }
}
