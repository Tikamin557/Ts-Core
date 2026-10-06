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
-   ✅ **BigCraftable Extension** *(現在のページ)*
-   📄 [Dialogue System](ModderGuide_DialogueSystem.md)
-   📄 [Migration System](ModderGuide_MigrationSystem.md)
-   📄 [Notification System](ModderGuide_NotificationSystem.md)
-   📄 [Content Patcher
    Integration](ModderGuide_ContentPatcherIntegration.md)
-   📄 [Position Picker](ModderGuide_PositionPicker.md)
-   📄 [Post Renovation Patch](ModderGuide_PostRenovationPatch.md)
-   📄 [Polyamory Sweet Rooms
    Integration](ModderGuide_PolyamorySweetRoomsIntegration.md)
-   📄 [Other Features](ModderGuide_OtherFeatures.md)

← [READMEに戻る](../../README.md)

← [Modder Guideに戻る](ModderGuide.md)

------------------------------------------------------------------------

# BigCraftable Extension

BigCraftable Extensionを使用すると、Content Patcher PackからBig
Craftableへ追加機能を設定できます。

Stardew Valleyの `Data/BigCraftables` システムと、T's
Coreの次のカスタムData Assetを組み合わせて使用します:

``` text
TsCore/BigCraftableExtension
```

BigCraftable Extensionでは、次の機能を追加できます:

-   カスタムCollision Size
-   プレイヤーが通過可能なCollision Tile
-   カスタムTile Property
-   Game State Queryを使用したPlacement Condition
-   カスタムTexture Size
-   カスタムNormal TextureとAnimation
-   追加のDraw Layer
-   Tile Action
-   Game State Queryを使用したInteraction Condition
-   手に持っている必要があるRequired Item
-   Machine Effect
-   一時的なAction Light
-   Action Wobble Effect
-   Action TextureとAnimation
-   Idle Effect
-   Idle Wobble Effect
-   Normal Light

BigCraftable Extensionは通常のBig
Craftableにも使用でき、一般的なBigCraftable
Extension機能を利用するために、そのBig Craftableへ `Data/Machines`
Entryを用意する必要はありません。

C#コードは必要ありません。

------------------------------------------------------------------------

## 目次

