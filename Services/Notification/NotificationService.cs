using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System.Text;

namespace Ts_Core.Services.Notification
{
    /// <summary>
    /// 通知の表示と管理を行うサービスです。
    /// </summary>
    public static class NotificationService
    {
        //----------------------------------------
        // 通知キュー
        //----------------------------------------

        private static readonly List<NotificationData> queue =
            new();

        //----------------------------------------
        // 現在表示中の通知
        //----------------------------------------

        private static NotificationData? current;

        /// <summary>
        /// 通知サービスを初期化します。
        /// </summary>
        public static void Initialize(IModHelper helper)
        {
            helper.Events.Display.RenderedHud += OnRenderedHud;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
        }

        /// <summary>
        /// 通知をキューへ追加します。
        /// </summary>
        public static void Show(NotificationRequest request)
        {
            NotificationData data =
                new(request);

            //----------------------------------------
            // 表示中より優先度が高いなら割り込み
            //----------------------------------------

            if (current != null &&
                data.Priority > current.Priority)
            {
                //----------------------------------------
                // 現在の通知をキューへ戻す
                //----------------------------------------

                current.Timer = current.Duration;

                queue.Add(current);

                current = data;

                return;
            }

            queue.Add(data);
        }

        /// <summary>
        /// 通知を簡単に表示します。
        /// </summary>
        public static void Show(
            string message,
            NotificationType type = NotificationType.Info,
            NotificationPriority priority = NotificationPriority.Normal,
            int duration = 120)
        {
            Show(new NotificationRequest()
            {
                Message = message,
                Type = type,
                Priority = priority,
                Duration = duration
            });
        }

        /// <summary>
        /// 現在表示中の通知を閉じます。
        /// </summary>
        public static void Hide()
        {
            if (current == null)
                return;

            current = null;
        }

        /// <summary>
        /// キューから次に表示する通知を取り出します。
        /// </summary>
        private static void ShowNextNotification()
        {
            if (current != null)
                return;

            if (queue.Count == 0)
                return;

            //----------------------------------------
            // 一番優先度の高い通知を取得
            //----------------------------------------

            current = queue
                .OrderByDescending(p => p.Priority)
                .First();

            queue.Remove(current);
        }

        /// <summary>
        /// 通知を表示中かどうか
        /// </summary>
        public static bool IsVisible
            => current != null;

        /// <summary>
        /// 通知の表示時間を更新します。
        /// </summary>
        private static void OnUpdateTicked(
            object? sender,
            UpdateTickedEventArgs e)
        {
            //----------------------------------------
            // 次の通知を表示
            //----------------------------------------

            if (current == null)
            {
                ShowNextNotification();
                return;
            }

            //----------------------------------------
            // 表示時間更新
            //----------------------------------------

            current.Timer--;

            //----------------------------------------
            // 表示終了
            //----------------------------------------

            if (current.Timer <= 0)
            {
                Hide();
                ShowNextNotification();
            }
        }

        /// <summary>
        /// 通知を描画します。
        /// </summary>
        private static void OnRenderedHud(
            object? sender,
            RenderedHudEventArgs e)
        {
            if (current == null)
                return;

            SpriteBatch b = e.SpriteBatch;

            //----------------------------------------
            // サイズ
            //----------------------------------------

            // 最低幅からテキストの折り返し幅を計算
            float wrapWidth =
                current.MinWidth
                - current.PaddingX * 2
                - current.BorderPadding * 2;

            // テキストを折り返す
            string wrappedText =
                WrapText(
                    current.Text,
                    wrapWidth / current.TextScale);

            // 折り返し後サイズ
            Vector2 textSize =
                MeasureText(
                    wrappedText,
                    current);

            // ウインドウ幅
            int width =
                Math.Max(
                    current.MinWidth,
                    (int)textSize.X
                    + current.PaddingX * 2
                    + current.BorderPadding * 2);

            // ウインドウ高さ
            int height =
                Math.Max(
                    current.MinHeight,
                    (int)textSize.Y
                    + current.PaddingY * 2
                    + current.BorderPadding * 2);

            //----------------------------------------
            // 表示位置
            //----------------------------------------

            Rectangle bounds =
                current.GetBounds(
                    width,
                    height);

            //----------------------------------------
            // 外枠
            //----------------------------------------

            DrawBorder(
                b,
                bounds,
                current);


            //----------------------------------------
            // 背景
            //----------------------------------------

            DrawBackground(
                b,
                bounds,
                current);

            //----------------------------------------
            // テキスト
            //----------------------------------------

            DrawText(
                b,
                bounds,
                current,
                wrappedText);
        }

