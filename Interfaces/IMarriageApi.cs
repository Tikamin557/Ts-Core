using StardewValley;

namespace Ts_Core.Interfaces
{
    /// <summary>
    /// 結婚Modが提供するAPIの共通インターフェースです。
    /// </summary>
    public interface IMarriageApi
    {
        //----------------------------------------
        // パートナー一覧取得
        //----------------------------------------

        /// <summary>
        /// 指定したプレイヤーのパートナー一覧を取得します。
        /// </summary>
        /// <param name="farmer">対象プレイヤー</param>
        /// <param name="all">
        /// true の場合は全てのパートナーを取得します。
        /// </param>
        /// <returns>
        /// キーがNPC名、値がNPCオブジェクトのDictionaryを返します。
        /// </returns>
        Dictionary<string, NPC> GetSpouses(Farmer farmer, bool all = false);
    }
}