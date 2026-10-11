using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;

namespace Ts_Core.Services.TerrainFeaturesRelated
{
    /// <summary>
    /// Reset Terrain Features の Clear 操作で使用する低レベルの削除処理です。
    /// UI からの呼び出しは、確認画面とホスト権限のチェックを実装してから接続します。
    /// </summary>
    internal static class TerrainFeaturesClearService
    {
        /// <summary>削除対象を明示的に指定します。未指定の分類は変更しません。</summary>
        internal sealed class Selection
        {
            public HashSet<Type> TerrainTypes { get; } = new();
            public HashSet<Type> ObjectTypes { get; } = new();
            public HashSet<int> ObjectIndices { get; } = new();
            public HashSet<Type> LargeTerrainTypes { get; } = new();
            public HashSet<int> ResourceClumpIndices { get; } = new();
            public bool ClearCrops { get; set; }
            public bool ClearForage { get; set; }
            public bool ClearAllResourceClumps { get; set; }
        }

        /// <summary>
        /// 指定した地形要素を削除します。呼び出し元は実行前にプレイヤーの確認を得てください。
        /// </summary>
        internal static void Clear(GameLocation location, Selection selection)
        {
            if (location == null)
                throw new ArgumentNullException(nameof(location));
            if (selection == null)
                throw new ArgumentNullException(nameof(selection));

            //----------------------------------------
            // 通常の地形要素と作物
            //----------------------------------------
            foreach (Vector2 tile in location.terrainFeatures.Keys.ToList())
            {
                TerrainFeature feature = location.terrainFeatures[tile];
                if (selection.TerrainTypes.Contains(feature.GetType()) ||
                    selection.TerrainTypes.Contains(typeof(TerrainFeature)))
                {
                    location.terrainFeatures.Remove(tile);
                }
                else if (selection.ClearCrops && feature is HoeDirt dirt)
                {
                    dirt.crop = null;
                }
            }

            //----------------------------------------
            // 通常オブジェクト
            //----------------------------------------
            foreach (Vector2 tile in location.objects.Keys.ToList())
            {
                StardewValley.Object obj = location.objects[tile];
                if (selection.ObjectTypes.Contains(obj.GetType()) ||
                    selection.ObjectTypes.Contains(typeof(StardewValley.Object)) ||
                    selection.ObjectIndices.Contains(obj.ParentSheetIndex) ||
                    (selection.ClearForage && obj.isForage()))
                {
                    location.objects.Remove(tile);
                }
            }

            //----------------------------------------
            // 大型地形要素
            //----------------------------------------
            foreach (LargeTerrainFeature feature in location.largeTerrainFeatures.ToList())
            {
                if (selection.LargeTerrainTypes.Contains(feature.GetType()) ||
                    selection.LargeTerrainTypes.Contains(typeof(LargeTerrainFeature)))
                    location.largeTerrainFeatures.Remove(feature);
            }

            //----------------------------------------
            // Forest 固有の倒木
            //----------------------------------------
            // 元 Mod の Regenerator.Clear と同様に、通常の resourceClumps とは
            // 別フィールドで管理される倒木を対象 ID に一致した場合だけ削除します。
            if (location is Forest forest && forest.obsolete_log != null &&
                (selection.ClearAllResourceClumps || selection.ResourceClumpIndices.Contains(forest.obsolete_log.parentSheetIndex.Value)))
            {
                forest.obsolete_log = null;
            }

            //----------------------------------------
            // 大型資源（切り株・丸太・大岩）
            //----------------------------------------
            // Generate と同じ ResourceClumpIndices を参照し、
            // Reset 時に既存の大型資源が残らないようにします。
            if (selection.ClearAllResourceClumps || selection.ResourceClumpIndices.Count > 0)
            {
                foreach (ResourceClump clump in location.resourceClumps.ToList())
                {
                    if (selection.ClearAllResourceClumps || selection.ResourceClumpIndices.Contains(clump.parentSheetIndex.Value))
                        location.resourceClumps.Remove(clump);
                }
            }
        }
    }
}