        /// <summary>
        /// 通知の枠を描画します。
        /// </summary>
        private static void DrawBorder(
            SpriteBatch b,
            Rectangle bounds,
            NotificationData notification)
        {
            switch (notification.BorderStyle)
            {
                case NotificationBorderStyle.TextureBox:

                    DrawTextureBorder(
                        b,
                        bounds,
                        notification);

                    break;

                case NotificationBorderStyle.Solid:

                    DrawSolidBorder(
                        b,
                        bounds,
                        notification);

                    break;

                case NotificationBorderStyle.None:

                    break;
            }
        }

        /// <summary>
        /// テクスチャ枠を描画します。
        /// </summary>
        private static void DrawTextureBorder(
            SpriteBatch b,
            Rectangle bounds,
            NotificationData notification)
        {
            IClickableMenu.drawTextureBox(
                b,
                bounds.X,
                bounds.Y,
                bounds.Width,
                bounds.Height,
                notification.BorderColor);
        }

        /// <summary>
        /// 単色の枠を描画します。
        /// </summary>
        private static void DrawSolidBorder(
            SpriteBatch b,
            Rectangle bounds,
            NotificationData notification)
        {
            int thickness = notification.BorderThickness;

            //----------------------------------------
            // 上辺
            //----------------------------------------

            b.Draw(
                Game1.staminaRect,
                new Rectangle(
                    bounds.X,
                    bounds.Y,
                    bounds.Width,
                    thickness),
                notification.BorderColor);

            //----------------------------------------
            // 下辺
            //----------------------------------------

            b.Draw(
                Game1.staminaRect,
                new Rectangle(
                    bounds.X,
                    bounds.Bottom - thickness,
                    bounds.Width,
                    thickness),
                notification.BorderColor);

            //----------------------------------------
            // 左辺
            //----------------------------------------

            b.Draw(
                Game1.staminaRect,
                new Rectangle(
                    bounds.X,
                    bounds.Y,
                    thickness,
                    bounds.Height),
                notification.BorderColor);

            //----------------------------------------
            // 右辺
            //----------------------------------------

            b.Draw(
                Game1.staminaRect,
                new Rectangle(
                    bounds.Right - thickness,
                    bounds.Y,
                    thickness,
                    bounds.Height),
                notification.BorderColor);
        }

        /// <summary>
        /// 通知の背景を描画します。
        /// </summary>
        private static void DrawBackground(
            SpriteBatch b,
            Rectangle bounds,
            NotificationData notification)
        {
            // 枠の内側を背景色で塗りつぶす
            b.Draw(
                Game1.staminaRect,
                new Rectangle(
                    bounds.X + notification.BorderPadding,
                    bounds.Y + notification.BorderPadding,
                    bounds.Width - notification.BorderPadding * 2,
                    bounds.Height - notification.BorderPadding * 2),
                notification.BackgroundColor);
        }

        /// <summary>
        /// 指定した幅に収まるようにテキストを折り返します。
        /// </summary>
        private static string WrapText(
            string text,
            float maxWidth)
        {
            //----------------------------------------
            // 空文字
            //----------------------------------------

            if (string.IsNullOrWhiteSpace(text))
                return "";

            //----------------------------------------
            // 初期化
            //----------------------------------------

            StringBuilder result = new();
            StringBuilder line = new();

            int i = 0;

            //----------------------------------------
            // 1文字ずつ処理
            //----------------------------------------

            while (i < text.Length)
            {
                //----------------------------------------
                // 改行
                //----------------------------------------

                if (text[i] == '\n')
                {
                    result.AppendLine(line.ToString());

                    line.Clear();

                    i++;

                    continue;
                }

                //----------------------------------------
                // 英単語
                //----------------------------------------

                if (char.IsLetterOrDigit(text[i]) || text[i] == '_')
                {
                    int start = i;

                    // 英数字・アンダースコアを1単語として取得
                    while (i < text.Length &&
                          (char.IsLetterOrDigit(text[i]) ||
                           text[i] == '_'))
                    {
                        i++;
                    }

                    string word =
                        text.Substring(
                            start,
                            i - start);

                    string test =
                        line.ToString() + word;

                    if (Game1.dialogueFont.MeasureString(test).X <= maxWidth)
                    {
                        line.Append(word);
                    }
                    else
                    {
                        if (line.Length > 0)
                        {
                            result.AppendLine(line.ToString());

                            line.Clear();
                        }

                        line.Append(word);
                    }

                    continue;
                }

                //----------------------------------------
                // 半角スペース
                //----------------------------------------

                if (text[i] == ' ')
                {
                    string test =
                        line.ToString() + " ";

                    // 行末にはみ出すスペースは追加しない
                    if (Game1.dialogueFont.MeasureString(test).X <= maxWidth)
                    {
                        line.Append(' ');
                    }

                    i++;

                    continue;
                }

                //----------------------------------------
                // 日本語など1文字単位
                //----------------------------------------

                char c = text[i];

                string testLine =
                    line.ToString() + c;

                if (Game1.dialogueFont.MeasureString(testLine).X > maxWidth)
                {
                    result.AppendLine(line.ToString());

                    line.Clear();
                }

                line.Append(c);

                i++;
            }

            //----------------------------------------
            // 最終行を追加
            //----------------------------------------

            if (line.Length > 0)
                result.Append(line);

            return result.ToString();
        }

