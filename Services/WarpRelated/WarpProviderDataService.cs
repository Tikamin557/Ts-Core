using StardewModdingAPI.Events;
using Ts_Core.Models;

namespace Ts_Core.Services.WarpRelated
{
    /// <summary>
    /// Warp Provider用の
    /// Data Assetを管理するサービスです。
    /// </summary>
    public static class WarpProviderDataService
    {
        //----------------------------------------
        // Constants
        //----------------------------------------

        /// <summary>
        /// Warp Provider用Data Assetの名前です。
        /// </summary>
        public const string AssetName =
            "TsCore/WarpProviders";

        //----------------------------------------
        // Default Provider IDs
        //----------------------------------------

        /// <summary>
        /// TsCore標準Warp ProviderのIDです。
        /// </summary>
        private static readonly HashSet<string>
            DefaultProviderIds =
                new(StringComparer.OrdinalIgnoreCase)
                {
                    "FarmHouseFront",
                    "GreenhouseFront",
                    "FarmCaveFront",
                    "IslandFarmHouseFront"
                };

        //----------------------------------------
        // Asset Requested
        //----------------------------------------

        /// <summary>
        /// Warp Provider用Data Assetが
        /// 要求された時の処理です。
        /// </summary>
        public static void OnAssetRequested(
            object? sender,
            AssetRequestedEventArgs e)
        {
            //----------------------------------------
            // Warp Providers Data Asset
            //----------------------------------------

            if (!e.NameWithoutLocale.IsEquivalentTo(
                    AssetName))
            {
                return;
            }

            //----------------------------------------
            // Data Assetには外部Providerのみ登録します。
            //----------------------------------------

            e.LoadFrom(
                () =>
                    new Dictionary<
                        string,
                        WarpProviderModel>(
                            StringComparer.OrdinalIgnoreCase),
                AssetLoadPriority.Exclusive);
        }

        //----------------------------------------
        // Default Provider Check
        //----------------------------------------

        /// <summary>
        /// 指定されたProvider IDが
        /// TsCore標準Warp Providerか確認します。
        /// </summary>
        internal static bool IsDefaultProvider(
            string providerId)
        {
            if (string.IsNullOrWhiteSpace(
                    providerId))
            {
                return false;
            }

            return DefaultProviderIds.Contains(
                providerId);
        }

        //----------------------------------------
        // Default Providers
        //----------------------------------------

        /// <summary>
        /// TsCore標準のWarp Providerを作成します。
        /// </summary>
        internal static Dictionary<
            string,
            WarpProviderModel> GetDefaultProviders()
        {
            return new Dictionary<
                string,
                WarpProviderModel>(
                    StringComparer.OrdinalIgnoreCase)
            {
                //----------------------------------------
                // FarmHouseFront
                //----------------------------------------

                ["FarmHouseFront"] =
                    new WarpProviderModel
                    {
                        Type = "Warp",
                        Source = "FarmHouse",
                        Target = "Farm"
                    },

                //----------------------------------------
                // GreenhouseFront
                //----------------------------------------

                ["GreenhouseFront"] =
                    new WarpProviderModel
                    {
                        Type = "Warp",
                        Source = "Greenhouse",
                        Target = "Farm"
                    },

                //----------------------------------------
                // FarmCaveFront
                //----------------------------------------

                ["FarmCaveFront"] =
                    new WarpProviderModel
                    {
                        Type = "Warp",
                        Source = "FarmCave",
                        Target = "Farm"
                    },

                //----------------------------------------
                // IslandFarmHouseFront
                //----------------------------------------

                ["IslandFarmHouseFront"] =
                    new WarpProviderModel
                    {
                        Type = "Warp",
                        Source = "IslandFarmHouse",
                        Target = "IslandWest"
                    }
            };
        }
    }
}