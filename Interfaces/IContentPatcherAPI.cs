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
        /// Content Patcherへ単純なトークンを登録します。
        /// </summary>
        /// <param name="mod">登録元ModのManifest</param>
        /// <param name="name">トークン名</param>
        /// <param name="getValue">トークン値を返すデリゲート</param>
        void RegisterToken(
            IManifest mod,
            string name,
            Func<IEnumerable<string>?> getValue);

        /// <summary>
        /// Content Patcherへ入力値を受け取る高度なトークンを登録します。
        /// </summary>
        /// <param name="mod">登録元ModのManifest</param>
        /// <param name="name">トークン名</param>
        /// <param name="token">トークン処理を行うオブジェクト</param>
        void RegisterToken(
            IManifest mod,
            string name,
            object token);
    }
}