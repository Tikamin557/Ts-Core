using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;

namespace Ts_Core.Services.FarmhouseFixes
{
    /// <summary>
    /// FarmHouseのセラー入口がマップ再構築後に消える場合に、
    /// Stardew Valley 本体の処理を使って再適用する。
    /// </summary>
    internal static class CellarEntranceFixService
    {
        private static IMonitor? Monitor;

        //----------------------------------------
        // Refresh Request
        //----------------------------------------

        private static bool refreshRequested;

        /// <summary>
        /// サービス初期化。
        /// </summary>
        internal static void Initialize(
            IModHelper helper,
            IMonitor monitor)
        {
            Monitor = monitor;

            helper.Events.GameLoop.DayStarted += OnDayStarted;
            helper.Events.Player.Warped += OnWarped;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
        }

        //----------------------------------------
        // DayStarted
        //----------------------------------------

        [EventPriority((EventPriority)(-2147483648))]
        private static void OnDayStarted(
            object? sender,
            DayStartedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            FarmHouse farmHouse =
                Utility.getHomeOfFarmer(Game1.player);

            // FarmHouse 内で朝を迎えた場合のみ。
            if (Game1.currentLocation != farmHouse)
                return;

            RefreshCellarMapState(farmHouse);
        }

        //----------------------------------------
        // Warped
        //----------------------------------------

        [EventPriority((EventPriority)(-2147483648))]
        private static void OnWarped(
            object? sender,
            WarpedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            // ローカルプレイヤーだけを対象にする。
            if (!e.Player.IsLocalPlayer)
                return;

            FarmHouse farmHouse =
                Utility.getHomeOfFarmer(Game1.player);

            // 自宅FarmHouseへ入った場合のみ。
            if (e.NewLocation != farmHouse)
                return;

            RefreshCellarMapState(farmHouse);
        }

        //----------------------------------------
        // UpdateTicked
        //----------------------------------------

        private static void OnUpdateTicked(
            object? sender,
            UpdateTickedEventArgs e)
        {
            // 再適用要求がなければ何もしない。
            if (!refreshRequested)
                return;

            // 要求は1回だけ処理する。
            refreshRequested = false;

            if (!Context.IsWorldReady)
                return;

            FarmHouse farmHouse =
                Utility.getHomeOfFarmer(Game1.player);

            // 現在FarmHouse内にいる場合のみ。
            if (Game1.currentLocation != farmHouse)
                return;

            RefreshCellarMapState(farmHouse);
        }

        //----------------------------------------
        // Refresh Request
        //----------------------------------------

        /// <summary>
        /// 次のUpdateTickedで、
        /// 現在のFarmHouseのセラー状態を再適用する。
        /// </summary>
        internal static void RequestRefresh()
        {
            refreshRequested = true;
        }

        //----------------------------------------
        // Cellar Refresh
        //----------------------------------------

        private static void RefreshCellarMapState(
            FarmHouse farmHouse)
        {
            // セラー未開放なら何もしない。
            if (farmHouse.upgradeLevel < 3)
                return;

            try
            {
                //----------------------------------------
                // セラーパッチを再適用
                //----------------------------------------

                farmHouse.AddCellarTiles();

                //----------------------------------------
                // セラーへのWarpを再構築
                //----------------------------------------

                farmHouse.createCellarWarps();

                //----------------------------------------
                // 現在のMapを基準に
                // 壁紙・床データを再取得
                //----------------------------------------

                farmHouse.ReadWallpaperAndFloorTileData();

                //----------------------------------------
                // 床を現在の設定で再描画
                //----------------------------------------

                farmHouse.setFloors();
            }
            catch (Exception ex)
            {
                Monitor?.Log(
                    $"Failed refreshing FarmHouse cellar.\n{ex}",
                    LogLevel.Error);
            }
        }
    }
}