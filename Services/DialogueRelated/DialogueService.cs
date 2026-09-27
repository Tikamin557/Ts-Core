using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
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
            Farmer player,
            Building? sourceBuilding = null)
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
                player,
                sourceBuilding);

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
            Farmer player,
            Building? sourceBuilding)
        {
            //----------------------------------------
            // Condition
            //----------------------------------------

            if (!CheckCondition(
                    dialogue.Condition,
                    location,
                    player))
            {
                HandleConditionFailure(
                    dialogueId,
                    dialogue.FailText,
                    dialogue.FailAudioCue,
                    null,
                    null,
                    player,
                    dialogue.HideDialogue,
                    sourceBuilding);

                return;
            }

            //----------------------------------------
            // Conditions
            //----------------------------------------

            if (!CheckConditions(
                    dialogueId,
                    dialogue.Conditions,
                    location,
                    player,
                    dialogue.HideDialogue,
                    sourceBuilding))
            {
                return;
            }

            //----------------------------------------
            // Usage Limit
            //----------------------------------------

            UsageLimitResult usageLimitResult =
                CheckUsageLimit(
                    dialogueId,
                    dialogue.UsageLimit,
                    sourceBuilding);

            if (usageLimitResult
                == UsageLimitResult.Invalid)
            {
                return;
            }

            if (usageLimitResult
                == UsageLimitResult.AlreadyUsed)
            {
                HandleConditionFailure(
                    dialogueId,
                    dialogue.FailText,
                    dialogue.FailAudioCue,
                    null,
                    null,
                    player,
                    dialogue.HideDialogue,
                    sourceBuilding);

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
            // Dialogue非表示
            //----------------------------------------

            if (dialogue.HideDialogue)
            {
                if (RunSuccessActions(
                        dialogueId,
                        dialogue.AfterActions,
                        dialogue.RandomActions,
                        sourceBuilding))
                {
                    MarkUsageLimit(
                        dialogueId,
                        dialogue.UsageLimit,
                        sourceBuilding);
                }

                return;
            }

            //----------------------------------------
            // Dialogue終了後Action
            //----------------------------------------

            RegisterAfterActions(
                dialogueId,
                dialogue.AfterActions,
                dialogue.RandomActions,
                dialogue.UsageLimit,
                sourceBuilding);

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
                            who,
                            sourceBuilding);
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
            Farmer player,
            Building? sourceBuilding)
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
                HandleConditionFailure(
                    dialogueId,
                    response.FailText,
                    response.FailAudioCue,
                    null,
                    null,
                    player,
                    dialogue.HideDialogue,
                    sourceBuilding);

                return;
            }

            //----------------------------------------
            // Conditions
            //----------------------------------------

            if (!CheckConditions(
                    dialogueId,
                    response.Conditions,
                    location,
                    player,
                    dialogue.HideDialogue,
                    sourceBuilding))
            {
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
                    player,
                    sourceBuilding);
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
        // Conditions
        //----------------------------------------

        /// <summary>
        /// 複数のGame State Query条件を上から順番に判定します。
        /// 最初に失敗した条件のFail処理を実行します。
        /// </summary>
        private static bool CheckConditions(
            string dialogueId,
            List<DialogueConditionModel> conditions,
            GameLocation location,
            Farmer player,
            bool hideDialogue,
            Building? sourceBuilding)
        {
            foreach (DialogueConditionModel condition in conditions)
            {
                if (CheckCondition(
                        condition.Condition,
                        location,
                        player))
                {
                    continue;
                }

                HandleConditionFailure(
                    dialogueId,
                    condition.FailText,
                    condition.FailAudioCue,
                    condition.FailActions,
                    condition.FailNext,
                    player,
                    hideDialogue,
                    sourceBuilding);

                return false;
            }

            return true;
        }

        //----------------------------------------
        // Condition Fail
        //----------------------------------------

        /// <summary>
        /// Condition失敗時の処理を実行します。
        /// FailTextがある場合、FailActionsとFailNextは
        /// FailTextを閉じた後に実行します。
        /// </summary>
        private static void HandleConditionFailure(
            string dialogueId,
            string? failText,
            string? failAudioCue,
            List<string>? failActions,
            string? failNext,
            Farmer player,
            bool hideDialogue,
            Building? sourceBuilding)
        {
            //----------------------------------------
            // Fail Audio Cue
            //----------------------------------------

            if (!string.IsNullOrWhiteSpace(
                    failAudioCue))
            {
                Game1.playSound(
                    failAudioCue);
            }

            //----------------------------------------
            // Fail後処理
            //----------------------------------------

            void RunFailContinuation()
            {
                if (failActions != null
                    && !RunActions(
                        dialogueId,
                        failActions))
                {
                    return;
                }

                if (!string.IsNullOrWhiteSpace(
                        failNext))
                {
                    Show(
                        failNext,
                        Game1.currentLocation,
                        player,
                        sourceBuilding);
                }
            }

            //----------------------------------------
            // Dialogue非表示
            //----------------------------------------

            if (hideDialogue)
            {
                RunFailContinuation();
                return;
            }

            //----------------------------------------
            // FailTextなし
            //----------------------------------------

            if (string.IsNullOrWhiteSpace(
                    failText))
            {
                RunFailContinuation();
                return;
            }

            //----------------------------------------
            // FailTextあり
            //----------------------------------------

            bool hasContinuation =
                (failActions != null
                    && failActions.Count > 0)
                || !string.IsNullOrWhiteSpace(
                    failNext);

            if (hasContinuation)
            {
                var previousAfterDialogues =
                    Game1.afterDialogues;

                Game1.afterDialogues =
                    () =>
                    {
                        previousAfterDialogues?.Invoke();
                        RunFailContinuation();
                    };
            }

            ShowText(
                failText);
        }

        //----------------------------------------
        // Usage Limit
        //----------------------------------------

        /// <summary>
        /// Dialogueの使用回数制限を確認します。
        /// 現在はBuilding / Dayのみ対応しています。
        /// </summary>
        private enum UsageLimitResult
        {
            Available,
            AlreadyUsed,
            Invalid
        }

        private static UsageLimitResult CheckUsageLimit(
            string dialogueId,
            DialogueUsageLimitModel? usageLimit,
            Building? sourceBuilding)
        {
            if (usageLimit == null)
                return UsageLimitResult.Available;

            if (!string.Equals(
                    usageLimit.Scope,
                    "Building",
                    StringComparison.OrdinalIgnoreCase)
                || !string.Equals(
                    usageLimit.Period,
                    "Day",
                    StringComparison.OrdinalIgnoreCase))
            {
                Monitor?.Log(
                    $"Dialogue '{dialogueId}' has an unsupported "
                    + $"UsageLimit (Scope='{usageLimit.Scope}', "
                    + $"Period='{usageLimit.Period}').",
                    LogLevel.Warn);

                return UsageLimitResult.Invalid;
            }

            if (string.IsNullOrWhiteSpace(
                    usageLimit.Key))
            {
                Monitor?.Log(
                    $"Dialogue '{dialogueId}' has UsageLimit but "
                    + "its Key is empty.",
                    LogLevel.Warn);

                return UsageLimitResult.Invalid;
            }

            if (sourceBuilding == null)
            {
                Monitor?.Log(
                    $"Dialogue '{dialogueId}' uses a Building "
                    + "UsageLimit, but it wasn't started from a "
                    + "Building instance.",
                    LogLevel.Warn);

                return UsageLimitResult.Invalid;
            }

            string today =
                Game1.Date.TotalDays.ToString();

            if (sourceBuilding.modData.TryGetValue(
                    usageLimit.Key,
                    out string? lastUsedDay)
                && lastUsedDay == today)
            {
                return UsageLimitResult.AlreadyUsed;
            }

            return UsageLimitResult.Available;
        }

        /// <summary>
        /// Dialogueの使用済み状態を記録します。
        /// </summary>
        private static void MarkUsageLimit(
            string dialogueId,
            DialogueUsageLimitModel? usageLimit,
            Building? sourceBuilding)
        {
            if (usageLimit == null
                || sourceBuilding == null)
            {
                return;
            }

            if (!string.Equals(
                    usageLimit.Scope,
                    "Building",
                    StringComparison.OrdinalIgnoreCase)
                || !string.Equals(
                    usageLimit.Period,
                    "Day",
                    StringComparison.OrdinalIgnoreCase)
                || string.IsNullOrWhiteSpace(
                    usageLimit.Key))
            {
                return;
            }

            sourceBuilding.modData[
                usageLimit.Key] =
                    Game1.Date.TotalDays.ToString();
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
        // Random Action
        //----------------------------------------

        private const string RandomActionBuildingIdKey =
            "Tikamin557.TsCore/DialogueBuildingId";

        /// <summary>
        /// 通常ActionとランダムActionを順番に実行します。
        /// </summary>
        private static bool RunSuccessActions(
            string dialogueId,
            List<string> actions,
            List<DialogueRandomActionModel> randomActions,
            Building? sourceBuilding)
        {
            if (!RunActions(
                    dialogueId,
                    actions))
            {
                return false;
            }

            return RunRandomActions(
                dialogueId,
                randomActions,
                sourceBuilding);
        }

        /// <summary>
        /// RandomActionsからWeightに応じて1件を選択し、
        /// そのActionsを実行します。
        /// </summary>
        private static bool RunRandomActions(
            string dialogueId,
            List<DialogueRandomActionModel> randomActions,
            Building? sourceBuilding)
        {
            if (randomActions.Count == 0)
                return true;

            int totalWeight = 0;

            foreach (DialogueRandomActionModel candidate in randomActions)
            {
                if (candidate.Weight <= 0)
                {
                    Monitor?.Log(
                        $"Dialogue '{dialogueId}' has RandomActions "
                        + "with Weight <= 0. All weights must be at least 1.",
                        LogLevel.Warn);

                    return false;
                }

                try
                {
                    checked
                    {
                        totalWeight += candidate.Weight;
                    }
                }
                catch (OverflowException)
                {
                    Monitor?.Log(
                        $"Dialogue '{dialogueId}' has RandomActions "
                        + "whose total Weight is too large.",
                        LogLevel.Warn);

                    return false;
                }
            }

            string buildingSeed =
                GetRandomActionBuildingSeed(
                    sourceBuilding);

            string seedText =
                $"{dialogueId}|{Game1.Date.TotalDays}|{buildingSeed}";

            int seed =
                Game1.hash.GetDeterministicHashCode(
                    seedText);

            Random random = new Random(seed);
            int roll = random.Next(totalWeight);

            DialogueRandomActionModel selected =
                randomActions[0];

            foreach (DialogueRandomActionModel candidate in randomActions)
            {
                if (roll < candidate.Weight)
                {
                    selected = candidate;
                    break;
                }

                roll -= candidate.Weight;
            }

            return RunActions(
                dialogueId,
                selected.Actions);
        }

        /// <summary>
        /// Building由来の場合は永続IDを取得します。
        /// Building以外ではDialogue共通Seedを使用します。
        /// </summary>
        private static string GetRandomActionBuildingSeed(
            Building? sourceBuilding)
        {
            if (sourceBuilding == null)
                return "NoBuilding";

            if (!sourceBuilding.modData.TryGetValue(
                    RandomActionBuildingIdKey,
                    out string? buildingId)
                || string.IsNullOrWhiteSpace(
                    buildingId))
            {
                buildingId = Guid.NewGuid()
                    .ToString("N");

                sourceBuilding.modData[
                    RandomActionBuildingIdKey] =
                        buildingId;
            }

            return buildingId;
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
            List<string> actions,
            List<DialogueRandomActionModel> randomActions,
            DialogueUsageLimitModel? usageLimit,
            Building? sourceBuilding)
        {
            if (actions.Count == 0
                && randomActions.Count == 0
                && usageLimit == null)
            {
                return;
            }

            var previousAfterDialogues =
                Game1.afterDialogues;

            Game1.afterDialogues =
                () =>
                {
                    previousAfterDialogues?.Invoke();

                    if (RunSuccessActions(
                            dialogueId,
                            actions,
                            randomActions,
                            sourceBuilding))
                    {
                        MarkUsageLimit(
                            dialogueId,
                            usageLimit,
                            sourceBuilding);
                    }
                };
        }
    }
}