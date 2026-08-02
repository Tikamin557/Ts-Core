using Microsoft.Xna.Framework;

namespace Ts_Core.Services.Notification
{
    /// <summary>
    /// 通知テーマを定義します。
    /// </summary>
    public static class NotificationThemes
    {
        //-------------------------------------------------
        // 基本テーマ
        //-------------------------------------------------

        /// <summary>
        /// 情報通知テーマです。
        /// </summary>
        public static NotificationTheme Info
            => NotificationThemeManager.GetTheme(nameof(Info));

        /// <summary>
        /// 情報通知の既定テーマです。
        /// </summary>
        internal static readonly NotificationTheme DefaultInfo =
            new()
            {
                BackgroundColor = new Color(35, 80, 200, 185),

                BorderStyle = NotificationBorderStyle.TextureBox,
                BorderColor = Color.White,

                TextColor = Color.White,
                ShadowColor = Color.Black,
                DrawShadow = true,
                ShadowOffset = new Vector2(2, 2),
                TextScale = 1.0f,

                MinHeight = 96,
                MinWidth = 420,

                PaddingX = 40,
                PaddingY = 20,
                BorderPadding = 12,

                Anchor = NotificationAnchor.Bottom,
                OffsetY = -60
            };

        /// <summary>
        /// 成功通知テーマです。
        /// </summary>
        public static NotificationTheme Success
            => NotificationThemeManager.GetTheme(nameof(Success));

        /// <summary>
        /// 成功通知の既定テーマです。
        /// </summary>
        internal static readonly NotificationTheme DefaultSuccess =
            new()
            {
                BackgroundColor = new Color(45, 130, 70, 190),
                BorderStyle = NotificationBorderStyle.TextureBox,
                BorderColor = Color.White,

                TextColor = Color.White,
                ShadowColor = Color.Black,
                DrawShadow = true,
                ShadowOffset = new Vector2(2, 2),
                TextScale = 1.0f,

                MinHeight = 96,
                MinWidth = 420,

                PaddingX = 40,
                PaddingY = 20,
                BorderPadding = 12,

                Anchor = NotificationAnchor.Bottom,
                OffsetY = -60
            };

        /// <summary>
        /// エラー通知テーマです。
        /// </summary>
        public static NotificationTheme Error
            => NotificationThemeManager.GetTheme(nameof(Error));

        /// <summary>
        /// エラー通知の既定テーマです。
        /// </summary>
        internal static readonly NotificationTheme DefaultError =
            new()
            {
                BackgroundColor = new Color(170, 45, 45, 200),
                BorderStyle = NotificationBorderStyle.TextureBox,
                BorderColor = Color.White,

                TextColor = Color.White,
                ShadowColor = Color.Black,
                DrawShadow = true,
                ShadowOffset = new Vector2(2, 2),
                TextScale = 1.0f,

                MinHeight = 96,
                MinWidth = 420,

                PaddingX = 40,
                PaddingY = 20,
                BorderPadding = 12,

                Anchor = NotificationAnchor.Bottom,
                OffsetY = -60
            };

        //-------------------------------------------------
        // ゲーム用テーマ
        //-------------------------------------------------

        /// <summary>
        /// 警告通知テーマです。
        /// </summary>
        public static NotificationTheme Warning
            => NotificationThemeManager.GetTheme(nameof(Warning));

        /// <summary>
        /// 警告通知の既定テーマです。
        /// </summary>
        internal static readonly NotificationTheme DefaultWarning =
            new()
            {
                BackgroundColor = new Color(180, 40, 40, 210),
                BorderStyle = NotificationBorderStyle.TextureBox,
                BorderColor = Color.Gold,

                TextColor = Color.White,
                ShadowColor = Color.Black,
                DrawShadow = true,
                ShadowOffset = new Vector2(2, 2),
                TextScale = 1.0f,

                MinHeight = 96,
                MinWidth = 420,

                PaddingX = 40,
                PaddingY = 20,
                BorderPadding = 12,

                Anchor = NotificationAnchor.Top,
                OffsetY = 40
            };

