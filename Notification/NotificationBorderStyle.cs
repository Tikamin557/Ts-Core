namespace Ts_Core.Services.Notification
{
    /// <summary>
    /// 通知ウィンドウの枠の描画方法です。
    /// </summary>
    public enum NotificationBorderStyle
    {
        /// <summary>
        /// drawTextureBoxを使用（バニラ風）
        /// </summary>
        TextureBox,

        /// <summary>
        /// 四角形を組み合わせて描画
        /// </summary>
        Solid,

        /// <summary>
        /// 枠を描画しません
        /// </summary>
        None
    }
}