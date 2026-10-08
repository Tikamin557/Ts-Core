using StardewModdingAPI.Events;
using Ts_Core.Models.ContentPatcherRelated;

namespace Ts_Core.Services.ContentPatcherRelated
{
    /// <summary>
    /// PSR Room Presetsボタン定義をContent Patcherから登録するための
    /// Data Asset <c>TsCore/PsrRoomPresetButtons</c> を提供します。
    /// </summary>
    internal static class PsrRoomPresetButtonDataService
    {
        /// <summary>Content Patcher側がEditDataする対象Asset名です。</summary>
        public const string AssetName = "TsCore/PsrRoomPresetButtons";

        /// <summary>
        /// SMAPIから対象Assetを要求された時、空の定義辞書をベースとして提供します。
        /// </summary>
        internal static void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            if (!e.NameWithoutLocale.IsEquivalentTo(AssetName))
                return;

            e.LoadFrom(
                () => new Dictionary<string, PsrRoomPresetButtonModel>(StringComparer.OrdinalIgnoreCase),
                AssetLoadPriority.Exclusive);
        }
    }
}
