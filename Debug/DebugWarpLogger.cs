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

            monitor.Log(
                $"Registered Providers: {providers.Count}",
                LogLevel.Info);

            if (providers.Count == 0)
            {
                DebugLogHelper.LogBlankLine(
                    monitor);

                monitor.Log(
                    "No warp providers are registered.",
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

                    break;

                //----------------------------------------
                // Building
                //----------------------------------------

                case "Building":

                    string fallback =
                        string.IsNullOrWhiteSpace(provider.Fallback)
                            ? "FarmHouseFront"
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
                        fallback);

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