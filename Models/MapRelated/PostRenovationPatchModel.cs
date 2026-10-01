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
}
