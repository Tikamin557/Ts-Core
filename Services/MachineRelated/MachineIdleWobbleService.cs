using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Machines;
using Ts_Core.Models.MachineRelated;

namespace Ts_Core.Services.MachineRelated
{
    /// <summary>
    /// Machine Interactionの
    /// Idle Wobbleを管理するサービスです。
    /// </summary>
    public static class MachineIdleWobbleService
    {
        //----------------------------------------
        // Active Idle Wobbles
        //----------------------------------------

        /// <summary>
        /// 現在Idle Wobbleが有効な
        /// Machineを管理します。
        /// </summary>
        private static readonly HashSet<
            StardewValley.Object> ActiveIdleWobbles =
                new();

        //----------------------------------------
        // Update Ticked
        //----------------------------------------

        /// <summary>
        /// Idle Wobbleの状態を更新します。
        /// </summary>
        public static void OnUpdateTicked(
            object? sender,
            UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            if (!e.IsMultipleOf(10))
                return;

            GameLocation? location =
                Game1.currentLocation;

            if (location == null)
                return;

            UpdateIdleWobbles(
                location);
        }

        //----------------------------------------
        // Update Idle Wobbles
        //----------------------------------------

        /// <summary>
        /// 現在LocationのIdle Wobble状態を
        /// 更新します。
        /// </summary>
        private static void UpdateIdleWobbles(
            GameLocation location)
        {
            Dictionary<string, MachineInteractionData>
                interactionData =
                    Game1.content.Load<
                        Dictionary<
                            string,
                            MachineInteractionData>>(
                                MachineInteractionDataService.AssetName);

            HashSet<StardewValley.Object>
                activeMachines =
                    new();

            //----------------------------------------
            // Machine確認
            //----------------------------------------

            foreach (StardewValley.Object machine
                in location.Objects.Values)
            {
                MachineData? machineData =
                    machine.GetMachineData();

                if (machineData?.CustomFields == null)
                    continue;

                //----------------------------------------
                // Interaction ID取得
                //----------------------------------------

                if (!machineData.CustomFields.TryGetValue(
                    MachineInteractionDataService.InteractionField,
                    out string? interactionId))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(
                    interactionId))
                {
                    continue;
                }

                //----------------------------------------
                // Interaction Data取得
                //----------------------------------------

                if (!interactionData.TryGetValue(
                    interactionId,
                    out MachineInteractionData? interaction))
                {
                    continue;
                }

                //----------------------------------------
                // Idle Wobble確認
                //----------------------------------------

                if (!interaction.IdleWobble)
                    continue;

                //----------------------------------------
                // Idle状態確認
                //----------------------------------------

                if (machine.readyForHarvest.Value)
                    continue;

                if (machine.minutesUntilReady.Value > 0)
                    continue;

                activeMachines.Add(
                    machine);
            }

            //----------------------------------------
            // 状態更新
            //----------------------------------------

            ActiveIdleWobbles.Clear();

            foreach (StardewValley.Object machine
                in activeMachines)
            {
                ActiveIdleWobbles.Add(
                    machine);
            }
        }

        //----------------------------------------
        // Active Check
        //----------------------------------------

        /// <summary>
        /// 指定MachineのIdle Wobbleが
        /// 現在有効か取得します。
        /// </summary>
        public static bool IsActive(
            StardewValley.Object machine)
        {
            return ActiveIdleWobbles.Contains(
                machine);
        }

        //----------------------------------------
        // Clear
        //----------------------------------------

        /// <summary>
        /// Idle Wobbleの状態を全て削除します。
        /// </summary>
        public static void Clear()
        {
            ActiveIdleWobbles.Clear();
        }
    }
}