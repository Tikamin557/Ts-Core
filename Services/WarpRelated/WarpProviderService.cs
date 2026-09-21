using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using Ts_Core.Models;

namespace Ts_Core.Services.WarpRelated
{
    /// <summary>
    /// Warp Providerのデバッグ情報です。
    /// </summary>
    internal sealed class RegisteredWarpProviderInfo
    {
        /// <summary>
        /// Provider IDです。
        /// </summary>
        public string Id { get; init; } = "";

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
        /// 基準座標から加算するX座標です。
        /// </summary>
        public int OffsetX { get; init; }

        /// <summary>
        /// 基準座標から加算するY座標です。
        /// </summary>
        public int OffsetY { get; init; }

        /// <summary>
        /// Warp先を解決できなかった場合に使用するProviderです。
        /// </summary>
        public string? Fallback { get; init; }
    }

    /// <summary>
    /// Warp Providerの管理・Warp先解決を行うサービスです。
    /// </summary>
    internal static class WarpProviderService
    {
        //----------------------------------------
        // 組み込みProvider ID
        //----------------------------------------

        private const string PlayerHomeProviderId =
            "PlayerHome";

        private const string PreviousHomeProviderId =
            "PreviousHome";

        private const string CurrentHomeProviderId =
            "CurrentHome";

        //----------------------------------------
        // Monitor
        //----------------------------------------

        private static IMonitor? Monitor;

        //----------------------------------------
        // Reserved Provider Warning
        //----------------------------------------

        private static readonly HashSet<string>
            WarnedReservedProviderIds =
                new(StringComparer.OrdinalIgnoreCase);

        //----------------------------------------
        // Initialize
        //----------------------------------------

        /// <summary>
        /// Warp Provider Serviceを初期化します。
        /// </summary>
        internal static void Initialize(
            IMonitor monitor)
        {
            Monitor =
                monitor;
        }

        //----------------------------------------
        // Provider情報取得
        //----------------------------------------

        /// <summary>
        /// 現在のWarp Provider Data Assetを取得します。
        /// </summary>
        private static Dictionary<
            string,
            WarpProviderModel> GetProviderData()
        {
            Dictionary<
                string,
                WarpProviderModel> providers =
                    Game1.content.Load<
                        Dictionary<
                            string,
                            WarpProviderModel>>(
                                WarpProviderDataService.AssetName);

            //----------------------------------------
            // 予約Provider ID確認
            //----------------------------------------

            foreach (string providerId in providers.Keys)
            {
                if (!WarpProviderDataService
                    .IsDefaultProvider(providerId))
                {
                    continue;
                }

                WarnReservedProvider(
                    providerId);
            }

            return providers;
        }

        //----------------------------------------
        // Reserved Provider Warning
        //----------------------------------------

        /// <summary>
        /// TsCore標準Provider IDへの
        /// 外部定義についてWarningを表示します。
        /// </summary>
        private static void WarnReservedProvider(
            string providerId)
        {
            //----------------------------------------
            // 同じIDは1回だけWarning
            //----------------------------------------

            if (!WarnedReservedProviderIds.Add(
                    providerId))
            {
                return;
            }

            Monitor?.Log(
                $"Warp Provider ID '{providerId}' is " +
                $"reserved by T's Core and cannot be overridden.",
                LogLevel.Warn);
        }

        //----------------------------------------
        // Provider一覧取得
        //----------------------------------------

        /// <summary>
        /// 現在登録されている
        /// Warp Provider情報を取得します。
        /// </summary>
        internal static IReadOnlyList<
            RegisteredWarpProviderInfo>
            GetRegisteredProviders()
        {
            List<RegisteredWarpProviderInfo> providers =
                new();

            //----------------------------------------
            // TsCore標準Provider
            //----------------------------------------

            foreach (
                KeyValuePair<
                    string,
                    WarpProviderModel> entry
                in WarpProviderDataService
                    .GetDefaultProviders())
            {
                AddProviderInfo(
                    providers,
                    entry.Key,
                    entry.Value);
            }

            //----------------------------------------
            // 外部Data Asset Provider
            //----------------------------------------

            foreach (
                KeyValuePair<
                    string,
                    WarpProviderModel> entry
                in GetProviderData())
            {
                string providerId =
                    entry.Key;

                WarpProviderModel provider =
                    entry.Value;

                //----------------------------------------
                // TsCore標準IDは予約済み
                //----------------------------------------

                if (WarpProviderDataService
                    .IsDefaultProvider(providerId))
                {
                    WarnReservedProvider(
                        providerId);

                    continue;
                }

                //----------------------------------------
                // 無効なProviderは表示対象外
                //----------------------------------------

                if (!IsValidProvider(
                        providerId,
                        provider))
                {
                    continue;
                }

                AddProviderInfo(
                    providers,
                    providerId,
                    provider);
            }

            return providers
                .OrderBy(provider =>
                    provider.Id)
                .ToList();
        }

