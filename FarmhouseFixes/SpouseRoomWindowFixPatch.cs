using HarmonyLib;
using StardewValley;
using StardewValley.Locations;

namespace Ts_Core.Services.FarmhouseFixes
{
    /// <summary>
    /// Content Patcher等から読み込まれた配偶者部屋で、
    /// indoor TileSheetがMapOverride時に別IDへ変換された場合でも、
    /// Stardew Valley標準の窓・照明処理を正しく適用します。
    /// </summary>
    internal static class SpouseRoomWindowFixPatch
    {
        //----------------------------------------
        // TileSheet ID
        //----------------------------------------

        private const string VanillaIndoorTileSheet =
            "indoor";

        private const string SpouseRoomIndoorTileSheet =
            "zzzzz_spouse_room_indoor";

        //----------------------------------------
        // Patch状態
        //----------------------------------------

        /// <summary>
        /// FarmHouse.loadSpouseRoom実行中の深さ。
        /// </summary>
        private static int loadSpouseRoomDepth;

        //----------------------------------------
        // Patch適用
        //----------------------------------------

        /// <summary>
        /// 配偶者部屋の読み込みとTileSheet ID取得処理へ
        /// Harmonyパッチを適用します。
        /// </summary>
        internal static void Apply(
            Harmony harmony)
        {
            //----------------------------------------
            // 配偶者部屋読み込み範囲を追跡
            //----------------------------------------

            harmony.Patch(
                original:
                    AccessTools.Method(
                        typeof(FarmHouse),
                        "loadSpouseRoom"),
                prefix:
                    new HarmonyMethod(
                        typeof(SpouseRoomWindowFixPatch),
                        nameof(BeforeLoadSpouseRoom)),
                postfix:
                    new HarmonyMethod(
                        typeof(SpouseRoomWindowFixPatch),
                        nameof(AfterLoadSpouseRoom)),
                finalizer:
                    new HarmonyMethod(
                        typeof(SpouseRoomWindowFixPatch),
                        nameof(FinalizeLoadSpouseRoom)));

            //----------------------------------------
            // TileSheet ID判定を補完
            //----------------------------------------

            harmony.Patch(
                original:
                    AccessTools.Method(
                        typeof(GameLocation),
                        "getTileSheetIDAt",
                        new[]
                        {
                            typeof(int),
                            typeof(int),
                            typeof(string)
                        }),
                postfix:
                    new HarmonyMethod(
                        typeof(SpouseRoomWindowFixPatch),
                        nameof(AfterGetTileSheetIDAt)));
        }

        //----------------------------------------
        // FarmHouse.loadSpouseRoom
        //----------------------------------------

        /// <summary>
        /// 配偶者部屋の読み込み開始を記録します。
        /// </summary>
        private static void BeforeLoadSpouseRoom()
        {
            loadSpouseRoomDepth++;
        }

        /// <summary>
        /// 配偶者部屋の再読み込み後、すでに夜または雨なら
        /// Stardew Valley標準のNightTilesを再適用します。
        /// </summary>
        private static void AfterLoadSpouseRoom(
            FarmHouse __instance)
        {
            int nightTilesTime =
                Game1.getTrulyDarkTime(__instance) - 100;

            if (Game1.timeOfDay < nightTilesTime
                && !__instance.IsRainingHere())
            {
                return;
            }

            __instance.switchOutNightTiles();
        }

        /// <summary>
        /// 配偶者部屋の読み込み終了を記録します。
        /// 例外時にも必ず状態を戻します。
        /// </summary>
        private static Exception? FinalizeLoadSpouseRoom(
            Exception? __exception)
        {
            if (loadSpouseRoomDepth > 0)
            {
                loadSpouseRoomDepth--;
            }

            return __exception;
        }

        //----------------------------------------
        // GameLocation.getTileSheetIDAt
        //----------------------------------------

        /// <summary>
        /// 配偶者部屋読み込み中にMapOverrideによって
        /// 別IDへ変換されたindoor TileSheetだけを、
        /// Stardew Valley標準のindoorとして扱います。
        /// </summary>
        private static void AfterGetTileSheetIDAt(
            ref string __result)
        {
            //----------------------------------------
            // 配偶者部屋読み込み中以外は変更しない
            //----------------------------------------

            if (loadSpouseRoomDepth <= 0)
                return;

            //----------------------------------------
            // spouse_room MapOverrideによって
            // 変換されたindoor以外は変更しない
            //----------------------------------------

            if (!string.Equals(
                    __result,
                    SpouseRoomIndoorTileSheet,
                    StringComparison.Ordinal))
            {
                return;
            }

            //----------------------------------------
            // Vanillaの窓・照明判定へ通す
            //----------------------------------------

            __result =
                VanillaIndoorTileSheet;
        }
    }
}
