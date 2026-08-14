using StardewModdingAPI;
using Ts_Core.Interfaces;
using Ts_Core.Services.Relationship;

namespace Ts_Core.Tokens
{
    /// <summary>
    /// Ts_Coreで使用するContent Patcherトークンを登録するクラスです。
    /// </summary>
    public static class TokenRegistrar
    {
        //----------------------------------------
        // 全Token登録
        //----------------------------------------

        public static void Register(
            IContentPatcherAPI api,
            IManifest manifest,
            PartnerService partnerService,
            Func<IEnumerable<string>> getOrderedPartners)
        {
            //----------------------------------------
            // Location
            //----------------------------------------

            LocationTokens.Register(
                api,
                manifest);

            //----------------------------------------
            // Marriage
            //----------------------------------------

            MarriageTokens.Register(
                api,
                manifest,
                partnerService,
                getOrderedPartners);
        }
    }
}