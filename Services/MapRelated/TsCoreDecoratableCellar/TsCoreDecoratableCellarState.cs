using StardewValley;
using System.Text.Json;

namespace Ts_Core.Services.MapRelated.TsCoreDecoratableCellar
{
    /// <summary>
    /// Cellarごとに床材・壁紙の選択状態を保存するための補助処理です。
    /// マップの描画やアイテム消費は担当せず、選択状態の永続化だけを行います。
    /// </summary>
    internal static class TsCoreDecoratableCellarState
    {
        private const string FloorDataKey = "Tikamin557.TsCore/DecoratableCellar/Floors";
        private const string WallDataKey = "Tikamin557.TsCore/DecoratableCellar/Walls";

        //----------------------------------------
        // Persistence
        //----------------------------------------

        /// <summary>
        /// 指定したCellarに保存された領域IDと素材IDの対応を取得します。
        /// 保存値がない場合や形式が不正な場合は空の辞書を返します。
        /// </summary>
        internal static Dictionary<string, string> Read(GameLocation location, bool isFloor)
        {
            string key = isFloor ? FloorDataKey : WallDataKey;
            if (!TsCoreDecoratableCellarService.IsEnabled(location)
                || !location.modData.TryGetValue(key, out string? json)
                || string.IsNullOrWhiteSpace(json))
                return new Dictionary<string, string>(StringComparer.Ordinal);

            try
            {
                var values = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                return values == null
                    ? new Dictionary<string, string>(StringComparer.Ordinal)
                    : new Dictionary<string, string>(values, StringComparer.Ordinal);
            }
            catch (JsonException)
            {
                return new Dictionary<string, string>(StringComparer.Ordinal);
            }
        }

        /// <summary>
        /// 張り替えに成功した領域の素材IDを保存します。
        /// 描画やアイテム消費は行いません。
        /// </summary>
        internal static void Write(GameLocation location, string regionId, string materialId, bool isFloor)
        {
            if (!TsCoreDecoratableCellarService.IsEnabled(location)
                || string.IsNullOrWhiteSpace(regionId)
                || string.IsNullOrWhiteSpace(materialId))
                return;

            var values = Read(location, isFloor);
            values[regionId] = materialId;
            location.modData[isFloor ? FloorDataKey : WallDataKey] = JsonSerializer.Serialize(values);
        }
    }
}
