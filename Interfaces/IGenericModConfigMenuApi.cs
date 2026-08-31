using StardewModdingAPI;

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
    }
}