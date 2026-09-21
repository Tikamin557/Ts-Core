using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Triggers;
using Ts_Core.Models.DialogueRelated;

namespace Ts_Core.Services.DialogueRelated
{
    /// <summary>
    /// TsCore Dialogueの表示・選択肢・
    /// Action実行を管理するサービスです。
    /// </summary>
    public static class DialogueService
    {
        private static IMonitor? Monitor;

        //----------------------------------------
        // 初期化
        //----------------------------------------

        /// <summary>
        /// Dialogue Serviceを初期化します。
        /// </summary>
        public static void Initialize(
            IMonitor monitor)
        {
            Monitor = monitor;
        }

        //----------------------------------------
        // Dialogue開始
        //----------------------------------------

        /// <summary>
        /// 指定したIDのDialogueを開始します。
        /// </summary>
        /// <param name="dialogueId">
        /// TsCore/Dialoguesに登録されたDialogue IDです。
        /// </param>
        /// <param name="location">
        /// Dialogueを開始したLocationです。
        /// </param>
        /// <param name="player">
        /// Dialogueを開始したプレイヤーです。
        /// </param>
        /// <returns>
        /// Dialogueを開始できた場合はtrueを返します。
        /// </returns>
        public static bool Show(
            string dialogueId,
            GameLocation location,
            Farmer player)
        {
            if (string.IsNullOrWhiteSpace(
                    dialogueId))
            {
                return false;
            }

            //----------------------------------------
            // Dialogue Data取得
            //----------------------------------------

            Dictionary<string, DialogueModel>
                dialogues =
                    Game1.content.Load<
                        Dictionary<
                            string,
                            DialogueModel>>(
                                DialogueDataService.AssetName);

            if (!dialogues.TryGetValue(
                    dialogueId,
                    out DialogueModel? dialogue))
            {
                Monitor?.Log(
                    $"Dialogue ID '{dialogueId}' was not found in "
                    + $"{DialogueDataService.AssetName}.",
                    LogLevel.Warn);

                return false;
            }

            //----------------------------------------
            // Dialogue表示
            //----------------------------------------

            ShowDialogue(
                dialogueId,
                dialogue,
                location,
                player);

            return true;


        }

        //----------------------------------------
        // Dialogue表示
        //----------------------------------------

        /// <summary>
        /// Dialogueの条件を確認して表示します。
        /// </summary>
        private static void ShowDialogue(
            string dialogueId,
            DialogueModel dialogue,
            GameLocation location,
            Farmer player)
        {
            //----------------------------------------
            // Condition
            //----------------------------------------

            if (!CheckCondition(
                    dialogue.Condition,
                    location,
                    player))
            {
                //----------------------------------------
                // Fail Audio Cue
                //----------------------------------------

                if (!string.IsNullOrWhiteSpace(
                        dialogue.FailAudioCue))
                {
                    Game1.playSound(
                        dialogue.FailAudioCue);
                }

                ShowText(
                    dialogue.FailText);

                return;
            }

            //----------------------------------------
            // Audio Cue
            //----------------------------------------

            if (!string.IsNullOrWhiteSpace(
                    dialogue.AudioCue))
            {
                Game1.playSound(
                    dialogue.AudioCue);
            }

            //----------------------------------------
            // Dialogue終了後Action
            //----------------------------------------

            RegisterAfterActions(
                dialogueId,
                dialogue.AfterActions);

            //----------------------------------------
            // 選択肢なし
            //----------------------------------------

            if (dialogue.Responses.Count == 0)
            {
                ShowText(
                    dialogue.Text,
                    dialogue.Duration);

                return;
            }

            //----------------------------------------
            // 選択肢作成
            //----------------------------------------

            Response[] responses =
                new Response[
                    dialogue.Responses.Count];

            for (int i = 0;
                i < dialogue.Responses.Count;
                i++)
            {
                DialogueResponseModel response =
                    dialogue.Responses[i];

                responses[i] =
                    new Response(
                        i.ToString(),
                        response.Text);
            }

            //----------------------------------------
            // Question Dialogue表示
            //----------------------------------------

            GameLocation.afterQuestionBehavior
                callback =
                    (Farmer who, string answer) =>
                    {
                        HandleResponse(
                            dialogueId,
                            dialogue,
                            answer,
                            location,
                            who);
                    };

            location.createQuestionDialogue(
                dialogue.Text,
                responses,
                callback);
        }

        //----------------------------------------
        // Response処理
        //----------------------------------------

