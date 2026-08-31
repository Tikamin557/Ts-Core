using StardewModdingAPI;
using Ts_Core.Services.WarpRelated;

namespace Ts_Core.Debug
{
    /// <summary>
    /// Warp関連のデバッグ情報をログへ出力します。
    /// </summary>
    public static class DebugWarpLogger
    {
        //----------------------------------------
        // Warp Provider
        //----------------------------------------

        /// <summary>
        /// 現在登録されているWarp Providerを表示します。
        /// </summary>
        public static void LogWarpProviders(
            IMonitor monitor)
        {
            IReadOnlyList<RegisteredWarpProviderInfo> providers =
                WarpService.GetRegisteredProviders();

            monitor.Log(
                "===== Warp Providers =====",
                LogLevel.Info);

            DebugLogHelper.LogBlankLine(
                monitor);

            //----------------------------------------
            // TsCore組み込みProvider
            //----------------------------------------

            LogBuiltInProviders(
                monitor);

            DebugLogHelper.LogBlankLine(
                monitor);

            monitor.Log(
                $"Registered JSON Providers: {providers.Count}",
                LogLevel.Info);

            if (providers.Count == 0)
            {
                DebugLogHelper.LogBlankLine(
                    monitor);

                monitor.Log(
                    "No JSON warp providers are registered.",
                    LogLevel.Info);

                return;
            }

            //----------------------------------------
            // 登録元ごとにグループ化
            //----------------------------------------

            List<IGrouping<string, RegisteredWarpProviderInfo>> groupList =
                providers
                    .GroupBy(provider => provider.Owner)
                    .OrderBy(group =>
                        string.Equals(
                            group.Key,
                            "T's Core",
                            StringComparison.OrdinalIgnoreCase)
                            ? 0
                            : 1)
                    .ThenBy(group => group.Key)
                    .ToList();

            DebugLogHelper.LogBlankLine(
                monitor);

            for (int groupIndex = 0;
                 groupIndex < groupList.Count;
                 groupIndex++)
            {
                IGrouping<string, RegisteredWarpProviderInfo> group =
                    groupList[groupIndex];

                monitor.Log(
                    $"----- {group.Key} -----",
                    LogLevel.Info);

                DebugLogHelper.LogBlankLine(
                    monitor);

                List<RegisteredWarpProviderInfo> providerList =
                    group
                        .OrderBy(provider => provider.Id)
                        .ToList();

                for (int providerIndex = 0;
                     providerIndex < providerList.Count;
                     providerIndex++)
                {
                    LogWarpProvider(
                        monitor,
                        providerList[providerIndex]);

                    // 同じグループ内のProvider間だけ空行を入れる
                    if (providerIndex < providerList.Count - 1)
                    {
                        DebugLogHelper.LogBlankLine(
                            monitor);
                    }
                }

                // グループ間だけ空行を入れる
                if (groupIndex < groupList.Count - 1)
                {
                    DebugLogHelper.LogBlankLine(
                        monitor);
                }
            }
        }

        //----------------------------------------
        // TsCore組み込みProvider
        //----------------------------------------

        /// <summary>
        /// TsCore組み込みWarp Providerを表示します。
        /// </summary>
        private static void LogBuiltInProviders(
            IMonitor monitor)
        {
            monitor.Log(
                "----- Built-in Providers -----",
                LogLevel.Info);

            DebugLogHelper.LogBlankLine(
                monitor);

            //----------------------------------------
            // PlayerHome
            //----------------------------------------

            monitor.Log(
                "PlayerHome",
                LogLevel.Info);

            DebugLogHelper.LogField(
                monitor,
                "Type",
                "Built-in");

            DebugLogHelper.LogField(
                monitor,
                "Destination",
                "Player's own home");

            DebugLogHelper.LogBlankLine(
                monitor);

            //----------------------------------------
            // PreviousHome
            //----------------------------------------

            monitor.Log(
                "PreviousHome",
                LogLevel.Info);

            DebugLogHelper.LogField(
                monitor,
                "Type",
                "Built-in");

            DebugLogHelper.LogField(
                monitor,
                "Destination",
                "Previously exited home");

            DebugLogHelper.LogBlankLine(
                monitor);

            //----------------------------------------
            // CurrentHome
            //----------------------------------------

            monitor.Log(
                "CurrentHome",
                LogLevel.Info);

            DebugLogHelper.LogField(
                monitor,
                "Type",
                "Built-in");

            DebugLogHelper.LogField(
                monitor,
                "Destination",
                "Current FarmHouse/Cabin");
        }

        //----------------------------------------
        // Warp Provider 1件
        //----------------------------------------

        /// <summary>
        /// Warp Provider一件分の情報を表示します。
        /// </summary>
        private static void LogWarpProvider(
            IMonitor monitor,
            RegisteredWarpProviderInfo provider)
        {
            monitor.Log(
                provider.Id,
                LogLevel.Info);

            switch (provider.Type)
            {
                //----------------------------------------
                // Warp
                //----------------------------------------

                case "Warp":

                    string warpFallback =
                        string.IsNullOrWhiteSpace(provider.Fallback)
                            ? "(none)"
                            : provider.Fallback;

                    DebugLogHelper.LogField(
                        monitor,
                        "Type",
                        "Warp");

                    DebugLogHelper.LogField(
                        monitor,
                        "Source",
                        provider.SourceLocation);

                    DebugLogHelper.LogField(
                        monitor,
                        "Target",
                        provider.TargetLocation);

                    DebugLogHelper.LogField(
                        monitor,
                        "Fallback",
                        warpFallback);

                    break;

                //----------------------------------------
                // MapEntry
                //----------------------------------------

                case "MapEntry":

                    string mapEntryFallback =
                        string.IsNullOrWhiteSpace(provider.Fallback)
                            ? "(none)"
                            : provider.Fallback;

                    DebugLogHelper.LogField(
                        monitor,
                        "Type",
                        "MapEntry");

                    DebugLogHelper.LogField(
                        monitor,
                        "Map",
                        provider.MapLocation);

                    DebugLogHelper.LogField(
                        monitor,
                        "Target",
                        provider.TargetLocation);

                    DebugLogHelper.LogField(
                        monitor,
                        "Offset",
                        $"({provider.OffsetX}, {provider.OffsetY})");

                    DebugLogHelper.LogField(
                        monitor,
                        "Fallback",
                        mapEntryFallback);

                    break;

                //----------------------------------------
                // Building
                //----------------------------------------

                case "Building":

                    string buildingFallback =
                        string.IsNullOrWhiteSpace(provider.Fallback)
                            ? "(none)"
                            : provider.Fallback;

                    DebugLogHelper.LogField(
                        monitor,
                        "Type",
                        "Building");

                    DebugLogHelper.LogField(
                        monitor,
                        "Building",
                        provider.BuildingType);

                    DebugLogHelper.LogField(
                        monitor,
                        "Offset",
                        $"({provider.OffsetX}, {provider.OffsetY})");

                    DebugLogHelper.LogField(
                        monitor,
                        "Fallback",
                        buildingFallback);

                    break;

                //----------------------------------------
                // Unknown
                //----------------------------------------

                default:

                    DebugLogHelper.LogField(
                        monitor,
                        "Type",
                        provider.Type);

                    break;
            }
        }
    }
}