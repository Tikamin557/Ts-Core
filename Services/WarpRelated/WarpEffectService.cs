using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Extensions;

namespace Ts_Core.Services.WarpRelated
{
    /// <summary>
    /// Warp時の演出処理を管理します。
    /// </summary>
    internal static class WarpEffectService
    {
        //----------------------------------------
        // Fade Duration
        //----------------------------------------

        // MagicWarpで、画面が完全に暗転するまでのフェードイン時間
        internal const int MagicFadeDuration = 500;

        // Simple MagicWarpで、画面が完全に暗転するまでのフェードイン時間
        internal const int SimpleMagicFadeDuration = 300;

        // Magic系Warpで、Warp後に暗転が完全に解除されるまでのフェードアウト時間
        internal const int MagicFadeOutDuration = 100;

        //----------------------------------------
        // Magic Effect Duration
        //----------------------------------------

        // Magicエフェクトを表示してから暗転を開始するまでの時間
        internal const int MagicEffectDuration = 1000;

        // Simple Magicエフェクトを表示してから暗転を開始するまでの時間
        internal const int SimpleMagicEffectDuration = 500;

        //----------------------------------------
        // Magic Warp演出
        //----------------------------------------

        /// <summary>
        /// Magic Warp演出を開始します。
        /// </summary>
        internal static void StartMagicWarpEffect(
            bool simple)
        {
            GameLocation? currentLocation =
                Game1.currentLocation;

            Farmer player =
                Game1.player;

            if (currentLocation == null)
                return;

            //----------------------------------------
            // Full Magic Warp専用エフェクト
            //----------------------------------------

            if (!simple)
            {
                // プレイヤー周囲に魔法エフェクトを表示
                for (int j = 0; j < 12; j++)
                {
                    currentLocation.TemporarySprites.Add(
                        new TemporaryAnimatedSprite(
                            354,
                            Game1.random.Next(25, 75),
                            6,
                            1,
                            new Vector2(
                                Game1.random.Next(
                                    (int)player.Position.X - 256,
                                    (int)player.Position.X + 192),
                                Game1.random.Next(
                                    (int)player.Position.Y - 256,
                                    (int)player.Position.Y + 192)
                            ),
                            flicker: false,
                            Game1.random.NextBool()
                        ));
                }

                //----------------------------------------
                // 横方向の光エフェクト
                //----------------------------------------

                int j2 = 0;

                Point playerTile =
                    player.TilePoint;

                for (int x = playerTile.X + 8;
                     x >= playerTile.X - 8;
                     x--)
                {
                    currentLocation.TemporarySprites.Add(
                        new TemporaryAnimatedSprite(
                            6,
                            new Vector2(
                                x,
                                playerTile.Y) * 64f,
                            Color.White,
                            8,
                            flipped: false,
                            50f)
                        {
                            layerDepth = 1f,

                            delayBeforeAnimationStart =
                                j2 * 25,

                            motion =
                                new Vector2(
                                    -0.25f,
                                    0f)
                        });

                    j2++;
                }

                //----------------------------------------
                // Full Magicではプレイヤーを非表示
                //----------------------------------------

                Game1.displayFarmer = false;
            }
            else
            {
                //----------------------------------------
                // Simple Magic
                //----------------------------------------

                player.Halt();
                player.FarmerSprite.StopAnimation();
            }

            //----------------------------------------
            // Magic共通フラッシュ
            //----------------------------------------

            Game1.flashAlpha = 1f;
        }
    }
}