using Microsoft.Xna.Framework;

namespace Ts_Core.Models.BigCraftableRelated
{
    /// <summary>
    /// BigCraftable Extensionに追加する
    /// DrawLayerの定義です。
    /// </summary>
    public sealed class BigCraftableDrawLayerData
    {
        //----------------------------------------
        // Basic
        //----------------------------------------

        /// <summary>
        /// DrawLayer IDです。
        /// </summary>
        public string Id { get; set; } = string.Empty;

        //----------------------------------------
        // Texture
        //----------------------------------------

        /// <summary>
        /// 使用するTextureです。
        ///
        /// 未指定の場合は
        /// BigCraftable本体のTextureを使用します。
        /// </summary>
        public string? Texture { get; set; }

        /// <summary>
        /// DrawLayerのTextureから使用する
        /// 基準Source Rectangleです。
        /// Animation時はこの位置を
        /// 第1Frameとして使用します。
        /// </summary>
        public Rectangle SourceRect { get; set; }
            = Rectangle.Empty;

        //----------------------------------------
        // Draw
        //----------------------------------------

        /// <summary>
        /// BigCraftable本体の描画位置を基準とした
        /// 描画位置です。
        /// </summary>
        public Vector2 DrawPosition { get; set; }
            = Vector2.Zero;

        /// <summary>
        /// 描画に使用するLayerです。
        /// "Back" または "Front" を指定します。
        /// </summary>
        public string DrawLayer { get; set; } = "Back";

        //----------------------------------------
        // Animation
        //----------------------------------------

        /// <summary>
        /// アニメーションのフレーム数です。
        /// </summary>
        public int FrameCount { get; set; } = 1;

        /// <summary>
        /// Texture内の1行あたりのフレーム数です。
        ///
        /// -1の場合は横一列として扱います。
        /// </summary>
        public int FramesPerRow { get; set; } = -1;

        /// <summary>
        /// フレームごとの表示時間です。
        ///
        /// 数値1つ、
        /// またはFrameCountと同数の配列を指定できます。
        /// </summary>
        public object FrameDuration { get; set; } = 90;

        //----------------------------------------
        // Frame Duration
        //----------------------------------------

        /// <summary>
        /// 指定したフレーム番号の
        /// FrameDurationを取得します。
        /// </summary>
        public int GetFrameDuration(
            int frameIndex)
        {
            //----------------------------------------
            // 配列
            //----------------------------------------

            if (FrameDuration
                is IEnumerable<object> values)
            {
                object[] durations =
                    values.ToArray();

                if (durations.Length
                    == FrameCount
                    && frameIndex >= 0
                    && frameIndex < durations.Length)
                {
                    if (int.TryParse(
                            durations[frameIndex]
                                ?.ToString(),
                            out int duration))
                    {
                        return duration;
                    }
                }
            }

            //----------------------------------------
            // IEnumerable
            //----------------------------------------

            if (FrameDuration
                is System.Collections.IEnumerable enumerable
                && FrameDuration is not string)
            {
                List<object> durations =
                    new();

                foreach (object? value
                    in enumerable)
                {
                    if (value != null)
                    {
                        durations.Add(
                            value);
                    }
                }

                if (durations.Count
                    == FrameCount
                    && frameIndex >= 0
                    && frameIndex < durations.Count)
                {
                    if (int.TryParse(
                            durations[frameIndex]
                                .ToString(),
                            out int duration))
                    {
                        return duration;
                    }
                }
            }

            //----------------------------------------
            // 単一値
            //----------------------------------------

            if (int.TryParse(
                    FrameDuration?.ToString(),
                    out int singleDuration))
            {
                return singleDuration;
            }

            //----------------------------------------
            // Default
            //----------------------------------------

            return 90;
        }
    }
}