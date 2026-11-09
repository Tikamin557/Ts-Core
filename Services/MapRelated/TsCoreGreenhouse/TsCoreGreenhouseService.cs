using StardewValley;
using StardewValley.TerrainFeatures;

namespace Ts_Core.Services.MapRelated.TsCoreGreenhouse
{
    /// <summary>
    /// TsCoreGreenhouse Map Propertyの判定と
    /// 果樹を植え付け可能なTile Typeの管理を行います。
    /// </summary>
    internal static class TsCoreGreenhouseService
    {
        private const string MapPropertyName = "TsCoreGreenhouse";
        private const string NoFruitTreePropertyName = "TsCoreNoFruitTree";

        private const string FruitTreeGroundPropertyName = "TsCoreFruitTreeGround";

        //----------------------------------------
        // Fruit Tree Ground
        //----------------------------------------

        /// <summary>
        /// 果樹の根元の地面描画をマッププロパティで制御します。
        /// 未設定ならVanillaの判定を維持し、設定時は指定したBack Typeのみ表示します。
        /// NoneはType未設定を表し、空文字はすべて非表示にします。
        /// </summary>
        internal static bool ShouldDrawFruitTreeGround(
            FruitTree tree,
            bool vanillaResult)
        {
            GameLocation? location = tree.Location;
            if (location?.Map == null
                || !location.Map.Properties.TryGetValue(
                    FruitTreeGroundPropertyName,
                    out var propertyValue))
            {
                return vanillaResult;
            }

            string? tileType = location.doesTileHaveProperty(
                (int)tree.Tile.X,
                (int)tree.Tile.Y,
                "Type",
                "Back");

            string targetType = tileType ?? "None";
            string value = propertyValue?.ToString() ?? string.Empty;

            foreach (string type in value.Split(','))
            {
                if (string.Equals(
                    type.Trim(),
                    targetType,
                    StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

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

            // マッププロパティで指定された長方形の範囲内は、
            // Typeの許可設定よりも優先して植樹を禁止します。
            if (IsInNoFruitTreeArea(location, tileX, tileY))
                return false;

            string? tileType =
                location.doesTileHaveProperty(
                    tileX,
                    tileY,
                    "Type",
                    "Back");

            // None はBackレイヤーのTypeが未設定のタイルを表します。
            // 既存のType指定による判定はそのまま維持します。
            if (tileType == null)
                return allowedTypes.Contains("None");

            return allowedTypes.Contains(
                tileType);
        }

        /// <summary>
        /// TsCoreNoFruitTree Map Propertyの各範囲に
        /// 指定タイルが含まれているか確認します。
        /// 書式: 開始X 開始Y 終了X 終了Y, ...（両端を含む）。
        /// </summary>
        private static bool IsInNoFruitTreeArea(
            GameLocation location,
            int tileX,
            int tileY)
        {
            if (location.Map == null
                || !location.Map.Properties.TryGetValue(
                    NoFruitTreePropertyName,
                    out var propertyValue))
            {
                return false;
            }

            string value = propertyValue?.ToString() ?? string.Empty;
            foreach (string area in value.Split(','))
            {
                string[] coordinates = area.Split(
                    (char[]?)null,
                    StringSplitOptions.RemoveEmptyEntries);

                if (coordinates.Length != 4
                    || !int.TryParse(coordinates[0], out int startX)
                    || !int.TryParse(coordinates[1], out int startY)
                    || !int.TryParse(coordinates[2], out int endX)
                    || !int.TryParse(coordinates[3], out int endY))
                {
                    continue;
                }

                if (tileX >= Math.Min(startX, endX)
                    && tileX <= Math.Max(startX, endX)
                    && tileY >= Math.Min(startY, endY)
                    && tileY <= Math.Max(startY, endY))
                {
                    return true;
                }
            }

            return false;
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
