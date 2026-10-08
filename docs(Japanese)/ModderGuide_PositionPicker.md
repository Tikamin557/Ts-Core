# 📖 Modder Guide

このガイドでは、Content PatcherのContent PackへT's Coreの **Position Picker** を追加する方法を説明します。

<a id="top"></a>

## Guide Index

-   📄 [Relationship Services](ModderGuide_RelationshipServices.md)
-   📄 [Location Services](ModderGuide_LocationServices.md)
-   📄 [Warp Services](ModderGuide_WarpServices.md)
-   📄 [Map Properties](ModderGuide_MapProperties.md)
-   📄 [Building Services](ModderGuide_BuildingServices.md)
-   📄 [BigCraftable Extension](ModderGuide_BigCraftableExtension.md)
-   📄 [Dialogue System](ModderGuide_DialogueSystem.md)
-   📄 [Migration System](ModderGuide_MigrationSystem.md)
-   📄 [Notification System](ModderGuide_NotificationSystem.md)
-   📄 [Content Patcher Integration](ModderGuide_ContentPatcherIntegration.md)
-   ✅ **Position Picker** *(現在のページ)*
-   📄 [Post Renovation Patch](ModderGuide_PostRenovationPatch.md)
-   📄 [Polyamory Sweet Rooms Integration](ModderGuide_PolyamorySweetRoomsIntegration.md)
-   📄 [Other Features](ModderGuide_OtherFeatures.md)

← [READMEに戻る](../../README.md)

← [Modder Guideに戻る](ModderGuide.md)

------------------------------------------------------------------------

# Position Picker

Position Pickerを使用すると、Content PatcherのContent Pack自身のGeneric Mod Config Menuへボタンを追加し、プレイヤーがMap上でX/Y座標を直接選択できるようになります。

選択した座標はContent Packに既存のConfigSchema項目へ書き込まれ、Content Patcherの通常のConfig保存・適用処理を通して保存されます。C#コードは必要ありません。

Position Pickerは次のData Assetへ登録します。

``` text
TsCore/PositionPickers
```

## 必要なもの

- T's Core
- Content Patcher
- Generic Mod Config Menu
- Content Packの `ConfigSchema` に定義済みのX/Y項目
- 配置プレビューとして使用するMap Asset

Position Pickerはプレイヤーを対象Locationへ自動ワープしません。ボタンを使用するには、プレイヤーが設定された `Location` にいる必要があります。

## Position Pickerの登録

``` json
{
  "Action": "EditData",
  "Target": "TsCore/PositionPickers",
  "Entries": {
    "{{ModId}}_ExamplePosition": {
      "ContentPackId": "{{ModId}}",
      "GMCM_Name": "{{i18n:position-picker.name}}",
      "GMCM_Description": "{{i18n:position-picker.description}}",
      "GMCM_Button": "{{i18n:position-picker.button}}",
      "GMCM_WorldRequired": "{{i18n:position-picker.world-required}}",
      "GMCM_LocationRequired": "{{i18n:position-picker.location-required}}",
      "XField": "ExamplePosition_X",
      "YField": "ExamplePosition_Y",
      "AfterField": "ExamplePosition_Y",
      "Location": "FarmHouse",
      "PreviewMap": "Mods/{{ModId}}/ExamplePosition_Preview",
      "PreviewNote": "{{i18n:position-picker.preview-note}}",
      "AnchorX": 0,
      "AnchorY": 0
    }
  }
}
```

## Property