-   [Overview](#overview)
-   [Basic Setup](#basic-setup)
-   [BigCraftable Extension Data](#bigcraftable-extension-data)
-   [Collision Size](#collision-size)
-   [Player-Passable Tiles](#player-passable-tiles)
-   [Tile Properties](#tile-properties)
-   [Placement Condition](#placement-condition)
-   [Texture Size](#texture-size)
-   [Normal Texture and Animation](#normal-texture-and-animation)
-   [Draw Layers](#draw-layers)
-   [Tile Action](#tile-action)
-   [Conditions](#conditions)
-   [Required Items](#required-items)
-   [Action Effects](#action-effects)
-   [Action Light](#action-light)
-   [Action Wobble](#action-wobble)
-   [Action Texture and Animation](#action-texture-and-animation)
-   [Idle Effects](#idle-effects)
-   [Idle Wobble](#idle-wobble)
-   [Normal Light](#normal-light)
-   [Texture Priority](#texture-priority)
-   [Complete Example](#complete-example)
-   [Notes](#notes)

------------------------------------------------------------------------

<a id="overview"></a>

## 概要

Big Craftableは、`Data/BigCraftables` の次のCustom Fieldを通してT's
CoreのBigCraftable Extensionと関連付けます:

``` text
TsCore/BigCraftableExtension
```

例:

``` json
"CustomFields": {
    "TsCore/BigCraftableExtension": "MyMod/MyExtension"
}
```

この値は、次のData Asset内のEntryを指定します:

``` text
TsCore/BigCraftableExtension
```

基本的な関係は次のとおりです:

``` text
Data/BigCraftables
        ↓
CustomFields
        ↓
TsCore/BigCraftableExtension
        ↓
BigCraftable Extension Data
```

Interaction機能は、基本的に次の順序で処理されます:

``` text
Player interacts with the Big Craftable
        ↓
Condition
        ↓
Required Item
        ↓
Tile Action
        ↓
Action Effects / Light / Wobble
        ↓
Action Texture / Animation
        ↓
Required Item consumption
```

BigCraftable
Extensionには、次のようなInteractionを必要としない機能もあります:

``` text
Collision Size
Player-Passable Tiles
Tile Properties
Texture Size
Normal Texture / Animation
Draw Layers
Idle Effects
Idle Wobble
Normal Light
```

------------------------------------------------------------------------

<a id="basic-setup"></a>

## 基本設定

BigCraftable Extensionを設定するには、大きく2つの手順が必要です:

1.  `Data/BigCraftables` からBig CraftableをExtension IDへ関連付けます。
2.  対応するEntryを `TsCore/BigCraftableExtension` へ追加します。

### Step 1: Big Craftableを関連付ける

Big Craftableの `Data/BigCraftables` Entryへ、次のCustom
Fieldを追加します:

``` json
{
    "Action": "EditData",
    "Target": "Data/BigCraftables",
    "TargetField": [
        "MyMod_MyBigCraftable",
        "CustomFields"
    ],
    "Entries": {
        "TsCore/BigCraftableExtension": "MyMod/MyExtension"
    }
}
```

この例では:

``` text
MyMod/MyExtension
```

がBigCraftable Extension IDです。

Big Craftableを追加するときに、Custom Fieldを直接含めることもできます:

``` json
{
    "Action": "EditData",
    "Target": "Data/BigCraftables",
    "Entries": {
        "MyMod_MyBigCraftable": {
            "Name": "MyBigCraftable",
            "DisplayName": "My Big Craftable",
            "Description": "An example Big Craftable.",
            "Price": 500,
            "Fragility": 0,
            "CanBePlacedOutdoors": true,
            "CanBePlacedIndoors": true,
            "Texture": "MyMod/MyBigCraftableTexture",
            "SpriteIndex": 0,
            "CustomFields": {
                "TsCore/BigCraftableExtension": "MyMod/MyExtension"
            }
        }
    }
}
```

------------------------------------------------------------------------

### Step 2: Extensionを追加する

次のData AssetへEntryを追加します:

``` text
TsCore/BigCraftableExtension
```

例:

``` json
{
    "Action": "EditData",
    "Target": "TsCore/BigCraftableExtension",
    "Entries": {
        "MyMod/MyExtension": {
            "TileAction": "Message \"Hello!\""
        }
    }
}
```

関連付けたBig Craftableを操作すると、指定したTile
Actionが実行されるようになります。

------------------------------------------------------------------------

## BigCraftable Extension Data

次のPropertyに対応しています。

  ----------------------------------------------------------------------------------------------------------
  Property                    デフォルト          説明
  --------------------------- ------------------- ----------------------------------------------------------
  `CollisionSize`             `"1, 1"`            Collisionの幅と高さをTile単位で指定します。

  `PlayerPassableTiles`       ---                 プレイヤーが通過できる、相対位置で指定したCollision Tile。

  `TileProperties`            ---                 Big Craftable周辺の相対Tileへ適用するカスタムTile
                                                  Property。

  `PlacementCondition`        ---                 Big Craftableを設置するために満たす必要があるGame State
                                                  Query。

  `Texture`                   ---                 任意のNormal Texture上書き。

  `TexturePosition`           ---                 Normal Textureの任意の基準Pixel位置。

  `TextureSize`               `"16, 32"`          Texture 1Frameの幅と高さをPixel単位で指定します。

  `Frames`                    ---                 Normal Animationで使用するFrame番号。

  `FrameDurationMs`           ---                 Normal AnimationのFrame表示時間。

  `DrawLayers`                ---                 Big Craftableと一緒に描画する追加のVisual Layer。

  `TileAction`                ---                 Interaction成功時に実行するTile Action。

  `Condition`                 ---                 Interactionを続行するために満たす必要があるGame State
                                                  Query。

  `InvalidConditionMessage`   ---                 `Condition` を満たしていない場合に表示するMessage。

  `RequiredItem`              ---                 プレイヤーが手に持っている必要があるItem。

  `RequiredItemCount`         `1`                 手に持つ必要があるItem数。

  `ConsumeRequiredItem`       `true`              Interaction成功後にRequired Itemを消費するか。

  `InvalidItemMessage`        ---                 Required Itemを手に持っていない場合に表示するMessage。

  `InvalidCountMessage`       ---                 手に持っているRequired
                                                  Itemの数が不足している場合に表示するMessage。

  `ActionEffects`             ---                 Tile Action成功後に再生するMachine Effect。

  `ActionEffectChance`        `1`                 `ActionEffects` を再生する確率。

  `ActionLight`               `false`             Tile Action成功後に一時的なLightを表示するか。

  `ActionLightOffset`         `"0, 0"`            Action LightのOffsetをTile単位で指定します。

  `ActionLightRadius`         `2`                 Action Lightの半径。

  `ActionLightColor`          `"White"`           Action Lightの色。

  `ActionLightDurationMs`     `1000`              Action Lightの表示時間（ミリ秒）。

  `ActionWobble`              `false`             Tile Action成功後にBig CraftableをWobbleさせるか。

  `ActionWobbleDurationMs`    `1000`              Action Wobbleの継続時間（ミリ秒）。

  `ActionTexture`             ---                 Action状態で表示するTexture Asset。

  `ActionTexturePosition`     `"0, 0"`            Action Textureの基準Pixel位置。

  `ActionFrames`              ---                 Action Animationで使用するFrame番号。

  `ActionFrameDurationMs`     ---                 Action AnimationのFrame表示時間。

  `ActionDurationMs`          `1000`              静止Action Textureの表示時間。

  `IdleEffects`               ---                 Big
                                                  CraftableがIdle状態のときに再生される可能性があるMachine
                                                  Effect。

  `IdleEffectChance`          `1`                 `IdleEffects` を再生する確率。

  `IdleWobble`                `false`             Big CraftableがIdle状態のときにWobble Effectを使用するか。

  `Light`                     `false`             Big Craftableに通常のT's Core Lightを設定するか。

  `LightOffset`               `"0, 0"`            Normal LightのOffsetをTile単位で指定します。

  `LightRadius`               `2`                 Normal Lightの半径。

  `LightColor`                `"White"`           Normal Lightの色。
  ----------------------------------------------------------------------------------------------------------

各Property Groupについて、以下で詳しく説明します。

------------------------------------------------------------------------

## Collision Size

`CollisionSize` はBig CraftableのCollision範囲を変更します。

形式:

``` text
"Width, Height"
```

値はTile単位です。

例:

``` json
{
    "CollisionSize": "1, 2"
}
```

これにより、幅1Tile・高さ2TileのCollision範囲が作成されます。

デフォルト:

``` text
"1, 1"
```

### Collisionの方向

実際に設置されたBig Craftable
Objectの位置が、Collision範囲の左下のAnchorになります。

Widthは右方向へ広がります。

Heightは上方向へ広がります。

例:

``` json
"CollisionSize": "1, 2"
```

では次の範囲になります:

``` text
[X]
[A]
```

ここで:

``` text
A = actual Big Craftable placement tile
X = additional collision tile
```

3 × 3のCollision範囲:

``` json
"CollisionSize": "3, 3"
```

では次の範囲になります:

``` text
[X][X][X]
[X][X][X]
[A][X][X]
```

実際のStardew Valley Objectが存在するのはAnchor Tileだけです。

追加された範囲のTileはT's Coreによって仮想的に処理されます。

これにより、追加Tileへ重複したObjectを作成せずに、Big
Craftableをより広い範囲へ配置できます。

拡張された範囲は、設置、Collision、Interaction、Object削除処理などのT's
Core機能で使用されます。

------------------------------------------------------------------------

## Player-Passable Tiles

`PlayerPassableTiles` を使用すると、Big
Craftableの拡張Collision範囲内で指定したTileをプレイヤーが通過できるようになります。

位置はBig CraftableのAnchor Tileを基準とした相対Tile
Offsetで指定します。

例:

``` json
{
    "CollisionSize": "1, 2",
    "PlayerPassableTiles": [
        "0, -1",
        "0, 0"
    ]
}
```

Anchor Tile:

``` text
"0, 0"
```

上方向へ広がるCollision範囲では、Anchorの真上のTileは:

``` text
"0, -1"
```

上の例では、プレイヤーは1 ×
2のCollision範囲にある両方のTileを通過できます。

`PlayerPassableTiles`
が変更するのはプレイヤーに対するCollisionだけです。他のCharacterには通常のBigCraftable
Extension Collisionが適用されます。

見た目上は範囲を占有していても、プレイヤーが内部へ入ったり中に立ったりできるBig
Craftableを作る場合に便利です。

------------------------------------------------------------------------

## Tile Properties

`TileProperties` は、Big
CraftableのAnchorを基準とした相対TileへカスタムTile
Propertyを追加できます。

各Entryでは次の項目を指定できます:

  Property   説明
  ---------- --------------------------------------------
  `Id`       Tile Property Entryの識別子。
  `Name`     Tile Property名。
  `Value`    Tile Propertyの値。
  `Layer`    `Back` など、Propertyを設定するMap Layer。
  `Tiles`    Propertyを適用する相対Tile Offset。

例:

``` json
{
    "TileProperties": [
        {
            "Id": "SleepAction",
            "Name": "TouchAction",
            "Value": "TsCore_Sleep",
            "Layer": "Back",
            "Tiles": [
                "0, -1",
                "0, 0"
            ]
        }
    ]
}
```

Tile OffsetではBig CraftableのAnchorを:

``` text
"0, 0"
```

負のY値はAnchorから上方向へ移動します。

例:

``` text
"0, -1"
```

はAnchorの真上のTileです。

`TileProperties` では、`TouchAction` などStardew
Valleyや互換Modが認識するTile Propertyを設定できます。

同じTile・Layerに対象のTile
PropertyがMap側ですでに設定されている場合、既存のMap
Propertyが優先されます。

------------------------------------------------------------------------

## Placement Condition

`PlacementCondition` はBig Craftableを設置できる場所を制限できます。

Stardew ValleyのGame State Queryシステムを使用します。

たとえばT's Coreが提供する `TsCore_LOCATION_CATEGORY` Game State
Queryを使用すると、Dungeon Locationへの設置を禁止できます:

``` json
{
    "PlacementCondition": "!TsCore_LOCATION_CATEGORY Target Dungeon"
}
```

この例ではDungeon以外のLocationには通常どおり設置できますが、`Dungeon`
に分類されるLocationには設置できません。

ConditionはプレイヤーがBig
Craftableを設置しようとしたときに判定されます。

Conditionを満たしていない場合、そのLocationにはBig
Craftableを設置できません。

Game State
QueryのContextには、設置先Locationと現在のプレイヤーが含まれます。

> **注意:** `PlacementCondition` が制御するのはBig
> Craftableを新しく設置できるかどうかだけです。設置後にConditionがfalseになっても、すでに設置済みのBig
> Craftableを削除したり無効化したりはしません。

`PlacementCondition` と `Condition` は別の機能です。

``` text
PlacementCondition
    ↓
Checked when placing the Big Craftable

Condition
    ↓
Checked when interacting with the placed Big Craftable
```

`PlacementCondition` を省略した場合、T's
Coreによる追加の設置条件は適用されません。

------------------------------------------------------------------------

## Texture Size

`TextureSize` はBig CraftableのTexture 1Frameのサイズを指定します。

形式:

``` text
"Width, Height"
```

値はSource TextureのPixel単位です。

例:

``` json
{
    "TextureSize": "17, 32"
}
```

は次の意味になります:

``` text
Width  = 17 pixels
Height = 32 pixels
```

デフォルト:

``` text
"16, 32"
```

これはStardew Valley標準のBig Craftable Frame Sizeです。

`TextureSize` はT's CoreのNormal AnimationとAction AnimationのFrame
Sizeとしても使用されます。

たとえば次の場合:

``` json
"TextureSize": "17, 32"
```

横方向に並んだFrameは17Pixelごとに開始します。

------------------------------------------------------------------------

## Normal Texture and Animation

BigCraftable Extensionでは、Big Craftableが使用するNormal
Textureを上書きし、任意でAnimationさせることができます。

関連するProperty:

``` text
Texture
TexturePosition
TextureSize
Frames
FrameDurationMs
```

### Normal Texture

`Texture` は任意のTexture Assetを指定します。

例:

``` json
{
    "Texture": "MyMod/MyBigCraftableTexture"
}
```

`Texture` を省略した場合、T's CoreはBig
Craftableに設定されている通常のTextureを使用します。

そのため、別のTextureを定義せずにBigCraftable
Extension機能を利用することもできます。

------------------------------------------------------------------------

### Texture Position

`TexturePosition` はFrame 0の左上Pixel位置を指定します。

形式:

``` text
"X, Y"
```

例:

``` json
{
    "TexturePosition": "0, 32"
}
```

`TexturePosition` を省略した場合、T's CoreはBig CraftableのStardew
Valley Dataによって決まる通常のSource位置を維持します。

そのため、シンプルなExtensionでは:

``` json
{
    "TextureSize": "17, 32"
}
```

のように、通常のTexture位置を改めて指定する必要はありません。

------------------------------------------------------------------------

### Normal Animation

`Frames` を使用すると、継続的にLoopするNormal Animationを作成できます。

例:

``` json
{
    "TextureSize": "17, 32",
    "Frames": [ 0, 1, 2, 3 ],
    "FrameDurationMs": 300
}
```

Frameは横方向に並びます。

Source X位置は、設定したFrame Widthを使用して計算されます:

``` text
X = BasePosition.X + (Frame × TextureWidth)
Y = BasePosition.Y
```

たとえば次の場合:

``` text
TextureSize = "17, 32"
BasePosition = "0, 0"
```

Frame位置は次のとおりです:

``` text
Frame 0 → (0, 0)
Frame 1 → (17, 0)
Frame 2 → (34, 0)
Frame 3 → (51, 0)
```

Normal Animationは継続的にLoopします。

------------------------------------------------------------------------

### Normal Frame Duration

`FrameDurationMs` には、すべてのFrameで共通の表示時間:

``` json
{
    "Frames": [ 0, 1, 2, 3 ],
    "FrameDurationMs": 300
}
```

またはFrameごとの表示時間を指定できます:

``` json
{
    "Frames": [ 0, 1, 2, 3 ],
    "FrameDurationMs": [ 300, 300, 600, 300 ]
}
```

値はミリ秒単位です。

配列を使用する場合、表示時間の値の数は `Frames`
のEntry数と完全に一致させる必要があります。

値が不正、または数が一致しない場合、Animationは無効になり、T's
Coreが警告をログへ出力します。

------------------------------------------------------------------------

## Draw Layers

`DrawLayers` はBig Craftableへ追加のVisual Layerを追加します。

Draw Layerはプレイヤーの背面または前面へ描画でき、Big
Craftableの現在のTextureまたは別のTexture Assetを使用できます。

例:

``` json
{
    "DrawLayers": [
        {
            "Id": "FrontLayer",
            "Texture": null,
            "SourceRect": {
                "X": 0,
                "Y": 32,
                "Width": 17,
                "Height": 32
            },
            "DrawPosition": "0, 0",
            "DrawLayer": "Front",
            "FrameCount": 1,
            "FramesPerRow": -1,
            "FrameDuration": 100
        }
    ]
}
```

各Draw Layerでは次のPropertyを使用できます:

  -------------------------------------------------------------------------------------------------
  Property               デフォルト          説明
  ---------------------- ------------------- ------------------------------------------------------
  `Id`                   ---                 Draw Layerの識別子。

  `Texture`              ---                 任意のTexture Asset。省略または `null` の場合、Big
                                             Craftableの現在のTextureを使用します。

  `SourceRect`           ---                 Draw Layerが使用するSource Rectangle。Texture
                                             Pixel単位です。

  `DrawPosition`         `"0, 0"`            Big Craftableの通常描画位置を基準としたPixel Offset。

  `DrawLayer`            `"Back"`            `"Back"` はプレイヤーの背面、`"Front"`
                                             はプレイヤーの前面へ描画します。

  `FrameCount`           `1`                 Animation Frame数。

  `FramesPerRow`         `-1`                Texture
                                             1行あたりのFrame数。負の値では横一列として扱います。

  `FrameDuration`        `90`                Animation Frameの表示時間（ミリ秒）。
  -------------------------------------------------------------------------------------------------

### Draw Position

`DrawPosition` はSource Pixel単位のOffsetを使用します。

例:

``` json
"DrawPosition": "4, -2"
```

では、通常の描画位置からDraw Layerを右へ4 Source Pixel、上へ2 Source
Pixel移動します。

<a id="front-and-back-layers"></a>

### Front LayerとBack Layer

使用:

``` json
"DrawLayer": "Back"
```

を指定するとLayerをプレイヤーの背面へ描画します。

使用:

``` json
"DrawLayer": "Front"
```

を指定するとLayerをプレイヤーの前面へ描画します。

`PlayerPassableTiles` と組み合わせることで、プレイヤーが見た目上Big
Craftableの内部へ入ったり背後に立ったりできる表現を作成できます。

### Draw Layer Animation

`FrameCount` を `1` より大きくするとDraw LayerをAnimationできます。

横一列のTextureの場合:

``` json
{
    "FrameCount": 4,
    "FramesPerRow": -1,
    "FrameDuration": 100
}
```

複数行に配置されたTextureでは、`FramesPerRow`
に1行あたりのFrame数を指定します。

`FrameDuration`
には、全Frame共通の表示時間またはFrameごとの個別の表示時間を指定できます。

Draw Layer Animationはゲーム内に設置されたBig
Craftableで使用されます。Menu、手持ちItem、Crafting、Recipe表示では、最初のFrameを静止Previewとして使用します。

------------------------------------------------------------------------

## Tile Action

`TileAction` は、プレイヤーがBig
CraftableとのInteractionに成功したときに実行するActionを指定します。

例:

``` json
{
    "TileAction": "Message \"Hello!\""
}
```

ActionはBig CraftableのAnchor Tile位置で実行されます。

T's CoreはStardew ValleyのTile
Actionシステムを使用するため、バニラのTile Actionと互換Modが登録したTile
Actionの両方を使用できます。

例:

``` json
{
    "TileAction": "Warp 10 10 FarmHouse"
}
```

T's CoreのカスタムTile Actionも使用できます。

例:

``` json
{
    "TileAction": "TsCoreMagicWarp FarmHouseFront"
}
```

`TileAction` が存在しない、または空の場合、Interactionは実行されません。

------------------------------------------------------------------------

<a id="conditions"></a>

## Condition

`Condition` はInteractionを利用できる条件を制限できます。

Stardew ValleyのGame State Queryシステムを使用します。

例:

``` json
{
    "TileAction": "Message \"The condition was met!\"",
    "Condition": "PLAYER_HAS_MAIL MyMail"
}
```

Conditionを満たしていない場合、Tile Actionは実行されません。

任意のMessageは次のように設定できます:

``` json
{
    "Condition": "PLAYER_HAS_MAIL MyMail",
    "InvalidConditionMessage": "You can't use this yet."
}
```

ConditionはRequired Itemの条件より先に判定されます。

InteractionのContextには、現在のLocation、プレイヤー、手に持っているItemが含まれます。

------------------------------------------------------------------------

<a id="required-items"></a>

## Required Item

BigCraftable
Extensionでは、プレイヤーが特定のItemを手に持っていることを必須条件にできます。

例:

``` json
{
    "TileAction": "Message \"The Big Craftable accepted the Wood.\"",
    "RequiredItem": "(O)388"
}
```

`(O)388` はWoodです。

Qualified Item IDとUnqualified Item IDの両方に対応しています。

Itemはプレイヤーが現在手に持っているObjectに対して判定されます。

Inventory内の別の場所にItemを所持しているだけでは条件を満たしません。

------------------------------------------------------------------------

### Required Item Count

`RequiredItemCount` を使用すると、2個以上のItemを必要条件にできます。

``` json
{
    "RequiredItem": "(O)388",
    "RequiredItemCount": 5
}
```

プレイヤーはWoodを5個以上手に持っている必要があります。

デフォルト:

``` text
1
```

------------------------------------------------------------------------

<a id="consuming-the-required-item"></a>

### Required Itemの消費

デフォルトでは、Required ItemはTile Action成功後に消費されます。

``` json
{
    "RequiredItem": "(O)388",
    "RequiredItemCount": 5,
    "ConsumeRequiredItem": true
}
```

Itemを消費せずに所持条件だけを確認する場合:

``` json
{
    "RequiredItem": "(O)388",
    "ConsumeRequiredItem": false
}
```

Required ItemはTile Actionが正常に実行された後にのみ消費されます。

------------------------------------------------------------------------

### Invalid Item Message

`InvalidItemMessage` は、プレイヤーがRequired
Itemを手に持っていない場合に表示されます。

``` json
{
    "RequiredItem": "(O)388",
    "InvalidItemMessage": "You need to hold Wood."
}
```

------------------------------------------------------------------------

### Invalid Count Message

`InvalidCountMessage`
は、正しいItemを手に持っているもののStack数が不足している場合に表示されます。

``` json
{
    "RequiredItem": "(O)388",
    "RequiredItemCount": 5,
    "InvalidCountMessage": "You need at least [ItemCount] Wood."
}
```

このMessageでは、次のT's Core Tokenを使用できます:

``` text
[ItemCount]
```

設定された `RequiredItemCount` の値に置き換えられます。

上の例では、表示されるMessageは次のようになります:

``` text
You need at least 5 Wood.
```

> **重要:** `[ItemCount]` はStardew ValleyのTokenizable
> String構文を使用します。`{{ItemCount}}` のようなContent Patcher
> Tokenとして記述しないでください。

------------------------------------------------------------------------

<a id="action-effects"></a>

## Action Effect

`ActionEffects` では、Tile Action成功後にStardew ValleyのMachine
Effectを再生できます。

例:

``` json
{
    "ActionEffects": [
        {
            "Id": "MyActionEffect",
            "Sounds": [
                {
                    "Id": "coin"
                }
            ]
        }
    ]
}
```

EffectにはStardew Valleyの `MachineEffects` Data Structureを使用します。

Machine Effectでは、Soundやその他のVisual Effectなど、Stardew
ValleyのMachine Effectシステムが対応するEffectを使用できます。

T's
Coreは設定されたEffectを順番に試し、最初に正常に再生されたEffectで処理を終了します。

------------------------------------------------------------------------

### Action Effect Chance

`ActionEffectChance` は `ActionEffects` が再生される確率を制御します。

値は `0`～`1` です。

例:

``` json
{
    "ActionEffectChance": 0.5
}
```

では、Action Effectが50%の確率で再生されます。

デフォルト:

``` text
1
```

------------------------------------------------------------------------

## Action Light

Tile Action成功後に一時的なLightを表示できます。

例:

``` json
{
    "ActionLight": true,
    "ActionLightOffset": "0, -0.5",
    "ActionLightRadius": 2.0,
    "ActionLightColor": "White",
    "ActionLightDurationMs": 1000
}
```

### Action Light Offset

`ActionLightOffset` は次の形式を使用します:

``` text
"X, Y"
```

値はBig CraftableのAnchor Tile中央を基準としたTile単位です。

小数値と負の値に対応しています。

例:

  値            結果
  ------------- ------------------------
  `"0, 0"`      Anchor Tileの中央。
  `"0, -0.5"`   上へ0.5Tile。
  `"0.5, 0"`    右へ0.5Tile。
  `"-1, 1"`     左へ1Tile、下へ1Tile。

Machine Lightの位置にはWorld座標を使用し、1Tileは64 × 64 World
Pixelです。

``` text
0.5 tile = 32 world pixels
```

------------------------------------------------------------------------

### Action Light Radius

`ActionLightRadius` はLightの半径を制御します。

デフォルト:

``` text
2
```

------------------------------------------------------------------------

### Action Light Color

`ActionLightColor` はLightの色を指定します。

例:

``` json
"ActionLightColor": "DarkCyan"
```

Stardew Valley / MonoGameが対応しているColor名を使用できます。

デフォルト:

``` text
White
```

------------------------------------------------------------------------

### Action Light Duration

`ActionLightDurationMs` はAction Lightを表示する時間を制御します。

値はミリ秒単位で指定します。

例:

``` json
"ActionLightDurationMs": 2000
```

では、Lightが約2秒間表示されます。

デフォルト:

``` text
1000
```

前回のAction
Lightが終了する前にInteractionが再度発動した場合、表示時間は最初から再開されます。

------------------------------------------------------------------------

## Action Wobble

`ActionWobble` はTile Action成功後、一時的にBig CraftableへWobble
Effectを適用します。

例:

``` json
{
    "ActionWobble": true,
    "ActionWobbleDurationMs": 1000
}
```

`ActionWobbleDurationMs` はミリ秒単位で指定します。

デフォルト:

``` text
1000
```

Action WobbleはIdle Wobbleより優先されます。

Big CraftableのProcessing Stateは変更しません。

------------------------------------------------------------------------

## Action Texture and Animation

Tile Action成功後、一時的にカスタムTextureを表示できます。

<a id="loading-the-texture"></a>

### Textureの読み込み

最初にContent PatcherからTexture Assetを読み込みます。

例:

``` json
{
    "Action": "Load",
    "Target": "MyMod/MyBigCraftableTexture",
    "FromFile": "assets/MyBigCraftableTexture.png"
}
```

------------------------------------------------------------------------

<a id="static-action-texture"></a>

### 静止Action Texture

静止Action Textureを表示する場合:

``` json
{
    "TextureSize": "17, 32",
    "ActionTexture": "MyMod/MyBigCraftableTexture",
    "ActionTexturePosition": "17, 0",
    "ActionDurationMs": 2000
}
```

`ActionTexturePosition` はAction Texture内にあるFrame
0の左上Pixel座標を指定します。

形式:

``` text
"X, Y"
```

`ActionFrames` を指定していない場合、Action Frame `0` が
`ActionDurationMs` の間表示されます。

表示時間が終了すると、次に適用可能なTexture Stateへ戻ります。

------------------------------------------------------------------------

<a id="animated-action-texture"></a>

### Action TextureのAnimation

Action Animationを作成する場合:

``` json
{
    "TextureSize": "17, 32",
    "ActionTexture": "MyMod/MyBigCraftableTexture",
    "ActionTexturePosition": "0, 0",
    "ActionFrames": [ 0, 1, 2, 3, 2, 1 ],
    "ActionFrameDurationMs": [ 300, 300, 450, 1200, 450, 300 ]
}
```

Frameは横方向に並びます。

各FrameのSource位置には、`TextureSize` で定義したWidthを使用します:

``` text
X = ActionTexturePosition.X + (Frame × TextureWidth)
Y = ActionTexturePosition.Y
```

たとえば次の場合:

``` text
TextureSize = "17, 32"
ActionTexturePosition = "0, 0"
```

Frame位置は次のとおりです:

``` text
Frame 0 → (0, 0)
Frame 1 → (17, 0)
Frame 2 → (34, 0)
Frame 3 → (51, 0)
```

Action Animationは最初から最後まで1回再生されます。

Animationが終了すると、次に適用可能なVisual Stateが表示されます。

------------------------------------------------------------------------

### Action Frame Duration

`ActionFrameDurationMs` には単一の値:

``` json
"ActionFrameDurationMs": 300
```

またはFrameごとの値を指定できます:

``` json
"ActionFrameDurationMs": [ 300, 300, 450, 1200, 450, 300 ]
```

単一の値を使用した場合、すべてのFrameで同じ表示時間を使用します。

配列を使用する場合、表示時間の値の数は `ActionFrames`
のEntry数と完全に一致させる必要があります。

数が一致しない場合、Action Animationは無効になり、T's
Coreが警告をログへ出力します。

`ActionFrames` を指定している場合、`ActionDurationMs` は無視されます。

Action全体の継続時間は各Frameの表示時間によって決まります。

------------------------------------------------------------------------

<a id="idle-effects"></a>

## Idle Effect

`IdleEffects` では、Big CraftableがIdle状態のときにMachine
Effectを再生できます。

この機能では、次の状態のBig CraftableをIdleと判定します:

``` text
readyForHarvest = false
and
minutesUntilReady <= 0
```

そのため、Machine型Big
CraftableがItemを処理中、または回収可能なOutputがある状態ではIdle
Effectは実行されません。

例:

``` json
{
    "IdleEffects": [
        {
            "Id": "MyIdleEffect",
            "Sounds": [
                {
                    "Id": "bubbles"
                }
            ]
        }
    ],
    "IdleEffectChance": 0.5
}
```

Idle Effectはゲーム内時刻が変化したときに判定されます。

T's
Coreは設定されたEffectを順番に試し、最初に正常に再生されたEffectで処理を終了します。

------------------------------------------------------------------------

### Idle Effect Chance

`IdleEffectChance` は判定時に `IdleEffects` を再生する確率を制御します。

値は `0`～`1` です。

デフォルト:

``` text
1
```

例:

``` json
"IdleEffectChance": 0.25
```

では、判定のたびに設定したIdle Effectが25%の確率で再生されます。

------------------------------------------------------------------------

## Idle Wobble

`IdleWobble` は、Big CraftableがIdle状態の間、Stardew
ValleyのMachine風Wobble Effectを適用します。

例:

``` json
{
    "IdleWobble": true
}
```

Idle Wobbleは次の状態でのみ有効です:

``` text
readyForHarvest = false
and
minutesUntilReady <= 0
```

Machine型Big
Craftableが処理中、または回収可能なOutputがある場合は無効になります。

Action Wobbleが有効な間は、Action Wobbleが優先されます。

------------------------------------------------------------------------

## Normal Light

`Light` はBig Craftableへ通常のT's Core Lightを追加します。

例:

``` json
{
    "Light": true,
    "LightOffset": "0, -0.5",
    "LightRadius": 2.0,
    "LightColor": "White"
}
```

`ActionLight` とは異なり、このLightには特定の表示時間制限はありません。

T's CoreのNormal Lightが適用可能な間、設置されたBig
Craftableに関連付けられた状態を維持します。

Big Craftableを撤去すると、対応するT's Core Lightも削除されます。

------------------------------------------------------------------------

### Light Offset

`LightOffset` は `ActionLightOffset` と同じTile単位の形式を使用します:

``` text
"X, Y"
```

小数値と負の値に対応しています。

例:

``` json
"LightOffset": "0, 0"
```

ではLightをAnchor Tileの中央へ配置します。

``` json
"LightOffset": "0, -0.5"
```

ではLightを上へ0.5Tile移動します。

``` json
"LightOffset": "0.5, -1"
```

ではLightを右へ0.5Tile、上へ1Tile移動します。

------------------------------------------------------------------------

### Light Radius

`LightRadius` はNormal Lightの半径を制御します。

デフォルト:

``` text
2
```

------------------------------------------------------------------------

### Light Color

`LightColor` はNormal Lightの色を指定します。

例:

``` json
"LightColor": "DarkCyan"
```

デフォルト:

``` text
White
```

------------------------------------------------------------------------

<a id="light-priority-for-machines"></a>

### MachineでのLightの優先順位

Big CraftableにStardew ValleyのMachine Dataも設定されている場合、T's
Coreは一時的なAction LightとStardew Valleyバニラの `LightWhileWorking`
を優先関係に含めます。

優先順位:

``` text
Action Light
    ↓
Vanilla LightWhileWorking
    ↓
T's Core Normal Light
```

Action Lightが有効な間、通常のT's Core Lightは一時的に抑制されます。

Big Craftableにバニラの `LightWhileWorking`
Dataがあり、現在処理中の場合、通常のT's Core
Lightの代わりにバニラのWorking Lightが使用されます。

それらの上位Lightが適用されなくなると、通常のT's Core Lightへ戻ります。

Machine Dataを持たない通常のBig Craftableでは、バニラの
`LightWhileWorking` の段階は適用されません。

------------------------------------------------------------------------

<a id="texture-priority"></a>

## Textureの優先順位

BigCraftable ExtensionのTextureは、Stardew Valleyの通常のMachine
Animationシステムと共存するよう設計されています。

Visualの優先順位:

``` text
T's Core Action Texture / Animation
        ↓
Vanilla Working Texture / Animation
        ↓
T's Core Normal Texture / Animation
```

### Action

Action Texture / AnimationはT's CoreのTextureの中で最優先です。

Tile Action成功後、一時的に現在のVisual Stateを上書きします。

Action状態が終了すると、次に適用可能なStateが表示されます。

------------------------------------------------------------------------

### Working

Working TextureとAnimationはStardew Valleyによって処理されます。

T's Coreはバニラの `WorkingTexture` やWorking
Animationシステムを置き換えたり変更したりしません。

Big
CraftableがMachineとして動作しており現在処理中の場合、バニラのWorking
StateがT's CoreのNormal Texture Stateより優先されます。

------------------------------------------------------------------------

### Normal

上位のAction StateまたはWorking Stateが適用されていない場合、T's
CoreのNormal Texture / AnimationがBigCraftable
ExtensionのデフォルトVisual Stateになります。

BigCraftable Extensionの `Texture` Propertyを省略した場合、Big
Craftableの通常Textureを使用します。

`TexturePosition` も省略した場合、Stardew
Valleyの通常のSource位置が維持されます。

これにより、別のTexture Assetを用意せずにNormal
Animationを追加できます。

------------------------------------------------------------------------

<a id="complete-example"></a>

## 完全な使用例

次の例では、BigCraftable Extensionの主な機能をまとめて使用しています。

### Big Craftable Data

最初にBig Craftableを追加または編集し、Extensionへ関連付けます:

``` json
{
    "Action": "EditData",
    "Target": "Data/BigCraftables",
    "Entries": {
        "MyMod_MyBigCraftable": {
            "Name": "MyBigCraftable",
            "DisplayName": "My Big Craftable",
            "Description": "An example Big Craftable.",
            "Price": 500,
            "Fragility": 0,
            "CanBePlacedOutdoors": true,
            "CanBePlacedIndoors": true,
            "Texture": "MyMod/MyBigCraftableTexture",
            "SpriteIndex": 0,
            "CustomFields": {
                "TsCore/BigCraftableExtension": "MyMod/MyExtension"
            }
        }
    }
}
```

------------------------------------------------------------------------

### Texture

Textureを読み込みます:

``` json
{
    "Action": "Load",
    "Target": "MyMod/MyBigCraftableTexture",
    "FromFile": "assets/MyBigCraftableTexture.png"
}
```

------------------------------------------------------------------------

### BigCraftable Extension

``` json
{
    "Action": "EditData",
    "Target": "TsCore/BigCraftableExtension",
    "Entries": {
        "MyMod/MyExtension": {
            "CollisionSize": "1, 2",
            "PlayerPassableTiles": [
                "0, -1",
                "0, 0"
            ],
            "TileProperties": [
                {
                    "Id": "ExampleTouchAction",
                    "Name": "TouchAction",
                    "Value": "Message \"You stepped onto the Big Craftable.\"",
                    "Layer": "Back",
                    "Tiles": [
                        "0, -1",
                        "0, 0"
                    ]
                }
            ],

            "TextureSize": "17, 32",
            "Frames": [ 0, 1, 2, 3 ],
            "FrameDurationMs": 300,

            "DrawLayers": [
                {
                    "Id": "FrontLayer",
                    "Texture": null,
                    "SourceRect": {
                        "X": 0,
                        "Y": 32,
                        "Width": 17,
                        "Height": 32
                    },
                    "DrawPosition": "0, 0",
                    "DrawLayer": "Front",
                    "FrameCount": 1,
                    "FramesPerRow": -1,
                    "FrameDuration": 100
                }
            ],

            "TileAction": "Message \"BigCraftable Extension Test\"",

            "Condition": "PLAYER_HAS_MAIL TestMail",
            "InvalidConditionMessage": "The condition has not been met.",

            "RequiredItem": "(O)388",
            "RequiredItemCount": 5,
            "ConsumeRequiredItem": true,
            "InvalidItemMessage": "You need to hold Wood.",
            "InvalidCountMessage": "You need at least [ItemCount] Wood.",

            "ActionEffects": [
                {
                    "Id": "ActionEffect1",
                    "Sounds": [
                        {
                            "Id": "coin"
                        }
                    ]
                }
            ],
            "ActionEffectChance": 1.0,

            "ActionLight": true,
            "ActionLightOffset": "0, -0.5",
            "ActionLightRadius": 2.0,
            "ActionLightColor": "White",
            "ActionLightDurationMs": 1000,

            "ActionWobble": true,
            "ActionWobbleDurationMs": 1000,

            "ActionTexture": "MyMod/MyBigCraftableTexture",
            "ActionTexturePosition": "0, 0",
            "ActionFrames": [ 3, 2, 1, 0 ],
            "ActionFrameDurationMs": 300,
            "ActionDurationMs": 1000,

            "IdleEffects": [
                {
                    "Id": "IdleEffect1",
                    "Sounds": [
                        {
                            "Id": "bubbles"
                        }
                    ]
                }
            ],
            "IdleEffectChance": 1.0,

            "IdleWobble": true,

            "Light": true,
            "LightOffset": "0, -0.5",
            "LightRadius": 2.0,
            "LightColor": "White"
        }
    }
}
```

この例では:

-   1 × 2のCollision範囲を作成します。
-   プレイヤーが両方のCollision Tileを通過できるようにします。
-   両方のCollision Tileへカスタム `TouchAction` Tile
    Propertyを追加します。
-   17 × 32 PixelのTexture Frameを使用します。
-   4FrameのNormal AnimationをLoop再生します。
-   追加のFront Draw Layerを描画します。
-   `PLAYER_HAS_MAIL TestMail` Conditionを必須にします。
-   プレイヤーがWoodを5個以上手に持っていることを必須にします。
-   Interaction成功後にWoodを5個消費します。
-   設定したTile Actionを実行します。
-   Action Machine Effectを再生します。
-   一時的なAction Lightを表示します。
-   Action Wobbleを適用します。
-   カスタムAction Animationを再生します。
-   Idle状態の間にIdle Effectを再生します。
-   Idle Wobbleを適用します。
-   適用可能な場合、通常のT's Core Lightを表示します。

> **注意:** 上の例には利用可能なPropertyを示すため `ActionDurationMs`
> も含めていますが、`ActionFrames`
> を指定している間は効果がありません。Action Animationの継続時間は
> `ActionFrameDurationMs` によって決まります。

------------------------------------------------------------------------

<a id="notes"></a>

## 注意事項

BigCraftable ExtensionはContent Patcher
Packから使用することを想定しており、独自のC#コードは必要ありません。

Extensionは次のCustom Fieldを通してBig Craftableへ関連付けます:

``` text
Data/BigCraftables
    CustomFields
        TsCore/BigCraftableExtension
```

Custom Fieldの値は、次のData Asset内のEntryと一致する必要があります:

``` text
TsCore/BigCraftableExtension
```

主なInteractionの処理順序:

``` text
Condition
    ↓
Required Item
    ↓
Tile Action
    ↓
Action Effects / Light / Wobble
    ↓
Action Texture / Animation
    ↓
Required Item consumption
```

Conditionを満たさない場合、Required Itemの判定とTile
Actionは実行されません。

Required Itemの判定に失敗した場合、Tile Actionは実行されません。

Required ItemはTile Action成功後にのみ消費されます。

ActionとIdleのMachine Effectには、Stardew Valleyの `MachineEffects` Data
Structureを使用します。

Texture Frameのサイズは次のPropertyで制御します:

``` json
"TextureSize": "Width, Height"
```

例:

``` json
"TextureSize": "17, 32"
```

Normal AnimationとAction AnimationのFrameは、設定したTexture
Widthを使用して横方向に配置されます。

Collisionのサイズは次のPropertyで制御します:

``` json
"CollisionSize": "Width, Height"
```

例:

``` json
"CollisionSize": "1, 2"
```

Collision WidthはAnchor Tileから右方向へ広がります。

Collision HeightはAnchor Tileから上方向へ広がります。

プレイヤーが通過可能なCollision Tileは、Anchorを基準とした相対Tile
Offsetを使用します:

``` json
"PlayerPassableTiles": [ "0, -1", "0, 0" ]
```

`TileProperties` もAnchorを基準とした相対Tile Offsetを使用します。

Draw Layerの位置には **Source Pixel Offset** を使用し、Draw Layerの
`SourceRect` には **Source TextureのPixel座標** を使用します。

Light Offsetには **Tile座標** を使用し、小数値にも対応しています:

``` text
"0, -0.5"
```

Textureの位置とサイズには **Source TextureのPixel座標** を使用します:

``` text
TexturePosition       = "0, 32"
ActionTexturePosition = "0, 64"
TextureSize           = "17, 32"
```

これらの値では、意図的に異なる単位を使用しています:

  -------------------------------------------------------------------------------------------------
  Property                     単位              例
  ---------------------------- ----------------- --------------------------------------------------
  `CollisionSize`              Tile              `"1, 2"`

  `PlayerPassableTiles`        相対Tile          `"0, -1"`

  `TileProperties.Tiles`       相対Tile          `"0, -1"`

  `TextureSize`                Source Pixel      `"17, 32"`

  `DrawLayers.DrawPosition`    Source Pixel      `"4, -2"`
                               Offset            

  `DrawLayers.SourceRect`      Source Pixel      `{ "X": 0, "Y": 32, "Width": 17, "Height": 32 }`

  `TexturePosition`            Source Pixel      `"0, 32"`

  `ActionTexturePosition`      Source Pixel      `"0, 64"`

  `ActionLightOffset`          Tile              `"0, -0.5"`

  `LightOffset`                Tile              `"0.5, -1"`
  -------------------------------------------------------------------------------------------------

T's CoreはStardew ValleyバニラのWorking TextureやWorking
Animationの動作を変更しません。

今後のT's Coreで、BigCraftable
Extensionの機能が追加される場合があります。

------------------------------------------------------------------------

## Modder Guide

-   ← [Building Services](ModderGuide_BuildingServices.md)
-   ↑ [Guide Index](#top)
-   → [Dialogue System](ModderGuide_DialogueSystem.md)

← [READMEに戻る](../../README.md)

← [Modder Guideに戻る](ModderGuide.md)
