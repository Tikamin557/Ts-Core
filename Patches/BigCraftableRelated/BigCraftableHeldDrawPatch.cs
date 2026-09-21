using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using Ts_Core.Services.BigCraftableRelated;

namespace Ts_Core.Patches.BigCraftableRelated
{
    /// <summary>
    /// BigCraftable Extensionの
    /// Texture SizeをHeld描画に適用するPatchです。
    /// </summary>
    public static class BigCraftableHeldDrawPatch
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
                        nameof(StardewValley.Object.drawWhenHeld),
                        new[]
                        {
                            typeof(SpriteBatch),
                            typeof(Vector2),
                            typeof(Farmer)
                        }),
                prefix:
                    new HarmonyMethod(
                        typeof(BigCraftableHeldDrawPatch),
                        nameof(DrawWhenHeldPrefix)));
        }

        //----------------------------------------
        // Prefix
        //----------------------------------------

        /// <summary>
        /// BigCraftable Extension使用時の
        /// Held描画を行います。
        /// </summary>
        private static bool DrawWhenHeldPrefix(
            StardewValley.Object __instance,
            SpriteBatch spriteBatch,
            Vector2 objectPosition,
            Farmer f)
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
            // Texture
            //----------------------------------------

            Texture2D texture =
                BigCraftableExtensionTextureService
                    .GetTexture(
                        __instance,
                        extension);

            //----------------------------------------
            // Source Rectangle
            //----------------------------------------

            Rectangle sourceRect =
                BigCraftableExtensionTextureService
                    .GetSourceRect(
                        __instance,
                        extension);

            //----------------------------------------
            // 描画位置補正
            //----------------------------------------

            // 横方向は中央を維持
            float widthDifference =
                (textureWidth - 16)
                * 4f;

            objectPosition.X -=
                widthDifference / 2f;

            // 縦方向は下端を維持
            float heightDifference =
                (textureHeight - 32)
                * 4f;

            objectPosition.Y -=
                heightDifference;

            //----------------------------------------
            // Draw Layer
            //----------------------------------------

            float drawLayer =
                Math.Max(
                    0f,
                    (f.StandingPixel.Y + 3)
                    / 10000f);

            //----------------------------------------
            // Back Draw Layers
            //----------------------------------------

            BigCraftableDrawLayerService
                .DrawLayersWhenHeld(
                    __instance,
                    spriteBatch,
                    extension,
                    texture,
                    objectPosition,
                    drawLayer - 0.000001f,
                    "Back");

            //----------------------------------------
            // Draw
            //----------------------------------------

            spriteBatch.Draw(
                texture,
                objectPosition,
                sourceRect,
                Color.White,
                0f,
                Vector2.Zero,
                4f,
                SpriteEffects.None,
                drawLayer);

            //----------------------------------------
            // Front Draw Layers
            //----------------------------------------

            BigCraftableDrawLayerService
                .DrawLayersWhenHeld(
                    __instance,
                    spriteBatch,
                    extension,
                    texture,
                    objectPosition,
                    drawLayer + 0.000001f,
                    "Front");

            //----------------------------------------
            // Vanilla描画をスキップ
            //----------------------------------------

            return false;
        }
    }
}