using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Delegates;
using StardewValley.Objects;
using Ts_Core.Models;

namespace Ts_Core.Services.BuildingRelated
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
            BuildingProviderModel provider,
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
                // 個別DrawLayer有効判定
                //----------------------------------------

                if (!BuildingProviderService.IsEnabledField(
                        provider,
                        drawLayer.EnabledField))
                {
                    continue;
                }

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
            // フレーム数
            //----------------------------------------

            int frameCount =
                Math.Max(
                    1,
                    drawLayer.FrameCount);

            //----------------------------------------
            // 現在時刻
            //----------------------------------------

            int time =
                (int)Game1.currentGameTime
                    .TotalGameTime
                    .TotalMilliseconds;

            //----------------------------------------
            // 1ループ分の総時間
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

            //----------------------------------------
            // 不正値対策
            //----------------------------------------

            if (totalDuration <= 0)
                totalDuration = 1;

            //----------------------------------------
            // 現在フレーム
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
            IReadOnlyList<BuildingProviderModel> providers =
                BuildingProviderService.GetProvidersForBuilding(
                    building.buildingType.Value);

            foreach (BuildingProviderModel provider in providers)
            {
                //----------------------------------------
                // Building Provider全体
                //----------------------------------------

                if (!BuildingProviderService.IsProviderEnabled(
                        provider))
                {
                    continue;
                }

                //----------------------------------------
                // DrawLayers
                //----------------------------------------

                if (!BuildingProviderService.IsEnabledField(
                        provider,
                        provider.DrawLayersEnabledField))
                {
                    continue;
                }

                DrawLayers(
                    building,
                    spriteBatch,
                    provider,
                    provider.DrawLayers,
                    drawInBackground);
            }
        }

        //----------------------------------------
        // 建設メニュー用DrawLayer描画
        //----------------------------------------

        /// <summary>
        /// 建設メニューのBuildingプレビューに
        /// TsCore DrawLayerを描画します。
        /// </summary>
        public static void DrawLayersInMenu(
            Building building,
            SpriteBatch spriteBatch,
            int x,
            int y,
            GameLocation targetLocation)
        {
            IReadOnlyList<BuildingProviderModel> providers =
                BuildingProviderService.GetProvidersForBuilding(
                    building.buildingType.Value);

            if (providers.Count == 0)
                return;

            var buildingData =
                building.GetData();

            //----------------------------------------
            // BuildingData.DrawOffset
            //----------------------------------------

            if (buildingData != null)
            {
                x +=
                    (int)(buildingData.DrawOffset.X * 4f);

                y +=
                    (int)(buildingData.DrawOffset.Y * 4f);
            }

            //----------------------------------------
            // 基本SortY
            //----------------------------------------

            float baseSortY =
                building.tilesHigh.Value * 64f;

            //----------------------------------------
            // Provider一覧
            //----------------------------------------

            foreach (BuildingProviderModel provider in providers)
            {
                //----------------------------------------
                // Building Provider全体
                //----------------------------------------

                if (!BuildingProviderService.IsProviderEnabled(
                        provider))
                {
                    continue;
                }

                //----------------------------------------
                // DrawLayers全体
                //----------------------------------------

                if (!BuildingProviderService.IsEnabledField(
                        provider,
                        provider.DrawLayersEnabledField))
                {
                    continue;
                }

                //----------------------------------------
                // DrawLayer一覧
                //----------------------------------------

                foreach (BuildingDrawLayerModel drawLayer
                         in provider.DrawLayers)
                {
                    //----------------------------------------
                    // 個別DrawLayer
                    //----------------------------------------

                    if (!BuildingProviderService.IsEnabledField(
                            provider,
                            drawLayer.EnabledField))
                    {
                        continue;
                    }

                    //----------------------------------------
                    // Condition
                    //----------------------------------------

                    if (!CheckCondition(
                            drawLayer.Condition,
                            targetLocation))
                    {
                        continue;
                    }

                    //----------------------------------------
                    // Chest条件
                    //----------------------------------------
                    //
                    // 建設メニューには実際のChestが存在しないため、
                    // バニラdrawInMenuと同様に表示しません。
                    //----------------------------------------

                    if (!string.IsNullOrWhiteSpace(
                            drawLayer.OnlyDrawIfChestHasContents))
                    {
                        continue;
                    }

                    //----------------------------------------
                    // SortY
                    //----------------------------------------

                    float sortY =
                        baseSortY
                        - drawLayer.SortTileOffset * 64f;

                    sortY += 1f;

                    if (drawLayer.DrawInBackground)
                    {
                        sortY = 0f;
                    }

                    sortY /= 10000f;

                    //----------------------------------------
                    // SourceRect
                    //----------------------------------------

                    Rectangle sourceRect =
                        GetSourceRect(
                            drawLayer);

                    sourceRect =
                        building.ApplySourceRectOffsets(
                            sourceRect);

                    //----------------------------------------
                    // Texture
                    //----------------------------------------

                    Texture2D layerTexture =
                        building.texture.Value;

                    if (!string.IsNullOrWhiteSpace(
                            drawLayer.Texture))
                    {
                        layerTexture =
                            Game1.content.Load<Texture2D>(
                                drawLayer.Texture);
                    }

                    //----------------------------------------
                    // 描画
                    //----------------------------------------

                    spriteBatch.Draw(
                        layerTexture,
                        new Vector2(x, y)
                            + drawLayer.DrawPosition * 4f,
                        sourceRect,
                        Color.White,
                        0f,
                        Vector2.Zero,
                        4f,
                        SpriteEffects.None,
                        sortY);
                }
            }
        }
    }
}