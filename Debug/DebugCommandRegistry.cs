using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using Ts_Core.Providers;
using Ts_Core.Readers;
using Ts_Core.Services;
using Ts_Core.Services.Location;
using Ts_Core.Services.Notification;
using Ts_Core.Services.Relationship;

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
            IPartnerProvider provider,
            LocationService locationService)
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

                    LogLocation(
                        monitor);

                    LogWarp(
                        monitor,
                        locationService);
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

            // Warp関連トークンを表示
            helper.ConsoleCommands.Add(
                "tscore_tokens_warp",
                "Print warp tokens.",
                (command, args) =>
                    LogWarp(
                        monitor,
                        locationService));

            //----------------------------------------
            // Debug
            //----------------------------------------

            // 農場に存在する建物情報を表示
            helper.ConsoleCommands.Add(
                "tscore_debug_buildings",
                "Print all farm buildings.",
                (command, args) =>
                    LogBuildings(
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
                        "Usage: tscore_debug_trigger <Type> <Priority> <Duration> <Message...>",
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
        // Relationship のログ表示
        //----------------------------------------

        private static void LogRelationship(
            IMonitor monitor,
            PartnerService service,
            IPartnerProvider provider)
        {
            monitor.Log(
                "===== Relationship =====",
                LogLevel.Info);

            var partners =
                service.GetPartners()?.ToList()
                ?? new List<string>();

            var orderedPartners =
                service.GetRoomOrderedPartners()?.ToList()
                ?? new List<string>();

            monitor.Log(
                $"Provider: {service.GetProviderName()} ({provider.Description})",
                LogLevel.Info);

            monitor.Log(
                $"Partners ({partners.Count}): {string.Join(", ", partners)}",
                LogLevel.Info);

            monitor.Log(
                $"RoomMod: {RoomOrderReader.CurrentRoomMod}",
                LogLevel.Info);

            monitor.Log(
                $"SpouseRoomOrder ({orderedPartners.Count}): {string.Join(", ", orderedPartners)}",
                LogLevel.Info);

            monitor.Log(
                "SpouseRoomOrder (room index):",
                LogLevel.Info);

            for (int i = 0; i < orderedPartners.Count; i++)
            {
                monitor.Log(
                    $"[{i}] {orderedPartners[i]}",
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

            monitor.Log(
                $"CurrentLocation: {Game1.currentLocation?.NameOrUniqueName}",
                LogLevel.Info);

            monitor.Log(
                $"PreviousLocation: {LocationTracker.PreviousLocation}",
                LogLevel.Info);

            monitor.Log(
                $"LocationElapsed: {LocationTracker.LocationElapsed}",
                LogLevel.Info);

            monitor.Log(
                $"VisitCount: {LocationTracker.VisitCount()}",
                LogLevel.Info);

            monitor.Log(
                $"SessionVisitCount: {LocationTracker.SessionVisitCount()}",
                LogLevel.Info);

            monitor.Log(
                $"EnteredToday: {LocationTracker.EnteredToday()}",
                LogLevel.Info);

            monitor.Log(
                $"IsOutdoors: {LocationTracker.IsOutdoors()}",
                LogLevel.Info);

            monitor.Log(
                $"IsIndoors: {LocationTracker.IsIndoors()}",
                LogLevel.Info);

            // デバッグ情報
            monitor.Log(
                $"viewport: {Game1.viewport.Width} x {Game1.viewport.Height}",
                LogLevel.Info);

            // デバッグ情報
            monitor.Log(
                $"uiViewport: {Game1.uiViewport.Width} x {Game1.uiViewport.Height}",
                LogLevel.Info);
        }

        //----------------------------------------
        // Warp のログ表示
        //----------------------------------------

        private static void LogWarp(
            IMonitor monitor,
            LocationService locationService)
        {
            monitor.Log(
                "===== Warp =====",
                LogLevel.Info);

            monitor.Log(
                $"FarmHouseEntry: {locationService.GetFarmHouseEntry().FirstOrDefault()}",
                LogLevel.Info);

            monitor.Log(
                $"FarmHouseEntryX: {locationService.GetFarmHouseEntryX().FirstOrDefault()}",
                LogLevel.Info);

            monitor.Log(
                $"FarmHouseEntryY: {locationService.GetFarmHouseEntryY().FirstOrDefault()}",
                LogLevel.Info);
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

            foreach (Building building in farm.buildings)
            {
                monitor.Log(
                    $"Type: {building.buildingType.Value}",
                    LogLevel.Info);

                monitor.Log(
                    $"  Tile: ({building.tileX.Value}, {building.tileY.Value})",
                    LogLevel.Info);

                monitor.Log(
                    $"  Size: {building.tilesWide.Value} x {building.tilesHigh.Value}",
                    LogLevel.Info);

                monitor.Log(
                    $"  Indoors: {building.GetIndoors()?.NameOrUniqueName ?? "(none)"}",
                    LogLevel.Info);
            }
        }

        //----------------------------------------
        // Notification Theme のログ表示
        //----------------------------------------

        private static void LogNotificationThemes(
            IMonitor monitor)
        {
            monitor.Log(
                "===== Notification Themes =====",
                LogLevel.Info);

            //----------------------------------------
            // TsCore自体のテーマ
            //----------------------------------------

            monitor.Log(
                "[TsCore]",
                LogLevel.Info);

            foreach (string name in NotificationThemeManager.GetBuiltinThemeNames())
            {
                monitor.Log(
                    $"  {name}",
                    LogLevel.Info);
            }

            //----------------------------------------
            // Content Packで追加されたテーマ
            //----------------------------------------

            monitor.Log(
                "[Content Packs]",
                LogLevel.Info);

            foreach (string name in NotificationThemeManager.GetContentPackThemeNames())
            {
                int separator = name.LastIndexOf('.');

                string shortName =
                    separator >= 0
                        ? name[(separator + 1)..]
                        : name;

                monitor.Log(
                    $"  {shortName} ({name})",
                    LogLevel.Info);
            }
        }
    }
}