using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace Ts_Core.Services.WarpRelated
{
    /// <summary>
    /// Warp中の暗転オーバーレイを管理します。
    /// </summary>
    internal static class WarpBlackoutOverlayService
    {
        //----------------------------------------
        // Fade Duration
        //----------------------------------------

        // フェード時間が個別指定されていない場合に使用するデフォルトのフェード時間
        private const int DefaultFadeDurationMs = 150;

        //----------------------------------------
        // 状態
        //----------------------------------------

        private static float Alpha;
        private static float TargetAlpha;

        private static bool IsActive;

        private static int FadeInDurationMs =
            DefaultFadeDurationMs;

        private static int FadeOutDurationMs =
            DefaultFadeDurationMs;

        private static Action? OnShown;
        private static Action? OnHidden;

        //----------------------------------------
        // 初期化
        //----------------------------------------

        /// <summary>
        /// 暗転オーバーレイを初期化します。
        /// </summary>
        internal static void Initialize(
            IModHelper helper)
        {
            helper.Events.GameLoop.UpdateTicked +=
                OnUpdateTicked;
        }

        //----------------------------------------
        // 表示
        //----------------------------------------

        /// <summary>
        /// 暗転オーバーレイをフェードイン表示します。
        /// 完全に暗転した後、指定された処理を実行します。
        /// </summary>
        internal static void Show(
            Action? onShown = null,
            int fadeInDurationMs = DefaultFadeDurationMs,
            int fadeOutDurationMs = DefaultFadeDurationMs)
        {
            IsActive = true;
            TargetAlpha = 1f;

            FadeInDurationMs =
                Math.Max(
                    1,
                    fadeInDurationMs);

            FadeOutDurationMs =
                Math.Max(
                    1,
                    fadeOutDurationMs);

            OnShown =
                onShown;
        }

        //----------------------------------------
        // 非表示
        //----------------------------------------

        /// <summary>
        /// 暗転オーバーレイをフェードアウトします。
        /// 完全に透明になった後、指定された処理を実行します。
        /// </summary>
        internal static void Hide(
            Action? onHidden = null)
        {
            TargetAlpha = 0f;

            OnHidden =
                onHidden;
        }

        //----------------------------------------
        // 更新
        //----------------------------------------

        /// <summary>
        /// 暗転オーバーレイのAlphaを更新します。
        /// </summary>
        private static void OnUpdateTicked(
            object? sender,
            UpdateTickedEventArgs e)
        {
            if (!IsActive)
                return;

            //----------------------------------------
            // 1フレームあたりの基準時間
            //----------------------------------------

            const float millisecondsPerFrame =
                1000f / 60f;

            //----------------------------------------
            // フェードイン
            //----------------------------------------

            if (Alpha < TargetAlpha)
            {
                float alphaStep =
                    millisecondsPerFrame
                    / FadeInDurationMs;

                Alpha =
                    Math.Min(
                        TargetAlpha,
                        Alpha + alphaStep);

                //----------------------------------------
                // 完全暗転
                //----------------------------------------

                if (Alpha >= 1f)
                {
                    Alpha = 1f;

                    Action? callback =
                        OnShown;

                    OnShown = null;

                    callback?.Invoke();
                }

                return;
            }

            //----------------------------------------
            // フェードアウト
            //----------------------------------------

            if (Alpha > TargetAlpha)
            {
                float alphaStep =
                    millisecondsPerFrame
                    / FadeOutDurationMs;

                Alpha =
                    Math.Max(
                        TargetAlpha,
                        Alpha - alphaStep);

                //----------------------------------------
                // 完全透明
                //----------------------------------------

                if (Alpha <= 0f)
                {
                    Alpha = 0f;
                    IsActive = false;

                    Action? callback =
                        OnHidden;

                    OnHidden = null;

                    callback?.Invoke();
                }
            }
        }

        //----------------------------------------
        // 描画
        //----------------------------------------

        /// <summary>
        /// 画面全体へ黒いオーバーレイを描画します。
        /// </summary>
        internal static void Draw()
        {
            if (!IsActive
                || Alpha <= 0f)
            {
                return;
            }

            GraphicsDevice graphicsDevice =
                Game1.graphics.GraphicsDevice;

            Viewport viewport =
                graphicsDevice.Viewport;

            SpriteBatch spriteBatch =
                Game1.spriteBatch;

            spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointClamp);

            spriteBatch.Draw(
                Game1.staminaRect,
                new Rectangle(
                    0,
                    0,
                    viewport.Width,
                    viewport.Height),
                Color.Black * Alpha);

            spriteBatch.End();
        }
    }
}