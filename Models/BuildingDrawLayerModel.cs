using Microsoft.Xna.Framework;

namespace Ts_Core.Models
{
    /// <summary>
    /// Buildingに追加する条件付きDrawLayerの定義です。
    /// </summary>
    public sealed class BuildingDrawLayerModel
    {
        /// <summary>
        /// DrawLayer IDです。
        /// </summary>
        public string Id { get; set; } = "";

        /// <summary>
        /// このDrawLayerの有効・無効を制御するCustomFieldsのキーです。
        /// 未指定の場合は常に有効です。
        /// </summary>
        public string? EnabledField { get; set; }

        /// <summary>
        /// 描画に使用するテクスチャです。
        /// 未指定の場合はBuildingDataのTextureを使用します。
        /// </summary>
        public string? Texture { get; set; }

        /// <summary>
        /// テクスチャ内の描画範囲です。
        /// </summary>
        public Rectangle SourceRect { get; set; } =
            Rectangle.Empty;

        /// <summary>
        /// 建物基準の描画位置です。
        /// </summary>
        public Vector2 DrawPosition { get; set; }

        /// <summary>
        /// 建物本体より後ろに描画するかどうかです。
        /// </summary>
        public bool DrawInBackground { get; set; }

        /// <summary>
        /// 描画順を調整するY方向オフセットです。
        /// </summary>
        public float SortTileOffset { get; set; }

        /// <summary>
        /// アニメーションフレームの表示時間です。
        /// 数値の場合は全フレーム共通、
        /// 配列の場合は各フレームごとの表示時間として使用します。
        /// </summary>
        public object FrameDuration { get; set; } = 90;

        /// <summary>
        /// 指定したフレームの表示時間を取得します。
        /// </summary>
        public int GetFrameDuration(
            int frameIndex)
        {
            const int defaultDuration = 90;

            //----------------------------------------
            // 配列
            //----------------------------------------

            if (FrameDuration is System.Collections.IEnumerable enumerable
                && FrameDuration is not string)
            {
                List<int> durations =
                    new();

                foreach (object? item in enumerable)
                {
                    if (item == null
                        || !int.TryParse(
                            item.ToString(),
                            out int duration))
                    {
                        return defaultDuration;
                    }

                    durations.Add(
                        Math.Max(
                            1,
                            duration));
                }

                //----------------------------------------
                // FrameCountと要素数が違う
                //----------------------------------------

                if (durations.Count != FrameCount)
                    return defaultDuration;

                if (frameIndex < 0
                    || frameIndex >= durations.Count)
                {
                    return defaultDuration;
                }

                return durations[frameIndex];
            }

            //----------------------------------------
            // 単一値
            //----------------------------------------

            if (FrameDuration != null
                && int.TryParse(
                    FrameDuration.ToString(),
                    out int value))
            {
                return Math.Max(
                    1,
                    value);
            }

            //----------------------------------------
            // 不正値
            //----------------------------------------

            return defaultDuration;
        }

        /// <summary>
        /// アニメーションのフレーム数です。
        /// </summary>
        public int FrameCount { get; set; } = 1;

        /// <summary>
        /// 1行あたりのアニメーションフレーム数です。
        /// </summary>
        public int FramesPerRow { get; set; } = -1;

        /// <summary>
        /// 指定したBuilding Chestにアイテムが入っている場合のみ描画します。
        /// </summary>
        public string? OnlyDrawIfChestHasContents { get; set; }

        /// <summary>
        /// Animal Doorの開閉状態に応じて適用する描画位置オフセットです。
        /// </summary>
        public Point AnimalDoorOffset { get; set; } =
            Point.Zero;

        /// <summary>
        /// DrawLayerを表示するGame State Query条件です。
        /// 未指定の場合は常に表示します。
        /// </summary>
        public string? Condition { get; set; }
    }
}