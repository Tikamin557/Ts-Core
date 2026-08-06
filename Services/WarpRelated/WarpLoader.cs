using StardewModdingAPI;
using Ts_Core.Models;

namespace Ts_Core.Services.WarpRelated
{
    /// <summary>
    /// Warp定義ファイルを読み込み、Warp情報を管理するクラスです。
    /// </summary>
    public static class WarpLoader
    {

        //----------------------------------------
        // アセットフォルダ
        //----------------------------------------

        private const string AssetFolder = "warp";

        //----------------------------------------
        // 標準Warp一覧
        //----------------------------------------

        private static readonly (string Name, WarpProviderModel Warp)[] DefaultWarps =
        {
            (
                "FarmHouseFront",
                new WarpProviderModel
                {
                    Id = "FarmHouseFront",
                    Type = "Warp",
                    Source = "FarmHouse",
                    Target = "Farm"
                }
            ),

            (
                "GreenhouseFront",
                new WarpProviderModel
                {
                    Id = "GreenhouseFront",
                    Type = "Warp",
                    Source = "Greenhouse",
                    Target = "Farm"
                }
            ),

            (
                "FarmCaveFront",
                new WarpProviderModel
                {
                    Id = "FarmCaveFront",
                    Type = "Warp",
                    Source = "FarmCave",
                    Target = "Farm"
                }
            ),

            (
                "IslandFarmHouseFront",
                new WarpProviderModel
                {
                    Id = "IslandFarmHouseFront",
                    Type = "Warp",
                    Source = "IslandFarmHouse",
                    Target = "IslandWest"
                }
            ),
        };

        //----------------------------------------
        // 読み込み
        //----------------------------------------

        public static void Load(
            IModHelper helper,
            IMonitor monitor)
        {
            ExportDefaultWarps(helper);

            ReadBuiltinWarps(
                helper,
                monitor);

            ReadContentPackWarps(
                helper,
                monitor);
        }

        //----------------------------------------
        // 再読み込み
        //----------------------------------------

        /// <summary>
        /// Warp Providerをすべて再読み込みします。
        /// </summary>
        public static void Reload(
            IModHelper helper,
            IMonitor monitor)
        {
            monitor.Log(
                "Reloading Warp Providers...",
                LogLevel.Info);

            //----------------------------------------
            // 現在の登録内容を削除
            //----------------------------------------

            WarpService.ClearProviders();

            //----------------------------------------
            // Providerを再読み込み
            //----------------------------------------

            Load(
                helper,
                monitor);

            int providerCount =
                WarpService
                    .GetRegisteredProviders()
                    .Count;

            monitor.Log(
                $"Warp Providers reloaded successfully. Registered Providers: {providerCount}",
                LogLevel.Info);
        }

        //----------------------------------------
        // デフォルトWarpを書き出し
        //----------------------------------------

        private static void ExportDefaultWarps(
            IModHelper helper)
        {
            string folder =
                Path.Combine(
                    helper.DirectoryPath,
                    "assets",
                    AssetFolder);

            Directory.CreateDirectory(folder);

            foreach (var warp in DefaultWarps)
            {
                ExportIfMissing(
                    helper,
                    $"{warp.Name}.json",
                    warp.Warp);
            }
        }

        //----------------------------------------
        // Warpファイルを書き出し
        //----------------------------------------

        private static void ExportIfMissing(
            IModHelper helper,
            string fileName,
            WarpProviderModel warp)
        {
            string relative =
                Path.Combine(
                    "assets",
                    AssetFolder,
                    fileName);

            string full =
                Path.Combine(
                    helper.DirectoryPath,
                    relative);

            if (File.Exists(full))
                return;

            helper.Data.WriteJsonFile(
                relative,
                warp);
        }

        //----------------------------------------
        // TsCore内のWarpを読み込み
        //----------------------------------------

        private static void ReadBuiltinWarps(
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
                //----------------------------------------
                // assets からの相対パス
                //----------------------------------------

                string relative =
                    Path.GetRelativePath(
                        helper.DirectoryPath,
                        file);

                WarpProviderModel? provider =
                    helper.Data.ReadJsonFile<WarpProviderModel>(relative);

                if (provider == null)
                    continue;

                WarpService.RegisterProvider(
                    provider,
                    "T's Core",
                    relative,
                    monitor);

                monitor.Log(
                    $"Loaded builtin warp provider: {provider.Id}",
                    LogLevel.Trace);
            }
        }

        //----------------------------------------
        // ContentPackのWarpを読み込み
        //----------------------------------------

        private static void ReadContentPackWarps(
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
                    //----------------------------------------
                    // ContentPack内からの相対パス
                    //----------------------------------------

                    string relativePath =
                        Path.GetRelativePath(
                            pack.DirectoryPath,
                            file);

                    try
                    {
                        monitor.Log(
                            $"Loading warp provider: {pack.Manifest.UniqueID}/{relativePath}",
                            LogLevel.Trace);

                        WarpProviderModel? provider =
                            pack.ReadJsonFile<WarpProviderModel>(relativePath);

                        if (provider == null)
                            continue;

                        WarpService.RegisterProvider(
                            provider,
                            pack.Manifest.UniqueID,
                            relativePath,
                            monitor);

                        monitor.Log(
                            $"Loaded warp provider: {provider.Id}",
                            LogLevel.Trace);
                    }
                    catch (Exception ex)
                    {
                        monitor.Log(
                            $"Failed to load provider '{pack.Manifest.UniqueID}/{relativePath}': {ex.Message}",
                            LogLevel.Warn);
                    }
                }
            }
        }
    }
}