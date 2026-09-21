using StardewValley;
using Ts_Core.Models;

namespace Ts_Core.Services.BuildingRelated
{
    /// <summary>
    /// 登録されているBuilding Providerの情報です。
    ///
    /// Debug表示用に使用します。
    /// Provider IDは
    /// TsCore/BuildingProvidersのEntry Keyです。
    /// </summary>
    internal sealed class RegisteredBuildingProviderInfo
    {
        //----------------------------------------
        // Provider
        //----------------------------------------

        /// <summary>
        /// Provider IDです。
        ///
        /// TsCore/BuildingProvidersのEntry Keyです。
        /// </summary>
        public string Id { get; init; } = "";

        /// <summary>
        /// 対象となる建物タイプです。
        /// </summary>
        public string BuildingType { get; init; } = "";

        /// <summary>
        /// Building Provider全体が
        /// 有効かどうかです。
        /// </summary>
        public bool BuildingsEnabled { get; init; } = true;

        /// <summary>
        /// バレーのメイン農場でのみ
        /// 建築可能かどうかです。
        /// </summary>
        public bool ValleyFarmOnly { get; init; }

        //----------------------------------------
        // Enabled Fields
        //----------------------------------------

        /// <summary>
        /// Building Provider全体の有効・無効を制御する
        /// Data/BuildingsのCustomFieldsキーです。
        /// </summary>
        public string? BuildingsEnabledField { get; init; }

        /// <summary>
        /// Lights全体が
        /// 有効かどうかです。
        /// </summary>
        public bool LightsEnabled { get; init; } = true;

        /// <summary>
        /// Lightの有効・無効を制御する
        /// Data/BuildingsのCustomFieldsキーです。
        /// </summary>
        public string? LightsEnabledField { get; init; }

        /// <summary>
        /// DrawLayers全体が
        /// 有効かどうかです。
        /// </summary>
        public bool DrawLayersEnabled { get; init; } = true;

        /// <summary>
        /// DrawLayerの有効・無効を制御する
        /// Data/BuildingsのCustomFieldsキーです。
        /// </summary>
        public string? DrawLayersEnabledField { get; init; }

        //----------------------------------------
        // Extensions
        //----------------------------------------

        /// <summary>
        /// 登録されているLight一覧です。
        /// </summary>
        public IReadOnlyList<BuildingLightModel> Lights { get; init; }
            = Array.Empty<BuildingLightModel>();

        /// <summary>
        /// 登録されているDrawLayer一覧です。
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
        // Provider Data
        //----------------------------------------

        /// <summary>
        /// TsCore/BuildingProvidersから
        /// Building Providerデータを取得します。
        /// </summary>
        private static Dictionary<
            string,
            BuildingProviderModel> GetProviderData()
        {
            return Game1.content.Load<
                Dictionary<
                    string,
                    BuildingProviderModel>>(
                        BuildingProviderDataService.AssetName);
        }

        //----------------------------------------
        // Registered Providers
        //----------------------------------------

