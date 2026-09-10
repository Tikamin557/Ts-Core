using StardewValley;
using Ts_Core.Models.MachineRelated;

namespace Ts_Core.Services.MachineRelated
{
    /// <summary>
    /// Machine Interactionの
    /// Action Wobbleを管理するサービスです。
    /// </summary>
    public static class MachineActionWobbleService
    {
        //----------------------------------------
        // Active Wobbles
        //----------------------------------------

        /// <summary>
        /// 現在Action Wobble中のMachineと
        /// 終了時刻を管理します。
        /// </summary>
        private static readonly Dictionary<
            StardewValley.Object,
            double> ActiveWobbles =
                new();

        //----------------------------------------
        // Start
        //----------------------------------------

        /// <summary>
        /// Action Wobbleを開始します。
        /// </summary>
        public static void Start(
            StardewValley.Object machine,
            MachineInteractionData interaction)
        {
            if (!interaction.ActionWobble)
                return;

            if (interaction.ActionWobbleDurationMs <= 0)
                return;

            double currentTime =
                Game1.currentGameTime
                    .TotalGameTime
                    .TotalMilliseconds;

            double endTime =
                currentTime
                + interaction.ActionWobbleDurationMs;

            //----------------------------------------
            // 終了時刻更新
            //----------------------------------------

            ActiveWobbles[machine] =
                endTime;

            //----------------------------------------
            // vanilla Wobble開始時と同じ初期値
            //----------------------------------------

            machine.scale.X =
                5f;
        }

        //----------------------------------------
        // Active Check
        //----------------------------------------

        /// <summary>
        /// 指定MachineがAction Wobble中か取得します。
        /// </summary>
        public static bool IsActive(
            StardewValley.Object machine)
        {
            if (!ActiveWobbles.TryGetValue(
                machine,
                out double endTime))
            {
                return false;
            }

            double currentTime =
                Game1.currentGameTime
                    .TotalGameTime
                    .TotalMilliseconds;

            if (currentTime < endTime)
                return true;

            //----------------------------------------
            // 終了済み状態を削除
            //----------------------------------------

            ActiveWobbles.Remove(
                machine);

            return false;
        }

        //----------------------------------------
        // Clear
        //----------------------------------------

        /// <summary>
        /// Action Wobbleの状態を全て削除します。
        /// </summary>
        public static void Clear()
        {
            ActiveWobbles.Clear();
        }
    }
}