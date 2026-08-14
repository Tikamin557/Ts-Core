namespace Ts_Core.Models
{
    /// <summary>
    /// 建物に追加するLight Providerの定義です。
    /// </summary>
    public sealed class BuildingLightProviderModel
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
        /// Data/Buildings の CustomFields から
        /// Lightの有効・無効を取得するキーです。
        /// 未指定の場合は常に有効です。
        /// </summary>
        public string? LightsEnabledField { get; set; }

        /// <summary>
        /// 建物に追加するライト一覧です。
        /// </summary>
        public List<BuildingLightModel> Lights { get; set; } = new();

        /// <summary>
        /// 建物に追加する条件付きDrawLayer一覧です。
        /// </summary>
        public List<BuildingDrawLayerModel> DrawLayers { get; set; } = new();
    }

    /// <summary>
    /// 建物に追加するライト1件分の定義です。
    /// </summary>
    public sealed class BuildingLightModel
    {
        /// <summary>
        /// Light IDです。
        /// </summary>
        public string Id { get; set; } = "";

        /// <summary>
        /// 建物左上からのX方向オフセットです。
        /// 負の値も使用できます。
        /// </summary>
        public int OffsetX { get; set; }

        /// <summary>
        /// 建物左上からのY方向オフセットです。
        /// 負の値も使用できます。
        /// </summary>
        public int OffsetY { get; set; }

        /// <summary>
        /// ライトの半径です。
        /// </summary>
        public float Radius { get; set; } = 4f;

        /// <summary>
        /// ライトカラーです。
        /// 例: "255,220,160"
        /// </summary>
        public string Color { get; set; } = "0,0,0";
    }
}