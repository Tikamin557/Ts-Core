using StardewModdingAPI.Events;
using Ts_Core.Models.PolyamorySweetRooms;

namespace Ts_Core.Services.PolyamorySweetRoomsRelated
{
    /// <summary>
    /// PSR配偶者部屋PresetをContent Patcherから登録するための
    /// Data Asset <c>TsCore/PsrRoomPresets</c> を提供します。
    /// </summary>
    internal static class PsrRoomPresetDataService
    {
        /// <summary>Content Patcher側がEditDataする対象Asset名です。</summary>
        public const string AssetName = "TsCore/PsrRoomPresets";

        //----------------------------------------
        // Data Asset提供
        //----------------------------------------

        /// <summary>
        /// SMAPIから対象Assetを要求された時、空のPreset辞書をベースとして提供します。
        /// Content Patcherはこの後に各Farmhouse ModのPreset定義を追加できます。
        /// </summary>
        internal static void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            if (!e.NameWithoutLocale.IsEquivalentTo(AssetName))
                return;

            e.LoadFrom(
                () => new Dictionary<string, PsrRoomPresetModel>(StringComparer.OrdinalIgnoreCase),
                AssetLoadPriority.Exclusive);
        }
    }
}
