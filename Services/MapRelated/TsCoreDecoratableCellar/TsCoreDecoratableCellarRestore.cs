using StardewValley;

namespace Ts_Core.Services.MapRelated.TsCoreDecoratableCellar
{
    /// <summary>
    /// Cellarへ入った際に、保存済みの壁紙と床材を現在のマップへ再適用します。
    /// Content Patcher等が変更した現在のマップを対象とし、元のCellar型は変更しません。
    /// </summary>
    internal static class TsCoreDecoratableCellarRestore
    {
        //----------------------------------------
        // Saved Wallpaper Restoration
        //----------------------------------------

        /// <summary>
        /// 保存済みの壁紙と床材を再適用します。マップの初期素材は上書きしません。
        /// 未登録素材や適用できない領域は変更せずに残します。
        /// </summary>
        internal static void RestoreSavedWallpaper(GameLocation location)
        {
            if (!TsCoreDecoratableCellarService.IsEnabled(location))
                return;

            foreach (var entry in TsCoreDecoratableCellarState.Read(location, isFloor: false))
            {
                try
                {
                    TsCoreDecoratableCellarWallPainter.TryApplyWallpaperMaterial(location, entry.Key, entry.Value);
                }
                catch (Exception)
                {
                    // 一つの領域の失敗で他の領域の復元を止めないようにします。
                }
            }

            foreach (var entry in TsCoreDecoratableCellarState.Read(location, isFloor: true))
            {
                try
                {
                    TsCoreDecoratableCellarTilePainter.TryApplyFloorMaterial(location, entry.Key, entry.Value);
                }
                catch (Exception)
                {
                    // 一つの領域の失敗で他の領域の復元を止めないようにします。
                }
            }
        }
    }
}
