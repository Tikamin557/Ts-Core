using StardewValley;
using xTile.Tiles;

namespace Ts_Core.Services.MapRelated.TsCoreDecoratableCellar
{
    /// <summary>
    /// Cellarの領域に対応するタイルを変更するための描画補助処理です。
    /// 既存タイルのプロパティを保持しながら、静的タイルを差し替えます。
    /// </summary>
    internal static class TsCoreDecoratableCellarTilePainter
    {
        //----------------------------------------
        // Floor Region Application
        //----------------------------------------

        /// <summary>
        /// 指定した床領域に2×2パターンを適用します。
        /// 呼び出し側で床材のタイルシートIDとパターン開始位置を解決する必要があります。
        /// 全タイルを事前検証し、対象外のタイルがある場合は変更しません。
        /// </summary>
        internal static bool TryApplyFloorRegion(GameLocation location, string regionId, string tileSheetId, int patternStartIndex)
        {
            if (!TsCoreDecoratableCellarService.IsEnabled(location)
                || string.IsNullOrWhiteSpace(regionId)
                || string.IsNullOrWhiteSpace(tileSheetId)
                || patternStartIndex < 0)
                return false;

            var map = location.Map;
            var layer = map?.GetLayer("Back");
            var sheet = map?.GetTileSheet(tileSheetId);
            if (layer == null || sheet == null || sheet.SheetWidth < 2)
                return false;

            var regionTiles = TsCoreDecoratableCellarService.GetRegionTiles(location, regionId, true);
            if (regionTiles.Count == 0)
                return false;

            // 2×2模様はマップ全体ではなく、FloorID領域の左上を基準に配置します。
            // 領域が奇数座標から始まる場合でも、意図した模様の向きを維持します。
            int originX = int.MaxValue;
            int originY = int.MaxValue;
            foreach (var (x, y) in regionTiles)
            {
                originX = Math.Min(originX, x);
                originY = Math.Min(originY, y);
            }

            // バニラUpdateFloorと同様、適用できないタイルは個別にスキップします。
            // Stage38で確認した領域左上基準の2×2配置だけは維持します。
            var updates = new List<TsCoreDecoratableCellarTileTransaction.Replacement>();
            foreach (var (x, y) in regionTiles)
            {
                if (x < 0 || y < 0 || x >= layer.LayerWidth || y >= layer.LayerHeight
                    || layer.Tiles[x, y] is not StaticTile original || original.TileSheet == null)
                    continue;

                string originalSheetId = original.TileSheet.Id;
                // DecoratableLocation.IsWallAndFloorTilesheetと同じID判定です。
                if (!originalSheetId.Contains("walls_and_floors", StringComparison.Ordinal)
                    && !originalSheetId.StartsWith("x_WallsAndFloors_", StringComparison.Ordinal))
                    continue;

                // 追加素材のシート幅が元の床シートと異なる場合も、
                // 張り替え先の2×2パターンを正しく参照します。
                int targetWidth = sheet.SheetWidth;
                long index = (long)patternStartIndex
                    + (x - originX) % 2
                    + (long)targetWidth * ((y - originY) % 2);
                if (index < 0 || index >= sheet.TileCount)
                    continue;

                // バニラUpdateFloorと同じタイル更新APIを使用します。
                updates.Add(new(x, y, (int)index, "Back"));
            }

            return TsCoreDecoratableCellarTileTransaction.TryApply(location, tileSheetId, updates);
        }

        /// <summary>
        /// 床材の数値インデックスからパターン開始位置を計算し、指定領域へ適用します。
        /// タイルシートの選択と最初の床材タイル位置は、呼び出し元が
        /// DecoratableLocationと同じ規則で解決する必要があります。
        /// </summary>
        internal static bool TryApplyFloorMaterial(GameLocation location, string regionId, string tileSheetId,
            int materialIndex, int firstFlooringTile)
        {
            if (!TsCoreDecoratableCellarService.IsEnabled(location)
                || string.IsNullOrWhiteSpace(tileSheetId))
                return false;

            var sheet = location.Map?.GetTileSheet(tileSheetId);
            if (sheet == null
                || !TryGetFloorPatternStartIndex(materialIndex, sheet.SheetWidth,
                    firstFlooringTile, out int patternStartIndex))
                return false;

            return TryApplyFloorRegion(location, regionId, tileSheetId, patternStartIndex);
        }

