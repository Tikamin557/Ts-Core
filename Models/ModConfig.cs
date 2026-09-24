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
        // FarmHouse修正
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
        // ショートカットパネル
        //----------------------------------------

        /// <summary>
        /// ショートカットパネルを表示するかどうか。
        /// </summary>
        public bool ShortcutPanelEnabled
        {
            get;
            set;
        } = true;

        /// <summary>
        /// ショートカットパネルの開閉タブの
        /// 表示倍率です。
        /// 100～200で指定します。
        /// </summary>
        public int ShortcutPanelTabScale
        {
            get;
            set;
        } = 100;

        /// <summary>
        /// ショートカットパネル本体の
        /// 表示倍率です。
        /// 100～200で指定します。
        /// </summary>
        public int ShortcutPanelScale
        {
            get;
            set;
        } = 100;

        /// <summary>
        /// ショートカットパネルの開閉タブ背景の
        /// 不透明度です。
        /// 0～100で指定します。
        /// </summary>
        public int ShortcutPanelTabOpacity
        {
            get;
            set;
        } = 100;

        /// <summary>
        /// ショートカットパネル本体とスロット背景の
        /// 不透明度です。
        /// 0～100で指定します。
        /// </summary>
        public int ShortcutPanelOpacity
        {
            get;
            set;
        } = 100;

        /// <summary>
        /// 「現在の画面」のスクリーンショットに
        /// ゲームUIを含めるかどうか。
        /// </summary>
        public bool ScreenshotIncludeUi
        {
            get;
            set;
        } = true;

        /// <summary>
        /// 「現在の画面」のスクリーンショットに
        /// ショートカットパネルを含めるかどうか。
        /// </summary>
        public bool ScreenshotIncludeShortcutPanel
        {
            get;
            set;
        } = false;

        /// <summary>
        /// 「現在の画面」のスクリーンショットに
        /// マウスカーソルを含めるかどうか。
        /// </summary>
        public bool ScreenshotIncludeMouseCursor
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
        /// Time Skip (Duration)を実行するキーです。
        /// </summary>
        public KeybindList TimeSkipDurationKey
        {
            get;
            set;
        } = new(
            new Keybind(
                SButton.LeftShift,
                SButton.W));

        /// <summary>
        /// Time Skip (Duration)で進める時間です。
        /// 分単位で指定します。
        /// </summary>
        public int TimeSkipDuration
        {
            get;
            set;
        } = 60;

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
