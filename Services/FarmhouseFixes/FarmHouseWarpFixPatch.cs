using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;

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

        private static State? PendingWarpState;

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

            harmony.Patch(
                original: AccessTools.Method(
                    typeof(DecoratableLocation),
                    nameof(DecoratableLocation.MakeMapModifications)
                ),
                postfix: new HarmonyMethod(
                    typeof(FarmHouseWarpFixPatch),
                    nameof(AfterMakeMapModifications)
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
            // ShopMenu表示中は対象外
            //----------------------------------------

            if (Game1.activeClickableMenu
                is ShopMenu)
            {
                return;
            }

            //----------------------------------------
            // 通常のFarmHouse玄関からのWarpは対象外
            //----------------------------------------

            if (FarmHouseEntranceTracker
                .ConsumeEntranceWarp(__instance))
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

            // DecoratableLocation.MakeMapModificationsでは、
            // Buildingsタイル上にいるプレイヤーを無条件で
            // 1タイル下へ移動するため、後段の補正用に保存
            PendingWarpState = __state;
        }

        //----------------------------------------
        // MakeMapModifications後処理
        //----------------------------------------

        private static void AfterMakeMapModifications(
            DecoratableLocation __instance)
        {
            State? state = PendingWarpState;

            if (state == null)
                return;

            // FarmHouseへの対象Warpに対して一度だけ判定する
            PendingWarpState = null;

            if (__instance is not FarmHouse
                || !ReferenceEquals(
                    Game1.player.currentLocation,
                    __instance))
            {
                return;
            }

            //----------------------------------------
            // VanillaのBuildingsタイル退避確認
            //----------------------------------------

            Vector2 shiftedPosition =
                state.Position + new Vector2(0f, 64f);

            if (Game1.player.Position != shiftedPosition)
                return;

            //----------------------------------------
            // 本来のWarp地点で実際に衝突するか確認
            //----------------------------------------

            Rectangle targetBounds =
                Game1.player.GetBoundingBox();

            targetBounds.Offset(
                (int)(state.Position.X - Game1.player.Position.X),
                (int)(state.Position.Y - Game1.player.Position.Y));

            bool isBlocked =
                __instance.isCollidingPosition(
                    targetBounds,
                    Game1.viewport,
                    isFarmer: true,
                    damagesFarmer: 0,
                    glider: false,
                    character: Game1.player,
                    pathfinding: false,
                    projectile: false,
                    ignoreCharacterRequirement: false,
                    skipCollisionEffects: true);

            if (isBlocked)
                return;

            //----------------------------------------
            // 通行可能ならVanillaの1タイル退避を取消
            //----------------------------------------

            Game1.player.Position =
                state.Position;

            Game1.xLocationAfterWarp =
                state.WarpX;

            Game1.yLocationAfterWarp =
                state.WarpY;
        }
    }
}