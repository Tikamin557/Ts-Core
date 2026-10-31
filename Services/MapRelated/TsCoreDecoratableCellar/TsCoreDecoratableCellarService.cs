using StardewValley;
using StardewValley.Locations;

namespace Ts_Core.Services.MapRelated.TsCoreDecoratableCellar
{
    /// <summary>
    /// Cellarの装飾機能を有効化するマッププロパティと、
    /// バニラ互換の床・壁領域定義を読み取ります。
    /// この段階ではアイテム適用やタイルの書き換えは行いません。
    /// </summary>
    internal static class TsCoreDecoratableCellarService
    {
        private const string MapPropertyName = "TsCoreDecoratableCellar";

        //----------------------------------------
        // Map Property
        //----------------------------------------

        /// <summary>
        /// マッププロパティがTであるCellarのみを対象とします。
        /// </summary>
        internal static bool IsEnabled(GameLocation? location)
        {
            if (location is not Cellar cellar
                || cellar.Map == null
                || !cellar.Map.Properties.TryGetValue(MapPropertyName, out var property))
            {
                return false;
            }

            return string.Equals(property?.ToString()?.Trim(), "T", StringComparison.OrdinalIgnoreCase);
        }

        //----------------------------------------
        // Tile Regions
        //----------------------------------------

        /// <summary>
        /// BackレイヤーのFloorIDを取得します。
        /// </summary>
        internal static string? GetFloorRegionId(GameLocation location, int tileX, int tileY)
        {
            return location.doesTileHaveProperty(tileX, tileY, "FloorID", "Back");
        }

        /// <summary>
        /// BackレイヤーのWallIDを取得します。
        /// </summary>
        internal static string? GetWallRegionId(GameLocation location, int tileX, int tileY)
        {
            return location.doesTileHaveProperty(tileX, tileY, "WallID", "Back");
        }

        //----------------------------------------
        // Default Materials
        //----------------------------------------

        /// <summary>
        /// FloorIDs / WallIDsを「領域ID 初期素材ID」の組として解析します。
        /// 初期素材IDのない領域も登録します。
        /// </summary>
        internal static Dictionary<string, string?> ReadRegionDefaults(GameLocation location, bool isFloor)
        {
            var regions = new Dictionary<string, string?>(StringComparer.Ordinal);
            string key = isFloor ? "FloorIDs" : "WallIDs";
            if (location.Map == null || !location.Map.Properties.TryGetValue(key, out var property))
                return regions;

            string value = property?.ToString() ?? string.Empty;
            foreach (string entry in value.Split(','))
            {
                string[] parts = entry.Split((char[]?)null, 2, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0)
                    continue;

                regions[parts[0]] = parts.Length > 1 ? parts[1].Trim() : null;
            }

            return regions;
        }

        //----------------------------------------
        // Region Snapshot
        //----------------------------------------

        /// <summary>
        /// 現在のマップから領域IDごとのタイル座標を収集します。
        /// 初期素材の指定だけが存在する領域も含めます。
        /// </summary>
        internal static Dictionary<string, List<(int X, int Y)>> ReadRegionTiles(GameLocation location, bool isFloor)
        {
            var regions = new Dictionary<string, List<(int X, int Y)>>(StringComparer.Ordinal);
            if (!IsEnabled(location))
                return regions;

            foreach (string id in ReadRegionDefaults(location, isFloor).Keys)
                regions[id] = new List<(int X, int Y)>();

            var layer = location.Map?.GetLayer("Back");
            if (layer == null)
                return regions;

            string propertyName = isFloor ? "FloorID" : "WallID";
            for (int y = 0; y < layer.LayerHeight; y++)
            {
                for (int x = 0; x < layer.LayerWidth; x++)
                {
                    string? id = location.doesTileHaveProperty(x, y, propertyName, "Back");
                    if (string.IsNullOrWhiteSpace(id))
                        continue;

                    if (!regions.TryGetValue(id, out var tiles))
                    {
                        tiles = new List<(int X, int Y)>();
                        regions[id] = tiles;
                    }
                    tiles.Add((x, y));
                }
            }

            return regions;
        }

        //----------------------------------------
        // Region Lookup
        //----------------------------------------

        /// <summary>
        /// マップのBackレイヤーを走査し、指定した床または壁の領域に属する座標を取得します。
        /// Content Patcherによるマップ拡張後の現在のマップを参照します。
        /// </summary>
        internal static List<(int X, int Y)> GetRegionTiles(GameLocation location, string regionId, bool isFloor)
        {
            var result = new List<(int X, int Y)>();
            if (!IsEnabled(location) || string.IsNullOrWhiteSpace(regionId))
                return result;

            var layer = location.Map?.GetLayer("Back");
            if (layer == null)
                return result;

            string propertyName = isFloor ? "FloorID" : "WallID";
            for (int y = 0; y < layer.LayerHeight; y++)
            {
                for (int x = 0; x < layer.LayerWidth; x++)
                {
                    string? value = location.doesTileHaveProperty(x, y, propertyName, "Back");
                    if (string.Equals(value, regionId, StringComparison.Ordinal))
                        result.Add((x, y));
                }
            }

            return result;
        }
    }
}
