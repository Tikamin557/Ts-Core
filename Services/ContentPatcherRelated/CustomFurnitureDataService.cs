using StardewModdingAPI.Events;
using Ts_Core.Models.ContentPatcherRelated;

namespace Ts_Core.Services.ContentPatcherRelated
{
    /// <summary>Content Patcher向けのカスタム家具定義アセットを提供します。</summary>
    internal static class CustomFurnitureDataService
    {
        public const string AssetName = "TsCore/CustomFurniture";

        internal static void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo(AssetName))
                e.LoadFrom(() => new Dictionary<string, CustomFurnitureModel>(StringComparer.Ordinal), AssetLoadPriority.Exclusive);
        }
    }
}
