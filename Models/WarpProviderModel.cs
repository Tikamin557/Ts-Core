namespace Ts_Core.Models
{
    /// <summary>
    /// Warp Provider定義です。
    /// JSONから読み込まれるワープ情報を保持します。
    /// </summary>
    public class WarpProviderModel
    {
        //----------------------------------------
        // 共通
        //----------------------------------------

        /// <summary>
        /// Warp Providerの一意なIDです。
        /// </summary>
        public string Id { get; set; } = "";

        /// <summary>
        /// Warp Providerの種類です。
        /// （Warp / MapEntry / Building）
        /// </summary>
        public string Type { get; set; } = "";

        //----------------------------------------
        // Warp用
        //----------------------------------------

        /// <summary>
        /// ワープ元のマップ名です。
        /// </summary>
        public string? Source { get; set; }

        /// <summary>
        /// ワープ先のマップ名です。
        /// </summary>
        public string? Target { get; set; }

        //----------------------------------------
        // Building用
        //----------------------------------------

        /// <summary>
        /// 対象となる建物タイプです。
        /// </summary>
        public string? BuildingType { get; set; }

        //----------------------------------------
        // MapEntry用
        //----------------------------------------

        /// <summary>
        /// MapEntryで検索対象となるマップ名です。
        /// </summary>
        public string? Map { get; set; }

        //----------------------------------------
        // MapEntry / Building用
        //----------------------------------------

        /// <summary>
        /// 基準座標から加算するXオフセットです。
        /// </summary>
        public int OffsetX { get; set; }

        /// <summary>
        /// 基準座標から加算するYオフセットです。
        /// </summary>
        public int OffsetY { get; set; }

        //----------------------------------------
        // Fallback
        //----------------------------------------

        /// <summary>
        /// Warp先を解決できなかった場合に使用するWarp Provider IDです。
        /// </summary>
        public string? Fallback { get; set; }
    }
}