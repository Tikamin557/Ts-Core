using StardewModdingAPI;
using StardewValley;
using Ts_Core.Models.DialogueRelated;
using Ts_Core.Services.DialogueRelated;

namespace Ts_Core.Debug
{
    /// <summary>
    /// Dialogue関連のデバッグ処理を行います。
    /// </summary>
    internal static class DebugDialogueLogger
    {
        //----------------------------------------
        // Dialogue
        //----------------------------------------

        /// <summary>
        /// Dialogue一覧の表示、
        /// または指定Dialogueの表示テストを行います。
        /// </summary>
        public static void Handle(
            IMonitor monitor,
            string[] args)
        {
            if (!Context.IsWorldReady)
            {
                monitor.Log(
                    "A save must be loaded before using this command.",
                    LogLevel.Warn);

                return;
            }

            //----------------------------------------
            // Dialogue一覧
            //----------------------------------------

            if (args.Length == 0)
            {
                LogDialogues(
                    monitor);

                return;
            }

            //----------------------------------------
            // Dialogue表示
            //----------------------------------------

            ShowDialogue(
                monitor,
                args[0]);
        }

        //----------------------------------------
        // Dialogue一覧
        //----------------------------------------

        /// <summary>
        /// 現在登録されているDialogue IDを表示します。
        /// </summary>
        private static void LogDialogues(
            IMonitor monitor)
        {
            Dictionary<string, DialogueModel> dialogues =
                Game1.content.Load<
                    Dictionary<string, DialogueModel>>(
                        DialogueDataService.AssetName);

            monitor.Log(
                "===== Dialogues =====",
                LogLevel.Info);

            DebugLogHelper.LogBlankLine(
                monitor);

            //----------------------------------------
            // Dialogueなし
            //----------------------------------------

            if (dialogues.Count == 0)
            {
                monitor.Log(
                    "(none)",
                    LogLevel.Info);

                return;
            }

            //----------------------------------------
            // Dialogue表示
            //----------------------------------------

            foreach (
                string dialogueId in dialogues.Keys
                    .OrderBy(id => id))
            {
                monitor.Log(
                    dialogueId,
                    LogLevel.Info);
            }
        }

        //----------------------------------------
        // Dialogue表示テスト
        //----------------------------------------

        /// <summary>
        /// 指定したDialogueを表示します。
        /// </summary>
        private static void ShowDialogue(
            IMonitor monitor,
            string dialogueId)
        {
            if (string.IsNullOrWhiteSpace(
                    dialogueId))
            {
                monitor.Log(
                    "Dialogue ID is required.",
                    LogLevel.Warn);

                return;
            }

            bool success =
                DialogueService.Show(
                    dialogueId,
                    Game1.currentLocation,
                    Game1.player);

            if (!success)
            {
                monitor.Log(
                    $"Failed to show Dialogue '{dialogueId}'.",
                    LogLevel.Warn);
            }
        }
    }
}