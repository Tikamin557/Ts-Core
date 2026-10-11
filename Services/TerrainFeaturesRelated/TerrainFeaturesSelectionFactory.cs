using StardewValley;
using StardewValley.TerrainFeatures;

namespace Ts_Core.Services.TerrainFeaturesRelated
{
    /// <summary>
    /// Reset Terrain Features のメニュー項目を、Clear / Generate / Reset 共通の選択条件に変換します。
    /// ID の対応は元 Mod の Regenerator の初期化処理（IL）を基準にしています。
    /// </summary>
    internal static class TerrainFeaturesSelectionFactory
    {
        /// <summary>元 Mod のメニューで選択できる対象の識別名です。</summary>
        internal enum Target
        {
            Bush, Tree, Grass, ResourceClump, Weeds, Twig, Rock, Forage,
            Stump, Log, Boulder, Path, Fence, Crop, TilledSoil, Object, TFeature
        }

        /// <summary>選択された対象を共通の Selection に変換します。</summary>
        internal static TerrainFeaturesClearService.Selection Create(IEnumerable<Target> targets)
        {
            if (targets == null)
                throw new ArgumentNullException(nameof(targets));

            var selection = new TerrainFeaturesClearService.Selection();
            foreach (Target target in targets)
            {
                switch (target)
                {
                    case Target.Bush:
                        selection.LargeTerrainTypes.Add(typeof(Bush));
                        break;
                    case Target.Tree:
                        selection.TerrainTypes.Add(typeof(Tree));
                        break;
                    case Target.Grass:
                        selection.TerrainTypes.Add(typeof(Grass));
                        break;
                    case Target.ResourceClump:
                        // 元 Mod の ResourceClump は型指定です。現在の Selection は
                        // ID 指定しか持たないため、全種類指定を別途追加する必要があります。
                        selection.ClearAllResourceClumps = true;
                        break;
                    case Target.Weeds:
                        selection.ObjectIndices.UnionWith(new[] { 343, 450, 674, 675, 676, 677, 678, 679, 784, 785, 786, 792, 793, 794 });
                        break;
                    case Target.Twig:
                        selection.ObjectIndices.UnionWith(new[] { 294, 295 });
                        break;
                    case Target.Rock:
                        selection.ObjectIndices.UnionWith(new[] { 450, 343 });
                        break;
                    case Target.Forage:
                        selection.ClearForage = true;
                        break;
                    case Target.Stump:
                        selection.ResourceClumpIndices.Add(600);
                        break;
                    case Target.Log:
                        selection.ResourceClumpIndices.Add(602);
                        break;
                    case Target.Boulder:
                        selection.ResourceClumpIndices.Add(672);
                        break;
                    case Target.Path:
                        selection.TerrainTypes.Add(typeof(Flooring));
                        break;
                    case Target.Fence:
                        selection.ObjectTypes.Add(typeof(Fence));
                        break;
                    case Target.Crop:
                        selection.ClearCrops = true;
                        break;
                    case Target.TilledSoil:
                        selection.TerrainTypes.Add(typeof(HoeDirt));
                        break;
                    case Target.Object:
                        selection.ObjectTypes.Add(typeof(StardewValley.Object));
                        break;
                    case Target.TFeature:
                        selection.TerrainTypes.Add(typeof(TerrainFeature));
                        break;
                }
            }
            return selection;
        }
    }
}
