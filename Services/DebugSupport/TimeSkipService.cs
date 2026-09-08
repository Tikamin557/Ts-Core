using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using Ts_Core.Models;
using Ts_Core.Services.Notification;

namespace Ts_Core.Services.DebugSupport
{
    /// <summary>
    /// 指定したゲーム内時刻まで時間を進めます。
    /// </summary>
    internal static class TimeSkipService
    {
        private static IModHelper helper = null!;

        private static bool isSkipping;

        private static int targetTime;

        private static bool waitingForSchedule;

        //----------------------------------------
        // 初期化
        //----------------------------------------

        /// <summary>
        /// Time Skipサービスを初期化します。
        /// </summary>
        internal static void Initialize(
            IModHelper modHelper)
        {
            helper =
                modHelper;

            helper.Events.Input.ButtonPressed
                += OnButtonPressed;

            helper.Events.GameLoop.UpdateTicked
                += OnUpdateTicked;
        }

        //----------------------------------------
        // Input
        //----------------------------------------

        /// <summary>
        /// Time Skipキーが押された時の処理です。
        /// </summary>
        private static void OnButtonPressed(
            object? sender,
            ButtonPressedEventArgs e)
        {
            //----------------------------------------
            // セーブ未ロード時は対象外
            //----------------------------------------

            if (!Context.IsWorldReady)
                return;

            //----------------------------------------
            // ホスト以外は対象外
            //----------------------------------------

            if (!Context.IsMainPlayer)
                return;

            //----------------------------------------
            // イベント・メニュー中は対象外
            //----------------------------------------

            if (Game1.eventUp
                || Game1.activeClickableMenu != null)
            {
                return;
            }

            //----------------------------------------
            // Time Skip実行中は対象外
            //----------------------------------------

            if (isSkipping)
                return;

            //----------------------------------------
            // Time Skipキー判定
            //----------------------------------------

            ModConfig config =
                ModEntry.Config;

            if (!config.TimeSkipKey
                .JustPressed())
            {
                return;
            }

            //----------------------------------------
            // キー入力を抑制
            //----------------------------------------

            helper.Input.SuppressActiveKeybinds(
                config.TimeSkipKey);

            //----------------------------------------
            // Time Skip開始
            //----------------------------------------

            StartSkip(
                config.TimeSkipTime);
        }

        //----------------------------------------
        // Time Skip開始
        //----------------------------------------

        /// <summary>
        /// Time Skipを開始します。
        /// </summary>
        private static void StartSkip(
            int newTargetTime)
        {
            //----------------------------------------
            // 未来方向のみ
            //----------------------------------------

            if (Game1.timeOfDay >= newTargetTime)
                return;

            targetTime =
                newTargetTime;

            isSkipping =
                true;

            waitingForSchedule =
                false;

            //----------------------------------------
            // 開始SE
            //----------------------------------------

            Game1.playSound(
                "select");
        }

        //----------------------------------------
        // Update
        //----------------------------------------

