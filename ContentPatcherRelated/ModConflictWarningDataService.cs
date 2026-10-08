using StardewModdingAPI.Events;
using Ts_Core.Models.ContentPatcherRelated;

namespace Ts_Core.Services.ContentPatcherRelated
{
    /// <summary>CPから登録できるMod競合警告のData Assetを提供します。</summary>
    internal static class ModConflictWarningDataService
    {
        public const string AssetName = "TsCore/ModConflictWarnings";

        //----------------------------------------
        // Data Asset提供
        //----------------------------------------

        internal static void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            if (!e.NameWithoutLocale.IsEquivalentTo(AssetName))
                return;

            e.LoadFrom(
                () => new Dictionary<string, ModConflictWarningModel>(StringComparer.OrdinalIgnoreCase),
                AssetLoadPriority.Exclusive);
        }
    }
}
