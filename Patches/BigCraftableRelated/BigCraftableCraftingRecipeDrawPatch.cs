using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using Ts_Core.Services.BigCraftableRelated;

namespace Ts_Core.Patches.BigCraftableRelated
{
    /// <summary>
    /// BigCraftable Extensionの
    /// Texture設定をCrafting Recipe描画に適用するPatchです。
    /// </summary>
    public static class BigCraftableCraftingRecipeDrawPatch
    {
        public static void Apply(
            Harmony harmony)
        {
            harmony.Patch(
                original:
                    AccessTools.Method(
                        typeof(CraftingRecipe),
                        nameof(CraftingRecipe.drawMenuView),
                        new[]
                        {
                            typeof(SpriteBatch),
                            typeof(int),
                            typeof(int),
                            typeof(float),
                            typeof(bool)
                        }),
                prefix:
                    new HarmonyMethod(
                        typeof(BigCraftableCraftingRecipeDrawPatch),
                        nameof(DrawMenuViewPrefix)));
        }

        private static bool DrawMenuViewPrefix(
            CraftingRecipe __instance,
            SpriteBatch b,
            int x,
            int y,
            float layerDepth,
            bool shadow)
        {
            //----------------------------------------
            // BigCraftable確認
            //----------------------------------------

            if (!__instance.bigCraftable)
            {
                return true;
            }

            //----------------------------------------
            // Item Data取得
            //----------------------------------------

            ParsedItemData itemData =
                __instance.GetItemData(
                    useFirst: true);

            if (itemData == null)
            {
                return true;
            }

            //----------------------------------------
            // Extension Data取得
            //----------------------------------------

            if (!BigCraftableExtensionDataService
                .TryGetExtensionData(
                    itemData.QualifiedItemId,
                    out _,
                    out var extension))
            {
                return true;
            }

            //----------------------------------------
            // Object取得
            //----------------------------------------

            Item item =
                ItemRegistry.Create(
                    itemData.QualifiedItemId);

            if (item is not StardewValley.Object obj)
            {
                return true;
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
            // Vanilla設定なら元の処理を使用
            //----------------------------------------

            bool hasDrawLayers =
                extension.DrawLayers != null
                && extension.DrawLayers.Count > 0;

            if (textureWidth == 16
                && textureHeight == 32
                && string.IsNullOrWhiteSpace(
                    extension.Texture)
                && string.IsNullOrWhiteSpace(
                    extension.TexturePosition)
                && !hasDrawLayers)
            {
                return true;
            }

            //----------------------------------------
            // Texture
            //----------------------------------------

            Texture2D texture =
                BigCraftableExtensionTextureService
                    .GetTexture(
                        obj,
                        extension);

            //----------------------------------------
            // Source Rectangle
            //----------------------------------------

            Rectangle sourceRect =
                BigCraftableExtensionTextureService
                    .GetSourceRect(
                        obj,
                        extension);

            //----------------------------------------
            // Draw Position
            //----------------------------------------

            Vector2 drawPosition =
                new(
                    x,
                    y);

            //----------------------------------------
            // Back Draw Layers
            //----------------------------------------

            BigCraftableDrawLayerService
                .DrawLayersInCraftingRecipe(
                    obj,
                    b,
                    extension,
                    texture,
                    drawPosition,
                    layerDepth - 0.0001f,
                    "Back");

            //----------------------------------------
            // Draw
            //----------------------------------------

            Utility.drawWithShadow(
                b,
                texture,
                drawPosition,
                sourceRect,
                Color.White,
                0f,
                Vector2.Zero,
                4f,
                flipped: false,
                layerDepth);

            //----------------------------------------
            // Front Draw Layers
            //----------------------------------------

            BigCraftableDrawLayerService
                .DrawLayersInCraftingRecipe(
                    obj,
                    b,
                    extension,
                    texture,
                    drawPosition,
                    layerDepth + 0.0001f,
                    "Front");

            //----------------------------------------
            // Vanilla描画をスキップ
            //----------------------------------------

            return false;
        }
    }
}