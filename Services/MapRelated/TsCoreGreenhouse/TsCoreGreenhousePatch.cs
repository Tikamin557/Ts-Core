using HarmonyLib;
using StardewValley;
using StardewValley.TerrainFeatures;
using xTile.Tiles;

namespace Ts_Core.Services.MapRelated.TsCoreGreenhouse
{
    /// <summary>
    /// TsCoreGreenhouse Map Propertyを
    /// Stardew Valley本体の温室処理へ適用します。
    /// </summary>
    internal static class TsCoreGreenhousePatch
    {
        //----------------------------------------
        // Patch State
        //----------------------------------------

        private sealed class PlacementState
        {
            internal Tile? Tile;
            internal bool AddedDiggable;
            internal bool RestrictFruitTreeType;
        }

        private static int fruitTreePlacementDepth;

        //----------------------------------------
        // Apply
        //----------------------------------------

        /// <summary>
        /// TsCoreGreenhouseに必要なHarmonyパッチを適用します。
        /// </summary>
        internal static void Apply(
            Harmony harmony)
        {
            // Map読み込み時にIsGreenhouseを有効化
            harmony.Patch(
                original:
                    AccessTools.Method(
                        typeof(GameLocation),
                        nameof(GameLocation.loadMap),
                        new[]
                        {
                            typeof(string),
                            typeof(bool)
                        }),
                postfix:
                    new HarmonyMethod(
                        typeof(TsCoreGreenhousePatch),
                        nameof(AfterLoadMap)));

            // Fruit TreeのLocation側植え付け判定を制限
            harmony.Patch(
                original:
                    AccessTools.Method(
                        typeof(GameLocation),
                        nameof(GameLocation.CanPlantTreesHere),
                        new[]
                        {
                            typeof(string),
                            typeof(int),
                            typeof(int),
                            typeof(string).MakeByRefType()
                        }),
                postfix:
                    new HarmonyMethod(
                        typeof(TsCoreGreenhousePatch),
                        nameof(AfterCanPlantTreesHere)));

            // Fruit Treeの設置前に
            // Vanillaの設置判定へ進めるためDiggableを一時付与
            harmony.Patch(
                original:
                    AccessTools.Method(
                        typeof(StardewValley.Object),
                        nameof(StardewValley.Object.placementAction),
                        new[]
                        {
                            typeof(GameLocation),
                            typeof(int),
                            typeof(int),
                            typeof(Farmer)
                        }),
                prefix:
                    new HarmonyMethod(
                        typeof(TsCoreGreenhousePatch),
                        nameof(BeforePlacementAction)),
                postfix:
                    new HarmonyMethod(
                        typeof(TsCoreGreenhousePatch),
                        nameof(AfterPlacementAction)),
                finalizer:
                    new HarmonyMethod(
                        typeof(TsCoreGreenhousePatch),
                        nameof(FinalizePlacementAction)));

            // TsCoreGreenhouseでは各足音判定前に
            // Default Footstepへ戻してTypeなしで前回音が残るのを防止
            harmony.Patch(
                original:
                    AccessTools.Method(
                        typeof(FarmerSprite),
                        "checkForFootstep"),
                prefix:
                    new HarmonyMethod(
                        typeof(TsCoreGreenhousePatch),
                        nameof(BeforeCheckForFootstep)));
        }

        //----------------------------------------
        // GameLocation.loadMap
        //----------------------------------------

        /// <summary>
        /// TsCoreGreenhouse Map PropertyがあるLocationを
        /// VanillaのGreenhouseとして扱います。
        /// </summary>
        private static void AfterLoadMap(
            GameLocation __instance)
        {
            if (TsCoreGreenhouseService.IsTsCoreGreenhouse(
                __instance))
            {
                __instance.IsGreenhouse = true;
            }
        }

        //----------------------------------------
        // GameLocation.CanPlantTreesHere
        //----------------------------------------

