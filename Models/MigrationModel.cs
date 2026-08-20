namespace Ts_Core.Models
{
    /// <summary>
    /// ID Migration定義1件分です。
    /// </summary>
    public sealed class MigrationModel
    {
        /// <summary>
        /// Migrationの種類です。
        /// </summary>
        public string Type { get; set; } = "";

        /// <summary>
        /// 移行元となる旧IDです。
        /// </summary>
        public string OldId { get; set; } = "";

        /// <summary>
        /// 移行先となる新IDです。
        /// </summary>
        public string NewId { get; set; } = "";
    }
}