        /// <summary>
        /// 選択されたResponseを処理します。
        /// </summary>
        private static void HandleResponse(
            string dialogueId,
            DialogueModel dialogue,
            string answer,
            GameLocation location,
            Farmer player)
        {
            //----------------------------------------
            // Response Index取得
            //----------------------------------------

            if (!int.TryParse(
                    answer,
                    out int responseIndex))
            {
                Monitor?.Log(
                    $"Dialogue '{dialogueId}' returned an invalid "
                    + $"response key '{answer}'.",
                    LogLevel.Warn);

                return;
            }

            if (responseIndex < 0
                || responseIndex
                    >= dialogue.Responses.Count)
            {
                Monitor?.Log(
                    $"Dialogue '{dialogueId}' returned an invalid "
                    + $"response index '{responseIndex}'.",
                    LogLevel.Warn);

                return;
            }

            DialogueResponseModel response =
                dialogue.Responses[
                    responseIndex];

            //----------------------------------------
            // Condition
            //----------------------------------------

            if (!CheckCondition(
                    response.Condition,
                    location,
                    player))
            {
                //----------------------------------------
                // Fail Audio Cue
                //----------------------------------------

                if (!string.IsNullOrWhiteSpace(
                        response.FailAudioCue))
                {
                    Game1.playSound(
                        response.FailAudioCue);
                }

                ShowText(
                    response.FailText);

                return;
            }

            //----------------------------------------
            // Actions
            //----------------------------------------

            if (!RunActions(
                    dialogueId,
                    response.Actions))
            {
                return;
            }

            //----------------------------------------
            // Next Dialogue
            //----------------------------------------

            if (!string.IsNullOrWhiteSpace(
                    response.Next))
            {
                Show(
                    response.Next,
                    Game1.currentLocation,
                    player);
            }
        }

        //----------------------------------------
        // Condition
        //----------------------------------------

        /// <summary>
        /// Game State Query条件を判定します。
        /// 条件未指定の場合はtrueを返します。
        /// </summary>
        private static bool CheckCondition(
            string? condition,
            GameLocation location,
            Farmer player)
        {
            if (string.IsNullOrWhiteSpace(
                    condition))
            {
                return true;
            }

            return GameStateQuery.CheckConditions(
                condition,
                location,
                player,
                null,
                player.ActiveObject);
        }

        //----------------------------------------
        // Trigger Action
        //----------------------------------------

        /// <summary>
        /// Trigger Actionを順番に実行します。
        /// </summary>
        private static bool RunActions(
            string dialogueId,
            List<string> actions)
        {
            foreach (string action in actions)
            {
                if (string.IsNullOrWhiteSpace(
                        action))
                {
                    continue;
                }

                bool success =
                    TriggerActionManager.TryRunAction(
                        action,
                        out string error,
                        out Exception exception);

                if (success)
                    continue;

                //----------------------------------------
                // 実行失敗
                //----------------------------------------

                string message =
                    $"Dialogue '{dialogueId}' failed to run "
                    + $"Trigger Action '{action}': {error}";

                if (exception != null)
                {
                    Monitor?.Log(
                        message
                        + Environment.NewLine
                        + exception,
                        LogLevel.Error);
                }
                else
                {
                    Monitor?.Log(
                        message,
                        LogLevel.Error);
                }

                return false;
            }

            return true;
        }

        //----------------------------------------
        // Text表示
        //----------------------------------------

        /// <summary>
        /// 通常のDialogue Textを表示します。
        /// Durationが指定されている場合は、
        /// 指定時間後に自動で閉じます。
        /// </summary>
        private static void ShowText(
            string? text,
            int duration = 0)
        {
            if (string.IsNullOrWhiteSpace(
                    text))
            {
                return;
            }

            //----------------------------------------
            // Dialogue表示
            //----------------------------------------

            Game1.drawObjectDialogue(
                text);

            //----------------------------------------
            // 自動終了なし
            //----------------------------------------

            if (duration <= 0)
                return;

            //----------------------------------------
            // 表示したDialogueを取得
            //----------------------------------------

            if (Game1.activeClickableMenu
                is not DialogueBox dialogueBox)
            {
                return;
            }

            //----------------------------------------
            // 自動終了
            //----------------------------------------

            DelayedAction.functionAfterDelay(
                () =>
                {
                    // 表示したDialogueがまだ開いている場合だけ
                    // 通常のDialogue終了処理を実行します。
                    if (ReferenceEquals(
                            Game1.activeClickableMenu,
                            dialogueBox))
                    {
                        dialogueBox.closeDialogue();
                    }
                },
                duration);
        }

        //----------------------------------------
        // Dialogue終了後処理
        //----------------------------------------

        /// <summary>
        /// Dialogueを閉じた後に実行する
        /// Trigger Actionを登録します。
        /// </summary>
        private static void RegisterAfterActions(
            string dialogueId,
            List<string> actions)
        {
            if (actions.Count == 0)
                return;

            var previousAfterDialogues =
                Game1.afterDialogues;

            Game1.afterDialogues =
                () =>
                {
                    previousAfterDialogues?.Invoke();

                    RunActions(
                        dialogueId,
                        actions);
                };
        }
    }
}