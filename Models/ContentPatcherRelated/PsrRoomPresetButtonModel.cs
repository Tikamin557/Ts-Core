namespace Ts_Core.Models.ContentPatcherRelated
{
    /// <summary>
    /// Content Patcher Content PackのGMCMへ追加する、
    /// PSR Room Presets設定画面を開くボタンの定義です。
    /// </summary>
    public sealed class PsrRoomPresetButtonModel
    {
        /// <summary>対象Content PackのUniqueIDです。</summary>
        public string ContentPackId { get; set; } = "";

        /// <summary>GMCMに表示する項目名です。未指定時はTsCore標準の翻訳を使用します。</summary>
        public string GMCM_Name { get; set; } = "";

        /// <summary>GMCMの項目説明です。未指定時はTsCore標準の翻訳を使用します。</summary>
        public string GMCM_Description { get; set; } = "";

        /// <summary>GMCMボタンに表示する文字列です。未指定時はTsCore標準の翻訳を使用します。</summary>
        public string GMCM_Button { get; set; } = "";

        /// <summary>セーブ未読込時にボタン上へ表示する説明です。未指定時はTsCore標準の翻訳を使用します。</summary>
        public string GMCM_WorldRequired { get; set; } = "";

        /// <summary>Polyamory Sweet Rooms未導入時にボタン上へ表示する説明です。未指定時はTsCore標準の翻訳を使用します。</summary>
        public string GMCM_PsrRequired { get; set; } = "";

        /// <summary>利用可能なPresetが無い時にボタン上へ表示する説明です。未指定時はTsCore標準の翻訳を使用します。</summary>
        public string GMCM_NoPreset { get; set; } = "";

        /// <summary>GMCMでボタンをこのConfig項目の直前へ表示します。</summary>
        public string BeforeField { get; set; } = "";

        /// <summary>GMCMでボタンをこのConfig項目の直後へ表示します。</summary>
        public string AfterField { get; set; } = "";
    }
}
