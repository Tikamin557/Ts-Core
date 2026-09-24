using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Mods;
using Ts_Core.Services.ShortcutPanelRelated;

namespace Ts_Core.Services.ScreenshotRelated
{
    /// <summary>
    /// スクリーンショット撮影を管理します。
    /// </summary>
    internal static class ScreenshotService
    {
        private static IModHelper helper = null!;
        private static IMonitor monitor = null!;

        private static bool currentScreenRequested;
        private static bool waitForNextFrame;
        private static bool fullMapScreenshotInProgress;

        /// <summary>
        /// スクリーンショット機能で使用する描画イベントを登録し、サービスを初期化します。
        /// </summary>
        internal static void Initialize(
            IModHelper modHelper,
            IMonitor modMonitor)
        {
            helper = modHelper;
            monitor = modMonitor;

            helper.Events.Display.RenderedStep += OnRenderedStep;
            helper.Events.Display.Rendered += OnRendered;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
        }


        /// <summary>
        /// Stardew Valley本体のマップ全体スクリーンショットを実行します。
        /// </summary>
        internal static void TakeFullMapScreenshot()
        {
            if (!Context.IsWorldReady || Game1.game1.ScreenshotBusy)
                return;

            // マップ全体スクリーンショットでは、Stardew Valleyがマップを
            // 複数回に分けて描画します。その各描画でShortcut Panelが
            // 混入しないよう、撮影完了までパネル描画を一時停止します。
            fullMapScreenshotInProgress = true;
            ShortcutPanelService.SetScreenshotHidden(true);

            try
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff");
                string screenshotName = $"TsCore_FullMap_{timestamp}";

                string result = Game1.game1.takeMapScreenshot(
                    null,
                    screenshotName,
                    () =>
                    {
                        fullMapScreenshotInProgress = false;
                        ShortcutPanelService.SetScreenshotHidden(false);

                        monitor.Log(
                            "Saved Full Map screenshot.",
                            LogLevel.Info);
                    });

                if (!string.IsNullOrWhiteSpace(result))
                {
                    monitor.Log(
                        $"Full Map screenshot: {result}",
                        LogLevel.Trace);
                }
            }
            catch (Exception ex)
            {
                fullMapScreenshotInProgress = false;
                ShortcutPanelService.SetScreenshotHidden(false);

                monitor.Log(
                    $"Failed to save Full Map screenshot: {ex}",
                    LogLevel.Error);
            }
        }


        /// <summary>
        /// Full Map撮影終了後、ScreenshotBusyが解除されたことも確認して
        /// Shortcut Panelの一時非表示を確実に解除します。
        /// </summary>
        private static void OnUpdateTicked(
            object? sender,
            UpdateTickedEventArgs e)
        {
            if (!fullMapScreenshotInProgress)
                return;

            if (Game1.game1.ScreenshotBusy)
                return;

            fullMapScreenshotInProgress = false;
            ShortcutPanelService.SetScreenshotHidden(false);
        }

        /// <summary>
        /// 「現在の画面」のスクリーンショットを予約します。
        /// Shortcut Panelを含めない場合は、次フレームまで一時的に非表示にします。
        /// </summary>
        internal static void RequestCurrentScreenScreenshot()
        {
            if (!Context.IsWorldReady || currentScreenRequested)
                return;

            currentScreenRequested = true;
            waitForNextFrame = !ModEntry.Config.ScreenshotIncludeShortcutPanel;

            if (!ModEntry.Config.ScreenshotIncludeShortcutPanel)
            {
                ShortcutPanelService.SetScreenshotHidden(true);
            }
        }

