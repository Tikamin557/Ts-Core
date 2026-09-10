using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Machines;
using Ts_Core.Models.MachineRelated;

namespace Ts_Core.Services.MachineRelated
{
    /// <summary>
    /// Machine Interactionの
    /// Texture / Animationを管理するサービスです。
    /// </summary>
    public static class MachineAnimationTextureService
    {
        //----------------------------------------
        // Monitor
        //----------------------------------------

        /// <summary>
        /// Log出力に使用するMonitorです。
        /// </summary>
        private static IMonitor? Monitor;

        //----------------------------------------
        // Logged Warnings
        //----------------------------------------

        /// <summary>
        /// すでに表示した警告を管理します。
        /// </summary>
        private static readonly HashSet<string>
            LoggedWarnings =
                new();

        //----------------------------------------
        // Action Animation Start Times
        //----------------------------------------

        /// <summary>
        /// Action Texture / Animationの
        /// 開始時刻をMachineごとに管理します。
        /// </summary>
        private static readonly Dictionary<
            StardewValley.Object,
            double> ActionAnimationStartTimes =
                new();

        //----------------------------------------
        // Initialize
        //----------------------------------------

        /// <summary>
        /// サービスを初期化します。
        /// </summary>
        public static void Initialize(
            IMonitor monitor)
        {
            Monitor =
                monitor;
        }

        //----------------------------------------
        // Texture Position
        //----------------------------------------

        /// <summary>
        /// Textureの基準Pixel座標を取得します。
        /// </summary>
        private static bool TryGetTexturePosition(
            string? value,
            out Point position)
        {
            position =
                Point.Zero;

            if (string.IsNullOrWhiteSpace(
                value))
            {
                return true;
            }

            string[] parts =
                value.Split(
                    ',');

            if (parts.Length != 2)
                return false;

            if (!int.TryParse(
                parts[0].Trim(),
                out int x))
            {
                return false;
            }

            if (!int.TryParse(
                parts[1].Trim(),
                out int y))
            {
                return false;
            }

            position =
                new Point(
                    x,
                    y);

            return true;
        }

        //----------------------------------------
        // Frame Durations
        //----------------------------------------

        /// <summary>
        /// Frame Duration設定を取得します。
        /// </summary>
        public static bool TryGetFrameDurations(
            string interactionId,
            string animationType,
            object? value,
            int frameCount,
            out List<int> durations)
        {
            durations =
                new();

            if (frameCount <= 0)
                return false;

            //----------------------------------------
            // 全Frame共通
            //----------------------------------------

            if (value is long singleDuration)
            {
                if (singleDuration <= 0
                    || singleDuration > int.MaxValue)
                {
                    LogWarningOnce(
                        $"{interactionId}:{animationType}:InvalidDuration",
                        $"Machine Interaction '{interactionId}': "
                        + $"{animationType}FrameDurationMs must be greater than 0. "
                        + $"The {animationType} animation will be disabled.");

                    return false;
                }

                for (int i = 0;
                    i < frameCount;
                    i++)
                {
                    durations.Add(
                        (int)singleDuration);
                }

                return true;
            }

            //----------------------------------------
            // Frame個別
            //----------------------------------------

            if (value is System.Collections.IEnumerable enumerable
                && value is not string)
            {
                List<int> values =
                    new();

                foreach (object? item
                    in enumerable)
                {
                    if (item == null)
                    {
                        LogWarningOnce(
                            $"{interactionId}:{animationType}:InvalidDurationType",
                            $"Machine Interaction '{interactionId}': "
                            + $"{animationType}FrameDurationMs contains an invalid value. "
                            + $"The {animationType} animation will be disabled.");

                        return false;
                    }

                    if (!long.TryParse(
                        item.ToString(),
                        out long duration))
                    {
                        LogWarningOnce(
                            $"{interactionId}:{animationType}:InvalidDurationType",
                            $"Machine Interaction '{interactionId}': "
                            + $"{animationType}FrameDurationMs contains an invalid value. "
                            + $"The {animationType} animation will be disabled.");

                        return false;
                    }

                    if (duration <= 0
                        || duration > int.MaxValue)
                    {
                        LogWarningOnce(
                            $"{interactionId}:{animationType}:InvalidDuration",
                            $"Machine Interaction '{interactionId}': "
                            + $"{animationType}FrameDurationMs values must be greater than 0. "
                            + $"The {animationType} animation will be disabled.");

                        return false;
                    }

                    values.Add(
                        (int)duration);
                }

                if (values.Count != frameCount)
                {
                    LogWarningOnce(
                        $"{interactionId}:{animationType}:DurationCountMismatch",
                        $"Machine Interaction '{interactionId}': "
                        + $"{animationType}Frames has {frameCount} entries, "
                        + $"but {animationType}FrameDurationMs has {values.Count}. "
                        + $"The {animationType} animation will be disabled.");

                    return false;
                }

                durations =
                    values;

                return true;
            }

            //----------------------------------------
            // 未設定 / 不正型
            //----------------------------------------

            LogWarningOnce(
                $"{interactionId}:{animationType}:InvalidDurationType",
                $"Machine Interaction '{interactionId}': "
                + $"{animationType}FrameDurationMs has an invalid format. "
                + $"The {animationType} animation will be disabled.");

            return false;
        }

