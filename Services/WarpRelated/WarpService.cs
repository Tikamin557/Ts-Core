using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using Ts_Core.Models;

namespace Ts_Core.Services.WarpRelated
{
    /// <summary>
    /// Warp時に使用する演出の種類です。
    /// </summary>
    public enum WarpEffectMode
    {
        /// <summary>
        /// 通常Warp。
        /// </summary>
        None,

        /// <summary>
        /// 通常のMagic Warp演出。
        /// </summary>
        Magic,

        /// <summary>
        /// 簡易Magic Warp演出。
        /// </summary>
        MagicSimple
    }

    /// <summary>
    /// Warp Providerや座標を使用したワープ処理を実行するサービスです。
    /// </summary>
    public static class WarpService
    {
        //----------------------------------------
        // Blackout Duration
        //----------------------------------------

        // 通常Warpで、完全に暗転してからWarpを実行するまでの時間
        private const int NormalBlackoutDuration = 100;

        // MagicWarpで、完全に暗転してからWarpを実行するまでの時間
        private const int MagicBlackoutDuration = 100;

        // Simple MagicWarpで、完全に暗転してからWarpを実行するまでの時間
        private const int SimpleMagicBlackoutDuration = 100;

        //----------------------------------------
        // Monitor
        //----------------------------------------

        private static IMonitor? Monitor;

        //----------------------------------------
        // 初期化
        //----------------------------------------

        public static void Initialize(IMonitor monitor)
        {
            Monitor = monitor;
        }

        //----------------------------------------
        // 登録済みProvider情報
        //----------------------------------------

        /// <summary>
        /// 現在登録されているWarp Provider情報を取得します。
        /// </summary>
        internal static IReadOnlyList<RegisteredWarpProviderInfo>
            GetRegisteredProviders()
        {
            return WarpProviderService
                .GetRegisteredProviders();
        }

        //----------------------------------------
        // Provider再読み込み
        //----------------------------------------

        /// <summary>
        /// 登録済みのWarp Providerをすべて削除します。
        /// 再読み込み前に使用します。
        /// </summary>
        internal static void ClearProviders()
        {
            WarpProviderService.ClearProviders();
        }

        //----------------------------------------
        // Provider登録
        //----------------------------------------

        /// <summary>
        /// Warp Providerを登録します。
        /// </summary>
        public static void RegisterProvider(
            WarpProviderModel model,
            string owner,
            string sourceFile,
            IMonitor monitor)
        {
            WarpProviderService.RegisterProvider(
                model,
                owner,
                sourceFile,
                monitor);
        }

        //----------------------------------------
        // Warp実行
        //----------------------------------------

        /// <summary>
        /// Warp ProviderからWarp先を解決します。
        /// </summary>
        internal static bool TryResolveProvider(
            string key,
            GameLocation? sourceLocation,
            out (string Location, Point Point) destination)
        {
            destination = default;

            //----------------------------------------
            // Provider確認
            //----------------------------------------

            if (!WarpProviderService.ContainsProvider(
                    key))
            {
                Monitor?.Log(
                    $"Warp Provider '{key}' was not found.",
                    LogLevel.Warn);

                return false;
            }

            try
            {
                destination =
                    WarpProviderService.Resolve(
                        key,
                        sourceLocation);

                return true;
            }
            catch (InvalidOperationException ex)
            {
                Monitor?.Log(
                    $"Warp Provider '{key}' could not be resolved. {ex.Message}",
                    LogLevel.Warn);

                return false;
            }
        }

        /// <summary>
        /// Warp Providerを使用してWarpします。
        /// </summary>
        public static bool Warp(
            string key,
            WarpEffectMode effectMode,
            int? facingDirection = null,
            string? audioCue = null,
            int audioRepeatCount = 1,
            int audioIntervalMs = 100,
            int? blackoutDurationMs = null,
            int audioStartDelayMs = 0,
            GameLocation? sourceLocation = null)
        {
            //----------------------------------------
            // Warp先解決
            //----------------------------------------

            if (!TryResolveProvider(
                    key,
                    sourceLocation,
                    out (string Location, Point Point) destination))
            {
                return false;
            }

            //----------------------------------------
            // Warp実行
            //----------------------------------------

            Warp(
                destination.Location,
                destination.Point,
                effectMode,
                facingDirection,
                audioCue,
                audioRepeatCount,
                audioIntervalMs,
                blackoutDurationMs,
                audioStartDelayMs);

            return true;
        }

