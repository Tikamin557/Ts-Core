using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using Ts_Core.Models.BigCraftableRelated;

namespace Ts_Core.Services.BigCraftableRelated
{
    /// <summary>
    /// BigCraftable Extensionの
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
        /// 開始時刻をBigCraftableごとに管理します。
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
        // Texture Size
        //----------------------------------------

        /// <summary>
        /// Texture Size設定を取得します。
        /// </summary>
        private static bool TryGetTextureSize(
            string extensionId,
            BigCraftableExtensionData extension,
            out int width,
            out int height)
        {
            width =
                extension.TextureWidth;

            height =
                extension.TextureHeight;

            if (width <= 0
                || height <= 0)
            {
                LogWarningOnce(
                    $"{extensionId}:InvalidTextureSize",
                    $"BigCraftable Extension '{extensionId}': "
                    + "TextureWidth and TextureHeight must be greater than 0. "
                    + "The custom texture will be disabled.");

                return false;
            }

            return true;
        }

        //----------------------------------------
        // Frame Durations
        //----------------------------------------

        /// <summary>
        /// Frame Duration設定を取得します。
        /// </summary>
        public static bool TryGetFrameDurations(
            string extensionId,
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
                        $"{extensionId}:{animationType}:InvalidDuration",
                        $"BigCraftable Extension '{extensionId}': "
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
                            $"{extensionId}:{animationType}:InvalidDurationType",
                            $"BigCraftable Extension '{extensionId}': "
                            + $"{animationType}FrameDurationMs contains an invalid value. "
                            + $"The {animationType} animation will be disabled.");

                        return false;
                    }

                    if (!long.TryParse(
                        item.ToString(),
                        out long duration))
                    {
                        LogWarningOnce(
                            $"{extensionId}:{animationType}:InvalidDurationType",
                            $"BigCraftable Extension '{extensionId}': "
                            + $"{animationType}FrameDurationMs contains an invalid value. "
                            + $"The {animationType} animation will be disabled.");

                        return false;
                    }

                    if (duration <= 0
                        || duration > int.MaxValue)
                    {
                        LogWarningOnce(
                            $"{extensionId}:{animationType}:InvalidDuration",
                            $"BigCraftable Extension '{extensionId}': "
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
                        $"{extensionId}:{animationType}:DurationCountMismatch",
                        $"BigCraftable Extension '{extensionId}': "
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
                $"{extensionId}:{animationType}:InvalidDurationType",
                $"BigCraftable Extension '{extensionId}': "
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
            string extensionId,
            BigCraftableExtensionData extension)
        {
            //----------------------------------------
            // Texture
            //----------------------------------------

            if (string.IsNullOrWhiteSpace(
                extension.ActionTexture))
            {
                return;
            }

            //----------------------------------------
            // Animation
            //----------------------------------------

            if (extension.ActionFrames != null
                && extension.ActionFrames.Count > 0)
            {
                if (!TryGetFrameDurations(
                    extensionId,
                    "Action",
                    extension.ActionFrameDurationMs,
                    extension.ActionFrames.Count,
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
                if (extension.ActionDurationMs <= 0)
                {
                    LogWarningOnce(
                        $"{extensionId}:Action:InvalidDuration",
                        $"BigCraftable Extension '{extensionId}': "
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
            string extensionId,
            BigCraftableExtensionData extension,
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

            if (extension.ActionFrames == null
                || extension.ActionFrames.Count == 0)
            {
                if (extension.ActionDurationMs <= 0)
                {
                    ActionAnimationStartTimes.Remove(
                        machine);

                    return false;
                }

                if (elapsed >=
                    extension.ActionDurationMs)
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
                extensionId,
                "Action",
                extension.ActionFrameDurationMs,
                extension.ActionFrames.Count,
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
                        extension.ActionFrames[i];

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
        /// 現在のAction Texture / Animationの
        /// 描画情報を取得します。
        /// </summary>
        public static bool TryGetActionDrawData(
            StardewValley.Object machine,
            string extensionId,
            BigCraftableExtensionData extension,
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
                extension.ActionTexture))
            {
                return false;
            }

            //----------------------------------------
            // 現在Frame取得
            //----------------------------------------

            if (!TryGetActionFrame(
                machine,
                extensionId,
                extension,
                out int frame))
            {
                return false;
            }

            //----------------------------------------
            // Texture Position
            //----------------------------------------

            if (!TryGetTexturePosition(
                extension.ActionTexturePosition,
                out Point position))
            {
                LogWarningOnce(
                    $"{extensionId}:Action:InvalidTexturePosition",
                    $"BigCraftable Extension '{extensionId}': "
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
                        extension.ActionTexture);
            }
            catch (Exception ex)
            {
                LogWarningOnce(
                    $"{extensionId}:Action:TextureLoad",
                    $"BigCraftable Extension '{extensionId}': "
                    + $"failed to load ActionTexture "
                    + $"'{extension.ActionTexture}'. "
                    + $"The Action texture will be disabled. "
                    + $"({ex.Message})");

                return false;
            }

            //----------------------------------------
            // Texture Size
            //----------------------------------------

            if (!TryGetTextureSize(
                extensionId,
                extension,
                out int width,
                out int height))
            {
                return false;
            }

            //----------------------------------------
            // Source Rectangle
            //----------------------------------------

            sourceRect =
                new Rectangle(
                    position.X
                        + frame * width,
                    position.Y,
                    width,
                    height);

            return true;
        }

        //----------------------------------------
        // Normal Frame
        //----------------------------------------

        /// <summary>
        /// 通常Texture / Animationの
        /// 現在のFrameを取得します。
        /// </summary>
        public static bool TryGetNormalFrame(
            string extensionId,
            BigCraftableExtensionData extension,
            out int frame)
        {
            frame =
                0;

            //----------------------------------------
            // Static Texture
            //----------------------------------------

            if (extension.Frames == null
                || extension.Frames.Count == 0)
            {
                frame =
                    0;

                return true;
            }

            //----------------------------------------
            // Animation Duration
            //----------------------------------------

            if (!TryGetFrameDurations(
                extensionId,
                string.Empty,
                extension.FrameDurationMs,
                extension.Frames.Count,
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
                        extension.Frames[i];

                    return true;
                }
            }

            return false;
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