using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Locations;

namespace Ts_Core.Services.FarmhouseFixes
{
    internal static class FarmHouseWarpFixPatch
    {
        private sealed class State
        {
            public Vector2 Position { get; init; }

            public int WarpX { get; init; }

            public int WarpY { get; init; }
        }

        //----------------------------------------
        // Patch登録
        //----------------------------------------

        internal static void Apply(
            Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(
                    typeof(FarmHouse),
                    "resetLocalState"
                ),
                prefix: new HarmonyMethod(
                    typeof(FarmHouseWarpFixPatch),
                    nameof(Prefix)
                ),
                postfix: new HarmonyMethod(
                    typeof(FarmHouseWarpFixPatch),
                    nameof(Postfix)
                )
            );
        }

        //----------------------------------------
        // Prefix
        //----------------------------------------

        private static void Prefix(
            FarmHouse __instance,
            ref State? __state)
        {
            GameLocation? previousLocation =
                Game1.player.currentLocation;

            //----------------------------------------
            // 移動元Location確認
            //----------------------------------------

            if (previousLocation == null)
                return;

            //----------------------------------------
            // 同じFarmHouse内の場合は対象外
            //----------------------------------------

            if (ReferenceEquals(
                    previousLocation,
                    __instance))
            {
                return;
            }

            //----------------------------------------
            // CellarからFarmHouseへのWarpは対象外
            //----------------------------------------

            if (previousLocation.NameOrUniqueName
                .StartsWith(
                    "Cellar",
                    StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            //----------------------------------------
            // Warp先座標を保存
            //----------------------------------------

            __state =
                new State
                {
                    Position =
                        Game1.player.Position,

                    WarpX =
                        Game1.xLocationAfterWarp,

                    WarpY =
                        Game1.yLocationAfterWarp
                };
        }

        //----------------------------------------
        // Postfix
        //----------------------------------------

        private static void Postfix(
            State? __state)
        {
            if (__state == null)
                return;

            //----------------------------------------
            // resetLocalStateで変更された座標を復元
            //----------------------------------------

            Game1.player.Position =
                __state.Position;

            Game1.xLocationAfterWarp =
                __state.WarpX;

            Game1.yLocationAfterWarp =
                __state.WarpY;
        }
    }
}