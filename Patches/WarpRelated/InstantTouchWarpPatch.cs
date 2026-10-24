using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;

namespace Ts_Core.Patches
{
    /// <summary>
    /// TsCoreInstantTouchWarpが有効なマップで、ワープ系TouchActionを
    /// 通常のマップWarpと同じ移動前の当たり判定で起動します。
    /// </summary>
    internal static class InstantTouchWarpPatch
    {
        private const string PropertyName = "TsCoreInstantTouchWarp";

        internal static void Apply(Harmony harmony)
        {
            var target = AccessTools.Method(typeof(Farmer), "MovePositionImpl");
            if (target != null)
            {
                harmony.Patch(target, prefix: new HarmonyMethod(
                    typeof(InstantTouchWarpPatch), nameof(BeforeMove)));
            }
        }

        /// <summary>
        /// ローカルプレイヤーの次の当たり判定がワープタイルに触れたら、
        /// 既存のTouchAction処理を呼び出します。
        /// </summary>
        private static bool BeforeMove(Farmer __instance, int direction, ref bool __result)
        {
            if (!__instance.IsLocalPlayer || Game1.eventUp)
                return true;

            GameLocation location = __instance.currentLocation;
            if (location?.Map == null
                || !location.Map.Properties.TryGetValue(PropertyName, out var property)
                || !bool.TryParse(property?.ToString(), out bool enabled)
                || !enabled)
                return true;

            Rectangle next = __instance.nextPosition(direction);
            int left = next.Left / 64;
            int right = (next.Right - 1) / 64;
            int top = next.Top / 64;
            int bottom = (next.Bottom - 1) / 64;

            for (int y = top; y <= bottom; y++)
            {
                for (int x = left; x <= right; x++)
                {
                    // 既にプレイヤーが立っているタイルでは起動しない。
                    if (__instance.GetBoundingBox().Intersects(
                        new Rectangle(x * 64, y * 64, 64, 64)))
                        continue;

                    string? action = location.doesTileHaveProperty(x, y, "TouchAction", "Back")
                        ?? location.doesTileHaveProperty(x, y, "TouchAction", "Buildings");
                    if (string.IsNullOrWhiteSpace(action))
                        continue;

                    string name = action.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];
                    if (name != "Warp" && name != "TsCoreWarp"
                        && name != "TsCoreMagicWarp" && name != "TsCoreMagicWarp_Simple")
                        continue;

                    location.performTouchAction(action, new Vector2(x * 64, y * 64));
                    __result = true;
                    return false;
                }
            }
            return true;
        }
    }
}
