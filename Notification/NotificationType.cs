namespace Ts_Core.Services.Notification
{
    /// <summary>
    /// 標準通知の種類です。
    /// </summary>
    public enum NotificationType
    {
        //-------------------------------------------------
        // 基本
        //-------------------------------------------------

        /// <summary>
        /// 情報通知
        /// </summary>
        Info,

        /// <summary>
        /// 成功通知
        /// </summary>
        Success,

        /// <summary>
        /// エラー通知
        /// </summary>
        Error,

        //-------------------------------------------------
        // ゲーム
        //-------------------------------------------------

        /// <summary>
        /// 警告通知
        /// </summary>
        Warning,

        /// <summary>
        /// クエスト通知
        /// </summary>
        Quest,

        /// <summary>
        /// 実績解除通知
        /// </summary>
        Achievement,

        //-------------------------------------------------
        // 特殊
        //-------------------------------------------------

        /// <summary>
        /// ボス通知
        /// </summary>
        Boss,

        /// <summary>
        /// レトロウインドウ通知
        /// </summary>
        RetroWindow
    }

    /// <summary>
    /// NotificationType の拡張メソッドです。
    /// </summary>
    internal static class NotificationTypeExtensions
    {
        /// <summary>
        /// 通知タイプに対応するテーマを取得します。
        /// </summary>
        public static NotificationTheme GetTheme(
            this NotificationType type)
        {
            return NotificationThemeManager.GetTheme(type.ToString());
        }

        /// <summary>
        /// 文字列から通知タイプへ変換します。
        /// </summary>
        public static bool TryParse(
            string text,
            out NotificationType type)
        {
            return Enum.TryParse(
                text,
                true,
                out type);
        }

        /// <summary>
        /// 通知タイプに対応する NotificationRequest を作成します。
        /// </summary>
        public static NotificationRequest CreateRequest(
            this NotificationType type,
            string message,
            int duration)
        {
            return type switch
            {
                NotificationType.Info =>
                    NotificationRequest.Info(message, duration),

                NotificationType.Success =>
                    NotificationRequest.Success(message, duration),

                NotificationType.Error =>
                    NotificationRequest.Error(message, duration),

                NotificationType.Warning =>
                    NotificationRequest.Warning(message, duration),

                NotificationType.Quest =>
                    NotificationRequest.Quest(message, duration),

                NotificationType.Achievement =>
                    NotificationRequest.Achievement(message, duration),

                NotificationType.Boss =>
                    NotificationRequest.Boss(message, duration),

                NotificationType.RetroWindow =>
                    NotificationRequest.RetroWindow(message, duration),

                _ => throw new ArgumentOutOfRangeException(nameof(type))
            };
        }
    }
}