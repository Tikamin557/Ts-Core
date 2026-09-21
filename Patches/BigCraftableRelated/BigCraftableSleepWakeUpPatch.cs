using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using Ts_Core.Models.BigCraftableRelated;
using Ts_Core.Services.BigCraftableRelated;

namespace Ts_Core.Patches.BigCraftableRelated
{
    /// <summary>
    /// BigCraftable Extensionの
    /// 継続寝床での起床判定を補完します。
    /// </summary>
    public static class BigCraftableSleepWakeUpPatch
    {
        //----------------------------------------
        // Apply
        //----------------------------------------

        public static void Apply(
            Harmony harmony)
        {
            harmony.Patch(
                original:
                    AccessTools.Method(
                        typeof(GameLocation),
                        nameof(GameLocation.CanWakeUpHere),
                        new[]
                        {
                            typeof(Farmer),
                            typeof(Point?)
                        }),
                postfix:
                    new HarmonyMethod(
                        typeof(BigCraftableSleepWakeUpPatch),
                        nameof(CanWakeUpHerePostfix)));
        }

        //----------------------------------------
        // Postfix
        //----------------------------------------

        private static void CanWakeUpHerePostfix(
            GameLocation __instance,
            Farmer who,
            Point? tile,
            ref bool __result)
        {
            //----------------------------------------
            // Vanillaですでに起床可能
            //----------------------------------------

            if (__result)
            {
                return;
            }

            //----------------------------------------
            // 継続寝床取得
            //----------------------------------------

            if (!BigCraftableSleepPersistenceService
                .TryGetValid(
                    who,
                    out BigCraftableSleepData? data)
                || data == null)
            {
                return;
            }

            //----------------------------------------
            // Location確認
            //----------------------------------------

            if (!string.Equals(
                __instance.NameOrUniqueName,
                data.LocationName,
                StringComparison.Ordinal))
            {
                return;
            }

            //----------------------------------------
            // 起床Tile確認
            //----------------------------------------

            Point wakeUpTile =
                tile
                ?? who.lastSleepPoint.Value;

            if (wakeUpTile
                != data.WakeUpPoint)
            {
                return;
            }

            //----------------------------------------
            // TsCore継続寝床として起床可能
            //----------------------------------------

            __result =
                true;
        }
    }
}