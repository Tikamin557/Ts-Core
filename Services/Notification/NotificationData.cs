using Microsoft.Xna.Framework;
using StardewValley;

namespace Ts_Core.Services.Notification
{
    /// <summary>
    /// 通知1件分の表示データです
    /// </summary>
    public sealed class NotificationData
    {
        /// <summary>
        /// 空の通知データを生成します
        /// </summary>
        public NotificationData()
        {
        }

        /// <summary>
        /// NotificationRequestから表示用データを生成します
        /// </summary>
        public NotificationData(NotificationRequest request)
        {
            NotificationTheme theme;

            if (!string.IsNullOrWhiteSpace(request.ThemeName))
            {
                theme = NotificationThemeManager.GetTheme(request.ThemeName);
            }
            else
            {
                theme = request.Type.GetTheme();
            }

            Text = request.Message;

            Duration = request.Duration;

            Timer = request.Duration;

            Priority = request.Priority;

            BackgroundColor = theme.BackgroundColor
                ?? Color.Transparent;

            BorderColor = theme.BorderColor
                ?? Color.White;

            BorderStyle = theme.BorderStyle
                ?? NotificationBorderStyle.TextureBox;

            BorderThickness = theme.BorderThickness
                ?? 4;

            MinHeight = theme.MinHeight
                ?? 96;

            MinWidth = theme.MinWidth
                ?? 420;

            PaddingX = theme.PaddingX
                ?? 40;

            PaddingY = theme.PaddingY
                ?? 20;

            BorderPadding = theme.BorderPadding
                ?? 12;

            Anchor = theme.Anchor
                ?? NotificationAnchor.Bottom;

            OffsetX = theme.OffsetX
                ?? 0;

            OffsetY = theme.OffsetY
                ?? 0;

            TextColor = request.TextColor
                ?? theme.TextColor
                ?? Color.White;

            ShadowColor = request.ShadowColor
                ?? theme.ShadowColor
                ?? Color.Black;

            DrawShadow = request.DrawShadow
                ?? theme.DrawShadow
                ?? true;

            ShadowOffset = request.ShadowOffset
                ?? theme.ShadowOffset
                ?? new Vector2(2, 2);

            TextScale = request.TextScale
                ?? theme.TextScale
                ?? 1f;

            TextAnchor =
                theme.TextAnchor
                ?? NotificationTextAnchor.Center;
        }

        /// <summary>
        /// 表示する文字列
        /// </summary>
        public string Text { get; set; } = "";

        //----------------------------------------
        // 背景
        //----------------------------------------

        public Color BackgroundColor { get; set; }

        public Color BorderColor { get; set; }

        public NotificationBorderStyle BorderStyle { get; set; }

        public int BorderThickness { get; set; }

        //----------------------------------------
        // テキスト
        //----------------------------------------

        public Color TextColor { get; set; }

        public Color ShadowColor { get; set; }

        public bool DrawShadow { get; set; }

        public Vector2 ShadowOffset { get; set; }

        public float TextScale { get; set; }

        //----------------------------------------
        // レイアウト
        //----------------------------------------

        public int MinHeight { get; set; }

        public int MinWidth { get; set; }

        public int PaddingX { get; set; }

        public int PaddingY { get; set; }

        public int BorderPadding { get; set; }

        public NotificationTextAnchor TextAnchor { get; set; }

        public NotificationAnchor Anchor { get; set; }

        public int OffsetX { get; set; }

        public int OffsetY { get; set; }

        /// <summary>
        /// 表示時間 (UpdateTick)
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// 残り表示時間 (UpdateTick)
        /// </summary>
        public int Timer { get; set; }

        /// <summary>
        /// 通知の表示優先度
        /// </summary>
        public NotificationPriority Priority { get; set; }
            = NotificationPriority.Normal;

        /// <summary>
        /// バニラ会話ウィンドウのおおよそのY座標
        /// </summary>
        private const int VanillaDialogueY = 240;

        /// <summary>
        /// 通知ウィンドウの表示位置を計算します。
        /// </summary>
        public Rectangle GetBounds(
          int width,
          int height)
        {
            int x = 0;
            int y = 0;

            switch (Anchor)
            {
                case NotificationAnchor.Top:

                    x = (Game1.uiViewport.Width - width) / 2;
                    y = 0;
                    break;

                case NotificationAnchor.TopLeft:

                    x = 0;
                    y = 0;
                    break;

                case NotificationAnchor.TopRight:

                    x = Game1.uiViewport.Width - width;
                    y = 0;
                    break;

                case NotificationAnchor.Center:

                    x = (Game1.uiViewport.Width - width) / 2;
                    y = (Game1.uiViewport.Height - height) / 2;
                    break;

                case NotificationAnchor.BottomLeft:

                    x = 0;
                    y = Game1.uiViewport.Height - height;
                    break;

                case NotificationAnchor.BottomRight:

                    x = Game1.uiViewport.Width - width;
                    y = Game1.uiViewport.Height - height;
                    break;

                case NotificationAnchor.VanillaDialogue:

                    x = (Game1.uiViewport.Width - width) / 2;
                    y = Game1.uiViewport.Height - VanillaDialogueY;
                    break;

                default: // Bottom

                    x = (Game1.uiViewport.Width - width) / 2;
                    y = Game1.uiViewport.Height - height;
                    break;
            }

            x += OffsetX;
            y += OffsetY;

            return new Rectangle(
                x,
                y,
                width,
                height);
        }
    }
}