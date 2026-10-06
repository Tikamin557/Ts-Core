using StardewValley;

namespace Ts_Core.Services.MapRelated.TsCoreGreenhouse
{
    /// <summary>
    /// TsCoreGreenhouse Map Propertyの判定と
    /// 果樹を植え付け可能なTile Typeの管理を行います。
    /// </summary>
    internal static class TsCoreGreenhouseService
    {
        private const string MapPropertyName = "TsCoreGreenhouse";

        //----------------------------------------
        // Map Property
        //----------------------------------------

        /// <summary>
        /// 指定LocationにTsCoreGreenhouse Map Propertyが
        /// 設定されているか確認します。
        /// </summary>
        internal static bool IsTsCoreGreenhouse(
            GameLocation? location)
        {
            return location?.Map?.Properties.ContainsKey(
                MapPropertyName) == true;
        }

        /// <summary>
        /// 指定TileのBack Layer Typeが
        /// TsCoreGreenhouseで果樹植え付けを許可されているか確認します。
        /// </summary>
        internal static bool IsFruitTreeTypeAllowed(
            GameLocation location,
            int tileX,
            int tileY)
        {
            if (!TryGetAllowedFruitTreeTypes(
                location,
                out HashSet<string> allowedTypes))
            {
                return false;
            }

            string? tileType =
                location.doesTileHaveProperty(
                    tileX,
                    tileY,
                    "Type",
                    "Back");

            return tileType != null
                && allowedTypes.Contains(
                    tileType);
        }

        /// <summary>
        /// TsCoreGreenhouse Map Propertyから
        /// 果樹を植え付け可能なTile Type一覧を取得します。
        /// </summary>
        private static bool TryGetAllowedFruitTreeTypes(
            GameLocation location,
            out HashSet<string> allowedTypes)
        {
            allowedTypes =
                new HashSet<string>(
                    StringComparer.Ordinal);

            if (location.Map == null
                || !location.Map.Properties.TryGetValue(
                    MapPropertyName,
                    out var propertyValue))
            {
                return false;
            }

            string value =
                propertyValue?.ToString()
                ?? string.Empty;

            foreach (string type in value.Split(','))
            {
                string trimmedType =
                    type.Trim();

                if (!string.IsNullOrEmpty(
                    trimmedType))
                {
                    allowedTypes.Add(
                        trimmedType);
                }
            }

            return true;
        }
    }
}
