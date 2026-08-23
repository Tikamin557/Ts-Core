using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using Ts_Core.Models;

namespace Ts_Core.Services.WarpRelated
{
    /// <summary>
    /// 登録済みWarp Providerのデバッグ情報です。
    /// </summary>
    internal sealed class RegisteredWarpProviderInfo
    {
        /// <summary>
        /// Provider IDです。
        /// </summary>
        public string Id { get; init; } = "";

        /// <summary>
        /// Providerを登録したModまたはContent Packです。
        /// </summary>
        public string Owner { get; init; } = "";

        /// <summary>
        /// Provider定義ファイルのパスです。
        /// </summary>
        public string SourceFile { get; init; } = "";

        /// <summary>
        /// Providerの種類です。
        /// </summary>
        public string Type { get; init; } = "";

        /// <summary>
        /// Warp検索元のLocation名です。
        /// </summary>
        public string? SourceLocation { get; init; }

        /// <summary>
        /// Warpの移動先Location名です。
        /// </summary>
        public string? TargetLocation { get; init; }

        /// <summary>
        /// MapEntryで検索対象となるLocation名です。
        /// </summary>
        public string? MapLocation { get; init; }

        /// <summary>
        /// 検索対象の建物タイプです。
        /// </summary>
        public string? BuildingType { get; init; }

        /// <summary>
        /// 建物座標から加算するX座標です。
        /// </summary>
        public int OffsetX { get; init; }

        /// <summary>
        /// 建物座標から加算するY座標です。
        /// </summary>
        public int OffsetY { get; init; }

        /// <summary>
        /// Warp先を解決できなかった場合に使用するProviderです。
        /// </summary>
        public string? Fallback { get; init; }
    }

    /// <summary>
    /// Warp Providerの登録・管理・Warp先解決を行うサービスです。
    /// </summary>
    internal static class WarpProviderService
    {
        //----------------------------------------
        // 登録済みProvider
        //----------------------------------------

        private static readonly Dictionary<
            string,
            Func<(string Location, Point Point)>>
            WarpProviders =
                new(
                    StringComparer.OrdinalIgnoreCase);

        //----------------------------------------
        // 登録済みProvider情報
        //----------------------------------------

        private static readonly Dictionary<
            string,
            RegisteredWarpProviderInfo>
            RegisteredProviders =
                new(
                    StringComparer.OrdinalIgnoreCase);

        //----------------------------------------
        // Provider情報取得
        //----------------------------------------

        /// <summary>
        /// 現在登録されているWarp Provider情報を取得します。
        /// </summary>
        internal static IReadOnlyList<RegisteredWarpProviderInfo>
            GetRegisteredProviders()
        {
            return RegisteredProviders.Values
                .OrderBy(provider => provider.Owner)
                .ThenBy(provider => provider.Id)
                .ToList();
        }

        //----------------------------------------
        // Provider再読み込み
        //----------------------------------------

        /// <summary>
        /// 登録済みのWarp Providerをすべて削除します。
        /// </summary>
        internal static void ClearProviders()
        {
            WarpProviders.Clear();
            RegisteredProviders.Clear();
        }

        //----------------------------------------
        // Provider存在確認
        //----------------------------------------

        /// <summary>
        /// 指定されたProviderが登録されているか確認します。
        /// </summary>
        internal static bool ContainsProvider(
            string key)
        {
            return WarpProviders.ContainsKey(
                key);
        }

        //----------------------------------------
        // Provider解決
        //----------------------------------------

        /// <summary>
        /// 指定されたProviderからWarp先を解決します。
        /// </summary>
        internal static (string Location, Point Point)
            Resolve(
                string key)
        {
            if (!WarpProviders.TryGetValue(
                    key,
                    out Func<(string Location, Point Point)>? provider))
            {
                throw new InvalidOperationException(
                    $"Warp Provider '{key}' was not found.");
            }

            return provider();
        }

        //----------------------------------------
        // Provider追加
        //----------------------------------------

        private static void AddProvider(
            string key,
            Func<(string Location, Point Point)> provider)
        {
            WarpProviders[key] =
                provider;
        }

