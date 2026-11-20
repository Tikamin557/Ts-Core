﻿namespace Ts_Core.Models.ContentPatcherRelated
{
    /// <summary>Data/Furnitureに登録された家具の追加機能を定義します。</summary>
    public sealed class CustomFurnitureModel
    {
        public string Type { get; set; } = "TV";
        /// <summary>家具の説明文。未指定時はゲーム本来の説明文を使用します。</summary>
        public string? Description { get; set; }
        /// <summary>衝突タイルの相対座標 (X, Y) ごとのActionを定義します。</summary>
        public Dictionary<string, CustomFurnitureTileProperty>? TileProperties { get; set; }
        public CustomFurnitureScreenArea? ScreenArea { get; set; }
        public string ScreenFit { get; set; } = "Stretch";
        public CustomFurnitureRotationVariants? RotationVariants { get; set; }
    }

    /// <summary>家具の衝突タイルで実行するアクションです。</summary>
    public sealed class CustomFurnitureTileProperty
    {
        public string? Action { get; set; }
        /// <summary>指定タイルの操作。Tableは設置・回収、TVは通常の番組操作です。</summary>
        public string? Interaction { get; set; }
        /// <summary>Table上のアイテム描画位置の補正値（元画像のピクセル単位、X, Y）。未指定時は0, 0です。</summary>
        public string? ItemOffset { get; set; }
    }

    /// <summary>回転操作で切り替える同寸法スプライトの配置を定義します。</summary>
    public sealed class CustomFurnitureRotationVariants
    {
        public bool Enabled { get; set; }
        public int Count { get; set; } = 1;

        /// <summary>1行あたりのスプライト数。未指定時はCountと同じ横一列として扱います。</summary>
        public int? Columns { get; set; }

        /// <summary>1から始まるバリエーション番号ごとのTV画面領域。未指定時は共通のScreenAreaを使用します。</summary>
        public Dictionary<string, CustomFurnitureScreenArea>? ScreenAreas { get; set; }
    }

    /// <summary>家具画像の左上を基準とする、元画像のピクセル単位の画面領域です。</summary>
    public sealed class CustomFurnitureScreenArea
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