        /// <summary>
        /// Warp Providerのデバッグ情報を追加します。
        /// </summary>
        private static void AddProviderInfo(
            List<RegisteredWarpProviderInfo> providers,
            string providerId,
            WarpProviderModel provider)
        {
            providers.Add(
                new RegisteredWarpProviderInfo
                {
                    Id = providerId,
                    Type = provider.Type,

                    SourceLocation =
                        provider.Source,

                    TargetLocation =
                        provider.Target,

                    MapLocation =
                        provider.Map,

                    BuildingType =
                        provider.BuildingType,

                    OffsetX =
                        provider.OffsetX,

                    OffsetY =
                        provider.OffsetY,

                    Fallback =
                        provider.Fallback
                });
        }

        //----------------------------------------
        // Provider存在確認
        //----------------------------------------

        /// <summary>
        /// 指定されたProviderが存在するか確認します。
        /// </summary>
        internal static bool ContainsProvider(
            string key)
        {
            if (string.IsNullOrWhiteSpace(
                    key))
            {
                return false;
            }

            //----------------------------------------
            // 組み込みProvider
            //----------------------------------------

            if (IsBuiltInProviderId(
                    key))
            {
                return true;
            }

            //----------------------------------------
            // Data Asset Provider
            //----------------------------------------

            return TryGetProvider(
                key,
                out _,
                out _);
        }

        //----------------------------------------
        // 組み込みProvider確認
        //----------------------------------------

        /// <summary>
        /// 指定されたIDがTsCore組み込みProviderの
        /// 予約IDか確認します。
        /// </summary>
        private static bool IsBuiltInProviderId(
            string key)
        {
            return string.Equals(
                    key,
                    PlayerHomeProviderId,
                    StringComparison.OrdinalIgnoreCase)
                || string.Equals(
                    key,
                    PreviousHomeProviderId,
                    StringComparison.OrdinalIgnoreCase)
                || string.Equals(
                    key,
                    CurrentHomeProviderId,
                    StringComparison.OrdinalIgnoreCase);
        }

        //----------------------------------------
        // Location名との重複確認
        //----------------------------------------

        /// <summary>
        /// 指定された名前と同名のGameLocationが
        /// 存在するか確認します。
        /// </summary>
        internal static bool HasLocationNameConflict(
            string key)
        {
            if (string.IsNullOrWhiteSpace(
                    key))
            {
                return false;
            }

            //----------------------------------------
            // 通常検索
            //----------------------------------------

            GameLocation? location =
                Game1.getLocationFromName(
                    key);

            if (location != null)
            {
                return true;
            }

            //----------------------------------------
            // 大文字小文字を無視して確認
            //----------------------------------------

            return Game1.locations.Any(location =>
                string.Equals(
                    location.Name,
                    key,
                    StringComparison.OrdinalIgnoreCase)
                || string.Equals(
                    location.NameOrUniqueName,
                    key,
                    StringComparison.OrdinalIgnoreCase));
        }

        //----------------------------------------
        // Provider取得
        //----------------------------------------

