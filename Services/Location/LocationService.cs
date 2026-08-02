using Microsoft.Xna.Framework;
using StardewValley;
using StardewModdingAPI;

namespace Ts_Core.Services.Location
{
    /// <summary>
    /// 現在のゲーム内位置情報や座標に関する値を取得するサービスです。
    /// </summary>
    public class LocationService
    {
        //----------------------------------------
        // 農家の入口座標
        //----------------------------------------

        /// <summary>
        /// 農家の正面入口座標を取得します。
        /// バニラと同じ取得方法を使用することで、
        /// 農家の移動やカスタムFarmマップにも対応します。
        /// </summary>
        public Point GetFarmHouseEntryPoint()
        {
            if (!Context.IsWorldReady)
                return new Point(64, 15);

            Farm farm = Game1.getFarm();

            if (farm == null)
                return new Point(64, 15);

            return farm.GetMainFarmHouseEntry();
        }

        //----------------------------------------
        // Content Patcher Token
        //----------------------------------------

        /// <summary>
        /// Content Patcher Token
        /// {{Tikamin557.TsCore/FarmHouseEntryX}}
        /// </summary>
        public IEnumerable<string> GetFarmHouseEntryX()
        {
            yield return GetFarmHouseEntryPoint().X.ToString();
        }

        /// <summary>
        /// Content Patcher token:
        /// {{Tikamin557.TsCore/FarmHouseEntryY}}
        /// </summary>
        public IEnumerable<string> GetFarmHouseEntryY()
        {
            yield return GetFarmHouseEntryPoint().Y.ToString();
        }

        /// <summary>
        /// Content Patcher Token
        /// {{Tikamin557.TsCore/FarmHouseEntry}}
        /// "X Y" 形式の文字列を返します。
        /// Warp文字列へそのまま使用できます。
        /// </summary>
        public IEnumerable<string> GetFarmHouseEntry()
        {
            Point point = GetFarmHouseEntryPoint();
            yield return $"{point.X} {point.Y}";
        }
    }
}