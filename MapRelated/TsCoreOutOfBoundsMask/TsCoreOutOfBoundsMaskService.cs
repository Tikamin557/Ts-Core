using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Mods;

namespace Ts_Core.Services.MapRelated.TsCoreOutOfBoundsMask
{
    /// <summary>
    /// TsCoreOutOfBoundsMask Map Propertyが有効なLocationで、
    /// Map範囲外をプレイヤー等より前面・AlwaysFrontより背面の黒で覆います。
    /// </summary>
    internal static class TsCoreOutOfBoundsMaskService
    {
        private const string MapPropertyName = "TsCoreOutOfBoundsMask";

        //----------------------------------------
        // Initialize
        //----------------------------------------

        /// <summary>
        /// Map範囲外マスクの描画イベントを登録します。
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
        /// World_Sortedの描画完了後に、
        /// 有効なLocationのMap範囲外を黒で覆います。
        /// </summary>
        private static void OnRenderedStep(
            object? sender,
            RenderedStepEventArgs e)
        {
            if (e.Step != RenderSteps.World_Sorted
                || Game1.currentLocation == null)
            {
                return;
            }

            Draw(
                Game1.currentLocation,
                e.SpriteBatch);
        }

        //----------------------------------------
        // Map Property
        //----------------------------------------

        /// <summary>
        /// 指定LocationでMap範囲外マスクが有効か確認します。
        /// </summary>
        private static bool IsEnabled(
            GameLocation? location)
        {
            if (location?.Map == null
                || !location.Map.Properties.TryGetValue(
                    MapPropertyName,
                    out var propertyValue))
            {
                return false;
            }

            string value =
                propertyValue?.ToString()?.Trim()
                ?? string.Empty;

            return bool.TryParse(
                value,
                out bool enabled)
                && enabled;
        }

        //----------------------------------------
        // Draw
        //----------------------------------------

        /// <summary>
        /// 画面内に見えているMap範囲外を黒で覆います。
        /// </summary>
        internal static void Draw(
            GameLocation location,
            SpriteBatch spriteBatch)
        {
            if (!IsEnabled(location)
                || location.Map.Layers.Count == 0)
            {
                return;
            }

            int screenWidth =
                Game1.graphics.GraphicsDevice.Viewport.Width;

            int screenHeight =
                Game1.graphics.GraphicsDevice.Viewport.Height;

            int mapLeft =
                -Game1.viewport.X;

            int mapTop =
                -Game1.viewport.Y;

            int mapRight =
                mapLeft
                + location.Map.Layers[0].LayerWidth * 64;

            int mapBottom =
                mapTop
                + location.Map.Layers[0].LayerHeight * 64;

            //----------------------------------------
            // Left / Right
            //----------------------------------------

            int leftWidth =
                Math.Clamp(
                    mapLeft,
                    0,
                    screenWidth);

            if (leftWidth > 0)
            {
                spriteBatch.Draw(
                    Game1.fadeToBlackRect,
                    new Rectangle(
                        0,
                        0,
                        leftWidth,
                        screenHeight),
                    null,
                    Color.Black,
                    0f,
                    Vector2.Zero,
                    SpriteEffects.None,
                    1f);
            }

            int rightX =
                Math.Clamp(
                    mapRight,
                    0,
                    screenWidth);

            if (rightX < screenWidth)
            {
                spriteBatch.Draw(
                    Game1.fadeToBlackRect,
                    new Rectangle(
                        rightX,
                        0,
                        screenWidth - rightX,
                        screenHeight),
                    null,
                    Color.Black,
                    0f,
                    Vector2.Zero,
                    SpriteEffects.None,
                    1f);
            }

            //----------------------------------------
            // Top / Bottom
            //----------------------------------------

            int topHeight =
                Math.Clamp(
                    mapTop,
                    0,
                    screenHeight);

            if (topHeight > 0)
            {
                spriteBatch.Draw(
                    Game1.fadeToBlackRect,
                    new Rectangle(
                        0,
                        0,
                        screenWidth,
                        topHeight),
                    null,
                    Color.Black,
                    0f,
                    Vector2.Zero,
                    SpriteEffects.None,
                    1f);
            }

            int bottomY =
                Math.Clamp(
                    mapBottom,
                    0,
                    screenHeight);

            if (bottomY < screenHeight)
            {
                spriteBatch.Draw(
                    Game1.fadeToBlackRect,
                    new Rectangle(
                        0,
                        bottomY,
                        screenWidth,
                        screenHeight - bottomY),
                    null,
                    Color.Black,
                    0f,
                    Vector2.Zero,
                    SpriteEffects.None,
                    1f);
            }
        }
    }
}
