using HarmonyLib;
using StardewValley;
using StardewValley.GameData.Buildings;
using StardewValley.Menus;
using Ts_Core.Services.BuildingRelated;

namespace Ts_Core.Patches
{
    /// <summary>
    /// Building ProviderのLocation建築制限を適用するHarmonyパッチです。
    /// </summary>
    internal static class BuildingLocationRestrictionPatch
    {
        //----------------------------------------
        // Patch適用
        //----------------------------------------

        /// <summary>
        /// 建築Location判定にHarmonyパッチを適用します。
        /// </summary>
        public static void Apply(
            Harmony harmony)
        {
            var method =
                AccessTools.Method(
                    typeof(CarpenterMenu),
                    nameof(CarpenterMenu.IsValidBuildingForLocation),
                    new[]
                    {
                        typeof(string),
                        typeof(BuildingData),
                        typeof(GameLocation)
                    });

            if (method == null)
                return;

            harmony.Patch(
                method,
                postfix: new HarmonyMethod(
                    typeof(BuildingLocationRestrictionPatch),
                    nameof(IsValidBuildingForLocationPostfix)));
        }

        //----------------------------------------
        // Valley Farm限定
        //----------------------------------------

        /// <summary>
        /// ValleyFarmOnlyが有効な建物を
        /// バレーのメイン農場以外では非表示にします。
        /// </summary>
        private static void IsValidBuildingForLocationPostfix(
            string typeId,
            GameLocation targetLocation,
            ref bool __result)
        {
            //----------------------------------------
            // バニラ側で既に無効なら変更しない
            //----------------------------------------

            if (!__result)
                return;

            //----------------------------------------
            // Valley Farm限定でなければ変更しない
            //----------------------------------------

            if (!BuildingProviderService.IsValleyFarmOnly(
                    typeId))
            {
                return;
            }

            //----------------------------------------
            // バレーのメイン農場以外は無効
            //----------------------------------------

            if (!ReferenceEquals(
                    targetLocation,
                    Game1.getFarm()))
            {
                __result = false;
            }
        }
    }
}