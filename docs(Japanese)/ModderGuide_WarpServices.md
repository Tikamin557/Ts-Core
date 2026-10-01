# 📖 Modder Guide

このガイドでは、**T's Core** がContent
Patcher向けに提供している公開機能の使用方法を説明します。

<a id="top"></a>

## Guide Index

-   📄 [Relationship Services](ModderGuide_RelationshipServices.md)
-   📄 [Location Services](ModderGuide_LocationServices.md)
-   ✅ **Warp Services** *(現在のページ)*
-   📄 [Map Properties](ModderGuide_MapProperties.md)
-   📄 [Building Services](ModderGuide_BuildingServices.md)
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

# Warp Services

Warp Servicesは、Content
Patcherから利用できる再利用可能なワープ機能を提供します。

T's Coreは、カスタムWarp Action、組み込みWarp Provider、およびContent
Patcher Packから独自のWarp Providerを登録できるカスタムData
Assetを提供します。

``` text
TsCore/WarpProviders
```

C#コードは必要ありません。

このガイドでは、Warp Actionの使用方法、カスタムWarp
Providerの登録方法、およびContent Patcher
Packへの組み込み方法を説明します。

------------------------------------------------------------------------

## 目次

-   [Warp Actions](#warp-actions)
-   [Tile Property Warp](#tile-property-warp)
-   [Warp Providers](#warp-providers)
-   [Content Packのセットアップ](#content-pack-setup)
-   [Warp Providerの登録](#registering-warp-providers)
-   [Warp Providerの種類](#warp-provider-types)
-   [標準Warp Provider](#standard-warp-providers)
-   [特殊な組み込みWarp Provider](#special-built-in-warp-providers)
-   [使用例](#example)
-   [デバッグ](#debugging)
-   [注意事項](#notes)

------------------------------------------------------------------------

## Warp Actions

T's Coreでは、3種類のカスタムWarp Actionを提供しています。

  --------------------------------------------------------------------------------------------------------------------------
  Action                      説明
  --------------------------- ----------------------------------------------------------------------------------------------
  `TsCoreWarp`                Magic Warpの視覚エフェクトを使用せず、通常のワープを実行します。

  `TsCoreMagicWarp`           Stardew Valley標準のMagic Warpに近いエフェクトを使用してワープします。

  `TsCoreMagicWarp_Simple`    簡略化したMagic Warpを実行します。標準のMagic
                              Warpエフェクトの一部を省略し、ワープアニメーション中もプレイヤーが表示されたままになります。
  --------------------------------------------------------------------------------------------------------------------------

3つのActionすべてで、以下を利用できます。

-   Warp Providers
-   Location名
-   座標の直接指定
-   `TsCoreWarpPoint` を使用したTile Property Warpの行き先
-   任意の向き指定
-   カスタムAudio Cue
-   Audio Cueの繰り返し再生
-   Audio Cueの再生間隔指定
-   Blackout時間の指定
-   Audio Cueの再生開始遅延

以下で使用できます。

-   Tile Actions
-   Touch Actions

> **注意:** カスタムWarp ActionをTile
> Actionとして使用すると、通常Stardew Valleyは旧仕様由来の **"unknown
> warp property"** 警告をSMAPIコンソールへ出力します。T's Coreは、T's
> CoreのWarp Actionについてこの警告を自動的に抑制します。

------------------------------------------------------------------------

### 構文

T's CoreのWarp Actionは、すべて同じ構文を使用します。

#### Provider / Location

``` text
<Action> <ProviderOrLocation> [FacingDirection] [AudioCue] [RepeatCount] [IntervalMs] [BlackoutDurationMs] [AudioStartDelayMs]
```

#### 座標の直接指定

``` text
<Action> <LocationName> <X> <Y> [FacingDirection] [AudioCue] [RepeatCount] [IntervalMs] [BlackoutDurationMs] [AudioStartDelayMs]
```

`<Action>` は次のいずれかに置き換えます。

-   `TsCoreWarp`
-   `TsCoreMagicWarp`
-   `TsCoreMagicWarp_Simple`

例:

``` text
TsCoreWarp FarmHouseFront
TsCoreMagicWarp FarmHouseFront Down
TsCoreMagicWarp_Simple FarmHouseFront Auto wand
TsCoreMagicWarp FarmHouseFront Auto wand 3 100
TsCoreMagicWarp FarmHouseFront Auto wand 3 100 200
TsCoreMagicWarp FarmHouseFront Auto wand 3 100 200 250

TsCoreWarp Farm 64 15
TsCoreMagicWarp Farm 64 15 Down
TsCoreMagicWarp_Simple Farm 64 15 Auto wand
TsCoreMagicWarp Farm 64 15 Auto wand 3 100 200 250
```

Location名だけを指定した場合、T's
CoreはそのLocationのデフォルトのワープ地点を自動的に使用します。

> **注意:**
> 省略可能な引数は位置によって判定されます。後ろの引数を指定する場合は、それより前の引数もすべて指定する必要があります。

------------------------------------------------------------------------

## Tile Property Warp

T's CoreのWarp Actionでは、固定座標の代わりにカスタムTile
Propertyから行き先を解決できます。

行き先MapのTileに、次のPropertyを追加します。

``` text
TsCoreWarpPoint = <UniqueKey> [OffsetX] [OffsetY]
```

そのKeyをT's CoreのWarp Actionで指定します。

``` text
<Action> <LocationName> <UniqueKey> [FacingDirection] [AudioCue] [RepeatCount] [IntervalMs] [BlackoutDurationMs] [AudioStartDelayMs]
```

たとえば、行き先Tileに次のPropertyを設定します。

``` text
TsCoreWarpPoint = MyRoomEntrance
```

そしてWarp Actionでは次のように指定できます。

``` text
TsCoreMagicWarp Farmhouse MyRoomEntrance Left
```

T's Coreは指定されたLocationのすべてのLayerから一致する
`TsCoreWarpPoint` の値を検索し、そのTileを行き先として使用します。

### 行き先のOffset

Propertyの値には、任意でX・YのOffsetを追加できます。

``` text
TsCoreWarpPoint = MyRoomEntrance 1 -2
```

この例では、Propertyが設定されたTileから右へ1Tile、上へ2Tile移動した位置が最終的な行き先になります。

どちらのOffsetも整数で指定する必要があります。省略した場合は `0`
になります。

### Unique Key

`TsCoreWarpPoint` の最初の値が、Warp Actionから指定するUnique
Keyになります。

Keyは完全一致で判定されます。各行き先Location内で重複しない値を使用してください。

同じLocation内で同じKeyが複数見つかった場合、T's
Coreは警告をログへ出力し、最初に見つかったTileを使用します。

Offsetが不正な場合、またはPropertyに対応している3つを超える値が含まれている場合、そのPropertyは無視され、警告がログへ出力されます。

#<a id="example"></a>

## 使用例

MapのTile Property:

``` text
TsCoreWarpPoint = Example_SpouseRoom_Upper
```

Warp Action:

``` text
TsCoreMagicWarp Farmhouse Example_SpouseRoom_Upper Left
```

これは、Content
Patcherの設定や他のMap編集によって行き先座標が変化するMapで便利です。Map
Patchによって `TsCoreWarpPoint` を設定したTileの位置が移動しても、Warp
Action側では同じUnique Keyを使い続けられます。

------------------------------------------------------------------------

### 向きの指定

ワープ後の向きは、名前または数値で指定できます。

  Direction     数値 説明
  ----------- ------ --------------------------------------
  `Up`           `0` ワープ後に上を向きます。
  `Right`        `1` ワープ後に右を向きます。
  `Down`         `2` ワープ後に下を向きます。
  `Left`         `3` ワープ後に左を向きます。
  `Auto`         `4` プレイヤーの現在の向きを維持します。

`Auto`
は、後ろにある省略可能な引数を指定したいものの、プレイヤーの現在の向きは変更したくない場合に便利です。

例:

``` text
TsCoreMagicWarp FarmHouseFront Auto wand
```

------------------------------------------------------------------------

### カスタムAudio Cue

T's CoreのWarp Actionでは、ワープ実行時に任意のAudio Cueを再生できます。

Audio Cueは `FacingDirection` の直後に指定します。

``` text
TsCoreMagicWarp FarmHouseFront Down wand
TsCoreMagicWarp_Simple FarmHouseFront Auto wand
TsCoreWarp Farm 64 15 Left doorClose
```

プレイヤーの向きを変更せずにAudio Cueを指定したい場合は、`Auto`
を使用します。

``` text
TsCoreMagicWarp_Simple FarmHouseFront Auto MyAudioCue
```

`TsCoreMagicWarp` と `TsCoreMagicWarp_Simple` のデフォルトAudio
Cueは次のとおりです。

``` text
wand
```

カスタムAudio Cueを指定すると、デフォルトの `wand`
の代わりにそのCueが再生されます。

`TsCoreWarp` はデフォルトではワープ音を再生しませんが、カスタムAudio
Cueを指定することはできます。

------------------------------------------------------------------------

### Audio Cueの繰り返し回数

`RepeatCount` はAudio Cueを再生する回数を指定します。

``` text
TsCoreMagicWarp FarmHouseFront Auto wand 3
```

この例では、`wand` が3回再生されます。

値は `1` 以上である必要があります。

デフォルト:

``` text
1
```

------------------------------------------------------------------------

### Audio Cueの再生間隔

`IntervalMs` は、Audio
Cueを繰り返し再生する際の間隔をミリ秒単位で指定します。

``` text
TsCoreMagicWarp FarmHouseFront Auto wand 3 200
```

この例では、Audio Cueを200ms間隔で3回再生します。

値は `0` 以上である必要があります。

デフォルト:

``` text
100
```

`IntervalMs` が実際に意味を持つのは、`RepeatCount` が `1`
より大きい場合です。

------------------------------------------------------------------------

### Blackout時間

`BlackoutDurationMs`
は、実際にワープを実行する前に画面を完全な黒の状態で維持する時間を指定します。

``` text
TsCoreMagicWarp FarmHouseFront Auto wand 1 100 500
```

この例では、ワープが実行される前に画面が500msの間、完全な黒の状態になります。

値は `0` 以上である必要があります。

この引数を省略すると、選択したWarp
ActionのデフォルトBlackout時間が使用されます。

現在のデフォルト:

``` text
100 ms
```

`0` を指定すると、画面が完全に黒くなった直後にワープを実行します。

------------------------------------------------------------------------

### Audio Cueの再生開始遅延

`AudioStartDelayMs` は、Audio Cueの再生を開始するまでT's
Coreが待機する時間を指定します。

``` text
TsCoreMagicWarp FarmHouseFront Auto wand 1 100 100 300
```

この例では、300msの遅延後に `wand` の再生が始まります。

値は `0` 以上である必要があります。

デフォルト:

``` text
0
```

繰り返し再生と組み合わせた場合、再生タイミングは開始遅延を基準に計算されます。

例:

``` text
TsCoreMagicWarp FarmHouseFront Auto wand 3 200 100 300
```

Audio Cueはおおよそ次のタイミングで再生されます。

``` text
300 ms
500 ms
700 ms
```

Warp Actionの開始後です。

------------------------------------------------------------------------

### 省略可能な引数の順序

省略可能な引数の完全な順序は次のとおりです。

  ------------------------------------------------------------------------------------------------
  引数                   デフォルト               説明
  ---------------------- ------------------------ ------------------------------------------------
  `FacingDirection`      現在の向き               ワープ後にプレイヤーが向く方向。

  `AudioCue`             Magic系は                ワープ中に再生するAudio Cue。
                         `wand`、通常Warpはなし   

  `RepeatCount`          `1`                      Audio Cueを再生する回数。

  `IntervalMs`           `100`                    Audio Cueを繰り返し再生する間隔。

  `BlackoutDurationMs`   `100`                    ワープ前に画面を完全な黒の状態で維持する時間。

  `AudioStartDelayMs`    `0`                      Audio Cueの再生を開始するまでの遅延。
  ------------------------------------------------------------------------------------------------

これらの引数は位置によって判定されるため、後ろのオプションを指定する場合は、それより前の値も指定する必要があります。

たとえば、他の設定を通常の値のままにしてBlackout時間だけを変更する場合は、次のように指定します。

``` text
TsCoreMagicWarp FarmHouseFront Auto wand 1 100 500
```

------------------------------------------------------------------------

## Warp Providers

Warp Providerを使用すると、Content Patcher
PackからMap名や座標を直接記述する代わりに、**Provider ID**
で行き先を参照できます。

例:

``` text
TsCoreWarp FarmHouseFront
```

は、次のような固定の行き先の代わりに使用できます。

``` text
Warp Farm 64 15
```

Providerを使用することでContent Patcher
Packを保守しやすくなり、Buildingを移動したりWarpの行き先を変更したりするカスタムMapや他Modとの互換性も高められます。

たとえば `FarmHouseFront` はFarmhouse入口を動的に解決するため、Content
Patcher Pack側で正確な座標を把握する必要がありません。

T's Coreには、複数の標準Warp Providerと組み込みWarp Providerがあります。

Content Patcher Packから、次のData
Assetを通して追加のProviderを登録することもできます。

``` text
TsCore/WarpProviders
```

このData Assetは、**外部ModのカスタムWarp Provider** 用です。

------------------------------------------------------------------------

### Providerの解決

T's CoreのWarp Actionへ行き先を渡すと、T's Coreはまずその値をWarp
Providerとして解決しようとします。

一致するProviderが存在しない場合、その値はLocation名として扱われます。

例:

``` text
TsCoreWarp FarmHouseFront
TsCoreWarp BusStop
```

`FarmHouseFront` はWarp Providerとして解決されます。一方、`BusStop`
というIDのProviderが存在しない場合、`BusStop`
はLocation名として扱われます。

Location名を直接指定した場合、T's CoreはそのMapに設定されたStardew
ValleyのデフォルトWarp位置を使用します。

------------------------------------------------------------------------

<a id="content-pack-setup"></a>

## Content Packのセットアップ

カスタムWarp Providerは、通常の **Content Patcher Content Pack**
から登録します。

Content Packでは、Content PatcherとT's
Coreの両方を依存Modに指定してください。

### manifest.json

``` json
{
  "Name": "[CP] My Warp Pack",
  "Author": "YourName",
  "Version": "1.0.0",
  "UniqueID": "YourName.MyWarpPack",
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

### フォルダ構成

シンプルなContent Packは、たとえば次のような構成になります。

``` text
[CP] My Warp Pack
├── manifest.json
└── content.json
```

Warp Providerの定義に専用フォルダは必要ありません。

`content.json` 内でContent Patcherの `EditData`
Actionを使用して追加します。

------------------------------------------------------------------------

<a id="registering-warp-providers"></a>

## Warp Providerの登録

`EditData` で次を指定します。

``` text
Target: TsCore/WarpProviders
```

各EntryのKeyがWarp Provider IDになります。

例:

``` json
{
  "Action": "EditData",
  "Target": "TsCore/WarpProviders",
  "Entries": {
    "MyWarpProvider": {
      "Type": "Warp",
      "Source": "Mine",
      "Target": "Mountain"
    }
  }
}
```

この例のProvider IDは次のとおりです。

``` text
MyWarpProvider
```

Providerデータ自体には `Id` Propertyを**含めません**。

IDには `Entries` のKeyが使用されます。

``` json
"Entries": {
  "MyWarpProvider": {
```

1つの `EditData` Patchで複数のProviderを登録できます。

例:

``` json
{
  "Action": "EditData",
  "Target": "TsCore/WarpProviders",
  "Entries": {
    "MyWarpProvider": {
      "Type": "Warp",
      "Source": "Mine",
      "Target": "Mountain"
    },
    "MyBuildingWarp": {
      "Type": "Building",
      "BuildingType": "YourName.MyMod_MyBuilding",
      "OffsetX": 1,
      "OffsetY": 3
    }
  }
}
```

> **重要:** Provider IDは全体で一意になるようにしてください。T's
> Coreが予約しているIDは使用しないでください。

カスタムProviderでは、可能であれば自分のModのUniqueIDを基にしたIDを使用することを推奨します。

例:

``` text
YourName.MyMod_MyWarp
```

次の標準Provider IDはT's Coreによって予約されています。

``` text
FarmHouseFront
GreenhouseFront
FarmCaveFront
IslandFarmHouseFront
```

これらのProviderはT's Core内部で実装されており、`TsCore/WarpProviders`
から置き換えることはできません。

Content Patcher
PackがこれらのIDを登録しようとした場合、外部定義は無視され、代わりにT's
Core自身の標準Providerが使用されます。

また、T's Coreは次のような警告をログへ出力します。

``` text
Warp Provider ID 'FarmHouseFront' is reserved by T's Core and cannot be overridden.
```

------------------------------------------------------------------------

<a id="warp-provider-types"></a>

## Warp Providerの種類

現在、T's Coreでは3種類のカスタムWarp Providerに対応しています。

  Type         用途
  ------------ --------------------------------------------------------
  `Warp`       既存Map Warpの行き先を解決します。
  `MapEntry`   既存Map Warpの元位置を解決します。
  `Building`   Farmに設置されたBuildingを基準とした位置を解決します。

------------------------------------------------------------------------

### Warp Provider (Type: Warp)

**Warp** Providerは、Source
Locationに存在するWarpを読み取り、その行き先を解決します。

``` json
{
  "Action": "EditData",
  "Target": "TsCore/WarpProviders",
  "Entries": {
    "MyFarmHouseFront": {
      "Type": "Warp",
      "Source": "FarmHouse",
      "Target": "Farm",
      "Fallback": "FarmHouseFront"
    }
  }
}
```

  Property     必須   説明
  ------------ ------ ----------------------------------------------------
  Entry key    ✅     T's CoreのWarp Actionで使用する一意のProvider ID。
  `Type`       ✅     `"Warp"` を指定します。
  `Source`     ✅     調べるWarpが存在するLocation。
  `Target`     ✅     解決対象となるWarpの行き先Location。
  `Fallback`   任意   行き先を解決できなかった場合に使用するProvider。

T's Coreは `Source` から `Target` へ移動するWarpを検索します。

一致するWarpが見つかった場合、そのWarpの **行き先Locationと行き先座標**
を返します。

例:

``` text
FarmHouse
    Warp at (4, 11)
        → Farm (64, 15)
```

`Warp` Providerを次のように設定すると:

``` json
{
  "Type": "Warp",
  "Source": "FarmHouse",
  "Target": "Farm"
}
```

次の位置を解決します:

``` text
Farm (64, 15)
```

行き先は実行時に解決されるため、他Modが対応するMap
Warpを変更した場合にも追従できます。

Warpが見つからず `Fallback` が指定されている場合、T's
Coreは代わりにFallback Providerの解決を試みます。

`FarmHouseFront` などT's Coreの標準ProviderもFallback
Providerとして使用できます。

------------------------------------------------------------------------

### Warp Provider (Type: MapEntry)

**MapEntry**
Providerは、既存Warpが**そのWarpを含むMap内のどこに配置されているか**を解決します。

既存Warpの行き先を返す `Warp` Providerとは異なり、`MapEntry`
ProviderはそのWarp自体が置かれている位置を返します。

``` json
{
  "Action": "EditData",
  "Target": "TsCore/WarpProviders",
  "Entries": {
    "MyFarmHouseEntry": {
      "Type": "MapEntry",
      "Map": "FarmHouse",
      "Target": "Farm",
      "OffsetX": 0,
      "OffsetY": -1,
      "Fallback": "FarmHouseFront"
    }
  }
}
```

  ----------------------------------------------------------------------------------------------
  Property              必須                  説明
  --------------------- --------------------- --------------------------------------------------
  Entry key             ✅                    T's CoreのWarp Actionで使用する一意のProvider ID。

  `Type`                ✅                    `"MapEntry"` を指定します。

  `Map`                 ✅                    調べるWarpが存在するLocation。

  `Target`              ✅                    Warpを特定するために使用する行き先Location。

  `OffsetX`             任意                  WarpのSource Tileからの横方向Offset。デフォルトは
                                              `0`。

  `OffsetY`             任意                  WarpのSource Tileからの縦方向Offset。デフォルトは
                                              `0`。

  `Fallback`            任意                  行き先を解決できなかった場合に使用するProvider。
  ----------------------------------------------------------------------------------------------

T's Coreは `Map` から `Target` へ移動するWarpを検索します。

一致するWarpが見つかった場合、T's Coreは **`Map`
内でのそのWarpのSource位置** を返します。必要に応じて `OffsetX` と
`OffsetY` で位置を調整できます。

例:

``` text
FarmHouse
    Warp at (4, 11)
        → Farm (64, 15)
```

`MapEntry` Providerを次のように設定すると:

``` json
{
  "Type": "MapEntry",
  "Map": "FarmHouse",
  "Target": "Farm"
}
```

次の位置を解決します:

``` text
FarmHouse (4, 11)
```

比較すると、同じMapの関係を使用する `Warp`
Providerでは次の位置が解決されます。

``` text
Farm (64, 15)
```

そのため `MapEntry`
は、Tile座標を直接指定せずにMapの入口や出口付近を行き先にしたい場合に便利です。

Offsetを使用すると、検出したWarpを基準に行き先を移動できます。

例:

``` json
{
  "Type": "MapEntry",
  "Map": "FarmHouse",
  "Target": "Farm",
  "OffsetX": 0,
  "OffsetY": -1
}
```

一致するWarpが `(4, 11)` にある場合、このProviderは次の位置を返します。

``` text
FarmHouse (4, 10)
```

Mapまたは一致するWarpが見つからず `Fallback` が指定されている場合、T's
Coreは代わりにFallback Providerの解決を試みます。

------------------------------------------------------------------------

### Warp Provider (Type: Building)

**Building**
Providerは、プレイヤーのFarmに設置されているBuildingの位置から行き先を計算します。

次の例は、カスタムBuilding用のBuilding Providerです。

``` json
{
  "Action": "EditData",
  "Target": "TsCore/WarpProviders",
  "Entries": {
    "MonsterHouseFront": {
      "Type": "Building",
      "BuildingType": "Tikamin557.SF.MonsterHouse.Buildings_MonsterHouse",
      "OffsetX": 0,
      "OffsetY": 1,
      "Fallback": "FarmHouseFront"
    }
  }
}
```

  ------------------------------------------------------------------------------------------------
  Property              必須                  説明
  --------------------- --------------------- ----------------------------------------------------
  Entry key             ✅                    T's CoreのWarp Actionで使用する一意のProvider ID。

  `Type`                ✅                    `"Building"` を指定します。

  `BuildingType`        ✅                    プレイヤーのFarmから検索するBuildingの内部Type。

  `OffsetX`             任意                  Building左上Tileからの横方向Offset。デフォルトは
                                              `0`。

  `OffsetY`             任意                  Building左上Tileからの縦方向Offset。デフォルトは
                                              `0`。

  `Fallback`            任意                  Buildingが見つからなかった場合に使用するProvider。
  ------------------------------------------------------------------------------------------------

T's Coreは指定された `BuildingType`
をFarmから検索し、Buildingの左上Tileに `OffsetX` と `OffsetY`
を加えて行き先を計算します。

たとえば、**2 × 1** の範囲を使用するBuildingでは次のようになります。

``` text
■■
□
↑ Warp destination
```

次の設定では:

``` json
"OffsetX": 0,
"OffsetY": 1
```

行き先はBuilding左上Tileの真下1Tileになります。

Buildingが見つからない場合は、`Fallback`
に指定されたProviderが代わりに使用されます。

> **ヒント:** `tscore_debug_farmbuildings`
> を使用すると、現在Farmに存在するBuildingのType、位置、サイズを確認できます。

------------------------------------------------------------------------

<a id="standard-warp-providers"></a>

## 標準Warp Provider

T's Coreでは4つの標準Warp Providerを提供しています。

  ------------------------------------------------------------------------------------------------------
  Provider                 Source              Target         説明
  ------------------------ ------------------- -------------- ------------------------------------------
  `FarmHouseFront`         `FarmHouse`         `Farm`         Farmhouse入口の外側のTileを解決します。

  `GreenhouseFront`        `Greenhouse`        `Farm`         Greenhouse入口の外側のTileを解決します。

  `FarmCaveFront`          `FarmCave`          `Farm`         Farm Cave入口の外側のTileを解決します。

  `IslandFarmHouseFront`   `IslandFarmHouse`   `IslandWest`   Island
                                                              Farmhouse入口の外側のTileを解決します。
  ------------------------------------------------------------------------------------------------------

これらのProviderはT's Core内部で実装されています。

これらは **`TsCore/WarpProviders` のEntryではなく**、Content
Patcherから置き換えたり変更したりすることはできません。

T's CoreのどのWarp Actionからでも直接使用できます。

例:

``` text
TsCoreWarp FarmHouseFront
TsCoreMagicWarp GreenhouseFront Down
```

カスタムProviderのFallback Providerとして使用することもできます。

例:

``` json
{
  "Action": "EditData",
  "Target": "TsCore/WarpProviders",
  "Entries": {
    "MyCustomProvider": {
      "Type": "Building",
      "BuildingType": "YourName.MyMod_MyBuilding",
      "OffsetX": 0,
      "OffsetY": 1,
      "Fallback": "FarmHouseFront"
    }
  }
}
```

これらの行き先は現在のMap
Warpから解決されるため、対応する入口を移動・変更する互換性のあるカスタムMapやModにも追従できます。

> **注意:** Providerが動作するには、設定されたSource LocationとTarget
> Locationの間に有効なWarpが必要です。他ModによってそのWarp自体が完全に削除された場合、Providerが行き先を解決できないことがあります。

次のIDはT's Coreによって予約されています。

``` text
FarmHouseFront
GreenhouseFront
FarmCaveFront
IslandFarmHouseFront
```

これらのIDを使用した外部Entryは無視されます。

今後のT's Coreで、標準Providerが追加される場合があります。

------------------------------------------------------------------------

<a id="special-built-in-warp-providers"></a>

## 特殊な組み込みWarp Provider

T's Coreには、T's Core内部で直接実装された3つの特殊なWarp
Providerもあります。

これらのProviderは特殊な実行時処理を使用しており、`TsCore/WarpProviders`
のEntryではありません。

  ---------------------------------------------------------------------------------------------------
  Provider                       説明
  ------------------------------ --------------------------------------------------------------------
  `PlayerHome`                   現在のプレイヤー自身のFarmHouseまたはCabinの入口を解決します。

  `PreviousHome`                 プレイヤーが最後に退出したFarmHouseまたはCabinの入口を解決します。

  `CurrentHome`                  現在Actionを使用しているFarmHouseまたはCabinの入口を解決します。
  ---------------------------------------------------------------------------------------------------

これらのProviderはT's CoreのWarp Actionから直接使用できます。

例:

``` text
TsCoreWarp PlayerHome
TsCoreMagicWarp PreviousHome
TsCoreMagicWarp_Simple CurrentHome
```

------------------------------------------------------------------------

### PlayerHome

`PlayerHome` は現在のプレイヤー自身のHomeを解決します。

メインのFarmHouseとプレイヤーCabinの両方に対応しています。

例:

``` text
TsCoreWarp PlayerHome
```

プレイヤー自身のHomeの入口位置へワープします。

------------------------------------------------------------------------

### PreviousHome

`PreviousHome`
は、ローカルプレイヤーが最後に退出したFarmHouseまたはCabinを記憶します。

例:

``` text
TsCoreWarp PreviousHome
```

そのHomeの入口へプレイヤーを戻すために使用できます。

`PreviousHome` は、T's
Coreがプレイヤーの退出したHomeを記録した後に利用できるようになります。

まだ以前のHomeが記録されていない場合、このProviderは行き先を解決できません。

------------------------------------------------------------------------

### CurrentHome

`CurrentHome` は、現在Warp
Actionを使用しているFarmHouseまたはCabinの入口位置を解決します。

例:

``` text
TsCoreWarp CurrentHome
```

このProviderは、FarmHouseまたはCabin内部で使用するAction向けです。

別の種類のLocationから使用した場合、行き先を解決できません。

------------------------------------------------------------------------

### 予約済みの組み込みProvider ID

次のIDは、T's Coreの特殊な組み込みProvider用に予約されています。

``` text
PlayerHome
PreviousHome
CurrentHome
```

これらのIDをカスタムWarp Providerに使用しないでください。

これらのIDを使用したカスタムEntryは、T's
Coreの組み込みProviderの置き換えとしては扱われません。

------------------------------------------------------------------------

## 使用例

次の例では、**Pelican Town** の噴水前にTile Actionを追加します。

噴水をクリックすると `FarmHouseFront`
Providerを使用してFarmhouse入口へMagic
Warpし、プレイヤーを**下向き**にします。

``` json
{
    "Action": "EditMap",
    "Target": "Maps/Town",
    "MapTiles": [
        {
            "Position": { "X": 26, "Y": 28 },
            "Layer": "Buildings",
            "SetProperties": {
                "Action": "TsCoreMagicWarp FarmHouseFront Down"
            }
        }
    ]
}
```

同じワープをMagic Warpエフェクトなしで実行する場合は、`TsCoreMagicWarp`
を `TsCoreWarp` に置き換えます。

アニメーション中もプレイヤーを表示したまま、簡略化したMagic
Warpエフェクトを使用する場合は、代わりに `TsCoreMagicWarp_Simple`
を使用します。

例:

``` json
{
    "Action": "EditMap",
    "Target": "Maps/Town",
    "MapTiles": [
        {
            "Position": { "X": 26, "Y": 28 },
            "Layer": "Buildings",
            "SetProperties": {
                "Action": "TsCoreMagicWarp_Simple FarmHouseFront Auto wand 2 150 100 0"
            }
        }
    ]
}
```

この例では:

-   `FarmHouseFront` Providerを使用します。
-   プレイヤーの現在の向きを維持します。
-   `wand` Audio Cueを使用します。
-   Cueを2回再生します。
-   Audio Cueの再生間隔を150msにします。
-   ワープ前に画面を100msの間、完全な黒の状態にします。
-   Audio Cueをすぐに再生開始します。

------------------------------------------------------------------------

<a id="debugging"></a>

## デバッグ

T's Coreでは、Warp
ProviderやFarmのBuildingを確認するためのデバッグコマンドを提供しています。

`TsCore/WarpProviders` から登録されたカスタムWarp ProviderはContent
Patcher経由で管理されるため、変更内容はT's CoreのContent
Patcherリロードコマンドで再読み込みできます。

------------------------------------------------------------------------

### Warp Provider変更の再読み込み

Content Patcher Packの開発中は、ゲームを再起動せずにWarp Provider
Patchの変更を再読み込みできます。

使い分けは次のとおりです。

``` text
tscore_cp_reload <ContentPackId>
```

例:

``` text
tscore_cp_reload YourName.MyWarpPack
```

Content Patcher Packを再読み込みすると、そのPackの
`TsCore/WarpProviders` Entryへの変更がすぐに反映されます。

以下の変更が対象です。

-   Warp Providerの追加
-   Warp Providerの削除
-   Provider Typeの変更
-   SourceまたはTarget Locationの変更
-   Offsetの変更
-   Fallback Providerの変更

`tscore_cp_reload`、ConfigSchemaの再読み込み、Config
Token、GMCM連携、DynamicTokensの詳細については、[Content Patcher
Integration](ModderGuide_ContentPatcherIntegration.md)
ガイドを参照してください。

------------------------------------------------------------------------

### Warp Providerの確認

現在利用可能なすべてのWarp
Providerを表示するには、次のコマンドを使用します。

``` text
tscore_debug_warp
```

出力ではProviderが3つのグループに分けて表示されます。

``` text
Built-in Providers
TsCore Standard Providers
External Data Asset Providers
```

各グループの意味は次のとおりです。

-   **Built-in Providers** --- T's
    Core内部の特殊な実行時処理で直接実装されているProvider。
-   **TsCore Standard Providers** --- T's
    Core内部で提供される標準Provider。
-   **External Data Asset Providers** --- `TsCore/WarpProviders`
    から登録されたカスタムProvider。

出力には次のような情報が含まれます。

-   Provider ID
-   Provider Type
-   Source / Target Location
-   `MapEntry` で使用するMap
-   Building Type
-   座標Offset
-   Fallback Provider

<details>
<summary>
出力例
</summary>
``` text
tscore_debug_warp
[T's Core] ===== Warp Providers =====
[T's Core]
[T's Core] ----- Built-in Providers -----
[T's Core]
[T's Core] PlayerHome
[T's Core]     Type                : Built-in
[T's Core]     Destination         : Player's own home
[T's Core]
[T's Core] PreviousHome
[T's Core]     Type                : Built-in
[T's Core]     Destination         : Previously exited home
[T's Core]
[T's Core] CurrentHome
[T's Core]     Type                : Built-in
[T's Core]     Destination         : Current FarmHouse/Cabin
[T's Core]
[T's Core] ----- TsCore Standard Providers -----
[T's Core]
[T's Core] FarmCaveFront
[T's Core]     Type                : Warp
[T's Core]     Source              : FarmCave
[T's Core]     Target              : Farm
[T's Core]     Fallback            : (none)
[T's Core]
[T's Core] FarmHouseFront
[T's Core]     Type                : Warp
[T's Core]     Source              : FarmHouse
[T's Core]     Target              : Farm
[T's Core]     Fallback            : (none)
[T's Core]
[T's Core] GreenhouseFront
[T's Core]     Type                : Warp
[T's Core]     Source              : Greenhouse
[T's Core]     Target              : Farm
[T's Core]     Fallback            : (none)
[T's Core]
[T's Core] IslandFarmHouseFront
[T's Core]     Type                : Warp
[T's Core]     Source              : IslandFarmHouse
[T's Core]     Target              : IslandWest
[T's Core]     Fallback            : (none)
[T's Core]
[T's Core] ----- External Data Asset Providers -----
[T's Core]
[T's Core] MonsterHouseFront
[T's Core]     Type                : Building
[T's Core]     Building            : Tikamin557.SF.MonsterHouse.Buildings_MonsterHouse
[T's Core]     Offset              : (0, 1)
[T's Core]     Fallback            : FarmHouseFront
```

</details>
外部のContent Patcher PackがT's Coreの予約済み標準Provider
IDを使用しようとした場合、T's
Coreはその外部定義を無視して警告をログへ出力します。

例:

``` text
[T's Core] Warp Provider ID 'FarmHouseFront' is reserved by T's Core and cannot be overridden.
```

------------------------------------------------------------------------

### Farm Buildingの確認

`Building`
Providerを作成する際は、現在Farmに設置されているBuildingを確認するために次のコマンドを使用します。

``` text
tscore_debug_farmbuildings
```

各Buildingの内部Type、Tile位置、サイズ、Interior
Locationが表示されます。

出力に表示されたBuilding名を `BuildingType` の値として使用できます。

<details>
<summary>
出力例
</summary>
``` text
tscore_debug_farmbuildings
[T's Core] ===== Farm Buildings =====
[T's Core] Registered Buildings: 2
[T's Core]
[T's Core] Farmhouse
[T's Core]     Tile                : (59, 12)
[T's Core]     Size                : 9 x 5
[T's Core]     Indoors             : FarmHouse
[T's Core]
[T's Core] Tikamin557.SF.MonsterHouse.Buildings_MonsterHouse
[T's Core]     Tile                : (56, 12)
[T's Core]     Size                : 2 x 1
[T's Core]     Indoors             : (none)
```

</details>
Monster Houseの例では、対応する値は次のとおりです。

``` json
"BuildingType": "Tikamin557.SF.MonsterHouse.Buildings_MonsterHouse"
```

`Tile` の値はBuildingの左上Tileを表しており、`OffsetX` と `OffsetY`
から計算される行き先の確認に使用できます。

> **注意:** `tscore_debug_farmbuildings`
> を使用する前に、セーブデータを読み込んでおく必要があります。

------------------------------------------------------------------------

<a id="notes"></a>

## 注意事項

Warp ServicesはContent
Patcherから使用することを想定しており、C#コードは必要ありません。

可能な場合はMap名や座標を直接指定する代わりにWarp
Providerを使用すると、カスタムMapや他Modとの互換性を高められます。

使い分けは次のとおりです。

-   **既存Warpの行き先**が必要な場合は `Warp`。
-   **既存Warpが置かれている位置**が必要な場合は `MapEntry`。
-   **Farmに設置されたBuilding**を基準とする位置が必要な場合は
    `Building`。

Warpの行き先は実行時に解決されるため、対応するContent Patcher
Packは座標を直接指定せずにMap変更へ追従できます。

カスタムWarp Providerは次のData Assetから追加してください。

``` text
TsCore/WarpProviders
```

Content Patcherの `EditData` Actionを使用して登録します。

`TsCore/WarpProviders` は外部のカスタムProvider用です。T's
Coreの標準Providerは内部実装されており、このData
AssetのEntryとしては保存されていません。

Provider IDはDictionaryのEntry Keyであり、Provider
Model内には保存されません。

次の標準Provider ID:

``` text
FarmHouseFront
GreenhouseFront
FarmCaveFront
IslandFarmHouseFront
```

はT's Coreによって予約されており、`TsCore/WarpProviders`
から上書きすることはできません。

次の特殊な組み込みProvider:

``` text
PlayerHome
PreviousHome
CurrentHome
```

もT's Core内部で直接実装されており、`TsCore/WarpProviders`
のEntryではありません。

今後のT's Coreで、Providerや機能が追加される場合があります。

------------------------------------------------------------------------

## Modder Guide

-   ← [Location Services](ModderGuide_LocationServices.md)
-   ↑ [Guide Index](#top)
-   → [Map Properties](ModderGuide_MapProperties.md)

← [READMEに戻る](../../README.md)

← [Modder Guideに戻る](ModderGuide.md)
