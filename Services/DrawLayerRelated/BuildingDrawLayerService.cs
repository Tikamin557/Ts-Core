using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Delegates;
using StardewValley.Objects;
using Ts_Core.Models;
using Ts_Core.Services.LightRelated;

namespace Ts_Core.Services.DrawLayerRelated
{
    /// <summary>
    /// TsCoreの条件付きBuilding DrawLayerを描画するサービスです。
    /// </summary>
    public static class BuildingDrawLayerService
    {
        //----------------------------------------
        // DrawLayer描画
        //----------------------------------------

        /// <summary>
        /// 指定した建物のTsCore DrawLayerを描画します。
        /// </summary>
        public static void DrawLayers(
            Building building,
            SpriteBatch spriteBatch,
            IReadOnlyList<BuildingDrawLayerModel> drawLayers,
            bool drawInBackground)
        {
            if (drawLayers.Count == 0)
                return;

            if (building.isMoving)
                return;

            if (building.daysOfConstructionLeft.Value > 0)
                return;

            GameLocation? location =
                building.GetParentLocation();

            if (location == null)
                return;

            //----------------------------------------
            // 建物描画用基本値
            //----------------------------------------

            Rectangle mainSourceRect =
                building.getSourceRect();

            Vector2 drawOrigin =
                new(
                    0f,
                    mainSourceRect.Height);

            Vector2 drawPosition =
                new(
                    building.tileX.Value * 64,
                    building.tileY.Value * 64
                        + building.tilesHigh.Value * 64);

            float baseSortY =
                (building.tileY.Value
                    + building.tilesHigh.Value)
                * 64f;

            //----------------------------------------
            // DrawLayer一覧
            //----------------------------------------

            foreach (BuildingDrawLayerModel drawLayer in drawLayers)
            {
                //----------------------------------------
                // Background / Foreground判定
                //----------------------------------------

                if (drawLayer.DrawInBackground != drawInBackground)
                    continue;

                //----------------------------------------
                // Condition判定
                //----------------------------------------

                if (!CheckCondition(
                        drawLayer.Condition,
                        location))
                {
                    continue;
                }

                //----------------------------------------
                // Chest条件
                //----------------------------------------

                if (!string.IsNullOrWhiteSpace(
                        drawLayer.OnlyDrawIfChestHasContents))
                {
                    Chest? chest =
                        building.GetBuildingChest(
                            drawLayer.OnlyDrawIfChestHasContents);

                    if (chest == null || chest.isEmpty())
                        continue;
                }

                //----------------------------------------
                // SourceRect
                //----------------------------------------

                Rectangle sourceRect =
                    GetSourceRect(
                        drawLayer);

                // BuildingDataのSeasonOffset等を適用
                sourceRect =
                    building.ApplySourceRectOffsets(
                        sourceRect);

                //----------------------------------------
                // Texture
                //----------------------------------------

                Texture2D layerTexture;

                if (!string.IsNullOrWhiteSpace(
                        drawLayer.Texture))
                {
                    layerTexture =
                        Game1.content.Load<Texture2D>(
                            drawLayer.Texture);
                }
                else
                {
                    var buildingData =
                        building.GetData();

                    if (buildingData == null
                        || string.IsNullOrWhiteSpace(
                            buildingData.Texture))
                    {
                        layerTexture =
                            building.texture.Value;
                    }
                    else
                    {
                        layerTexture =
                            Game1.content.Load<Texture2D>(
                                buildingData.Texture);
                    }
                }

                //----------------------------------------
                // Animal Door Offset
                //----------------------------------------

                Vector2 drawOffset =
                    Vector2.Zero;

                if (drawLayer.AnimalDoorOffset != Point.Zero)
                {
                    drawOffset =
                        new Vector2(
                            drawLayer.AnimalDoorOffset.X
                                * building.animalDoorOpenAmount.Value,
                            drawLayer.AnimalDoorOffset.Y
                                * building.animalDoorOpenAmount.Value);
                }

                //----------------------------------------
                // DrawPosition
                //----------------------------------------

                Vector2 position =
                    Game1.GlobalToLocal(
                        Game1.viewport,
                        drawPosition
                        + (drawOffset
                           - drawOrigin
                           + drawLayer.DrawPosition)
                        * 4f);

                //----------------------------------------
                // SortY
                //----------------------------------------

                float sortY;

                if (drawInBackground)
                {
                    sortY = 0f;
                }
                else
                {
                    sortY =
                        baseSortY
                        - drawLayer.SortTileOffset * 64f;

                    sortY += 1f;
                    sortY /= 10000f;
                }

                //----------------------------------------
                // Draw
                //----------------------------------------

                spriteBatch.Draw(
                    layerTexture,
                    position,
                    sourceRect,
                    building.color * building.alpha,
                    0f,
                    Vector2.Zero,
                    4f,
                    SpriteEffects.None,
                    sortY);
            }
        }

        //----------------------------------------
        // Condition判定
        //----------------------------------------

        /// <summary>
        /// Game State Query条件を評価します。
        /// </summary>
        private static bool CheckCondition(
            string? condition,
            GameLocation location)
        {
            if (string.IsNullOrWhiteSpace(condition))
                return true;

            GameStateQueryContext context =
                new(
                    location,
                    Game1.player,
                    null,
                    null,
                    Game1.random);

            return GameStateQuery.CheckConditions(
                condition,
                context);
        }

        //----------------------------------------
        // SourceRect取得
        //----------------------------------------

        /// <summary>
        /// DrawLayerの現在のSourceRectを取得します。
        /// </summary>
        private static Rectangle GetSourceRect(
            BuildingDrawLayerModel drawLayer)
        {
            Rectangle sourceRect =
                drawLayer.SourceRect;

            //----------------------------------------
            // アニメーションなし
            //----------------------------------------

            if (drawLayer.FrameCount <= 1)
                return sourceRect;

            //----------------------------------------
            // 不正値対策
            //----------------------------------------

            int frameDuration =
                Math.Max(
                    1,
                    drawLayer.FrameDuration);

            int frameCount =
                Math.Max(
                    1,
                    drawLayer.FrameCount);

            //----------------------------------------
            // 現在フレーム
            //----------------------------------------

            int time =
                (int)Game1.currentGameTime
                    .TotalGameTime
                    .TotalMilliseconds;

            int frame =
                time / frameDuration;

            frame %= frameCount;

            //----------------------------------------
            // SourceRect移動
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

        //----------------------------------------
        // BuildingからDrawLayer描画
        //----------------------------------------

        /// <summary>
        /// 指定した建物に登録されているTsCore DrawLayerを描画します。
        /// </summary>
        public static void DrawLayers(
            Building building,
            SpriteBatch spriteBatch,
            bool drawInBackground)
        {
            IReadOnlyList<BuildingLightProviderModel> providers =
                BuildingLightService.GetProvidersForBuilding(
                    building.buildingType.Value);

            foreach (BuildingLightProviderModel provider in providers)
            {
                DrawLayers(
                    building,
                    spriteBatch,
                    provider.DrawLayers,
                    drawInBackground);
            }
        }
    }
}