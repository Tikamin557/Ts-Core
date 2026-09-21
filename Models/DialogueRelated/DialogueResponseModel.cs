namespace Ts_Core.Models.DialogueRelated
{
    /// <summary>
    /// TsCore Dialogueの選択肢1件分の定義です。
    /// </summary>
    public sealed class DialogueResponseModel
    {
        //----------------------------------------
        // 表示内容
        //----------------------------------------

        /// <summary>
        /// 選択肢に表示するテキストです。
        /// </summary>
        public string Text { get; set; } = "";

        //----------------------------------------
        // 条件
        //----------------------------------------

        /// <summary>
        /// この選択肢を実行するための
        /// Game State Query条件です。
        /// </summary>
        public string? Condition { get; set; }

        /// <summary>
        /// Conditionを満たさなかった場合に
        /// 表示するテキストです。
        /// </summary>
        public string? FailText { get; set; }

        //----------------------------------------
        // Fail Audio
        //----------------------------------------

        /// <summary>
        /// Condition失敗時にFailTextと一緒に再生する
        /// Audio Cueです。
        /// </summary>
        public string? FailAudioCue { get; set; }

        //----------------------------------------
        // Action
        //----------------------------------------

        /// <summary>
        /// 選択時に実行するTrigger Action一覧です。
        /// </summary>
        public List<string> Actions { get; set; } =
            new();

        //----------------------------------------
        // 次のDialogue
        //----------------------------------------

        /// <summary>
        /// Action実行後に続けて表示する
        /// Dialogue IDです。
        /// </summary>
        public string? Next { get; set; }
    }
}