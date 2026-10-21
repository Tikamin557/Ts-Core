using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using System.Reflection;

namespace Ts_Core.Patches
{
    /// <summary>
    /// Better Crafting独自のツールチップにも素材不要の説明を表示します。
    /// Better Crafting未導入時はパッチを登録しません。
    /// </summary>
    internal static class BetterCraftingTooltipCompatibility
    {
        private const string Marker = "TsCoreNullRecipeIngredients";
        private static ITranslationHelper? translations;
        private static IMonitor? monitor;
        private static PropertyInfo? activeRecipe;
        private static FieldInfo? cookingField;
        private static ConstructorInfo? textConstructor;
        private static ConstructorInfo? layoutConstructor;
        private static Type? nodeType;
        private static Type? dividerType;
        private static PropertyInfo? childrenProperty;
        private static object? vertical;
        private static object? horizontal;

        public static void Apply(Harmony harmony, IMonitor modMonitor, ITranslationHelper translationHelper)
        {
            monitor = modMonitor;
            Type? page = AccessTools.TypeByName("Leclair.Stardew.BetterCrafting.Menus.BetterCraftingPage");
            if (page == null)
            {
                monitor.Log("[Better Crafting tooltip] Better Crafting not detected; skipping.", LogLevel.Trace);
                return;
            }
            MethodInfo? tooltip = AccessTools.Method(page, "GetRecipeTooltip");
            Type? text = AccessTools.TypeByName("Leclair.Stardew.Common.UI.SimpleLayout.TextNode");
            Type? layout = AccessTools.TypeByName("Leclair.Stardew.Common.UI.SimpleLayout.LayoutNode");
            Type? direction = AccessTools.TypeByName("Leclair.Stardew.Common.UI.SimpleLayout.LayoutDirection");
            nodeType = AccessTools.TypeByName("Leclair.Stardew.Common.UI.SimpleLayout.ISimpleNode");
            dividerType = AccessTools.TypeByName("Leclair.Stardew.Common.UI.SimpleLayout.DividerNode");
            childrenProperty = layout == null ? null : AccessTools.Property(layout, "Children");
            activeRecipe = AccessTools.Property(page, "ActiveRecipe");
            cookingField = AccessTools.Field(page, "cooking");
            if (tooltip == null || text == null || layout == null || direction == null || nodeType == null || activeRecipe == null)
            {
                monitor.Log($"[Better Crafting tooltip] Missing member: tooltip={tooltip != null}, text={text != null}, layout={layout != null}, direction={direction != null}, node={nodeType != null}, activeRecipe={activeRecipe != null}.", LogLevel.Warn);
                return;
            }
            textConstructor = text.GetConstructors()[0];
            layoutConstructor = layout.GetConstructors()[0];
            if (!Enum.TryParse(direction, "Vertical", true, out object? directionValue))
            {
                monitor.Log("[Better Crafting tooltip] Vertical layout direction not found.", LogLevel.Warn);
                return;
            }
            vertical = directionValue;
            if (!Enum.TryParse(direction, "Horizontal", true, out horizontal))
            {
                monitor.Log("[Better Crafting tooltip] Horizontal layout direction not found.", LogLevel.Warn);
                return;
            }
            translations = translationHelper;
            MethodInfo patch = AccessTools.Method(typeof(BetterCraftingTooltipCompatibility), nameof(TooltipPostfix))!
                .MakeGenericMethod(tooltip.ReturnType);
            try
            {
                harmony.Patch(tooltip, postfix: new HarmonyMethod(patch));
            }
            catch (Exception ex)
            {
                monitor.Log($"[Better Crafting tooltip] Patch registration failed: {ex}", LogLevel.Warn);
            }
        }

