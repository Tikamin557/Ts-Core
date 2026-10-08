using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using Ts_Core.Models.BigCraftableRelated;

namespace Ts_Core.Services.BigCraftableRelated
{
    /// <summary>
    /// BigCraftable Extensionの
    /// DrawLayerを描画するサービスです。
    /// </summary>
    public static class BigCraftableDrawLayerService
    {
        //----------------------------------------
        // Draw Layers
        //----------------------------------------

        /// <summary>
        /// BigCraftableに設定されている
        /// DrawLayerを描画します。
        /// </summary>
        public static void DrawLayers(
            StardewValley.Object machine,
            SpriteBatch spriteBatch,
            BigCraftableExtensionData extension,
            Texture2D defaultTexture,
            Color color)
        {
            if (extension.DrawLayers == null
                || extension.DrawLayers.Count == 0)
            {
                return;
            }

            //----------------------------------------
            // Anchor
            //----------------------------------------

            Vector2 anchor =
                machine.TileLocation;

            //----------------------------------------
            // Draw Position
            //----------------------------------------

            Vector2 basePosition =
                new(
                    anchor.X * 64f,
                    anchor.Y * 64f - 64f);

            //----------------------------------------
            // DrawLayer一覧
            //----------------------------------------

            foreach (BigCraftableDrawLayerData drawLayer
                in extension.DrawLayers)
            {
                //----------------------------------------
                // SourceRect
                //----------------------------------------

                Rectangle sourceRect =
                    GetSourceRect(
                        drawLayer);

                if (sourceRect.Width <= 0
                    || sourceRect.Height <= 0)
                {
                    continue;
                }

                //----------------------------------------
                // Texture
                //----------------------------------------

                Texture2D texture =
                    GetTexture(
                        drawLayer,
                        defaultTexture);

                //----------------------------------------
                // Position
                //----------------------------------------

                Vector2 position =
                    Game1.GlobalToLocal(
                        Game1.viewport,
                        basePosition
                        + drawLayer.DrawPosition * 4f);

                //----------------------------------------
                // Layer Depth
                //----------------------------------------

                float layerDepth =
                    GetLayerDepth(
                        anchor,
                        extension,
                        drawLayer.DrawLayer);

                //----------------------------------------
                // Draw
                //----------------------------------------

                spriteBatch.Draw(
                    texture,
                    position,
                    sourceRect,
                    color,
                    0f,
                    Vector2.Zero,
                    4f,
                    SpriteEffects.None,
                    layerDepth);
            }
        }

        //----------------------------------------
        // Draw Layers In Menu
        //----------------------------------------

        /// <summary>
        /// Menu表示用のDrawLayerを描画します。
        /// </summary>
        public static void DrawLayersInMenu(
            StardewValley.Object machine,
            SpriteBatch spriteBatch,
            BigCraftableExtensionData extension,
            Texture2D defaultTexture,
            Vector2 drawPosition,
            float drawScale,
            float layerDepth,
            Color color,
            string targetLayer)
        {
            DrawStaticLayers(
                spriteBatch,
                extension,
                defaultTexture,
                drawPosition,
                drawScale,
                layerDepth,
                color,
                targetLayer,
                useCenterOrigin: true);
        }

        //----------------------------------------
        // Draw Layers When Held
        //----------------------------------------

        /// <summary>
        /// Held表示用のDrawLayerを描画します。
        /// </summary>
        public static void DrawLayersWhenHeld(
            StardewValley.Object machine,
            SpriteBatch spriteBatch,
            BigCraftableExtensionData extension,
            Texture2D defaultTexture,
            Vector2 drawPosition,
            float layerDepth,
            string targetLayer)
        {
            DrawStaticLayers(
                spriteBatch,
                extension,
                defaultTexture,
                drawPosition,
                4f,
                layerDepth,
                Color.White,
                targetLayer,
                useCenterOrigin: false);
        }

        //----------------------------------------
        // Draw Layers In Crafting Recipe
        //----------------------------------------

        /// <summary>
        /// Crafting Recipe表示用のDrawLayerを描画します。
        /// </summary>
        public static void DrawLayersInCraftingRecipe(
            StardewValley.Object machine,
            SpriteBatch spriteBatch,
            BigCraftableExtensionData extension,
            Texture2D defaultTexture,
            Vector2 drawPosition,
            float layerDepth,
            string targetLayer)
        {
            DrawStaticLayers(
                spriteBatch,
                extension,
                defaultTexture,
                drawPosition,
                4f,
                layerDepth,
                Color.White,
                targetLayer,
                useCenterOrigin: false);
        }

        //----------------------------------------
        // Draw Static Layers
        //----------------------------------------

