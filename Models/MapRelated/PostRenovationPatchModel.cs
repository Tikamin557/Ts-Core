using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace Ts_Core.Models.MapRelated
{
    /// <summary>
    /// FarmHouseのRenovation適用後に実行する
    /// Map Patch定義です。
    /// </summary>
    public sealed class PostRenovationPatchModel
    {
        /// <summary>
        /// Patch対象のGame Content Asset名です。
        /// </summary>
        public string Target { get; set; } = "";

        /// <summary>
        /// Patch元として使用するGame Content Asset名です。
        /// </summary>
        public string FromFile { get; set; } = "";

        /// <summary>
        /// Renovation適用後のMap Tileへ適用する変更です。
        /// Position / Layer / SetPropertiesを使用できます。
        /// </summary>
        public List<PostRenovationMapTileModel> MapTiles { get; set; } = new();

        /// <summary>
        /// Patch元Mapから使用する範囲です。
        /// 未指定の場合はMap全体を使用します。
        /// </summary>
        public Rectangle? FromArea { get; set; }

        /// <summary>
        /// Patch先の範囲です。
        /// 未指定の場合はFromAreaと同じサイズで
        /// 左上から適用します。
        /// </summary>
        public Rectangle? ToArea { get; set; }

        /// <summary>
        /// Map Patchの適用方法です。
        /// </summary>
        public PatchMapMode PatchMode { get; set; } =
            PatchMapMode.Overlay;
    }

    /// <summary>
    /// Post Renovation Patchで変更する
    /// Map Tileの定義です。
    /// </summary>
    public sealed class PostRenovationMapTileModel
    {
        /// <summary>
        /// 変更するTileの座標です。
        /// </summary>
        public Point Position { get; set; }

        /// <summary>
        /// 変更するLayer名です。
        /// </summary>
        public string Layer { get; set; } = "";

        /// <summary>
        /// Tileへ設定するPropertyです。
        /// </summary>
        public Dictionary<string, string> SetProperties { get; set; } = new();
    }
}
