namespace Ts_Core.Models
{
    /// <summary>
    /// Buildingに追加するLightの定義です。
    /// </summary>
    public sealed class BuildingLightModel
    {
        //----------------------------------------
        // Basic
        //----------------------------------------

        /// <summary>
        /// Light IDです。
        /// </summary>
        public string Id { get; set; } = "";

        /// <summary>
        /// このLightが有効かどうかです。
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// このLightの有効・無効を制御する
        /// CustomFieldsのキーです。
        ///
        /// 未指定の場合は
        /// Enabledのみで判定します。
        /// </summary>
        public string? EnabledField { get; set; }

        //----------------------------------------
        // Position
        //----------------------------------------

        /// <summary>
        /// 建物左上を基準とした
        /// X方向のオフセットです。
        /// </summary>
        public int OffsetX { get; set; }

        /// <summary>
        /// 建物左上を基準とした
        /// Y方向のオフセットです。
        /// </summary>
        public int OffsetY { get; set; }

        //----------------------------------------
        // Light
        //----------------------------------------

        /// <summary>
        /// Lightの半径です。
        /// </summary>
        public float Radius { get; set; } = 4f;

        /// <summary>
        /// Lightの色です。
        /// </summary>
        public string Color { get; set; } = "0,0,0";
    }
}