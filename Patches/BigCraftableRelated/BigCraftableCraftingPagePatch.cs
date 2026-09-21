using HarmonyLib;
using StardewValley;
using StardewValley.Menus;
using System.Reflection;
using Ts_Core.Services.BigCraftableRelated;

namespace Ts_Core.Patches.BigCraftableRelated
{
    /// <summary>
    /// BigCraftable Extensionの
    /// Texture設定をCrafting Page描画に適用するPatchです。
    /// </summary>
    public static class BigCraftableCraftingPagePatch
    {
        //----------------------------------------
        // Android Fields
        //----------------------------------------

        private static FieldInfo? RecipeImageField;

        private static FieldInfo? RecipeActualField;

        //----------------------------------------
        // Apply
        //----------------------------------------

        public static void Apply(
            Harmony harmony)
        {
            //----------------------------------------
            // PC版
            //
            // Stardew Valley 1.6.15 PCでは
            // RepositionElementsがRecipe Componentを
            // 構築・再配置します。
            //----------------------------------------

            MethodInfo? repositionElementsMethod =
                AccessTools.Method(
                    typeof(CraftingPage),
                    "RepositionElements");

            if (repositionElementsMethod != null)
            {
                harmony.Patch(
                    original:
                        repositionElementsMethod,
                    postfix:
                        new HarmonyMethod(
                            typeof(BigCraftableCraftingPagePatch),
                            nameof(RepositionElementsPostfix)));

                return;
            }

            //----------------------------------------
            // Android版
            //
            // Stardew Valley 1.6.15.3 Androidでは
            // RepositionElementsが存在せず、
            // setupRecipesでrecipeImage / recipeActualを
            // 構築します。
            //----------------------------------------

            MethodInfo? setupRecipesMethod =
                AccessTools.Method(
                    typeof(CraftingPage),
                    "setupRecipes",
                    new[]
                    {
                        typeof(List<string>)
                    });

            if (setupRecipesMethod == null)
            {
                throw new InvalidOperationException(
                    "Could not find CraftingPage.RepositionElements or CraftingPage.setupRecipes.");
            }

            //----------------------------------------
            // Android Field取得
            //----------------------------------------

            RecipeImageField =
                AccessTools.Field(
                    typeof(CraftingPage),
                    "recipeImage");

            RecipeActualField =
                AccessTools.Field(
                    typeof(CraftingPage),
                    "recipeActual");

            if (RecipeImageField == null
                || RecipeActualField == null)
            {
                throw new InvalidOperationException(
                    "Could not find CraftingPage recipeImage or recipeActual.");
            }

            //----------------------------------------
            // Android Patch
            //----------------------------------------

            harmony.Patch(
                original:
                    setupRecipesMethod,
                postfix:
                    new HarmonyMethod(
                        typeof(BigCraftableCraftingPagePatch),
                        nameof(SetupRecipesPostfix)));
        }

        //----------------------------------------
        // PC
        // RepositionElements Postfix
        //----------------------------------------

        private static void RepositionElementsPostfix(
            CraftingPage __instance)
        {
            //----------------------------------------
            // Target初期化
            //----------------------------------------

            BigCraftableCraftingPageDrawPatch
                .ClearTargets();

            //----------------------------------------
            // Page確認
            //----------------------------------------

            foreach (Dictionary<
                ClickableTextureComponent,
                CraftingRecipe> page
                in __instance.pagesOfCraftingRecipes)
            {
                //----------------------------------------
                // Recipe確認
                //----------------------------------------

                foreach (KeyValuePair<
                    ClickableTextureComponent,
                    CraftingRecipe> pair
                    in page)
                {
                    ProcessRecipeComponent(
                        pair.Key,
                        pair.Value);
                }
            }
        }

        //----------------------------------------
        // Android
        // setupRecipes Postfix
        //----------------------------------------

        private static void SetupRecipesPostfix(
            CraftingPage __instance)
        {
            //----------------------------------------
            // Target初期化
            //----------------------------------------

            BigCraftableCraftingPageDrawPatch
                .ClearTargets();

            //----------------------------------------
            // Field確認
            //----------------------------------------

            FieldInfo? recipeImageField =
                RecipeImageField;

            FieldInfo? recipeActualField =
                RecipeActualField;

            if (recipeImageField == null
                || recipeActualField == null)
            {
                return;
            }

            //----------------------------------------
            // recipeImage
            //----------------------------------------

            if (recipeImageField.GetValue(__instance)
                is not ClickableTextureComponent[,] recipeImages)
            {
                return;
            }

            //----------------------------------------
            // recipeActual
            //----------------------------------------

            if (recipeActualField.GetValue(__instance)
                is not CraftingRecipe[,] recipes)
            {
                return;
            }

            //----------------------------------------
            // Array Size
            //----------------------------------------

            int width =
                Math.Min(
                    recipeImages.GetLength(0),
                    recipes.GetLength(0));

            int height =
                Math.Min(
                    recipeImages.GetLength(1),
                    recipes.GetLength(1));

            //----------------------------------------
            // Recipe確認
            //----------------------------------------

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    ClickableTextureComponent? component =
                        recipeImages[x, y];

                    CraftingRecipe? recipe =
                        recipes[x, y];

                    if (component == null
                        || recipe == null)
                    {
                        continue;
                    }

                    ProcessRecipeComponent(
                        component,
                        recipe);
                }
            }
        }

        //----------------------------------------
        // ProcessRecipeComponent
        //----------------------------------------

        /// <summary>
        /// Crafting RecipeのComponentに
        /// BigCraftable ExtensionのTexture設定を適用します。
        /// </summary>
        private static void ProcessRecipeComponent(
            ClickableTextureComponent component,
            CraftingRecipe recipe)
        {
            //----------------------------------------
            // BigCraftable確認
            //----------------------------------------

            if (!recipe.bigCraftable)
            {
                return;
            }

            //----------------------------------------
            // Item Data取得
            //----------------------------------------

            var itemData =
                recipe.GetItemData(
                    useFirst: true);

            //----------------------------------------
            // Extension Data取得
            //----------------------------------------

            if (!BigCraftableExtensionDataService
                .TryGetExtensionData(
                    itemData.QualifiedItemId,
                    out _,
                    out var extension))
            {
                return;
            }

            //----------------------------------------
            // Object取得
            //----------------------------------------

            Item item =
                ItemRegistry.Create(
                    itemData.QualifiedItemId);

            if (item is not StardewValley.Object obj)
            {
                return;
            }

            //----------------------------------------
            // Texture Size
            //----------------------------------------

            BigCraftableExtensionTextureService
                .GetTextureSize(
                    obj,
                    extension,
                    out int textureWidth,
                    out int textureHeight);

            //----------------------------------------
            // Texture
            //----------------------------------------

            component.texture =
                BigCraftableExtensionTextureService
                    .GetTexture(
                        obj,
                        extension);

            //----------------------------------------
            // Source Rectangle
            //----------------------------------------

            component.sourceRect =
                BigCraftableExtensionTextureService
                    .GetSourceRect(
                        obj,
                        extension);

            //----------------------------------------
            // Draw Scale計算
            //----------------------------------------

            float drawScale =
                Math.Min(
                    4f,
                    Math.Min(
                        64f / textureWidth,
                        128f / textureHeight));

            //----------------------------------------
            // Draw Scale適用
            //----------------------------------------

            component.baseScale =
                drawScale;

            component.scale =
                drawScale;

            //----------------------------------------
            // 中央配置対象に登録
            //----------------------------------------

            BigCraftableCraftingPageDrawPatch
                .RegisterTarget(
                    component,
                    obj);
        }
    }
}