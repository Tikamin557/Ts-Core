namespace Ts_Core.Models
{
    /// <summary>
    /// Buildingに追加するTsCore拡張機能の定義です。
    /// </summary>
    public sealed class BuildingProviderModel
    {
        /// <summary>
        /// Provider IDです。
        /// </summary>
        public string Id { get; set; } = "";

        /// <summary>
        /// 対象となる建物タイプです。
        /// </summary>
        public string BuildingType { get; set; } = "";

        /// <summary>
        /// この建物をバレーのメイン農場でのみ建築可能にするかどうかです。
        /// </summary>
        public bool ValleyFarmOnly { get; set; }

        /// <summary>
        /// Building Provider全体の有効・無効を制御するCustomFieldsのキーです。
        /// 未指定の場合は常に有効です。
        /// </summary>
        public string? BuildingsEnabledField { get; set; }

        /// <summary>
        /// Lightの有効・無効を制御するCustomFieldsのキーです。
        /// 未指定の場合は常に有効です。
        /// </summary>
        public string? LightsEnabledField { get; set; }

        /// <summary>
        /// DrawLayerの有効・無効を制御するCustomFieldsのキーです。
        /// 未指定の場合は常に有効です。
        /// </summary>
        public string? DrawLayersEnabledField { get; set; }

        /// <summary>
        /// 建物に追加するライト一覧です。
        /// </summary>
        public List<BuildingLightModel> Lights { get; set; } = new();

        /// <summary>
        /// 建物に追加する条件付きDrawLayer一覧です。
        /// </summary>
        public List<BuildingDrawLayerModel> DrawLayers { get; set; } = new();
    }
}