        //----------------------------------------
        // Warp Provider登録
        //----------------------------------------

        /// <summary>
        /// Warp Providerを登録します。
        /// </summary>
        internal static void RegisterProvider(
            WarpProviderModel model,
            string owner,
            string sourceFile,
            IMonitor monitor)
        {
            //----------------------------------------
            // Provider IDチェック
            //----------------------------------------

            if (string.IsNullOrWhiteSpace(
                    model.Id))
            {
                monitor.Log(
                    $"Warp Provider in '{sourceFile}' has no Id.",
                    LogLevel.Warn);

                return;
            }

            //----------------------------------------
            // 重複チェック
            //----------------------------------------

            if (RegisteredProviders.TryGetValue(
                    model.Id,
                    out RegisteredWarpProviderInfo? existingProvider))
            {
                monitor.Log(
                    $"Duplicate Warp Provider '{model.Id}' ignored.\n" +
                    $"Already registered by: {existingProvider.Owner}\n" +
                    $"Existing file: {existingProvider.SourceFile}\n" +
                    $"Ignored provider owner: {owner}\n" +
                    $"Ignored file: {sourceFile}",
                    LogLevel.Warn);

                return;
            }

            //----------------------------------------
            // 必須項目チェック
            //----------------------------------------

            switch (model.Type)
            {
                case "Warp":

                    if (string.IsNullOrWhiteSpace(
                            model.Source))
                    {
                        monitor.Log(
                            $"Warp Provider '{model.Id}' of type Warp has no Source.",
                            LogLevel.Warn);

                        return;
                    }

                    if (string.IsNullOrWhiteSpace(
                            model.Target))
                    {
                        monitor.Log(
                            $"Warp Provider '{model.Id}' of type Warp has no Target.",
                            LogLevel.Warn);

                        return;
                    }

                    break;

                case "MapEntry":

                    if (string.IsNullOrWhiteSpace(
                            model.Map))
                    {
                        monitor.Log(
                            $"Warp Provider '{model.Id}' of type MapEntry has no Map.",
                            LogLevel.Warn);

                        return;
                    }

                    if (string.IsNullOrWhiteSpace(
                            model.Target))
                    {
                        monitor.Log(
                            $"Warp Provider '{model.Id}' of type MapEntry has no Target.",
                            LogLevel.Warn);

                        return;
                    }

                    break;

                case "Building":

                    if (string.IsNullOrWhiteSpace(
                            model.BuildingType))
                    {
                        monitor.Log(
                            $"Warp Provider '{model.Id}' of type Building has no BuildingType.",
                            LogLevel.Warn);

                        return;
                    }

                    break;
            }

            //----------------------------------------
            // Provider登録
            //----------------------------------------

            switch (model.Type)
            {
                case "Warp":

                    AddProvider(
                        model.Id,
                        () => GetWarpDestination(
                            model.Source!,
                            model.Target!,
                            model.Fallback));

                    break;

                case "MapEntry":

                    AddProvider(
                        model.Id,
                        () => GetMapEntryDestination(
                            model.Map!,
                            model.Target!,
                            model.OffsetX,
                            model.OffsetY,
                            model.Fallback));

                    break;

                case "Building":

                    RegisterBuildingWarp(
                        model.Id,
                        model.BuildingType!,
                        model.OffsetX,
                        model.OffsetY,
                        model.Fallback);

                    break;

                default:

                    monitor.Log(
                        $"Unknown Warp Provider type '{model.Type}' " +
                        $"in '{Path.GetFileName(sourceFile)}'.",
                        LogLevel.Warn);

                    return;
            }

            //----------------------------------------
            // Provider情報保存
            //----------------------------------------

            RegisteredProviders[model.Id] =
                new RegisteredWarpProviderInfo
                {
                    Id = model.Id,
                    Owner = owner,
                    SourceFile = sourceFile,
                    Type = model.Type,

                    SourceLocation = model.Source,
                    TargetLocation = model.Target,
                    MapLocation = model.Map,

                    BuildingType = model.BuildingType,
                    OffsetX = model.OffsetX,
                    OffsetY = model.OffsetY,

                    Fallback = model.Fallback
                };

            monitor.Log(
                $"Registered Warp Provider '{model.Id}' " +
                $"from '{owner}'.",
                LogLevel.Trace);
        }

