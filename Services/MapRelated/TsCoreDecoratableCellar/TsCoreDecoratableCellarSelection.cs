using StardewValley;

namespace Ts_Core.Services.MapRelated.TsCoreDecoratableCellar
{
    /// <summary>
    /// Cellarの領域を確認し、張り替え素材の選択状態を保存します。
    /// 実際のタイル更新は行わないため、アイテム使用パッチからは
    /// 描画処理が成功した後にのみ呼び出してください。
    /// </summary>
    internal static class TsCoreDecoratableCellarSelection
    {
        //----------------------------------------
        // Selection
        //----------------------------------------

        /// <summary>
        /// 指定座標が有効な領域に属している場合、その領域IDを返します。
        /// </summary>
        internal static bool TryGetRegion(GameLocation location, int tileX, int tileY, bool isFloor, out string regionId)
        {
            regionId = string.Empty;
            if (!TsCoreDecoratableCellarService.IsEnabled(location))
                return false;

            string? id = isFloor
                ? TsCoreDecoratableCellarService.GetFloorRegionId(location, tileX, tileY)
                : TsCoreDecoratableCellarService.GetWallRegionId(location, tileX, tileY)
                    ?? location.doesTileHaveProperty(tileX, tileY, "WallID", "Buildings");

            if (string.IsNullOrWhiteSpace(id))
                return false;

            regionId = id;
            return true;
        }

        /// <summary>
        /// 描画成功後に呼び出し、指定領域に適用した素材IDを記録します。
        /// 存在しない領域や無効な素材IDは保存しません。
        /// </summary>
        internal static bool RecordAppliedMaterial(GameLocation location, string regionId, string materialId, bool isFloor)
        {
            if (!TsCoreDecoratableCellarService.IsEnabled(location)
                || string.IsNullOrWhiteSpace(regionId)
                || string.IsNullOrWhiteSpace(materialId))
                return false;

            bool regionExists;
            if (isFloor)
            {
                var regions = TsCoreDecoratableCellarService.ReadRegionTiles(location, true);
                regionExists = regions.TryGetValue(regionId, out var tiles) && tiles.Count > 0;
            }
            else
            {
                var regions = TsCoreDecoratableCellarLayerRegions.ReadWallRegionLayers(location);
                regionExists = regions.TryGetValue(regionId, out var tiles) && tiles.Count > 0;
            }

            if (!regionExists)
                return false;

            TsCoreDecoratableCellarState.Write(location, regionId, materialId, isFloor);
            return true;
        }
    }
}
