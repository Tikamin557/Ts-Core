using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Extensions;
using Ts_Core.Models;

namespace Ts_Core.Services.BuildingRelated
{

    /// <summary>
    /// Data/Buildingsの建物に追加するライトを管理します。
    /// </summary>
    public static class BuildingLightService
    {
        //----------------------------------------
        // Light更新
        //----------------------------------------

        /// <summary>
        /// 登録済みBuilding Lightを現在の建物位置と時刻に合わせて更新します。
        /// </summary>
        public static void UpdateLights()
        {
            if (!StardewModdingAPI.Context.IsWorldReady)
                return;

            Farm farm =
                Game1.getFarm();

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

            foreach (BuildingProviderModel provider
                     in BuildingProviderService.GetProviders())
            {
                bool buildingProviderEnabled =
                    BuildingProviderService.IsProviderEnabled(
                        provider);

                bool lightsEnabled =
                    BuildingProviderService.IsEnabledField(
                        provider,
                        provider.LightsEnabledField);

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
                        // 昼間またはBuilding / Light無効時はLight削除
                        //----------------------------------------

                        if (!buildingProviderEnabled
                            || !shouldLight
                            || !lightsEnabled)
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
            if (!StardewModdingAPI.Context.IsWorldReady)
                return;

            Farm farm =
                Game1.getFarm();

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
    }
}