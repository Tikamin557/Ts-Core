using StardewModdingAPI;

namespace Ts_Core.Debug
{
    /// <summary>
    /// デバッグログ表示用の共通処理です。
    /// </summary>
    internal static class DebugLogHelper
    {
        //----------------------------------------
        // 項目ログ
        //----------------------------------------

        /// <summary>
        /// 項目名と値を位置を揃えて表示します。
        /// </summary>
        internal static void LogField(
            IMonitor monitor,
            string name,
            object? value,
            int labelWidth = 20,
            int indent = 4)
        {
            string indentation =
                new(' ', indent);

            monitor.Log(
                $"{indentation}{name.PadRight(labelWidth)}: {value}",
                LogLevel.Info);
        }

        //----------------------------------------
        // 空行
        //----------------------------------------

        /// <summary>
        /// 空行を表示します。
        /// </summary>
        internal static void LogBlankLine(
            IMonitor monitor)
        {
            monitor.Log(
                "",
                LogLevel.Info);
        }
    }
}