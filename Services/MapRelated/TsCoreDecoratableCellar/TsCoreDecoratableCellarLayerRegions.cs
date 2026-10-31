using StardewValley;

namespace Ts_Core.Services.MapRelated.TsCoreDecoratableCellar
{
    /// <summary>
    /// Cellarの壁領域をレイヤーごとに収集します。
    /// BackだけでなくBuildingsにもWallIDが設定されているマップに対応するための補助処理です。
    /// タイルの変更は行いません。
    /// </summary>
    internal static class TsCoreDecoratableCellarLayerRegions
    {
        //----------------------------------------
        // Layered Wall Regions
        //----------------------------------------

        /// <summary>
        /// 各WallIDに属する座標とレイヤー名を収集します。
        /// 同じ座標でもレイヤーが異なれば別のタイルとして保持します。
        /// </summary>
        internal static Dictionary<string, List<(int X, int Y, string Layer)>> ReadWallRegionLayers(GameLocation location)
        {
            var regions = new Dictionary<string, List<(int X, int Y, string Layer)>>(StringComparer.Ordinal);
            if (!TsCoreDecoratableCellarService.IsEnabled(location))
                return regions;

            foreach (string id in TsCoreDecoratableCellarService.ReadRegionDefaults(location, false).Keys)
                regions[id] = new List<(int X, int Y, string Layer)>();

            foreach (string layerName in new[] { "Back", "Buildings" })
            {
                var layer = location.Map?.GetLayer(layerName);
                if (layer == null)
                    continue;

                for (int y = 0; y < layer.LayerHeight; y++)
                {
                    for (int x = 0; x < layer.LayerWidth; x++)
                    {
                        string? id = location.doesTileHaveProperty(x, y, "WallID", layerName);
                        if (string.IsNullOrWhiteSpace(id))
                            continue;

                        if (!regions.TryGetValue(id, out var tiles))
                        {
                            tiles = new List<(int X, int Y, string Layer)>();
                            regions[id] = tiles;
                        }

                        tiles.Add((x, y, layerName));
                    }
                }
            }

            return regions;
        }
    }
}