        /// <summary>
        /// 通知テキストを描画します。
        /// </summary>
        private static void DrawText(
            SpriteBatch b,
            Rectangle bounds,
            NotificationData notification,
            string wrappedText)
        {
            Vector2 textSize =
                MeasureText(
                    wrappedText,
                    notification);

            Vector2 textPos =
                GetTextPosition(
                    bounds,
                    textSize,
                    notification);

            DrawString(
                b,
                wrappedText,
                textPos,
                notification);
        }

        /// <summary>
        /// テキストの描画サイズを取得します。
        /// </summary>
        private static Vector2 MeasureText(
            string text,
            NotificationData notification)
        {
            return Game1.dialogueFont.MeasureString(text)
                * notification.TextScale;
        }

        /// <summary>
        /// テキストの描画位置を計算します。
        /// </summary>
        private static Vector2 GetTextPosition(
            Rectangle bounds,
            Vector2 textSize,
            NotificationData notification)
        {
            //----------------------------------------
            // テキスト配置に使用する基準座標
            //----------------------------------------

            float left =
                bounds.Left
                + notification.BorderPadding
                + notification.PaddingX;

            float right =
                bounds.Right
                - notification.BorderPadding
                - notification.PaddingX
                - textSize.X;

            float top =
                bounds.Top
                + notification.BorderPadding
                + notification.PaddingY;

            float bottom =
                bounds.Bottom
                - notification.BorderPadding
                - notification.PaddingY
                - textSize.Y;

            //----------------------------------------
            // 中央座標
            //----------------------------------------

            float centerX =
                bounds.Left
                + (bounds.Width - textSize.X) / 2f;

            float centerY =
                bounds.Top
                + (bounds.Height - textSize.Y) / 2f;

            return notification.TextAnchor switch
            {
                NotificationTextAnchor.TopLeft
                    => new Vector2(left, top),

                NotificationTextAnchor.Top
                    => new Vector2(centerX, top),

                NotificationTextAnchor.TopRight
                    => new Vector2(right, top),

                NotificationTextAnchor.Left
                    => new Vector2(left, centerY),

                NotificationTextAnchor.Center
                    => new Vector2(centerX, centerY),

                NotificationTextAnchor.Right
                    => new Vector2(right, centerY),

                NotificationTextAnchor.BottomLeft
                    => new Vector2(left, bottom),

                NotificationTextAnchor.Bottom
                    => new Vector2(centerX, bottom),

                NotificationTextAnchor.BottomRight
                    => new Vector2(right, bottom),

                _ => new Vector2(centerX, centerY)
            };
        }

        /// <summary>
        /// テキストを描画します。
        /// </summary>
        private static void DrawString(
            SpriteBatch b,
            string text,
            Vector2 position,
            NotificationData notification)
        {
            //----------------------------------------
            // 空文字なら描画しない
            //----------------------------------------

            if (string.IsNullOrWhiteSpace(text))
                return;

            //----------------------------------------
            // 影
            //----------------------------------------

            if (notification.DrawShadow)
            {
                b.DrawString(
                    Game1.dialogueFont,
                    text,
                    position + notification.ShadowOffset,
                    notification.ShadowColor,
                    0f,
                    Vector2.Zero,
                    notification.TextScale,
                    SpriteEffects.None,
                    1f);
            }

            //----------------------------------------
            // テキスト本体
            //----------------------------------------

            b.DrawString(
                Game1.dialogueFont,
                text,
                position,
                notification.TextColor,
                0f,
                Vector2.Zero,
                notification.TextScale,
                SpriteEffects.None,
                1f);
        }
    }
}