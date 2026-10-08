using StardewModdingAPI.Events;
using Ts_Core.Models.MapRelated;

namespace Ts_Core.Services.MapRelated.PostRenovation
{
    /// <summary>
    /// Post Renovation Patch用Data Assetを管理します。
    /// </summary>
    internal static class PostRenovationPatchDataService
    {
        public const string AssetName =
            "TsCore/PostRenovationPatches";

        public static void OnAssetRequested(
            object? sender,
            AssetRequestedEventArgs e)
        {
            if (!e.NameWithoutLocale.IsEquivalentTo(
                    AssetName))
            {
                return;
            }

            e.LoadFrom(
                () =>
                    new Dictionary<
                        string,
                        PostRenovationPatchModel>(
                            StringComparer.OrdinalIgnoreCase),
                AssetLoadPriority.Exclusive);
        }
    }
}
