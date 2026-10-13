using StardewValley.Menus;
using System.Reflection;

namespace Ts_Core.Services.ContentPatcherRelated
{
    /// <summary>
    /// GMCMの通常の保存処理を使用し、Content Packの未保存設定を確定します。
    /// GMCMの非公開実装に依存するため、互換性がない場合は画面遷移を中止します。
    /// </summary>
    internal static class GmcmMenuSaveService
    {
        internal static bool TrySave(IClickableMenu? menu, StardewModdingAPI.IMonitor monitor)
        {
            // GMCMの一覧画面から開いた場合、設定画面は子メニューになります。
            // 親メニューを保持したまま、保存対象だけ実際の設定画面へ進めます。
            IClickableMenu? settingsMenu = menu;
            while (settingsMenu != null && settingsMenu.GetChildMenu() != null)
                settingsMenu = settingsMenu.GetChildMenu();

            if (settingsMenu?.GetType().FullName !=
                "GenericModConfigMenu.Framework.SpecificModConfigMenu")
                return false;

            try
            {
                MethodInfo? save = settingsMenu.GetType().GetMethod("SaveConfig",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                    null, new[] { typeof(bool) }, null);
                if (save == null)
                {
                    monitor.Log("GMCM SaveConfig(bool) was not found; keeping the settings menu open.",
                        StardewModdingAPI.LogLevel.Warn);
                    return false;
                }

                save.Invoke(settingsMenu, new object[] { false });
                return true;
            }
            catch (Exception ex)
            {
                monitor.Log($"Could not save GMCM settings; keeping the menu open.\n{ex}",
                    StardewModdingAPI.LogLevel.Warn);
                return false;
            }
        }
    }
}
