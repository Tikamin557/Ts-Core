using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using Ts_Core.Models;

namespace Ts_Core.Services
{
    /// <summary>
    /// メニューでカーソルに持っているアイテムを
    /// バニラのゴミ箱と同じ処理で処分します。
    /// </summary>
    internal static class QuickTrashService
    {
        /// <summary>
        /// キー入力を登録します。
        /// </summary>
        internal static void Initialize(IModHelper helper, Func<ModConfig> getConfig)
        {
            helper.Events.Input.ButtonPressed += (sender, e) =>
            {
                if (!Context.IsWorldReady || Game1.activeClickableMenu == null
                    || getConfig().QuickTrashKey == null
                    || !getConfig().QuickTrashKey.JustPressed())
                    return;

                // 通常のインベントリ画面で保持しているアイテムだけを対象にします。
                if (Game1.activeClickableMenu is not GameMenu gameMenu
                    || gameMenu.GetCurrentPage() is not InventoryPage inventoryPage)
                    return;

                // InventoryPageの保持アイテムは非公開メソッドで取得・設定します。
                var takeMethod = AccessTools.Method(typeof(InventoryPage), "takeHeldItem");
                var setMethod = AccessTools.Method(typeof(InventoryPage), "setHeldItem");
                if (takeMethod == null || setMethod == null)
                    return;

                Item? item = takeMethod.Invoke(inventoryPage, null) as Item;
                if (item == null)
                    return;

                if (!item.canBeTrashed())
                {
                    setMethod.Invoke(inventoryPage, new object?[] { item });
                    return;
                }

                Utility.trashItem(item);
                setMethod.Invoke(inventoryPage, new object?[] { null });
                Game1.playSound("trashcan");
                helper.Input.SuppressActiveKeybinds(getConfig().QuickTrashKey);
            };
        }
    }
}
