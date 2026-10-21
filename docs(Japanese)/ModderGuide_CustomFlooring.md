# Custom Flooring（カスタム床材）

← [Modder Guide](ModderGuide.md) | [README](../README.md)

T's Core 1.9.9では、バニラの `Flooring` を拡張するContent Patcher用データアセット `TsCore/CustomFlooring` を追加しました。定義のキーは**`Data/FloorsAndPaths` の床材ID**です。アイテムIDではありません。`Data/FloorsAndPaths` 側の `ItemId` で床材アイテムと関連付けます。

## 登録例

以下はContent Patcherの `Changes` に記述する例です。床材IDや画像アセット名は自分のModのものに置き換えてください。

```json
{
  "Action": "EditData",
  "Target": "TsCore/CustomFlooring",
  "Entries": {
    "Example.Mod_StoneFloor": {
      "PlacementHighlight": "Outline",
      "ShowRemovalDebris": false,
      "VariantChangeSound": "shwip",
      "VariantPreviewEnabled": true,
      "VariantPreviewMode": "Builder",
      "VariantPreviewColumns": 4,
      "Variants": [
        { "Id": "plain", "DisplayName": "通常", "Texture": "Mods/Example/FloorTextures", "X": 0, "Y": 0 },
        { "Id": "moss", "DisplayName": "苔", "Texture": "Mods/Example/FloorTextures", "X": 16, "Y": 0 }
      ]
    }
  }
}
```

`Mods/Example/FloorTextures` はContent Patcherで別途読み込む画像アセットです。`X` と `Y` は16×16画像の**ピクセル座標**です。各Variantには**重複しない固定の `Id`** を設定してください。設置済みの床材はこのIDで種類を保存するため、並べ替え後もIDは変更しないでください。

## 床材全体のプロパティ

| プロパティ | 省略時 | 説明 |
|---|---|---|
| `Variants` | `[]` | テクスチャバリエーションの配列。 |
| `ObjectTexture` | なし | アイテム画像の差し替え・合成。`Texture`、`X`、`Y`、`PatchMode` を指定。 |
| `ShowRemovalDebris` | `true` | 撤去時の破片表示。 |
| `AllowFlooringReplacement` | `false` | 設置済みのこのカスタム床材の張り替えを許可し、成功時に元の床材アイテムを返す。 |
| `PlacementHighlight` | `Default` | 設置プレビューの表示方式。`Default` / `None` / `Outline`。 |
| `VariantChangeSound` | なし（無音） | 設置前にバリエーションを切り替えた際の効果音。 |
| `VariantPreviewEnabled` | `false` | 手持ちの床材のテクスチャ選択UIを表示。 |
| `VariantPreviewShowName` | `false` | 選択中のバリエーション名を別ウィンドウに表示。 |
| `VariantPreviewMode` | `Carousel` | `Carousel`（前後を横並び）/ `Builder`（一覧）。 |
| `VariantPreviewColumns` | `5` | Builderの列数。1～10。 |
| `SelectablePreviewEmptyOrders` | `""` | Builderで空欄にする位置番号。1始まり、カンマ区切り。 |
| `VariantPreviewCount` | `5` | Carouselの表示数。3・5・7・9。 |
| `VariantPreviewScale` | `0` | UIの拡大補正。0～5。 |
| `VariantPreviewOffsetX`, `VariantPreviewOffsetY` | `0` | UI位置調整。10 UIピクセル単位。 |
| `IdentificationMarker` | なし | 識別表示に使う画像。`Texture`、`X`、`Y` を指定。 |

`ObjectTexture.PatchMode` は `Replace`（省略時）と `Overlay` に対応します。アイテム画像と設置後の床材画像は別々に処理されます。

## Variantのプロパティ

| プロパティ | 省略時 | 説明 |
|---|---|---|
| `Id` | `""` | 必須の固定ID。床材内で重複不可。 |
| `DisplayName` | なし | 選択UIに表示する名前。 |
| `SelectablePreviewOrder` | なし | Builder専用の表示順。未指定は指定済みの後に表示。 |
| `Texture`, `X`, `Y` | `""`, `0`, `0` | 16×16画像アセットとピクセル座標。 |
| `PlacementSound`, `RemovalSound`, `FootstepSound` | 継承 | Variant単位の効果音。省略時は基本設定、明示的な `null` は無音。 |
| `RandomTextures` | なし | 重み付きランダムテクスチャ候補。 |
| `RandomTextureChange` | `Daily` | `AMPM` / `DayNight` / `Daily` / `Weekly` / `Seasonal`。 |
| `RandomTextureShared` | `false` | 同じ床材ID・Variant IDの設置物で抽選結果を共有。 |
| `Animation` | なし | `X`、`Y`、`Duration`（ミリ秒）を持つフレーム配列。 |
| `AnimationId` | なし | アニメーションの位相を分ける任意のID。 |
| `AnimationSync` | `true` | `false` で設置ごとの再生位相を保存。 |

`RandomTextures` の各候補は `Texture`、`X`、`Y`、任意の `FootstepSound`、`Animation`、`Weight` を指定できます。`Weight` は省略時 `1`、正の値は相対抽選確率、**0以下は抽選対象外**です。全候補が0以下の場合は全候補から均等抽選する処理に戻ります。

```json
"RandomTextures": [
  { "Texture": "Mods/Example/FloorTextures", "X": 0, "Y": 16, "Weight": 3 },
  { "Texture": "Mods/Example/FloorTextures", "X": 16, "Y": 16, "Weight": 1 }
]
```

この例では抽選比率は3:1です。同じ変更周期中は抽選結果が安定します。`AMPM` は12:00、`DayNight` は18:00、`Daily` は毎日、`Weekly` は各季節の1・8・15・22日、`Seasonal` は季節変更時に切り替わります。候補ごとにアニメーションや足音も設定できます。

## 操作と互換性

カスタム床材を手に持った状態で、**右クリックで設置、左クリックでテクスチャ変更、Shift＋左クリックで回収**します。T's Coreの設定から前のテクスチャに戻すキーや設置済み床材の識別表示キーも変更できます。テクスチャ選択UIは床材ごとに有効化できます。

通常の床材オブジェクトを拡張する方式です。Content PatcherのContent PackにはT's Coreへの依存関係を設定してください。設置・張り替え・セーブ／ロード・季節変更・対応するRecolor系Modとの動作を確認してください。