        /// <summary>
        /// 指定座標へWarpします。
        /// </summary>
        public static void Warp(
            string location,
            Point point,
            WarpEffectMode effectMode,
            int? facingDirection = null,
            string? audioCue = null,
            int audioRepeatCount = 1,
            int audioIntervalMs = 100,
            int? blackoutDurationMs = null,
            int audioStartDelayMs = 0)
        {
            WarpInternal(
                location,
                point,
                effectMode,
                facingDirection,
                audioCue,
                audioRepeatCount,
                audioIntervalMs,
                blackoutDurationMs,
                audioStartDelayMs);
        }

        //----------------------------------------
        // 互換用 overload
        //----------------------------------------

        /// <summary>
        /// 従来のbool指定によるWarpです。
        /// </summary>
        public static bool Warp(
            string key,
            bool magic,
            int? facingDirection = null)
        {
            return Warp(
                key,
                magic
                    ? WarpEffectMode.Magic
                    : WarpEffectMode.None,
                facingDirection);
        }

        /// <summary>
        /// 従来のbool指定によるWarpです。
        /// </summary>
        public static void Warp(
            string location,
            Point point,
            bool magic,
            int? facingDirection = null)
        {
            Warp(
                location,
                point,
                magic
                    ? WarpEffectMode.Magic
                    : WarpEffectMode.None,
                facingDirection);
        }

        //----------------------------------------
        // Map Warp
        //----------------------------------------

        public static bool WarpToMap(
            string locationName,
            WarpEffectMode effectMode,
            int? facingDirection = null,
            string? audioCue = null,
            int audioRepeatCount = 1,
            int audioIntervalMs = 100,
            int? blackoutDurationMs = null,
            int audioStartDelayMs = 0)
        {
            GameLocation? location =
                Game1.getLocationFromName(locationName);

            if (location == null)
                return false;

            int x = 0;
            int y = 0;

            Utility.getDefaultWarpLocation(
                locationName,
                ref x,
                ref y);

            if (x == 0 && y == 0)
            {
                Monitor?.Log(
                    $"WarpToMap failed: '{locationName}' has no default warp location.",
                    LogLevel.Warn);

                return false;
            }

            Warp(
                location.Name,
                new Point(x, y),
                effectMode,
                facingDirection,
                audioCue,
                audioRepeatCount,
                audioIntervalMs,
                blackoutDurationMs,
                audioStartDelayMs);

            return true;
        }

        //----------------------------------------
        // 互換用 overload
        //----------------------------------------

        public static bool WarpToMap(
            string locationName,
            bool magic,
            int? facingDirection = null)
        {
            return WarpToMap(
                locationName,
                magic
                    ? WarpEffectMode.Magic
                    : WarpEffectMode.None,
                facingDirection);
        }

        //----------------------------------------
        // 内部Warp
        //----------------------------------------

