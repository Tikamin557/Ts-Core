using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System.Reflection;

namespace Ts_Core.Services.TerrainFeaturesRelated
{
    /// <summary>
    /// GMCMのキー設定とショートカットパネルから同じ管理メニューを開きます。
    /// </summary>
    internal static class TerrainFeaturesMenuService
    {
        private static IModHelper helper = null!;
        private static IMonitor monitor = null!;
        private static bool originalModPatchInstalled;

        /// <summary>
        /// 元Modの入力ハンドラーだけを抑制します。キーが一致しない場合は元の処理を維持します。
        /// </summary>
        private static bool OriginalButtonPressedPrefix(object __instance, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady || Game1.activeClickableMenu != null)
                return true;

            if (!ModEntry.Config.TerrainFeaturesMenuKey.JustPressed())
                return true;

            // 元Modが実際にこのキーを使用している場合に限り抑制します。
            object? config = AccessTools.Field(__instance.GetType(), "Config")?.GetValue(__instance);
            if (config == null)
                return true;

            object? menuKey = AccessTools.Property(config.GetType(), "MenuKey")?.GetValue(config);
            return menuKey is not SButton originalKey || originalKey != e.Button;
        }

        /// <summary>
        /// 元Modが存在するときだけ、そのButtonPressedに限定して互換パッチを適用します。
        /// </summary>
        internal static void ApplyOriginalModCompatibility(Harmony harmony, IModRegistry registry)
        {
            if (originalModPatchInstalled || !registry.IsLoaded("MabelSyrup.ResetTerrainFeatures"))
                return;

            try
            {
                Type? modType = AccessTools.TypeByName("ResetTerrainFeatures.ModEntry");
                MethodInfo? target = modType == null ? null : AccessTools.Method(
                    modType, "ButtonPressed", new[] { typeof(object), typeof(ButtonPressedEventArgs) });
                if (target == null)
                {
                    monitor.Log("Reset Terrain Features input handler was not found; compatibility patch skipped.", LogLevel.Warn);
                    return;
                }

                harmony.Patch(target, prefix: new HarmonyMethod(
                    typeof(TerrainFeaturesMenuService), nameof(OriginalButtonPressedPrefix)));
                originalModPatchInstalled = true;
            }
            catch (Exception ex)
            {
                monitor.Log($"Reset Terrain Features input compatibility patch failed: {ex}", LogLevel.Warn);
            }
        }


        internal static void Initialize(IModHelper modHelper, IMonitor modMonitor)
        {
            helper = modHelper;
            monitor = modMonitor;
            helper.Events.Input.ButtonPressed += OnButtonPressed;
        }

        private static void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady || Game1.activeClickableMenu != null)
                return;

            var keybind = ModEntry.Config.TerrainFeaturesMenuKey;
            if (!keybind.JustPressed())
                return;

            helper.Input.SuppressActiveKeybinds(keybind);
            TryOpen();
        }

        internal static void TryOpen()
        {
            if (!Context.IsWorldReady || Game1.activeClickableMenu != null)
                return;

            Game1.activeClickableMenu = new TerrainFeaturesMenu(monitor, helper.Translation, helper);
        }
    }
}
