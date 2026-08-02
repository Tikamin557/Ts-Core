using StardewModdingAPI;
using Ts_Core.Interfaces;
using Ts_Core.Services.Location;
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
            LocationService locationService,
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

            //----------------------------------------
            // Warp
            //----------------------------------------

            WarpTokens.Register(
                api,
                manifest,
                locationService);
        }
    }
}