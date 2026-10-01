using HarmonyLib;
using StardewValley.Locations;

namespace Ts_Core.Services.MapRelated.PostRenovation
{
    /// <summary>
    /// FarmHouseのRenovation適用後に
    /// TsCore Post Renovation Patchを適用します。
    /// </summary>
    internal static class PostRenovationPatch
    {
        internal static void Apply(
            Harmony harmony)
        {
            harmony.Patch(
                original:
                    AccessTools.Method(
                        typeof(FarmHouse),
                        nameof(FarmHouse.updateFarmLayout)),
                postfix:
                    new HarmonyMethod(
                        typeof(PostRenovationPatch),
                        nameof(AfterUpdateFarmLayout)));
        }

        private static void AfterUpdateFarmLayout(
            FarmHouse __instance)
        {
            PostRenovationPatchService.ApplyPatches(
                __instance);
        }
    }
}
