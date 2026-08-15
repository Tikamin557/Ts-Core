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
        /// アニメーション1フレームの表示時間です。
        /// </summary>
        public int FrameDuration { get; set; } = 90;

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