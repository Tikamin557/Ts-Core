using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using Ts_Core.Models.BigCraftableRelated;
using Ts_Core.Services.BigCraftableRelated;

namespace Ts_Core.Patches.BigCraftableRelated
{
    /// <summary>
    /// BigCraftable Extensionで指定された
    /// 設置条件と設置範囲を適用するPatchです。
    /// </summary>
    public static class BigCraftablePlacementPatch
    {
        //----------------------------------------
        // Apply
        //----------------------------------------

        /// <summary>
        /// BigCraftableの設置判定に
        /// Patchを適用します。
        /// </summary>
        public static void Apply(
            Harmony harmony)
        {
            harmony.Patch(
                original:
                    AccessTools.Method(
                        typeof(StardewValley.Object),
                        nameof(StardewValley.Object.canBePlacedHere),
                        new[]
                        {
                            typeof(GameLocation),
                            typeof(Vector2),
                            typeof(CollisionMask),
                            typeof(bool)
                        }),
                postfix:
                    new HarmonyMethod(
                        typeof(BigCraftablePlacementPatch),
                        nameof(CanBePlacedHerePostfix)));
        }

        //----------------------------------------
        // Object.canBePlacedHere
        //----------------------------------------

        /// <summary>
        /// BigCraftable Extensionで指定された
        /// 設置条件と追加設置範囲を確認します。
        /// </summary>
        private static void CanBePlacedHerePostfix(
            StardewValley.Object __instance,
            GameLocation l,
            Vector2 tile,
            CollisionMask collisionMask,
            ref bool __result)
        {
            //----------------------------------------
            // Vanilla判定
            //----------------------------------------

            if (!__result)
            {
                return;
            }

            //----------------------------------------
            // Extension Data取得
            //----------------------------------------

            if (!BigCraftableExtensionDataService.TryGetExtensionData(
                __instance,
                out _,
                out BigCraftableExtensionData extension))
            {
                return;
            }

            //----------------------------------------
            // Placement Condition
            //----------------------------------------

            if (!string.IsNullOrWhiteSpace(
                extension.PlacementCondition)
                && !GameStateQuery.CheckConditions(
                    extension.PlacementCondition,
                    l,
                    Game1.player))
            {
                __result = false;
                return;
            }

            //----------------------------------------
            // Collision Size
            //----------------------------------------

            int width =
                Math.Max(
                    1,
                    extension.CollisionWidth);

            int height =
                Math.Max(
                    1,
                    extension.CollisionHeight);

            if (width == 1
                && height == 1)
            {
                return;
            }

            //----------------------------------------
            // 追加範囲確認
            //----------------------------------------

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // Anchor Tileは
                    // Vanillaですでに確認済み
                    if (x == 0
                        && y == 0)
                    {
                        continue;
                    }

                    Vector2 checkTile =
                        tile
                        + new Vector2(
                            x,
                            -y);

                    if (!l.CanItemBePlacedHere(
                        checkTile,
                        __instance.isPassable(),
                        collisionMask))
                    {
                        __result = false;
                        return;
                    }
                }
            }
        }
    }
}