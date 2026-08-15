using StardewModdingAPI;
using Ts_Core.Providers;
using Ts_Core.Readers;
using Ts_Core.Services.Relationship;

namespace Ts_Core.Debug
{
    /// <summary>
    /// Relationship関連のデバッグ情報をログへ出力します。
    /// </summary>
    internal static class DebugRelationshipLogger
    {
        //----------------------------------------
        // Relationship
        //----------------------------------------

        /// <summary>
        /// Relationship関連情報を表示します。
        /// </summary>
        internal static void Log(
            IMonitor monitor,
            PartnerService service,
            IPartnerProvider provider)
        {
            List<string> partners =
                service.GetPartners()?.ToList()
                ?? new List<string>();

            List<string> orderedPartners =
                service.GetRoomOrderedPartners()?.ToList()
                ?? new List<string>();

            monitor.Log(
                "===== Relationship =====",
                LogLevel.Info);

            DebugLogHelper.LogBlankLine(
                monitor);

            DebugLogHelper.LogField(
                monitor,
                "Provider",
                service.GetProviderName());

            DebugLogHelper.LogField(
                monitor,
                "Description",
                provider.Description);

            DebugLogHelper.LogField(
                monitor,
                "Room Mod",
                RoomOrderReader.CurrentRoomMod);

            DebugLogHelper.LogField(
                monitor,
                $"Partners ({partners.Count})",
                partners.Count > 0
                    ? string.Join(", ", partners)
                    : "(none)");

            DebugLogHelper.LogField(
                monitor,
                $"OrderedPartners ({orderedPartners.Count})",
                orderedPartners.Count > 0
                    ? string.Join(", ", orderedPartners)
                    : "(none)");

            if (orderedPartners.Count == 0)
                return;

            DebugLogHelper.LogBlankLine(
                monitor);

            monitor.Log(
                "----- OrderedPartners Index -----",
                LogLevel.Info);

            DebugLogHelper.LogBlankLine(
                monitor);

            for (int i = 0; i < orderedPartners.Count; i++)
            {
                monitor.Log(
                    $"    [{i}] {orderedPartners[i]}",
                    LogLevel.Info);
            }
        }
    }
}