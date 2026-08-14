using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Delegates;
using Ts_Core.Services.Location;

namespace Ts_Core.Services.Notification
{
    /// <summary>
    /// 通知Actionを実行するクラスです。
    /// TouchAction・TileAction・TriggerActionから
    /// 共通処理を呼び出します。
    /// </summary>
    internal static class NotificationAction
    {
        //----------------------------------------
        // TouchAction
        //----------------------------------------

        /// <summary>
        /// TouchActionから通知を表示します。
        /// </summary>
        public static void HandleTouchAction(
            GameLocation location,
            string[] args,
            Farmer who,
            Vector2 tile)
        {
            Execute(args.Skip(1).ToArray());
        }

        //----------------------------------------
        // TileAction
        //----------------------------------------

        /// <summary>
        /// TileActionから通知を表示します。
        /// </summary>
        public static bool HandleTileAction(
            GameLocation location,
            string[] args,
            Farmer who,
            Point tile)
        {
            Execute(args.Skip(1).ToArray());
            return true;
        }

        //----------------------------------------
        // TriggerAction
        //----------------------------------------

        /// <summary>
        /// TriggerActionから通知を表示します。
        /// </summary>
        public static bool Run(
            string[] args,
            TriggerActionContext context,
            out string? error)
        {

            error = null;

            Execute(args.Skip(1).ToArray());

            return true;
        }

        //----------------------------------------
        // 共通処理
        //----------------------------------------

        /// <summary>
        /// 通知Actionの共通処理です。
        /// 引数を解析し、通知内容を組み立てて表示します。
        /// </summary>
        private static void Execute(string[] args)
        {
            //----------------------------------------
            // オプション解析
            //----------------------------------------

            const int FirstVisitArgumentCount = 6;

            int index = 0;

            bool markVisited = false;
            string? visitLocation = null;

            //----------------------------------------
            // FirstVisitToday
            //----------------------------------------

            if (args.Length >= FirstVisitArgumentCount &&
                args[0].Equals(
                    "FirstVisitToday",
                    StringComparison.OrdinalIgnoreCase))
            {
                string location = args[1];

                //----------------------------------------
                // 現在地が一致しなければ終了
                //----------------------------------------

                if (Game1.currentLocation?.NameOrUniqueName != location)
                    return;

                //----------------------------------------
                // 今日初回でなければ終了
                //----------------------------------------

                if (!LocationTracker.IsFirstVisitToday(location))
                    return;

                //----------------------------------------
                // 後で訪問済みにする
                //----------------------------------------

                markVisited = true;
                visitLocation = location;

                index += 2;
            }

            //----------------------------------------
            // 引数チェック
            //----------------------------------------

            if (args.Length - index < 4)
                return;

            //----------------------------------------
            // 通知テーマ
            //----------------------------------------

            string themeName = args[index];

            bool isBuiltinType =
                Enum.TryParse(
                    themeName,
                    true,
                    out NotificationType type);

            //----------------------------------------
            // Priority (通知優先度)
            //----------------------------------------

            if (!Enum.TryParse(
                args[index + 1],
                true,
                out NotificationPriority priority))
            {
                priority = NotificationPriority.Normal;
            }

            //----------------------------------------
            // Duration (表示時間)
            //----------------------------------------

            if (!int.TryParse(
                args[index + 2],
                out int duration))
            {
                duration = 120;
            }

            //----------------------------------------
            // 表示メッセージ
            //----------------------------------------

            string message =
                string.Join(
                    " ",
                    args.Skip(index + 3));

            //----------------------------------------
            // 通知リクエスト作成
            //----------------------------------------

            NotificationRequest request;

            if (isBuiltinType)
            {
                request =
                    type.CreateRequest(
                        message,
                        duration);
            }
            else
            {
                request =
                    NotificationRequest.Theme(
                        themeName,
                        message,
                        duration);
            }

            //----------------------------------------
            // PriorityだけAction側で指定
            //----------------------------------------

            request.Priority = priority;

            //----------------------------------------
            // 表示
            //----------------------------------------

            request.Show();

            //----------------------------------------
            // 表示できたら訪問済みにする
            //----------------------------------------

            if (markVisited)
                LocationTracker.MarkVisitedToday(visitLocation);
        }
    }
}
