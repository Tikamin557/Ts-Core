using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using Ts_Core.Models;

namespace Ts_Core.Services
{
    /// <summary>
    /// Content Patcherが編集する専用レシピページのグループを管理します。
    /// グループはDisplayOrder（0～10）の昇順、同順位はIDのOrdinal昇順です。
    /// グループ内はRecipeIdsの指定順です。
    /// </summary>
    public static class CustomRecipePageDataService
    {
        public const string AssetName = "TsCore/CustomRecipePages";
        private static readonly HashSet<string> WarnedIds = new(StringComparer.Ordinal);

        public static void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo(AssetName))
                e.LoadFrom(() => new Dictionary<string, CustomRecipePageModel>(), AssetLoadPriority.Exclusive);
        }

        public static Dictionary<string, CustomRecipePageModel> LoadGroups()
        {
            try
            {
                return Game1.content.Load<Dictionary<string, CustomRecipePageModel>>(AssetName)
                    ?? new Dictionary<string, CustomRecipePageModel>();
            }
            catch
            {
                return new Dictionary<string, CustomRecipePageModel>();
            }
        }

        /// <summary>
        /// 全グループの登録IDを取得します。未習得レシピも通常ページから除外します。
        /// 存在しないIDだけを一度警告します。
        /// </summary>
        public static List<(string Id, bool DedicatedStart, bool DedicatedEnd)> GetEntries(
            Dictionary<string, CustomRecipePageModel> groups, IEnumerable<string> knownIds,
            bool cooking, IMonitor? monitor)
        {
            var known = new HashSet<string>(knownIds, StringComparer.Ordinal);
            var claimed = new HashSet<string>(StringComparer.Ordinal);
            var result = new List<(string Id, bool DedicatedStart, bool DedicatedEnd)>();
            foreach (var pair in groups
                .OrderBy(p => Math.Clamp(p.Value?.DisplayOrder ?? 10, 0, 10))
                .ThenBy(p => p.Key, StringComparer.Ordinal))
            {
                var group = pair.Value;
                if (group == null || !string.Equals(group.RecipeType,
                    cooking ? "Cooking" : "Crafting", StringComparison.OrdinalIgnoreCase))
                    continue;
                var ids = new List<string>();
                foreach (string? id in group.RecipeIds ?? new List<string>())
                {
                    if (string.IsNullOrWhiteSpace(id))
                        continue;
                    if (!known.Contains(id))
                    {
                        if (WarnedIds.Add($"{cooking}:{pair.Key}:{id}"))
                            monitor?.Log($"CustomRecipePages: unknown {(cooking ? "cooking" : "crafting")} recipe ID '{id}' in group '{pair.Key}'.", LogLevel.Warn);
                        continue;
                    }
                    if (claimed.Add(id))
                        ids.Add(id);
                }
                for (int i = 0; i < ids.Count; i++)
                    result.Add((ids[i], group.DedicatedRows && i == 0,
                        group.DedicatedRows && i == ids.Count - 1));
            }
            return result;
        }
    }
}
