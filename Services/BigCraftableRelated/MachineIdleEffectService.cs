using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Machines;
using Ts_Core.Models.BigCraftableRelated;

namespace Ts_Core.Services.BigCraftableRelated
{
    /// <summary>
    /// BigCraftable Extensionの
    /// Idle Effectsを管理するサービスです。
    /// </summary>
    public static class MachineIdleEffectService
    {
        //----------------------------------------
        // Time Changed
        //----------------------------------------

        /// <summary>
        /// ゲーム内時間が進んだ時に
        /// 現在LocationのIdle BigCraftableを確認します。
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
            // 現在LocationのBigCraftableを確認
            //----------------------------------------

            foreach (StardewValley.Object machine
                in location.Objects.Values)
            {
                TryPlayIdleEffect(
                    machine);
            }
        }

        //----------------------------------------
        // Idle Effects
        //----------------------------------------

        /// <summary>
        /// 指定BigCraftableがIdle状態の場合、
        /// 設定されたIdle Effectsを実行します。
        /// </summary>
        private static void TryPlayIdleEffect(
            StardewValley.Object machine)
        {
            //----------------------------------------
            // Extension Data取得
            //----------------------------------------

            if (!BigCraftableExtensionDataService
                .TryGetExtensionData(
                    machine,
                    out _,
                    out BigCraftableExtensionData extension))
            {
                return;
            }

            //----------------------------------------
            // Idle Effects確認
            //----------------------------------------

            if (extension.IdleEffects == null
                || extension.IdleEffects.Count == 0)
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
                    extension.IdleEffectChance,
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
                in extension.IdleEffects)
            {
                if (machine.PlayMachineEffect(
                    effect))
                {
                    break;
                }
            }
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