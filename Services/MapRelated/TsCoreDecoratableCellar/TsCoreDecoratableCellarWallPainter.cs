using StardewValley;
using xTile.Tiles;

namespace Ts_Core.Services.MapRelated.TsCoreDecoratableCellar
{
    /// <summary>
    /// CellarのWallIDが指定された起点から、壁紙をBackの2段とBuildingsの1段へ適用します。
    /// 元のCellar型と壁以外のロケーション状態は変更しません。
    /// </summary>
    internal static class TsCoreDecoratableCellarWallPainter
    {
        //----------------------------------------
        // Wallpaper Application
        //----------------------------------------

        /// <summary>
        /// WallIDのあるBackタイルを上段とし、直下のBackとその直下のBuildingsを描き替えます。
        /// 適用できないタイルは個別にスキップし、他の壁タイルへの適用を継続します。
        /// タイルの更新にはバニラと同じGameLocation.setMapTileを使用します。
        /// </summary>
        internal static bool TryApplyWallpaperTiles(GameLocation location, string regionId,
            string tileSheetId, int sourceIndex)
        {
            if (!TsCoreDecoratableCellarService.IsEnabled(location) || sourceIndex < 0)
                return false;

            var map = location.Map;
            var sheet = map?.GetTileSheet(tileSheetId);
            var back = map?.GetLayer("Back");
            var buildings = map?.GetLayer("Buildings");
            if (sheet == null || sheet.SheetWidth <= 0 || back == null || buildings == null)
                return false;

            var regions = TsCoreDecoratableCellarLayerRegions.ReadWallRegionLayers(location);
            if (!regions.TryGetValue(regionId, out var anchors) || anchors.Count == 0)
                return false;

            var replacements = new Dictionary<(string Layer, int X, int Y),
                (int X, int Y, int Index, string LayerName)>();

            foreach (var (x, y, layerName) in anchors)
            {
                // WallIDを付けたBackのタイルが壁の最上段です。
                // Buildings側に明示されたWallIDは、ここでは起点にしません。
                if (layerName != "Back")
                    continue;

                for (int row = 0; row < 3; row++)
                {
                    var layer = row == 2 ? buildings : back;
                    int targetY = y + row;
                    if (x < 0 || x >= layer.LayerWidth || targetY < 0 || targetY >= layer.LayerHeight
                        || layer.Tiles[x, targetY] is not StaticTile original)
                        continue;

                    // 別の壁領域に属するタイルは上書きしません。
                    string? otherId = location.doesTileHaveProperty(x, targetY, "WallID", layer.Id);
                    if (!string.IsNullOrEmpty(otherId) && !string.Equals(otherId, regionId, StringComparison.Ordinal))
                        continue;

                    if (!TsCoreDecoratableCellarWallpaperIndex.TryGetTileIndex(
                        sourceIndex, sheet.SheetWidth, row, out int index) || index >= sheet.TileCount)
                        continue;

                    var key = (layer.Id, x, targetY);
                    // 複数の起点が同じ座標を指す場合は、先に登録した置換を優先します。
                    replacements.TryAdd(key, (x, targetY, index, layer.Id));
                }
            }

            if (replacements.Count == 0)
                return false;

            // バニラのDecoratableLocation.UpdateWallpaperと同じsetMapTileを使用します。
            // 同じシートならTileIndexのみ変更し、異なるシートならプロパティをコピーします。
            var updates = new List<TsCoreDecoratableCellarTileTransaction.Replacement>();
            foreach (var item in replacements.Values)
                updates.Add(new(item.X, item.Y, item.Index, item.LayerName));
            return TsCoreDecoratableCellarTileTransaction.TryApply(location, tileSheetId, updates);
        }

        /// <summary>
        /// 壁紙素材IDを解決して、対象の壁領域に適用します。
        /// </summary>
        internal static bool TryApplyWallpaperMaterial(GameLocation location, string regionId, string materialId)
        {
            return TsCoreDecoratableCellarWallpaperSource.TryResolve(location, materialId,
                    out string tileSheetId, out int sourceIndex)
                && TryApplyWallpaperTiles(location, regionId, tileSheetId, sourceIndex);
        }
    }
}
