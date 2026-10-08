using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using xTile.Layers;
using xTile.Tiles;

namespace Ts_Core.Services.WarpRelated
{
    /// <summary>
    /// Mapの各レイヤーに設定されたTsCoreWarpPointから
    /// Warp先のTile座標を解決するサービスです。
    /// </summary>
    public static class TilePropertyWarpService
    {
        //----------------------------------------
        // Tile Property
        //----------------------------------------

        private const string PropertyName =
            "TsCoreWarpPoint";

        //----------------------------------------
        // Monitor
        //----------------------------------------

        private static IMonitor? Monitor;

        //----------------------------------------
        // 初期化
        //----------------------------------------

        public static void Initialize(IMonitor monitor)
        {
            Monitor = monitor;
        }

        //----------------------------------------
        // Warp先解決
        //----------------------------------------

        /// <summary>
        /// 指定Locationの全レイヤーからTsCoreWarpPointを検索し、
        /// 一致するTile座標にOffsetを加えたWarp先を取得します。
        /// Property値は「UniqueKey [OffsetX] [OffsetY]」形式です。
        /// </summary>
        public static bool TryResolve(
            string locationName,
            string uniqueKey,
            out Point point)
        {
            point = default;

            //----------------------------------------
            // Location取得
            //----------------------------------------

            GameLocation? location =
                Game1.getLocationFromName(
                    locationName);

            if (location?.Map == null)
                return false;

            //----------------------------------------
            // 現在のMapから検索
            //----------------------------------------

            if (TryResolveFromMap(
                    location,
                    uniqueKey,
                    out point))
            {
                return true;
            }

            //----------------------------------------
            // FarmHouse初期Map対策
            //----------------------------------------

            // FarmHouse以外のLocationからセーブを再開した直後は、
            // FarmHouseが現在のUpgrade/Renovation状態へ更新される前の
            // Mapを保持している場合がある。
            // 通常の入室時にも呼ばれるupdateFarmLayoutでMap構成を
            // 更新してから、同じPropertyをもう一度検索する。
            if (location is FarmHouse farmHouse)
            {
                farmHouse.updateFarmLayout();

                if (farmHouse.Map != null
                    && TryResolveFromMap(
                        farmHouse,
                        uniqueKey,
                        out point))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 現在Locationに読み込まれているMapから
        /// TsCoreWarpPointを検索します。
        /// </summary>
        private static bool TryResolveFromMap(
            GameLocation location,
            string uniqueKey,
            out Point point)
        {
            point = default;

            if (location.Map == null)
                return false;

            Point? foundPoint = null;

            foreach (Layer layer in location.Map.Layers)
            {
                for (int y = 0;
                    y < layer.LayerHeight;
                    y++)
                {
                    for (int x = 0;
                        x < layer.LayerWidth;
                        x++)
                    {
                        Tile? tile =
                            layer.Tiles[x, y];

                        if (tile == null
                            || !tile.Properties.TryGetValue(
                                PropertyName,
                                out var propertyValue))
                        {
                            continue;
                        }

                        string value =
                            propertyValue?.ToString()?.Trim()
                            ?? string.Empty;

                        string[] parts =
                            value.Split(
                                ' ',
                                StringSplitOptions.RemoveEmptyEntries);

                        if (parts.Length == 0
                            || !string.Equals(
                                parts[0],
                                uniqueKey,
                                StringComparison.Ordinal))
                        {
                            continue;
                        }

                        //----------------------------------------
                        // Offset取得
                        //----------------------------------------

                        int offsetX = 0;
                        int offsetY = 0;

                        if (parts.Length >= 2
                            && !int.TryParse(
                                parts[1],
                                out offsetX))
                        {
                            Monitor?.Log(
                                $"Tile Property Warp: '{PropertyName}={value}' in location '{location.NameOrUniqueName}' has an invalid OffsetX. The property will be ignored.",
                                LogLevel.Warn);

                            continue;
                        }

                        if (parts.Length >= 3
                            && !int.TryParse(
                                parts[2],
                                out offsetY))
                        {
                            Monitor?.Log(
                                $"Tile Property Warp: '{PropertyName}={value}' in location '{location.NameOrUniqueName}' has an invalid OffsetY. The property will be ignored.",
                                LogLevel.Warn);

                            continue;
                        }

                        if (parts.Length > 3)
                        {
                            Monitor?.Log(
                                $"Tile Property Warp: '{PropertyName}={value}' in location '{location.NameOrUniqueName}' has too many values. The property will be ignored.",
                                LogLevel.Warn);

                            continue;
                        }

                        Point warpPoint =
                            new Point(
                                x + offsetX,
                                y + offsetY);

                        //----------------------------------------
                        // 重複確認
                        //----------------------------------------

                        if (foundPoint.HasValue)
                        {
                            Monitor?.Log(
                                $"Tile Property Warp: '{PropertyName}={uniqueKey}' is duplicated in location '{location.NameOrUniqueName}'. The first matching tile will be used.",
                                LogLevel.Warn);

                            point =
                                foundPoint.Value;

                            return true;
                        }

                        foundPoint =
                            warpPoint;
                    }
                }
            }

            if (!foundPoint.HasValue)
                return false;

            point =
                foundPoint.Value;

            return true;
        }

    }
}