        /// <summary>
        /// 既存のノードを保持したまま、Textモードのレシピだけ補足行を付け加えます。
        /// </summary>
        private static void TooltipPostfix<T>(object __instance, ref T __result) where T : class
        {
            if (__result == null || activeRecipe?.GetValue(__instance) is not object recipe ||
                AccessTools.Property(recipe.GetType(), "Name")?.GetValue(recipe) is not string id)
                return;
            bool cooking = cookingField?.GetValue(__instance) is true;
            Dictionary<string, string> recipes = cooking ? CraftingRecipe.cookingRecipes : CraftingRecipe.craftingRecipes;
            if (!recipes.TryGetValue(id, out string? raw) || raw == null)
                return;
            string ingredient = raw.Split('/')[0].Trim();
            if (ingredient != Marker && ingredient != Marker + "_Text")
                return;
            try
            {
                // Better Craftingのツールチップ構造を維持し、説明文の直前に素材欄を挿入します。
                string message = translations?.Get("nullRecipeIngredients.label").ToString() ?? "No ingredients required";
                object CreateText(string value)
                {
                    ParameterInfo[] parameters = textConstructor!.GetParameters();
                    object?[] values = new object?[parameters.Length];
                    values[0] = value;
                    for (int i = 1; i < values.Length; i++)
                        values[i] = parameters[i].HasDefaultValue ? parameters[i].DefaultValue : Activator.CreateInstance(parameters[i].ParameterType);
                    return textConstructor.Invoke(values);
                }
                // Better Craftingの材料一覧と同様に、材料テキストを横方向の
                // LayoutNodeへ格納します。見出し・説明文は既存ノードを維持します。
                object CreateIngredientRow()
                {
                    Array rowChildren = Array.CreateInstance(nodeType!, 1);
                    rowChildren.SetValue(CreateText(message), 0);
                    ParameterInfo[] rowParameters = layoutConstructor!.GetParameters();
                    object?[] rowArgs = new object?[rowParameters.Length];
                    rowArgs[0] = horizontal;
                    rowArgs[1] = rowChildren;
                    for (int i = 2; i < rowArgs.Length; i++)
                        rowArgs[i] = rowParameters[i].HasDefaultValue
                            ? rowParameters[i].DefaultValue
                            : Activator.CreateInstance(rowParameters[i].ParameterType);
                    // 通常の材料行はアイコン（2倍表示）を含むため、
                    // 文字だけの行にも同等の高さを確保し、上下に余白を設けます。
                    rowArgs[3] = new Vector2(0f, 40f);
                    return layoutConstructor.Invoke(rowArgs);
                }
                if (childrenProperty?.GetValue(__result) is not Array children || children.Length < 2)
                {
                    monitor?.Log($"[Better Crafting tooltip] Unexpected tooltip structure for '{id}'; keeping original.", LogLevel.Warn);
                    return;
                }
                // Better Craftingの材料欄は、レシピ名と説明文の間にあります。
                // 既存の最初の区切り線の直後へ材料欄を挿入し、説明文と
                // クラフト回数の表示は元の順序のまま維持します。
                int insert = -1;
                for (int i = 0; i < children.Length; i++)
                {
                    if (dividerType?.IsInstanceOfType(children.GetValue(i)) == true)
                    {
                        insert = i + 1;
                        break;
                    }
                }
                if (insert < 0 || dividerType == null)
                {
                    monitor?.Log($"[Better Crafting tooltip] No header divider for '{id}'; keeping original.", LogLevel.Warn);
                    return;
                }
                var all = new List<object>();
                for (int i = 0; i < children.Length; i++)
                {
                    if (i == insert)
                    {
                        all.Add(CreateText(translations?.Get("nullRecipeIngredients.heading").ToString() ?? "Ingredients:"));
                        all.Add(CreateIngredientRow());
                    }
                    all.Add(children.GetValue(i)!);
                }
                Array nodes = Array.CreateInstance(nodeType!, all.Count);
                for (int i = 0; i < all.Count; i++)
                    nodes.SetValue(all[i], i);
                ParameterInfo[] layoutParams = layoutConstructor!.GetParameters();
                object?[] args = new object?[layoutParams.Length];
                args[0] = vertical;
                args[1] = nodes;
                for (int i = 2; i < args.Length; i++)
                    args[i] = layoutParams[i].HasDefaultValue ? layoutParams[i].DefaultValue : Activator.CreateInstance(layoutParams[i].ParameterType);
                args[3] = new Vector2(400f, 0f);
                object root = layoutConstructor.Invoke(args);
                ConstructorInfo dividerCtor = dividerType.GetConstructors()[0];
                object MakeDivider(bool thick)
                {
                    ParameterInfo[] parameters = dividerCtor.GetParameters();
                    object?[] values = new object?[parameters.Length];
                    values[0] = root;
                    values[1] = thick;
                    for (int i = 2; i < values.Length; i++)
                        values[i] = parameters[i].HasDefaultValue ? parameters[i].DefaultValue : null;
                    return dividerCtor.Invoke(values);
                }
                // 「材料:」の直後、および「素材不要」の直後に標準の区切り線を置きます。
                // Better Crafting標準の材料欄と同じく、見出しの上は太いDivider、
                // 見出しの下と材料行の下は細いDividerを使用します。
                all[insert - 1] = MakeDivider(true);
                all.Insert(insert + 1, MakeDivider(false));
                all.Insert(insert + 3, MakeDivider(false));
                Array withDividers = Array.CreateInstance(nodeType!, all.Count);
                for (int i = 0; i < all.Count; i++)
                    withDividers.SetValue(all[i], i);
                AccessTools.Field(root.GetType(), "_Children")?.SetValue(root, withDividers);
                __result = (T)root;
            }
            catch (Exception ex)
            {
                monitor?.Log($"[Better Crafting tooltip] Failed to append text for '{id}': {ex}", LogLevel.Warn);
            }
        }
    }
}
