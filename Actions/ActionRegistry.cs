using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Triggers;
using Ts_Core.Services.Notification;
using Ts_Core.Services.WarpRelated;

namespace Ts_Core.Actions
{
    /// <summary>
    /// Ts_Coreで使用する
    /// TouchAction・TileAction・TriggerActionを登録します。
    /// </summary>
    public static class ActionRegistry
    {
        //----------------------------------------
        // Action登録
        //----------------------------------------

        /// <summary>
        /// Ts_Coreで使用するActionを登録します。
        /// </summary>
        public static void Register()
        {
            RegisterAction(
                "TsCoreWarp",
                HandleTouchAction,
                HandleTileAction);

            RegisterAction(
                "TsCoreMagicWarp",
                HandleTouchAction,
                HandleTileAction);

            RegisterAction(
                "TsCoreMagicWarp_Simple",
                HandleTouchAction,
                HandleTileAction);

            RegisterAction(
                "TsCoreNotification",
                NotificationAction.HandleTouchAction,
                NotificationAction.HandleTileAction);

            TriggerActionManager.RegisterAction(
                "TsCoreNotification",
                NotificationAction.Run);
        }

        //----------------------------------------
        // TouchAction / TileAction登録
        //----------------------------------------

        /// <summary>
        /// TouchAction と TileAction を登録します。
        /// </summary>
        private static void RegisterAction(
            string name,
            Action<GameLocation, string[], Farmer, Vector2> touchHandler,
            Func<GameLocation, string[], Farmer, Point, bool> tileHandler)
        {
            GameLocation.RegisterTouchAction(
                name,
                touchHandler);

            GameLocation.RegisterTileAction(
                name,
                tileHandler);
        }

        //----------------------------------------
        // 向き変換
        //----------------------------------------

        /// <summary>
        /// 向き文字列または数値をゲーム内部の方向へ変換します。
        /// Autoまたは4の場合は現在の向きを維持します。
        /// </summary>
        private static bool TryParseFacingDirection(
            string value,
            out int? direction)
        {
            //----------------------------------------
            // 数値
            //----------------------------------------

            if (int.TryParse(
                    value,
                    out int numericDirection))
            {
                if (numericDirection >= 0
                    && numericDirection <= 3)
                {
                    direction =
                        numericDirection;

                    return true;
                }

                // 4 = Auto
                if (numericDirection == 4)
                {
                    direction = null;

                    return true;
                }

                direction = null;

                return false;
            }

            //----------------------------------------
            // 文字列
            //----------------------------------------

            switch (value.ToLowerInvariant())
            {
                case "up":

                    direction = 0;

                    return true;

                case "right":

                    direction = 1;

                    return true;

                case "down":

                    direction = 2;

                    return true;

                case "left":

                    direction = 3;

                    return true;

                case "auto":

                    direction = null;

                    return true;

                default:

                    direction = null;

                    return false;
            }
        }

        //----------------------------------------
        // Warp実行
        //----------------------------------------

        /// <summary>
        /// Ts_CoreのWarpActionを実行します。
        /// </summary>
        private static bool ExecuteWarp(string[] action)
        {
            if (action.Length < 1)
                return false;

            //----------------------------------------
            // Warp演出
            //----------------------------------------

            WarpEffectMode effectMode;

            switch (action[0])
            {
                case "TsCoreWarp":
                    effectMode = WarpEffectMode.None;
                    break;

                case "TsCoreMagicWarp":
                    effectMode = WarpEffectMode.Magic;
                    break;

                case "TsCoreMagicWarp_Simple":
                    effectMode = WarpEffectMode.MagicSimple;
                    break;

                default:
                    return false;
            }

            //----------------------------------------
            // Warp形式
            //----------------------------------------

            //----------------------------------------
            // 座標Warp
            //----------------------------------------
            //
            // <Action> <Location> <X> <Y>
            // <Action> <Location> <X> <Y> <Facing>
            // <Action> <Location> <X> <Y> <Facing> <AudioCue>
            //----------------------------------------

            if (action.Length >= 4
                && int.TryParse(action[2], out int x)
                && int.TryParse(action[3], out int y))
            {
                if (action.Length > 6)
                    return false;

                int? facingDirection = null;
                string? audioCue = null;

                if (action.Length >= 5)
                {
                    if (!TryParseFacingDirection(
                            action[4],
                            out facingDirection))
                    {
                        return false;
                    }
                }

                if (action.Length == 6)
                {
                    audioCue =
                        action[5];
                }

                WarpService.Warp(
                    action[1],
                    new Point(x, y),
                    effectMode,
                    facingDirection,
                    audioCue);

                return true;
            }

            //----------------------------------------
            // Provider / Map Warp
            //----------------------------------------
            //
            // <Action> <Provider>
            // <Action> <Provider> <Facing>
            // <Action> <Provider> <Facing> <AudioCue>
            //----------------------------------------

            if (action.Length < 2
                || action.Length > 4)
            {
                return false;
            }

            int? providerFacingDirection = null;
            string? providerAudioCue = null;

            if (action.Length >= 3)
            {
                if (!TryParseFacingDirection(
                        action[2],
                        out providerFacingDirection))
                {
                    return false;
                }
            }

            if (action.Length == 4)
            {
                providerAudioCue =
                    action[3];
            }

            if (WarpService.Warp(
                    action[1],
                    effectMode,
                    providerFacingDirection,
                    providerAudioCue))
            {
                return true;
            }

            // Providerが見つからない場合はMap名として扱う
            return WarpService.WarpToMap(
                action[1],
                effectMode,
                providerFacingDirection,
                providerAudioCue);
        }

        //----------------------------------------
        // TouchAction
        //----------------------------------------

        /// <summary>
        /// TouchActionからWarpを実行します。
        /// </summary>
        private static void HandleTouchAction(
            GameLocation location,
            string[] action,
            Farmer who,
            Vector2 playerStandingPosition)
        {
            ExecuteWarp(action);
        }

        //----------------------------------------
        // TileAction
        //----------------------------------------

        /// <summary>
        /// TileActionからWarpを実行します。
        /// </summary>
        private static bool HandleTileAction(
            GameLocation location,
            string[] action,
            Farmer who,
            Point tile)
        {
            return ExecuteWarp(action);
        }
    }
}