using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace Ts_Core.Services.Location
{
    /// <summary>
    /// プレイヤーの移動履歴や訪問情報を管理するクラスです。
    /// </summary>
    public static class LocationTracker
    {
        //----------------------------------------
        // 現在地情報
        //----------------------------------------

        private static string currentLocation = "";
        private static string previousLocation = "";

        private static int enteredTime;

        //----------------------------------------
        // 訪問履歴
        //----------------------------------------

        private static readonly Dictionary<string, int> visitCounts = new();
        private static readonly Dictionary<string, int> sessionVisitCounts = new();
        private static readonly HashSet<string> enteredToday = new();

        //----------------------------------------
        // 現在地
        //----------------------------------------

        public static string CurrentLocation
            => currentLocation;

        public static string PreviousLocation
            => previousLocation;

        //----------------------------------------
        // Location名解決
        //----------------------------------------

        private static string ResolveLocation(string? location)
        {
            if (string.IsNullOrWhiteSpace(location))
                return currentLocation;

            return location;
        }

        //----------------------------------------
        // 滞在時間
        //----------------------------------------

        /// <summary>
        /// 現在の場所に滞在している時間（分）を取得します。
        /// </summary>
        public static int LocationElapsed
        {
            get
            {
                return Utility.CalculateMinutesBetweenTimes(
                    enteredTime,
                    Game1.timeOfDay);
            }
        }

        //----------------------------------------
        // 訪問回数
        //----------------------------------------

        /// <summary>
        /// 指定した場所の累計訪問回数を取得します。
        /// </summary>
        public static int VisitCount(string? location = null)
        {
            location = ResolveLocation(location);

            return visitCounts.TryGetValue(location, out int count)
                ? count
                : 0;
        }

        /// <summary>
        /// ゲーム起動後の訪問回数を取得します。
        /// </summary>
        public static int SessionVisitCount(string? location = null)
        {
            location = ResolveLocation(location);

            return sessionVisitCounts.TryGetValue(location, out int count)
                ? count
                : 0;
        }

        //----------------------------------------
        // 今日訪問したか
        //----------------------------------------

        /// <summary>
        /// 今日その場所を訪問済みかどうかを返します。
        /// </summary>
        public static bool EnteredToday(string? location = null)
        {
            location = ResolveLocation(location);

            return enteredToday.Contains(location);
        }

        /// <summary>
        /// 今日初めて訪れる場所なら true を返します。
        /// （まだ訪問済みにはしません）
        /// </summary>
        public static bool IsFirstVisitToday(string? location = null)
        {
            location = ResolveLocation(location);

            return !enteredToday.Contains(location);
        }

        /// <summary>
        /// 指定した場所を今日訪問済みとして記録します。
        /// </summary>
        public static void MarkVisitedToday(string? location = null)
        {
            location = ResolveLocation(location);

            enteredToday.Add(location);
        }

        //----------------------------------------
        // 初期化
        //----------------------------------------

        public static void Initialize(IModHelper helper)
        {
            helper.Events.Player.Warped += OnWarped;
            helper.Events.GameLoop.DayStarted += OnDayStarted;
        }

        //----------------------------------------
        // 解放
        //----------------------------------------

        public static void Dispose(IModHelper helper)
        {
            helper.Events.Player.Warped -= OnWarped;
            helper.Events.GameLoop.DayStarted -= OnDayStarted;
        }

        //----------------------------------------
        // Warp時
        //----------------------------------------

        private static void OnWarped(object? sender, WarpedEventArgs e)
        {
            if (!e.IsLocalPlayer)
                return;

            previousLocation = e.OldLocation.NameOrUniqueName;
            currentLocation = e.NewLocation.NameOrUniqueName;

            enteredTime = Game1.timeOfDay;

            if (!visitCounts.ContainsKey(currentLocation))
                visitCounts[currentLocation] = 0;

            visitCounts[currentLocation]++;

            if (!sessionVisitCounts.ContainsKey(currentLocation))
                sessionVisitCounts[currentLocation] = 0;

            sessionVisitCounts[currentLocation]++;
        }

        //----------------------------------------
        // 日付変更時
        //----------------------------------------

        private static void OnDayStarted(
            object? sender,
            DayStartedEventArgs e)
        {
            enteredToday.Clear();
        }

        //----------------------------------------
        // 屋内・屋外判定
        //----------------------------------------

        public static bool IsOutdoors()
        {
            return Game1.currentLocation?.IsOutdoors ?? false;
        }

        public static bool IsIndoors()
        {
            return !IsOutdoors();
        }
    }
}