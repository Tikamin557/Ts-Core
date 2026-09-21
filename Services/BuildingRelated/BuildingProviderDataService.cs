using StardewModdingAPI.Events;
using Ts_Core.Models;

namespace Ts_Core.Services.BuildingRelated
{
    /// <summary>
    /// Building Provider用の
    /// Data Assetを管理するサービスです。
    /// </summary>
    public static class BuildingProviderDataService
    {
        //----------------------------------------
        // Constants
        //----------------------------------------

        /// <summary>
        /// Building Provider用Data Assetの名前です。
        /// </summary>
        public const string AssetName =
            "TsCore/BuildingProviders";

        //----------------------------------------
        // Asset Requested
        //----------------------------------------

        /// <summary>
        /// Building Provider用Data Assetが
        /// 要求された時の処理です。
        /// </summary>
        public static void OnAssetRequested(
            object? sender,
            AssetRequestedEventArgs e)
        {
            //----------------------------------------
            // Building Providers Data Asset
            //----------------------------------------

            if (!e.NameWithoutLocale.IsEquivalentTo(
                AssetName))
            {
                return;
            }

            e.LoadFrom(
                () =>
                    new Dictionary<
                        string,
                        BuildingProviderModel>(),
                AssetLoadPriority.Exclusive);
        }
    }
}