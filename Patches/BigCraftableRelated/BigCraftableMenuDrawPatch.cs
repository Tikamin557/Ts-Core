using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using Ts_Core.Services.BigCraftableRelated;

namespace Ts_Core.Patches.BigCraftableRelated
{
    /// <summary>
    /// BigCraftable Extensionの
    /// Texture SizeをMenu描画に適用するPatchです。
    /// </summary>
    public static class BigCraftableMenuDrawPatch
    {
        //----------------------------------------
        // Apply
        //----------------------------------------

        /// <summary>
        /// Patchを適用します。
        /// </summary>
        public static void Apply(
            Harmony harmony)
        {
            harmony.Patch(
                original:
                    AccessTools.Method(
                        typeof(StardewValley.Object),
                        nameof(StardewValley.Object.drawInMenu),
                        new[]
                        {
                            typeof(SpriteBatch),
                            typeof(Vector2),
                            typeof(float),
                            typeof(float),
                            typeof(float),
                            typeof(StackDrawType),
                            typeof(Color),
                            typeof(bool)
                        }),
                prefix:
                    new HarmonyMethod(
                        typeof(BigCraftableMenuDrawPatch),
                        nameof(DrawInMenuPrefix)));
        }

        //----------------------------------------
        // Prefix
        //----------------------------------------

        /// <summary>
        /// BigCraftable Extension使用時の
        /// Menu描画を行います。
        /// </summary>
        private static bool DrawInMenuPrefix(
            StardewValley.Object __instance,
            SpriteBatch spriteBatch,
            Vector2 location,
            float scaleSize,
            float transparency,
            float layerDepth,
            StackDrawType drawStackNumber,
            Color color,
            bool drawShadow)
        {
            //----------------------------------------
            // BigCraftable
            //----------------------------------------

            if (!__instance.bigCraftable.Value)
            {
                return true;
            }

            //----------------------------------------
            // Extension Data
            //----------------------------------------

            if (!BigCraftableExtensionDataService
                .TryGetExtensionData(
                    __instance,
                    out _,
                    out var extension))
            {
                return true;
            }

            //----------------------------------------
            // Texture Size
            //----------------------------------------

            BigCraftableExtensionTextureService
                .GetTextureSize(
                    __instance,
                    extension,
                    out int textureWidth,
                    out int textureHeight);

            //----------------------------------------
            // Vanilla Size
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
            // Recipe補正
            //----------------------------------------

            __instance.AdjustMenuDrawForRecipes(
                ref transparency,
                ref scaleSize);

            //----------------------------------------
            // Texture
            //----------------------------------------

            Texture2D texture =
                BigCraftableExtensionTextureService
                    .GetTexture(
                        __instance,
                        extension);

            //----------------------------------------
            // Vanilla Draw Scale
            //----------------------------------------

            float drawnScale =
                scaleSize;

            if (drawnScale > 0.2f)
            {
                drawnScale /=
                    2f;
            }

            //----------------------------------------
            // Source Rectangle
            //----------------------------------------

            Rectangle sourceRect =
                BigCraftableExtensionTextureService
                    .GetSourceRect(
                        __instance,
                        extension);

            //----------------------------------------
            // Draw Scale
            //----------------------------------------

            float vanillaDrawScale =
                4f * drawnScale;

            float fitScale =
                Math.Min(
                    64f / textureWidth,
                    64f / textureHeight);

            float baseDrawScale =
                Math.Min(
                    2f,
                    fitScale);

            float hoverScale =
                vanillaDrawScale / 2f;

            float drawScale =
                baseDrawScale
                * hoverScale;

            //----------------------------------------
            // Draw Size
            //----------------------------------------

            float drawHeight =
                textureHeight
                * drawScale;

            //----------------------------------------
            // Draw Position
            //----------------------------------------

            Vector2 drawPosition =
                location
                + new Vector2(
                    32f,
                    64f - drawHeight / 2f);

            //----------------------------------------
            // Back Draw Layers
            //----------------------------------------

            BigCraftableDrawLayerService
                .DrawLayersInMenu(
                    __instance,
                    spriteBatch,
                    extension,
                    texture,
                    drawPosition,
                    drawScale,
                    layerDepth - 0.000001f,
                    color * transparency,
                    "Back");

            //----------------------------------------
            // Draw
            //----------------------------------------

            spriteBatch.Draw(
                texture,
                drawPosition,
                sourceRect,
                color * transparency,
                0f,
                new Vector2(
                    textureWidth / 2f,
                    textureHeight / 2f),
                drawScale,
                SpriteEffects.None,
                layerDepth);

            //----------------------------------------
            // Front Draw Layers
            //----------------------------------------

            BigCraftableDrawLayerService
                .DrawLayersInMenu(
                    __instance,
                    spriteBatch,
                    extension,
                    texture,
                    drawPosition,
                    drawScale,
                    layerDepth + 0.000001f,
                    color * transparency,
                    "Front");

            //----------------------------------------
            // Menu Icons
            //----------------------------------------

            __instance.DrawMenuIcons(
                spriteBatch,
                location,
                scaleSize,
                transparency,
                layerDepth,
                drawStackNumber,
                color);

            //----------------------------------------
            // Vanilla描画をスキップ
            //----------------------------------------

            return false;
        }
    }
}