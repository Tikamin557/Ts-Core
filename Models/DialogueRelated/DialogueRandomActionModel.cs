namespace Ts_Core.Models.DialogueRelated
{
    /// <summary>
    /// DialogueのランダムAction候補1件分です。
    /// </summary>
    public sealed class DialogueRandomActionModel
    {
        /// <summary>
        /// この候補が選ばれる重みです。
        /// 1以上を指定します。
        /// </summary>
        public int Weight { get; set; } = 1;

        /// <summary>
        /// この候補が選ばれた場合に、上から順に実行する
        /// Trigger Action一覧です。
        /// </summary>
        public List<string> Actions { get; set; } = new();
    }
}
