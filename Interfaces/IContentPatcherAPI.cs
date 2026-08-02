using StardewModdingAPI;

namespace Ts_Core.Interfaces
{
    /// <summary>
    /// Content Patcher APIの最小インターフェースです。
    /// </summary>
    public interface IContentPatcherAPI
    {
        //----------------------------------------
        // トークン登録
        //----------------------------------------

        /// <summary>
        /// Content Patcherへトークンを登録します。
        /// </summary>
        /// <param name="mod">登録元ModのManifest</param>
        /// <param name="name">トークン名</param>
        /// <param name="getValue">トークン値を返すデリゲート</param>
        void RegisterToken(IManifest mod, string name, Func<IEnumerable<string>?> getValue);
    }
}
