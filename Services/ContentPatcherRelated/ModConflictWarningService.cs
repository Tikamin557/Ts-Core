using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using Ts_Core.Models.ContentPatcherRelated;

namespace Ts_Core.Services.ContentPatcherRelated
{
    /// <summary>
    /// タイトル画面の初回表示時に、CPが登録した競合定義を評価して
    /// SMAPIログと確認式ダイアログに警告を表示します。
    /// </summary>
    internal static class ModConflictWarningService
    {
        private static IModHelper helper = null!;
        private static IMonitor monitor = null!;
        private static ITranslationHelper translation = null!;
        private static bool checkedOnce;
        private static bool dialogShown;
        private static string? pendingMessage;
        private static string? pendingTitle;
        private static int titleTicks;

        //----------------------------------------
        // 初期化
        //----------------------------------------

        internal static void Initialize(IModHelper modHelper, IMonitor modMonitor)
        {
            helper = modHelper;
            monitor = modMonitor;
            translation = modHelper.Translation;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
        }

        //----------------------------------------
        // 競合検出・警告
        //----------------------------------------

        private static void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            // CPの初期パッチ処理が終わり、タイトル画面が表示されるまで待機します。
            if (Game1.activeClickableMenu is not TitleMenu)
                return;

            // 起動直後はゲームの言語設定がSMAPIの翻訳へ反映される前の場合があるため、
            // タイトル画面が安定してから競合警告の翻訳を取得します。
            if (!checkedOnce && ++titleTicks < 90)
                return;

            if (!checkedOnce)
            {
                checkedOnce = true;
                try
                {
                    var definitions = Game1.content.Load<Dictionary<string, ModConflictWarningModel>>(
                        ModConflictWarningDataService.AssetName);
                    var messages = new List<string>();

                    foreach (var entry in definitions)
                    {
                        var ids = entry.Value?.ModIds;
                        if (ids == null)
                            continue;

                        var loaded = ids
                            .Where(id => !string.IsNullOrWhiteSpace(id))
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .Where(id => helper.ModRegistry.IsLoaded(id))
                            .ToList();

                        if (loaded.Count < 2)
                            continue;

                        var names = loaded.Select(id => helper.ModRegistry.Get(id)?.Manifest.Name ?? id).ToList();
                        string message = !string.IsNullOrWhiteSpace(entry.Value!.Message)
                            ? entry.Value.Message!
                            : translation.Get("modConflict.defaultMessage", new { mods = string.Join("\n", names) }).ToString();

                        // コンソールではMod名とUniqueIDを個別の行に表示します。
                        string logHeader = translation.Get("modConflict.logHeader").ToString();
                        string logMessage = translation.Get("modConflict.logMessage").ToString();
                        string modLines = string.Join("\n", loaded.Select(id =>
                            $"{helper.ModRegistry.Get(id)?.Manifest.Name ?? id} /ID: {id}"));
                        monitor.Log($"{logHeader} [{entry.Key}]\n{logMessage}\n{modLines}", LogLevel.Warn);
                        messages.Add(message);
                    }

                    if (messages.Count > 0)
                    {
                        pendingTitle = translation.Get("modConflict.title").ToString();
                        pendingMessage = string.Join("\n\n", messages);
                    }
                }
                catch (Exception ex)
                {
                    // 登録データに不備があってもゲーム起動を妨げません。
                    monitor.Log($"Failed to check mod conflicts: {ex}", LogLevel.Warn);
                }
            }

            if (dialogShown || pendingMessage == null || TitleMenu.subMenu != null)
                return;

            // タイトルメニューを維持しながら確認式ダイアログを表示します。
            TitleMenu.subMenu = new ModConflictWarningMenu(pendingTitle ?? "Mod Conflict", pendingMessage);
            dialogShown = true;
            helper.Events.GameLoop.UpdateTicked -= OnUpdateTicked;
        }
    }
}
