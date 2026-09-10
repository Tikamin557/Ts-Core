using HarmonyLib;
using System.Reflection;
using Ts_Core.Services.MachineRelated;

namespace Ts_Core.Patches
{
    /// <summary>
    /// Machine InteractionのWobble状態を
    /// vanillaのWobble処理へ反映するPatchです。
    /// </summary>
    public static class MachineShouldWobblePatch
    {
        //----------------------------------------
        // Apply
        //----------------------------------------

        /// <summary>
        /// Patchを適用します。
        /// </summary>
        public static void Apply(
            Harmony harmony)
        {
            MethodInfo original =
                AccessTools.Method(
                    typeof(StardewValley.Object),
                    nameof(StardewValley.Object.ShouldWobble));

            MethodInfo postfix =
                AccessTools.Method(
                    typeof(MachineShouldWobblePatch),
                    nameof(Postfix));

            harmony.Patch(
                original,
                postfix:
                    new HarmonyMethod(
                        postfix));
        }

        //----------------------------------------
        // Postfix
        //----------------------------------------

        /// <summary>
        /// TsCoreのWobble状態を
        /// vanillaの判定結果へ反映します。
        /// </summary>
        private static void Postfix(
            StardewValley.Object __instance,
            ref bool __result)
        {
            //----------------------------------------
            // TsCore Action Wobble
            //----------------------------------------

            if (MachineActionWobbleService.IsActive(
                __instance))
            {
                __result =
                    true;

                return;
            }

            //----------------------------------------
            // vanilla Wobble
            //----------------------------------------

            if (__result)
                return;

            //----------------------------------------
            // TsCore Idle Wobble
            //----------------------------------------

            if (MachineIdleWobbleService.IsActive(
                __instance))
            {
                __result =
                    true;
            }
        }
    }
}