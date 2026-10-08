using StardewModdingAPI.Events;
using Ts_Core.Models.ContentPatcherRelated;

namespace Ts_Core.Services.ContentPatcherRelated
{
    /// <summary>
    /// Position Picker定義をContent Patcherから登録するための
    /// Data Asset <c>TsCore/PositionPickers</c> を提供します。
    /// </summary>
    internal static class PositionPickerDataService
    {
        /// <summary>Content Patcher側がEditDataする対象Asset名です。</summary>
        public const string AssetName = "TsCore/PositionPickers";

        //----------------------------------------
        // Data Asset提供
        //----------------------------------------

        /// <summary>
        /// SMAPIから対象Assetを要求された時、空のPosition Picker辞書をベースとして提供します。
        /// Content Patcherはこの後に各Content PackのPosition Picker定義を追加できます。
        /// </summary>
        internal static void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            if (!e.NameWithoutLocale.IsEquivalentTo(AssetName))
                return;

            e.LoadFrom(
                () => new Dictionary<string, PositionPickerModel>(StringComparer.OrdinalIgnoreCase),
                AssetLoadPriority.Exclusive);
        }
    }
}
