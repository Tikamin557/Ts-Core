using StardewModdingAPI;
using Ts_Core.Services.Notification;

namespace Ts_Core.Debug
{
    /// <summary>
    /// Notification関連のデバッグ情報をログへ出力します。
    /// </summary>
    internal static class DebugNotificationLogger
    {
        //----------------------------------------
        // Notification Theme
        //----------------------------------------

        /// <summary>
        /// 登録済みNotification Theme一覧を表示します。
        /// </summary>
        internal static void LogThemes(
            IMonitor monitor)
        {
            List<string> builtinThemes =
                NotificationThemeManager
                    .GetBuiltinThemeNames()
                    .ToList();

            List<string> contentPackThemes =
                NotificationThemeManager
                    .GetContentPackThemeNames()
                    .ToList();

            monitor.Log(
                "===== Notification Themes =====",
                LogLevel.Info);

            monitor.Log(
                $"Registered Themes: {builtinThemes.Count + contentPackThemes.Count}",
                LogLevel.Info);

            //----------------------------------------
            // T's Core
            //----------------------------------------

            DebugLogHelper.LogBlankLine(
                monitor);

            monitor.Log(
                $"----- T's Core ({builtinThemes.Count}) -----",
                LogLevel.Info);

            DebugLogHelper.LogBlankLine(
                monitor);

            foreach (string name in builtinThemes)
            {
                monitor.Log(
                    $"    {name}",
                    LogLevel.Info);
            }

            //----------------------------------------
            // Content Packs
            //----------------------------------------

            DebugLogHelper.LogBlankLine(
                monitor);

            monitor.Log(
                $"----- Content Packs ({contentPackThemes.Count}) -----",
                LogLevel.Info);

            DebugLogHelper.LogBlankLine(
                monitor);

            if (contentPackThemes.Count == 0)
            {
                monitor.Log(
                    "    (none)",
                    LogLevel.Info);

                return;
            }

            for (int i = 0;
                 i < contentPackThemes.Count;
                 i++)
            {
                string name =
                    contentPackThemes[i];

                int separator =
                    name.LastIndexOf('.');

                string shortName =
                    separator >= 0
                        ? name[(separator + 1)..]
                        : name;

                monitor.Log(
                    $"    {shortName}",
                    LogLevel.Info);

                DebugLogHelper.LogField(
                    monitor,
                    "Full Name",
                    name,
                    indent: 8);

                if (i < contentPackThemes.Count - 1)
                {
                    DebugLogHelper.LogBlankLine(
                        monitor);
                }
            }
        }
    }
}