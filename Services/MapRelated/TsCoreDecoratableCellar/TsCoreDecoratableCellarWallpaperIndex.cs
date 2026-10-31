namespace Ts_Core.Services.MapRelated.TsCoreDecoratableCellar
{
    /// <summary>
    /// DecoratableLocation.UpdateWallpaperのタイル番号計算を分離した補助処理です。
    /// 素材IDからのタイルシート解決や壁領域の高さ判定は呼び出し側が担当します。
    /// </summary>
    internal static class TsCoreDecoratableCellarWallpaperIndex
    {
        //----------------------------------------
        // Wallpaper Tile Index
        //----------------------------------------

        /// <summary>
        /// バニラのUpdateWallpaperと同じ計算式で壁紙タイル番号を取得します。
        /// sourceIndexはGetWallpaperSourceが返す番号、wallRowは壁領域内の高さです。
        /// </summary>
        internal static bool TryGetTileIndex(int sourceIndex, int sheetWidth, int wallRow, out int tileIndex)
        {
            tileIndex = -1;
            if (sourceIndex < 0 || sheetWidth <= 0 || wallRow < 0)
                return false;

            long result = (long)(sourceIndex / sheetWidth) * sheetWidth * 3
                + sourceIndex % sheetWidth
                + (long)wallRow * sheetWidth;
            if (result < 0 || result > int.MaxValue)
                return false;

            tileIndex = (int)result;
            return true;
        }
    }
}