        /// <summary>
        /// Menu、Held、Crafting Recipe用の
        /// 静止DrawLayerを描画します。
        /// </summary>
        private static void DrawStaticLayers(
            SpriteBatch spriteBatch,
            BigCraftableExtensionData extension,
            Texture2D defaultTexture,
            Vector2 drawPosition,
            float drawScale,
            float layerDepth,
            Color color,
            string targetLayer,
            bool useCenterOrigin)
        {
            if (extension.DrawLayers == null
                || extension.DrawLayers.Count == 0)
            {
                return;
            }

            //----------------------------------------
            // DrawLayer一覧
            //----------------------------------------

            foreach (BigCraftableDrawLayerData drawLayer
                in extension.DrawLayers)
            {
                //----------------------------------------
                // Draw Layer
                //----------------------------------------

                if (!string.Equals(
                    drawLayer.DrawLayer,
                    targetLayer,
                    StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                //----------------------------------------
                // SourceRect
                //----------------------------------------

                Rectangle sourceRect =
                    drawLayer.SourceRect;

                if (sourceRect.Width <= 0
                    || sourceRect.Height <= 0)
                {
                    continue;
                }

                //----------------------------------------
                // Texture
                //----------------------------------------

                Texture2D texture =
                    GetTexture(
                        drawLayer,
                        defaultTexture);

                //----------------------------------------
                // Position
                //----------------------------------------

                Vector2 position =
                    drawPosition
                    + drawLayer.DrawPosition
                    * drawScale;

                //----------------------------------------
                // Origin
                //----------------------------------------

                Vector2 origin =
                    useCenterOrigin
                        ? new Vector2(
                            sourceRect.Width / 2f,
                            sourceRect.Height / 2f)
                        : Vector2.Zero;

                //----------------------------------------
                // Draw
                //----------------------------------------

                spriteBatch.Draw(
                    texture,
                    position,
                    sourceRect,
                    color,
                    0f,
                    origin,
                    drawScale,
                    SpriteEffects.None,
                    layerDepth);
            }
        }

        //----------------------------------------
        // Texture
        //----------------------------------------

        /// <summary>
        /// DrawLayerで使用するTextureを取得します。
        /// </summary>
        private static Texture2D GetTexture(
            BigCraftableDrawLayerData drawLayer,
            Texture2D defaultTexture)
        {
            if (!string.IsNullOrWhiteSpace(
                drawLayer.Texture))
            {
                return
                    Game1.content.Load<Texture2D>(
                        drawLayer.Texture);
            }

            return defaultTexture;
        }

        //----------------------------------------
        // Layer Depth
        //----------------------------------------

        /// <summary>
        /// DrawLayerの描画深度を取得します。
        /// </summary>
        private static float GetLayerDepth(
            Vector2 anchor,
            BigCraftableExtensionData extension,
            string drawLayer)
        {
            int height =
                Math.Max(
                    1,
                    extension.CollisionHeight);

            //----------------------------------------
            // Collision Bounds
            //----------------------------------------

            float top =
                (anchor.Y - height + 1f)
                * 64f;

            float bottom =
                (anchor.Y + 1f)
                * 64f;

            //----------------------------------------
            // Front
            //----------------------------------------

            if (string.Equals(
                drawLayer,
                "Front",
                StringComparison.OrdinalIgnoreCase))
            {
                return
                    Math.Max(
                        0f,
                        (bottom - 1f) / 10000f);
            }

            //----------------------------------------
            // Back
            //----------------------------------------

            return
                Math.Max(
                    0f,
                    (top + 1f) / 10000f);
        }

        //----------------------------------------
        // SourceRect
        //----------------------------------------

        /// <summary>
        /// DrawLayerの現在のSourceRectを取得します。
        /// </summary>
        private static Rectangle GetSourceRect(
            BigCraftableDrawLayerData drawLayer)
        {
            Rectangle sourceRect =
                drawLayer.SourceRect;

            //----------------------------------------
            // Animationなし
            //----------------------------------------

            if (drawLayer.FrameCount <= 1)
            {
                return sourceRect;
            }

            //----------------------------------------
            // Frame Count
            //----------------------------------------

            int frameCount =
                Math.Max(
                    1,
                    drawLayer.FrameCount);

            //----------------------------------------
            // Current Time
            //----------------------------------------

            int time =
                (int)Game1.currentGameTime
                    .TotalGameTime
                    .TotalMilliseconds;

            //----------------------------------------
            // Total Duration
            //----------------------------------------

            int totalDuration = 0;

            for (int i = 0;
                i < frameCount;
                i++)
            {
                totalDuration +=
                    drawLayer.GetFrameDuration(
                        i);
            }

            if (totalDuration <= 0)
            {
                totalDuration = 1;
            }

            //----------------------------------------
            // Current Frame
            //----------------------------------------

            int animationTime =
                time % totalDuration;

            int frame = 0;
            int elapsed = 0;

            for (int i = 0;
                i < frameCount;
                i++)
            {
                elapsed +=
                    drawLayer.GetFrameDuration(
                        i);

                if (animationTime < elapsed)
                {
                    frame = i;
                    break;
                }
            }

            //----------------------------------------
            // SourceRect Offset
            //----------------------------------------

            if (drawLayer.FramesPerRow < 0)
            {
                sourceRect.X +=
                    sourceRect.Width
                    * frame;
            }
            else
            {
                int framesPerRow =
                    Math.Max(
                        1,
                        drawLayer.FramesPerRow);

                sourceRect.X +=
                    sourceRect.Width
                    * (frame % framesPerRow);

                sourceRect.Y +=
                    sourceRect.Height
                    * (frame / framesPerRow);
            }

            return sourceRect;
        }
    }
}