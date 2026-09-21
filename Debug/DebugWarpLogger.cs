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
            // Built-in Providers
            //----------------------------------------

            LogBuiltInProviders(
                monitor);

            DebugLogHelper.LogBlankLine(
                monitor);

            //----------------------------------------
            // Standard / External Providers
            //----------------------------------------

            List<RegisteredWarpProviderInfo> standardProviders =
                providers
                    .Where(provider =>
                        WarpProviderDataService.IsDefaultProvider(
                            provider.Id))
                    .OrderBy(provider =>
                        provider.Id)
                    .ToList();

            List<RegisteredWarpProviderInfo> externalProviders =
                providers
                    .Where(provider =>
                        !WarpProviderDataService.IsDefaultProvider(
                            provider.Id))
                    .OrderBy(provider =>
                        provider.Id)
                    .ToList();

            LogProviderGroup(
                monitor,
                "TsCore Standard Providers",
                standardProviders);

            DebugLogHelper.LogBlankLine(
                monitor);

            LogProviderGroup(
                monitor,
                "External Data Asset Providers",
                externalProviders);
        }

        //----------------------------------------
        // Built-in Providers
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
        // Provider Group
        //----------------------------------------

        /// <summary>
        /// Warp Providerのグループを表示します。
        /// </summary>
        private static void LogProviderGroup(
            IMonitor monitor,
            string groupName,
            IReadOnlyList<RegisteredWarpProviderInfo> providers)
        {
            monitor.Log(
                $"----- {groupName} -----",
                LogLevel.Info);

            DebugLogHelper.LogBlankLine(
                monitor);

            if (providers.Count == 0)
            {
                monitor.Log(
                    "(none)",
                    LogLevel.Info);

                return;
            }

            for (int providerIndex = 0;
                 providerIndex < providers.Count;
                 providerIndex++)
            {
                LogWarpProvider(
                    monitor,
                    providers[providerIndex]);

                if (providerIndex
                    < providers.Count - 1)
                {
                    DebugLogHelper.LogBlankLine(
                        monitor);
                }
            }
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