using StardewModdingAPI;
using Ts_Core.Interfaces;
using Ts_Core.Services.Location;

namespace Ts_Core.Tokens
{
    /// <summary>
    /// Warp関連のContent Patcherトークンを登録します。
    /// </summary>
    public static class WarpTokens
    {
        //----------------------------------------
        // Token登録
        //----------------------------------------

        public static void Register(
            IContentPatcherAPI api,
            IManifest manifest,
            LocationService locationService)
        {
            //----------------------------------------
            // FarmHouseEntry
            //----------------------------------------

            api.RegisterToken(
                manifest,
                "FarmHouseEntry",
                locationService.GetFarmHouseEntry);

            //----------------------------------------
            // FarmHouseEntryX
            //----------------------------------------

            api.RegisterToken(
                manifest,
                "FarmHouseEntryX",
                locationService.GetFarmHouseEntryX);

            //----------------------------------------
            // FarmHouseEntryY
            //----------------------------------------

            api.RegisterToken(
                manifest,
                "FarmHouseEntryY",
                locationService.GetFarmHouseEntryY);
        }
    }
}