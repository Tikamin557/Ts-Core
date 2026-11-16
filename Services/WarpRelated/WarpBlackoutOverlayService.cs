using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
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

        /// <summary>
        /// 分割画面ごとに保持する暗転状態です。
        /// 一方の画面の完了コールバックが他方の画面で実行されることを防ぎます。
        /// </summary>
        private sealed class OverlayState
        {
            public float Alpha;
            public float TargetAlpha;
            public bool IsActive;
            public int FadeInDurationMs = DefaultFadeDurationMs;
            public int FadeOutDurationMs = DefaultFadeDurationMs;
            public Action? OnShown;
            public Action? OnHidden;
        }

        private static readonly PerScreen<OverlayState> States =
            new(() => new OverlayState());

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
            OverlayState state = States.Value;

            state.IsActive = true;
            state.TargetAlpha = 1f;

            state.FadeInDurationMs =
                Math.Max(
                    1,
                    fadeInDurationMs);

            state.FadeOutDurationMs =
                Math.Max(
                    1,
                    fadeOutDurationMs);

            state.OnShown =
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
            OverlayState state = States.Value;

            state.TargetAlpha = 0f;

            state.OnHidden =
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
            OverlayState state = States.Value;

            if (!state.IsActive)
                return;

            //----------------------------------------
            // 1フレームあたりの基準時間
            //----------------------------------------

            const float millisecondsPerFrame =
                1000f / 60f;

            //----------------------------------------
            // フェードイン
            //----------------------------------------

            if (state.Alpha < state.TargetAlpha)
            {
                float alphaStep =
                    millisecondsPerFrame
                    / state.FadeInDurationMs;

                state.Alpha =
                    Math.Min(
                        state.TargetAlpha,
                        state.Alpha + alphaStep);

                //----------------------------------------
                // 完全暗転
                //----------------------------------------

                if (state.Alpha >= 1f)
                {
                    state.Alpha = 1f;

                    Action? callback =
                        state.OnShown;

                    state.OnShown = null;

                    callback?.Invoke();
                }

                return;
            }

            //----------------------------------------
            // フェードアウト
            //----------------------------------------

            if (state.Alpha > state.TargetAlpha)
            {
                float alphaStep =
                    millisecondsPerFrame
                    / state.FadeOutDurationMs;

                state.Alpha =
                    Math.Max(
                        state.TargetAlpha,
                        state.Alpha - alphaStep);

                //----------------------------------------
                // 完全透明
                //----------------------------------------

                if (state.Alpha <= 0f)
                {
                    state.Alpha = 0f;
                    state.IsActive = false;

                    Action? callback =
                        state.OnHidden;

                    state.OnHidden = null;

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
            OverlayState state = States.Value;

            if (!state.IsActive
                || state.Alpha <= 0f)
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
                Color.Black * state.Alpha);

            spriteBatch.End();
        }
    }
}