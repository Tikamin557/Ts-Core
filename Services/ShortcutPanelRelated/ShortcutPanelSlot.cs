namespace Ts_Core.Services.ShortcutPanelRelated
{
    /// <summary>
    /// Shortcut Panelの
    /// 1つのスロットを表します。
    /// </summary>
    internal sealed class ShortcutPanelSlot
    {
        //----------------------------------------
        // Shortcut ID
        //----------------------------------------

        /// <summary>
        /// このスロットに割り当てられている
        /// ショートカットのID。
        ///
        /// nullの場合は空スロットです。
        /// </summary>
        internal string? ShortcutId { get; set; }

        //----------------------------------------
        // Entry
        //----------------------------------------

        /// <summary>
        /// このスロットに割り当てられている
        /// Shortcut Entryを取得します。
        /// </summary>
        internal ShortcutPanelEntry? GetEntry()
        {
            if (string.IsNullOrWhiteSpace(
                ShortcutId))
            {
                return null;
            }

            return ShortcutPanelRegistry.Get(
                ShortcutId);
        }

        //----------------------------------------
        // Clear
        //----------------------------------------

        /// <summary>
        /// スロットの割り当てを解除します。
        /// </summary>
        internal void Clear()
        {
            ShortcutId =
                null;
        }
    }
}