        /// <summary>
        /// 現在登録されている
        /// Building Provider情報を取得します。
        ///
        /// Debug表示用です。
        /// </summary>
        internal static IReadOnlyList<
            RegisteredBuildingProviderInfo>
            GetRegisteredProviders()
        {
            return GetProviderData()
                .Select(
                    entry =>
                        new RegisteredBuildingProviderInfo
                        {
                            Id =
                                entry.Key,

                            BuildingType =
                                entry.Value.BuildingType,

                            BuildingsEnabled =
                                entry.Value.BuildingsEnabled,

                            ValleyFarmOnly =
                                entry.Value.ValleyFarmOnly,

                            BuildingsEnabledField =
                                entry.Value.BuildingsEnabledField,

                            LightsEnabled =
                                entry.Value.LightsEnabled,

                            LightsEnabledField =
                                entry.Value.LightsEnabledField,

                            DrawLayersEnabled =
                                entry.Value.DrawLayersEnabled,

                            DrawLayersEnabledField =
                                entry.Value.DrawLayersEnabledField,

                            Lights =
                                entry.Value.Lights
                                    .ToList(),

                            DrawLayers =
                                entry.Value.DrawLayers
                                    .ToList()
                        })
                .OrderBy(
                    provider =>
                        provider.Id,
                    StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        //----------------------------------------
        // Provider取得
        //----------------------------------------

        /// <summary>
        /// 登録されているすべての
        /// Building Providerを
        /// Provider IDと一緒に取得します。
        /// </summary>
        internal static IReadOnlyList<
            KeyValuePair<
                string,
                BuildingProviderModel>>
            GetProviderEntries()
        {
            return GetProviderData()
                .ToList();
        }

        /// <summary>
        /// 指定したBuildingTypeに対応する
        /// Building Providerを取得します。
        /// </summary>
        internal static IReadOnlyList<
            BuildingProviderModel>
            GetProvidersForBuilding(
                string buildingType)
        {
            return GetProviderData()
                .Values
                .Where(
                    provider =>
                        string.Equals(
                            provider.BuildingType,
                            buildingType,
                            StringComparison.Ordinal))
                .ToList();
        }

        /// <summary>
        /// 登録されているすべての
        /// Building Providerを取得します。
        /// </summary>
        internal static IReadOnlyCollection<
            BuildingProviderModel>
            GetProviders()
        {
            return GetProviderData()
                .Values
                .ToList();
        }

        //----------------------------------------
        // Provider有効判定
        //----------------------------------------

        /// <summary>
        /// Building Provider全体が
        /// 有効か判定します。
        /// </summary>
        internal static bool IsProviderEnabled(
            BuildingProviderModel provider)
        {
            //----------------------------------------
            // Direct Enabled
            //----------------------------------------

            if (!provider.BuildingsEnabled)
            {
                return false;
            }

            //----------------------------------------
            // EnabledField
            //----------------------------------------

            return IsEnabledField(
                provider,
                provider.BuildingsEnabledField);
        }

        //----------------------------------------
        // Lights有効判定
        //----------------------------------------

        /// <summary>
        /// Building ProviderのLights全体が
        /// 有効か判定します。
        /// </summary>
        internal static bool AreLightsEnabled(
            BuildingProviderModel provider)
        {
            //----------------------------------------
            // Provider
            //----------------------------------------

            if (!IsProviderEnabled(
                    provider))
            {
                return false;
            }

            //----------------------------------------
            // Direct Enabled
            //----------------------------------------

            if (!provider.LightsEnabled)
            {
                return false;
            }

            //----------------------------------------
            // EnabledField
            //----------------------------------------

            return IsEnabledField(
                provider,
                provider.LightsEnabledField);
        }

        //----------------------------------------
        // DrawLayers有効判定
        //----------------------------------------

        /// <summary>
        /// Building ProviderのDrawLayers全体が
        /// 有効か判定します。
        /// </summary>
        internal static bool AreDrawLayersEnabled(
            BuildingProviderModel provider)
        {
            //----------------------------------------
            // Provider
            //----------------------------------------

            if (!IsProviderEnabled(
                    provider))
            {
                return false;
            }

            //----------------------------------------
            // Direct Enabled
            //----------------------------------------

            if (!provider.DrawLayersEnabled)
            {
                return false;
            }

            //----------------------------------------
            // EnabledField
            //----------------------------------------

            return IsEnabledField(
                provider,
                provider.DrawLayersEnabledField);
        }

        //----------------------------------------
        // Enabled Field
        //----------------------------------------

        /// <summary>
        /// Data/BuildingsのCustomFieldsを確認し、
        /// 指定されたEnabledFieldが
        /// 有効か判定します。
        /// </summary>
        internal static bool IsEnabledField(
            BuildingProviderModel provider,
            string? enabledField)
        {
            //----------------------------------------
            // 未指定
            //----------------------------------------

            if (string.IsNullOrWhiteSpace(
                    enabledField))
            {
                return true;
            }

            //----------------------------------------
            // Building Data
            //----------------------------------------

            Dictionary<
                string,
                StardewValley.GameData.Buildings.BuildingData>
                buildingData =
                    DataLoader.Buildings(
                        Game1.content);

            if (!buildingData.TryGetValue(
                    provider.BuildingType,
                    out StardewValley.GameData.Buildings.BuildingData? data))
            {
                return true;
            }

            //----------------------------------------
            // CustomFields
            //----------------------------------------

            if (data.CustomFields == null)
            {
                return true;
            }

            if (!data.CustomFields.TryGetValue(
                    enabledField,
                    out string? value))
            {
                return true;
            }

            //----------------------------------------
            // true / false
            //----------------------------------------

            if (bool.TryParse(
                    value,
                    out bool enabled))
            {
                return enabled;
            }

            //----------------------------------------
            // 不正な値
            //----------------------------------------

            return true;
        }

        //----------------------------------------
        // Valley Farm Only
        //----------------------------------------

        /// <summary>
        /// 指定したBuildingTypeが
        /// バレーのメイン農場限定か判定します。
        /// </summary>
        internal static bool IsValleyFarmOnly(
            string buildingType)
        {
            return GetProviderData()
                .Values
                .Any(
                    provider =>
                        string.Equals(
                            provider.BuildingType,
                            buildingType,
                            StringComparison.Ordinal)
                        && provider.ValleyFarmOnly
                        && IsProviderEnabled(
                            provider));
        }
    }
}