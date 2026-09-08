using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace Ts_Core.Interfaces
{
    /// <summary>
    /// Generic Mod Config Menu APIです。
    /// </summary>
    public interface IGenericModConfigMenuApi
    {
        /// <summary>
        /// Modの設定メニューを登録します。
        /// </summary>
        void Register(
            IManifest mod,
            Action reset,
            Action save,
            bool titleScreenOnly = false);

        /// <summary>
        /// bool設定項目を追加します。
        /// </summary>
        void AddBoolOption(
            IManifest mod,
            Func<bool> getValue,
            Action<bool> setValue,
            Func<string> name,
            Func<string>? tooltip = null,
            string? fieldId = null);

        /// <summary>
        /// キーバインド設定項目を追加します。
        /// </summary>
        void AddKeybindList(
            IManifest mod,
            Func<KeybindList> getValue,
            Action<KeybindList> setValue,
            Func<string> name,
            Func<string>? tooltip = null,
            string? fieldId = null);

        /// <summary>
        /// 文字列設定項目を追加します。
        /// </summary>
        void AddTextOption(
            IManifest mod,
            Func<string> getValue,
            Action<string> setValue,
            Func<string> name,
            Func<string>? tooltip = null,
            string[]? allowedValues = null,
            Func<string, string>? formatAllowedValue = null,
            string? fieldId = null);

        /// <summary>
        /// セクションタイトルを追加します。
        /// </summary>
        void AddSectionTitle(
            IManifest mod,
            Func<string> text,
            Func<string>? tooltip = null);

        /// <summary>
        /// 数値設定項目を追加します。
        /// </summary>
        void AddNumberOption(
            IManifest mod,
            Func<int> getValue,
            Action<int> setValue,
            Func<string> name,
            Func<string>? tooltip = null,
            int? min = null,
            int? max = null,
            int? interval = null,
            Func<int, string>? formatValue = null,
            string? fieldId = null);
    }
}