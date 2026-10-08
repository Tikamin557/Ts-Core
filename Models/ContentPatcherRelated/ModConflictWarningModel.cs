namespace Ts_Core.Models.ContentPatcherRelated
{
    /// <summary>同時導入を警告するModの組み合わせと任意の警告文です。</summary>
    public sealed class ModConflictWarningModel
    {
        /// <summary>このうち2件以上が読み込まれている場合に警告します。</summary>
        public List<string> ModIds { get; set; } = new();

        /// <summary>省略時はT's Coreの標準メッセージを使用します。</summary>
        public string? Message { get; set; }
    }
}
