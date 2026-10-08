using HarmonyLib;
using StardewValley.Locations;
using xTile.Layers;
using xTile.Tiles;

namespace Ts_Core.Services.FarmhouseFixes
{
    /// <summary>
    /// FarmHouse._ApplyRenovationsによって、
    /// 配偶者部屋左上付近のFront (49, 19)に
    /// Stardew Valley標準のタイルが強制的に設定される際、
    /// 既存のカスタムマップ状態を上書きしないよう補正します。
    /// </summary>
    internal static class SpouseRoomTileFixPatch
    {
        //----------------------------------------
        // 対象座標
        //----------------------------------------

        private const int TargetX = 49;
        private const int TargetY = 19;

        //----------------------------------------
        // Stardew Valley標準タイル
        //----------------------------------------

        private const string VanillaIndoorTileSheet =
            "indoor";

        private const string VanillaFrontTileSheet =
            "untitled tile sheet";

        //----------------------------------------
        // Corner Roomなし
        //----------------------------------------

        private const int CornerClosedBuildingsTile =
            3;

        private const int CornerClosedFrontTile =
            87;

        //----------------------------------------
        // Corner Roomあり
        //----------------------------------------

        private const int CornerOpenBuildingsTile =
            68;

        private const int CornerOpenFrontTile =
            229;

        //----------------------------------------
        // Patch State
        //----------------------------------------

        /// <summary>
        /// _ApplyRenovations実行前の状態を保持します。
        /// </summary>
        private sealed class PatchState
        {
            /// <summary>
            /// 処理前のFrontタイル。
            /// nullの場合は元々タイルが存在しません。
            /// </summary>
            internal Tile? OriginalFrontTile
            {
                get;
                set;
            }

            /// <summary>
            /// Renovation後にFrontを
            /// 処理前の状態へ戻すか。
            /// </summary>
            internal bool RestoreFront
            {
                get;
                set;
            }
        }

        //----------------------------------------
        // Patch適用
        //----------------------------------------

        /// <summary>
        /// FarmHouse._ApplyRenovationsへPatchを適用します。
        /// </summary>
        internal static void Apply(
            Harmony harmony)
        {
            harmony.Patch(
                original:
                    AccessTools.Method(
                        typeof(FarmHouse),
                        "_ApplyRenovations"),
                prefix:
                    new HarmonyMethod(
                        typeof(SpouseRoomTileFixPatch),
                        nameof(BeforeApplyRenovations)),
                postfix:
                    new HarmonyMethod(
                        typeof(SpouseRoomTileFixPatch),
                        nameof(AfterApplyRenovations)));
        }

        //----------------------------------------
        // Prefix
        //----------------------------------------

        /// <summary>
        /// Renovation処理前のMap状態を確認し、
        /// カスタム状態の場合はFrontを保存します。
        /// </summary>
        private static void BeforeApplyRenovations(
            FarmHouse __instance,
            out PatchState __state)
        {
            __state =
                new PatchState();

            //----------------------------------------
            // 設定確認
            //----------------------------------------

            if (!ModEntry.Config.EnableSpouseRoomTileFix)
                return;

            //----------------------------------------
            // Cabinは対象外
            //----------------------------------------

            if (__instance is Cabin)
                return;

            //----------------------------------------
            // 対象となるUpgradeのみ
            //----------------------------------------

            if (__instance.upgradeLevel < 2)
                return;

            //----------------------------------------
            // Map確認
            //----------------------------------------

            if (__instance.Map == null)
                return;

            Layer? frontLayer =
                __instance.Map.GetLayer(
                    "Front");

            if (frontLayer == null)
                return;

            //----------------------------------------
            // 座標確認
            //----------------------------------------

            if (!IsCoordinateValid(
                    frontLayer))
            {
                return;
            }

            //----------------------------------------
            // 処理前のFrontを保存
            //----------------------------------------

            __state.OriginalFrontTile =
                frontLayer.Tiles[
                    TargetX,
                    TargetY];

            //----------------------------------------
            // 現在のMapがバニラ状態か確認
            //----------------------------------------

            bool isVanillaState =
                IsVanillaTileState(
                    __instance);

            //----------------------------------------
            // バニラ状態ではない場合のみ、
            // Renovation後にFrontを復元する
            //----------------------------------------

            __state.RestoreFront =
                !isVanillaState;
        }

        //----------------------------------------
        // Postfix
        //----------------------------------------

        /// <summary>
        /// カスタムMap状態だった場合、
        /// _ApplyRenovations実行前のFrontへ戻します。
        /// </summary>
        private static void AfterApplyRenovations(
            FarmHouse __instance,
            PatchState __state)
        {
            if (!__state.RestoreFront)
                return;

            //----------------------------------------
            // Map確認
            //----------------------------------------

            if (__instance.Map == null)
                return;

            Layer? frontLayer =
                __instance.Map.GetLayer(
                    "Front");

            if (frontLayer == null)
                return;

            //----------------------------------------
            // 座標確認
            //----------------------------------------

            if (!IsCoordinateValid(
                    frontLayer))
            {
                return;
            }

            //----------------------------------------
            // 処理前のFrontへ復元
            //----------------------------------------

            frontLayer.Tiles[
                TargetX,
                TargetY] =
                __state.OriginalFrontTile;
        }

        //----------------------------------------
        // バニラMap状態判定
        //----------------------------------------

