namespace Ts_Core.Models.DialogueRelated
{
    /// <summary>
    /// TsCore Dialogueの条件1件分の定義です。
    /// </summary>
    public sealed class DialogueConditionModel
    {
        //----------------------------------------
        // 条件
        //----------------------------------------

        /// <summary>
        /// 判定するGame State Query条件です。
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
        // Fail Action
        //----------------------------------------

        /// <summary>
        /// Condition失敗時に実行する
        /// Trigger Action一覧です。
        /// FailTextがある場合は、Dialogueを閉じた後に実行します。
        /// </summary>
        public List<string> FailActions { get; set; } =
            new();

        //----------------------------------------
        // Fail Next
        //----------------------------------------

        /// <summary>
        /// Condition失敗時に続けて表示するDialogue IDです。
        /// FailTextがある場合は、Dialogueを閉じた後に表示します。
        /// </summary>
        public string? FailNext { get; set; }
    }
}
