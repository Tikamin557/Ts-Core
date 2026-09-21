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
    }
}