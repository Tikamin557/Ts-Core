using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using Ts_Core.Actions;

namespace Ts_Core.Services.MapRelated.TimedExit
{
    /// <summary>
    /// マッププロパティで指定された時刻以降に、
    /// メッセージ表示後または即座にプレイヤーを退出させます。
    /// </summary>
    internal static class TimedExitService
    {
        //----------------------------------------
        // 定数
        //----------------------------------------

        private const string TimedExitProperty =
            "TsCoreTimedExit";

        private const string TimedExitMessageProperty =
            "TsCoreTimedExitMessage";

        private const string TimedExitSoundProperty =
            "TsCoreTimedExitSound";

        //----------------------------------------
        // 状態
        //----------------------------------------

        private static IMonitor? monitor;

        private static bool isProcessing;

        private static DialogueBox? activeDialogue;

        private static GameLocation? sourceLocation;

        private static string[]? pendingWarpAction;

        //----------------------------------------
        // 初期化
        //----------------------------------------

        /// <summary>
        /// TimedExitサービスを初期化します。
        /// </summary>
        internal static void Initialize(
            IModHelper helper,
            IMonitor modMonitor)
        {
            monitor =
                modMonitor;

            helper.Events.GameLoop.TimeChanged +=
                OnTimeChanged;

            helper.Events.Player.Warped +=
                OnWarped;

            helper.Events.GameLoop.UpdateTicked +=
                OnUpdateTicked;

            helper.Events.GameLoop.DayStarted +=
                OnDayStarted;
        }

        //----------------------------------------
        // TimeChanged
        //----------------------------------------

        /// <summary>
        /// ゲーム内時間が変更されたときに
        /// TimedExitを確認します。
        /// </summary>
        private static void OnTimeChanged(
            object? sender,
            TimeChangedEventArgs e)
        {
            CheckCurrentLocation();
        }

        //----------------------------------------
        // Warped
        //----------------------------------------

        /// <summary>
        /// プレイヤーが別ロケーションへ移動したときに
        /// TimedExitを確認します。
        /// </summary>
        private static void OnWarped(
            object? sender,
            WarpedEventArgs e)
        {
            if (!e.IsLocalPlayer)
                return;

            CheckLocation(
                e.NewLocation);
        }

        //----------------------------------------
        // UpdateTicked
        //----------------------------------------

        /// <summary>
        /// TimedExitによって表示した
        /// ダイアログが閉じられたか確認します。
        /// </summary>
        private static void OnUpdateTicked(
            object? sender,
            UpdateTickedEventArgs e)
        {
            if (!isProcessing
                || activeDialogue == null)
            {
                return;
            }

            //----------------------------------------
            // 表示中
            //----------------------------------------

            if (ReferenceEquals(
                    Game1.activeClickableMenu,
                    activeDialogue))
            {
                return;
            }

            //----------------------------------------
            // 閉じられた
            //----------------------------------------

            ExecutePendingWarp();
        }

        //----------------------------------------
        // DayStarted
        //----------------------------------------

        /// <summary>
        /// 新しい日の開始時に状態をリセットします。
        /// </summary>
        private static void OnDayStarted(
            object? sender,
            DayStartedEventArgs e)
        {
            ClearState();
        }

        //----------------------------------------
        // 現在地確認
        //----------------------------------------

        /// <summary>
        /// 現在のロケーションの
        /// TimedExitを確認します。
        /// </summary>
        private static void CheckCurrentLocation()
        {
            if (!Context.IsWorldReady)
                return;

            CheckLocation(
                Game1.currentLocation);
        }

