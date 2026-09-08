using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;

namespace Ts_Core.Services.FarmhouseFixes
{
    internal static class FarmHouseEntranceTracker
    {
        //----------------------------------------
        // 現在doAction中のFarmHouse
        //----------------------------------------

        private static FarmHouse? activeDoorInterior;

        //----------------------------------------
        // 通常玄関からWarp中のFarmHouse
        //----------------------------------------

        private static FarmHouse? pendingEntranceWarp;

        //----------------------------------------
        // Patch登録
        //----------------------------------------

        internal static void Apply(
            Harmony harmony)
        {
            //----------------------------------------
            // Building.doAction
            //----------------------------------------

            harmony.Patch(
                original: AccessTools.Method(
                    typeof(Building),
                    nameof(Building.doAction),
                    new[]
                    {
                        typeof(Vector2),
                        typeof(Farmer)
                    }
                ),
                prefix: new HarmonyMethod(
                    typeof(FarmHouseEntranceTracker),
                    nameof(BuildingDoActionPrefix)
                ),
                postfix: new HarmonyMethod(
                    typeof(FarmHouseEntranceTracker),
                    nameof(BuildingDoActionPostfix)
                )
            );

            //----------------------------------------
            // Game1.warpFarmer
            //----------------------------------------

            harmony.Patch(
                original: AccessTools.Method(
                    typeof(Game1),
                    nameof(Game1.warpFarmer),
                    new[]
                    {
                        typeof(LocationRequest),
                        typeof(int),
                        typeof(int),
                        typeof(int)
                    }
                ),
                prefix: new HarmonyMethod(
                    typeof(FarmHouseEntranceTracker),
                    nameof(WarpFarmerPrefix)
                )
            );
        }

        //----------------------------------------
        // Building.doAction Prefix
        //----------------------------------------

        private static void BuildingDoActionPrefix(
            Building __instance,
            Vector2 tileLocation,
            ref FarmHouse? __state)
        {
            __state = null;

            GameLocation? interior =
                __instance.GetIndoors();

            //----------------------------------------
            // FarmHouse以外は対象外
            //----------------------------------------

            if (interior is not FarmHouse farmHouse)
                return;

            //----------------------------------------
            // humanDoorのタイルか確認
            //----------------------------------------

            int doorX =
                __instance.tileX.Value
                + __instance.humanDoor.X;

            int doorY =
                __instance.tileY.Value
                + __instance.humanDoor.Y;

            if ((int)tileLocation.X != doorX
                || (int)tileLocation.Y != doorY)
            {
                return;
            }

            //----------------------------------------
            // 通常玄関処理開始
            //----------------------------------------

            activeDoorInterior =
                farmHouse;

            __state =
                farmHouse;
        }

        //----------------------------------------
        // Building.doAction Postfix
        //----------------------------------------

        private static void BuildingDoActionPostfix(
            FarmHouse? __state)
        {
            if (__state == null)
                return;

            //----------------------------------------
            // このdoActionで設定した状態だけ解除
            //----------------------------------------

            if (ReferenceEquals(
                    activeDoorInterior,
                    __state))
            {
                activeDoorInterior = null;
            }
        }

        //----------------------------------------
        // Game1.warpFarmer Prefix
        //----------------------------------------

        private static void WarpFarmerPrefix(
            LocationRequest locationRequest)
        {
            FarmHouse? doorInterior =
                activeDoorInterior;

            if (doorInterior == null)
                return;

            //----------------------------------------
            // doAction中のFarmHouseと
            // 実際のWarp先が一致するか確認
            //----------------------------------------

            if (!ReferenceEquals(
                    locationRequest.Location,
                    doorInterior))
            {
                return;
            }

            //----------------------------------------
            // 通常玄関から開始されたWarpとして記録
            //----------------------------------------

            pendingEntranceWarp =
                doorInterior;
        }

        //----------------------------------------
        // 通常玄関Warpか確認して消費
        //----------------------------------------

        internal static bool ConsumeEntranceWarp(
            FarmHouse farmHouse)
        {
            if (!ReferenceEquals(
                    pendingEntranceWarp,
                    farmHouse))
            {
                return false;
            }

            pendingEntranceWarp = null;

            return true;
        }
    }
}