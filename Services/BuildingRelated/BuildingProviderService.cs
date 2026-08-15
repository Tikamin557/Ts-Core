using StardewModdingAPI;
using Ts_Core.Models;

namespace Ts_Core.Services.BuildingRelated
{
    /// <summary>
    /// 登録済みBuilding Providerの情報です。
    /// </summary>
    internal sealed class RegisteredBuildingProviderInfo
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
        /// 対象となる建物タイプです。
        /// </summary>
        public string BuildingType { get; init; } = "";

        /// <summary>
        /// Lightの有効・無効を制御するData/BuildingsのCustomFieldsキーです。
        /// </summary>
        public string? LightsEnabledField { get; init; }

        /// <summary>
        /// 登録されているライト一覧です。
        /// </summary>
        public IReadOnlyList<BuildingLightModel> Lights { get; init; }
            = Array.Empty<BuildingLightModel>();

        /// <summary>
        /// 登録されている条件付きDrawLayer一覧です。
        /// </summary>
        public IReadOnlyList<BuildingDrawLayerModel> DrawLayers { get; init; }
            = Array.Empty<BuildingDrawLayerModel>();
    }

    /// <summary>
    /// Building Providerを管理します。
    /// </summary>
    public static class BuildingProviderService
    {
        //----------------------------------------
        // 登録済みProvider
        //----------------------------------------

        private static readonly Dictionary<string, BuildingProviderModel>
            Providers =
                new(StringComparer.OrdinalIgnoreCase);

        //----------------------------------------
        // 登録済みProvider情報
        //----------------------------------------

        private static readonly Dictionary<string, RegisteredBuildingProviderInfo>
            RegisteredProviders =
                new(StringComparer.OrdinalIgnoreCase);

        //----------------------------------------
        // Provider取得
        //----------------------------------------

        /// <summary>
        /// 現在登録されているBuilding Provider情報を取得します。
        /// </summary>
        internal static IReadOnlyList<RegisteredBuildingProviderInfo>
            GetRegisteredProviders()
        {
            return RegisteredProviders.Values
                .OrderBy(provider => provider.Owner)
                .ThenBy(provider => provider.Id)
                .ToList();
        }

        /// <summary>
        /// 指定したBuildingTypeに対応するProviderを取得します。
        /// </summary>
        internal static IReadOnlyList<BuildingProviderModel>
            GetProvidersForBuilding(
                string buildingType)
        {
            return Providers.Values
                .Where(provider =>
                    string.Equals(
                        provider.BuildingType,
                        buildingType,
                        StringComparison.Ordinal))
                .ToList();
        }

        /// <summary>
        /// 登録されているすべてのProviderを取得します。
        /// </summary>
        internal static IReadOnlyCollection<BuildingProviderModel>
            GetProviders()
        {
            return Providers.Values;
        }

        //----------------------------------------
        // Provider登録
        //----------------------------------------

        /// <summary>
        /// Building Providerを登録します。
        /// </summary>
        public static void RegisterProvider(
            BuildingProviderModel model,
            string owner,
            string sourceFile,
            IMonitor monitor)
        {
            if (string.IsNullOrWhiteSpace(model.Id))
            {
                monitor.Log(
                    $"Building Provider in '{sourceFile}' has no Id.",
                    LogLevel.Warn);

                return;
            }

            if (string.IsNullOrWhiteSpace(model.BuildingType))
            {
                monitor.Log(
                    $"Building Provider '{model.Id}' has no BuildingType.",
                    LogLevel.Warn);

                return;
            }

            if (RegisteredProviders.TryGetValue(
                    model.Id,
                    out RegisteredBuildingProviderInfo? existingProvider))
            {
                monitor.Log(
                    $"Duplicate Building Provider '{model.Id}' ignored.\n" +
                    $"Already registered by: {existingProvider.Owner}\n" +
                    $"Existing file: {existingProvider.SourceFile}\n" +
                    $"Ignored provider owner: {owner}\n" +
                    $"Ignored file: {sourceFile}",
                    LogLevel.Warn);

                return;
            }

            Providers[model.Id] =
                model;

            RegisteredProviders[model.Id] =
                new RegisteredBuildingProviderInfo
                {
                    Id = model.Id,
                    Owner = owner,
                    SourceFile = sourceFile,
                    BuildingType = model.BuildingType,
                    LightsEnabledField = model.LightsEnabledField,

                    Lights = model.Lights
                        .ToList(),

                    DrawLayers = model.DrawLayers
                        .ToList()
                };

            monitor.Log(
                $"Registered Building Provider '{model.Id}' from '{owner}'.",
                LogLevel.Trace);
        }

        //----------------------------------------
        // Provider削除
        //----------------------------------------

        /// <summary>
        /// 登録済みBuilding Providerをすべて削除します。
        /// </summary>
        internal static void ClearProviders()
        {
            BuildingLightService.RemoveAllLights();

            Providers.Clear();
            RegisteredProviders.Clear();
        }
    }
}