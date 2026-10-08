using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Mods;
using StardewValley.TerrainFeatures;
using xTile.Layers;

namespace Ts_Core.Services.MapRelated.TsCoreAlwaysFront
{
    /// <summary>
    /// Map内の「TsCoreAlwaysFront」レイヤーを、プレイヤーや通常Frontより前、
    /// AlwaysFrontより後ろに描画します。
    /// 果樹はバニラの描画順を維持しつつ、樹冠部分のみこのレイヤーより前へ再描画します。
    /// </summary>
    internal static class TsCoreAlwaysFrontService
    {
        private const string LayerName = "TsCoreAlwaysFront";

        // FruitTreeの成木スプライトは48x64pxで、下端16pxが幹・根元側です。
        // 樹冠側48pxだけを再描画するため、画面上では下端64pxを除外します。
        private const int FruitTreeCanopyBottomOffset = 64;

        private static readonly RasterizerState ScissorRasterizerState =
            new RasterizerState
            {
                ScissorTestEnable = true
            };

        //----------------------------------------
        // Initialize
        //----------------------------------------

        /// <summary>
        /// TsCoreAlwaysFrontの描画イベントを登録します。
        /// </summary>
        internal static void Initialize(
            IModHelper helper)
        {
            helper.Events.Display.RenderedStep +=
                OnRenderedStep;
        }

        //----------------------------------------
        // Events
        //----------------------------------------

        /// <summary>
        /// World_Sortedの通常描画が完了した後にTsCoreAlwaysFrontを描画し、
        /// その前面に果樹の樹冠部分だけを再描画します。
        /// </summary>
        private static void OnRenderedStep(
            object? sender,
            RenderedStepEventArgs e)
        {
            if (e.Step != RenderSteps.World_Sorted
                || Game1.currentLocation?.Map == null)
            {
                return;
            }

            Layer? layer =
                Game1.currentLocation.Map.Layers
                    .FirstOrDefault(p => p.Id == LayerName);

            if (layer == null)
            {
                return;
            }

            Draw(
                Game1.currentLocation,
                layer,
                e.SpriteBatch);
        }

        //----------------------------------------
        // Draw
        //----------------------------------------

        /// <summary>
        /// バニラのWorld_Sortedを確定した後、TsCoreAlwaysFrontと果樹の樹冠を描画し、
        /// SMAPI側が処理を継続できるようWorld_Sortedと同じ設定でバッチを再開します。
        /// </summary>
        private static void Draw(
            GameLocation location,
            Layer layer,
            SpriteBatch spriteBatch)
        {
            spriteBatch.End();

            //----------------------------------------
            // TsCoreAlwaysFront
            //----------------------------------------

            spriteBatch.Begin(
                SpriteSortMode.Texture,
                BlendState.AlphaBlend,
                SamplerState.PointClamp);

            layer.Draw(
                Game1.mapDisplayDevice,
                Game1.viewport,
                xTile.Dimensions.Location.Origin,
                wrapAround: false,
                4,
                -1f);

            spriteBatch.End();

            //----------------------------------------
            // FruitTree canopy
            //----------------------------------------

            DrawFruitTreeCanopies(
                location,
                layer,
                spriteBatch);

            //----------------------------------------
            // Resume World_Sorted
            //----------------------------------------

            spriteBatch.Begin(
                SpriteSortMode.FrontToBack,
                BlendState.AlphaBlend,
                SamplerState.PointClamp);
        }

        /// <summary>
        /// 成木のFruitTreeを、TsCoreAlwaysFrontのタイルと樹冠が重なる範囲だけ再描画します。
        /// 重なっていない樹冠は再描画しないことで、半透明時の二重描画による濃化を防ぎます。
        /// FruitTree本来の描画処理をそのまま呼ぶため、テクスチャや果実等の描画内容はバニラ側に追従します。
        /// </summary>
        private static void DrawFruitTreeCanopies(
            GameLocation location,
            Layer layer,
            SpriteBatch spriteBatch)
        {
            Rectangle originalScissor =
                Game1.graphics.GraphicsDevice.ScissorRectangle;

            try
            {
                foreach (var pair in location.terrainFeatures.Pairs)
                {
                    if (pair.Value is not FruitTree fruitTree
                        || fruitTree.growthStage.Value < 4)
                    {
                        continue;
                    }

                    Vector2 tile = pair.Key;

                    int left =
                        (int)(tile.X * 64f)
                        - Game1.viewport.X
                        - 64;

                    int top =
                        (int)(tile.Y * 64f)
                        - Game1.viewport.Y
                        - 256;

                    Rectangle canopyBounds =
                        new Rectangle(
                            left,
                            top,
                            192,
                            256 - FruitTreeCanopyBottomOffset);

                    Rectangle screenBounds =
                        Game1.graphics.GraphicsDevice.Viewport.Bounds;

                    canopyBounds =
                        Rectangle.Intersect(
                            canopyBounds,
                            screenBounds);

                    if (canopyBounds.Width <= 0
                        || canopyBounds.Height <= 0)
                    {
                        continue;
                    }

                    DrawFruitTreeCanopyOverLayerTiles(
                        fruitTree,
                        layer,
                        canopyBounds,
                        spriteBatch);
                }
            }
            finally
            {
                Game1.graphics.GraphicsDevice.ScissorRectangle =
                    originalScissor;
            }
        }

        /// <summary>
        /// FruitTreeの樹冠とTsCoreAlwaysFrontの実タイルが重なる範囲だけを再描画します。
        /// </summary>
        private static void DrawFruitTreeCanopyOverLayerTiles(
            FruitTree fruitTree,
            Layer layer,
            Rectangle canopyBounds,
            SpriteBatch spriteBatch)
        {
            int tileXMin =
                Math.Max(
                    0,
                    (canopyBounds.Left + Game1.viewport.X) / 64);

            int tileYMin =
                Math.Max(
                    0,
                    (canopyBounds.Top + Game1.viewport.Y) / 64);

            int tileXMax =
                Math.Min(
                    layer.LayerWidth - 1,
                    (canopyBounds.Right - 1 + Game1.viewport.X) / 64);

            int tileYMax =
                Math.Min(
                    layer.LayerHeight - 1,
                    (canopyBounds.Bottom - 1 + Game1.viewport.Y) / 64);

            for (int tileY = tileYMin; tileY <= tileYMax; tileY++)
            {
                for (int tileX = tileXMin; tileX <= tileXMax; tileX++)
                {
                    if (layer.Tiles[tileX, tileY] == null)
                    {
                        continue;
                    }

                    Rectangle layerTileBounds =
                        new Rectangle(
                            tileX * 64 - Game1.viewport.X,
                            tileY * 64 - Game1.viewport.Y,
                            64,
                            64);

                    Rectangle redrawBounds =
                        Rectangle.Intersect(
                            canopyBounds,
                            layerTileBounds);

                    if (redrawBounds.Width <= 0
                        || redrawBounds.Height <= 0)
                    {
                        continue;
                    }

                    Game1.graphics.GraphicsDevice.ScissorRectangle =
                        redrawBounds;

                    spriteBatch.Begin(
                        SpriteSortMode.Deferred,
                        BlendState.AlphaBlend,
                        SamplerState.PointClamp,
                        DepthStencilState.None,
                        ScissorRasterizerState);

                    fruitTree.draw(
                        spriteBatch);

                    spriteBatch.End();
                }
            }
        }
    }
}
