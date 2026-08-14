using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.LocationContexts;
using System.Reflection;

namespace Ts_Core.Patches
{
    /// <summary>
    /// RainTotemAffectsContextで指定されたLocation Contextへ
    /// レイントーテムの効果が正しく適用されるよう補正します。
    /// </summary>
    internal static class RainTotemPatch
    {
        //----------------------------------------
        // Monitor
        //----------------------------------------

        private static IMonitor? Monitor;

        //----------------------------------------
        // Patch適用
        //----------------------------------------

        /// <summary>
        /// Stardew Valley本体のRain Totem処理に
        /// Harmonyパッチを適用します。
        /// </summary>
        public static void Apply(
            Harmony harmony,
            IMonitor monitor)
        {
            Monitor = monitor;

            MethodInfo? method = AccessTools.Method(
                typeof(StardewValley.Object),
                "rainTotem");

            if (method == null)
            {
                return;
            }

            harmony.Patch(
                method,
                prefix: new HarmonyMethod(
                    typeof(RainTotemPatch),
                    nameof(Prefix)));
        }

        //----------------------------------------
        // Prefix
        //----------------------------------------

        /// <summary>
        /// RainTotemAffectsContextで別Contextが指定されている場合、
        /// 指定されたContextの翌日天候をRainに設定します。
        /// </summary>
        private static void Prefix(Farmer who)
        {
            GameLocation? location =
                who.currentLocation;

            if (location == null)
                return;

            LocationContextData context =
                location.GetLocationContext();

            string currentContextId =
                location.GetLocationContextId();

            string? targetContextId =
                context.RainTotemAffectsContext;

            //----------------------------------------
            // Debug
            //----------------------------------------

            Monitor?.Log(
                "===== Rain Totem Context =====",
                LogLevel.Trace);

            Monitor?.Log(
                $"Current Context : {currentContextId}",
                LogLevel.Trace);

            Monitor?.Log(
                $"Target Context  : {targetContextId ?? "(none)"}",
                LogLevel.Trace);

            //----------------------------------------
            // Rain Totem使用不可
            //----------------------------------------

            if (!context.AllowRainTotem)
            {
                Monitor?.Log(
                    "Result          : Rain Totem not allowed.",
                    LogLevel.Trace);

                return;
            }

            //----------------------------------------
            // 影響先Contextなし
            //----------------------------------------

            if (string.IsNullOrWhiteSpace(targetContextId))
            {
                Monitor?.Log(
                    "Result          : No context redirect.",
                    LogLevel.Trace);

                return;
            }

            //----------------------------------------
            // DefaultはVanilla側で正常に処理される
            //----------------------------------------

            if (targetContextId == "Default")
            {
                Monitor?.Log(
                    "Result          : Default context handled by vanilla.",
                    LogLevel.Trace);

                return;
            }

            //----------------------------------------
            // 現在Contextと同じ場合は補正不要
            //----------------------------------------

            if (string.Equals(
                    targetContextId,
                    currentContextId,
                    StringComparison.OrdinalIgnoreCase))
            {
                Monitor?.Log(
                    "Result          : Target matches current context.",
                    LogLevel.Trace);

                return;
            }

            //----------------------------------------
            // 指定Contextの翌日天候をRainにする
            //----------------------------------------

            var weather =
                Game1.netWorldState.Value
                    .GetWeatherForLocation(targetContextId);

            weather.WeatherForTomorrow =
                "Rain";

            Monitor?.Log(
                $"Result          : Redirected to {targetContextId}",
                LogLevel.Trace);

            Monitor?.Log(
                $"Tomorrow Weather : {weather.WeatherForTomorrow}",
                LogLevel.Trace);
        }
    }
}