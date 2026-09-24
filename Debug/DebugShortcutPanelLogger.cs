using StardewModdingAPI;
using Ts_Core.Services.ShortcutPanelRelated;

namespace Ts_Core.Debug
{
    /// <summary>
    /// ショートカットパネルの位置計算に関する診断情報を出力します。
    /// </summary>
    internal static class DebugShortcutPanelLogger
    {
        /// <summary>
        /// 現在のショートカットパネル位置診断情報をログへ出力します。
        /// </summary>
        internal static void Log(IMonitor monitor)
        {
            monitor.Log(
                ShortcutPanelService.BuildPositionDiagnosticReport(),
                LogLevel.Info);
        }
    }
}
