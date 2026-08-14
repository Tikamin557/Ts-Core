using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using Ts_Core.Models;
using Ts_Core.Providers;
using Ts_Core.Readers;
using Ts_Core.Services;
using Ts_Core.Services.LightRelated;
using Ts_Core.Services.Location;
using Ts_Core.Services.Notification;
using Ts_Core.Services.Relationship;
using Ts_Core.Services.WarpRelated;

namespace Ts_Core.Debug
{
    /// <summary>
    /// デバッグ用コンソールコマンドを登録するクラスです。
    /// </summary>
    public static class DebugCommandRegistry
    {
        /// <summary>
        /// デバッグ用コンソールコマンドを登録します。
        /// </summary>
        public static void Register(
            IModHelper helper,
            IMonitor monitor,
            PartnerService service,
            IPartnerProvider provider)
        {
            // 全トークン値を表示
            helper.ConsoleCommands.Add(
                "tscore_tokens",
                "Print all T's Core token values.",
                (command, args) =>
                {
                    LogRelationship(
                        monitor,
                        service,
                        provider);

                    LogBlankLine(monitor);

                    LogLocation(
                        monitor);
                });

            // Relationship関連トークンを表示
            helper.ConsoleCommands.Add(
                "tscore_tokens_relationship",
                "Print relationship tokens.",
                (command, args) =>
                    LogRelationship(
                        monitor,
                        service,
                        provider));

            // Location関連トークンを表示
            helper.ConsoleCommands.Add(
                "tscore_tokens_location",
                "Print location tokens.",
                (command, args) =>
                    LogLocation(
                        monitor));

            //----------------------------------------
            // Debug
            //----------------------------------------

            // T's Coreの機能を再読み込み
            helper.ConsoleCommands.Add(
                "tscore_reload",
                "Reload T's Core data. Usage: tscore_reload [all|warp|notification|light]",
                (command, args) =>
                    ReloadTsCore(
                        helper,
                        monitor,
                        args));

            // 登録されているWarp Providerを表示
            helper.ConsoleCommands.Add(
                "tscore_debug_warp",
                "Print all registered warp providers.",
                (command, args) =>
                    LogWarpProviders(
                        monitor));

            // 農場に存在する建物情報を表示
            helper.ConsoleCommands.Add(
                "tscore_debug_buildings",
                "Print all farm buildings.",
                (command, args) =>
                    LogBuildings(
                        monitor));

            // 登録されているBuilding Light Providerを表示
            helper.ConsoleCommands.Add(
                "tscore_debug_lights",
                "Print all registered Building Light providers.",
                (command, args) =>
                    LogBuildingLightProviders(
                        monitor));

            //----------------------------------------
            // Notification
            //----------------------------------------

            // 登録済み通知テーマ一覧を表示
            helper.ConsoleCommands.Add(
                "tscore_debug_notification_themes",
                "Print notification themes.",
                (command, args) =>
                    LogNotificationThemes(
                        monitor));

            // 通知表示をテスト
            helper.ConsoleCommands.Add(
                "tscore_debug_notification",
                "Show notification. Usage: <Type or ThemeName>",
                (command, args) =>
                {
                    string notificationName = "Info";

                    if (args.Length > 0)
                    {
                        notificationName = args[0];
                    }

                    if (NotificationTypeExtensions.TryParse(
                        notificationName,
                        out NotificationType type))
                    {
                        NotificationService.Show(
                            $"Notification : {notificationName}",
                            type,
                            NotificationPriority.High,
                            180);
                    }
                    else
                    {
                        NotificationRequest.Theme(
                            notificationName,
                            $"Notification : {notificationName}",
                            180)
                            .Show();
                    }
                });

            // TriggerAction経由の通知をテスト
            helper.ConsoleCommands.Add(
                "tscore_debug_notification_trigger",
                "Usage: <Type> <Priority> <Duration> <Message...>",
                (command, args) =>
                {
                    if (args.Length < 4)
                    {
                        monitor.Log(
                            "Usage: tscore_debug_notification_trigger <Type> <Priority> <Duration> <Message...>",
                            LogLevel.Info);

                        return;
                    }

                    NotificationAction.Run(
                        args,
                        default,
                        out _);
                });
        }

        //----------------------------------------
        // Reload
        //----------------------------------------