| Property | 必須 | 説明 |
|----------|------|------|
| Entry key | ✅ | Position Picker定義の一意なIDです。 |
| `ContentPackId` | ✅ | Pickerを追加するContent PackのUniqueIDです。通常は `{{ModId}}` を使用します。 |
| `GMCM_Name` | 任意 | GMCMに表示する項目名です。未指定時はT's Core標準の翻訳を使用します。 |
| `GMCM_Description` | 任意 | 通常時の説明・Hover Textです。未指定時はT's Core標準の翻訳を使用します。 |
| `GMCM_Button` | 任意 | Pickerボタン内に表示する文字列です。 |
| `GMCM_WorldRequired` | 任意 | セーブ未読込時にボタン上へ表示するHover Textです。 |
| `GMCM_LocationRequired` | 任意 | 対象Location外にいる時にボタン上へ表示するHover Textです。 |
| `XField` | ✅ | 選択したX座標を書き込むConfigSchema項目です。 |
| `YField` | ✅ | 選択したY座標を書き込むConfigSchema項目です。 |
| `BeforeField` | 任意 | 指定したConfigSchema項目の直前へPickerボタンを挿入します。`AfterField` と両方指定した場合はこちらが優先されます。 |
| `AfterField` | 任意 | このConfigSchema項目の直後へPickerボタンを挿入します。対象が見つからない場合はGMCMの末尾へ追加されます。 |
| `Location` | ✅ | Pickerを使用できるLocation名です。T's Coreでは `Name` と `NameOrUniqueName` の両方を判定します。 |
| `PreviewMap` | ✅ | 配置プレビューとして描画するMap Assetです。この値の `{{ModId}}` はContent PackのUniqueIDへ置き換えられます。 |
| `PreviewNote` | 任意 | 位置選択中、座標・操作ガイドの下へ表示する注意書きです。長い文章は自動で折り返されます。 |
| `AnchorX` | 任意 | 選択座標へ合わせる `PreviewMap` 内のX Tile位置です。初期値は `0` です。 |
| `AnchorY` | 任意 | 選択座標へ合わせる `PreviewMap` 内のY Tile位置です。初期値は `0` です。 |

`BeforeField` は指定した項目の直前、`AfterField` は直後に配置します。対象項目が見つからない場合はGMCMページの末尾へ追加されます。

`XField`、`YField`、`Location`、`PreviewMap` は必須です。不正な定義は無視され、SMAPIログへWarningが出力されます。

## Preview MapとAnchor

`PreviewMap` は位置選択時の表示専用です。座標保存後にContent PatcherやPost Renovation Patchが実際に配置する内容を決めるものではありません。

そのため、実際のPatchには含まれない周囲の壁や梁などをPreviewへ含めることもできます。

たとえば1×4のPreview Mapが次の内容の場合:

``` text
Y=0  梁
Y=1  壁
Y=2  マーカー  ← 選択座標
Y=3  壁
```

次のように指定します。

``` json
"AnchorX": 0,
"AnchorY": 2
```

これにより、Preview Map内のY=2のマーカーTileがプレイヤーの選択座標へ合います。

Preview Map内のTileの回転・反転情報も描画へ反映されます。

## 操作方法

Position Picker使用中は次の操作ができます。

- マウスを動かしてTileを選択
- 左クリックで決定
- 右クリックまたは `Esc` でキャンセル
- 画面端へマウスを移動してカメラをスクロール
- `WASD` または矢印キーでもカメラをスクロール

選択座標は必ず実際のMap範囲内へ制限されます。Previewが左上のガイドに隠れにくいよう、カメラだけはMap端より少し外側までスクロールできます。

## 座標の保存と適用

プレイヤーがTileを決定すると、T's Coreは `XField` と `YField` を更新し、そのContent Packに対してContent Patcherの通常の保存・適用処理を実行します。

そのため、これらのConfig Tokenを使用している既存PatchはContent Patcherの通常のConfig更新処理を通して反映できます。

Position Picker自体が最終的なMap Patchを配置するわけではありません。保存されたX/Yをどのように使用するかはContent Pack側で設定します。

## GMCMでの動作

次の場合、Pickerボタンは無効になります。

- セーブデータが読み込まれていない
- プレイヤーが設定された `Location` にいない

無効状態のボタンへマウスを乗せると、その理由が表示されます。`GMCM_WorldRequired` と `GMCM_LocationRequired` で独自の文言を指定できます。

`AfterField` を使用すると、PickerボタンをGMCM末尾ではなく関連するX/Y設定の直後へ配置できます。

## ConfigSchemaの例

``` json
"ConfigSchema": {
  "ExamplePosition_X": {
    "Default": "20"
  },
  "ExamplePosition_Y": {
    "Default": "10"
  }
}
```

通常のConfig TokenとしてPatchから使用できます。

``` json
"X": "{{ExamplePosition_X}}",
"Y": "{{ExamplePosition_Y}}"
```

------------------------------------------------------------------------

## Modder Guide

- ← [Content Patcher Integration](ModderGuide_ContentPatcherIntegration.md)
- ↑ [Guide Index](#top)
- → [Post Renovation Patch](ModderGuide_PostRenovationPatch.md)

← [READMEに戻る](../../README.md)

← [Modder Guideに戻る](ModderGuide.md)
