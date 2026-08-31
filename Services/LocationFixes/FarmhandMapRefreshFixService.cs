using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;

namespace Ts_Core.Services.LocationFixes
{
    /// <summary>
    /// Farmhandで現在使用中のGameLocationが
    /// SMAPIのMap伝播対象から外れる場合に、
    /// 現在地のMapを補完更新します。
    /// </summary>
    internal static class FarmhandMapRefreshFixService
    {
        //----------------------------------------
        // サービス
        //----------------------------------------

        private static IModHelper? Helper;
        private static IMonitor? Monitor;

        //----------------------------------------
        // 更新予約
        //----------------------------------------

        private static GameLocation? PendingLocation;

        private static Building? PendingParentBuilding;

        //----------------------------------------
        // 初期化
        //----------------------------------------

        internal static void Initialize(
            IModHelper helper,
            IMonitor monitor)
        {
            Helper = helper;
            Monitor = monitor;

            helper.Events.Content.AssetsInvalidated
                += OnAssetsInvalidated;

            helper.Events.GameLoop.UpdateTicked
                += OnUpdateTicked;

            helper.Events.GameLoop.ReturnedToTitle
                += OnReturnedToTitle;
        }

        //----------------------------------------
        // Asset無効化
        //----------------------------------------

        private static void OnAssetsInvalidated(
            object? sender,
            AssetsInvalidatedEventArgs e)
        {
            //----------------------------------------
            // Farmhandのみ対象
            //----------------------------------------

            if (Context.IsMainPlayer)
                return;

            if (!Context.IsWorldReady)
                return;

            //----------------------------------------
            // 現在地取得
            //----------------------------------------

            GameLocation? current =
                Game1.currentLocation;

            if (current == null)
                return;

            string? mapPath =
                current.mapPath.Value;

            if (string.IsNullOrWhiteSpace(
                    mapPath))
            {
                return;
            }

            //----------------------------------------
            // 現在地のMap Assetが今回の
            // Invalidated対象か確認
            //----------------------------------------

            bool mapInvalidated =
                e.NamesWithoutLocale.Any(
                    name =>
                        name.IsEquivalentTo(
                            mapPath));

            if (!mapInvalidated)
                return;

            //----------------------------------------
            // SMAPIが認識している同名Locationと
            // ParentBuildingを検索
            //----------------------------------------

            LocationInfo? registered =
                FindRegisteredLocation(
                    current);

            if (registered == null)
                return;

            //----------------------------------------
            // 同一インスタンスならSMAPI自身の
            // Map伝播で更新されるため補完不要
            //----------------------------------------

            if (ReferenceEquals(
                    registered.Location,
                    current))
            {
                return;
            }

            //----------------------------------------
            // 現在地を補完更新対象として予約
            //
            // AssetsInvalidatedイベント終了後に
            // SMAPIのCoreAssetPropagatorが動くため、
            // 実際の更新はUpdateTickedで行う
            //----------------------------------------

            PendingLocation =
                current;

            PendingParentBuilding =
                registered.ParentBuilding;

            Monitor?.Log(
                $"[FarmhandMapRefreshFix] " +
                $"Scheduled map refresh: " +
                $"Location={current.NameOrUniqueName}, " +
                $"MapPath={mapPath}, " +
                $"CurrentId={current.GetHashCode()}, " +
                $"RegisteredId={registered.Location.GetHashCode()}, " +
                $"ParentBuilding=" +
                $"{registered.ParentBuilding?.buildingType.Value ?? "None"}",
                LogLevel.Trace);
        }

        //----------------------------------------
        // SMAPIが認識しているLocationを検索
        //----------------------------------------

        private static LocationInfo?
            FindRegisteredLocation(
                GameLocation current)
        {
            //----------------------------------------
            // Game1.locations
            //----------------------------------------

            foreach (GameLocation location
                in Game1.locations)
            {
                if (IsSameLocation(
                        location,
                        current))
                {
                    return new LocationInfo(
                        location,
                        null);
                }
            }

            //----------------------------------------
            // SaveGame.loaded.locations
            //----------------------------------------

            if (SaveGame.loaded?.locations
                != null)
            {
                foreach (GameLocation location
                    in SaveGame.loaded.locations)
                {
                    if (IsSameLocation(
                            location,
                            current))
                    {
                        return new LocationInfo(
                            location,
                            null);
                    }
                }
            }

            //----------------------------------------
            // Building interiors
            //
            // SMAPI CoreAssetPropagatorの
            // GetLocationsWithInfoと同じ範囲を検索
            //----------------------------------------

            foreach (GameLocation location
                in GetBaseLocations())
            {
                foreach (Building building
                    in location.buildings)
                {
                    GameLocation? interior =
                        building.indoors.Value;

                    if (interior == null)
                        continue;

                    if (!IsSameLocation(
                            interior,
                            current))
                    {
                        continue;
                    }

                    return new LocationInfo(
                        interior,
                        building);
                }
            }

            return null;
        }

