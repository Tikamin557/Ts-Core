using StardewValley;

namespace Ts_Core.Providers
{
    /// <summary>
     /// パートナー情報を取得するProviderの共通インターフェースです。
     /// </summary>
    public interface IPartnerProvider
    {
        //----------------------------------------
        // プロパティ
        //----------------------------------------

        /// <summary>
        /// Providerの説明を取得します。
        /// </summary>
        string Description { get; }

        //----------------------------------------
        // パートナー一覧取得
        //----------------------------------------

        /// <summary>
        /// 指定したプレイヤーのパートナー一覧を取得します。
        /// </summary>
        IEnumerable<string> GetPartners(Farmer farmer);
    }
}