        /// <summary>
        /// Time Skip実行中の処理です。
        /// </summary>
        private static void OnUpdateTicked(
            object? sender,
            UpdateTickedEventArgs e)
        {
            if (!isSkipping)
                return;

            //----------------------------------------
            // セーブが閉じられた場合は中止
            //----------------------------------------

            if (!Context.IsWorldReady)
            {
                StopSkip();
                return;
            }

            //----------------------------------------
            // Schedule完了待ち
            //----------------------------------------

            if (waitingForSchedule)
            {
                bool hasActiveSchedule =
                    false;

                foreach (NPC npc
                    in Utility.getAllVillagers())
                {
                    //----------------------------------------
                    // Schedule遷移待ち
                    //----------------------------------------

                    if (IsScheduleTransitionPending(
                        npc))
                    {
                        hasActiveSchedule =
                            true;

                        TryFinishScheduleTransition(
                            npc);
                    }

                    //----------------------------------------
                    // Schedule移動中
                    //----------------------------------------

                    if (IsScheduleMoving(
                        npc))
                    {
                        hasActiveSchedule =
                            true;

                        TryHurryNpc(
                            npc);
                    }
                }

                if (hasActiveSchedule)
                    return;

                waitingForSchedule =
                    false;
            }

            //----------------------------------------
            // 目標時刻へ到達
            //----------------------------------------

            if (Game1.timeOfDay >= targetTime)
            {
                Game1.timeOfDay =
                    targetTime;

                CompleteSkip();
                return;
            }

            //----------------------------------------
            // 10分進める
            //----------------------------------------

            int nextTime =
                AddTenMinutes(
                    Game1.timeOfDay);

            if (nextTime > targetTime)
            {
                nextTime =
                    targetTime;
            }

            Game1.timeOfDay =
                nextTime;

            //----------------------------------------
            // NPCのScheduleを更新
            //----------------------------------------

            bool startedSchedule =
                false;

            foreach (NPC npc
                in Utility.getAllVillagers())
            {
                npc.checkSchedule(
                    nextTime);

                //----------------------------------------
                // Schedule遷移待ち
                //----------------------------------------

                if (IsScheduleTransitionPending(
                    npc))
                {
                    startedSchedule =
                        true;

                    TryFinishScheduleTransition(
                        npc);
                }

                //----------------------------------------
                // Schedule移動中
                //----------------------------------------

                if (IsScheduleMoving(
                    npc))
                {
                    startedSchedule =
                        true;

                    TryHurryNpc(
                        npc);
                }
            }

            //----------------------------------------
            // Schedule処理がある場合は完了を待つ
            //----------------------------------------

            if (startedSchedule)
            {
                waitingForSchedule =
                    true;
            }
        }

        //----------------------------------------
        // Schedule状態
        //----------------------------------------

        /// <summary>
        /// NPCがScheduleによる移動中か判定します。
        /// </summary>
        private static bool IsScheduleMoving(
            NPC npc)
        {
            if (npc.DirectionsToNewLocation == null)
                return false;

            if (npc.controller == null)
                return false;

            if (!npc.controller.NPCSchedule)
                return false;

            if (npc.controller.pathToEndPoint == null)
                return false;

            return
                npc.controller.pathToEndPoint.Count > 0;
        }

        /// <summary>
        /// NPCがSchedule遷移待ちか判定します。
        /// </summary>
        private static bool IsScheduleTransitionPending(
            NPC npc)
        {
            if (npc.queuedSchedulePaths == null)
                return false;

            if (npc.queuedSchedulePaths.Count == 0)
                return false;

            return
                npc.IsWalkingInSquare;
        }

        //----------------------------------------
        // Schedule遷移
        //----------------------------------------

        /// <summary>
        /// NPCのSchedule遷移待ちを高速処理します。
        /// </summary>
        private static void TryFinishScheduleTransition(
            NPC npc)
        {
            //----------------------------------------
            // 次のScheduleが待機していない場合は対象外
            //----------------------------------------

            if (npc.queuedSchedulePaths == null
                || npc.queuedSchedulePaths.Count == 0)
            {
                return;
            }

            //----------------------------------------
            // 四角歩行終了待ちのみ処理
            //----------------------------------------

            if (!npc.IsWalkingInSquare)
                return;

            //----------------------------------------
            // 四角歩行の終了地点まで高速移動
            //----------------------------------------

            int maxUpdates =
                GetMaxUpdates();

            for (int i = 0;
                i < maxUpdates;
                i++)
            {
                if (!npc.IsWalkingInSquare)
                    break;

                npc.returnToEndPoint();

                npc.MovePosition(
                    Game1.currentGameTime,
                    Game1.viewport,
                    npc.currentLocation);
            }

            //----------------------------------------
            // 四角歩行が終了したら
            // 待機中のScheduleを開始
            //----------------------------------------

            if (!npc.IsWalkingInSquare)
            {
                npc.checkSchedule(
                    Game1.timeOfDay);
            }
        }

