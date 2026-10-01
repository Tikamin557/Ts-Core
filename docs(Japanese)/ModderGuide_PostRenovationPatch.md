# 📖 Modder Guide

このガイドでは、**T's Core** がContent Patcher向けに提供している公開機能の使用方法を説明します。

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
-   ✅ **Post Renovation Patch** *(現在のページ)*
-   📄 [Polyamory Sweet Rooms Integration](ModderGuide_PolyamorySweetRoomsIntegration.md)
-   📄 [Other Features](ModderGuide_OtherFeatures.md)

← [READMEに戻る](../../README.md)

← [Modder Guideに戻る](ModderGuide.md)

------------------------------------------------------------------------

# Post Renovation Patch

Post Renovation Patchは、FarmHouse Map向けのData DrivenなMap Patch Systemです。

通常のContent PatcherによるMap編集は、その後Stardew ValleyがFarmHouse Layoutを再構築し、RenovationなどのRuntime変更を適用した際に上書きされることがあります。T's Coreでは、`FarmHouse.updateFarmLayout()` の完了後に登録済みMap Patchを適用できるため、最終的に構築されたFarmHouse MapへPatchを適用できます。

Post Renovation Patchは次のData Assetを通して登録します:

``` text
TsCore/PostRenovationPatches
```

C#コードは必要ありません。

------------------------------------------------------------------------

<a id="contents"></a>

## 目次

