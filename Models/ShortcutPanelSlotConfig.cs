using StardewModdingAPI.Utilities;

namespace Ts_Core.Models
{
    /// <summary>
    /// ショートカットパネルの
    /// スロット1個分の保存データです。
    /// </summary>
    public sealed class ShortcutPanelSlotConfig
    {
        /// <summary>
        /// 登録タイプです。
        /// None / ModAction / Keybind
        /// </summary>
        public string Type
        {
            get;
            set;
        } = "None";

        /// <summary>
        /// Mod機能のShortcut IDです。
        /// </summary>
        public string? ShortcutId
        {
            get;
            set;
        }

        /// <summary>
        /// 登録されたキーです。
        /// </summary>
        public KeybindList? Keybind
        {
            get;
            set;
        }
    }
}