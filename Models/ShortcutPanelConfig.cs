namespace Ts_Core.Models
{
    /// <summary>
    /// ショートカットパネルの登録内容を保存する専用設定です。
    /// GMCMのDefault操作で初期化される通常のconfig.jsonとは分離します。
    /// </summary>
    public sealed class ShortcutPanelConfig
    {
        public List<ShortcutPanelSlotConfig> Slots
        {
            get;
            set;
        } = new();
    }
}
