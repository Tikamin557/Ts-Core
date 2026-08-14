using HarmonyLib;
using StardewValley;
using StardewValley.Buildings;
using Ts_Core.Services.LightRelated;

namespace Ts_Core.Patches
{
    /// <summary>
    /// 建物の変更時にBuilding Lightを更新するHarmonyパッチです。
    /// </summary>
    internal static class BuildingLightPatch
    {
        //----------------------------------------
        // Patch適用
        //----------------------------------------

        /// <summary>
        /// 建物関連処理にHarmonyパッチを適用します。
        /// </summary>
        public static void Apply(
            Harmony harmony)
        {
            PatchDestroyStructure(
                harmony);

            PatchBuildingPlacement(
                harmony);
        }

        //----------------------------------------
        // 建物撤去
        //----------------------------------------

        /// <summary>
        /// 建物撤去処理にPostfixを適用します。
        /// </summary>
        private static void PatchDestroyStructure(
            Harmony harmony)
        {
            var method =
                AccessTools.Method(
                    typeof(GameLocation),
                    nameof(GameLocation.destroyStructure),
                    new[]
                    {
                        typeof(Building)
                    });

            if (method == null)
                return;

            harmony.Patch(
                method,
                postfix: new HarmonyMethod(
                    typeof(BuildingLightPatch),
                    nameof(DestroyStructurePostfix)));
        }

        /// <summary>
        /// 建物が正常に撤去された後、
        /// Building Lightを更新します。
        /// </summary>
        private static void DestroyStructurePostfix(
            bool __result)
        {
            if (!__result)
                return;

            BuildingLightService.UpdateLights();
        }

        //----------------------------------------
        // 建物移動
        //----------------------------------------

        /// <summary>
        /// 建物移動後の処理にPostfixを適用します。
        /// </summary>
        private static void PatchBuildingPlacement(
            Harmony harmony)
        {
            var method =
                AccessTools.Method(
                    typeof(Building),
                    nameof(Building.performActionOnBuildingPlacement));

            if (method == null)
                return;

            harmony.Patch(
                method,
                postfix: new HarmonyMethod(
                    typeof(BuildingLightPatch),
                    nameof(BuildingPlacementPostfix)));
        }

        /// <summary>
        /// 建物が移動された後、
        /// Building Lightを新しい位置へ更新します。
        /// </summary>
        private static void BuildingPlacementPostfix()
        {
            BuildingLightService.UpdateLights();
        }
    }
}