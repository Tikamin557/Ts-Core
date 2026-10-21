namespace Ts_Core.Services
{
    /// <summary>
    /// RecipeLayoutsの専用行を確保するため、レシピの配置前に必要な行境界を計算します。
    /// 実際の画面配置はCraftingPage側で行い、このクラスはバニラのUIを変更しません。
    /// </summary>
    internal static class RecipeLayoutRowPlanner
    {
        /// <summary>
        /// 専用グループの開始・終了時に、次の行へ移動する必要があるかを返します。
        /// グループがページをまたぐ場合も、次のレシピを同じ行に混在させません。
        /// </summary>
        internal static (bool Before, bool After) GetRowBreaks(
            string recipeId,
            HashSet<string> starts,
            HashSet<string> ends,
            int column)
        {
            bool before = column > 0 && starts.Contains(recipeId);
            bool after = ends.Contains(recipeId);
            return (before, after);
        }

        /// <summary>
        /// 現在の行を終了し、次の行の先頭へ進めます。
        /// ページ外への移動は呼び出し側のCraftingPageが処理します。
        /// </summary>
        internal static void AdvanceRow(ref int column, ref int row)
        {
            if (column == 0)
                return;

            column = 0;
            row++;
        }

        /// <summary>
        /// レシピの配置が終わった後に、専用グループの末尾で行を閉じるか判定します。
        /// 既に次の行の先頭へ移動している場合は空行を追加しません。
        /// </summary>
        internal static bool ShouldFinishRow(string recipeId, HashSet<string> ends, int column)
        {
            return column > 0 && ends.Contains(recipeId);
        }

        /// <summary>
        /// 先頭レシピを配置する前に、現在の行を閉じる必要があるか判定します。
        /// </summary>
        internal static bool ShouldStartRow(string recipeId, HashSet<string> starts, int column)
        {
            return column > 0 && starts.Contains(recipeId);
        }
    }
}
