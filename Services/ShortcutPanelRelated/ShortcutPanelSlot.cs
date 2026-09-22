using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace Ts_Core.Services.ShortcutPanelRelated
{
    /// <summary>
    /// Shortcut Panelの1つのスロットを表します。
    /// </summary>
    internal sealed class ShortcutPanelSlot
    {
        //----------------------------------------
        // Type
        //----------------------------------------

        internal ShortcutPanelSlotType Type
        {
            get;
            private set;
        }

        //----------------------------------------
        // Mod Action
        //----------------------------------------

        internal string? ShortcutId
        {
            get;
            private set;
        }

        //----------------------------------------
        // Keybind
        //----------------------------------------

        internal KeybindList? Keybind
        {
            get;
            private set;
        }

        //----------------------------------------
        // Mod Action
        //----------------------------------------

        /// <summary>
        /// Mod機能をこのスロットへ登録します。
        /// </summary>
        internal void SetModAction(
            string shortcutId)
        {
            Clear();

            Type =
                ShortcutPanelSlotType.ModAction;

            ShortcutId =
                shortcutId;
        }

        /// <summary>
        /// 登録されているMod機能を取得します。
        /// </summary>
        internal ShortcutPanelEntry? GetEntry()
        {
            if (Type
                != ShortcutPanelSlotType.ModAction)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(
                ShortcutId))
            {
                return null;
            }

            return ShortcutPanelRegistry.Get(
                ShortcutId);
        }

        //----------------------------------------
        // Keybind
        //----------------------------------------

        /// <summary>
        /// キー入力をこのスロットへ登録します。
        /// </summary>
        internal void SetKeybind(
            KeybindList keybind)
        {
            Clear();

            Type =
                ShortcutPanelSlotType.Keybind;

            Keybind =
                keybind;
        }

        /// <summary>
        /// 登録されているキー入力を実行します。
        /// </summary>
        internal void ExecuteKeybind(
            IInputHelper input)
        {
            if (Type
                != ShortcutPanelSlotType.Keybind)
            {
                return;
            }

            if (Keybind == null
                || !Keybind.IsBound)
            {
                return;
            }

            Keybind? keybind =
                Keybind.Keybinds.FirstOrDefault();

            if (keybind == null)
                return;

            //----------------------------------------
            // 登録されている全キーを押す
            //----------------------------------------

            foreach (SButton button
                     in keybind.Buttons)
            {
                input.Press(
                    button);
            }
        }

        //----------------------------------------
        // State
        //----------------------------------------

        /// <summary>
        /// このスロットに何か登録されているか取得します。
        /// </summary>
        internal bool IsAssigned()
        {
            return Type
                != ShortcutPanelSlotType.None;
        }

        //----------------------------------------
        // Clear
        //----------------------------------------

        /// <summary>
        /// スロットを未登録状態へ戻します。
        /// </summary>
        internal void Clear()
        {
            Type =
                ShortcutPanelSlotType.None;

            ShortcutId =
                null;

            Keybind =
                null;
        }
    }
}