using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Extensions;
using Ts_Core.Models;

namespace Ts_Core.Services.LightRelated
{
    /// <summary>
    /// 登録済みBuilding Light Providerの情報です。
    /// </summary>
    internal sealed class RegisteredBuildingLightProviderInfo
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
        /// 登録されているライト数です。
        /// </summary>
        public int LightCount { get; init; }

        /// <summary>
        /// 登録されているライト一覧です。
        /// </summary>
        public IReadOnlyList<BuildingLightModel> Lights { get; init; }
            = Array.Empty<BuildingLightModel>();

        /// <summary>
        /// Lightの有効・無効を制御するData/BuildingsのCustomFieldsキーです。
        /// </summary>
        public string? LightsEnabledField { get; init; }
    }

    /// <summary>
    /// Data/Buildingsの建物に追加するライトを管理します。
    /// </summary>
    public static class BuildingLightService
    {
        //----------------------------------------
        // 登録済みProvider
        //----------------------------------------

        private static readonly Dictionary<string, BuildingLightProviderModel>
            Providers =
                new(StringComparer.OrdinalIgnoreCase);

        //----------------------------------------
        // 登録済みProvider情報
        //----------------------------------------

        private static readonly Dictionary<string, RegisteredBuildingLightProviderInfo>
            RegisteredProviders =
                new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 現在登録されているBuilding Light Provider情報を取得します。
        /// </summary>
        internal static IReadOnlyList<RegisteredBuildingLightProviderInfo>
            GetRegisteredProviders()
        {
            return RegisteredProviders.Values
                .OrderBy(provider => provider.Owner)
                .ThenBy(provider => provider.Id)
                .ToList();
        }

        //----------------------------------------
        // Provider登録
        //----------------------------------------

        /// <summary>
        /// Building Light Providerを登録します。
        /// </summary>
        public static void RegisterProvider(
            BuildingLightProviderModel model,
            string owner,
            string sourceFile,
            StardewModdingAPI.IMonitor monitor)
        {
            //----------------------------------------
            // 基本チェック
            //----------------------------------------

            if (string.IsNullOrWhiteSpace(model.Id))
            {
                monitor.Log(
                    $"Building Light Provider in '{sourceFile}' has no Id.",
                    StardewModdingAPI.LogLevel.Warn);

                return;
            }

            if (string.IsNullOrWhiteSpace(model.BuildingType))
            {
                monitor.Log(
                    $"Building Light Provider '{model.Id}' has no BuildingType.",
                    StardewModdingAPI.LogLevel.Warn);

                return;
            }

            //----------------------------------------
            // 重複チェック
            //----------------------------------------

            if (RegisteredProviders.TryGetValue(
                    model.Id,
                    out RegisteredBuildingLightProviderInfo? existingProvider))
            {
                monitor.Log(
                    $"Duplicate Building Light Provider '{model.Id}' ignored.\n" +
                    $"Already registered by: {existingProvider.Owner}\n" +
                    $"Existing file: {existingProvider.SourceFile}\n" +
                    $"Ignored provider owner: {owner}\n" +
                    $"Ignored file: {sourceFile}",
                    StardewModdingAPI.LogLevel.Warn);

                return;
            }

            //----------------------------------------
            // Provider登録
            //----------------------------------------

            Providers[model.Id] =
                model;

            RegisteredProviders[model.Id] =
                new RegisteredBuildingLightProviderInfo
                {
                    Id = model.Id,
                    Owner = owner,
                    SourceFile = sourceFile,
                    BuildingType = model.BuildingType,
                    LightCount = model.Lights.Count,
                    LightsEnabledField = model.LightsEnabledField,

                    Lights = model.Lights
                        .ToList()
                };

            monitor.Log(
                $"Registered Building Light Provider '{model.Id}' from '{owner}'.",
                StardewModdingAPI.LogLevel.Trace);
        }

        //----------------------------------------
        // Provider再読み込み
        //----------------------------------------

        /// <summary>
        /// 登録済みBuilding Light Providerをすべて削除します。
        /// </summary>
        internal static void ClearProviders()
        {
            RemoveAllLights();

            Providers.Clear();
            RegisteredProviders.Clear();
        }

        //----------------------------------------
        // Light更新
        //----------------------------------------

        /// <summary>
        /// 登録済みBuilding Lightを現在の建物位置と時刻に合わせて更新します。
        /// </summary>
        public static void UpdateLights()
        {
            Farm farm =
                Game1.getFarm();

            if (farm == null)
                return;

            //----------------------------------------
            // 現在存在するLight ID
            //----------------------------------------

            HashSet<string> activeLightIds =
                new(StringComparer.OrdinalIgnoreCase);

            bool shouldLight =
                Game1.isDarkOut(farm);

            //----------------------------------------
            // 全Providerを処理
            //----------------------------------------

            foreach (BuildingLightProviderModel provider in Providers.Values)
            {
                bool providerEnabled =
                    IsProviderEnabled(
                        provider);

                List<Building> buildings =
                    farm.buildings
                        .Where(
                            building =>
                                building.buildingType.Value
                                == provider.BuildingType)
                        .ToList();

                //----------------------------------------
                // 対象建物なし
                //----------------------------------------

                if (buildings.Count == 0)
                    continue;

                //----------------------------------------
                // 同一BuildingTypeの複数建物にも対応
                //----------------------------------------

                foreach (Building building in buildings)
                {
                    foreach (BuildingLightModel light in provider.Lights)
                    {
                        string lightId =
                            GetLightId(
                                provider.Id,
                                building.id.Value,
                                light.Id);

                        //----------------------------------------
                        // 現在存在するLightとして記録
                        //----------------------------------------

                        activeLightIds.Add(
                            lightId);

                        //----------------------------------------
                        // 昼間またはProvider無効時はLight削除
                        //----------------------------------------

                        if (!shouldLight || !providerEnabled)
                        {
                            farm.removeLightSource(
                                lightId);

                            continue;
                        }

                        //----------------------------------------
                        // Light座標
                        //----------------------------------------

                        Vector2 position =
                            GetLightPosition(
                                building,
                                light);

                        //----------------------------------------
                        // 既存Lightなら座標更新
                        //----------------------------------------

                        if (farm.hasLightSource(lightId))
                        {
                            farm.repositionLightSource(
                                lightId,
                                position);

                            continue;
                        }

                        //----------------------------------------
                        // 新規Light生成
                        //----------------------------------------

                        Color color =
                            ParseColor(
                                light.Color);

                        LightSource source =
                            new(
                                lightId,
                                LightSource.sconceLight,
                                position,
                                light.Radius,
                                color,
                                LightSource.LightContext.None,
                                0L,
                                farm.NameOrUniqueName);

                        farm.sharedLights.AddLight(
                            source);
                    }
                }
            }

            //----------------------------------------
            // 不要になったLightを削除
            //----------------------------------------

            List<string> obsoleteLightIds =
                farm.sharedLights.Keys
                    .Where(id =>
                        id.StartsWith(
                            "TsCore.BuildingLight.",
                            StringComparison.OrdinalIgnoreCase)
                        && !activeLightIds.Contains(id))
                    .ToList();

            foreach (string lightId in obsoleteLightIds)
            {
                farm.removeLightSource(
                    lightId);
            }
        }

        //----------------------------------------
        // Provider有効判定
        //----------------------------------------

        /// <summary>
        /// Data/Buildings の CustomFields を確認して、
        /// Building Light Providerが有効か判定します。
        /// </summary>
        private static bool IsProviderEnabled(
            BuildingLightProviderModel provider)
        {
            //----------------------------------------
            // EnableField未指定なら常に有効
            //----------------------------------------

            if (string.IsNullOrWhiteSpace(
                    provider.LightsEnabledField))
            {
                return true;
            }

            //----------------------------------------
            // BuildingData取得
            //----------------------------------------

            var buildingData =
                DataLoader.Buildings(
                    Game1.content);

            if (!buildingData.TryGetValue(
                    provider.BuildingType,
                    out var data))
            {
                return false;
            }

            //----------------------------------------
            // CustomFields未設定なら有効
            //----------------------------------------

            if (data.CustomFields == null)
                return true;

            if (!data.CustomFields.TryGetValue(
                    provider.LightsEnabledField,
                    out string? value))
            {
                return true;
            }

            //----------------------------------------
            // true / false 判定
            //----------------------------------------

            if (bool.TryParse(
                    value,
                    out bool enabled))
            {
                return enabled;
            }

            //----------------------------------------
            // 不正な値の場合もデフォルトは有効
            //----------------------------------------

            return true;
        }

        //----------------------------------------
        // Light ID
        //----------------------------------------

        /// <summary>
        /// TsCoreが管理するLight Source IDを生成します。
        /// </summary>
        private static string GetLightId(
            string providerId,
            Guid buildingId,
            string lightId)
        {
            return
                $"TsCore.BuildingLight.{providerId}.{buildingId}.{lightId}";
        }

        //----------------------------------------
        // Light座標
        //----------------------------------------

        /// <summary>
        /// 建物左上を基準にLightのピクセル座標を計算します。
        /// </summary>
        private static Vector2 GetLightPosition(
            Building building,
            BuildingLightModel light)
        {
            float x =
                (building.tileX.Value + light.OffsetX)
                * 64f
                + 32f;

            float y =
                (building.tileY.Value + light.OffsetY)
                * 64f
                + 32f;

            return new Vector2(
                x,
                y);
        }

        //----------------------------------------
        // Color変換
        //----------------------------------------

        /// <summary>
        /// "R,G,B"形式の文字列をColorへ変換します。
        /// </summary>
        private static Color ParseColor(
            string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return Color.Black;

            string[] parts =
                value.Split(',');

            if (parts.Length != 3)
                return Color.Black;

            if (!byte.TryParse(
                    parts[0].Trim(),
                    out byte r))
            {
                return Color.Black;
            }

            if (!byte.TryParse(
                    parts[1].Trim(),
                    out byte g))
            {
                return Color.Black;
            }

            if (!byte.TryParse(
                    parts[2].Trim(),
                    out byte b))
            {
                return Color.Black;
            }

            return new Color(
                r,
                g,
                b);
        }

        //----------------------------------------
        // Light削除
        //----------------------------------------

        /// <summary>
        /// TsCoreが管理するBuilding Lightをすべて削除します。
        /// </summary>
        public static void RemoveAllLights()
        {
            Farm farm =
                Game1.getFarm();

            if (farm == null)
                return;

            List<string> lightIds =
                farm.sharedLights.Keys
                    .Where(id =>
                        id.StartsWith(
                            "TsCore.BuildingLight.",
                            StringComparison.OrdinalIgnoreCase))
                    .ToList();

            foreach (string lightId in lightIds)
            {
                farm.removeLightSource(
                    lightId);
            }
        }

        //----------------------------------------
        // Provider取得
        //----------------------------------------

        /// <summary>
        /// 指定したBuildingTypeに対応するProviderを取得します。
        /// </summary>
        internal static IReadOnlyList<BuildingLightProviderModel>
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
    }
}