using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using xTile;

namespace Ts_Core.Services.MapRelated.TsCoreDecoratableCellar
{
    /// <summary>
    /// Content Patcherなどが現在のCellarマップを再読み込みした際、
    /// 保存済みの壁紙・床材を新しいマップへ再適用します。
    /// </summary>
    internal static class TsCoreDecoratableCellarMapReload
    {
        private static GameLocation? observedLocation;
        private static Map? observedMap;
        private static int pendingTicks;

        //----------------------------------------
        // Map Asset Reload
        //----------------------------------------

        /// <summary>
        /// Cellarのマップアセットが再読み込みされたら、マップ反映後の復元を予約します。
        /// </summary>
        internal static void OnAssetReady(object? sender, AssetReadyEventArgs e)
        {
            if (Context.IsWorldReady
                && e.NameWithoutLocale.IsEquivalentTo("Maps/Cellar")
                && TsCoreDecoratableCellarService.IsEnabled(Game1.currentLocation))
            {
                pendingTicks = 2;
            }
        }

        //----------------------------------------
        // Active Map Observation
        //----------------------------------------

        /// <summary>
        /// アセットの再読み込み後、または現在のマップ参照が変わった後に一度だけ復元します。
        /// 通常のフレームではタイルの走査を行いません。
        /// </summary>
        internal static void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady || !TsCoreDecoratableCellarService.IsEnabled(Game1.currentLocation))
            {
                observedLocation = null;
                observedMap = null;
                pendingTicks = 0;
                return;
            }

            GameLocation location = Game1.currentLocation;
            Map? currentMap = location.Map;

            if (!ReferenceEquals(location, observedLocation))
            {
                observedLocation = location;
                observedMap = currentMap;
                pendingTicks = 0;
                return;
            }

            if (!ReferenceEquals(currentMap, observedMap))
            {
                observedMap = currentMap;
                pendingTicks = 2;
            }

            if (pendingTicks <= 0)
                return;

            pendingTicks--;
            if (pendingTicks == 0)
                TsCoreDecoratableCellarRestore.RestoreSavedWallpaper(location);
        }
    }
}
