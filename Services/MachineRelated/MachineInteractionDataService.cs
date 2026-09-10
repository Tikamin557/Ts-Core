using StardewModdingAPI.Events;
using StardewValley.GameData.Machines;
using Ts_Core.Models.MachineRelated;

namespace Ts_Core.Services.MachineRelated
{
    /// <summary>
    /// Machine Interaction用の
    /// Data Assetを管理するサービスです。
    /// </summary>
    public static class MachineInteractionDataService
    {
        //----------------------------------------
        // Constants
        //----------------------------------------

        /// <summary>
        /// Machine Interaction用Data Assetの名前です。
        /// </summary>
        public const string AssetName =
            "TsCore/MachineInteraction";

        /// <summary>
        /// Data/Machines側で
        /// Machine Interaction IDを指定するCustomField名です。
        /// </summary>
        public const string InteractionField =
            "TsCore/MachineInteraction";

        /// <summary>
        /// Machine Interactionで使用する
        /// InteractMethodです。
        /// </summary>
        private const string InteractMethod =
            "Ts_Core.Services.MachineRelated.MachineInteractionService, Ts_Core: Interact";

        //----------------------------------------
        // Asset Requested
        //----------------------------------------

        /// <summary>
        /// Machine Interaction関連Assetが
        /// 要求された時の処理です。
        /// </summary>
        public static void OnAssetRequested(
            object? sender,
            AssetRequestedEventArgs e)
        {
            //----------------------------------------
            // Machine Interaction Data Asset
            //----------------------------------------

            if (e.NameWithoutLocale.IsEquivalentTo(
                AssetName))
            {
                e.LoadFrom(
                    () =>
                        new Dictionary<
                            string,
                            MachineInteractionData>(),
                    AssetLoadPriority.Exclusive);

                return;
            }

            //----------------------------------------
            // Data/Machines
            //----------------------------------------

            if (!e.NameWithoutLocale.IsEquivalentTo(
                "Data/Machines"))
            {
                return;
            }

            e.Edit(
                asset =>
                {
                    IDictionary<string, MachineData> machines =
                        asset.AsDictionary<
                            string,
                            MachineData>().Data;

                    //----------------------------------------
                    // Machine確認
                    //----------------------------------------

                    foreach (KeyValuePair<
                        string,
                        MachineData> entry
                        in machines)
                    {
                        MachineData machineData =
                            entry.Value;

                        if (machineData.CustomFields == null)
                            continue;

                        //----------------------------------------
                        // Machine Interaction確認
                        //----------------------------------------

                        if (!machineData.CustomFields.ContainsKey(
                            InteractionField))
                        {
                            continue;
                        }

                        //----------------------------------------
                        // 既存のInteractMethodを優先
                        //----------------------------------------

                        if (!string.IsNullOrWhiteSpace(
                            machineData.InteractMethod))
                        {
                            continue;
                        }

                        //----------------------------------------
                        // TsCore InteractMethodを自動設定
                        //----------------------------------------

                        machineData.InteractMethod =
                            InteractMethod;
                    }
                },
                AssetEditPriority.Late);
        }
    }
}