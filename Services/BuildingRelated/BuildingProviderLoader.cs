using StardewModdingAPI;
using Ts_Core.Models;

namespace Ts_Core.Services.BuildingRelated
{
    /// <summary>
    /// Building Provider定義ファイルを読み込みます。
    /// </summary>
    public static class BuildingProviderLoader
    {
        //----------------------------------------
        // アセットフォルダ
        //----------------------------------------

        private const string AssetFolder = "buildings";

        //----------------------------------------
        // 読み込み
        //----------------------------------------

        public static void Load(
            IModHelper helper,
            IMonitor monitor)
        {
            ReadBuiltinProviders(
                helper,
                monitor);

            ReadContentPackProviders(
                helper,
                monitor);
        }

        //----------------------------------------
        // 再読み込み
        //----------------------------------------

        /// <summary>
        /// Building Providerをすべて再読み込みします。
        /// </summary>
        public static void Reload(
            IModHelper helper,
            IMonitor monitor)
        {
            monitor.Log(
                "Reloading Building Providers...",
                LogLevel.Info);

            //----------------------------------------
            // 現在の登録内容を削除
            //----------------------------------------

            BuildingProviderService.ClearProviders();

            //----------------------------------------
            // Providerを再読み込み
            //----------------------------------------

            Load(
                helper,
                monitor);

            int providerCount =
                BuildingProviderService
                    .GetRegisteredProviders()
                    .Count;

            monitor.Log(
                $"Building Providers reloaded successfully. Registered Providers: {providerCount}",
                LogLevel.Info);
        }

        //----------------------------------------
        // TsCore内のProviderを読み込み
        //----------------------------------------

        private static void ReadBuiltinProviders(
            IModHelper helper,
            IMonitor monitor)
        {
            string folder =
                Path.Combine(
                    helper.DirectoryPath,
                    "assets",
                    AssetFolder);

            if (!Directory.Exists(folder))
                return;

            foreach (string file in Directory.EnumerateFiles(
                folder,
                "*.json",
                SearchOption.AllDirectories))
            {
                string relative =
                    Path.GetRelativePath(
                        helper.DirectoryPath,
                        file);

                try
                {
                    BuildingProviderModel? provider =
                        helper.Data.ReadJsonFile<BuildingProviderModel>(
                            relative);

                    if (provider == null)
                        continue;

                    BuildingProviderService.RegisterProvider(
                        provider,
                        "T's Core",
                        relative,
                        monitor);

                    monitor.Log(
                        $"Loaded builtin Building Provider: {provider.Id}",
                        LogLevel.Trace);
                }
                catch (Exception ex)
                {
                    monitor.Log(
                        $"Failed to load Building Provider '{relative}': {ex.Message}",
                        LogLevel.Warn);
                }
            }
        }

        //----------------------------------------
        // Content PackのProviderを読み込み
        //----------------------------------------

        private static void ReadContentPackProviders(
            IModHelper helper,
            IMonitor monitor)
        {
            foreach (IContentPack pack in helper.ContentPacks.GetOwned())
            {
                string folder =
                    Path.Combine(
                        pack.DirectoryPath,
                        "assets",
                        AssetFolder);

                if (!Directory.Exists(folder))
                    continue;

                foreach (string file in Directory.EnumerateFiles(
                    folder,
                    "*.json",
                    SearchOption.AllDirectories))
                {
                    string relativePath =
                        Path.GetRelativePath(
                            pack.DirectoryPath,
                            file);

                    try
                    {
                        monitor.Log(
                            $"Loading Building Provider: {pack.Manifest.UniqueID}/{relativePath}",
                            LogLevel.Trace);

                        BuildingProviderModel? provider =
                            pack.ReadJsonFile<BuildingProviderModel>(
                                relativePath);

                        if (provider == null)
                            continue;

                        BuildingProviderService.RegisterProvider(
                            provider,
                            pack.Manifest.UniqueID,
                            relativePath,
                            monitor);

                        monitor.Log(
                            $"Loaded Building Provider: {provider.Id}",
                            LogLevel.Trace);
                    }
                    catch (Exception ex)
                    {
                        monitor.Log(
                            $"Failed to load Building Provider '{pack.Manifest.UniqueID}/{relativePath}': {ex.Message}",
                            LogLevel.Warn);
                    }
                }
            }
        }
    }
}