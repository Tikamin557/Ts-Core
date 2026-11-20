# Custom Furniture（v2.0.7）

← [Modder Guideに戻る](ModderGuide.md)

T's Coreは、`Data/Furniture`に登録された家具に対して、Content Patcherのデータアセット`TsCore/CustomFurniture`から追加機能を設定できます。定義のキーには**家具の表示名ではなく、正確なItem ID**を使用します。T's Core 2.0.7以降が必要です。

## Content Patcherでの設定例

Content Packの`content.json`に`EditData`を追加します。

```json
{
  "Action": "EditData",
  "Target": "TsCore/CustomFurniture",
  "Entries": {
    "YourModId_CustomTV": {
      "Type": "TV",
      "Description": "カスタムTVです。\n右クリックで台の種類を変更できます。",
      "ScreenArea": { "X": 4, "Y": 3, "Width": 16, "Height": 12 },
      "ScreenFit": "Contain",
      "RotationVariants": {
        "Enabled": true,
        "Count": 3,
        "Columns": 3,
        "ScreenAreas": {
          "2": { "X": 5, "Y": 3, "Width": 16, "Height": 12 }
        }
      },
      "TileProperties": {
        "0,0": { "Interaction": "Table", "ItemOffset": "0, -2" },
        "1,0": { "Interaction": "TV" }
      }
    }
  }
}
```

この例は、`Data/Furniture`に対応するTV家具`YourModId_CustomTV`が登録され、テクスチャー上に同じ大きさのスプライトが横に3個並んでいることを前提とします。**ID・座標・画像上のピクセル値は実際の家具に合わせて変更してください。** この例だけでは`Data/Furniture`の定義や画像は追加されません。

## プロパティ一覧

| プロパティ | 説明 |
| --- | --- |
| `Type` | `TV`または`Furniture`。初期値は`TV`。TV映像を描画する場合は`TV`を使用します。 |
| `Description` | 任意の説明文。`\n`で改行でき、表示幅に合わせて自動で折り返します。 |
| `ScreenArea` | `TV`では必須。スプライト左上を基準に、**元画像のピクセル単位**で`X`、`Y`、`Width`、`Height`を指定します。幅と高さは正の値です。 |
| `ScreenFit` | `Stretch`（初期値：比率を無視して引き伸ばす）、`Contain`（比率を維持して全体を収める）、`Cover`（比率を維持して領域を埋め、はみ出す部分を切り取る）。 |
| `RotationVariants` | 回転操作で見た目のバリエーションを切り替えます。当たり判定は変更しません。 |
| `TileProperties` | 家具の衝突タイルごとに操作やActionを設定します。 |

### RotationVariants

- `Enabled`：`true`で有効にします。
- `Count`：バリエーション数。有効時は**2～64**。
- `Columns`：1行に並べるスプライト数。1～`Count`。省略時は`Count`（横一列）。
- `ScreenAreas`：バリエーションごとの画面領域。キーは**1から始まる番号**（`"1"`、`"2"`など）。未指定のバリエーションには共通の`ScreenArea`を使用します。
- スプライトは同じ大きさで、指定した列数に従って並べてください。回転では表示だけを変更し、家具の当たり判定は変更しません。選択状態は家具に保存されます。

### TileProperties

キーは衝突タイルの**相対座標**で、`"X,Y"`形式（例：`"0,0"`、`"1,0"`）です。

- `Action`：そのタイルを操作した際に実行するマップAction形式の文字列。カスタムショップを開くActionなどを指定できます。ゲームや導入Modが対応しているActionを使用してください。
- `Interaction`：`TV`は通常のTV操作、`Table`はTVの指定タイルをテーブルとして使い、アイテムを設置・回収します。家具が持つ単一の`heldObject`を利用するため、タイルごとに別の収納枠が作られるわけではありません。
- `ItemOffset`：`Table`上のアイテムの描画位置を補正する`"X, Y"`形式の値。元画像のピクセル単位で、省略時は`"0, 0"`です。

> **注意：** 既存の`Data/Furniture`の家具を拡張する機能です。TVの動作には対応するTV家具が必要で、あらゆる家具でTV機能を使えるわけではありません。また、テーブル機能を無効にする前に、載せているアイテムを回収してください。

## 関連ガイド

旧家具IDから既存セーブの家具を移行する場合は、[Migration System — Furniture Migration](ModderGuide_MigrationSystem.md#furniture-migration-v207)を参照してください。