        /// <summary>
        /// 指定されたProviderを取得します。
        /// </summary>
        private static bool TryGetProvider(
            string key,
            out string providerId,
            out WarpProviderModel? provider)
        {
            providerId =
                "";

            provider =
                null;

            //----------------------------------------
            // 外部Data Asset取得
            //----------------------------------------

            Dictionary<
                string,
                WarpProviderModel> assetProviders =
                    GetProviderData();

            //----------------------------------------
            // TsCore標準Provider
            //----------------------------------------

            Dictionary<
                string,
                WarpProviderModel> defaultProviders =
                    WarpProviderDataService
                        .GetDefaultProviders();

            if (defaultProviders.TryGetValue(
                    key,
                    out WarpProviderModel?
                        defaultProvider))
            {
                providerId =
                    key;

                provider =
                    defaultProvider;

                return true;
            }

            //----------------------------------------
            // 外部Data Asset Provider
            //----------------------------------------

            foreach (
                KeyValuePair<
                    string,
                    WarpProviderModel> entry
                in assetProviders)
            {
                if (!string.Equals(
                        entry.Key,
                        key,
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                //----------------------------------------
                // TsCore標準IDは予約済み
                //----------------------------------------

                if (WarpProviderDataService
                    .IsDefaultProvider(entry.Key))
                {
                    WarnReservedProvider(
                        entry.Key);

                    return false;
                }

                if (!IsValidProvider(
                        entry.Key,
                        entry.Value))
                {
                    return false;
                }

                providerId =
                    entry.Key;

                provider =
                    entry.Value;

                return true;
            }

            return false;
        }

        //----------------------------------------
        // Provider Validation
        //----------------------------------------

        /// <summary>
        /// Warp Provider定義が有効か確認します。
        /// </summary>
        private static bool IsValidProvider(
            string providerId,
            WarpProviderModel provider)
        {
            //----------------------------------------
            // Provider ID
            //----------------------------------------

            if (string.IsNullOrWhiteSpace(
                    providerId))
            {
                return false;
            }

            //----------------------------------------
            // 組み込みProvider ID
            //----------------------------------------

            if (IsBuiltInProviderId(
                    providerId))
            {
                return false;
            }

            //----------------------------------------
            // Location名との重複
            //----------------------------------------

            if (HasLocationNameConflict(
                    providerId))
            {
                return false;
            }

            //----------------------------------------
            // Type別必須項目
            //----------------------------------------

            switch (provider.Type)
            {
                case "Warp":

                    return
                        !string.IsNullOrWhiteSpace(
                            provider.Source)
                        && !string.IsNullOrWhiteSpace(
                            provider.Target);

                case "MapEntry":

                    return
                        !string.IsNullOrWhiteSpace(
                            provider.Map)
                        && !string.IsNullOrWhiteSpace(
                            provider.Target);

                case "Building":

                    return
                        !string.IsNullOrWhiteSpace(
                            provider.BuildingType);

                default:

                    return false;
            }
        }

        //----------------------------------------
        // Provider解決
        //----------------------------------------

        /// <summary>
        /// 指定されたProviderからWarp先を解決します。
        /// </summary>
        internal static (string Location, Point Point)
            Resolve(
                string key,
                GameLocation? sourceLocation = null)
        {
            //----------------------------------------
            // PlayerHome
            //----------------------------------------

            if (string.Equals(
                    key,
                    PlayerHomeProviderId,
                    StringComparison.OrdinalIgnoreCase))
            {
                return GetPlayerHomeDestination();
            }

            //----------------------------------------
            // PreviousHome
            //----------------------------------------

            if (string.Equals(
                    key,
                    PreviousHomeProviderId,
                    StringComparison.OrdinalIgnoreCase))
            {
                return GetPreviousHomeDestination();
            }

            //----------------------------------------
            // CurrentHome
            //----------------------------------------

            if (string.Equals(
                    key,
                    CurrentHomeProviderId,
                    StringComparison.OrdinalIgnoreCase))
            {
                return GetCurrentHomeDestination(
                    sourceLocation);
            }

            //----------------------------------------
            // Provider取得
            //----------------------------------------

            if (!TryGetProvider(
                    key,
                    out string providerId,
                    out WarpProviderModel? provider)
                || provider == null)
            {
                throw new InvalidOperationException(
                    $"Warp Provider '{key}' was not found.");
            }

            return ResolveProvider(
                providerId,
                provider,
                sourceLocation);
        }

        //----------------------------------------
        // Provider定義解決
        //----------------------------------------

        /// <summary>
        /// Warp Provider定義からWarp先を解決します。
        /// </summary>
        private static (string Location, Point Point)
            ResolveProvider(
                string providerId,
                WarpProviderModel provider,
                GameLocation? sourceLocation)
        {
            switch (provider.Type)
            {
                //----------------------------------------
                // Warp
                //----------------------------------------

                case "Warp":

                    return GetWarpDestination(
                        provider.Source!,
                        provider.Target!,
                        provider.Fallback,
                        sourceLocation);

                //----------------------------------------
                // MapEntry
                //----------------------------------------

                case "MapEntry":

                    return GetMapEntryDestination(
                        provider.Map!,
                        provider.Target!,
                        provider.OffsetX,
                        provider.OffsetY,
                        provider.Fallback,
                        sourceLocation);

                //----------------------------------------
                // Building
                //----------------------------------------

                case "Building":

                    return GetBuildingDestination(
                        provider.BuildingType!,
                        provider.OffsetX,
                        provider.OffsetY,
                        provider.Fallback,
                        sourceLocation);

                //----------------------------------------
                // Unknown
                //----------------------------------------

                default:

                    throw new InvalidOperationException(
                        $"Warp Provider '{providerId}' has unsupported Type '{provider.Type}'.");
            }
        }

        //----------------------------------------
        // Fallback解決
        //----------------------------------------

        /// <summary>
        /// Fallback ProviderからWarp先を解決します。
        /// </summary>
        private static bool TryResolveFallback(
            string? fallback,
            GameLocation? sourceLocation,
            out (string Location, Point Point) destination)
        {
            destination =
                default;

            if (string.IsNullOrWhiteSpace(
                    fallback))
            {
                return false;
            }

            if (!ContainsProvider(
                    fallback))
            {
                return false;
            }

            destination =
                Resolve(
                    fallback,
                    sourceLocation);

            return true;
        }

        //----------------------------------------
        // Warp Provider
        //----------------------------------------

        private static (string Location, Point Point)
            GetWarpDestination(
                string sourceLocationName,
                string targetLocation,
                string? fallback,
                GameLocation? actionSourceLocation)
        {
            GameLocation? location =
                Game1.getLocationFromName(
                    sourceLocationName);

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

            if (TryResolveFallback(
                    fallback,
                    actionSourceLocation,
                    out (string Location, Point Point) destination))
            {
                return destination;
            }

            throw new InvalidOperationException(
                $"Warp Provider could not be resolved. " +
                $"Source: '{sourceLocationName}', " +
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
                string? fallback,
                GameLocation? actionSourceLocation)
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

            if (TryResolveFallback(
                    fallback,
                    actionSourceLocation,
                    out (string Location, Point Point) destination))
            {
                return destination;
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

        private static (string Location, Point Point)
            GetBuildingDestination(
                string buildingType,
                int offsetX,
                int offsetY,
                string? fallback,
                GameLocation? actionSourceLocation)
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
                            + offsetX,
                        building.tileY.Value
                            + offsetY)
                );
            }

            //----------------------------------------
            // Fallback
            //----------------------------------------

            if (TryResolveFallback(
                    fallback,
                    actionSourceLocation,
                    out (string Location, Point Point) destination))
            {
                return destination;
            }

            throw new InvalidOperationException(
                $"Building Warp Provider could not be resolved. " +
                $"BuildingType: '{buildingType}', " +
                $"Fallback: '{fallback ?? "(none)"}'.");
        }

        //----------------------------------------
        // PlayerHome Provider
        //----------------------------------------

        private static (string Location, Point Point)
            GetPlayerHomeDestination()
        {
            FarmHouse home =
                Utility.getHomeOfFarmer(
                    Game1.player);

            return (
                home.NameOrUniqueName,
                home.getEntryLocation()
            );
        }

        //----------------------------------------
        // PreviousHome Provider
        //----------------------------------------

        private static (string Location, Point Point)
            GetPreviousHomeDestination()
        {
            if (!PreviousHomeService.TryGetPreviousHome(
                    out FarmHouse? home)
                || home == null)
            {
                throw new InvalidOperationException(
                    "PreviousHome has not been recorded yet.");
            }

            return (
                home.NameOrUniqueName,
                home.getEntryLocation()
            );
        }

        //----------------------------------------
        // CurrentHome Provider
        //----------------------------------------

        private static (string Location, Point Point)
            GetCurrentHomeDestination(
                GameLocation? sourceLocation)
        {
            if (sourceLocation is not FarmHouse home)
            {
                throw new InvalidOperationException(
                    "CurrentHome can only be used inside a FarmHouse or Cabin.");
            }

            return (
                home.NameOrUniqueName,
                home.getEntryLocation()
            );
        }
    }
}