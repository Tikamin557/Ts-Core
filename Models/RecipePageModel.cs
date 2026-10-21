namespace Ts_Core.Models
{
    /// <summary>
    /// T's Core専用レシピページに表示するグループを定義します。
    /// </summary>
    public sealed class RecipePageModel
    {
        public string RecipeType { get; set; } = "Crafting";
        public bool DedicatedRows { get; set; }
        public List<string> RecipeIds { get; set; } = new();
    }
}
