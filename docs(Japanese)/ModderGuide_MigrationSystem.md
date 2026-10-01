# 📖 Modder Guide

このガイドでは、**T's Core** がContent
Patcher向けに提供している公開機能の使用方法を説明します。

<a id="top"></a>

## Guide Index

-   📄 [Relationship Services](ModderGuide_RelationshipServices.md)
-   📄 [Location Services](ModderGuide_LocationServices.md)
-   📄 [Warp Services](ModderGuide_WarpServices.md)
-   📄 [Map Properties](ModderGuide_MapProperties.md)
-   📄 [Building Services](ModderGuide_BuildingServices.md)
-   📄 [BigCraftable Extension](ModderGuide_BigCraftableExtension.md)
-   📄 [Dialogue System](ModderGuide_DialogueSystem.md)
-   ✅ **Migration System** *(現在のページ)*
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

# Migration System

Migration Systemを使用すると、Modの内部IDを変更した際に、既存のSave
Dataで使用されているIDをContent Patcher Content Packから移行できます。

Updateによって、プレイヤーのSaveにすでに保存されている可能性があるIDを変更する場合に便利です。

Migration定義は、T's Coreの次のカスタムData Assetを通して登録します:

``` text
TsCore/Migrations
```

現在、Migration Systemはすでに建築済みのBuildingに対する **Building Type
ID** の移行に対応しています。

------------------------------------------------------------------------

## 目次

