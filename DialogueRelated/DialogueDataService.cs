using StardewModdingAPI.Events;
using Ts_Core.Models.DialogueRelated;

namespace Ts_Core.Services.DialogueRelated
{
    /// <summary>
    /// TsCore Dialogue用Data Assetを提供します。
    /// </summary>
    internal static class DialogueDataService
    {
        //----------------------------------------
        // Data Asset
        //----------------------------------------

        /// <summary>
        /// Dialogue定義を登録するData Asset名です。
        /// </summary>
        public const string AssetName =
            "TsCore/Dialogues";

        //----------------------------------------
        // AssetRequested
        //----------------------------------------

        /// <summary>
        /// TsCore/Dialogues のData Assetを提供します。
        /// </summary>
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
                        DialogueModel>(
                            StringComparer.OrdinalIgnoreCase),
                AssetLoadPriority.Exclusive);
        }
    }
}