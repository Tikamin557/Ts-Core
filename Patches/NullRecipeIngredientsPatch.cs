using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System.Reflection;
using System.Reflection.Emit;

namespace Ts_Core.Patches
{
    /// <summary>
    /// レシピの素材欄に指定された特殊IDを解釈し、素材不要のレシピとして扱います。
    /// </summary>
    public static class NullRecipeIngredientsPatch
    {
        private const string Marker = "TsCoreNullRecipeIngredients";
        private static ITranslationHelper? translations;
        [ThreadStatic] private static bool drawingTextRecipe;

        /// <summary>レシピの生成・作成可能数・素材表示を制御するパッチを登録します。</summary>
        public static void Apply(Harmony harmony, IMonitor monitor, ITranslationHelper translationHelper)
        {
            translations = translationHelper;
            harmony.Patch(
                AccessTools.Constructor(typeof(CraftingRecipe), new[] { typeof(string), typeof(bool) }),
                postfix: new HarmonyMethod(typeof(NullRecipeIngredientsPatch), nameof(RecipeConstructed)));
            harmony.Patch(
                AccessTools.Method(typeof(CraftingRecipe), nameof(CraftingRecipe.getCraftableCount), new[] { typeof(System.Collections.Generic.IList<Item>) }),
                prefix: new HarmonyMethod(typeof(NullRecipeIngredientsPatch), nameof(CraftableCountPrefix)));
            harmony.Patch(
                AccessTools.Method(typeof(CraftingRecipe), nameof(CraftingRecipe.getDescriptionHeight)),
                postfix: new HarmonyMethod(typeof(NullRecipeIngredientsPatch), nameof(DescriptionHeightPostfix)));
            BetterCraftingTooltipCompatibility.Apply(harmony, monitor, translationHelper);
            harmony.Patch(
                AccessTools.Method(typeof(CraftingRecipe), nameof(CraftingRecipe.drawRecipeDescription)),
                prefix: new HarmonyMethod(typeof(NullRecipeIngredientsPatch), nameof(DrawDescriptionPrefix)),
                postfix: new HarmonyMethod(typeof(NullRecipeIngredientsPatch), nameof(DrawDescriptionPostfix)),
                finalizer: new HarmonyMethod(typeof(NullRecipeIngredientsPatch), nameof(DrawDescriptionFinalizer)),
                transpiler: new HarmonyMethod(typeof(NullRecipeIngredientsPatch), nameof(DrawDescriptionTranspiler)));
        }

        //----------------------------------------
        // 特殊ID判定
        //----------------------------------------

        private static bool TryGetMode(CraftingRecipe recipe, out bool hide)
        {
            hide = false;
            var recipes = recipe.isCookingRecipe ? CraftingRecipe.cookingRecipes : CraftingRecipe.craftingRecipes;
            if (!recipes.TryGetValue(recipe.name, out string? raw) || raw == null)
                return false;
            string ingredient = raw.Split('/')[0].Trim();
            if (ingredient == Marker || ingredient == Marker + "_Text")
                return true;
            if (ingredient == Marker + "_Hide")
            {
                hide = true;
                return true;
            }
            return false;
        }

        //----------------------------------------
        // 必要素材の除去
        //----------------------------------------

        private static void RecipeConstructed(CraftingRecipe __instance)
        {
            if (TryGetMode(__instance, out _))
                __instance.recipeList.Clear();
        }

        private static bool CraftableCountPrefix(CraftingRecipe __instance, ref int __result)
        {
            if (!TryGetMode(__instance, out _))
                return true;
            // UI上で作成可能として扱う。実際の作成は既存の所持数・所持枠処理に従う。
            __result = 999;
            return false;
        }

        //----------------------------------------
        // 素材欄の描画
        //----------------------------------------

        private static void DescriptionHeightPostfix(CraftingRecipe __instance, ref int __result)
        {
            if (!TryGetMode(__instance, out bool hide))
                return;

            // Textはバニラの素材1行分、Hideは説明文のみの高さに調整します。
            __result = Math.Max(0, __result + (hide ? -52 : 36));
        }

        private static bool DrawDescriptionPrefix(CraftingRecipe __instance, SpriteBatch b, Vector2 position, int width)
        {
            if (!TryGetMode(__instance, out bool hide))
                return true;

            if (!hide)
            {
                // 見出し・区切り線・説明文はバニラに描画させます。
                drawingTextRecipe = true;
                return true;
            }

            // Hideは従来の描画を維持します。
            // Hide: 素材欄・区切り線を描かず説明文のみ表示します。
            const int descriptionY = 24;


            string description = Game1.parseText(__instance.description, Game1.smallFont, width - 8);
            Utility.drawTextWithShadow(b, description, Game1.smallFont,
                position + new Vector2(0f, descriptionY), Game1.textColor * 0.75f);
            return false;
        }

        /// <summary>
        /// Textモードでは素材行だけを空にし、バニラの区切り線と説明文を素材1行分下げます。
        /// </summary>
        private static IEnumerable<CodeInstruction> DrawDescriptionTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo countGetter = AccessTools.PropertyGetter(typeof(Dictionary<string, int>), nameof(Dictionary<string, int>.Count));
            MethodInfo replacement = AccessTools.Method(typeof(NullRecipeIngredientsPatch), nameof(GetVisualIngredientCount));
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.Calls(countGetter))
                {
                    instruction.opcode = OpCodes.Call;
                    instruction.operand = replacement;
                }
                yield return instruction;
            }
        }

        private static int GetVisualIngredientCount(Dictionary<string, int> ingredients)
        {
            return drawingTextRecipe ? 1 : ingredients.Count;
        }

        /// <summary>素材行はバニラの高さに合わせて文字だけを追加します。</summary>
        private static void DrawDescriptionPostfix(CraftingRecipe __instance, SpriteBatch b, Vector2 position)
        {
            if (!drawingTextRecipe)
                return;
            drawingTextRecipe = false;
            string label = translations?.Get("nullRecipeIngredients.label").ToString() ?? "No ingredients required";
            Utility.drawTextWithShadow(b, label, Game1.smallFont,
                position + new Vector2(8f, 68f), Game1.textColor * 0.75f);
        }

        private static Exception? DrawDescriptionFinalizer(Exception? __exception)
        {
            drawingTextRecipe = false;
            return __exception;
        }

    }
}