-   [Content Patcher Setup](#content-patcher-setup)
-   [Migration Definitions](#migration-definitions)
-   [Migration Properties](#migration-properties)
-   [Migration IDs](#migration-ids)
-   [Building Migration](#building-migration)
-   [When Migrations Are Applied](#when-migrations-are-applied)
-   [Multiple Migrations](#multiple-migrations)
-   [Duplicate Migrations](#duplicate-migrations)
-   [Examples](#examples)
-   [Notes](#notes)

------------------------------------------------------------------------

<a id="content-patcher-setup"></a>

## Content Patcherのセットアップ

Migration定義は、Content Patcherから次のカスタムData Assetへ追加します:

``` text
TsCore/Migrations
```

通常のContent Patcher Content Packを使用します。

### manifest.json

``` json
{
  "Name": "[CP] My Mod",
  "Author": "YourName",
  "Version": "1.0.0",
  "UniqueID": "YourName.MyMod",
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

その後、Content PatcherのContent Fileで `EditData`
を使用してMigration定義を追加できます。

基本例:

``` json
{
  "Action": "EditData",
  "Target": "TsCore/Migrations",
  "Entries": {
    "YourModId_MyBuildingMigration": {
      "Type": "Building",
      "OldId": "YourModId_OldBuilding",
      "NewId": "YourModId_NewBuilding"
    }
  }
}
```

EntryのKey:

``` text
YourModId_MyBuildingMigration
```

がMigration IDです。

> **推奨:**
> 他Modとの競合を避けるため、一意またはNamespace化したMigration
> IDを使用してください。

------------------------------------------------------------------------

<a id="migration-definitions"></a>

## Migration定義

次のData Asset内の各Entryが:

``` text
TsCore/Migrations
```

1つのMigrationを定義します。

例:

``` json
{
  "Action": "EditData",
  "Target": "TsCore/Migrations",
  "Entries": {
    "YourModId_MyBuildingMigration": {
      "Type": "Building",
      "OldId": "YourModId_OldBuilding",
      "NewId": "YourModId_NewBuilding"
    }
  }
}
```

これはT's Coreに対して、次のIDを使用している既存Save Dataを:

``` text
YourModId_OldBuilding
```

次のIDへ移行するよう指定します:

``` text
YourModId_NewBuilding
```

対応するMigration Typeが処理される際に移行されます。

現在対応しているMigration Typeは次のものだけです:

``` text
Building
```

今後のT's Coreで、対応するMigration Typeが追加される場合があります。

------------------------------------------------------------------------

<a id="migration-properties"></a>

## MigrationのProperty

各Migration定義では次のPropertyを使用できます:

  ------------------------------------------------------------------------
  Property              必須                  説明
  --------------------- --------------------- ----------------------------
  `Type`                ✅                    MigrationのType。現在は
                                              `Building`
                                              のみに対応しています。

  `OldId`               ✅                    既存Save
                                              Dataに保存されている旧ID。

  `NewId`               ✅                    旧IDを置き換える新ID。
  ------------------------------------------------------------------------

3つのPropertyはすべて必須です。

例:

``` json
{
  "Action": "EditData",
  "Target": "TsCore/Migrations",
  "Entries": {
    "YourModId_BarnMigration": {
      "Type": "Building",
      "OldId": "YourModId_OldBarn",
      "NewId": "YourModId_NewBarn"
    }
  }
}
```

`OldId` と `NewId` は異なる値である必要があります。

両方のIDが同じ値のMigrationは無効となり、無視されます。

> **注意:** IDには正確な内部値を指定してください。Building
> IDの照合には、そのBuildingに保存されている正確な `BuildingType`
> IDが使用されます。

------------------------------------------------------------------------

<a id="migration-ids"></a>

## Migration ID

次のData Asset内にあるDictionary EntryのKeyが:

``` text
TsCore/Migrations
```

Migration IDとして使用されます。

例:

``` json
{
  "Action": "EditData",
  "Target": "TsCore/Migrations",
  "Entries": {
    "YourModId_MyBuildingMigration": {
      "Type": "Building",
      "OldId": "YourModId_OldBuilding",
      "NewId": "YourModId_NewBuilding"
    }
  }
}
```

この例でのMigration IDは:

``` text
YourModId_MyBuildingMigration
```

Migration IDは `OldId` や `NewId` とは別のものです。

Migration定義自体を識別するために使用されます。

Migration IDは一意にしてください。

自分のModのUniqueIDを基にNamespace化したIDを使用することを推奨します。

例:

``` text
YourName.MyMod_BarnMigration
```

------------------------------------------------------------------------

## Building Migration

Building
Migrationは、すでに建築されプレイヤーのSaveに保存されているBuildingのBuilding
Type IDを更新します。

Modが次のData Asset内のBuilding IDを変更する場合に便利です:

``` text
Data/Buildings
```

たとえば、古いVersionのModでは次のIDを登録していて:

``` text
YourModId_OldBuilding
```

新しいVersionでは次のIDを使用する場合があります:

``` text
YourModId_NewBuilding
```

Migrationを行わない場合、既存Saveですでに建築されているBuildingは旧IDを参照したままになる可能性があります。

Building Migrationを使用すると、それらのBuildingを自動的に更新できます:

``` json
{
  "Action": "EditData",
  "Target": "TsCore/Migrations",
  "Entries": {
    "YourModId_BuildingMigration": {
      "Type": "Building",
      "OldId": "YourModId_OldBuilding",
      "NewId": "YourModId_NewBuilding"
    }
  }
}
```

Saveを読み込むと、T's Coreは設置済みBuildingから次のIDを検索し:

``` text
YourModId_OldBuilding
```

保存されているBuilding Type IDを次のIDへ変更します:

``` text
YourModId_NewBuilding
```

### Data/Buildingsの検証

Building Type IDを変更する前に、T's Coreは `NewId` が現在次のData
Assetに存在することを確認します:

``` text
Data/Buildings
```

例:

``` json
{
  "Action": "EditData",
  "Target": "TsCore/Migrations",
  "Entries": {
    "YourModId_BuildingMigration": {
      "Type": "Building",
      "OldId": "YourModId_OldBuilding",
      "NewId": "YourModId_NewBuilding"
    }
  }
}
```

が適用されるのは、次のIDが:

``` text
YourModId_NewBuilding
```

現在の `Data/Buildings` Assetに存在する場合だけです。

移行先IDが存在しない場合、Buildingは移行されず、T's
Coreが警告をログへ出力します。

これにより、既存Buildingが現在利用できないBuilding
Typeへ移行されることを防ぎます。

### 既存のBuilding

Building Migrationは、既存Buildingに保存されているBuilding Type
IDを変更します。

代わりのBuildingを新しく作成したり、既存Buildingを移動したりはしません。

Buildingは現在の設置位置を維持したまま、Building Type IDだけが `OldId`
から `NewId` へ変更されます。

### Location

T's Coreは読み込まれているGame Location内のBuildingを確認します。

対象はMain Farmに設置されたBuildingだけに限定されません。

LocationにBuildingが存在し、そのうちのBuildingが一致する `OldId`
を使用している場合、そのLocationでもMigrationを適用できます。

------------------------------------------------------------------------

<a id="when-migrations-are-applied"></a>

## Migrationが適用されるタイミング

Migration定義は次のカスタムData Assetを通して提供されます:

``` text
TsCore/Migrations
```

Building MigrationはSaveを読み込んだときに適用されます。

基本的な処理の流れ:

``` text
Content Patcher edits TsCore/Migrations
↓
A save is loaded
↓
T's Core reads the Migration definitions
↓
T's Core checks placed buildings
↓
A building matches OldId
↓
T's Core verifies that NewId exists in Data/Buildings
↓
The Building Type ID is changed to NewId
```

Buildingが一度移行されると、そのBuilding Type IDは `OldId`
と一致しなくなります。

更新後のBuilding
Typeは、その後GameをSaveした際に通常どおり保存されます。

以降のSave読み込みでは、そのBuildingの現在のBuilding
Typeが再び設定済みの `OldId`
と一致しない限り、すでに移行済みのBuildingにそのMigrationが再適用されることはありません。

> **注意:** Migration SystemはSave読み込み時に既存Save
> Dataを処理することを目的としています。現在、T's
> CoreにはGame実行中にMigrationを手動適用するConsole
> Commandはありません。

------------------------------------------------------------------------

<a id="multiple-migrations"></a>

## 複数のMigration

同じ `EditData` Patchに複数のMigration定義を追加できます。

例:

``` json
{
  "Action": "EditData",
  "Target": "TsCore/Migrations",
  "Entries": {
    "YourModId_BarnMigration": {
      "Type": "Building",
      "OldId": "YourModId_OldBarn",
      "NewId": "YourModId_NewBarn"
    },
    "YourModId_CoopMigration": {
      "Type": "Building",
      "OldId": "YourModId_OldCoop",
      "NewId": "YourModId_NewCoop"
    },
    "YourModId_ShedMigration": {
      "Type": "Building",
      "OldId": "YourModId_OldShed",
      "NewId": "YourModId_NewShed"
    }
  }
}
```

また、次のData Assetを編集するContent Patcher
Patchを複数使用することもできます:

``` text
TsCore/Migrations
```

各MigrationはData Asset内の個別Entryとして保存されます。

------------------------------------------------------------------------

<a id="duplicate-migrations"></a>

## 重複するMigration

次の組み合わせが同じ場合、使用できる有効なMigrationは1つだけです:

``` text
Type + OldId
```

たとえば、次の2つの定義は競合します:

``` json
{
  "Action": "EditData",
  "Target": "TsCore/Migrations",
  "Entries": {
    "YourModId_MigrationA": {
      "Type": "Building",
      "OldId": "YourModId_OldBuilding",
      "NewId": "YourModId_NewBuildingA"
    },
    "YourModId_MigrationB": {
      "Type": "Building",
      "OldId": "YourModId_OldBuilding",
      "NewId": "YourModId_NewBuildingB"
    }
  }
}
```

どちらの定義も同じ `Building` ID:

``` text
YourModId_OldBuilding
```

を異なる移行先IDへ変更しようとしています。

T's
Coreは最初に処理した有効なMigrationを使用し、重複するMigrationを無視します。

重複が検出されると、SMAPI Logへ警告が出力されます。

> **推奨:** 各旧IDの移行先は1つだけにしてください。同じ `Type` と
> `OldId` に対して複数の `NewId` 候補を定義しないでください。

------------------------------------------------------------------------

<a id="examples"></a>

## 使用例

### Building Typeの名前を変更する

古いVersionのModで次のIDを使用していて:

``` text
YourModId_MyBarn
```

新しいVersionで `Data/Buildings` のIDを次の値へ変更したとします:

``` text
YourModId_LargeBarn
```

Migration定義を追加します:

``` json
{
  "Action": "EditData",
  "Target": "TsCore/Migrations",
  "Entries": {
    "YourModId_MyBarnMigration": {
      "Type": "Building",
      "OldId": "YourModId_MyBarn",
      "NewId": "YourModId_LargeBarn"
    }
  }
}
```

すでに `YourModId_MyBarn`
を建築しているプレイヤーは、Saveを読み込んだときに保存済みのBuilding
Typeを更新できるようになります。

------------------------------------------------------------------------

### 複数の旧Building IDを移行する

Updateで複数のBuilding IDを変更する場合:

``` json
{
  "Action": "EditData",
  "Target": "TsCore/Migrations",
  "Entries": {
    "YourModId_BarnMigration": {
      "Type": "Building",
      "OldId": "YourModId_OldBarn",
      "NewId": "YourModId_Barn"
    },
    "YourModId_CoopMigration": {
      "Type": "Building",
      "OldId": "YourModId_OldCoop",
      "NewId": "YourModId_Coop"
    },
    "YourModId_ShedMigration": {
      "Type": "Building",
      "OldId": "YourModId_OldShed",
      "NewId": "YourModId_Shed"
    }
  }
}
```

Save読み込み時に、設置済みBuildingがそれぞれ個別に確認されます。

現在のBuilding Typeがいずれかの `OldId`
と一致するBuildingだけが変更されます。

------------------------------------------------------------------------

### 古いVersionとの互換性を維持する

新しいIDを導入したUpdate後も、Migration定義をContent Patcher Content
Pack内に残しておくことができます。

例:

``` json
{
  "Action": "EditData",
  "Target": "TsCore/Migrations",
  "Entries": {
    "YourModId_BarnV1Migration": {
      "Type": "Building",
      "OldId": "YourModId_Barn_V1",
      "NewId": "YourModId_Barn"
    },
    "YourModId_BarnV2Migration": {
      "Type": "Building",
      "OldId": "YourModId_Barn_V2",
      "NewId": "YourModId_Barn"
    }
  }
}
```

これにより、どちらの旧IDで保存されているBuildingも現在のIDへ移行できます。

既存Buildingが一度移行されると、そのBuildingは新IDを使用するため、これらの定義とは一致しなくなります。

------------------------------------------------------------------------

<a id="notes"></a>

## 注意事項

Migration Systemは、Modが内部IDを変更した際に、既存Save
Dataへすでに保存されているIDを更新するための機能です。

Migration定義はContent Patcherから次のData Assetへ登録します:

``` text
TsCore/Migrations
```

現在対応しているのはBuilding TypeのMigrationだけです。

Building Migrationは:

-   すでに建築済みのBuildingに適用されます。
-   Save読み込み時に実行されます。
-   Buildingに保存されているBuilding Type IDを変更します。
-   変更を適用する前に、移行先の `NewId` が `Data/Buildings`
    に存在することを確認します。
-   Main Farm以外のLocationにあるBuildingにも適用できます。
-   Buildingの移動や再作成は行いません。

旧IDで作成されたSaveをMod側でサポートする期間中は、基本的にMigration定義も残しておいてください。

`OldId` と `NewId` には正確な内部IDを使用してください。

Migration
IDには一意な値を使用し、できれば自分のModのUniqueIDを使ってNamespace化してください。

同じ `Type` と `OldId` に対して複数のMigrationを定義しないでください。

無効なMigration定義は無視され、SMAPI
Logへ警告が出力される場合があります。

今後のT's Coreで、対応するMigration Typeが追加される場合があります。

------------------------------------------------------------------------

## Modder Guide

-   ← [Dialogue System](ModderGuide_DialogueSystem.md)
-   ↑ [Guide Index](#top)
-   → [Notification System](ModderGuide_NotificationSystem.md)

← [READMEに戻る](../../README.md)

← [Modder Guideに戻る](ModderGuide.md)
