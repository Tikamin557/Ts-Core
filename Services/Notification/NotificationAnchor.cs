namespace Ts_Core.Services.Notification
{
    /// <summary>
    /// 通知を表示する画面上の位置です。
    /// </summary>
    public enum NotificationAnchor
    {
        /// <summary>
        /// 画面左上
        /// </summary>
        TopLeft,

        /// <summary>
        /// 画面上中央
        /// </summary>
        Top,

        /// <summary>
        /// 画面右上
        /// </summary>
        TopRight,

        /// <summary>
        /// 画面中央
        /// </summary>
        Center,

        /// <summary>
        /// 画面左下
        /// </summary>
        BottomLeft,


        /// <summary>
        /// 画面下中央
        /// </summary>
        Bottom,

        /// <summary>
        /// 画面右下
        /// </summary>
        BottomRight,

        /// <summary>
        /// バニラのダイアログ位置
        /// </summary>
        VanillaDialogue
    }
}