        /// <summary>
        /// 指定ロケーションの
        /// TimedExitを確認します。
        /// </summary>
        private static void CheckLocation(
            GameLocation location)
        {
            if (isProcessing)
                return;

            if (!Context.IsWorldReady)
                return;

            //----------------------------------------
            // TimedExit取得
            //----------------------------------------

            if (!location.Map.Properties.TryGetValue(
                    TimedExitProperty,
                    out var timedExitValue))
            {
                return;
            }

            string timedExit =
                timedExitValue?.ToString()?.Trim()
                ?? string.Empty;

            if (string.IsNullOrWhiteSpace(
                    timedExit))
            {
                LogError(
                    location,
                    $"'{TimedExitProperty}' is empty.");

                return;
            }

            //----------------------------------------
            // 分解
            //----------------------------------------

            string[] parts =
                timedExit.Split(
                    ' ',
                    StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 2)
            {
                LogError(
                    location,
                    $"'{TimedExitProperty}' has an invalid value: \"{timedExit}\".");

                return;
            }

            //----------------------------------------
            // Time
            //----------------------------------------

            if (!int.TryParse(
                    parts[0],
                    out int exitTime))
            {
                LogError(
                    location,
                    $"'{TimedExitProperty}' has an invalid time: \"{parts[0]}\".");

                return;
            }

            //----------------------------------------
            // 時刻前
            //----------------------------------------

            if (Game1.timeOfDay < exitTime)
                return;

            //----------------------------------------
            // Warp Action
            //----------------------------------------

            string[] warpAction =
                parts
                    .Skip(1)
                    .ToArray();

            if (!IsSupportedWarpAction(
                    warpAction[0]))
            {
                LogError(
                    location,
                    $"'{TimedExitProperty}' has an unsupported warp action: \"{warpAction[0]}\".");

                return;
            }

            //----------------------------------------
            // Message取得
            //----------------------------------------

            if (!location.Map.Properties.TryGetValue(
                    TimedExitMessageProperty,
                    out var messageValue))
            {
                LogError(
                    location,
                    $"'{TimedExitMessageProperty}' is missing.");

                return;
            }

            string message =
                messageValue?.ToString()?.Trim()
                ?? string.Empty;

            if (string.IsNullOrWhiteSpace(
                    message))
            {
                LogError(
                    location,
                    $"'{TimedExitMessageProperty}' is empty.");

                return;
            }

            //----------------------------------------
            // false判定
            //----------------------------------------

            bool skipMessage =
                message.Equals(
                    "false",
                    StringComparison.OrdinalIgnoreCase);

            //----------------------------------------
            // Strings/StringsFromMaps
            //----------------------------------------

            if (!skipMessage
                && message.Length >= 2
                && message.StartsWith("\"")
                && message.EndsWith("\""))
            {
                string translationKey =
                    message.Substring(
                        1,
                        message.Length - 2);

                //----------------------------------------
                // Keyなし
                //----------------------------------------

                if (string.IsNullOrWhiteSpace(
                        translationKey))
                {
                    LogError(
                        location,
                        $"'{TimedExitMessageProperty}' has an empty Strings/StringsFromMaps key.");

                    return;
                }

                //----------------------------------------
                // 翻訳テキスト取得
                //----------------------------------------

                string? translatedMessage =
                    Game1.content.LoadStringReturnNullIfNotFound(
                        "Strings\\StringsFromMaps:"
                        + translationKey);

                if (translatedMessage == null)
                {
                    LogError(
                        location,
                        $"'{TimedExitMessageProperty}' references an unknown Strings/StringsFromMaps key: \"{translationKey}\".");

                    return;
                }

                message =
                    translatedMessage;
            }

            //----------------------------------------
            // Sound取得
            //----------------------------------------

            string? soundCue = null;

            if (location.Map.Properties.TryGetValue(
                    TimedExitSoundProperty,
                    out var soundValue))
            {
                string sound =
                    soundValue?.ToString()?.Trim()
                    ?? string.Empty;

                if (!string.IsNullOrWhiteSpace(
                        sound))
                {
                    soundCue =
                        sound;
                }
            }

            //----------------------------------------
            // 処理開始
            //----------------------------------------

            isProcessing = true;

            sourceLocation =
                location;

            pendingWarpAction =
                warpAction;

            //----------------------------------------
            // Sound再生
            //----------------------------------------

            if (!string.IsNullOrWhiteSpace(
                    soundCue))
            {
                try
                {
                    Game1.playSound(
                        soundCue);
                }
                catch (Exception ex)
                {
                    LogError(
                        location,
                        $"'{TimedExitSoundProperty}' has an invalid audio cue: \"{soundCue}\".");

                    monitor?.Log(
                        ex.ToString(),
                        LogLevel.Trace);
                }
            }

            //----------------------------------------
            // Messageなし
            //----------------------------------------

            if (skipMessage)
            {
                ExecutePendingWarp();

                return;
            }

            //----------------------------------------
            // Message表示
            //----------------------------------------

            Game1.drawObjectDialogue(
                message);

            activeDialogue =
                Game1.activeClickableMenu
                as DialogueBox;

            if (activeDialogue == null)
            {
                LogError(
                    location,
                    $"Failed to create the dialogue for '{TimedExitMessageProperty}'.");

                ClearState();
            }
        }

        //----------------------------------------
        // Warp Action確認
        //----------------------------------------

        /// <summary>
        /// TimedExitで使用可能な
        /// Warp Actionか確認します。
        /// </summary>
        private static bool IsSupportedWarpAction(
            string action)
        {
            return action == "TsCoreWarp"
                || action == "TsCoreMagicWarp"
                || action == "TsCoreMagicWarp_Simple";
        }

        //----------------------------------------
        // Warp実行
        //----------------------------------------

        /// <summary>
        /// 保留中のWarp Actionを実行します。
        /// </summary>
        private static void ExecutePendingWarp()
        {
            GameLocation? location =
                sourceLocation;

            string[]? action =
                pendingWarpAction;

            //----------------------------------------
            // 先に状態を解除
            //----------------------------------------

            ClearState();

            if (location == null
                || action == null)
            {
                return;
            }

            //----------------------------------------
            // Warp実行
            //----------------------------------------

            if (!ActionRegistry.ExecuteWarp(
                    location,
                    action))
            {
                monitor?.Log(
                    $"Failed to execute '{TimedExitProperty}' warp action on map '{location.NameOrUniqueName}': \"{string.Join(" ", action)}\".",
                    LogLevel.Error);
            }
        }

        //----------------------------------------
        // 状態解除
        //----------------------------------------

        /// <summary>
        /// TimedExitの実行状態を解除します。
        /// </summary>
        private static void ClearState()
        {
            isProcessing = false;

            activeDialogue = null;

            sourceLocation = null;

            pendingWarpAction = null;
        }

        //----------------------------------------
        // Error Log
        //----------------------------------------

        /// <summary>
        /// TimedExitの設定エラーをログへ出力します。
        /// </summary>
        private static void LogError(
            GameLocation location,
            string message)
        {
            monitor?.Log(
                $"TimedExit error on map '{location.NameOrUniqueName}': {message}",
                LogLevel.Error);
        }
    }
}