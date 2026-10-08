using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;

namespace Ts_Core.Services.MapRelated.TsCoreAmbientLight
{
    /// <summary>
    /// TsCoreAmbientLight = Farmhouse が設定されたLocationへ
    /// Vanilla FarmHouseと同じAmbient Light処理を適用します。
    /// </summary>
    internal static class TsCoreAmbientLightPatch
    {
        private static readonly Color NightLightingColor =
            new Color(180, 180, 0);

        private static readonly Color RainLightingColor =
            new Color(90, 90, 0);

        //----------------------------------------
        // Apply
        //----------------------------------------

        /// <summary>
        /// TsCoreAmbientLightに必要なHarmonyパッチを適用します。
        /// </summary>
        internal static void Apply(
            Harmony harmony)
        {
            harmony.Patch(
                original:
                    AccessTools.Method(
                        typeof(GameLocation),
                        "_updateAmbientLighting"),
                prefix:
                    new HarmonyMethod(
                        typeof(TsCoreAmbientLightPatch),
                        nameof(BeforeUpdateAmbientLighting)));
        }

        //----------------------------------------
        // GameLocation._updateAmbientLighting
        //----------------------------------------

        /// <summary>
        /// TsCoreAmbientLight = Farmhouse のLocationでは
        /// Vanilla FarmHouse._updateAmbientLightingと同じ計算を行います。
        /// </summary>
        private static bool BeforeUpdateAmbientLighting(
            GameLocation __instance)
        {
            if (!TsCoreAmbientLightService.IsFarmhouseMode(
                __instance))
            {
                return true;
            }

            if (Game1.isStartingToGetDarkOut(__instance)
                || __instance.LightLevel > 0f)
            {
                int time =
                    Game1.timeOfDay
                    + Game1.gameTimeInterval
                    / (Game1.realMilliSecondsPerGameMinute
                        + __instance.ExtraMillisecondsPerInGameMinute);

                float lerp =
                    1f - Utility.Clamp(
                        (float)Utility.CalculateMinutesBetweenTimes(
                            time,
                            Game1.getTrulyDarkTime(__instance))
                        / 120f,
                        0f,
                        1f);

                Game1.ambientLight =
                    new Color(
                        (byte)Utility.Lerp(
                            Game1.isRaining
                                ? RainLightingColor.R
                                : 0,
                            NightLightingColor.R,
                            lerp),
                        (byte)Utility.Lerp(
                            Game1.isRaining
                                ? RainLightingColor.G
                                : 0,
                            NightLightingColor.G,
                            lerp),
                        (byte)Utility.Lerp(
                            0f,
                            NightLightingColor.B,
                            lerp));
            }
            else
            {
                Game1.ambientLight =
                    Game1.isRaining
                        ? RainLightingColor
                        : Color.White;
            }

            return false;
        }
    }
}