        /// <summary>
        /// TsCoreGreenhouseで実際にFruit Treeを設置する場合のみ、
        /// Map Propertyに指定されたBack Layer Type以外への植え付けを禁止します。
        /// 設置プレビュー時はVanillaのGreenhouse判定を維持します。
        /// </summary>
        private static void AfterCanPlantTreesHere(
            GameLocation __instance,
            string itemId,
            int tileX,
            int tileY,
            ref bool __result)
        {
            if (!__result
                || fruitTreePlacementDepth <= 0
                || !TsCoreGreenhouseService.IsTsCoreGreenhouse(
                    __instance)
                || !FruitTree.TryGetData(
                    itemId,
                    out _))
            {
                return;
            }

            if (!TsCoreGreenhouseService.IsFruitTreeTypeAllowed(
                __instance,
                tileX,
                tileY))
            {
                __result = false;
            }
        }

        //----------------------------------------
        // Object.placementAction
        //----------------------------------------

        /// <summary>
        /// TsCoreGreenhouseでFruit Treeを植えようとした場合、
        /// VanillaのCanPlantTreesHere判定まで進めるためDiggableを一時的に付与し、
        /// 実際の設置中だけTsCoreGreenhouseのType制限を有効にします。
        /// </summary>
        private static void BeforePlacementAction(
            StardewValley.Object __instance,
            GameLocation location,
            int x,
            int y,
            out PlacementState __state)
        {
            __state =
                new PlacementState();

            if (!TsCoreGreenhouseService.IsTsCoreGreenhouse(
                location)
                || !FruitTree.TryGetData(
                    __instance.ItemId,
                    out _))
            {
                return;
            }

            __state.RestrictFruitTreeType =
                true;

            fruitTreePlacementDepth++;

            int tileX =
                x / 64;

            int tileY =
                y / 64;

            if (location.doesTileHaveProperty(
                tileX,
                tileY,
                "Diggable",
                "Back") != null)
            {
                return;
            }

            Tile? tile =
                location.Map?.GetLayer("Back")?.Tiles[
                    tileX,
                    tileY];

            if (tile == null)
            {
                return;
            }

            tile.Properties["Diggable"] =
                "T";

            __state.Tile =
                tile;

            __state.AddedDiggable =
                true;
        }

        /// <summary>
        /// Fruit Tree設置判定後に一時追加したDiggableを戻します。
        /// </summary>
        private static void AfterPlacementAction(
            PlacementState __state)
        {
            RestoreTemporaryDiggable(
                __state);
        }

        /// <summary>
        /// 例外発生時にも一時追加したDiggableを戻します。
        /// </summary>
        private static Exception? FinalizePlacementAction(
            Exception? __exception,
            PlacementState __state)
        {
            RestoreTemporaryDiggable(
                __state);

            return __exception;
        }

        /// <summary>
        /// 一時追加したDiggable Tile Propertyを削除します。
        /// </summary>
        private static void RestoreTemporaryDiggable(
            PlacementState? state)
        {
            if (state == null)
            {
                return;
            }

            if (state.AddedDiggable
                && state.Tile != null)
            {
                state.Tile.Properties.Remove(
                    "Diggable");

                state.AddedDiggable =
                    false;
            }

            if (state.RestrictFruitTreeType)
            {
                fruitTreePlacementDepth =
                    Math.Max(
                        0,
                        fruitTreePlacementDepth - 1);

                state.RestrictFruitTreeType =
                    false;
            }
        }

        //----------------------------------------
        // FarmerSprite.checkForFootstep
        //----------------------------------------

        /// <summary>
        /// TsCoreGreenhouseではFootstep判定前にDefaultへ戻し、
        /// Type未設定Tileで直前の足音が残らないようにします。
        /// </summary>
        private static void BeforeCheckForFootstep(
            ref string ___currentStep)
        {
            if (TsCoreGreenhouseService.IsTsCoreGreenhouse(
                Game1.currentLocation))
            {
                ___currentStep =
                    "thudStep";
            }
        }
    }
}
