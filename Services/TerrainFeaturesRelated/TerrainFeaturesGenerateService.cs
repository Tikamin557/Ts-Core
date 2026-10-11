using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;

namespace Ts_Core.Services.TerrainFeaturesRelated
{
    /// <summary>
    /// マップから生成した複製ロケーションの初期配置を、既存の配置を壊さずに反映します。
    /// UI と接続する前の内部処理です。
    /// </summary>
    internal static class TerrainFeaturesGenerateService
    {
        /// <summary>
        /// 複製元を先に作成し、成功した場合だけ配置を反映します。
        /// 現段階では通常の地形・オブジェクト・大型地形要素を対象とします。
        /// </summary>
        internal static bool TryGenerate(GameLocation location, TerrainFeaturesClearService.Selection selection)
        {
            if (location == null)
                throw new ArgumentNullException(nameof(location));
            if (selection == null)
                throw new ArgumentNullException(nameof(selection));

            if (!TerrainFeaturesCloneService.TryCreate(location, out GameLocation? source) || source == null)
                return false;

            Apply(location, source, selection);
            return true;
        }

        /// <summary>準備済みの複製元から、空いているタイルに対象要素を追加します。</summary>
        internal static void Apply(GameLocation target, GameLocation source, TerrainFeaturesClearService.Selection selection)
        {
            //----------------------------------------
            // 通常の地形要素
            //----------------------------------------
            foreach (var pair in source.terrainFeatures.Pairs.ToList())
            {
                TerrainFeature feature = pair.Value;
                if (!selection.TerrainTypes.Contains(feature.GetType()) &&
                    !selection.TerrainTypes.Contains(typeof(TerrainFeature)))
                    continue;

                Vector2 tile = pair.Key;
                if (!target.terrainFeatures.ContainsKey(tile) && !target.objects.ContainsKey(tile))
                    target.terrainFeatures[tile] = feature;
            }

            //----------------------------------------
            // 通常オブジェクト
            //----------------------------------------
            foreach (var pair in source.objects.Pairs.ToList())
            {
                StardewValley.Object obj = pair.Value;
                if (!selection.ObjectTypes.Contains(obj.GetType()) &&
                    !selection.ObjectTypes.Contains(typeof(StardewValley.Object)) &&
                    !selection.ObjectIndices.Contains(obj.ParentSheetIndex) &&
                    !(selection.ClearForage && obj.isForage()))
                    continue;

                Vector2 tile = pair.Key;
                if (!target.terrainFeatures.ContainsKey(tile) && !target.objects.ContainsKey(tile))
                    target.objects[tile] = obj;
            }

            //----------------------------------------
            // 作物（耕地を残して再生成）
            //----------------------------------------
            if (selection.ClearCrops)
            {
                foreach (var pair in source.terrainFeatures.Pairs.ToList())
                {
                    if (pair.Value is not HoeDirt sourceDirt || sourceDirt.crop == null)
                        continue;

                    if (target.terrainFeatures.TryGetValue(pair.Key, out TerrainFeature? existing) &&
                        existing is HoeDirt targetDirt && targetDirt.crop == null)
                        targetDirt.crop = sourceDirt.crop;
                }
            }

            //----------------------------------------
            // 大型地形要素
            //----------------------------------------
            foreach (LargeTerrainFeature feature in source.largeTerrainFeatures.ToList())
            {
                if (!selection.LargeTerrainTypes.Contains(feature.GetType()) &&
                    !selection.LargeTerrainTypes.Contains(typeof(LargeTerrainFeature)))
                    continue;

                if (!target.largeTerrainFeatures.Any(existing =>
                    existing.GetType() == feature.GetType() && existing.Tile == feature.Tile))
                    target.largeTerrainFeatures.Add(feature);
            }

            //----------------------------------------
            // Forest 固有の倒木
            //----------------------------------------
            // 通常の resourceClumps とは別に保持されるため、個別に復元します。
            if (target is Forest targetForest && source is Forest sourceForest &&
                sourceForest.obsolete_log != null && targetForest.obsolete_log == null &&
                (selection.ClearAllResourceClumps ||
                 selection.ResourceClumpIndices.Contains(sourceForest.obsolete_log.parentSheetIndex.Value)))
            {
                targetForest.obsolete_log = sourceForest.obsolete_log;
            }

            //----------------------------------------
            // Farm の大型資源（切り株・丸太・大岩）
            //----------------------------------------
            if (target is Farm && (selection.ClearAllResourceClumps || selection.ResourceClumpIndices.Count > 0))
            {
                foreach (ResourceClump clump in source.resourceClumps.ToList())
                {
                    if (!selection.ClearAllResourceClumps && !selection.ResourceClumpIndices.Contains(clump.parentSheetIndex.Value))
                        continue;

                    // 既存の地形やオブジェクトを削除するAPIは使用しません。
                    // 大型資源の占有範囲に何かある場合は生成を見送ります。
                    if (!CanPlaceResourceClump(target, clump))
                        continue;

                    target.resourceClumps.Add(clump);
                }
            }
        }

        /// <summary>
        /// 大型資源の占有範囲を確認し、選択していない既存要素との衝突を防ぎます。
        /// </summary>
        private static bool CanPlaceResourceClump(GameLocation location, ResourceClump clump)
        {
            int startX = (int)clump.Tile.X;
            int startY = (int)clump.Tile.Y;
            int endX = startX + clump.width.Value;
            int endY = startY + clump.height.Value;

            for (int x = startX; x < endX; x++)
            {
                for (int y = startY; y < endY; y++)
                {
                    Vector2 tile = new(x, y);
                    if (location.terrainFeatures.ContainsKey(tile) || location.objects.ContainsKey(tile))
                        return false;
                }
            }

            foreach (ResourceClump existing in location.resourceClumps)
            {
                if (startX < existing.Tile.X + existing.width.Value &&
                    endX > existing.Tile.X &&
                    startY < existing.Tile.Y + existing.height.Value &&
                    endY > existing.Tile.Y)
                    return false;
            }

            // LargeTerrainFeatureの描画・当たり判定領域は種類により異なるため、
            // 安全側に倒し、該当タイルにある場合は配置を見送ります。
            foreach (LargeTerrainFeature existing in location.largeTerrainFeatures)
            {
                if (existing.Tile.X >= startX && existing.Tile.X < endX &&
                    existing.Tile.Y >= startY && existing.Tile.Y < endY)
                    return false;
            }

            return true;
        }
    }
}
