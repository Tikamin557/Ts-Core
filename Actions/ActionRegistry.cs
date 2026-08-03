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
        /// </summary>
        private static int? ParseFacingDirection(string value)
        {
            if (int.TryParse(value, out int direction)
                && direction >= 0
                && direction <= 3)
            {
                return direction;
            }

            return value.ToLowerInvariant() switch
            {
                "up" => 0,
                "right" => 1,
                "down" => 2,
                "left" => 3,
                _ => null
            };
        }

        //----------------------------------------
        // Warp実行
        //----------------------------------------

        /// <summary>
        /// Ts_CoreのWarpActionを実行します。
        /// </summary>
        private static bool ExecuteWarp(string[] action)
        {
            // Content Patcher:
            // TsCoreWarp FarmHouseFront
            // TsCoreWarp FarmHouseFront Left

            if (action.Length < 1)
                return false;

            bool magic;

            switch (action[0])
            {
                case "TsCoreWarp":
                    magic = false;
                    break;

                case "TsCoreMagicWarp":
                    magic = true;
                    break;

                default:
                    return false;
            }

            switch (action.Length)
            {
                //----------------------------------------
                // ProviderWarp
                //----------------------------------------

                // TsCoreWarp FarmHouseFront
                // TsCoreWarp FarmHouseFront Left

                case 2:
                case 3:
                    {
                        int? facingDirection = null;

                        if (action.Length == 3)
                            facingDirection = ParseFacingDirection(action[2]);

                        if (WarpService.Warp(
                                action[1],
                                magic,
                                facingDirection))
                        {
                            return true;
                        }

                        // Providerが見つからない場合はMap名として扱う
                        return WarpService.WarpToMap(
                            action[1],
                            magic,
                            facingDirection);
                    }

                //----------------------------------------
                // 座標Warp
                //----------------------------------------

                // TsCoreWarp Farm 64 15
                // TsCoreWarp Farm 64 15 Left

                case 4:
                case 5:
                    {
                        if (!int.TryParse(action[2], out int x))
                            return false;

                        if (!int.TryParse(action[3], out int y))
                            return false;

                        int? facingDirection = null;

                        if (action.Length == 5)
                            facingDirection = ParseFacingDirection(action[4]);

                        WarpService.Warp(
                            action[1],
                            new Point(x, y),
                            magic,
                            facingDirection);

                        return true;
                    }

                default:
                    return false;
            }
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