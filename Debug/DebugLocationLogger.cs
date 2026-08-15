using StardewModdingAPI;
using StardewValley;
using Ts_Core.Services.Location;

namespace Ts_Core.Debug
{
    /// <summary>
    /// Location関連のデバッグ情報をログへ出力します。
    /// </summary>
    internal static class DebugLocationLogger
    {
        //----------------------------------------
        // Location
        //----------------------------------------

        /// <summary>
        /// Location関連情報を表示します。
        /// </summary>
        internal static void Log(
            IMonitor monitor)
        {
            monitor.Log(
                "===== Location =====",
                LogLevel.Info);

            DebugLogHelper.LogBlankLine(
                monitor);

            DebugLogHelper.LogField(
                monitor,
                "Current Location",
                Game1.currentLocation?.NameOrUniqueName
                    ?? "(none)");

            DebugLogHelper.LogField(
                monitor,
                "Previous Location",
                string.IsNullOrWhiteSpace(LocationTracker.PreviousLocation)
                    ? "(none)"
                    : LocationTracker.PreviousLocation);

            DebugLogHelper.LogField(
                monitor,
                "Location Elapsed",
                LocationTracker.LocationElapsed);

            DebugLogHelper.LogField(
                monitor,
                "Visit Count",
                LocationTracker.VisitCount());

            DebugLogHelper.LogField(
                monitor,
                "Session Visit Count",
                LocationTracker.SessionVisitCount());

            DebugLogHelper.LogField(
                monitor,
                "Entered Today",
                LocationTracker.EnteredToday());

            DebugLogHelper.LogField(
                monitor,
                "Is Outdoors",
                LocationTracker.IsOutdoors());

            DebugLogHelper.LogField(
                monitor,
                "Is Indoors",
                LocationTracker.IsIndoors());
        }
    }
}