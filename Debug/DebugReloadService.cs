using StardewModdingAPI;
using Ts_Core.Services.BuildingRelated;
using Ts_Core.Services.Notification;
using Ts_Core.Services.WarpRelated;

namespace Ts_Core.Debug
{
    /// <summary>
    /// T's Coreのデータ再読み込み処理を管理します。
    /// </summary>
    internal static class DebugReloadService
    {
        //----------------------------------------
        // Reload
        //----------------------------------------

        /// <summary>
        /// T's Coreの各機能を再読み込みします。
        /// </summary>
        internal static void Reload(
            IModHelper helper,
            IMonitor monitor,
            string[] args)
        {
            string target =
                args.Length > 0
                    ? args[0].Trim().ToLowerInvariant()
                    : "all";

            switch (target)
            {
                //----------------------------------------
                // すべて再読み込み
                //----------------------------------------

                case "all":

                    ReloadWarp(
                        helper,
                        monitor);

                    ReloadNotification(
                        monitor);

                    ReloadBuildingProviders(
                        helper,
                        monitor);

                    monitor.Log(
                        "T's Core data reloaded successfully.",
                        LogLevel.Info);

                    break;

                //----------------------------------------
                // Warp Provider
                //----------------------------------------

                case "warp":

                    ReloadWarp(
                        helper,
                        monitor);

                    break;

                //----------------------------------------
                // Building Provider
                //----------------------------------------

                case "building":
                case "buildings":

                    ReloadBuildingProviders(
                        helper,
                        monitor);

                    break;

                //----------------------------------------
                // Notification Theme
                //----------------------------------------

                case "notification":
                case "notifications":

                    ReloadNotification(
                        monitor);

                    break;

                //----------------------------------------
                // Help
                //----------------------------------------

                case "help":
                case "?":

                    LogUsage(
                        monitor);

                    break;

                //----------------------------------------
                // 不明な引数
                //----------------------------------------

                default:

                    monitor.Log(
                        $"Unknown reload target: '{target}'",
                        LogLevel.Warn);

                    LogUsage(
                        monitor);

                    break;
            }
        }

        //----------------------------------------
        // Warp
        //----------------------------------------

        /// <summary>
        /// Warp Providerを再読み込みします。
        /// </summary>
        private static void ReloadWarp(
            IModHelper helper,
            IMonitor monitor)
        {
            try
            {
                WarpLoader.Reload(
                    helper,
                    monitor);
            }
            catch (Exception ex)
            {
                monitor.Log(
                    $"Failed to reload Warp Providers: {ex}",
                    LogLevel.Error);
            }
        }

        //----------------------------------------
        // Building
        //----------------------------------------

        /// <summary>
        /// Building Providerを再読み込みします。
        /// </summary>
        private static void ReloadBuildingProviders(
            IModHelper helper,
            IMonitor monitor)
        {
            try
            {
                BuildingProviderLoader.Reload(
                    helper,
                    monitor);

                if (Context.IsWorldReady)
                {
                    BuildingLightService.UpdateLights();
                }
            }
            catch (Exception ex)
            {
                monitor.Log(
                    $"Failed to reload Building Providers: {ex}",
                    LogLevel.Error);
            }
        }

        //----------------------------------------
        // Notification
        //----------------------------------------

        /// <summary>
        /// Notification Themeを再読み込みします。
        /// </summary>
        private static void ReloadNotification(
            IMonitor monitor)
        {
            try
            {
                monitor.Log(
                    "Reloading Notification Themes...",
                    LogLevel.Info);

                NotificationThemeManager.ReloadThemes();

                int builtinThemeCount =
                    NotificationThemeManager
                        .GetBuiltinThemeNames()
                        .Count();

                int contentPackThemeCount =
                    NotificationThemeManager
                        .GetContentPackThemeNames()
                        .Count();

                int themeCount =
                    builtinThemeCount
                    + contentPackThemeCount;

                monitor.Log(
                    $"Notification Themes reloaded successfully. " +
                    $"Registered Themes: {themeCount}",
                    LogLevel.Info);
            }
            catch (Exception ex)
            {
                monitor.Log(
                    $"Failed to reload Notification Themes: {ex}",
                    LogLevel.Error);
            }
        }

        //----------------------------------------
        // Usage
        //----------------------------------------

        /// <summary>
        /// tscore_reloadの使用方法を表示します。
        /// </summary>
        private static void LogUsage(
            IMonitor monitor)
        {
            monitor.Log(
                "===== T's Core Reload =====",
                LogLevel.Info);

            DebugLogHelper.LogBlankLine(
                monitor);

            DebugLogHelper.LogField(
                monitor,
                "tscore_reload",
                "Reload all supported data.",
                labelWidth: 30);

            DebugLogHelper.LogField(
                monitor,
                "tscore_reload all",
                "Reload all supported data.",
                labelWidth: 30);

            DebugLogHelper.LogField(
                monitor,
                "tscore_reload warp",
                "Reload Warp Providers.",
                labelWidth: 30);

            DebugLogHelper.LogField(
                monitor,
                "tscore_reload building",
                "Reload Building Providers.",
                labelWidth: 30);

            DebugLogHelper.LogField(
                monitor,
                "tscore_reload notification",
                "Reload Notification Themes.",
                labelWidth: 30);
        }
    }
}