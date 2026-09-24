using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace Ts_Core.Services.ShortcutPanelRelated
{
    /// <summary>
    /// Shortcut Panelの1つのスロットを表します。
    /// </summary>
    internal sealed class ShortcutPanelSlot
    {
        internal ShortcutPanelSlotType Type { get; private set; }

        internal string? ShortcutId { get; private set; }

        internal KeybindList? Keybind { get; private set; }

        /// <summary>
        /// GMCM個別設定の対象Mod UniqueIDです。
        /// </summary>
        internal string? GmcmModId { get; private set; }

        //----------------------------------------
        // Mod Action
        //----------------------------------------

        /// <summary>
        /// このスロットへMod機能ショートカットを登録します。
        /// </summary>
        internal void SetModAction(string shortcutId)
        {
            Clear();
            Type = ShortcutPanelSlotType.ModAction;
            ShortcutId = shortcutId;
        }

        /// <summary>
        /// このスロットに登録されているMod機能の実体をレジストリから取得します。
        /// </summary>
        internal ShortcutPanelEntry? GetEntry()
        {
            if (Type != ShortcutPanelSlotType.ModAction
                || string.IsNullOrWhiteSpace(ShortcutId))
            {
                return null;
            }

            return ShortcutPanelRegistry.Get(ShortcutId);
        }

        //----------------------------------------
        // Keybind
        //----------------------------------------

        /// <summary>
        /// このスロットへキー入力ショートカットを登録します。
        /// </summary>
        internal void SetKeybind(KeybindList keybind)
        {
            Clear();
            Type = ShortcutPanelSlotType.Keybind;
            Keybind = keybind;
        }

        /// <summary>
        /// 登録されたキー入力を実行します。AndroidではSMAPIの違いに対応するためReflectionによる代替処理も使用します。
        /// </summary>
        internal void ExecuteKeybind(IInputHelper input)
        {
            if (Type != ShortcutPanelSlotType.Keybind
                || Keybind == null
                || !Keybind.IsBound)
            {
                return;
            }

            Keybind? keybind = Keybind.Keybinds.FirstOrDefault();
            if (keybind == null)
                return;

            // Android版SMAPIには IInputHelper.Press が存在しない版があります。
            // Pressを直接参照すると、Android側の互換性チェックで
            // TsCore.dll自体がロード拒否されるため、実行時にReflectionで取得します。
            // PC版SMAPIでは従来どおりPressを呼び出し、存在しない環境では
            // Keybindショートカットの実行だけを何もせず終了します。
            var pressMethod = input.GetType().GetMethod(
                "Press",
                new[] { typeof(SButton) });

            if (pressMethod != null)
            {
                foreach (SButton button in keybind.Buttons)
                {
                    pressMethod.Invoke(
                        input,
                        new object[] { button });
                }

                return;
            }

            // Android版SMAPI 4.3.2系では公開IInputHelper.Pressがありません。
            // ただし内部InputHelperは CurrentInputState -> SInputState を保持しており、
            // SInputState.OverrideButton(button, true) で次の入力更新に
            // 疑似的なPressed状態を追加できます。
            // Android専用APIを直接参照するとPC版でビルドできないため、
            // ここもReflectionだけで呼び出します。
            try
            {
                var currentInputStateField =
                    input.GetType().GetField(
                        "CurrentInputState",
                        System.Reflection.BindingFlags.Instance
                        | System.Reflection.BindingFlags.NonPublic);

                if (currentInputStateField?.GetValue(input)
                    is not Delegate currentInputState)
                {
                    return;
                }

                object? inputState =
                    currentInputState.DynamicInvoke();

                if (inputState == null)
                    return;

                var overrideButtonMethod =
                    inputState.GetType().GetMethod(
                        "OverrideButton",
                        System.Reflection.BindingFlags.Instance
                        | System.Reflection.BindingFlags.Public
                        | System.Reflection.BindingFlags.NonPublic,
                        binder: null,
                        types: new[] { typeof(SButton), typeof(bool) },
                        modifiers: null);

                if (overrideButtonMethod == null)
                    return;

                foreach (SButton button in keybind.Buttons)
                {
                    overrideButtonMethod.Invoke(
                        inputState,
                        new object[] { button, true });
                }
            }
            catch
            {
                // 内部実装が異なる環境ではキー実行だけを行いません。
                // TsCore本体や他のショートカット機能には影響させません。
            }
        }

        //----------------------------------------
        // GMCM
        //----------------------------------------

        /// <summary>
        /// このスロットへ指定ModのGMCM設定画面ショートカットを登録します。
        /// </summary>
        internal void SetGmcm(string modId)
        {
            Clear();
            Type = ShortcutPanelSlotType.Gmcm;
            GmcmModId = modId;
        }

        //----------------------------------------
        // State
        //----------------------------------------

        /// <summary>
        /// このスロットに何らかのショートカットが登録済みかを判定します。
        /// </summary>
        internal bool IsAssigned()
        {
            return Type != ShortcutPanelSlotType.None;
        }

        /// <summary>
        /// このスロットの登録内容をすべて解除し、未登録状態へ戻します。
        /// </summary>
        internal void Clear()
        {
            Type = ShortcutPanelSlotType.None;
            ShortcutId = null;
            Keybind = null;
            GmcmModId = null;
        }
    }
}
