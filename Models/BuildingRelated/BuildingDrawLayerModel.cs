using Microsoft.Xna.Framework;

namespace Ts_Core.Models.BuildingRelated
{
    /// <summary>
    /// Buildingに追加するDrawLayerの定義です。
    /// </summary>
    public sealed class BuildingDrawLayerModel
    {
        //----------------------------------------
        // Basic
        //----------------------------------------

        /// <summary>
        /// DrawLayer IDです。
        /// </summary>
        public string Id { get; set; } = "";

        /// <summary>
        /// このDrawLayerが有効かどうかです。
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// このDrawLayerの有効・無効を制御する
        /// CustomFieldsのキーです。
        ///
        /// 未指定の場合は
        /// Enabledのみで判定します。
        /// </summary>
        public string? EnabledField { get; set; }

        //----------------------------------------
        // Texture
        //----------------------------------------

        /// <summary>
        /// 使用するTextureです。
        ///
        /// 未指定の場合は
        /// Building本体のTextureを使用します。
        /// </summary>
        public string? Texture { get; set; }

        /// <summary>
        /// Textureから使用する範囲です。
        /// </summary>
        public Rectangle SourceRect { get; set; }
            = Rectangle.Empty;

        /// <summary>
        /// Building左上を基準とした
        /// 描画位置です。
        /// </summary>
        public Vector2 DrawPosition { get; set; }
            = Vector2.Zero;

        //----------------------------------------
        // Draw Order
        //----------------------------------------

        /// <summary>
        /// Background側へ描画するかどうかです。
        /// </summary>
        public bool DrawInBackground { get; set; }

        /// <summary>
        /// 背景DrawLayerを床材より上、建物本体より下に描画します。
        /// DrawInBackgroundがtrueの場合のみ有効です。
        /// </summary>
        public bool DrawAboveFlooring { get; set; }

        /// <summary>
        /// 描画順計算用のタイルオフセットです。
        /// </summary>
        public float SortTileOffset { get; set; }

        //----------------------------------------
        // Animation
        //----------------------------------------

        /// <summary>
        /// フレームごとの表示時間です。
        ///
        /// 数値1つ、
        /// またはFrameCountと同数の配列を指定できます。
        /// </summary>
        public object FrameDuration { get; set; } = 90;

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
        /// 建物ごとにアニメーションの再生開始位置をずらします。
        /// </summary>
        public bool RandomStartFrame { get; set; }

        /// <summary>
        /// 再生開始位置をずらす刻み幅（ミリ秒）です。
        /// 0以下の場合はずらしません。
        /// </summary>
        public int RandomStartIntervalMs { get; set; } = 100;

        //----------------------------------------
        // Conditions
        //----------------------------------------

        /// <summary>
        /// 指定したChestに中身がある場合のみ
        /// 描画します。
        /// </summary>
        public string? OnlyDrawIfChestHasContents { get; set; }

        /// <summary>
        /// Animal Doorを基準にする場合の
        /// オフセットです。
        /// </summary>
        public Vector2 AnimalDoorOffset { get; set; }
            = Vector2.Zero;

        /// <summary>
        /// 描画条件です。
        /// </summary>
        public string? Condition { get; set; }

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