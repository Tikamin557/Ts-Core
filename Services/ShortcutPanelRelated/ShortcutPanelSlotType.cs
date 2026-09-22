namespace Ts_Core.Services.ShortcutPanelRelated
{
    /// <summary>
    /// Shortcut Panelのスロットに
    /// 登録されている内容の種類です。
    /// </summary>
    internal enum ShortcutPanelSlotType
    {
        /// <summary>
        /// 未登録。
        /// </summary>
        None,

        /// <summary>
        /// Mod機能。
        /// </summary>
        ModAction,

        /// <summary>
        /// キー入力。
        /// </summary>
        Keybind
    }
}