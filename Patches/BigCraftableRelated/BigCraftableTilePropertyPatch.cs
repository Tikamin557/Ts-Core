using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using Ts_Core.Services.BigCraftableRelated;

namespace Ts_Core.Patches.BigCraftableRelated
{
    /// <summary>
    /// BigCraftable ExtensionのTile Propertyを
    /// GameLocationのTile Property取得処理へ適用するPatchです。
    /// </summary>
    public static class BigCraftableTilePropertyPatch
    {
        //----------------------------------------
        // Apply
        //----------------------------------------

        /// <summary>
        /// GameLocation.doesTileHavePropertyに
        /// Patchを適用します。
        /// </summary>
        public static void Apply(
            Harmony harmony)
        {
            harmony.Patch(
                original:
                    AccessTools.Method(
                        typeof(GameLocation),
                        nameof(GameLocation.doesTileHaveProperty),
                        new[]
                        {
                            typeof(int),
                            typeof(int),
                            typeof(string),
                            typeof(string),
                            typeof(bool)
                        }),
                postfix:
                    new HarmonyMethod(
                        typeof(BigCraftableTilePropertyPatch),
                        nameof(DoesTileHavePropertyPostfix)));
        }

        //----------------------------------------
        // doesTileHaveProperty
        //----------------------------------------

        /// <summary>
        /// VanillaでTile Propertyが見つからなかった場合、
        /// BigCraftable ExtensionのTile Propertyを確認します。
        /// </summary>
        private static void DoesTileHavePropertyPostfix(
            GameLocation __instance,
            int xTile,
            int yTile,
            string propertyName,
            string layerName,
            ref string __result)
        {
            //----------------------------------------
            // Vanilla Property
            //----------------------------------------

            if (__result != null)
            {
                return;
            }

            //----------------------------------------
            // BigCraftable Extension
            //----------------------------------------

            if (!BigCraftableExtensionTilePropertyService
                .TryGetTileProperty(
                    __instance,
                    new Vector2(
                        xTile,
                        yTile),
                    propertyName,
                    layerName,
                    out string propertyValue))
            {
                return;
            }

            //----------------------------------------
            // Property Value
            //----------------------------------------

            __result =
                propertyValue;
        }
    }
}