        /// <summary>
        /// T's Coreの各機能を再読み込みします。
        /// </summary>
        private static void ReloadTsCore(
            IModHelper helper,
            IMonitor monitor,
            string[] args)
        {
            string target =
                args.Length > 0
                    ? args[0].Trim().ToLowerInvariant()
                    : "all";

            switch (target)
            {
                //----------------------------------------
                // すべて再読み込み
                //----------------------------------------

                case "all":

                    ReloadWarp(
                        helper,
                        monitor);

                    ReloadNotification(
                        monitor);

                    ReloadBuildingLights(
                        helper,
                        monitor);

                    monitor.Log(
                        "T's Core data reloaded successfully.",
                        LogLevel.Info);

                    break;

                //----------------------------------------
                // Warp Provider
                //----------------------------------------

                case "warp":

                    ReloadWarp(
                        helper,
                        monitor);

                    break;

                //----------------------------------------
                // Building Light
                //----------------------------------------

                case "light":
                case "lights":

                    ReloadBuildingLights(
                        helper,
                        monitor);

                    break;

                //----------------------------------------
                // Notification Theme
                //----------------------------------------

                case "notification":
                case "notifications":

                    ReloadNotification(
                        monitor);

                    break;

                //----------------------------------------
                // Help
                //----------------------------------------

                case "help":
                case "?":

                    LogReloadUsage(
                        monitor);

                    break;

                //----------------------------------------
                // 不明な引数
                //----------------------------------------

                default:

                    monitor.Log(
                        $"Unknown reload target: '{target}'",
                        LogLevel.Warn);

                    LogReloadUsage(
                        monitor);

                    break;
            }
        }

        /// <summary>
        /// Warp Providerを再読み込みします。
        /// </summary>
        private static void ReloadWarp(
            IModHelper helper,
            IMonitor monitor)
        {
            try
            {
                WarpLoader.Reload(
                    helper,
                    monitor);
            }
            catch (Exception ex)
            {
                monitor.Log(
                    $"Failed to reload Warp Providers: {ex}",
                    LogLevel.Error);
            }
        }

        /// <summary>
        /// Building Light Providerを再読み込みします。
        /// </summary>
        private static void ReloadBuildingLights(
            IModHelper helper,
            IMonitor monitor)
        {
            try
            {
                BuildingLightLoader.Reload(
                    helper,
                    monitor);

                if (Context.IsWorldReady)
                {
                    BuildingLightService.UpdateLights();
                }
            }
            catch (Exception ex)
            {
                monitor.Log(
                    $"Failed to reload Building Light Providers: {ex}",
                    LogLevel.Error);
            }
        }

        /// <summary>
        /// Notification Themeを再読み込みします。
        /// </summary>
        private static void ReloadNotification(
            IMonitor monitor)
        {
            try
            {
                monitor.Log(
                    "Reloading Notification Themes...",
                    LogLevel.Info);

                NotificationThemeManager.ReloadThemes();

                int builtinThemeCount =
                    NotificationThemeManager
                        .GetBuiltinThemeNames()
                        .Count();

                int contentPackThemeCount =
                    NotificationThemeManager
                        .GetContentPackThemeNames()
                        .Count();

                int themeCount =
                    builtinThemeCount + contentPackThemeCount;

                monitor.Log(
                    $"Notification Themes reloaded successfully. " +
                    $"Registered Themes: {themeCount}",
                    LogLevel.Info);
            }
            catch (Exception ex)
            {
                monitor.Log(
                    $"Failed to reload Notification Themes: {ex}",
                    LogLevel.Error);
            }
        }

        /// <summary>
        /// tscore_reloadの使用方法を表示します。
        /// </summary>
        private static void LogReloadUsage(
            IMonitor monitor)
        {
            monitor.Log(
                "===== T's Core Reload =====",
                LogLevel.Info);

            LogBlankLine(monitor);

            LogField(
                monitor,
                "tscore_reload",
                "Reload all supported data.",
                labelWidth: 28);

            LogField(
                monitor,
                "tscore_reload all",
                "Reload all supported data.",
                labelWidth: 28);

            LogField(
                monitor,
                "tscore_reload warp",
                "Reload Warp Providers.",
                labelWidth: 28);

            LogField(
                monitor,
                "tscore_reload light",
                "Reload Building Light Providers.",
                labelWidth: 28);

            LogField(
                monitor,
                "tscore_reload notification",
                "Reload Notification Themes.",
                labelWidth: 28);
        }

        //----------------------------------------
        // Relationship のログ表示
        //----------------------------------------

        private static void LogRelationship(
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

            LogBlankLine(monitor);

            LogField(
                monitor,
                "Provider",
                service.GetProviderName());

            LogField(
                monitor,
                "Description",
                provider.Description);

            LogField(
                monitor,
                "Room Mod",
                RoomOrderReader.CurrentRoomMod);

            LogField(
                monitor,
                $"Partners ({partners.Count})",
                partners.Count > 0
                    ? string.Join(", ", partners)
                    : "(none)");

            LogField(
                monitor,
                $"OrderedPartners ({orderedPartners.Count})",
                orderedPartners.Count > 0
                    ? string.Join(", ", orderedPartners)
                    : "(none)");

            if (orderedPartners.Count == 0)
                return;

            LogBlankLine(monitor);

            monitor.Log(
                "----- OrderedPartners Index -----",
                LogLevel.Info);

            LogBlankLine(monitor);

            for (int i = 0; i < orderedPartners.Count; i++)
            {
                monitor.Log(
                    $"    [{i}] {orderedPartners[i]}",
                    LogLevel.Info);
            }
        }

