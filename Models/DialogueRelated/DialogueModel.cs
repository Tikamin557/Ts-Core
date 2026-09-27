namespace Ts_Core.Models.DialogueRelated
{
    /// <summary>
    /// TsCore Dialogue 1件分の定義です。
    /// </summary>
    public sealed class DialogueModel
    {
        //----------------------------------------
        // 自動終了
        //----------------------------------------

        /// <summary>
        /// Dialogueを自動で閉じるまでの時間です。
        /// ミリ秒単位で指定します。
        /// 0以下の場合は自動で閉じません。
        /// </summary>
        public int Duration { get; set; } = 0;

        //----------------------------------------
        // 表示内容
        //----------------------------------------

        /// <summary>
        /// 表示する会話テキストです。
        /// </summary>
        public string Text { get; set; } = "";

        /// <summary>
        /// trueの場合、このDialogueから発生する
        /// メッセージ表示をすべて非表示にします。
        /// 成功時のAfterActionsは即時実行されます。
        /// </summary>
        public bool HideDialogue { get; set; } = false;

        //----------------------------------------
        // 条件
        //----------------------------------------

        /// <summary>
        /// このDialogueを使用するための
        /// Game State Query条件です。
        /// </summary>
        public string? Condition { get; set; }

        /// <summary>
        /// Conditionを満たさなかった場合に
        /// 表示するテキストです。
        /// </summary>
        public string? FailText { get; set; }

        /// <summary>
        /// このDialogueを使用するための複数条件です。
        /// 上から順番に判定し、最初に失敗した条件の
        /// Fail処理を実行します。
        /// </summary>
        public List<DialogueConditionModel> Conditions { get; set; } =
            new();

        //----------------------------------------
        // 使用回数制限
        //----------------------------------------

        /// <summary>
        /// Dialogueの使用回数制限です。
        /// 未指定の場合は制限しません。
        /// 現在はScope=Building、Period=Dayに対応しています。
        /// </summary>
        public DialogueUsageLimitModel? UsageLimit { get; set; }

        //----------------------------------------
        // Audio
        //----------------------------------------

        /// <summary>
        /// Dialogue表示時に再生するAudio Cueです。
        /// </summary>
        public string? AudioCue { get; set; }

        //----------------------------------------
        // Fail Audio
        //----------------------------------------

        /// <summary>
        /// Condition失敗時にFailTextと一緒に再生する
        /// Audio Cueです。
        /// </summary>
        public string? FailAudioCue { get; set; }

        //----------------------------------------
        // 選択肢
        //----------------------------------------

        /// <summary>
        /// 表示する選択肢一覧です。
        /// </summary>
        public List<DialogueResponseModel> Responses { get; set; } =
            new();

        //----------------------------------------
        // Dialogue終了後Action
        //----------------------------------------

        /// <summary>
        /// Dialogueを閉じた後に実行する
        /// Trigger Action一覧です。
        /// </summary>
        public List<string> AfterActions { get; set; } =
            new();

        //----------------------------------------
        // ランダムAction
        //----------------------------------------

        /// <summary>
        /// Dialogue成功後に候補から重み付きで1件を選び、
        /// そのActionsを上から順に実行します。
        /// Buildingから開始された場合はBuilding個体ごと、
        /// かつゲーム内日ごとに抽選結果が決まります。
        /// </summary>
        public List<DialogueRandomActionModel> RandomActions { get; set; } =
            new();
    }
}