        //----------------------------------------
        // NPC Hurry
        //----------------------------------------

        /// <summary>
        /// NPCのSchedule移動を高速化します。
        /// </summary>
        private static void TryHurryNpc(
            NPC npc)
        {
            //----------------------------------------
            // VanillaのHurry処理
            //----------------------------------------

            try
            {
                npc.warpToPathControllerDestination();
            }
            catch (InvalidOperationException)
            {
                //----------------------------------------
                // handleWarps()等によって
                // 経路Stackが空になる場合がある
                //----------------------------------------

                if (npc.controller?.pathToEndPoint == null
                    || npc.controller.pathToEndPoint.Count == 0)
                {
                    return;
                }

                //----------------------------------------
                // Stackが残っている場合は
                // 想定外なので再送出
                //----------------------------------------

                throw;
            }

            //----------------------------------------
            // 残ったSchedule経路を高速消化
            //----------------------------------------

            int maxUpdates =
                GetMaxUpdates();

            for (int i = 0;
                i < maxUpdates;
                i++)
            {
                if (!IsScheduleMoving(
                    npc))
                {
                    break;
                }

                try
                {
                    npc.controller!.update(
                        Game1.currentGameTime);
                }
                catch (InvalidOperationException)
                {
                    //----------------------------------------
                    // update中にScheduleが完了して
                    // Stackが空になる場合がある
                    //----------------------------------------

                    if (npc.controller?.pathToEndPoint == null
                        || npc.controller.pathToEndPoint.Count == 0)
                    {
                        break;
                    }

                    //----------------------------------------
                    // Stackが残っている場合は
                    // 想定外なので再送出
                    //----------------------------------------

                    throw;
                }
            }
        }

        //----------------------------------------
        // Time Skip速度
        //----------------------------------------

        /// <summary>
        /// Time Skip中のNPC Schedule更新回数を取得します。
        /// </summary>
        private static int GetMaxUpdates()
        {
            return ModEntry.Config.TimeSkipSpeed switch
            {
                TimeSkipSpeed.Slow =>
                    32,

                TimeSkipSpeed.Normal =>
                    64,

                TimeSkipSpeed.Fast =>
                    128,

                TimeSkipSpeed.VeryFast =>
                    256,

                _ =>
                    64
            };
        }

        //----------------------------------------
        // Time Skip完了
        //----------------------------------------

        /// <summary>
        /// Time Skipを正常完了します。
        /// </summary>
        private static void CompleteSkip()
        {
            isSkipping =
                false;

            waitingForSchedule =
                false;

            //----------------------------------------
            // 完了通知
            //----------------------------------------

            if (!ModEntry.Config.TimeSkipNotification)
                return;

            NotificationRequest.Theme(
                nameof(NotificationThemes.Lavender),
                helper.Translation.Get(
                    "notification.TimeSkipComplete",
                    new
                    {
                        Time =
                            FormatTime(
                                targetTime)
                    }),
                120)
                .Show();
        }

        //----------------------------------------
        // Time Skip終了
        //----------------------------------------

        /// <summary>
        /// Time Skipを終了します。
        /// </summary>
        private static void StopSkip()
        {
            isSkipping =
                false;

            waitingForSchedule =
                false;
        }

        //----------------------------------------
        // Time
        //----------------------------------------

        /// <summary>
        /// ゲーム内時刻を表示用文字列へ変換します。
        /// </summary>
        private static string FormatTime(
            int time)
        {
            int hour =
                time / 100;

            int minute =
                time % 100;

            return
                $"{hour}:{minute:00}";
        }

        /// <summary>
        /// ゲーム内時刻を10分進めます。
        /// </summary>
        private static int AddTenMinutes(
            int time)
        {
            int hour =
                time / 100;

            int minute =
                time % 100;

            minute += 10;

            if (minute >= 60)
            {
                minute -= 60;
                hour++;
            }

            return
                hour * 100 + minute;
        }
    }
}