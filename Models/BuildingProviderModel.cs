namespace Ts_Core.Models
{
    /// <summary>
    /// Buildingに追加するTsCore拡張機能の定義です。
    /// </summary>
    public sealed class BuildingProviderModel
    {
        //----------------------------------------
        // Building
        //----------------------------------------

        /// <summary>
        /// 対象となる建物タイプです。
        /// </summary>
        public string BuildingType { get; set; } = "";

        /// <summary>
        /// このBuilding Provider全体が
        /// 有効かどうかです。
        /// </summary>
        public bool BuildingsEnabled { get; set; } = true;

        /// <summary>
        /// Building Provider全体の有効・無効を制御する
        /// CustomFieldsのキーです。
        ///
        /// 未指定の場合は
        /// BuildingsEnabledのみで判定します。
        /// </summary>
        public string? BuildingsEnabledField { get; set; }

        /// <summary>
        /// この建物をバレーのメイン農場でのみ
        /// 建築可能にするかどうかです。
        /// </summary>
        public bool ValleyFarmOnly { get; set; }

        //----------------------------------------
        // Lights
        //----------------------------------------

        /// <summary>
        /// Lights全体が有効かどうかです。
        /// </summary>
        public bool LightsEnabled { get; set; } = true;

        /// <summary>
        /// Light全体の有効・無効を制御する
        /// CustomFieldsのキーです。
        ///
        /// 未指定の場合は
        /// LightsEnabledのみで判定します。
        /// </summary>
        public string? LightsEnabledField { get; set; }

        /// <summary>
        /// 建物に追加するLight一覧です。
        /// </summary>
        public List<BuildingLightModel> Lights { get; set; } = new();

        //----------------------------------------
        // DrawLayers
        //----------------------------------------

        /// <summary>
        /// DrawLayers全体が有効かどうかです。
        /// </summary>
        public bool DrawLayersEnabled { get; set; } = true;

        /// <summary>
        /// DrawLayer全体の有効・無効を制御する
        /// CustomFieldsのキーです。
        ///
        /// 未指定の場合は
        /// DrawLayersEnabledのみで判定します。
        /// </summary>
        public string? DrawLayersEnabledField { get; set; }

        /// <summary>
        /// 建物に追加するDrawLayer一覧です。
        /// </summary>
        public List<BuildingDrawLayerModel> DrawLayers { get; set; } = new();
    }
}