        //----------------------------------------
        // Location のログ表示
        //----------------------------------------

        private static void LogLocation(
            IMonitor monitor)
        {
            monitor.Log(
                "===== Location =====",
                LogLevel.Info);

            LogBlankLine(monitor);

            LogField(
                monitor,
                "Current Location",
                Game1.currentLocation?.NameOrUniqueName
                    ?? "(none)");

            LogField(
                monitor,
                "Previous Location",
                string.IsNullOrWhiteSpace(LocationTracker.PreviousLocation)
                    ? "(none)"
                    : LocationTracker.PreviousLocation);

            LogField(
                monitor,
                "Location Elapsed",
                LocationTracker.LocationElapsed);

            LogField(
                monitor,
                "Visit Count",
                LocationTracker.VisitCount());

            LogField(
                monitor,
                "Session Visit Count",
                LocationTracker.SessionVisitCount());

            LogField(
                monitor,
                "Entered Today",
                LocationTracker.EnteredToday());

            LogField(
                monitor,
                "Is Outdoors",
                LocationTracker.IsOutdoors());

            LogField(
                monitor,
                "Is Indoors",
                LocationTracker.IsIndoors());
        }

        //----------------------------------------
        // Warp Provider のログ表示
        //----------------------------------------

        /// <summary>
        /// 現在登録されているWarp Providerを表示します。
        /// </summary>
        private static void LogWarpProviders(
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
                LogBlankLine(monitor);

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

            LogBlankLine(monitor);

            for (int groupIndex = 0; groupIndex < groupList.Count; groupIndex++)
            {
                IGrouping<string, RegisteredWarpProviderInfo> group =
                    groupList[groupIndex];

                monitor.Log(
                    $"----- {group.Key} -----",
                    LogLevel.Info);

                LogBlankLine(monitor);

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
                        LogBlankLine(monitor);
                }

                // グループ間だけ空行を入れる
                if (groupIndex < groupList.Count - 1)
                    LogBlankLine(monitor);
            }
        }

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

                    LogField(
                        monitor,
                        "Type",
                        "Warp");

                    LogField(
                        monitor,
                        "Source",
                        provider.SourceLocation);

