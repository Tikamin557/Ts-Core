using StardewModdingAPI;
using Ts_Core.Interfaces;
using Ts_Core.Services.Relationship;

namespace Ts_Core.Tokens
{
    /// <summary>
    /// 結婚・ルームメイト関連のContent Patcherトークンを登録します。
    /// </summary>
    public static class MarriageTokens
    {
        //----------------------------------------
        // Token登録
        //----------------------------------------

        public static void Register(
            IContentPatcherAPI api,
            IManifest manifest,
            PartnerService partnerService,
            Func<IEnumerable<string>> getOrderedPartners)
        {
            //----------------------------------------
            // Partners
            //----------------------------------------

            api.RegisterToken(
                manifest,
                "Partners",
                partnerService.GetPartners);

            //----------------------------------------
            // OrderedPartners
            //----------------------------------------

            api.RegisterToken(
                manifest,
                "OrderedPartners",
                getOrderedPartners);
        }
    }
}