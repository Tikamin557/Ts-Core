namespace Ts_Core.Models
{
    /// <summary>
    /// T's Coreの設定です。
    /// </summary>
    public class ModConfig
    {
        //----------------------------------------
        // FarmHouse Fix
        //----------------------------------------

        /// <summary>
        /// FarmHouseの配偶者部屋付近で
        /// カスタムタイルが上書きされる問題を修正します。
        /// </summary>
        public bool EnableSpouseRoomTileFix
        {
            get;
            set;
        } = false;
    }
}