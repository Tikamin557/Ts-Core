using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using Ts_Core.Services.BigCraftableRelated;

namespace Ts_Core.Patches.BigCraftableRelated
{
    /// <summary>
    /// BigCraftable Extensionの
    /// TileActionをBigCraftableの操作時に実行するPatchです。
    /// </summary>
    public static class BigCraftableActionPatch
    {
        //----------------------------------------
        // Apply
        //----------------------------------------

        /// <summary>
        /// BigCraftableの操作判定に
        /// Patchを適用します。
        /// </summary>
        public static void Apply(
            Harmony harmony)
        {
            //----------------------------------------
            // Object.checkForAction
            //----------------------------------------

            harmony.Patch(
                original:
                    AccessTools.Method(
                        typeof(StardewValley.Object),
                        nameof(StardewValley.Object.checkForAction),
                        new[]
                        {
                    typeof(Farmer),
                    typeof(bool)
                        }),
                postfix:
                    new HarmonyMethod(
                        typeof(BigCraftableActionPatch),
                        nameof(CheckForActionPostfix)));

            //----------------------------------------
            // GameLocation.checkAction
            //----------------------------------------

            harmony.Patch(
                original:
                    AccessTools.Method(
                        typeof(GameLocation),
                        nameof(GameLocation.checkAction),
                        new[]
                        {
                    typeof(
                        xTile.Dimensions.Location),
                    typeof(
                        xTile.Dimensions.Rectangle),
                    typeof(Farmer)
                        }),
                postfix:
                    new HarmonyMethod(
                        typeof(BigCraftableActionPatch),
                        nameof(LocationCheckActionPostfix)));

            //----------------------------------------
            // GameLocation.isActionableTile
            //----------------------------------------

            harmony.Patch(
                original:
                    AccessTools.Method(
                        typeof(GameLocation),
                        nameof(GameLocation.isActionableTile),
                        new[]
                        {
                typeof(int),
                typeof(int),
                typeof(Farmer)
                        }),
                postfix:
                    new HarmonyMethod(
                        typeof(BigCraftableActionPatch),
                        nameof(IsActionableTilePostfix)));
        }

        //----------------------------------------
        // Object.checkForAction
        //----------------------------------------

        /// <summary>
        /// Vanillaで操作が処理されなかった場合、
        /// BigCraftable ExtensionのTileActionを確認します。
        /// </summary>
        private static void CheckForActionPostfix(
            StardewValley.Object __instance,
            Farmer who,
            bool justCheckingForActivity,
            ref bool __result)
        {
            //----------------------------------------
            // Vanilla判定
            //----------------------------------------

            if (__result)
            {
                return;
            }

            //----------------------------------------
            // BigCraftable
            //----------------------------------------

            if (!__instance.bigCraftable.Value)
            {
                return;
            }

            //----------------------------------------
            // Location
            //----------------------------------------

            GameLocation location =
                __instance.Location;

            if (location == null)
            {
                return;
            }

            //----------------------------------------
            // BigCraftable Extension
            //----------------------------------------

            __result =
                BigCraftableExtensionService.Interact(
                    __instance,
                    location,
                    who,
                    justCheckingForActivity);
        }

        //----------------------------------------
        // GameLocation.checkAction
        //----------------------------------------

        /// <summary>
        /// Vanillaで操作対象が見つからなかった場合、
        /// BigCraftable Extensionの仮想Collision範囲から
        /// 対応するBigCraftableを取得して操作します。
        /// </summary>
        private static void LocationCheckActionPostfix(
            GameLocation __instance,
            xTile.Dimensions.Location tileLocation,
            xTile.Dimensions.Rectangle viewport,
            Farmer who,
            ref bool __result)
        {
            //----------------------------------------
            // Vanilla判定
            //----------------------------------------

            if (__result)
            {
                return;
            }

            //----------------------------------------
            // Tile
            //----------------------------------------

            Vector2 tile =
                new Vector2(
                    tileLocation.X,
                    tileLocation.Y);

            //----------------------------------------
            // 実Object確認
            //----------------------------------------

            if (__instance.objects.ContainsKey(
                tile))
            {
                return;
            }

            //----------------------------------------
            // Virtual Collision Object
            //----------------------------------------

            if (!BigCraftableExtensionCollisionService
                .TryGetCollisionObject(
                    __instance,
                    tile,
                    out StardewValley.Object obj))
            {
                return;
            }

            //----------------------------------------
            // Interaction
            //----------------------------------------

            __result =
                BigCraftableExtensionService.Interact(
                    obj,
                    __instance,
                    who,
                    justCheckingForActivity: false);
        }

        //----------------------------------------
        // GameLocation.isActionableTile
        //----------------------------------------

        /// <summary>
        /// VanillaでAction可能なTileと判定されなかった場合、
        /// BigCraftable Extensionの仮想Collision範囲にある
        /// BigCraftableのAction判定を行います。
        /// </summary>
        private static void IsActionableTilePostfix(
            GameLocation __instance,
            int xTile,
            int yTile,
            Farmer who,
            ref bool __result)
        {
            //----------------------------------------
            // Vanilla判定
            //----------------------------------------

            if (__result)
                return;

            //----------------------------------------
            // Tile
            //----------------------------------------

            Vector2 tile =
                new Vector2(
                    xTile,
                    yTile);

            //----------------------------------------
            // Virtual Collision Object
            //----------------------------------------

            if (!BigCraftableExtensionCollisionService
                .TryGetCollisionObject(
                    __instance,
                    tile,
                    out StardewValley.Object obj))
            {
                return;
            }

            //----------------------------------------
            // Actionable
            //----------------------------------------

            if (!obj.isActionable(
                who))
            {
                return;
            }

            __result =
                true;

            //----------------------------------------
            // Cursor Transparency
            //----------------------------------------

            if (!Utility.tileWithinRadiusOfPlayer(
                xTile,
                yTile,
                1,
                who))
            {
                Game1.mouseCursorTransparency =
                    0.5f;
            }
        }
    }
}