        //----------------------------------------
        // Warp Provider
        //----------------------------------------

        private static (string Location, Point Point)
            GetWarpDestination(
                string sourceLocation,
                string targetLocation,
                string? fallback)
        {
            GameLocation? location =
                Game1.getLocationFromName(
                    sourceLocation);

            if (location != null)
            {
                Warp? warp =
                    location.warps
                        .FirstOrDefault(w =>
                            string.Equals(
                                w.TargetName,
                                targetLocation,
                                StringComparison.OrdinalIgnoreCase));

                if (warp != null)
                {
                    return (
                        warp.TargetName,
                        new Point(
                            warp.TargetX,
                            warp.TargetY)
                    );
                }
            }

            //----------------------------------------
            // Fallback
            //----------------------------------------

            if (!string.IsNullOrWhiteSpace(
                    fallback)
                && WarpProviders.TryGetValue(
                    fallback,
                    out Func<(string Location, Point Point)>? fallbackProvider))
            {
                return fallbackProvider();
            }

            throw new InvalidOperationException(
                $"Warp Provider could not be resolved. " +
                $"Source: '{sourceLocation}', " +
                $"Target: '{targetLocation}', " +
                $"Fallback: '{fallback ?? "(none)"}'.");
        }

        //----------------------------------------
        // MapEntry Provider
        //----------------------------------------

        private static (string Location, Point Point)
            GetMapEntryDestination(
                string mapLocation,
                string targetLocation,
                int offsetX,
                int offsetY,
                string? fallback)
        {
            GameLocation? location =
                Game1.getLocationFromName(
                    mapLocation);

            if (location != null)
            {
                Warp? warp =
                    location.warps
                        .FirstOrDefault(w =>
                            string.Equals(
                                w.TargetName,
                                targetLocation,
                                StringComparison.OrdinalIgnoreCase));

                if (warp != null)
                {
                    return (
                        location.NameOrUniqueName,
                        new Point(
                            warp.X + offsetX,
                            warp.Y + offsetY)
                    );
                }
            }

            //----------------------------------------
            // Fallback
            //----------------------------------------

            if (!string.IsNullOrWhiteSpace(
                    fallback)
                && WarpProviders.TryGetValue(
                    fallback,
                    out Func<(string Location, Point Point)>? fallbackProvider))
            {
                return fallbackProvider();
            }

            throw new InvalidOperationException(
                $"MapEntry Provider could not be resolved. " +
                $"Map: '{mapLocation}', " +
                $"Target: '{targetLocation}', " +
                $"Fallback: '{fallback ?? "(none)"}'.");
        }

        //----------------------------------------
        // Building Provider
        //----------------------------------------

        private static void RegisterBuildingWarp(
            string key,
            string buildingType,
            int xoffset,
            int yoffset,
            string? fallback)
        {
            AddProvider(
                key,
                () =>
                {
                    Farm farm =
                        Game1.getFarm();

                    Building? building =
                        farm.buildings
                            .FirstOrDefault(b =>
                                b.buildingType.Value
                                == buildingType);

                    if (building != null)
                    {
                        return (
                            "Farm",
                            new Point(
                                building.tileX.Value
                                    + xoffset,
                                building.tileY.Value
                                    + yoffset)
                        );
                    }

                    //----------------------------------------
                    // Fallback
                    //----------------------------------------

                    if (!string.IsNullOrWhiteSpace(
                            fallback)
                        && WarpProviders.TryGetValue(
                            fallback,
                            out Func<(string Location, Point Point)>? fallbackProvider))
                    {
                        return fallbackProvider();
                    }

                    throw new InvalidOperationException(
                        $"Building Warp Provider could not be resolved. " +
                        $"BuildingType: '{buildingType}', " +
                        $"Fallback: '{fallback ?? "(none)"}'.");
                });
        }
    }
}