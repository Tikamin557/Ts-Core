using StardewModdingAPI;
using Ts_Core.Interfaces;
using Ts_Core.Services.Location;

namespace Ts_Core.Tokens
{
    /// <summary>
    /// Location関連のContent Patcherトークンを登録します。
    /// </summary>
    public static class LocationTokens
    {
        //----------------------------------------
        // Token登録
        //----------------------------------------

        public static void Register(
            IContentPatcherAPI api,
            IManifest manifest)
        {

            //----------------------------------------
            // LocationElapsed
            //----------------------------------------

            api.RegisterToken(
                manifest,
                "LocationElapsed",
                () => new[]
                {
                    LocationTracker.LocationElapsed.ToString()
                });

            //----------------------------------------
            // PreviousLocation
            //----------------------------------------

            api.RegisterToken(
                manifest,
                "PreviousLocation",
                () => new[]
                {
                    LocationTracker.PreviousLocation
                });

            //----------------------------------------
            // VisitCount
            //----------------------------------------

            api.RegisterToken(
                manifest,
                "VisitCount",
                () => new[]
                {
                    LocationTracker.VisitCount().ToString()
                });

            //----------------------------------------
            // SessionVisitCount
            //----------------------------------------

            api.RegisterToken(
                manifest,
                "SessionVisitCount",
                () => new[]
                {
                    LocationTracker.SessionVisitCount().ToString()
                });

            //----------------------------------------
            // EnteredToday
            //----------------------------------------

            api.RegisterToken(
                manifest,
                "EnteredToday",
                () => new[]
            {
                LocationTracker
                        .EnteredToday()
                        .ToString()
                        .ToLowerInvariant()
            });

            //----------------------------------------
            // IsOutdoors
            //----------------------------------------

            api.RegisterToken(
                manifest,
                "IsOutdoors",
                () => new[]
            {
                LocationTracker
                        .IsOutdoors()
                        .ToString()
                        .ToLowerInvariant()
            });

            //----------------------------------------
            // IsIndoors
            //----------------------------------------

            api.RegisterToken(
                manifest,
                "IsIndoors",
                () => new[]
            {
                LocationTracker
                        .IsIndoors()
                        .ToString()
                        .ToLowerInvariant()
            });
        }
    }
}