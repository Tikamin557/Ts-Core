using StardewModdingAPI;
using StardewModdingAPI.Utilities;

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

        //----------------------------------------
        // Time Skip
        //----------------------------------------

        /// <summary>
        /// Time Skipを実行するキーです。
        /// </summary>
        public KeybindList TimeSkipKey
        {
            get;
            set;
        } = new(
            new Keybind(
                SButton.LeftShift,
                SButton.Q));

        /// <summary>
        /// Time Skipの移動先時刻です。
        /// </summary>
        public int TimeSkipTime
        {
            get;
            set;
        } = 1000;

        /// <summary>
        /// Time Skip中のNPC Schedule処理速度です。
        /// </summary>
        public TimeSkipSpeed TimeSkipSpeed
        {
            get;
            set;
        } = TimeSkipSpeed.Normal;

        /// <summary>
        /// Time Skip完了時の通知設定です。
        /// </summary>
        public bool TimeSkipNotification
        {
            get;
            set;
        } = true;
    }
}