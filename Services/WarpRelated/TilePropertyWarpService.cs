using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
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
            // Tile検索
            //----------------------------------------

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
                                $"Tile Property Warp: '{PropertyName}={value}' in location '{locationName}' has an invalid OffsetX. The property will be ignored.",
                                LogLevel.Warn);

                            continue;
                        }

                        if (parts.Length >= 3
                            && !int.TryParse(
                                parts[2],
                                out offsetY))
                        {
                            Monitor?.Log(
                                $"Tile Property Warp: '{PropertyName}={value}' in location '{locationName}' has an invalid OffsetY. The property will be ignored.",
                                LogLevel.Warn);

                            continue;
                        }

                        if (parts.Length > 3)
                        {
                            Monitor?.Log(
                                $"Tile Property Warp: '{PropertyName}={value}' in location '{locationName}' has too many values. The property will be ignored.",
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
                                $"Tile Property Warp: '{PropertyName}={uniqueKey}' is duplicated in location '{locationName}'. The first matching tile will be used.",
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