-   [When to Use This System](#when-to-use-this-system)
-   [Data Asset](#data-asset)
-   [Properties](#properties)
-   [Patch Modes](#patch-modes)
-   [Basic Example](#basic-example)
-   [FromArea and ToArea](#fromarea-and-toarea)
-   [Map Size and Layers](#map-size-and-layers)
-   [Updating Patches](#updating-patches)
-   [Notes](#notes)

------------------------------------------------------------------------

<a id="when-to-use-this-system"></a>

## このSystemを使用する場合

Stardew ValleyがFarmHouse Layoutを再構築した後もMap編集を維持する必要がある場合に、Post Renovation Patchを使用します。

代表的な例は、FarmHouse Renovationの影響を受ける範囲と重なる階段、出入口、その他のMap追加です。通常のContent Patcher `EditMap` PatchはRuntimeでFarmHouse Layoutが変更される前に適用され、その後上書きされる場合があります。

Post Renovation Patchは、このようなFarmHouse固有のRuntime Map編集を目的としています。

------------------------------------------------------------------------

## Data Asset

次のData Assetを編集してPatchを登録します:

``` text
TsCore/PostRenovationPatches
```

各Entry KeyがPatchの一意なIDになります。

例:

``` json
{
  "Action": "EditData",
  "Target": "TsCore/PostRenovationPatches",
  "Entries": {
    "{{ModId}}_ExamplePatch": {
      "Target": "Maps/FarmHouse2",
      "FromFile": "Maps/ExamplePatch",
      "ToArea": {
        "X": 10,
        "Y": 20,
        "Width": 4,
        "Height": 4
      },
      "PatchMode": "Overlay"
    }
  }
}
```

`Target` と `FromFile` の値にはGame Content Asset名を指定します。

そのため、Source MapをContent PatcherでGame Content Pipelineへ読み込ませ、Post Renovation Patchの定義から参照できます。

------------------------------------------------------------------------

<a id="properties"></a>

## Property

  -----------------------------------------------------------------------------------------------------------------------------------------------------------
  Property              必須                  説明
  --------------------- --------------------- ---------------------------------------------------------------------------------------------------------------
  Entry key             ✅                    このPost Renovation Patch Entryの一意なID。

  `Target`              ✅                    このPatchを適用するFarmHouse MapのGame Content Asset名。

  `FromFile`            ✅                    Patchに使用するSource MapのGame Content Asset名。

  `FromArea`            ❌                    Source MapからコピーするRectangle。省略した場合はSource Map全体を使用します。

  `ToArea`              ❌                    Target Map内のDestination Rectangle。省略した場合は `FromArea` のSizeを使用し、`0, 0` からPatchを適用します。

  `PatchMode`           ❌                    Map Patch Mode。Defaultは `Overlay`。
  -----------------------------------------------------------------------------------------------------------------------------------------------------------

`FromArea` と `ToArea` はWidthとHeightが同じである必要があります。

------------------------------------------------------------------------

<a id="patch-modes"></a>

## Patch Mode

次のSMAPI `PatchMapMode` の値に対応しています:

  ---------------------------------------------------------------------------------------------------------------------------------
  値                       説明
  ------------------------ --------------------------------------------------------------------------------------------------------
  `Overlay`                空でないSource TileをTarget Map上へコピーします。

  `ReplaceByLayer`         Source Mapに存在するLayerについて、空のSource Tileも含めてDestination Areaを置き換えます。

  `Replace`                Map全体のDestination Areaを置き換え、その範囲内にあるTarget側にしか存在しないLayerのTileも消去します。
  ---------------------------------------------------------------------------------------------------------------------------------

`PatchMode` を省略した場合は `Overlay` が使用されます。

------------------------------------------------------------------------

<a id="basic-example"></a>

## 基本例

まず、Source MapをGame Content Assetとして読み込みます:

``` json
{
  "Action": "Load",
  "Target": "Maps/Example_PostRenovationPatch",
  "FromFile": "assets/ExamplePatch.tmx"
}
```

次にPost Renovation Patchを登録します:

``` json
{
  "Action": "EditData",
  "Target": "TsCore/PostRenovationPatches",
  "Entries": {
    "{{ModId}}_ExamplePatch": {
      "Target": "Maps/FarmHouse2",
      "FromFile": "Maps/Example_PostRenovationPatch",
      "ToArea": {
        "X": 10,
        "Y": 20,
        "Width": 4,
        "Height": 4
      },
      "PatchMode": "Overlay"
    }
  }
}
```

対象FarmHouse Layoutが再構築されると、T's CoreはFarmHouse LayoutのUpdate完了後に登録済みPatchを適用します。

------------------------------------------------------------------------

<a id="fromarea-and-toarea"></a>

## FromAreaとToArea

Source Mapの一部だけをコピーしたい場合は `FromArea` を使用できます。

``` json
"FromArea": {
  "X": 4,
  "Y": 8,
  "Width": 6,
  "Height": 5
}
```

`ToArea` は、その範囲をTarget Mapのどこへ配置するかを制御します:

``` json
"ToArea": {
  "X": 20,
  "Y": 12,
  "Width": 6,
  "Height": 5
}
```

2つのRectangleは同じDimensionである必要があります。

`FromArea` を省略した場合はSource Map全体が使用されます。

`ToArea` を省略した場合はSource AreaのWidthとHeightを使用し、Target Mapの左上へPatchが配置されます。

------------------------------------------------------------------------

<a id="map-size-and-layers"></a>

## Map SizeとLayer

Destination Areaが現在のTarget Map Sizeを超える場合、T's CoreはPatchが収まるようにTarget Mapを拡張します。

Source Mapには存在するもののTarget Mapには存在しないLayerは、必要に応じてTarget Mapへ追加されます。

またT's Coreは、適用するPatchでSource MapのTileを使用できるように、Source MapのTile SheetをTarget Mapへ対応付けます。

------------------------------------------------------------------------

<a id="updating-patches"></a>

## Patchの更新

`TsCore/PostRenovationPatches` は通常のGame Content Assetであり、Content PatcherのConditionを通して編集できます。

このAssetがInvalidateされると、T's Coreは影響を受けるFarmHouse Mapを再構築し、現在有効なPost Renovation Patchを再適用します。

これにより、Content PatcherがData Assetを更新した際にPatchを追加、削除、変更できます。

開発中は `tscore_cp_reload <ContentPackId>` を使用して、このData Assetを編集するContent Packを再読み込みできます。

------------------------------------------------------------------------

<a id="notes"></a>

## 注意事項

-   Post Renovation Patchは `FarmHouse` Instanceに対してのみ適用されます。
-   `Target` はFarmHouseの現在のMap Assetと照合されます。
-   `FromFile` はGame Content Pipelineから利用可能なMapを参照する必要があります。
-   `FromArea` はSource Mapの範囲内に収まっている必要があります。
-   `FromArea` と `ToArea` は同じDimensionである必要があります。
-   `ToArea` に負の座標は使用できません。
-   Defaultの `PatchMode` は `Overlay` です。
-   個々のPatch適用中に発生したErrorはSMAPI Consoleへ出力されます。
-   Content PatcherのConditionは、`TsCore/PostRenovationPatches` のEntryを追加または変更する `EditData` Patch側に設定してください。

------------------------------------------------------------------------

## Modder Guide

-   ← [Content Patcher Integration](ModderGuide_ContentPatcherIntegration.md)
-   ↑ [Guide Index](#top)
-   → [Polyamory Sweet Rooms Integration](ModderGuide_PolyamorySweetRoomsIntegration.md)

← [READMEに戻る](../../README.md)

← [Modder Guideに戻る](ModderGuide.md)
