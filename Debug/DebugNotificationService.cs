using StardewModdingAPI;
using Ts_Core.Services.Notification;

namespace Ts_Core.Debug
{
    /// <summary>
    /// Notification関連のデバッグ実行処理を管理します。
    /// </summary>
    internal static class DebugNotificationService
    {
        //----------------------------------------
        // Notification表示
        //----------------------------------------

        /// <summary>
        /// Notificationを表示します。
        /// </summary>
        internal static void ShowNotification(
            string[] args)
        {
            string notificationName =
                "Info";

            if (args.Length > 0)
            {
                notificationName =
                    args[0];
            }

            if (NotificationTypeExtensions.TryParse(
                    notificationName,
                    out NotificationType type))
            {
                NotificationService.Show(
                    $"Notification : {notificationName}",
                    type,
                    NotificationPriority.High,
                    180);
            }
            else
            {
                NotificationRequest.Theme(
                    notificationName,
                    $"Notification : {notificationName}",
                    180)
                    .Show();
            }
        }

        //----------------------------------------
        // TriggerAction経由のNotification表示
        //----------------------------------------

        /// <summary>
        /// TriggerAction経由でNotificationを表示します。
        /// </summary>
        internal static void ShowTriggerNotification(
            string[] args,
            IMonitor monitor)
        {
            if (args.Length < 4)
            {
                monitor.Log(
                    "Usage: tscore_debug_notification_trigger <Type> <Priority> <Duration> <Message...>",
                    LogLevel.Info);

                return;
            }

            NotificationAction.Run(
                args,
                default,
                out _);
        }
    }
}