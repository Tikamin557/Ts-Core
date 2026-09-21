using StardewModdingAPI.Events;
using Ts_Core.Models;

namespace Ts_Core.Services.Migration
{
    /// <summary>
    /// Migration用の
    /// Data Assetを管理するサービスです。
    /// </summary>
    public static class MigrationDataService
    {
        //----------------------------------------
        // Constants
        //----------------------------------------

        /// <summary>
        /// Migration用Data Assetの名前です。
        /// </summary>
        public const string AssetName =
            "TsCore/Migrations";

        //----------------------------------------
        // Asset Requested
        //----------------------------------------

        /// <summary>
        /// Migration用Data Assetが
        /// 要求された時の処理です。
        /// </summary>
        public static void OnAssetRequested(
            object? sender,
            AssetRequestedEventArgs e)
        {
            //----------------------------------------
            // Migrations Data Asset
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
                        MigrationModel>(),
                AssetLoadPriority.Exclusive);
        }
    }
}