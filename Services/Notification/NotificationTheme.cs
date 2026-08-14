using Microsoft.Xna.Framework;

namespace Ts_Core.Services.Notification
{
    /// <summary>
    /// 通知テーマの表示設定です。
    /// </summary>
    public sealed class NotificationTheme
    {
        /// <summary>
        /// 継承元テーマ名
        /// </summary>
        public string? Base { get; set; }

        //----------------------------------------
        // 背景
        //----------------------------------------

        public Color? BackgroundColor { get; set; }

        public Color? BorderColor { get; set; }

        //----------------------------------------
        // テキスト
        //----------------------------------------

        public Color? TextColor { get; set; }

        public Color? ShadowColor { get; set; }

        public bool? DrawShadow { get; set; }

        public Vector2? ShadowOffset { get; set; }

        public float? TextScale { get; set; }

        //----------------------------------------
        // レイアウト
        //----------------------------------------

        public int? MinHeight { get; set; }

        public int? MinWidth { get; set; }

        public int? PaddingX { get; set; }

        public int? PaddingY { get; set; }

        public int? BorderPadding { get; set; }

        public NotificationTextAnchor? TextAnchor { get; set; }

        public NotificationBorderStyle? BorderStyle { get; set; }

        public int? BorderThickness { get; set; }

        public NotificationAnchor? Anchor { get; set; }

        public int? OffsetX { get; set; }

        public int? OffsetY { get; set; }

        /// <summary>
        /// テーマのコピーを作成します。
        /// </summary>
        public NotificationTheme Clone()
        {
            return new NotificationTheme()
            {
                Base = Base,

                //----------------------------------------
                // 背景
                //----------------------------------------

                BackgroundColor = BackgroundColor,

                BorderColor = BorderColor,
                BorderStyle = BorderStyle,
                BorderThickness = BorderThickness,

                //----------------------------------------
                // テキスト
                //----------------------------------------

                TextColor = TextColor,
                ShadowColor = ShadowColor,
                DrawShadow = DrawShadow,
                ShadowOffset = ShadowOffset,
                TextScale = TextScale,

                //----------------------------------------
                // レイアウト
                //----------------------------------------

                MinHeight = MinHeight,
                MinWidth = MinWidth,

                PaddingX = PaddingX,
                PaddingY = PaddingY,
                BorderPadding = BorderPadding,

                Anchor = Anchor,
                OffsetX = OffsetX,
                OffsetY = OffsetY,

                TextAnchor = TextAnchor
            };
        }
    }
}