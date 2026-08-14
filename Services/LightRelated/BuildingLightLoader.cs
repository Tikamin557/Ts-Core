using StardewModdingAPI;
using Ts_Core.Models;

namespace Ts_Core.Services.LightRelated
{
    /// <summary>
    /// Building Light定義ファイルを読み込みます。
    /// </summary>
    public static class BuildingLightLoader
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
            ReadBuiltinLights(
                helper,
                monitor);

            ReadContentPackLights(
                helper,
                monitor);
        }

        //----------------------------------------
        // 再読み込み
        //----------------------------------------

        /// <summary>
        /// Building Light Providerをすべて再読み込みします。
        /// </summary>
        public static void Reload(
            IModHelper helper,
            IMonitor monitor)
        {
            monitor.Log(
                "Reloading Building Light Providers...",
                LogLevel.Info);

            //----------------------------------------
            // 現在の登録内容を削除
            //----------------------------------------

            BuildingLightService.ClearProviders();

            //----------------------------------------
            // Providerを再読み込み
            //----------------------------------------

            Load(
                helper,
                monitor);

            int providerCount =
                BuildingLightService
                    .GetRegisteredProviders()
                    .Count;

            monitor.Log(
                $"Building Light Providers reloaded successfully. Registered Providers: {providerCount}",
                LogLevel.Info);
        }

        //----------------------------------------
        // TsCore内のLightを読み込み
        //----------------------------------------

        private static void ReadBuiltinLights(
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
                    BuildingLightProviderModel? provider =
                        helper.Data.ReadJsonFile<BuildingLightProviderModel>(
                            relative);

                    if (provider == null)
                        continue;

                    BuildingLightService.RegisterProvider(
                        provider,
                        "T's Core",
                        relative,
                        monitor);

                    monitor.Log(
                        $"Loaded builtin Building Light Provider: {provider.Id}",
                        LogLevel.Trace);
                }
                catch (Exception ex)
                {
                    monitor.Log(
                        $"Failed to load Building Light Provider '{relative}': {ex.Message}",
                        LogLevel.Warn);
                }
            }
        }

        //----------------------------------------
        // Content PackのLightを読み込み
        //----------------------------------------

        private static void ReadContentPackLights(
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
                            $"Loading Building Light Provider: {pack.Manifest.UniqueID}/{relativePath}",
                            LogLevel.Trace);

                        BuildingLightProviderModel? provider =
                            pack.ReadJsonFile<BuildingLightProviderModel>(
                                relativePath);

                        if (provider == null)
                            continue;

                        BuildingLightService.RegisterProvider(
                            provider,
                            pack.Manifest.UniqueID,
                            relativePath,
                            monitor);

                        monitor.Log(
                            $"Loaded Building Light Provider: {provider.Id}",
                            LogLevel.Trace);
                    }
                    catch (Exception ex)
                    {
                        monitor.Log(
                            $"Failed to load Building Light Provider '{pack.Manifest.UniqueID}/{relativePath}': {ex.Message}",
                            LogLevel.Warn);
                    }
                }
            }
        }
    }
}