        private static void WarpInternal(
            string location,
            Point point,
            WarpEffectMode effectMode,
            int? facingDirection,
            string? audioCue,
            int audioRepeatCount,
            int audioIntervalMs,
            int? blackoutDurationMs,
            int audioStartDelayMs)
        {
            Farmer player =
                Game1.player;

            int direction =
                facingDirection
                ?? player.FacingDirection;

            //----------------------------------------
            // デフォルト暗転時間
            //----------------------------------------

            int defaultBlackoutDuration;

            switch (effectMode)
            {
                case WarpEffectMode.Magic:

                    defaultBlackoutDuration =
                        MagicBlackoutDuration;

                    break;

                case WarpEffectMode.MagicSimple:

                    defaultBlackoutDuration =
                        SimpleMagicBlackoutDuration;

                    break;

                default:

                    defaultBlackoutDuration =
                        NormalBlackoutDuration;

                    break;
            }

            int blackoutDuration =
                blackoutDurationMs
                ?? defaultBlackoutDuration;

            //----------------------------------------
            // Warp演出開始
            //----------------------------------------

            switch (effectMode)
            {
                case WarpEffectMode.Magic:

                    WarpEffectService.StartMagicWarpEffect(
                        simple: false);

                    break;

                case WarpEffectMode.MagicSimple:

                    WarpEffectService.StartMagicWarpEffect(
                        simple: true);

                    break;
            }

            //----------------------------------------
            // Audio Cue
            //----------------------------------------

            string? warpSound =
                effectMode == WarpEffectMode.None
                    ? audioCue
                    : string.IsNullOrWhiteSpace(audioCue)
                        ? "wand"
                        : audioCue;

            if (!string.IsNullOrWhiteSpace(
                    warpSound))
            {
                WarpAudioService.PlayAudioCue(
                    warpSound,
                    audioRepeatCount,
                    audioIntervalMs,
                    audioStartDelayMs);
            }

            //----------------------------------------
            // プレイヤー操作停止
            //----------------------------------------

            player.CanMove = false;

            //----------------------------------------
            // Warp実行処理
            //----------------------------------------

            void CompleteWarp()
            {
                //----------------------------------------
                // Location Request取得
                //----------------------------------------

                LocationRequest locationRequest =
                    Game1.getLocationRequest(
                        location);

                //----------------------------------------
                // Warp完了時
                //----------------------------------------

                locationRequest.OnWarp += () =>
                {
                    //----------------------------------------
                    // プレイヤー再表示
                    //----------------------------------------

                    Game1.displayFarmer = true;

                    //----------------------------------------
                    // 暗転解除
                    //----------------------------------------

                    WarpBlackoutOverlayService.Hide(
                        () =>
                        {
                            //----------------------------------------
                            // 操作再開
                            //----------------------------------------

                            Game1.screenGlow = false;
                            player.CanMove = true;
                        });
                };

                //----------------------------------------
                // Warp要求
                //----------------------------------------

                Game1.warpFarmer(
                    locationRequest,
                    point.X,
                    point.Y,
                    direction);

                //----------------------------------------
                // バニラWarpのフェードを即時完了させる
                //----------------------------------------

                Game1.globalFade = false;
                Game1.fadeToBlack = true;
                Game1.fadeIn = true;
                Game1.nonWarpFade = false;
                Game1.fadeToBlackAlpha = 1.2f;
            }

            //----------------------------------------
            // 暗転開始処理
            //----------------------------------------

            void StartBlackout()
            {
                Action onShown =
                    () =>
                    {
                        //----------------------------------------
                        // 完全暗転
                        //----------------------------------------

                        if (blackoutDuration <= 0)
                        {
                            CompleteWarp();

                            return;
                        }

                        //----------------------------------------
                        // 指定時間だけ完全暗転を維持
                        //----------------------------------------

                        DelayedAction.functionAfterDelay(
                            CompleteWarp,
                            blackoutDuration);
                    };

                //----------------------------------------
                // Magic Warp
                //----------------------------------------

                if (effectMode == WarpEffectMode.Magic)
                {
                    WarpBlackoutOverlayService.Show(
                        onShown,
                        WarpEffectService.MagicFadeDuration,
                        WarpEffectService.MagicFadeOutDuration);

                    return;
                }

                //----------------------------------------
                // Simple Magic Warp
                //----------------------------------------

                if (effectMode == WarpEffectMode.MagicSimple)
                {
                    WarpBlackoutOverlayService.Show(
                        onShown,
                        WarpEffectService.SimpleMagicFadeDuration,
                        WarpEffectService.MagicFadeOutDuration);

                    return;
                }

                //----------------------------------------
                // 通常Warp
                //----------------------------------------

                WarpBlackoutOverlayService.Show(
                    onShown);
            }

            //----------------------------------------
            // Magic演出時間
            //----------------------------------------

            int effectDuration;

            switch (effectMode)
            {
                case WarpEffectMode.Magic:

                    effectDuration =
                        WarpEffectService.MagicEffectDuration;

                    break;

                case WarpEffectMode.MagicSimple:

                    effectDuration =
                        WarpEffectService.SimpleMagicEffectDuration;

                    break;

                default:

                    effectDuration = 0;

                    break;
            }

            //----------------------------------------
            // 暗転開始
            //----------------------------------------

            if (effectDuration <= 0)
            {
                StartBlackout();

                return;
            }

            DelayedAction.functionAfterDelay(
                StartBlackout,
                effectDuration);
        }
    }
}