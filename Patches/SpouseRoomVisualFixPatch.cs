using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Locations;
using Ts_Core.Services.ContentPatcherRelated.ContentPatcherOption;
using Ts_Core.Services.FarmhouseFixes;

namespace Ts_Core.Patches
{
    internal static class SpouseRoomVisualFixPatch
    {
        //----------------------------------------
        // Patch登録
        //----------------------------------------

        internal static void Apply(
            Harmony harmony)
        {
            //----------------------------------------
            // FarmHouse表示物更新
            //----------------------------------------

            harmony.Patch(
                original:
                    AccessTools.Method(
                        typeof(FarmHouse),
                        "resetLocalState"),
                prefix:
                    new HarmonyMethod(
                        typeof(
                            SpouseRoomVisualFixPatch),
                        nameof(
                            ResetLocalStatePrefix)),
                postfix:
                    new HarmonyMethod(
                        typeof(
                            SpouseRoomVisualFixPatch),
                        nameof(
                            ResetLocalStatePostfix)));

            //----------------------------------------
            // 配偶者部屋追加衝突判定
            //----------------------------------------

            harmony.Patch(
                original:
                    AccessTools.Method(
                        typeof(GameLocation),
                        nameof(
                            GameLocation.isCollidingPosition),
                        new[]
                        {
                            typeof(Microsoft.Xna.Framework.Rectangle),
                            typeof(xTile.Dimensions.Rectangle),
                            typeof(bool),
                            typeof(int),
                            typeof(bool),
                            typeof(Character),
                            typeof(bool),
                            typeof(bool),
                            typeof(bool),
                            typeof(bool)
                        }),
                postfix:
                    new HarmonyMethod(
                        typeof(
                            SpouseRoomVisualFixPatch),
                        nameof(
                            IsCollidingPositionPostfix)));
        }

        //----------------------------------------
        // FarmHouse resetLocalState
        //----------------------------------------

        private static void ResetLocalStatePrefix(
            out HashSet<DelayedAction> __state)
        {
            //----------------------------------------
            // resetLocalState実行前の
            // DelayedActionを記録
            //----------------------------------------

            __state =
                new HashSet<DelayedAction>(
                    Game1.delayedActions);
        }

        private static void ResetLocalStatePostfix(
            FarmHouse __instance,
            HashSet<DelayedAction> __state)
        {
            //----------------------------------------
            // Sebastianのカエルを非表示にする場合
            // 入室時の鳴き声予約を削除
            //----------------------------------------

            if (ContentPatcherOptionService
                .IsEnabled(
                    ContentPatcherOptionIds
                        .HideSebastianFrog))
            {
                RemoveSebastianCroak(
                    __state);
            }

            //----------------------------------------
            // 表示物更新予約
            //----------------------------------------

            SpouseRoomVisualFixService
                .RequestRefresh(
                    __instance);
        }

        //----------------------------------------
        // Sebastian鳴き声
        //----------------------------------------

        private static void RemoveSebastianCroak(
            HashSet<DelayedAction> previousActions)
        {
            //----------------------------------------
            // resetLocalStateによって
            // 新しく追加されたActionだけ確認
            //----------------------------------------

            for (int i =
                     Game1.delayedActions.Count - 1;
                 i >= 0;
                 i--)
            {
                DelayedAction action =
                    Game1.delayedActions[i];

                //----------------------------------------
                // resetLocalState実行前から存在
                //----------------------------------------

                if (previousActions.Contains(
                        action))
                {
                    continue;
                }

                //----------------------------------------
                // Sebastianの入室時croak以外
                //----------------------------------------

                if (!string.Equals(
                        action.stringData,
                        "croak",
                        StringComparison.Ordinal))
                {
                    continue;
                }

                if (action.timeUntilAction != 1000)
                    continue;

                //----------------------------------------
                // 鳴き声予約を削除
                //----------------------------------------

                Game1.delayedActions
                    .RemoveAt(i);
            }
        }

        //----------------------------------------
        // 衝突判定
        //----------------------------------------

        private static void IsCollidingPositionPostfix(
            GameLocation __instance,
            Rectangle position,
            Character character,
            ref bool __result)
        {
            //----------------------------------------
            // 既にゲーム本体で衝突判定済み
            //----------------------------------------

            if (__result)
                return;

            //----------------------------------------
            // FarmHouse以外
            //----------------------------------------

            if (__instance is not FarmHouse farmHouse)
                return;

            //----------------------------------------
            // プレイヤー以外
            //----------------------------------------

            if (character is not Farmer farmer)
                return;

            //----------------------------------------
            // Emily足場跡
            //----------------------------------------

            if (SpouseRoomVisualFixService
                .IsEmilyParrotStandCollision(
                    farmHouse,
                    position,
                    farmer))
            {
                __result = true;
                return;
            }

            //----------------------------------------
            // Sebastian水槽
            //----------------------------------------

            if (SpouseRoomVisualFixService
                .IsSebastianFrogTankCollision(
                    farmHouse,
                    position,
                    farmer))
            {
                __result = true;
            }
        }
    }
}