using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;

namespace Ts_Core.Services.TerrainFeaturesRelated
{
    /// <summary>
    /// 選択した地形要素を再生成元の初期配置へ戻します。
    /// 変更前のコレクションを退避し、例外発生時には可能な範囲で復元します。
    /// </summary>
    internal static class TerrainFeaturesResetService
    {
        /// <summary>
        /// 再生成元の準備後に Clear と Generate を実行します。
        /// UI からの呼び出しには、別途確認画面とホスト権限の検証が必要です。
        /// </summary>
        internal static bool TryReset(GameLocation location, TerrainFeaturesClearService.Selection selection, IMonitor? monitor = null)
        {
            if (location == null)
                throw new ArgumentNullException(nameof(location));
            if (selection == null)
                throw new ArgumentNullException(nameof(selection));

            if (!TerrainFeaturesCloneService.TryCreate(location, out GameLocation? source) || source == null)
                return false;

            // コレクションと、Clear が直接変更する HoeDirt.crop を退避します。
            var terrain = location.terrainFeatures.Pairs.ToList();
            var objects = location.objects.Pairs.ToList();
            var large = location.largeTerrainFeatures.ToList();
            var clumps = location.resourceClumps.ToList();
            var crops = terrain.Where(pair => pair.Value is HoeDirt)
                .Select(pair => (Dirt: (HoeDirt)pair.Value, Crop: ((HoeDirt)pair.Value).crop))
                .ToList();
            ResourceClump? forestLog = (location as Forest)?.obsolete_log;

            try
            {
                TerrainFeaturesClearService.Clear(location, selection);
                TerrainFeaturesGenerateService.Apply(location, source, selection);
                return true;
            }
            catch (Exception resetError)
            {
                monitor?.Log($"Terrain Features Reset failed in {location.NameOrUniqueName}: {resetError}", LogLevel.Error);

                // 復元自体が失敗しても元の例外を隠さないよう、呼び出し元には false を返します。
                try
                {
                    location.terrainFeatures.Clear();
                    foreach (var pair in terrain)
                        location.terrainFeatures[pair.Key] = pair.Value;

                    location.objects.Clear();
                    foreach (var pair in objects)
                        location.objects[pair.Key] = pair.Value;

                    location.largeTerrainFeatures.Clear();
                    foreach (var feature in large)
                        location.largeTerrainFeatures.Add(feature);

                    location.resourceClumps.Clear();
                    foreach (var clump in clumps)
                        location.resourceClumps.Add(clump);

                    foreach (var entry in crops)
                        entry.Dirt.crop = entry.Crop;

                    if (location is Forest forest)
                        forest.obsolete_log = forestLog;
                }
                catch (Exception restoreError)
                {
                    monitor?.Log($"Terrain Features rollback failed in {location.NameOrUniqueName}: {restoreError}", LogLevel.Error);
                }
                return false;
            }
        }
    }
}