        /// <summary>
        /// 指定した描画段階で現在画面のスクリーンショットを取得します。
        /// </summary>
        private static void OnRenderedStep(
            object? sender,
            RenderedStepEventArgs e)
        {
            if (!currentScreenRequested || waitForNextFrame)
                return;

            RenderSteps targetStep =
                ModEntry.Config.ScreenshotIncludeMouseCursor
                    ? RenderSteps.FullScene
                    : RenderSteps.Overlays;

            if (e.Step != targetStep)
                return;

            try
            {
                SaveCurrentScreen();
            }
            catch (Exception ex)
            {
                monitor.Log(
                    $"Failed to save Current Screen screenshot: {ex}",
                    LogLevel.Error);
            }
            finally
            {
                currentScreenRequested = false;
                ShortcutPanelService.SetScreenshotHidden(false);
            }
        }

        /// <summary>
        /// パネルを消したフレームが終わるまで待ちます。
        /// </summary>
        private static void OnRendered(
            object? sender,
            RenderedEventArgs e)
        {
            if (!currentScreenRequested || !waitForNextFrame)
                return;

            waitForNextFrame = false;
        }

        /// <summary>
        /// Stardew Valley本体のrenderScreenBufferと同じ倍率で
        /// Game1.screenとGame1.uiScreenを合成して保存します。
        /// </summary>
        private static void SaveCurrentScreen()
        {
            RenderTarget2D? worldScreen = Game1.game1.screen;
            RenderTarget2D? uiScreen = Game1.game1.uiScreen;

            if (worldScreen == null || worldScreen.IsDisposed || worldScreen.IsContentLost)
            {
                monitor.Log(
                    "Current Screen screenshot failed: Game1.screen is unavailable.",
                    LogLevel.Warn);
                return;
            }

            GraphicsDevice graphicsDevice = Game1.graphics.GraphicsDevice;
            PresentationParameters presentation = graphicsDevice.PresentationParameters;

            int width = presentation.BackBufferWidth;
            int height = presentation.BackBufferHeight;

            RenderTargetBinding[] oldTargets = graphicsDevice.GetRenderTargets();

            using RenderTarget2D result = new RenderTarget2D(
                graphicsDevice,
                width,
                height);

            using SpriteBatch screenshotBatch = new SpriteBatch(graphicsDevice);

            try
            {
                graphicsDevice.SetRenderTarget(result);
                graphicsDevice.Clear(Game1.bgColor);

                screenshotBatch.Begin(
                    SpriteSortMode.Deferred,
                    BlendState.Opaque,
                    SamplerState.LinearClamp,
                    DepthStencilState.Default,
                    RasterizerState.CullNone);

                screenshotBatch.Draw(
                    worldScreen,
                    Vector2.Zero,
                    worldScreen.Bounds,
                    Color.White,
                    0f,
                    Vector2.Zero,
                    Game1.options.zoomLevel,
                    SpriteEffects.None,
                    1f);

                screenshotBatch.End();

                if (ModEntry.Config.ScreenshotIncludeUi
                    && uiScreen != null
                    && !uiScreen.IsDisposed
                    && !uiScreen.IsContentLost)
                {
                    screenshotBatch.Begin(
                        SpriteSortMode.Deferred,
                        BlendState.AlphaBlend,
                        SamplerState.LinearClamp,
                        DepthStencilState.Default,
                        RasterizerState.CullNone);

                    screenshotBatch.Draw(
                        uiScreen,
                        Vector2.Zero,
                        uiScreen.Bounds,
                        Color.White,
                        0f,
                        Vector2.Zero,
                        Game1.options.uiScale,
                        SpriteEffects.None,
                        1f);

                    screenshotBatch.End();
                }

                graphicsDevice.SetRenderTarget(null);

                string folder = Game1.game1.GetScreenshotFolder(true);
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff");
                string path = Path.Combine(
                    folder,
                    $"TsCore_CurrentScreen_{timestamp}.png");

                using FileStream stream = File.Create(path);
                result.SaveAsPng(stream, width, height);

                monitor.Log(
                    $"Saved Current Screen screenshot: {path} ({width}x{height})",
                    LogLevel.Info);
            }
            finally
            {
                if (oldTargets.Length > 0)
                    graphicsDevice.SetRenderTargets(oldTargets);
                else
                    graphicsDevice.SetRenderTarget(null);
            }
        }
    }
}
