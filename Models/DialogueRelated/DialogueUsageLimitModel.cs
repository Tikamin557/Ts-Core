namespace Ts_Core.Models.DialogueRelated
{
    /// <summary>
    /// TsCore Dialogueの使用回数制限を定義します。
    /// </summary>
    public sealed class DialogueUsageLimitModel
    {
        /// <summary>
        /// 使用回数を記録する単位です。
        /// 現在はBuildingのみ対応しています。
        /// </summary>
        public string Scope { get; set; } = "";

        /// <summary>
        /// 使用回数を制限する期間です。
        /// 現在はDayのみ対応しています。
        /// </summary>
        public string Period { get; set; } = "";

        /// <summary>
        /// 使用済み状態を保存するmodDataキーです。
        /// ScopeがBuildingの場合は対象BuildingのmodDataへ保存します。
        /// </summary>
        public string Key { get; set; } = "";
    }
}
