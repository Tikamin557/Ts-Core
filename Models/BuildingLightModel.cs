namespace Ts_Core.Models
{
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
