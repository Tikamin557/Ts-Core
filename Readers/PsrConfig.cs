namespace Ts_Core.Readers
{
    /// <summary>
    /// 重婚Mod の設定ファイルを読み込むためのクラスです。
    /// </summary>
    public class PsrConfig
    {
        /// <summary>
        /// 配偶者部屋の並び順。
        /// JSONから自動的に読み込まれます。
        /// </summary>
        public string SpouseRoomOrder { get; set; } = "";
    }
}