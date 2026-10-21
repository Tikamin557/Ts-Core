namespace Ts_Core.Models
{
    /// <summary>
    /// T's Core専用レシピページに表示するグループを定義します。
    /// </summary>
    public sealed class CustomRecipePageModel
    {
        public string RecipeType { get; set; } = "Crafting";
        /// <summary>
        /// グループの表示順（0～10）。省略時は10です。
        /// 同じ値のグループはグループIDのOrdinal昇順になります。
        /// </summary>
        public int DisplayOrder { get; set; } = 10;

        public bool DedicatedRows { get; set; }
        public List<string> RecipeIds { get; set; } = new();
    }
}
