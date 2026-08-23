using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using Ts_Core.Services.WarpRelated;

namespace Ts_Core.Patches
{
    /// <summary>
    /// Warp暗転オーバーレイを
    /// ゲームの最終描画後に描画します。
    /// </summary>
    internal static class WarpBlackoutOverlayPatch
    {
        //----------------------------------------
        // Patch適用
        //----------------------------------------

        internal static void Apply(
            Harmony harmony)
        {
            harmony.Patch(
                original:
                AccessTools.Method(
                    typeof(Game1),
                    "Draw",
                    new[]
                    {
                        typeof(GameTime)
                    }),
                postfix:
                new HarmonyMethod(
                    typeof(WarpBlackoutOverlayPatch),
                    nameof(AfterDraw)));
        }

        //----------------------------------------
        // Draw完了後
        //----------------------------------------

        private static void AfterDraw()
        {
            WarpBlackoutOverlayService.Draw();
        }
    }
}