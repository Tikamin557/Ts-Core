# 📖 Modder Guide

このガイドでは、**T's Core** がContent
Patcher向けに提供している公開機能の使用方法を説明します。

<a id="top"></a>

## Guide Index

-   📄 [Relationship Services](ModderGuide_RelationshipServices.md)
-   📄 [Location Services](ModderGuide_LocationServices.md)
-   📄 [Warp Services](ModderGuide_WarpServices.md)
-   📄 [Map Properties](ModderGuide_MapProperties.md)
-   ✅ **Building Services** *(現在のページ)*
-   📄 [BigCraftable Extension](ModderGuide_BigCraftableExtension.md)
-   📄 [Dialogue System](ModderGuide_DialogueSystem.md)
-   📄 [Migration System](ModderGuide_MigrationSystem.md)
-   📄 [Notification System](ModderGuide_NotificationSystem.md)
-   📄 [Content Patcher
    Integration](ModderGuide_ContentPatcherIntegration.md)
-   📄 [Post Renovation Patch](ModderGuide_PostRenovationPatch.md)
-   📄 [Polyamory Sweet Rooms
    Integration](ModderGuide_PolyamorySweetRoomsIntegration.md)
-   📄 [Other Features](ModderGuide_OtherFeatures.md)

← [READMEに戻る](../../README.md)

← [Modder Guideに戻る](ModderGuide.md)

------------------------------------------------------------------------

# Building Services

Building Servicesは、`Data/Buildings`
で定義されたBuildingへ追加機能を提供します。

T's Coreでは、Content Patcher Content PackからカスタムData Asset
`TsCore/BuildingProviders` を通してBuilding Providerを追加できます。

Building Providerを使用すると、C#コードを必要とせずに、夜間のLight
Source、条件付きDraw
Layer、機能の有効/無効制御、建築場所の制限などを追加できます。

このガイドでは、Content PatcherからBuilding
Providerを登録する方法と、利用可能なBuilding関連機能の設定方法を説明します。

------------------------------------------------------------------------

## 目次

