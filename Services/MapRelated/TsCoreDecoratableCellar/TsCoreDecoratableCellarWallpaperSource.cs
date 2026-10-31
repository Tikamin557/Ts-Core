using StardewValley;
using xTile.Tiles;

namespace Ts_Core.Services.MapRelated.TsCoreDecoratableCellar
{
    /// <summary>
    /// 壁紙の保存IDを、マップに登録済みのタイルシートと素材番号へ変換します。
    /// 未登録の追加素材は定義を確認してタイルシートの登録を試みます。
    /// </summary>
    internal static class TsCoreDecoratableCellarWallpaperSource
    {
        //----------------------------------------
        // Wallpaper Source Resolution
        //----------------------------------------

        /// <summary>
        /// バニラのGetWallpaperSourceと同じID形式（番号または素材ID:番号）を解釈します。
        /// 追加素材はゲームが使用するx_WallsAndFloors_接頭辞のシートを優先します。
        /// </summary>
        internal static bool TryResolve(GameLocation location, string materialId,
            out string tileSheetId, out int sourceIndex)
        {
            tileSheetId = string.Empty;
            sourceIndex = -1;
            if (!TsCoreDecoratableCellarService.IsEnabled(location)
                || string.IsNullOrWhiteSpace(materialId) || location.Map == null)
                return false;

            int separator = materialId.IndexOf(':');
            if (separator < 0)
            {
                if (!int.TryParse(materialId, out sourceIndex) || sourceIndex < 0)
                    return false;
                tileSheetId = "walls_and_floors";
            }
            else
            {
                string sourceId = materialId.Substring(0, separator);
                string indexText = materialId.Substring(separator + 1);
                if (string.IsNullOrWhiteSpace(sourceId)
                    || !int.TryParse(indexText, out sourceIndex) || sourceIndex < 0)
                    return false;

                string generatedSheetId = "x_WallsAndFloors_" + sourceId;
                // 未登録の追加素材を別名のシートへ誤適用しないようにします。
                tileSheetId = generatedSheetId;
                if (location.Map.GetTileSheet(tileSheetId) == null
                    && !TsCoreDecoratableCellarAdditionalTilesheets.TryResolve(location, sourceId, out tileSheetId))
                    return false;
            }

            if (location.Map.GetTileSheet(tileSheetId) != null)
                return true;

            // CPなどでタイルシートIDが変更されていても、画像の参照先が
            // バニラの壁紙シートと一致する場合は、その登録済みシートを使用します。
            // 追加素材については代替検索を行いません。
            if (separator >= 0)
                return false;

            TileSheet? matchedSheet = null;
            foreach (TileSheet sheet in location.Map.TileSheets)
            {
                string imageSource = sheet.ImageSource?.Replace('\\', '/').TrimStart('/') ?? string.Empty;
                if (!string.Equals(imageSource, "Maps/walls_and_floors", StringComparison.OrdinalIgnoreCase))
                    continue;

                // 同じ画像を使う複数シートがある場合は誤適用を避けます。
                if (matchedSheet != null)
                    return false;
                matchedSheet = sheet;
            }

            if (matchedSheet == null)
                return false;

            tileSheetId = matchedSheet.Id;
            return true;
        }
    }
}
