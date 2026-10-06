using StardewValley;

namespace Ts_Core.Services.MapRelated.TsCoreAmbientLight
{
    /// <summary>
    /// TsCoreAmbientLight Map Propertyの設定を判定します。
    /// </summary>
    internal static class TsCoreAmbientLightService
    {
        private const string MapPropertyName = "TsCoreAmbientLight";
        private const string FarmhouseMode = "Farmhouse";

        //----------------------------------------
        // Map Property
        //----------------------------------------

        /// <summary>
        /// 指定LocationがFarmHouse互換のAmbient Light設定か確認します。
        /// </summary>
        internal static bool IsFarmhouseMode(
            GameLocation? location)
        {
            if (location?.Map == null
                || !location.Map.Properties.TryGetValue(
                    MapPropertyName,
                    out var propertyValue))
            {
                return false;
            }

            string value =
                propertyValue?.ToString()?.Trim()
                ?? string.Empty;

            return string.Equals(
                value,
                FarmhouseMode,
                StringComparison.OrdinalIgnoreCase);
        }
    }
}