        /// <summary>
        /// (49, 19)のLayer状態が、
        /// Corner RoomのOpen / Closedいずれかの
        /// Stardew Valley標準状態か確認します。
        /// </summary>
        private static bool IsVanillaTileState(
            FarmHouse farmHouse)
        {
            //----------------------------------------
            // 標準Layer取得
            //----------------------------------------

            Layer? backLayer =
                farmHouse.Map.GetLayer(
                    "Back");

            Layer? buildingsLayer =
                farmHouse.Map.GetLayer(
                    "Buildings");

            Layer? buildings2Layer =
                farmHouse.Map.GetLayer(
                    "Buildings2");

            Layer? frontLayer =
                farmHouse.Map.GetLayer(
                    "Front");

            Layer? pathsLayer =
                farmHouse.Map.GetLayer(
                    "Paths");

            //----------------------------------------
            // 必要Layerがなければ
            // バニラ状態とは判定しない
            //----------------------------------------

            if (backLayer == null
                || buildingsLayer == null
                || buildings2Layer == null
                || frontLayer == null
                || pathsLayer == null)
            {
                return false;
            }

            //----------------------------------------
            // 座標確認
            //----------------------------------------

            if (!IsCoordinateValid(backLayer)
                || !IsCoordinateValid(buildingsLayer)
                || !IsCoordinateValid(buildings2Layer)
                || !IsCoordinateValid(frontLayer)
                || !IsCoordinateValid(pathsLayer))
            {
                return false;
            }

            //----------------------------------------
            // 対象座標の標準Layer状態
            //----------------------------------------

            Tile? backTile =
                backLayer.Tiles[
                    TargetX,
                    TargetY];

            Tile? buildingsTile =
                buildingsLayer.Tiles[
                    TargetX,
                    TargetY];

            Tile? buildings2Tile =
                buildings2Layer.Tiles[
                    TargetX,
                    TargetY];

            Tile? frontTile =
                frontLayer.Tiles[
                    TargetX,
                    TargetY];

            Tile? pathsTile =
                pathsLayer.Tiles[
                    TargetX,
                    TargetY];

            //----------------------------------------
            // Back / Buildings2 / Pathsは
            // バニラでは空
            //----------------------------------------

            if (backTile != null
                || buildings2Tile != null
                || pathsTile != null)
            {
                return false;
            }

            //----------------------------------------
            // Corner Roomなし
            //----------------------------------------

            bool isCornerClosed =
                IsTile(
                    buildingsTile,
                    VanillaIndoorTileSheet,
                    CornerClosedBuildingsTile)
                && IsTile(
                    frontTile,
                    VanillaFrontTileSheet,
                    CornerClosedFrontTile);

            //----------------------------------------
            // Corner Roomあり
            //----------------------------------------

            bool isCornerOpen =
                IsTile(
                    buildingsTile,
                    VanillaIndoorTileSheet,
                    CornerOpenBuildingsTile)
                && IsTile(
                    frontTile,
                    VanillaFrontTileSheet,
                    CornerOpenFrontTile);

            //----------------------------------------
            // どちらのバニラ状態でもない場合は
            // カスタムMapとして扱う
            //----------------------------------------

            if (!isCornerClosed
                && !isCornerOpen)
            {
                return false;
            }

            //----------------------------------------
            // 標準外Layer確認
            //
            // カスタムMapが追加Layerを使用している場合、
            // 対象座標にタイルがあれば
            // バニラ状態とは判定しない
            //----------------------------------------

            foreach (Layer layer
                in farmHouse.Map.Layers)
            {
                if (IsStandardLayer(
                        layer.Id))
                {
                    continue;
                }

                if (!IsCoordinateValid(
                        layer))
                {
                    continue;
                }

                if (layer.Tiles[
                        TargetX,
                        TargetY] != null)
                {
                    return false;
                }
            }

            return true;
        }

        //----------------------------------------
        // 標準Layer判定
        //----------------------------------------

        private static bool IsStandardLayer(
            string layerId)
        {
            return string.Equals(
                       layerId,
                       "Back",
                       StringComparison.OrdinalIgnoreCase)
                || string.Equals(
                       layerId,
                       "Buildings",
                       StringComparison.OrdinalIgnoreCase)
                || string.Equals(
                       layerId,
                       "Buildings2",
                       StringComparison.OrdinalIgnoreCase)
                || string.Equals(
                       layerId,
                       "Front",
                       StringComparison.OrdinalIgnoreCase)
                || string.Equals(
                       layerId,
                       "Paths",
                       StringComparison.OrdinalIgnoreCase);
        }

        //----------------------------------------
        // Tile判定
        //----------------------------------------

        /// <summary>
        /// TileSheetとTileIndexが
        /// 指定値と一致するか確認します。
        /// </summary>
        private static bool IsTile(
            Tile? tile,
            string tileSheetId,
            int tileIndex)
        {
            if (tile == null)
                return false;

            if (!string.Equals(
                    tile.TileSheet?.Id,
                    tileSheetId,
                    StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return tile.TileIndex ==
                tileIndex;
        }

        //----------------------------------------
        // 座標確認
        //----------------------------------------

        /// <summary>
        /// 対象座標がLayerの範囲内か確認します。
        /// </summary>
        private static bool IsCoordinateValid(
            Layer layer)
        {
            return TargetX >= 0
                && TargetY >= 0
                && TargetX < layer.LayerWidth
                && TargetY < layer.LayerHeight;
        }
    }
}