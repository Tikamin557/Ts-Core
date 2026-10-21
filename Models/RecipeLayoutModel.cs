namespace Ts_Core.Models
{
    /// <summary>
    /// クラフト・料理レシピの表示グループを定義します。
    /// </summary>
    public sealed class RecipeLayoutModel
    {
        public string RecipeType { get; set; } = "Crafting";
        public string Position { get; set; } = "Last";
        public bool DedicatedRows { get; set; }
        public List<string> RecipeIds { get; set; } = new();
    }
}
