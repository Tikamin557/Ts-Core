using StardewModdingAPI;
using System.Reflection;
using System.Text;

namespace Ts_Core.Services.ShortcutPanelRelated
{
    /// <summary>
    /// 現在のSMAPIプロセスが書き込んでいるログファイルを取得・読み込みします。
    /// Android版SMAPIの内部型にはコンパイル時参照せず、Reflectionで取得します。
    /// </summary>
    internal static class ShortcutPanelSmapiLogService
    {
        private static string? cachedLogPath;

        internal static bool TryGetCurrentLogPath(out string path, out string error)
        {
            path = string.Empty;
            error = string.Empty;

            if (!string.IsNullOrWhiteSpace(cachedLogPath) && File.Exists(cachedLogPath))
            {
                path = cachedLogPath;
                return true;
            }

            try
            {
                Assembly smapiAssembly = typeof(IModHelper).Assembly;
                Type? sCoreType = smapiAssembly.GetType("StardewModdingAPI.Framework.SCore");
                if (sCoreType == null)
                {
                    error = "SMAPI SCore type was not found.";
                    return false;
                }

                BindingFlags staticFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
                object? sCore = sCoreType.GetProperty("Instance", staticFlags)?.GetValue(null)
                    ?? sCoreType.GetField("Instance", staticFlags)?.GetValue(null);

                if (sCore == null)
                {
                    error = "SMAPI SCore.Instance was not found.";
                    return false;
                }

                object? logManager = GetMemberValue(sCore, "LogManager");
                if (logManager == null)
                {
                    error = "SMAPI LogManager was not found.";
                    return false;
                }

                object? logFile = GetMemberValue(logManager, "LogFile");
                if (logFile == null)
                {
                    error = "SMAPI LogFileManager was not found.";
                    return false;
                }

                object? pathValue = GetMemberValue(logFile, "Path");
                if (pathValue is not string logPath || string.IsNullOrWhiteSpace(logPath))
                {
                    error = "SMAPI log path was not found.";
                    return false;
                }

                cachedLogPath = logPath;
                path = logPath;
                return true;
            }
            catch (Exception ex)
            {
                error = ex.GetType().Name + ": " + ex.Message;
                return false;
            }
        }

        /// <summary>
        /// 現在SMAPIが書き込んでいるログファイルを共有読み取りで開き、全文を取得します。
        /// </summary>
        internal static bool TryReadAll(out string text, out string error)
        {
            text = string.Empty;

            if (!TryGetCurrentLogPath(out string path, out error))
                return false;

            try
            {
                using FileStream stream = new FileStream(
                    path,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite | FileShare.Delete);

                using StreamReader reader = new StreamReader(
                    stream,
                    Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: true);

                text = reader.ReadToEnd();
                return true;
            }
            catch (Exception ex)
            {
                error = ex.GetType().Name + ": " + ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Reflectionを使い、指定オブジェクトのフィールドまたはプロパティから値を取得します。
        /// </summary>
        private static object? GetMemberValue(object instance, string name)
        {
            Type type = instance.GetType();
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            return type.GetProperty(name, flags)?.GetValue(instance)
                ?? type.GetField(name, flags)?.GetValue(instance);
        }
    }
}
