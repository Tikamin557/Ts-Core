using Microsoft.Xna.Framework;

namespace Ts_Core.Services.Notification
{
    /// <summary>
    /// 通知の表示要求を表すデータです。
    /// </summary>
    public sealed class NotificationRequest
    {
        /// <summary>
        /// 表示するメッセージ
        /// </summary>
        public string Message { get; set; } = "";

        /// <summary>
        /// 通知の種類
        /// </summary>
        public NotificationType Type { get; set; }
            = NotificationType.Info;

        /// <summary>
        /// 使用する通知テーマ名
        /// null の場合は Type に対応するテーマを使用します。
        /// </summary>
        public string? ThemeName { get; set; }

        /// <summary>
        /// 通知の表示優先度
        /// </summary>
        public NotificationPriority Priority { get; set; }
            = NotificationPriority.Normal;

        /// <summary>
        /// 表示時間 (UpdateTick)
        /// </summary>
        public int Duration { get; set; } = 120;

        /// <summary>
        /// 文字倍率
        /// null の場合はテーマ設定を使用します。
        /// </summary>
        public float? TextScale { get; set; }

        /// <summary>
        /// 文字色
        /// null の場合はテーマ設定を使用します。
        /// </summary>
        public Color? TextColor { get; set; }

        /// <summary>
        /// 影色
        /// null の場合はテーマ設定を使用します。
        /// </summary>
        public Color? ShadowColor { get; set; }

        /// <summary>
        /// 影を描画するか
        /// null の場合はテーマ設定を使用します。
        /// </summary>
        public bool? DrawShadow { get; set; }

        /// <summary>
        /// 影の表示位置
        /// null の場合はテーマ設定を使用します。
        /// </summary>
        public Vector2? ShadowOffset { get; set; }

        /// <summary>
        /// この通知を表示します。
        /// </summary>
        public void Show()
        {
            NotificationService.Show(this);
        }

        /// <summary>
        /// 情報通知を作成します。
        /// </summary>
        public static NotificationRequest Info(
            string message,
            int duration = 120)
        {
            return new NotificationRequest()
            {
                Message = message,
                Type = NotificationType.Info,
                ThemeName = "Info",
                Duration = duration
            };
        }

        /// <summary>
        /// 成功通知を作成します。
        /// </summary>
        public static NotificationRequest Success(
            string message,
            int duration = 120)
        {
            return new NotificationRequest()
            {
                Message = message,
                Type = NotificationType.Success,
                ThemeName = "Success",
                Duration = duration
            };
        }

        /// <summary>
        /// エラー通知を作成します。
        /// </summary>
        public static NotificationRequest Error(
            string message,
            int duration = 180)
        {
            return new NotificationRequest()
            {
                Message = message,
                Type = NotificationType.Error,
                ThemeName = "Error",
                Priority = NotificationPriority.High,
                Duration = duration
            };
        }

        /// <summary>
        /// 警告通知を作成します。
        /// </summary>
        public static NotificationRequest Warning(
            string message,
            int duration = 180)
        {
            return new NotificationRequest()
            {
                Message = message,
                Type = NotificationType.Warning,
                ThemeName = "Warning",
                Priority = NotificationPriority.High,
                Duration = duration
            };
        }

        /// <summary>
        /// クエスト通知を作成します。
        /// </summary>
        public static NotificationRequest Quest(
            string message,
            int duration = 180)
        {
            return new NotificationRequest()
            {
                Message = message,
                Type = NotificationType.Quest,
                ThemeName = "Quest",
                Duration = duration
            };
        }

        /// <summary>
        /// 実績解除通知を作成します。
        /// </summary>
        public static NotificationRequest Achievement(
            string message,
            int duration = 240)
        {
            return new NotificationRequest()
            {
                Message = message,
                Type = NotificationType.Achievement,
                ThemeName = "Achievement",
                Duration = duration
            };
        }

        /// <summary>
        /// ボス通知を作成します。
        /// </summary>
        public static NotificationRequest Boss(
            string message,
            int duration = 300)
        {
            return new NotificationRequest()
            {
                Message = message,
                Type = NotificationType.Boss,
                ThemeName = "Boss",
                Priority = NotificationPriority.Critical,
                Duration = duration
            };
        }

        /// <summary>
        /// レトロウインドウ通知を作成します。
        /// </summary>
        public static NotificationRequest RetroWindow(
            string message,
            int duration = 240)
        {
            return new NotificationRequest()
            {
                Message = message,
                Type = NotificationType.RetroWindow,
                ThemeName = "RetroWindow",
                Priority = NotificationPriority.High,
                Duration = duration
            };
        }

        /// <summary>
        /// 指定したテーマで通知を作成します。
        /// </summary>
        public static NotificationRequest Theme(
            string themeName,
            string message,
            int duration = 120)
        {
            return new NotificationRequest()
            {
                Message = message,
                ThemeName = themeName,
                Duration = duration
            };
        }
    }
}