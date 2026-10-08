namespace Ts_Core.Services.Notification
{
    /// <summary>
    /// 通知の表示優先度です。
    /// 値が大きいほど先に表示されます。
    /// </summary>
    public enum NotificationPriority
    {
        /// <summary>
        /// 低優先度
        /// </summary>
        Low = 0,

        /// <summary>
        /// 通常優先度
        /// </summary>
        Normal = 100,

        /// <summary>
        /// 高優先度
        /// </summary>
        High = 200,

        /// <summary>
        /// 最優先
        /// </summary>
        Critical = 300
    }
}