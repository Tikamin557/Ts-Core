using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace Ts_Core.Services.ShortcutPanelRelated
{
    /// <summary>
    /// ゲームメニューの各タブを独立したショートカットとして登録します。
    /// 選択用の項目と保存するIDを分離し、登録済みスロットを復元できるようにします。
    /// </summary>
    internal static class ShortcutPanelGameMenuService
    {
        private static readonly List<ShortcutPanelEntry> entries = new();

        // バニラのGameMenuタブ順。ショートカットIDは番号ではなく名前で保存します。
        private static readonly string[] tabNames =
        {
            "Inventory", "Skills", "Social", "Map", "Crafting",
            "Animals", "Powers", "Collections", "Options", "Exit"
        };

        // 各ゲームメニュータブに対応するショートカットアイコン。
        private static readonly string[] tabIconNames =
        {
            "GameMenuInventory.png", "GameMenuSkills.png", "GameMenuSocial.png", "GameMenuMap.png", "GameMenuCrafting.png",
            "GameMenuAnimals.png", "GameMenuSpecialItems.png", "GameMenuCollections.png", "GameMenuOptions.png", "GameMenuExit.png"
        };

        /// <summary>現在の言語で各タブの表示名を更新します。</summary>
        internal static void RegisterShortcuts(ITranslationHelper translation, IModHelper helper)
        {
            entries.Clear();
            for (int i = 0; i < tabNames.Length; i++)
            {
                int tabIndex = i;
                string id = "TsCore/GameMenu/" + tabNames[i];
                var entry = new ShortcutPanelEntry(
                    id,
                    translation.Get("shortcutPanel.GameMenu." + tabNames[i]),
                    () => helper.ModContent.Load<Texture2D>(
                        "assets/ShortcutPanel/" + tabIconNames[tabIndex]),
                    iconSourceRect: null,
                    () =>
                    {
                        if (Context.IsWorldReady && Game1.activeClickableMenu is null)
                        {
                            // ButtonPressedの全購読者が処理を終えてからメニューを開きます。
                            // インベントリタブの中間フレームを表示しないよう、
                            // GameMenuを公開した同じ更新内で目的のタブまで切り替えます。
                            void OpenMenu(object? tickSender, EventArgs tickArgs)
                            {
                                helper.Events.GameLoop.UpdateTicking -= OpenMenu;
                                if (Context.IsWorldReady && Game1.activeClickableMenu is null)
                                {
                                    // UI Info Suite 2 は MenuChanged で SocialPage を取得するため、
                                    // ソーシャルだけはその通知後にタブを切り替えます。
                                    GameMenu menu = new GameMenu(0);
                                    if (tabIndex == 2)
                                    {
                                        void OnMenuChanged(object? sender, MenuChangedEventArgs e)
                                        {
                                            if (!ReferenceEquals(e.NewMenu, menu))
                                                return;
                                            helper.Events.Display.MenuChanged -= OnMenuChanged;
                                            void OpenSocial(object? tickSender2, EventArgs tickArgs2)
                                            {
                                                helper.Events.GameLoop.UpdateTicking -= OpenSocial;
                                                if (ReferenceEquals(Game1.activeClickableMenu, menu))
                                                    menu.changeTab(2);
                                            }
                                            helper.Events.GameLoop.UpdateTicking += OpenSocial;
                                        }
                                        helper.Events.Display.MenuChanged += OnMenuChanged;
                                    }
                                    Game1.activeClickableMenu = menu;
                                    if (tabIndex != 0 && tabIndex != 2 && ReferenceEquals(Game1.activeClickableMenu, menu))
                                        menu.changeTab(tabIndex);
                                }
                            }
                            helper.Events.GameLoop.UpdateTicking += OpenMenu;
                        }
                    });
                entries.Add(entry);
                ShortcutPanelRegistry.Register(entry);
            }
        }

        /// <summary>登録時に表示するタブ候補を取得します。</summary>
        internal static IReadOnlyList<ShortcutPanelEntry> GetEntries() => entries;
    }
}