                    LogField(
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

                    LogField(
                        monitor,
                        "Type",
                        "Building");

                    LogField(
                        monitor,
                        "Building",
                        provider.BuildingType);

                    LogField(
                        monitor,
                        "Offset",
                        $"({provider.OffsetX}, {provider.OffsetY})");

                    LogField(
                        monitor,
                        "Fallback",
                        fallback);

                    break;

                //----------------------------------------
                // Unknown
                //----------------------------------------

                default:

                    LogField(
                        monitor,
                        "Type",
                        provider.Type);

                    break;
            }
        }

        //----------------------------------------
        // Buildings のログ表示
        //----------------------------------------

        private static void LogBuildings(
            IMonitor monitor)
        {
            if (!Context.IsWorldReady)
            {
                monitor.Log(
                    "No save is loaded.",
                    LogLevel.Warn);

                return;
            }

            Farm farm = Game1.getFarm();

            monitor.Log(
                "===== Farm Buildings =====",
                LogLevel.Info);

            monitor.Log(
                $"Registered Buildings: {farm.buildings.Count}",
                LogLevel.Info);

            foreach (Building building in farm.buildings)
            {
                LogBlankLine(monitor);

                monitor.Log(
                    building.buildingType.Value,
                    LogLevel.Info);

                LogField(
                    monitor,
                    "Tile",
                    $"({building.tileX.Value}, {building.tileY.Value})");

                LogField(
                    monitor,
                    "Size",
                    $"{building.tilesWide.Value} x {building.tilesHigh.Value}");

                LogField(
                    monitor,
                    "Indoors",
                    building.GetIndoors()?.NameOrUniqueName
                        ?? "(none)");
            }
        }

        //----------------------------------------
        // Building Light Provider のログ表示
        //----------------------------------------

        /// <summary>
        /// 現在登録されているBuilding Light Providerを表示します。
        /// </summary>
        private static void LogBuildingLightProviders(
            IMonitor monitor)
        {
            IReadOnlyList<RegisteredBuildingLightProviderInfo> providers =
                BuildingLightService.GetRegisteredProviders();

            monitor.Log(
                "===== Building Light Providers =====",
                LogLevel.Info);

            monitor.Log(
                $"Registered Providers: {providers.Count}",
                LogLevel.Info);

            if (providers.Count == 0)
            {
                LogBlankLine(monitor);

                monitor.Log(
                    "No Building Light providers are registered.",
                    LogLevel.Info);

                return;
            }

            //----------------------------------------
            // 登録元ごとにグループ化
            //----------------------------------------

            List<IGrouping<string, RegisteredBuildingLightProviderInfo>> groupList =
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

            LogBlankLine(monitor);

            for (int groupIndex = 0;
                 groupIndex < groupList.Count;
                 groupIndex++)
            {
                IGrouping<string, RegisteredBuildingLightProviderInfo> group =
                    groupList[groupIndex];

                monitor.Log(
                    $"----- {group.Key} -----",
                    LogLevel.Info);

                LogBlankLine(monitor);

                List<RegisteredBuildingLightProviderInfo> providerList =
                    group
                        .OrderBy(provider => provider.Id)
                        .ToList();

                for (int providerIndex = 0;
                     providerIndex < providerList.Count;
                     providerIndex++)
                {
                    RegisteredBuildingLightProviderInfo provider =
                        providerList[providerIndex];

                    monitor.Log(
                        provider.Id,
                        LogLevel.Info);

                    LogField(
                        monitor,
                        "Building",
                        provider.BuildingType);

                    LogField(
                        monitor,
                        "Lights",
                        provider.LightCount);

                    LogField(
                        monitor,
                        "Enable Field",
                        string.IsNullOrWhiteSpace(provider.LightsEnabledField)
                            ? "(none)"
                            : provider.LightsEnabledField);

                    //----------------------------------------
                    // Light一覧
                    //----------------------------------------

                    if (provider.Lights.Count > 0)
                    {
                        LogBlankLine(monitor);

                        for (int lightIndex = 0;
                             lightIndex < provider.Lights.Count;
                             lightIndex++)
                        {
                            BuildingLightModel light =
                                provider.Lights[lightIndex];

                            monitor.Log(
                                $"    {light.Id}",
                                LogLevel.Info);

                            LogField(
                                monitor,
                                "Offset",
                                $"({light.OffsetX}, {light.OffsetY})",
                                indent: 8);

                            LogField(
                                monitor,
                                "Radius",
                                light.Radius,
                                indent: 8);

                            LogField(
                                monitor,
                                "Color",
                                light.Color,
                                indent: 8);

                            // 同じProvider内のLight間だけ空行
                            if (lightIndex < provider.Lights.Count - 1)
                                LogBlankLine(monitor);
                        }
                    }

                    // 同じグループ内のProvider間だけ空行
                    if (providerIndex < providerList.Count - 1)
                        LogBlankLine(monitor);
                }

                // グループ間だけ空行
                if (groupIndex < groupList.Count - 1)
                    LogBlankLine(monitor);
            }
        }

        //----------------------------------------
        // Notification Theme のログ表示
        //----------------------------------------

        private static void LogNotificationThemes(
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
            // T's Core自体のテーマ
            //----------------------------------------

            LogBlankLine(monitor);

            monitor.Log(
                $"----- T's Core ({builtinThemes.Count}) -----",
                LogLevel.Info);

            LogBlankLine(monitor);

            foreach (string name in builtinThemes)
            {
                monitor.Log(
                    $"    {name}",
                    LogLevel.Info);
            }

            //----------------------------------------
            // Content Packで追加されたテーマ
            //----------------------------------------

            LogBlankLine(monitor);

            monitor.Log(
                $"----- Content Packs ({contentPackThemes.Count}) -----",
                LogLevel.Info);

            LogBlankLine(monitor);

            if (contentPackThemes.Count == 0)
            {
                monitor.Log(
                    "    (none)",
                    LogLevel.Info);

                return;
            }

            for (int i = 0; i < contentPackThemes.Count; i++)
            {
                string name = contentPackThemes[i];

                int separator = name.LastIndexOf('.');

                string shortName =
                    separator >= 0
                        ? name[(separator + 1)..]
                        : name;

                monitor.Log(
                    $"    {shortName}",
                    LogLevel.Info);

                LogField(
                    monitor,
                    "Full Name",
                    name,
                    indent: 8);

                // 最後のテーマ以外だけ空行
                if (i < contentPackThemes.Count - 1)
                    LogBlankLine(monitor);
            }
        }

        //----------------------------------------
        // 共通ログ表示
        //----------------------------------------

        /// <summary>
        /// 項目名と値を位置を揃えて表示します。
        /// </summary>
        private static void LogField(
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

        /// <summary>
        /// 空行を表示します。
        /// </summary>
        private static void LogBlankLine(
            IMonitor monitor)
        {
            monitor.Log(
                "",
                LogLevel.Info);
        }
    }
}