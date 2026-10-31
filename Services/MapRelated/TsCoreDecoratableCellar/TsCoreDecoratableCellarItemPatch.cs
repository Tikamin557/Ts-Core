using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;

namespace Ts_Core.Services.MapRelated.TsCoreDecoratableCellar
{
    /// <summary>
    /// 通常のCellarを維持したまま、バニラの壁紙・床材アイテムの配置操作を
    /// TsCoreDecoratableCellarの領域に接続します。
    /// 対象外の場所ではバニラ処理を変更しません。
    /// </summary>
    internal static class TsCoreDecoratableCellarItemPatch
    {
        private static IMonitor? monitor;
        //----------------------------------------
        // Harmony Registration
        //----------------------------------------

        internal static void Apply(Harmony harmony, IMonitor monitor)
        {
            TsCoreDecoratableCellarItemPatch.monitor = monitor;
            var method = AccessTools.Method(typeof(Wallpaper), nameof(Wallpaper.placementAction));
            if (method == null)
            {
                monitor.Log("Cellar wallpaper item patch was not applied: placementAction not found.", LogLevel.Warn);
                return;
            }

            try
            {
                harmony.Patch(method, prefix: new HarmonyMethod(typeof(TsCoreDecoratableCellarItemPatch), nameof(Prefix)));
            }
            catch (Exception ex)
            {
                monitor.Log($"Cellar wallpaper item patch was not applied: {ex}", LogLevel.Warn);
            }
        }

        //----------------------------------------
        // Item Placement
        //----------------------------------------

        /// <summary>
        /// 壁紙アイテムのクリック座標からFloorID/WallIDを解決し、
        /// 描画成功時のみ素材を保存します。アイテム消費はゲーム側に任せます。
        /// </summary>
        private static bool Prefix(Wallpaper __instance, GameLocation location, int x, int y, ref bool __result)
        {
            if (!TsCoreDecoratableCellarService.IsEnabled(location))
                return true;

            try
            {
                __result = false;
                int tileX = x / 64;
                int tileY = y / 64;
                bool isFloor = __instance.isFloor.Value;

                string? regionId = isFloor
                    ? TsCoreDecoratableCellarService.GetFloorRegionId(location, tileX, tileY)
                    : FindWallRegion(location, tileX, tileY);
                if (string.IsNullOrWhiteSpace(regionId))
                    return false;

                string materialId = __instance.parentSheetIndex.Value.ToString();
                var set = __instance.GetSetData();
                if (set != null)
                    materialId = set.Id + ":" + materialId;

                bool applied = isFloor
                    ? TsCoreDecoratableCellarTilePainter.TryApplyFloorMaterial(location, regionId, materialId)
                    : TsCoreDecoratableCellarWallPainter.TryApplyWallpaperMaterial(location, regionId, materialId);
                if (!applied)
                    return false;

                TsCoreDecoratableCellarState.Write(location, regionId, materialId, isFloor);
                location.playSound("coin");
                __result = true;
                return false;
            }
            catch (Exception ex)
            {
                // 対象Cellarでは独自処理の途中で例外が起きる可能性があるため、
                // バニラ処理へ続行して二重適用・二重消費させないようにします。
                monitor?.Log($"Cellar wallpaper item placement failed: {ex}", LogLevel.Error);
                __result = false;
                return false;
            }
        }

        /// <summary>
        /// WallIDは壁上端のBackにだけ付くマップがあるため、
        /// クリックした壁面から上方向3タイル以内の定義を探します。
        /// </summary>
        private static string? FindWallRegion(GameLocation location, int x, int y)
        {
            for (int offset = 0; offset < 3; offset++)
            {
                if (y - offset < 0)
                    break;
                string? id = TsCoreDecoratableCellarService.GetWallRegionId(location, x, y - offset);
                if (!string.IsNullOrWhiteSpace(id))
                    return id;
            }
            return null;
        }
    }
}