-   [Content Patcherのセットアップ](#content-patcher-setup)
-   [Building Provider](#building-provider)
-   [機能の有効化と無効化](#enabling-and-disabling-features)
-   [Building Lights](#building-lights)
-   [Building Draw Layers](#building-draw-layers)
-   [Valley Farm Only](#valley-farm-only)
-   [使用例](#example)
-   [デバッグ](#debugging)
-   [注意事項](#notes)

------------------------------------------------------------------------

<a id="content-patcher-setup"></a>

## Content Patcherのセットアップ

Building Providerは、次のカスタムData Assetを編集して登録します:

``` text
TsCore/BuildingProviders
```

そのため、通常のContent Patcher Content PackからBuilding
Servicesを直接利用できます。

Content Packでは、Content PatcherとT's
Coreの両方を依存Modに指定してください。

### manifest.json

``` json
{
  "Name": "[CP] My Building Mod",
  "Author": "YourName",
  "Version": "1.0.0",
  "UniqueID": "YourName.MyBuildingMod",
  "UpdateKeys": [ "Nexus:12345" ],
  "ContentPackFor": {
    "UniqueID": "Pathoschild.ContentPatcher"
  },
  "Dependencies": [
    {
      "UniqueID": "Tikamin557.TsCore",
      "IsRequired": true
    }
  ]
}
```

Content Packを公開する前に、例の値を自分の情報へ置き換えてください。

### Building Providerの登録

Content Patcherの `EditData`
Actionを使用して、`TsCore/BuildingProviders` へEntryを追加します。

``` json
{
  "Action": "EditData",
  "Target": "TsCore/BuildingProviders",
  "Entries": {
    "YourModId_MyBuildingProvider": {
      "BuildingType": "YourModId_MyBuilding"
    }
  }
}
```

`TsCore/BuildingProviders` の各Entryが1つのBuilding Providerを表します。

EntryのKeyがProvider IDになります:

``` json
"YourModId_MyBuildingProvider": {
  "BuildingType": "YourModId_MyBuilding"
}
```

Provider IDは一意にしてください。他のContent
Packとの競合を避けるため、自分のModのUniqueIDなど、一意なPrefixを使用することを推奨します。

複数のBuilding Providerから同じ `BuildingType`
を対象にすることもできます。

------------------------------------------------------------------------

## Building Provider

Building Providerは `Data/Buildings` のBuilding Typeを対象として、T's
Core固有の機能を追加します。

基本例:

``` json
{
  "Action": "EditData",
  "Target": "TsCore/BuildingProviders",
  "Entries": {
    "YourModId_MyBuildingProvider": {
      "BuildingType": "YourModId_MyBuilding"
    }
  }
}
```

### Building ProviderのProperty

  --------------------------------------------------------------------------------------------------------------------------
  Property                   必須             デフォルト      説明
  -------------------------- ---------------- --------------- --------------------------------------------------------------
  `BuildingType`             ✅               ---             このProviderが対象とする `Data/Buildings` のBuilding Type ID。

  `BuildingsEnabled`         任意             `true`          Building Provider全体を有効または無効にします。

  `BuildingsEnabledField`    任意             ---             Building Provider全体の有効/無効を制御する `CustomFields`
                                                              のKey。

  `LightsEnabled`            任意             `true`          このProvider内のすべてのBuilding
                                                              Lightを有効または無効にします。

  `LightsEnabledField`       任意             ---             このProvider内のすべてのBuilding Lightの有効/無効を制御する
                                                              `CustomFields` のKey。

  `DrawLayersEnabled`        任意             `true`          このProvider内のすべてのBuilding Draw
                                                              Layerを有効または無効にします。

  `DrawLayersEnabledField`   任意             ---             このProvider内のすべてのBuilding Draw
                                                              Layerの有効/無効を制御する `CustomFields` のKey。

  `ValleyFarmOnly`           任意             `false`         `true` の場合、メインのValley
                                                              Farmを対象にしたときだけ建築メニューへBuildingを表示します。

  `Lights`                   任意             ---             このProviderが追加するBuilding Light。

  `DrawLayers`               任意             ---             このProviderが追加する条件付きBuilding Draw Layer。
  --------------------------------------------------------------------------------------------------------------------------

Provider
IDはProviderデータ内には指定しません。`TsCore/BuildingProviders` のEntry
KeyがProvider IDになります。

例:

``` json
"YourModId_MyBuildingProvider": {
  "BuildingType": "YourModId_MyBuilding"
}
```

ここでは `YourModId_MyBuildingProvider` がProvider IDです。

複数のBuilding Providerから同じ `BuildingType`
を対象にすることもできます。

同じBuildingでも設定ごとに異なるT's Core機能を使用したい場合に便利です。

------------------------------------------------------------------------

<a id="enabling-and-disabling-features"></a>

## 機能の有効化と無効化

Building
Servicesでは、機能を有効または無効にする方法を2種類提供しています:

1.  `TsCore/BuildingProviders` 内の直接指定する `Enabled` Property。
2.  対象Buildingの `Data/Buildings` → `CustomFields` と連動する
    `EnabledField` Property。

両方の方法を同時に使用できます。

両方を使用した場合、対応する機能を有効にするには
**両方が有効である必要があります**。

------------------------------------------------------------------------

### 直接指定するEnabled Property

Provider単位で直接制御できるPropertyは次のとおりです:

``` json
{
  "BuildingsEnabled": true,
  "LightsEnabled": true,
  "DrawLayersEnabled": true
}
```

3つとも省略した場合のデフォルトは `true` です。

#### BuildingsEnabled

`BuildingsEnabled` はBuilding Provider全体を制御します。

``` json
"BuildingsEnabled": false
```

`false` の場合、そのProviderが制御する機能は無効になります。

これにより、そのProviderのBuilding Light、Building Draw
Layer、`ValleyFarmOnly` 制限も無効になります。

#### LightsEnabled

`LightsEnabled` はProviderで定義されたすべてのBuilding
Lightを制御します。

``` json
"LightsEnabled": false
```

#### DrawLayersEnabled

`DrawLayersEnabled` はProviderで定義されたすべてのBuilding Draw
Layerを制御します。

``` json
"DrawLayersEnabled": false
```

これらの値は `TsCore/BuildingProviders` の一部なので、Content
PatcherのPatchから直接編集できます。

例:

``` json
{
  "Action": "EditData",
  "Target": "TsCore/BuildingProviders",
  "TargetField": [
    "YourModId_MyBuildingProvider",
    "LightsEnabled"
  ],
  "Value": false
}
```

そのため、Content PatcherのConditionを使用してBuilding
Servicesの機能を動的に変更できます。

------------------------------------------------------------------------

### EnabledField Property

Building Providerでは、対象Buildingの `Data/Buildings` → `CustomFields`
のFieldも使用できます。

Provider単位では次の3つのPropertyを利用できます:

``` json
{
  "BuildingsEnabledField": "MyBuilding/BuildingsEnabled",
  "LightsEnabledField": "MyBuilding/LightsEnabled",
  "DrawLayersEnabledField": "MyBuilding/DrawLayersEnabled"
}
```

ここで指定する値は、T's Coreが対象Buildingの `CustomFields`
から読み取るKey名です。

例:

``` json
"CustomFields": {
  "MyBuilding/BuildingsEnabled": "true",
  "MyBuilding/LightsEnabled": "false",
  "MyBuilding/DrawLayersEnabled": "true"
}
```

#### BuildingsEnabledField

`BuildingsEnabledField` は `CustomFields` の値を通してBuilding
Provider全体を制御します。

``` json
"BuildingsEnabledField": "MyBuilding/BuildingsEnabled"
```

#### LightsEnabledField

`LightsEnabledField` はProviderで定義されたすべてのBuilding
Lightを制御します。

``` json
"LightsEnabledField": "MyBuilding/LightsEnabled"
```

#### DrawLayersEnabledField

`DrawLayersEnabledField` はProviderで定義されたすべてのBuilding Draw
Layerを制御します。

``` json
"DrawLayersEnabledField": "MyBuilding/DrawLayersEnabled"
```

Enabled Fieldを指定していない場合、対応するEnabled
Fieldの判定は有効として扱われます。

指定した `CustomFields`
のKeyが存在しない場合も、その機能は有効として扱われます。

------------------------------------------------------------------------

### EnabledとEnabledFieldの組み合わせ

直接指定する `Enabled` Propertyと `EnabledField`
PropertyはAND条件で判定されます。

例:

``` json
{
  "LightsEnabled": true,
  "LightsEnabledField": "MyBuilding/LightsEnabled"
}
```

かつ:

``` json
"CustomFields": {
  "MyBuilding/LightsEnabled": "false"
}
```

の場合、Lightは無効になります。

同様に:

``` json
{
  "LightsEnabled": false,
  "LightsEnabledField": "MyBuilding/LightsEnabled"
}
```

は、`CustomFields` の値が `true` でも無効のままです。

Lightの有効判定は次の階層で行われます:

``` text
BuildingsEnabled
→ BuildingsEnabledField
→ LightsEnabled
→ LightsEnabledField
→ individual Light Enabled
→ individual Light EnabledField
```

Draw Layerの有効判定は次の階層で行われます:

``` text
BuildingsEnabled
→ BuildingsEnabledField
→ DrawLayersEnabled
→ DrawLayersEnabledField
→ individual Draw Layer Enabled
→ individual Draw Layer EnabledField
```

------------------------------------------------------------------------

### 同じBuildingに対する複数Provider

同じ `BuildingType` を対象とする複数のBuilding
Providerは、それぞれ個別に有効または無効にできます。

例:

``` json
{
  "Action": "EditData",
  "Target": "TsCore/BuildingProviders",
  "Entries": {
    "YourModId_MyBuilding_VariantA": {
      "BuildingType": "YourModId_MyBuilding",
      "BuildingsEnabled": true
    },
    "YourModId_MyBuilding_VariantB": {
      "BuildingType": "YourModId_MyBuilding",
      "BuildingsEnabled": false
    }
  }
}
```

Content Patcherから、Config Optionやその他のConditionに応じて各Provider
Entryを個別に編集できます。

------------------------------------------------------------------------

## Building Lights

Building LightはBuildingへLight Sourceを追加します。

Lightの位置はBuilding左上のTileを基準に決まり、Buildingを移動すると自動的に追従します。

Buildingを解体するとLightも削除されます。

例:

``` json
{
  "Action": "EditData",
  "Target": "TsCore/BuildingProviders",
  "Entries": {
    "YourModId_MyBuildingProvider": {
      "BuildingType": "YourModId_MyBuilding",

      "Lights": [
        {
          "Id": "LeftLamp",
          "OffsetX": 0,
          "OffsetY": -2,
          "Radius": 2,
          "Color": "0,0,0"
        }
      ]
    }
  }
}
```

### Building LightのProperty

  -------------------------------------------------------------------------------------------
  Property         必須             デフォルト      説明
  ---------------- ---------------- --------------- -----------------------------------------
  `Id`             ✅               ---             Provider内で一意なLight ID。

  `Enabled`        任意             `true`          この個別Lightを有効または無効にします。

  `EnabledField`   任意             ---             この個別Lightの有効/無効を制御する
                                                    `CustomFields` のKey。

  `OffsetX`        任意             `0`             Building左上Tileからの横方向Tile
                                                    Offset。負の値にも対応しています。

  `OffsetY`        任意             `0`             Building左上Tileからの縦方向Tile
                                                    Offset。負の値にも対応しています。

  `Radius`         任意             `4`             Light Sourceの半径。

  `Color`          任意             `"0,0,0"`       `"R,G,B"` 形式のLight Color。
  -------------------------------------------------------------------------------------------

Lightの位置は、一致する各Buildingの現在位置を基準に計算されます。

同じ `BuildingType`
のBuildingが複数存在する場合、それぞれのBuildingに個別のLight
Instanceが作成されます。

Building
Lightは、ゲームがそのLocationを暗いと判定している場合にのみ有効になります。

------------------------------------------------------------------------

### すべてのLightを制御する

Provider内のすべてのLightは、次のように直接制御できます:

``` json
{
  "LightsEnabled": false
}
```

または `CustomFields` のKeyを使用します:

``` json
{
  "LightsEnabledField": "MyBuilding/LightsEnabled"
}
```

かつ:

``` json
"CustomFields": {
  "MyBuilding/LightsEnabled": "false"
}
```

両方の方法を同時に使用できます。

------------------------------------------------------------------------

### 個別のLightを制御する

各Lightにも、それぞれ `Enabled` と `EnabledField` Propertyがあります。

直接制御:

``` json
{
  "Id": "LeftLamp",
  "Enabled": false,
  "OffsetX": 0,
  "OffsetY": -2
}
```

`CustomFields` を使用:

``` json
{
  "Id": "LeftLamp",
  "EnabledField": "MyBuilding/LeftLampEnabled",
  "OffsetX": 0,
  "OffsetY": -2
}
```

かつ:

``` json
"CustomFields": {
  "MyBuilding/LeftLampEnabled": "true"
}
```

`Enabled` のデフォルトは `true` です。

`Enabled` と `EnabledField`
の両方を使用した場合、Lightを表示するには両方が有効である必要があります。

------------------------------------------------------------------------

## Building Draw Layers

Building Draw LayerはBuildingへ追加の描画Layerを追加します。

Building本来の `Data/Buildings` Entryで定義されているDraw
Layerに追加して描画されます。

T's CoreはBuildingに既存の `DrawLayers` を置き換えません。

例:

``` json
{
  "Action": "EditData",
  "Target": "TsCore/BuildingProviders",
  "Entries": {
    "YourModId_MyBuildingProvider": {
      "BuildingType": "YourModId_MyBuilding",

      "DrawLayers": [
        {
          "Id": "Animal",
          "SourceRect": {
            "X": 0,
            "Y": 144,
            "Width": 16,
            "Height": 16
          },
          "DrawPosition": "48, 8",
          "FrameDuration": 1500,
          "FrameCount": 6,
          "Condition": "TIME 600 1530, WEATHER Here Sun Wind"
        }
      ]
    }
  }
}
```

### Draw LayerのProperty

  ----------------------------------------------------------------------------------------------------------------------------------------
  Property                       必須             デフォルト      説明
  ------------------------------ ---------------- --------------- ------------------------------------------------------------------------
  `Id`                           ✅               ---             Provider内で一意なDraw Layer ID。

  `Enabled`                      任意             `true`          この個別Draw Layerを有効または無効にします。

  `EnabledField`                 任意             ---             この個別Draw Layerの有効/無効を制御する `CustomFields` のKey。

  `Texture`                      任意             Building        Layerに使用するTexture Asset。
                                                  texture         

  `SourceRect`                   ✅               ---             Texture内で描画するPixel範囲。

  `DrawPosition`                 任意             `0, 0`          Buildingを基準とした描画位置。

  `DrawInBackground`             任意             `false`         LayerをBuildingの前面ではなく背面に描画します。

  `SortTileOffset`               任意             `0`             描画順を計算するときに使用するY方向のTile Offset。

  `OnlyDrawIfChestHasContents`   任意             ---             指定したBuilding ChestにItemが入っている場合のみ描画します。

  `FrameDuration`                任意             `90`            各Animation
                                                                  Frameの表示時間（ミリ秒）。単一の値またはFrameごとの値を指定できます。

  `FrameCount`                   任意             `1`             Animation Frame数。

  `FramesPerRow`                 任意             `-1`            Spritesheetの1行あたりのAnimation Frame数。

  `AnimalDoorOffset`             任意             `0, 0`          BuildingのAnimal Doorの開閉状態に応じて適用するPixel Offset。

  `Condition`                    任意             ---             Layerを描画するために一致する必要があるGame State Query Condition。
  ----------------------------------------------------------------------------------------------------------------------------------------

------------------------------------------------------------------------

### Texture

`Texture` を省略すると、T's Coreは対象BuildingのTextureを使用します。

``` json
{
  "SourceRect": {
    "X": 0,
    "Y": 144,
    "Width": 16,
    "Height": 16
  }
}
```

カスタムTextureを指定することもできます:

``` json
{
  "Texture": "Mods/YourModId/MyBuildingTexture",
  "SourceRect": {
    "X": 0,
    "Y": 0,
    "Width": 16,
    "Height": 16
  }
}
```

------------------------------------------------------------------------

### Season Offset

T's Coreは、対象Buildingの `Data/Buildings` に設定された `SeasonOffset`
をDraw LayerのSource Rectangleへ適用します。

そのため、Building本来のTextureを使用するDraw
Layerは、Building本体と同じ季節ごとのTexture
Offsetへ自動的に追従します。

たとえばBuildingに次の設定がある場合:

``` json
"SeasonOffset": {
  "X": 160,
  "Y": 0
}
```

Draw Layerの `SourceRect` は現在の季節に応じて自動的に調整されます。

------------------------------------------------------------------------

### Animation

Animation付きDraw Layerでは、バニラのBuilding Draw
Layerと同じ基本的なFrame配置動作を使用します。

例:

``` json
{
  "Id": "Animal",
  "SourceRect": {
    "X": 0,
    "Y": 144,
    "Width": 16,
    "Height": 16
  },
  "DrawPosition": "48, 8",
  "FrameDuration": 1500,
  "FrameCount": 6,
  "FramesPerRow": -1
}
```

`FramesPerRow: -1`
の場合、FrameはSpritesheetから横方向に読み取られます。

`FrameDuration` では、Frameごとに個別の表示時間を指定することもできます:

``` json
{
  "FrameDuration": [ 800, 80, 80, 80 ],
  "FrameCount": 4
}
```

Frameごとの表示時間を使用する場合、値の数は `FrameCount`
と一致させてください。

------------------------------------------------------------------------

### 条件付きDraw Layer

`Condition` PropertyにはStardew ValleyのGame State Queryを指定できます。

例:

``` json
"Condition": "TIME 600 1530, WEATHER Here Sun Wind"
```

このLayerは、現在の天候が `Sun` または `Wind`
で、6:00～15:30の間だけ表示されます。

Conditionには通常のGame State Query構文を使用できます。

例:

``` json
"Condition": "TIME 600 1530, WEATHER Here Sun Wind, !SEASON Winter"
```

or:

``` json
"Condition": "TIME 600 1530, WEATHER Here Sun, SEASON Winter"
```

`Condition` を省略した場合、Draw Layerは常に描画対象になります。

------------------------------------------------------------------------

### 前面と背面

デフォルトでは、T's CoreのDraw LayerはBuildingの前面に描画されます。

Set:

``` json
"DrawInBackground": true
```

を指定すると、代わりにLayerをBuildingの背面へ描画します。

------------------------------------------------------------------------

### すべてのDraw Layerを制御する

Provider内のすべてのDraw Layerは、次のように直接制御できます:

``` json
{
  "DrawLayersEnabled": false
}
```

または `CustomFields` のKeyを使用します:

``` json
{
  "DrawLayersEnabledField": "MyBuilding/DrawLayersEnabled"
}
```

かつ:

``` json
"CustomFields": {
  "MyBuilding/DrawLayersEnabled": "false"
}
```

両方の方法を同時に使用できます。

------------------------------------------------------------------------

### 個別のDraw Layerを制御する

各Draw Layerにも、それぞれ `Enabled` と `EnabledField`
Propertyがあります。

直接制御:

``` json
{
  "Id": "Animal",
  "Enabled": false,
  "SourceRect": {
    "X": 0,
    "Y": 144,
    "Width": 16,
    "Height": 16
  }
}
```

`CustomFields` を使用:

``` json
{
  "Id": "Animal",
  "EnabledField": "MyBuilding/AnimalEnabled",
  "SourceRect": {
    "X": 0,
    "Y": 144,
    "Width": 16,
    "Height": 16
  }
}
```

かつ:

``` json
"CustomFields": {
  "MyBuilding/AnimalEnabled": "true"
}
```

`Enabled` のデフォルトは `true` です。

`Enabled` と `EnabledField` の両方を使用した場合、Draw
Layerを表示するには両方が有効である必要があります。

------------------------------------------------------------------------

## Valley Farm Only

`ValleyFarmOnly` を使用すると、BuildingをメインのValley
Farmの建築メニューだけに制限できます。

例:

``` json
{
  "Action": "EditData",
  "Target": "TsCore/BuildingProviders",
  "Entries": {
    "YourModId_MyBuildingProvider": {
      "BuildingType": "YourModId_MyBuilding",
      "ValleyFarmOnly": true
    }
  }
}
```

`true` にすると、建築先としてメインのValley
Farmが選択されている場合にのみBuildingが建築メニューへ表示されます。

カスタムの建築可能Mapなど、別の建築可能Locationが建築先になっている場合は表示されません。

``` text
ValleyFarmOnly: false / omitted
→ Normal Stardew Valley construction rules are used.

ValleyFarmOnly: true
→ The building is available only when constructing on the main Valley farm.
```

この制限は、対応するBuilding Providerが有効な間だけ適用されます。

`BuildingsEnabled` または `BuildingsEnabledField`
によってProviderが無効になっている場合、そのProviderの `ValleyFarmOnly`
制限は適用されません。

> **注意:** `ValleyFarmOnly`
> が制御するのは、選択した建築先Locationの建築メニューにBuildingを表示するかどうかです。Buildingの
> `Data/Buildings` Entry自体は変更しません。

------------------------------------------------------------------------

<a id="example"></a>

## 使用例

次の例では、複数のBuilding Services機能を1つのContent Patcher
Patchにまとめています:

``` json
{
  "Action": "EditData",
  "Target": "TsCore/BuildingProviders",
  "Entries": {
    "YourModId_MyBuildingProvider": {
      "BuildingType": "YourModId_MyBuilding",

      "ValleyFarmOnly": true,

      "BuildingsEnabled": true,
      "BuildingsEnabledField": "MyBuilding/BuildingsEnabled",

      "LightsEnabled": true,
      "LightsEnabledField": "MyBuilding/LightsEnabled",

      "DrawLayersEnabled": true,
      "DrawLayersEnabledField": "MyBuilding/DrawLayersEnabled",

      "Lights": [
        {
          "Id": "LeftLamp",
          "Enabled": true,
          "EnabledField": "MyBuilding/LeftLampEnabled",
          "OffsetX": 0,
          "OffsetY": -2,
          "Radius": 2,
          "Color": "0,0,0"
        }
      ],

      "DrawLayers": [
        {
          "Id": "Animal",
          "Enabled": true,
          "EnabledField": "MyBuilding/AnimalEnabled",
          "SourceRect": {
            "X": 0,
            "Y": 144,
            "Width": 16,
            "Height": 16
          },
          "DrawPosition": "48, 8",
          "FrameDuration": 1500,
          "FrameCount": 6,
          "Condition": "TIME 600 1530, WEATHER Here Sun Wind"
        }
      ]
    }
  }
}
```

このProviderは:

-   `YourModId_MyBuilding` を対象にします。
-   Buildingの建築メニューへの表示をメインのValley
    Farmだけに制限します。
-   直接指定と `CustomFields` の両方による有効/無効制御に対応します。
-   夜間用のBuilding Lightを追加します。
-   個別のLightを有効または無効にできます。
-   Game State Query Condition付きのAnimation Draw Layerを追加します。
-   個別のDraw Layerを有効または無効にできます。
-   すべてのLightとすべてのDraw Layerをそれぞれ独立して制御できます。

------------------------------------------------------------------------

<a id="debugging"></a>

## デバッグ

T's Coreでは、Building
Providerと現在Farmに設置されているBuildingを確認するためのデバッグコマンドを提供しています。

### Content Patcher Content Packの再読み込み

Building Providerは `TsCore/BuildingProviders` Data
Assetを通して登録されるため、そのAssetを編集しているContent Patcher
Content Packを再読み込みすることで更新できます。

T's Coreでは次のコマンドを提供しています:

``` text
tscore_cp_reload <ContentPackId>
```

ゲームの実行中にContent Patcher Content Packを再読み込みできます。

`tscore_cp_reload` やその他のContent
Patcher連携機能の詳細については、[Content Patcher
Integration](ModderGuide_ContentPatcherIntegration.md)
ガイドを参照してください。

------------------------------------------------------------------------

### Building Providerの確認

Use:

``` text
tscore_debug_buildings
```

現在登録されているBuilding Providerを確認できます。

特定のProviderを確認する場合:

``` text
tscore_debug_buildings YourModId_MyBuildingProvider
```

------------------------------------------------------------------------

### Farm Buildingの確認

Use:

``` text
tscore_debug_farmbuildings
```

現在メインFarmに設置されているBuildingを確認できます。

Building Providerで使用する `BuildingType` を確認する場合に便利です。

> **注意:** `tscore_debug_farmbuildings`
> を使用する前に、セーブデータを読み込んでおく必要があります。

------------------------------------------------------------------------

<a id="notes"></a>

## 注意事項

Building ServicesはContent Patcher Content
Packから使用することを想定しており、C#コードは必要ありません。

Building ProviderはカスタムData Asset `TsCore/BuildingProviders`
を通して登録します。

Building Providerの機能は、Building本来の `Data/Buildings`
の動作に追加して適用されます。

Provider IDは `TsCore/BuildingProviders` のEntry Keyです。他のContent
Packとの競合を避けるため、自分のModのUniqueIDなど、一意なPrefixを使用することを推奨します。

直接指定する `Enabled` PropertyはContent
Patcherから編集できます。一方、対象Buildingの `CustomFields`
から有効状態をT's Coreに読み取らせたい場合は `EnabledField`
Propertyを使用できます。

今後のT's Coreで、Building Servicesの機能が追加される場合があります。

------------------------------------------------------------------------

## Modder Guide

-   ← [Map Properties](ModderGuide_MapProperties.md)
-   ↑ [Guide Index](#top)
-   → [BigCraftable Extension](ModderGuide_BigCraftableExtension.md)

← [READMEに戻る](../../README.md)

← [Modder Guideに戻る](ModderGuide.md)
