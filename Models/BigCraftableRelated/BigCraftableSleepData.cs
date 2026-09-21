using Microsoft.Xna.Framework;

namespace Ts_Core.Models.BigCraftableRelated
{
    /// <summary>
    /// TsCore_Sleepで使用した
    /// 継続寝床の情報です。
    /// </summary>
    public sealed class BigCraftableSleepData
    {
        //----------------------------------------
        // Location
        //----------------------------------------

        /// <summary>
        /// 寝床があるLocation名です。
        /// </summary>
        public string LocationName
        {
            get;
            set;
        } = string.Empty;

        //----------------------------------------
        // Wake Up Point
        //----------------------------------------

        /// <summary>
        /// 起床するTile位置です。
        /// </summary>
        public Point WakeUpPoint
        {
            get;
            set;
        }

        //----------------------------------------
        // Anchor
        //----------------------------------------

        /// <summary>
        /// 寝床として使用したBigCraftableの
        /// Anchor Tileです。
        /// </summary>
        public Vector2 Anchor
        {
            get;
            set;
        }

        //----------------------------------------
        // Qualified Item ID
        //----------------------------------------

        /// <summary>
        /// 寝床として使用したBigCraftableの
        /// Qualified Item IDです。
        /// </summary>
        public string QualifiedItemId
        {
            get;
            set;
        } = string.Empty;
    }
}