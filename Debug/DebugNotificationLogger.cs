using StardewModdingAPI;
using Ts_Core.Services.Notification;

namespace Ts_Core.Debug
{
    /// <summary>
    /// Notification ThemeのDebug表示を行います。
    /// </summary>
    internal static class DebugNotificationLogger
    {
        /// <summary>
        /// 登録されているNotification Themeを表示します。
        /// </summary>
        public static void LogNotificationThemes(
            IMonitor monitor)
        {
            monitor.Log(
                "===== Notification Themes =====",
                LogLevel.Info);

            //----------------------------------------
            // TsCore Built-in Themes
            //----------------------------------------

            LogThemeGroup(
                monitor,
                "TsCore Built-in Themes",
                NotificationThemeManager
                    .GetDefaultThemeNames()
                    .ToList());

            //----------------------------------------
            // External Data Asset Themes
            //----------------------------------------

            LogThemeGroup(
                monitor,
                "External Data Asset Themes",
                NotificationThemeManager
                    .GetExternalThemeNames()
                    .ToList());
        }

        /// <summary>
        /// Themeグループを表示します。
        /// </summary>
        private static void LogThemeGroup(
            IMonitor monitor,
            string groupName,
            IReadOnlyList<string> themeNames)
        {
            monitor.Log(
                "",
                LogLevel.Info);

            monitor.Log(
                $"----- {groupName} -----",
                LogLevel.Info);

            monitor.Log(
                "",
                LogLevel.Info);

            //----------------------------------------
            // Themeなし
            //----------------------------------------

            if (themeNames.Count == 0)
            {
                monitor.Log(
                    "(none)",
                    LogLevel.Info);

                return;
            }

            //----------------------------------------
            // Theme表示
            //----------------------------------------

            foreach (
                string themeName in themeNames
                    .OrderBy(p => p))
            {
                if (!NotificationThemeManager.TryGetTheme(
                    themeName,
                    out NotificationTheme? theme)
                    || theme == null)
                {
                    continue;
                }

                LogTheme(
                    monitor,
                    themeName,
                    theme);
            }
        }

        /// <summary>
        /// Theme情報を表示します。
        /// </summary>
        private static void LogTheme(
            IMonitor monitor,
            string themeName,
            NotificationTheme theme)
        {
            monitor.Log(
                themeName,
                LogLevel.Info);

            monitor.Log(
                $"    Base                : {FormatValue(theme.Base)}",
                LogLevel.Info);

            monitor.Log(
                $"    BackgroundColor     : {FormatValue(theme.BackgroundColor)}",
                LogLevel.Info);

            monitor.Log(
                $"    BorderColor         : {FormatValue(theme.BorderColor)}",
                LogLevel.Info);

            monitor.Log(
                $"    BorderStyle         : {FormatValue(theme.BorderStyle)}",
                LogLevel.Info);

            monitor.Log(
                $"    BorderThickness     : {FormatValue(theme.BorderThickness)}",
                LogLevel.Info);

            monitor.Log(
                $"    TextColor           : {FormatValue(theme.TextColor)}",
                LogLevel.Info);

            monitor.Log(
                $"    ShadowColor         : {FormatValue(theme.ShadowColor)}",
                LogLevel.Info);

            monitor.Log(
                $"    DrawShadow          : {FormatValue(theme.DrawShadow)}",
                LogLevel.Info);

            monitor.Log(
                $"    ShadowOffset        : {FormatValue(theme.ShadowOffset)}",
                LogLevel.Info);

            monitor.Log(
                $"    TextAnchor          : {FormatValue(theme.TextAnchor)}",
                LogLevel.Info);

            monitor.Log(
                $"    TextScale           : {FormatValue(theme.TextScale)}",
                LogLevel.Info);

            monitor.Log(
                $"    MinHeight           : {FormatValue(theme.MinHeight)}",
                LogLevel.Info);

            monitor.Log(
                $"    MinWidth            : {FormatValue(theme.MinWidth)}",
                LogLevel.Info);

            monitor.Log(
                $"    PaddingX            : {FormatValue(theme.PaddingX)}",
                LogLevel.Info);

            monitor.Log(
                $"    PaddingY            : {FormatValue(theme.PaddingY)}",
                LogLevel.Info);

            monitor.Log(
                $"    BorderPadding       : {FormatValue(theme.BorderPadding)}",
                LogLevel.Info);

            monitor.Log(
                $"    Anchor              : {FormatValue(theme.Anchor)}",
                LogLevel.Info);

            monitor.Log(
                $"    OffsetX             : {FormatValue(theme.OffsetX)}",
                LogLevel.Info);

            monitor.Log(
                $"    OffsetY             : {FormatValue(theme.OffsetY)}",
                LogLevel.Info);

            monitor.Log(
                $"    DismissOnLocationChange : {FormatValue(theme.DismissOnLocationChange)}",
                LogLevel.Info);

            monitor.Log(
                $"    DismissOnEnterLocations : {FormatLocations(theme.DismissOnEnterLocations)}",
                LogLevel.Info);

            monitor.Log(
                "",
                LogLevel.Info);
        }

        /// <summary>
        /// 値をDebug表示用文字列へ変換します。
        /// </summary>
        private static string FormatValue(
            object? value)
        {
            return value?.ToString()
                ?? "(null)";
        }

        /// <summary>
        /// Location一覧をDebug表示用文字列へ変換します。
        /// </summary>
        private static string FormatLocations(
            IReadOnlyList<string>? locations)
        {
            if (locations == null
                || locations.Count == 0)
            {
                return "(none)";
            }

            return string.Join(
                ", ",
                locations);
        }
    }
}