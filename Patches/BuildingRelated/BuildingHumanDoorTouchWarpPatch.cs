using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;
using Ts_Core.Services.BuildingRelated;

namespace Ts_Core.Patches
{
    /// <summary>
    /// Building Providerの設定に応じて、HumanDoorへの接触で入室します。
    /// タッチ入室が有効な建物では、通常のクリック入室を無効にします。
    /// </summary>
    internal static class BuildingHumanDoorTouchWarpPatch
    {
        //----------------------------------------
        // Patch登録
        //----------------------------------------

        /// <summary>
        /// 移動前判定とHumanDoorのアクション処理にパッチを適用します。
        /// </summary>
        internal static void Apply(Harmony harmony)
        {
            var move = AccessTools.Method(typeof(Farmer), "MovePositionImpl");
            if (move != null)
                harmony.Patch(move, prefix: new HarmonyMethod(
                    typeof(BuildingHumanDoorTouchWarpPatch), nameof(BeforeMove)));

            var action = AccessTools.Method(typeof(Building), nameof(Building.doAction),
                new[] { typeof(Vector2), typeof(Farmer) });
            if (action != null)
                harmony.Patch(action, prefix: new HarmonyMethod(
                    typeof(BuildingHumanDoorTouchWarpPatch), nameof(BeforeBuildingAction)));
            var sound = AccessTools.Method(typeof(GameLocation), nameof(GameLocation.playSound));
            if (sound != null)
                harmony.Patch(sound, prefix: new HarmonyMethod(
                    typeof(BuildingHumanDoorTouchWarpPatch), nameof(BeforeLocationSound)));

            var actionable = AccessTools.Method(typeof(GameLocation), nameof(GameLocation.isActionableTile),
                new[] { typeof(int), typeof(int), typeof(Farmer) });
            if (actionable != null)
                harmony.Patch(actionable, postfix: new HarmonyMethod(
                    typeof(BuildingHumanDoorTouchWarpPatch), nameof(AfterActionableTile)));
        }

        //----------------------------------------
        // HumanDoor判定
        //----------------------------------------

        /// <summary>
        /// 有効なProviderのHumanDoorタッチ設定を取得します。
        /// </summary>
        private static bool IsTouchWarpEnabled(Building building)
        {
            foreach (var provider in BuildingProviderService.GetProvidersForBuilding(
                building.buildingType.Value))
            {
                if (BuildingProviderService.IsProviderEnabled(provider)
                    && provider.HumanDoorTouchWarp)
                    return true;
            }

            return false;
        }

        //----------------------------------------
        // タッチ入室の効果音
        //----------------------------------------

        /// <summary>
        /// タッチ入室中のHumanDoor閉鎖音だけを抑制します。
        /// </summary>
        private static bool BeforeLocationSound(object[] __args)
        {
            return !touchActionInProgress || __args.Length == 0
                || __args[0] is not string cue || cue != "doorClose";
        }

        //----------------------------------------
        // カーソル判定
        //----------------------------------------

        /// <summary>
        /// タッチ入室を有効にしたHumanDoorでは操作可能カーソルを表示しません。
        /// </summary>
        private static void AfterActionableTile(
            GameLocation __instance, int xTile, int yTile, ref bool __result)
        {
            if (!__result)
                return;

            foreach (Building building in __instance.buildings)
            {
                if (!IsTouchWarpEnabled(building))
                    continue;

                if (xTile == building.tileX.Value + building.humanDoor.X
                    && yTile == building.tileY.Value + building.humanDoor.Y)
                {
                    __result = false;
                    return;
                }
            }
        }

        //----------------------------------------
        // クリック入室
        //----------------------------------------

        /// <summary>
        /// タッチ入室が有効なHumanDoorのクリック操作を無効化します。
        /// タッチ入室から呼ばれたdoActionは通常どおり実行します。
        /// </summary>
        private static bool BeforeBuildingAction(
            Building __instance, Vector2 tileLocation, ref bool __result)
        {
            if (touchActionInProgress || !IsTouchWarpEnabled(__instance))
                return true;

            int doorX = __instance.tileX.Value + __instance.humanDoor.X;
            int doorY = __instance.tileY.Value + __instance.humanDoor.Y;
            if ((int)tileLocation.X != doorX || (int)tileLocation.Y != doorY)
                return true;

            __result = false;
            return false;
        }

        //----------------------------------------
        // タッチ入室
        //----------------------------------------

        private static bool touchActionInProgress;

        /// <summary>
        /// 次の移動先がHumanDoorのタイルに初めて接触した場合、
        /// バニラのBuilding.doActionによる入室を試みます。
        /// </summary>
        private static bool BeforeMove(Farmer __instance, int direction, ref bool __result)
        {
            if (touchActionInProgress || !__instance.IsLocalPlayer || Game1.eventUp
                || __instance.currentLocation is not GameLocation location)
                return true;

            var current = __instance.GetBoundingBox();
            var next = __instance.nextPosition(direction);

            foreach (Building building in location.buildings)
            {
                if (!IsTouchWarpEnabled(building))
                    continue;

                int doorX = building.tileX.Value + building.humanDoor.X;
                int doorY = building.tileY.Value + building.humanDoor.Y;
                var touchTile = new Rectangle(doorX * 64, doorY * 64, 64, 64);
                if (!next.Intersects(touchTile) || current.Intersects(touchTile))
                    continue;

                try
                {
                    touchActionInProgress = true;
                    if (building.doAction(new Vector2(doorX, doorY), __instance))
                    {
                        __result = true;
                        return false;
                    }
                }
                finally
                {
                    touchActionInProgress = false;
                }
            }

            return true;
        }
    }
}