        //----------------------------------------
        // Floor Source Resolution
        //----------------------------------------

        /// <summary>
        /// DecoratableLocation.GetFloorSourceの素材ID形式を解釈します。
        /// 数値のみはバニラのwalls_and_floorsシートを参照し、
        /// 「シート名:番号」は追加素材のシートを参照し、未登録なら登録を試みます。
        /// </summary>
        internal static bool TryResolveFloorSource(GameLocation location, string materialId,
            out string tileSheetId, out int materialIndex, out int firstFlooringTile)
        {
            tileSheetId = string.Empty;
            materialIndex = -1;
            firstFlooringTile = 0;
            if (!TsCoreDecoratableCellarService.IsEnabled(location)
                || string.IsNullOrWhiteSpace(materialId))
                return false;

            int separator = materialId.IndexOf(':');
            if (separator < 0)
            {
                if (!int.TryParse(materialId, out materialIndex) || materialIndex < 0)
                    return false;
                tileSheetId = "walls_and_floors";
                firstFlooringTile = 336;
            }
            else
            {
                string sheetName = materialId.Substring(0, separator);
                string indexText = materialId.Substring(separator + 1);
                if (string.IsNullOrWhiteSpace(sheetName)
                    || !int.TryParse(indexText, out materialIndex) || materialIndex < 0)
                    return false;
                // 追加素材セットはバニラが使用する接頭辞付きのタイルシートを優先します。
                // 既存のマップで素材セットIDそのものをシート名にしている場合も維持します。
                string generatedSheetId = "x_WallsAndFloors_" + sheetName;
                tileSheetId = location.Map?.GetTileSheet(generatedSheetId) != null
                    ? generatedSheetId
                    : sheetName;
                if (location.Map?.GetTileSheet(tileSheetId) == null
                    && !TsCoreDecoratableCellarAdditionalTilesheets.TryResolve(location, sheetName, out tileSheetId))
                    return false;
            }

            // CPでバニラシートのIDが変更された場合も、壁紙と同じ画像参照先で解決します。
            // 追加素材の登録方式は現段階では変更しません。
            if (separator < 0 && location.Map?.GetTileSheet(tileSheetId) == null)
            {
                if (!TsCoreDecoratableCellarWallpaperSource.TryResolve(location, materialId,
                    out string resolvedSheetId, out _))
                    return false;
                tileSheetId = resolvedSheetId;
            }

            return location.Map?.GetTileSheet(tileSheetId) != null;
        }

        /// <summary>
        /// 保存された床材IDを解決し、指定された床領域へ適用します。
        /// タイルシートがマップに登録されていない場合は変更しません。
        /// </summary>
        internal static bool TryApplyFloorMaterial(GameLocation location, string regionId, string materialId)
        {
            if (string.IsNullOrWhiteSpace(regionId)
                || !TryResolveFloorSource(location, materialId,
                    out string tileSheetId, out int materialIndex, out int firstFlooringTile))
                return false;

            return TryApplyFloorMaterial(location, regionId, tileSheetId, materialIndex, firstFlooringTile);
        }

        //----------------------------------------
        // Floor Tile Index
        //----------------------------------------

        /// <summary>
        /// DecoratableLocation.UpdateFloorで使われる、床材パターンの先頭インデックスを計算します。
        /// タイルシート幅はタイル単位です。壁と床の個別形状によるオフセットは含みません。
        /// </summary>
        internal static bool TryGetFloorPatternStartIndex(int sourceIndex, int tileSheetWidth, int firstFlooringTile, out int tileIndex)
        {
            tileIndex = -1;
            if (sourceIndex < 0 || tileSheetWidth < 2)
                return false;

            int patternsPerRow = tileSheetWidth / 2;
            if (patternsPerRow == 0)
                return false;

            long result = (long)sourceIndex * 2
                + (long)(sourceIndex / patternsPerRow) * tileSheetWidth
                + firstFlooringTile;
            if (result < 0 || result > int.MaxValue)
                return false;

            tileIndex = (int)result;
            return true;
        }
    }
}