        /// <summary>
        /// クエスト通知テーマです。
        /// </summary>
        public static NotificationTheme Quest
            => NotificationThemeManager.GetTheme(nameof(Quest));

        /// <summary>
        /// クエスト通知の既定テーマです。
        /// </summary>
        internal static readonly NotificationTheme DefaultQuest =
            new()
            {
                BackgroundColor = new Color(90, 70, 30, 210),
                BorderStyle = NotificationBorderStyle.TextureBox,
                BorderColor = Color.White,

                TextColor = Color.White,
                ShadowColor = Color.Black,
                DrawShadow = true,
                ShadowOffset = new Vector2(2, 2),
                TextScale = 1.0f,

                MinHeight = 96,
                MinWidth = 520,

                PaddingX = 40,
                PaddingY = 20,
                BorderPadding = 12,

                Anchor = NotificationAnchor.Top,
                OffsetY = 40
            };

        /// <summary>
        /// 実績解除通知テーマです。
        /// </summary>
        public static NotificationTheme Achievement
            => NotificationThemeManager.GetTheme(nameof(Achievement));

        /// <summary>
        /// 実績解除通知の既定テーマです。
        /// </summary>
        internal static readonly NotificationTheme DefaultAchievement =
            new()
            {
                BackgroundColor = new Color(60, 110, 40, 210),
                BorderStyle = NotificationBorderStyle.TextureBox,
                BorderColor = Color.Gold,

                TextColor = Color.White,
                ShadowColor = Color.Black,
                DrawShadow = true,
                ShadowOffset = new Vector2(2, 2),
                TextScale = 1.0f,

                MinHeight = 96,
                MinWidth = 520,

                PaddingX = 40,
                PaddingY = 20,
                BorderPadding = 12,

                Anchor = NotificationAnchor.TopRight,
                OffsetX = -40,
                OffsetY = 40
            };

        //-------------------------------------------------
        // 特殊テーマ
        //-------------------------------------------------

        /// <summary>
        /// ボス通知テーマです。
        /// </summary>
        public static NotificationTheme Boss
            => NotificationThemeManager.GetTheme(nameof(Boss));

        /// <summary>
        /// ボス通知の既定テーマです。
        /// </summary>
        internal static readonly NotificationTheme DefaultBoss =
            new()
            {
                BackgroundColor = new Color(90, 20, 20, 220),
                BorderStyle = NotificationBorderStyle.TextureBox,
                BorderColor = Color.Gold,

                TextColor = Color.Gold,
                ShadowColor = Color.Black,
                DrawShadow = true,
                ShadowOffset = new Vector2(2, 2),
                TextScale = 1.3f,

                MinHeight = 110,
                MinWidth = 700,

                PaddingX = 50,
                PaddingY = 24,
                BorderPadding = 12,

                Anchor = NotificationAnchor.Top,
                OffsetY = 60
            };

        /// <summary>
        /// レトロウィンドウ通知テーマです。
        /// </summary>
        public static NotificationTheme RetroWindow
            => NotificationThemeManager.GetTheme(nameof(RetroWindow));

        /// <summary>
        /// レトロウィンドウ通知の既定テーマです。
        /// </summary>
        internal static readonly NotificationTheme DefaultRetroWindow =
            new()
            {
                BackgroundColor = new Color(35, 80, 200, 180),

                BorderStyle = NotificationBorderStyle.Solid,
                BorderThickness = 5,
                BorderColor = Color.White,

                TextColor = Color.White,
                ShadowColor = Color.Black,
                DrawShadow = true,
                ShadowOffset = new Vector2(2, 2),
                TextScale = 1.0f,

                MinHeight = 96,
                MinWidth = 420,

                PaddingX = 20,
                PaddingY = 20,
                BorderPadding = 2,

                TextAnchor = NotificationTextAnchor.Center,

                Anchor = NotificationAnchor.VanillaDialogue
            };
    }
}