        //----------------------------------------
        // Warning
        //----------------------------------------

        /// <summary>
        /// 同じ警告を1度だけ表示します。
        /// </summary>
        private static void LogWarningOnce(
            string key,
            string message)
        {
            if (!LoggedWarnings.Add(
                key))
            {
                return;
            }

            Monitor?.Log(
                message,
                LogLevel.Warn);
        }

        //----------------------------------------
        // Start Action
        //----------------------------------------

        /// <summary>
        /// Action Texture / Animationの
        /// 表示を開始します。
        /// </summary>
        public static void StartAction(
            StardewValley.Object machine,
            string interactionId,
            MachineInteractionData interaction)
        {
            //----------------------------------------
            // Texture
            //----------------------------------------

            if (string.IsNullOrWhiteSpace(
                interaction.ActionTexture))
            {
                return;
            }

            //----------------------------------------
            // Animation
            //----------------------------------------

            if (interaction.ActionFrames != null
                && interaction.ActionFrames.Count > 0)
            {
                if (!TryGetFrameDurations(
                    interactionId,
                    "Action",
                    interaction.ActionFrameDurationMs,
                    interaction.ActionFrames.Count,
                    out _))
                {
                    return;
                }
            }

            //----------------------------------------
            // Static Texture
            //----------------------------------------

            else
            {
                if (interaction.ActionDurationMs <= 0)
                {
                    LogWarningOnce(
                        $"{interactionId}:Action:InvalidDuration",
                        $"Machine Interaction '{interactionId}': "
                        + "ActionDurationMs must be greater than 0. "
                        + "The Action texture will be disabled.");

                    return;
                }
            }

            //----------------------------------------
            // Start Time
            //----------------------------------------

            ActionAnimationStartTimes[machine] =
                Game1.currentGameTime
                    .TotalGameTime
                    .TotalMilliseconds;
        }

        //----------------------------------------
        // Action Frame
        //----------------------------------------

        /// <summary>
        /// Action Texture / Animationの
        /// 現在のFrameを取得します。
        /// </summary>
        public static bool TryGetActionFrame(
            StardewValley.Object machine,
            string interactionId,
            MachineInteractionData interaction,
            out int frame)
        {
            frame =
                0;

            //----------------------------------------
            // Start Time
            //----------------------------------------

            if (!ActionAnimationStartTimes.TryGetValue(
                machine,
                out double startTime))
            {
                return false;
            }

            double elapsed =
                Game1.currentGameTime
                    .TotalGameTime
                    .TotalMilliseconds
                - startTime;

            //----------------------------------------
            // Static Texture
            //----------------------------------------

            if (interaction.ActionFrames == null
                || interaction.ActionFrames.Count == 0)
            {
                if (interaction.ActionDurationMs <= 0)
                {
                    ActionAnimationStartTimes.Remove(
                        machine);

                    return false;
                }

                if (elapsed >=
                    interaction.ActionDurationMs)
                {
                    ActionAnimationStartTimes.Remove(
                        machine);

                    return false;
                }

                frame =
                    0;

                return true;
            }

            //----------------------------------------
            // Animation Duration
            //----------------------------------------

            if (!TryGetFrameDurations(
                interactionId,
                "Action",
                interaction.ActionFrameDurationMs,
                interaction.ActionFrames.Count,
                out List<int> durations))
            {
                ActionAnimationStartTimes.Remove(
                    machine);

                return false;
            }

            //----------------------------------------
            // Current Frame
            //----------------------------------------

            double total =
                0;

            for (int i = 0;
                i < durations.Count;
                i++)
            {
                total +=
                    durations[i];

                if (elapsed < total)
                {
                    frame =
                        interaction.ActionFrames[i];

                    return true;
                }
            }

            //----------------------------------------
            // End
            //----------------------------------------

            ActionAnimationStartTimes.Remove(
                machine);

            return false;
        }

        //----------------------------------------
        // Action Draw Data
        //----------------------------------------

