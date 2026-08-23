using StardewModdingAPI;
using Ts_Core.Providers;
using Ts_Core.Services.ContentPatcherRelated;
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
            IPartnerProvider provider)
        {
            //----------------------------------------
            // Token
            //----------------------------------------

            // T's Coreのトークン関連情報を表示
            helper.ConsoleCommands.Add(
                "tscore_tokens",
                "Print all T's Core token values.",
                (command, args) =>
                {
                    DebugRelationshipLogger.Log(
                        monitor,
                        service,
                        provider);

                    DebugLogHelper.LogBlankLine(
                        monitor);

                    DebugLocationLogger.Log(
                        monitor);
                });

            // Relationship関連トークンを表示
            helper.ConsoleCommands.Add(
                "tscore_tokens_relationship",
                "Print relationship tokens.",
                (command, args) =>
                    DebugRelationshipLogger.Log(
                        monitor,
                        service,
                        provider));

            // Location関連トークンを表示
            helper.ConsoleCommands.Add(
                "tscore_tokens_location",
                "Print location tokens.",
                (command, args) =>
                    DebugLocationLogger.Log(
                        monitor));

            //----------------------------------------
            // Debug
            //----------------------------------------

            // T's Coreの機能を再読み込み
            helper.ConsoleCommands.Add(
                "tscore_reload",
                "Reload T's Core data. Usage: tscore_reload [all|warp|building|notification]",
                (command, args) =>
                    DebugReloadService.Reload(
                        helper,
                        monitor,
                        args));

            // Content PatcherのContent Packを再読み込み
            helper.ConsoleCommands.Add(
                "tscore_cp_reload",
                "Reload a Content Patcher content pack. Usage: tscore_cp_reload <ContentPackId>",
                (command, args) =>
                {
                    if (args.Length == 0
                        || string.IsNullOrWhiteSpace(args[0]))
                    {
                        monitor.Log(
                            "Content Pack ID is required. Usage: tscore_cp_reload <ContentPackId>",
                            LogLevel.Warn);

                        return;
                    }

                    ContentPatcherReloadService.ReloadContentPack(
                        args[0],
                        helper,
                        monitor);
                });

            // 登録されているWarp Providerを表示
            helper.ConsoleCommands.Add(
                "tscore_debug_warp",
                "Print all registered warp providers.",
                (command, args) =>
                    DebugWarpLogger.LogWarpProviders(
                        monitor));

            // 農場に存在する建物情報を表示
            helper.ConsoleCommands.Add(
                "tscore_debug_farmbuildings",
                "Print all farm buildings.",
                (command, args) =>
                    DebugBuildingLogger.LogFarmBuildings(
                        monitor));

            // 登録されているBuilding Providerを表示
            helper.ConsoleCommands.Add(
                "tscore_debug_buildings",
                "Print registered Building Providers. Usage: tscore_debug_buildings [ProviderId]",
                (command, args) =>
                    DebugBuildingLogger.LogBuildingProviders(
                        monitor,
                        args));

            // 開発用メモ:
            // ・内部確認用コマンド
            // ・Modder Guide等には記載しない
            // ・後々削除予定
            // 登録されているBuilding Lightを表示
            helper.ConsoleCommands.Add(
                "tscore_debug_lights",
                "Print all registered Building Lights.",
                (command, args) =>
                    DebugBuildingLogger.LogBuildingLights(
                        monitor));

            // 開発用メモ:
            // ・内部確認用コマンド
            // ・Modder Guide等には記載しない
            // ・後々削除予定
            // 登録されている条件付きBuilding DrawLayerを表示
            helper.ConsoleCommands.Add(
                "tscore_debug_drawlayers",
                "Print all registered conditional Building DrawLayers.",
                (command, args) =>
                    DebugBuildingLogger.LogBuildingDrawLayers(
                        monitor));

            //----------------------------------------
            // Notification
            //----------------------------------------

            // 登録済み通知テーマ一覧を表示
            helper.ConsoleCommands.Add(
                "tscore_debug_notification_themes",
                "Print notification themes.",
                (command, args) =>
                    DebugNotificationLogger.LogThemes(
                        monitor));

            // 通知表示をテスト
            helper.ConsoleCommands.Add(
                "tscore_debug_notification",
                "Show notification. Usage: <Type or ThemeName>",
                (command, args) =>
                    DebugNotificationService.ShowNotification(
                        args));

            // TriggerAction経由の通知をテスト
            helper.ConsoleCommands.Add(
                "tscore_debug_notification_trigger",
                "Usage: <Type> <Priority> <Duration> <Message...>",
                (command, args) =>
                    DebugNotificationService.ShowTriggerNotification(
                        args,
                        monitor));
        }
    }
}