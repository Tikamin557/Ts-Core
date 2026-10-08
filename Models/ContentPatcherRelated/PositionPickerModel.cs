namespace Ts_Core.Models.ContentPatcherRelated
{
    /// <summary>
    /// Content Patcher Content Pack用のMap Position Picker定義です。
    /// </summary>
    public sealed class PositionPickerModel
    {
        /// <summary>対象Content PackのUniqueIDです。</summary>
        public string ContentPackId { get; set; } = "";

        /// <summary>GMCMに表示する項目名です。未指定時はTsCore標準の翻訳を使用します。</summary>
        public string GMCM_Name { get; set; } = "";

        /// <summary>GMCMの項目説明です。未指定時はTsCore標準の翻訳を使用します。</summary>
        public string GMCM_Description { get; set; } = "";

        /// <summary>GMCMボタンに表示する文字列です。未指定時はTsCore標準の翻訳を使用します。</summary>
        public string GMCM_Button { get; set; } = "";

        /// <summary>ゲーム未開始時にボタンへ表示するHover Textです。未指定時はTsCore標準の翻訳を使用します。</summary>
        public string GMCM_WorldRequired { get; set; } = "";

        /// <summary>対象Location外でボタンへ表示するHover Textです。未指定時はTsCore標準の翻訳を使用します。</summary>
        public string GMCM_LocationRequired { get; set; } = "";

        /// <summary>X座標を書き込むConfig項目名です。</summary>
        public string XField { get; set; } = "";

        /// <summary>Y座標を書き込むConfig項目名です。</summary>
        public string YField { get; set; } = "";

        /// <summary>GMCMでPosition PickerボタンをこのConfig項目の直前へ表示します。</summary>
        public string BeforeField { get; set; } = "";

        /// <summary>GMCMでPosition PickerボタンをこのConfig項目の直後へ表示します。</summary>
        public string AfterField { get; set; } = "";

        /// <summary>Position Pickerを使用するLocation名です。</summary>
        public string Location { get; set; } = "";

        /// <summary>配置プレビューに使用するMap Asset名です。</summary>
        public string PreviewMap { get; set; } = "";

        /// <summary>配置プレビュー中に左上へ表示する任意の注意書きです。</summary>
        public string PreviewNote { get; set; } = "";

        /// <summary>保存座標から見たPreviewMap原点のXオフセットです。</summary>
        public int AnchorX { get; set; }

        /// <summary>保存座標から見たPreviewMap原点のYオフセットです。</summary>
        public int AnchorY { get; set; }
    }
}
