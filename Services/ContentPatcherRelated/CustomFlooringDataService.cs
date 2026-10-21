using StardewModdingAPI.Events;
using Ts_Core.Models.ContentPatcherRelated;

namespace Ts_Core.Services.ContentPatcherRelated
{
    /// <summary>Content Patcher向けのカスタム床材定義アセットを提供します。</summary>
    internal static class CustomFlooringDataService
    {
        public const string AssetName = "TsCore/CustomFlooring";

        internal static void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            if (!e.NameWithoutLocale.IsEquivalentTo(AssetName))
                return;

            e.LoadFrom(
                () => new Dictionary<string, CustomFlooringModel>(StringComparer.OrdinalIgnoreCase),
                AssetLoadPriority.Exclusive);
        }
    }
}
