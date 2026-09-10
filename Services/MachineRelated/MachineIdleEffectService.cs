using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Machines;
using Ts_Core.Models.MachineRelated;

namespace Ts_Core.Services.MachineRelated
{
    /// <summary>
    /// Machine Interactionの
    /// Idle Effectsを管理するサービスです。
    /// </summary>
    public static class MachineIdleEffectService
    {
        //----------------------------------------
        // Time Changed
        //----------------------------------------

        /// <summary>
        /// ゲーム内時間が進んだ時に
        /// 現在LocationのIdle Machineを確認します。
        /// </summary>
        public static void OnTimeChanged(
            object? sender,
            TimeChangedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            GameLocation? location =
                Game1.currentLocation;

            if (location == null)
                return;

            //----------------------------------------
            // Interaction Data取得
            //----------------------------------------

            Dictionary<string, MachineInteractionData>
                interactionData =
                    LoadInteractionData();

            //----------------------------------------
            // 現在LocationのMachineを確認
            //----------------------------------------

            foreach (StardewValley.Object machine
                in location.Objects.Values)
            {
                TryPlayIdleEffect(
                    machine,
                    interactionData);
            }
        }

        //----------------------------------------
        // Idle Effects
        //----------------------------------------

        /// <summary>
        /// 指定MachineがIdle状態の場合、
        /// 設定されたIdle Effectsを実行します。
        /// </summary>
        private static void TryPlayIdleEffect(
            StardewValley.Object machine,
            Dictionary<
                string,
                MachineInteractionData> interactionData)
        {
            //----------------------------------------
            // Interaction Data取得
            //----------------------------------------

            MachineInteractionData? interaction =
                GetInteractionData(
                    machine,
                    interactionData);

            if (interaction == null)
                return;

            //----------------------------------------
            // Idle Effects確認
            //----------------------------------------

            if (interaction.IdleEffects == null
                || interaction.IdleEffects.Count == 0)
            {
                return;
            }

            //----------------------------------------
            // Idle状態確認
            //----------------------------------------

            if (!IsIdle(
                machine))
            {
                return;
            }

            //----------------------------------------
            // Chance
            //----------------------------------------

            float chance =
                Math.Clamp(
                    interaction.IdleEffectChance,
                    0f,
                    1f);

            if (Game1.random.NextDouble()
                >= chance)
            {
                return;
            }

            //----------------------------------------
            // Machine Effects
            //----------------------------------------

            foreach (MachineEffects effect
                in interaction.IdleEffects)
            {
                if (machine.PlayMachineEffect(
                    effect))
                {
                    break;
                }
            }
        }

        //----------------------------------------
        // Interaction Data
        //----------------------------------------

        /// <summary>
        /// Machineに設定された
        /// Machine Interaction Dataを取得します。
        /// </summary>
        private static MachineInteractionData?
            GetInteractionData(
                StardewValley.Object machine,
                Dictionary<
                    string,
                    MachineInteractionData> interactionData)
        {
            MachineData? machineData =
                machine.GetMachineData();

            if (machineData?.CustomFields == null)
                return null;

            if (!machineData.CustomFields.TryGetValue(
                MachineInteractionDataService.InteractionField,
                out string? interactionId))
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(
                interactionId))
            {
                return null;
            }

            if (!interactionData.TryGetValue(
                interactionId,
                out MachineInteractionData? interaction))
            {
                return null;
            }

            return interaction;
        }

        //----------------------------------------
        // Interaction Data Load
        //----------------------------------------

        /// <summary>
        /// Machine Interaction Dataを読み込みます。
        /// </summary>
        private static Dictionary<
            string,
            MachineInteractionData> LoadInteractionData()
        {
            return
                Game1.content.Load<
                    Dictionary<
                        string,
                        MachineInteractionData>>(
                            MachineInteractionDataService.AssetName);
        }

        //----------------------------------------
        // Idle Check
        //----------------------------------------

        /// <summary>
        /// MachineがIdle状態か取得します。
        /// </summary>
        private static bool IsIdle(
            StardewValley.Object machine)
        {
            if (machine.readyForHarvest.Value)
                return false;

            if (machine.minutesUntilReady.Value > 0)
                return false;

            return true;
        }
    }
}