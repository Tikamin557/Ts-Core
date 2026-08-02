using StardewModdingAPI;
using StardewValley;
using Ts_Core.Interfaces;

namespace Ts_Core.Providers
{
    /// <summary>
    /// API対応している重婚Modからパートナー情報を取得します。
    /// </summary>
    public class ApiMarriageProvider : IPartnerProvider
    {
        //----------------------------------------
        // フィールド
        //----------------------------------------

        private readonly IModHelper helper;

        private IMarriageApi? api;

        //----------------------------------------
        // プロパティ
        //----------------------------------------

        /// <summary>
        /// Providerの説明を取得します。
        /// </summary>
        public string Description => $"MarriageMod: {ModId}";

        /// <summary>
        /// 使用している重婚ModのUniqueIDです。
        /// </summary>
        public string ModId { get; }

        //----------------------------------------
        // コンストラクタ
        //----------------------------------------

        public ApiMarriageProvider(IModHelper helper, string modId)
        {
            this.helper = helper;
            ModId = modId;
        }

        //----------------------------------------
        // API設定
        //----------------------------------------

        /// <summary>
        /// 利用する結婚APIを設定します。
        /// </summary>
        public void SetApi(IMarriageApi api)
        {
            this.api = api;
        }

        //----------------------------------------
        // パートナー一覧取得
        //----------------------------------------

        /// <summary>
        /// 重婚Modからパートナー一覧を取得します。
        /// </summary>
        public IEnumerable<string> GetPartners(Farmer farmer)
        {
            if (api == null)
                return Enumerable.Empty<string>();

            try
            {
                return api.GetSpouses(farmer, true).Keys;
            }
            catch
            {
                return Enumerable.Empty<string>();
            }
        }
    }
}