        /// <summary>
        /// Machineから現在のAction Texture / Animationの
        /// 描画情報を取得します。
        /// </summary>
        public static bool TryGetActionDrawData(
            StardewValley.Object machine,
            out Texture2D? texture,
            out Rectangle sourceRect)
        {
            texture =
                null;

            sourceRect =
                Rectangle.Empty;

            //----------------------------------------
            // Machine Data取得
            //----------------------------------------

            MachineData? machineData =
                machine.GetMachineData();

            if (machineData?.CustomFields == null)
                return false;

            //----------------------------------------
            // Interaction ID取得
            //----------------------------------------

            if (!machineData.CustomFields.TryGetValue(
                MachineInteractionDataService.InteractionField,
                out string? interactionId)
                || string.IsNullOrWhiteSpace(
                    interactionId))
            {
                return false;
            }

            //----------------------------------------
            // Interaction Data取得
            //----------------------------------------

            Dictionary<string, MachineInteractionData>
                interactionData =
                    Game1.content.Load<
                        Dictionary<
                            string,
                            MachineInteractionData>>(
                                MachineInteractionDataService.AssetName);

            if (!interactionData.TryGetValue(
                interactionId,
                out MachineInteractionData? interaction))
            {
                return false;
            }

            //----------------------------------------
            // Action描画情報取得
            //----------------------------------------

            return TryGetActionDrawData(
                machine,
                interactionId,
                interaction,
                out texture,
                out sourceRect);
        }

        /// <summary>
        /// 現在のAction Texture / Animationの
        /// 描画情報を取得します。
        /// </summary>
        public static bool TryGetActionDrawData(
            StardewValley.Object machine,
            string interactionId,
            MachineInteractionData interaction,
            out Texture2D? texture,
            out Rectangle sourceRect)
        {
            texture =
                null;

            sourceRect =
                Rectangle.Empty;

            //----------------------------------------
            // Texture確認
            //----------------------------------------

            if (string.IsNullOrWhiteSpace(
                interaction.ActionTexture))
            {
                return false;
            }

            //----------------------------------------
            // 現在Frame取得
            //----------------------------------------

            if (!TryGetActionFrame(
                machine,
                interactionId,
                interaction,
                out int frame))
            {
                return false;
            }

            //----------------------------------------
            // Texture Position
            //----------------------------------------

            if (!TryGetTexturePosition(
                interaction.ActionTexturePosition,
                out Point position))
            {
                LogWarningOnce(
                    $"{interactionId}:Action:InvalidTexturePosition",
                    $"Machine Interaction '{interactionId}': "
                    + "ActionTexturePosition has an invalid format. "
                    + "The Action texture will be disabled.");

                return false;
            }

            //----------------------------------------
            // Texture読み込み
            //----------------------------------------

            try
            {
                texture =
                    Game1.content.Load<Texture2D>(
                        interaction.ActionTexture);
            }
            catch (Exception ex)
            {
                LogWarningOnce(
                    $"{interactionId}:Action:TextureLoad",
                    $"Machine Interaction '{interactionId}': "
                    + $"failed to load ActionTexture "
                    + $"'{interaction.ActionTexture}'. "
                    + $"The Action texture will be disabled. "
                    + $"({ex.Message})");

                return false;
            }

            //----------------------------------------
            // Source Rectangle
            //----------------------------------------

            sourceRect =
                new Rectangle(
                    position.X + frame * 16,
                    position.Y,
                    16,
                    32);

            return true;
        }

        //----------------------------------------
        // Idle Frame
        //----------------------------------------

        /// <summary>
        /// Idle Texture / Animationの
        /// 現在のFrameを取得します。
        /// </summary>
        public static bool TryGetIdleFrame(
            StardewValley.Object machine,
            string interactionId,
            MachineInteractionData interaction,
            out int frame)
        {
            frame =
                0;

            //----------------------------------------
            // Texture
            //----------------------------------------

            if (string.IsNullOrWhiteSpace(
                interaction.IdleTexture))
            {
                return false;
            }

            //----------------------------------------
            // Idle状態確認
            //----------------------------------------

            if (machine.readyForHarvest.Value)
                return false;

            if (machine.minutesUntilReady.Value > 0)
                return false;

            //----------------------------------------
            // Static Texture
            //----------------------------------------

            if (interaction.IdleFrames == null
                || interaction.IdleFrames.Count == 0)
            {
                frame =
                    0;

                return true;
            }

            //----------------------------------------
            // Animation Duration
            //----------------------------------------

            if (!TryGetFrameDurations(
                interactionId,
                "Idle",
                interaction.IdleFrameDurationMs,
                interaction.IdleFrames.Count,
                out List<int> durations))
            {
                return false;
            }

            //----------------------------------------
            // Total Duration
            //----------------------------------------

            long totalDuration =
                0;

            foreach (int duration
                in durations)
            {
                totalDuration +=
                    duration;
            }

            if (totalDuration <= 0)
                return false;

            //----------------------------------------
            // Current Animation Time
            //----------------------------------------

            double elapsed =
                Game1.currentGameTime
                    .TotalGameTime
                    .TotalMilliseconds;

            double animationTime =
                elapsed % totalDuration;

            long current =
                0;

            //----------------------------------------
            // Current Frame
            //----------------------------------------

            for (int i = 0;
                i < durations.Count;
                i++)
            {
                current +=
                    durations[i];

                if (animationTime < current)
                {
                    frame =
                        interaction.IdleFrames[i];

                    return true;
                }
            }

            return false;
        }