        //----------------------------------------
        // 基準Location一覧
        //----------------------------------------

        private static IEnumerable<GameLocation>
            GetBaseLocations()
        {
            foreach (GameLocation location
                in Game1.locations)
            {
                yield return location;
            }

            if (SaveGame.loaded?.locations
                == null)
            {
                yield break;
            }

            foreach (GameLocation location
                in SaveGame.loaded.locations)
            {
                yield return location;
            }
        }

        //----------------------------------------
        // Location一致判定
        //----------------------------------------

        private static bool IsSameLocation(
            GameLocation first,
            GameLocation second)
        {
            return string.Equals(
                first.NameOrUniqueName,
                second.NameOrUniqueName,
                StringComparison.OrdinalIgnoreCase);
        }

        //----------------------------------------
        // 保留中Map更新
        //----------------------------------------

        private static void OnUpdateTicked(
            object? sender,
            UpdateTickedEventArgs e)
        {
            if (PendingLocation == null)
                return;

            //----------------------------------------
            // 予約を先に解除
            //----------------------------------------

            GameLocation location =
                PendingLocation;

            Building? parentBuilding =
                PendingParentBuilding;

            PendingLocation = null;
            PendingParentBuilding = null;

            //----------------------------------------
            // 予約後に別Locationへ移動していた場合は
            // 更新しない
            //----------------------------------------

            if (!ReferenceEquals(
                    Game1.currentLocation,
                    location))
            {
                return;
            }

            try
            {
                RefreshLocationMap(
                    location,
                    parentBuilding);

                Monitor?.Log(
                    $"[FarmhandMapRefreshFix] " +
                    $"Current map refreshed: " +
                    $"{location.NameOrUniqueName}",
                    LogLevel.Trace);
            }
            catch (Exception ex)
            {
                Monitor?.Log(
                    $"Failed refreshing the farmhand's " +
                    $"current map " +
                    $"'{location.NameOrUniqueName}'.\n{ex}",
                    LogLevel.Error);
            }
        }

        //----------------------------------------
        // Map補完更新
        //----------------------------------------

        private static void RefreshLocationMap(
            GameLocation location,
            Building? parentBuilding)
        {
            //----------------------------------------
            // プレイヤー位置保存
            //
            // SMAPI CoreAssetPropagator.UpdateMapと
            // 同様にMap更新による座標変更を防ぐ
            //----------------------------------------

            Vector2? playerPosition =
                Game1.player?.Position;

            //----------------------------------------
            // Map再読み込み
            //----------------------------------------

            location.interiorDoors.Clear();

            location.reloadMap();

            //----------------------------------------
            // Door / Warp更新
            //----------------------------------------

            location.interiorDoors.Clear();

            location.interiorDoors
                .ResetSharedState();

            location.interiorDoors
                .ResetLocalState();

            location.updateWarps();

            location.updateDoors();

            //----------------------------------------
            // Building内部Warp更新
            //
            // SMAPI CoreAssetPropagator.UpdateMapの
            // ParentBuilding.updateInteriorWarps()
            // に相当
            //----------------------------------------

            parentBuilding?
                .updateInteriorWarps(
                    location);

            //----------------------------------------
            // FarmHouse
            //----------------------------------------

            if (location is FarmHouse)
            {
                Helper?.Reflection
                    .GetField<bool>(
                        location,
                        "displayingSpouseRoom")
                    .SetValue(false);
            }

            //----------------------------------------
            // Map固有変更を再適用
            //----------------------------------------

            location.MakeMapModifications(
                force: true);

            //----------------------------------------
            // 冷蔵庫位置更新
            //----------------------------------------

            if (location
                is FarmHouse farmHouse)
            {
                farmHouse.fridgePosition =
                    farmHouse
                        .GetFridgePositionFromMap()
                    ?? Point.Zero;
            }
            else if (
                location
                    is IslandFarmHouse islandFarmHouse)
            {
                islandFarmHouse.fridgePosition =
                    islandFarmHouse
                        .GetFridgePositionFromMap()
                    ?? Point.Zero;
            }

            //----------------------------------------
            // プレイヤー位置復元
            //----------------------------------------

            if (playerPosition.HasValue
                && Game1.player != null)
            {
                Game1.player.Position =
                    playerPosition.Value;
            }
        }

        //----------------------------------------
        // タイトルへ戻った時
        //----------------------------------------

        private static void OnReturnedToTitle(
            object? sender,
            ReturnedToTitleEventArgs e)
        {
            PendingLocation = null;
            PendingParentBuilding = null;
        }

        //----------------------------------------
        // Location情報
        //----------------------------------------

        private sealed class LocationInfo
        {
            internal GameLocation Location
            {
                get;
            }

            internal Building? ParentBuilding
            {
                get;
            }

            internal LocationInfo(
                GameLocation location,
                Building? parentBuilding)
            {
                Location =
                    location;

                ParentBuilding =
                    parentBuilding;
            }
        }
    }
}