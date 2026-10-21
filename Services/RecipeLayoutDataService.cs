using StardewModdingAPI.Events;
using StardewValley;
using Ts_Core.Models;

namespace Ts_Core.Services
{
    /// <summary>
    /// Content Patcherから編集できるレシピ表示グループを管理します。
    /// </summary>
    public static class RecipeLayoutDataService
    {
        public const string AssetName = "TsCore/RecipeLayouts";

        public static void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo(AssetName))
            {
                e.LoadFrom(
                    () => new Dictionary<string, RecipeLayoutModel>(),
                    AssetLoadPriority.Exclusive);
            }
        }

        /// <summary>
        /// 専用行グループの開始・終了レシピを、並べ替え後の表示順から取得します。
        /// 表示されないレシピや他グループで先に割り当てられたIDは対象外です。
        /// </summary>
        public static List<(string Start, string End)> GetDedicatedRowRanges(List<string> orderedRecipes, bool cooking)
        {
            var ranges = new List<(string Start, string End)>();
            if (orderedRecipes.Count == 0)
                return ranges;

            Dictionary<string, RecipeLayoutModel> groups;
            try
            {
                groups = Game1.content.Load<Dictionary<string, RecipeLayoutModel>>(AssetName);
            }
            catch
            {
                return ranges;
            }

            if (groups == null || groups.Count == 0)
                return ranges;

            var available = new HashSet<string>(orderedRecipes, StringComparer.Ordinal);
            var claimed = new HashSet<string>(StringComparer.Ordinal);
            foreach (RecipeLayoutModel group in groups.Values)
            {
                if (group == null || !string.Equals(group.RecipeType,
                    cooking ? "Cooking" : "Crafting", StringComparison.OrdinalIgnoreCase))
                    continue;

                string? first = null;
                string? last = null;
                foreach (string id in group.RecipeIds ?? new List<string>())
                {
                    if (id != null && available.Contains(id) && claimed.Add(id))
                    {
                        first ??= id;
                        last = id;
                    }
                }

                if (group.DedicatedRows && first != null && last != null)
                    ranges.Add((first, last));
            }

            // グループ定義順ではなく、画面上の並び順で境界を返します。
            var positions = new Dictionary<string, int>(StringComparer.Ordinal);
            for (int i = 0; i < orderedRecipes.Count; i++)
                positions.TryAdd(orderedRecipes[i], i);
            ranges.Sort((a, b) => positions[a.Start].CompareTo(positions[b.Start]));
            return ranges;
        }

        /// <summary>
        /// 専用行グループの先頭レシピIDを取得します。
        /// </summary>
        public static HashSet<string> GetDedicatedRowStarts(List<string> orderedRecipes, bool cooking)
        {
            var starts = new HashSet<string>(StringComparer.Ordinal);
            foreach (var range in GetDedicatedRowRanges(orderedRecipes, cooking))
                starts.Add(range.Start);
            return starts;
        }

        /// <summary>
        /// 指定画面に表示できるレシピのみを、グループ指定順に並べ替えます。
        /// 未習得レシピや重複IDは追加しません。
        /// </summary>
        public static List<string> OrderRecipes(List<string> recipes, bool cooking)
        {
            if (recipes.Count == 0)
                return recipes;

            Dictionary<string, RecipeLayoutModel> groups;
            try
            {
                groups = Game1.content.Load<Dictionary<string, RecipeLayoutModel>>(AssetName);
            }
            catch
            {
                return recipes;
            }

            if (groups == null || groups.Count == 0)
                return recipes;

            var available = new HashSet<string>(recipes, StringComparer.Ordinal);
            var assigned = new HashSet<string>(StringComparer.Ordinal);
            var first = new List<string>();
            var last = new List<string>();

            foreach (RecipeLayoutModel group in groups.Values)
            {
                if (group == null || !string.Equals(
                    group.RecipeType, cooking ? "Cooking" : "Crafting",
                    StringComparison.OrdinalIgnoreCase))
                    continue;

                List<string> target = string.Equals(group.Position, "First",
                    StringComparison.OrdinalIgnoreCase) ? first : last;

                foreach (string id in group.RecipeIds ?? new List<string>())
                {
                    if (id != null && available.Contains(id) && assigned.Add(id))
                        target.Add(id);
                }
            }

            if (assigned.Count == 0)
                return recipes;

            var result = new List<string>(recipes.Count);
            result.AddRange(first);
            foreach (string id in recipes)
            {
                if (!assigned.Contains(id))
                    result.Add(id);
            }
            result.AddRange(last);
            return result;
        }
    }
}