        //----------------------------------------
        // Idle Draw Data
        //----------------------------------------

        /// <summary>
        /// Machineから現在のIdle Texture / Animationの
        /// 描画情報を取得します。
        /// </summary>
        public static bool TryGetIdleDrawData(
            StardewValley.Object machine,
            out Texture2D? texture,
            out Rectangle sourceRect)
        {
            texture =
                null;

            sourceRect =
                Rectangle.Empty;

            //----------------------------------------
            // Machine Data取得
            //----------------------------------------

            MachineData? machineData =
                machine.GetMachineData();

            if (machineData?.CustomFields == null)
                return false;

            //----------------------------------------
            // Interaction ID取得
            //----------------------------------------

            if (!machineData.CustomFields.TryGetValue(
                MachineInteractionDataService.InteractionField,
                out string? interactionId)
                || string.IsNullOrWhiteSpace(
                    interactionId))
            {
                return false;
            }

            //----------------------------------------
            // Interaction Data取得
            //----------------------------------------

            Dictionary<string, MachineInteractionData>
                interactionData =
                    Game1.content.Load<
                        Dictionary<
                            string,
                            MachineInteractionData>>(
                                MachineInteractionDataService.AssetName);

            if (!interactionData.TryGetValue(
                interactionId,
                out MachineInteractionData? interaction))
            {
                return false;
            }

            //----------------------------------------
            // Idle描画情報取得
            //----------------------------------------

            return TryGetIdleDrawData(
                machine,
                interactionId,
                interaction,
                out texture,
                out sourceRect);
        }

        /// <summary>
        /// 現在のIdle Texture / Animationの
        /// 描画情報を取得します。
        /// </summary>
        public static bool TryGetIdleDrawData(
            StardewValley.Object machine,
            string interactionId,
            MachineInteractionData interaction,
            out Texture2D? texture,
            out Rectangle sourceRect)
        {
            texture =
                null;

            sourceRect =
                Rectangle.Empty;

            //----------------------------------------
            // Texture確認
            //----------------------------------------

            if (string.IsNullOrWhiteSpace(
                interaction.IdleTexture))
            {
                return false;
            }

            //----------------------------------------
            // 現在Frame取得
            //----------------------------------------

            if (!TryGetIdleFrame(
                machine,
                interactionId,
                interaction,
                out int frame))
            {
                return false;
            }

            //----------------------------------------
            // Texture Position
            //----------------------------------------

            if (!TryGetTexturePosition(
                interaction.IdleTexturePosition,
                out Point position))
            {
                LogWarningOnce(
                    $"{interactionId}:Idle:InvalidTexturePosition",
                    $"Machine Interaction '{interactionId}': "
                    + "IdleTexturePosition has an invalid format. "
                    + "The Idle texture will be disabled.");

                return false;
            }

            //----------------------------------------
            // Texture読み込み
            //----------------------------------------

            try
            {
                texture =
                    Game1.content.Load<Texture2D>(
                        interaction.IdleTexture);
            }
            catch (Exception ex)
            {
                LogWarningOnce(
                    $"{interactionId}:Idle:TextureLoad",
                    $"Machine Interaction '{interactionId}': "
                    + $"failed to load IdleTexture "
                    + $"'{interaction.IdleTexture}'. "
                    + $"The Idle texture will be disabled. "
                    + $"({ex.Message})");

                return false;
            }

            //----------------------------------------
            // Source Rectangle
            //----------------------------------------

            sourceRect =
                new Rectangle(
                    position.X + frame * 16,
                    position.Y,
                    16,
                    32);

            return true;
        }

        //----------------------------------------
        // Clear
        //----------------------------------------

        /// <summary>
        /// Texture / Animationの管理情報を
        /// 全て削除します。
        /// </summary>
        public static void Clear()
        {
            ActionAnimationStartTimes.Clear();

            LoggedWarnings.Clear();
        }
    }
}