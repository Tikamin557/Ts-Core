using StardewValley;

namespace Ts_Core.Services.MapRelated.TsCoreDecoratableCellar
{
    /// <summary>
    /// Cellarのマップ定義と保存済みの張り替え情報を統合します。
    /// タイル描画やアイテム使用は行わず、次段階の適用処理へ渡す状態を構築します。
    /// </summary>
    internal static class TsCoreDecoratableCellarMaterials
    {
        //----------------------------------------
        // Effective Materials
        //----------------------------------------

        /// <summary>
        /// 現在のマップに存在する領域ごとに、保存済みの素材を優先して取得します。
        /// 保存済みの領域がマップ変更で一時的に消えても、保存データは削除しません。
        /// </summary>
        internal static Dictionary<string, string> ReadEffectiveMaterials(GameLocation location, bool isFloor)
        {
            var result = new Dictionary<string, string>(StringComparer.Ordinal);
            if (!TsCoreDecoratableCellarService.IsEnabled(location))
                return result;

            // 壁紙はBuildingsレイヤーのみに領域が存在する場合も対象にします。
            var floorRegions = isFloor
                ? TsCoreDecoratableCellarService.ReadRegionTiles(location, true)
                : null;
            var wallRegions = isFloor
                ? null
                : TsCoreDecoratableCellarLayerRegions.ReadWallRegionLayers(location);
            var defaults = TsCoreDecoratableCellarService.ReadRegionDefaults(location, isFloor);
            var saved = TsCoreDecoratableCellarState.Read(location, isFloor);

            IEnumerable<string> regionIds = isFloor
                ? floorRegions!.Keys
                : wallRegions!.Keys;

            foreach (string regionId in regionIds)
            {
                // 描画対象タイルが存在しない領域は適用対象から除外します。
                bool hasTiles = isFloor
                    ? floorRegions![regionId].Count > 0
                    : wallRegions![regionId].Count > 0;
                if (!hasTiles)
                    continue;

                if (saved.TryGetValue(regionId, out string? selected) && !string.IsNullOrWhiteSpace(selected))
                    result[regionId] = selected;
                else if (defaults.TryGetValue(regionId, out string? initial) && !string.IsNullOrWhiteSpace(initial))
                    result[regionId] = initial;
            }

            return result;
        }
    }
}
