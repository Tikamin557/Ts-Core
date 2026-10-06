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
-   ✅ **Dialogue System** *(現在のページ)*
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

# Dialogue System

Dialogue Systemを使用すると、Content Patcher PackからT's
Coreを利用したデータ駆動型のカスタムDialogueを作成できます。

Dialogueは次のカスタムData Assetを通して登録します:

``` text
TsCore/Dialogues
```

次のActionを使用して、MapのTile ActionやTouch
Actionから開くことができます:

``` text
TsCoreDialogue <DialogueId>
```

Dialogueでは、テキスト表示、プレイヤーのResponse、単一または複数のGame
State Query Condition、Trigger
Actionの実行、別Dialogueへの継続、Buildingごとの使用回数制限、重み付きRandom
Action、必要に応じたMessage非表示、指定時間後の自動終了、Audio
Cueの再生を設定できます。

C#コードは必要ありません。

------------------------------------------------------------------------

## 目次

-   [Overview](#overview)
-   [Content Pack Setup](#content-pack-setup)
-   [Dialogue Action](#dialogue-action)
-   [Dialogue Properties](#dialogue-properties)
-   [Text](#text)
-   [Conditions and FailText](#conditions-and-failtext)
-   [Multiple Conditions](#multiple-conditions)
-   [HideDialogue](#hidedialogue)
-   [UsageLimit](#usagelimit)
-   [Audio Cues](#audio-cues)
-   [Responses](#responses)
-   [Response Conditions](#response-conditions)
-   [Response Actions](#response-actions)
-   [Next Dialogue](#next-dialogue)
-   [AfterActions](#afteractions)
-   [RandomActions](#randomactions)
-   [Duration](#duration)
-   [Multi-page Dialogue](#multi-page-dialogue)
-   [Complete Example](#complete-example)
-   [Debugging](#debugging)
-   [Reloading Content Patcher Content
    Packs](#reloading-content-patcher-content-packs)
-   [Notes](#notes)

------------------------------------------------------------------------

<a id="overview"></a>

## 概要

T's CoreのDialogueは次のData Assetに保存されます:

``` text
TsCore/Dialogues
```

Data Asset内の各Entryが1つのDialogueを表します。

例:

``` json
{
  "Action": "EditData",
  "Target": "TsCore/Dialogues",
  "Entries": {
    "MyMod/MyDialogue": {
      "Text": "Hello!"
    }
  }
}
```

`Entries` のKeyがDialogue IDになります。

この例では:

``` text
MyMod/MyDialogue
```

がDialogue IDです。

その後、Mapから次のように表示できます:

``` text
TsCoreDialogue MyMod/MyDialogue
```

基本的な処理の流れ:

``` text
TsCoreDialogue
    ↓
Dialogue ID
    ↓
TsCore/Dialogues
    ↓
Condition / Conditions
    ↓
UsageLimit
    ↓
Dialogue Text / Responses
    ↓
Response Condition / Conditions
    ↓
Response Actions
    ↓
Next Dialogue
    ↓
AfterActions / RandomActions
```

設定したPropertyによっては、すべてのStepが必要になるわけではありません。

------------------------------------------------------------------------

<a id="content-pack-setup"></a>

## Content Packのセットアップ

DialogueはContent Patcherから次のData Assetへ登録します:

``` text
TsCore/Dialogues
```

通常のContent Patcher Content Packを使用します。

### manifest.json

``` json
{
  "Name": "[CP] My Dialogue Pack",
  "Author": "YourName",
  "Version": "1.0.0",
  "UniqueID": "YourName.MyDialoguePack",
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

------------------------------------------------------------------------

### content.json

`EditData` を使用してDialogueを追加します:

``` json
{
  "Format": "2.9.0",
  "Changes": [
    {
      "Action": "EditData",
      "Target": "TsCore/Dialogues",
      "Entries": {
        "YourName.MyMod_Greeting": {
          "Text": "Hello!"
        }
      }
    }
  ]
}
```

`Entries` のKeyがDialogue IDになります。

カスタムDialogueでは、可能な場合は自分のModのUniqueIDを基にしたIDを使用することを推奨します。

例:

``` text
YourName.MyMod_Greeting
```

1つの `EditData` Patchで複数のDialogueを登録できます。

``` json
{
  "Action": "EditData",
  "Target": "TsCore/Dialogues",
  "Entries": {
    "YourName.MyMod_Dialogue1": {
      "Text": "This is the first Dialogue."
    },
    "YourName.MyMod_Dialogue2": {
      "Text": "This is the second Dialogue."
    }
  }
}
```

T's
Coreでは、特別なFolder構成やDialogueごとの個別JSONファイルは必要ありません。

------------------------------------------------------------------------

## Dialogue Action

T's Coreでは次のカスタムActionを提供しています:

``` text
TsCoreDialogue
```

構文:

``` text
TsCoreDialogue <DialogueId>
```

例:

``` text
TsCoreDialogue YourName.MyMod_Greeting
```

このActionはTile ActionとTouch Actionのどちらでも使用できます。

------------------------------------------------------------------------

### Tile Action

たとえば次のMap
Patchでは、プレイヤーがTileを操作したときにDialogueを開きます:

``` json
{
  "Action": "EditMap",
  "Target": "Maps/Farm",
  "MapTiles": [
    {
      "Position": {
        "X": 68,
        "Y": 9
      },
      "Layer": "Buildings",
      "SetProperties": {
        "Action": "TsCoreDialogue YourName.MyMod_Greeting"
      }
    }
  ]
}
```

------------------------------------------------------------------------

### Touch Action

Touch Actionでも同じ構文を使用できます:

``` json
{
  "Action": "EditMap",
  "Target": "Maps/Farm",
  "MapTiles": [
    {
      "Position": {
        "X": 68,
        "Y": 9
      },
      "Layer": "Back",
      "SetProperties": {
        "TouchAction": "TsCoreDialogue YourName.MyMod_Greeting"
      }
    }
  ]
}
```

Touch Actionが発動するとDialogueが開きます。

------------------------------------------------------------------------

<a id="dialogue-properties"></a>

## DialogueのProperty

Dialogue Entryでは次のPropertyを使用できます。

  -------------------------------------------------------------------------------------------------------------------------------------
  Property               デフォルト          説明
  ---------------------- ------------------- ------------------------------------------------------------------------------------------
  `Text`                 `""`                Dialogueに表示するテキスト。

  `HideDialogue`         `false`             このDialogueが生成するすべてのMessageを非表示にしつつ、LogicとActionの処理は継続します。

  `Condition`            ---                 Dialogueを表示する前に満たす必要がある単一のGame State Query。

  `FailText`             ---                 `Condition` を満たしていない場合に表示するテキスト。

  `Conditions`           ---                 個別の失敗時動作を設定できる複数のGame State Query Condition。

  `UsageLimit`           ---                 任意の使用回数制限。現在はBuildingごとに1日1回の使用に対応しています。

  `AudioCue`             ---                 DialogueがConditionを正常に通過したときに再生するAudio Cue。

  `FailAudioCue`         ---                 単一の `Condition` が失敗したときに再生するAudio Cue。

  `Responses`            ---                 プレイヤーのResponse選択肢。

  `AfterActions`         ---                 Dialogue Chainが閉じた後に実行するTrigger Action。

  `RandomActions`        ---                 重み付きのAction候補。Dialogue成功後に1つの候補を選択して実行します。

  `Duration`             `0`                 Responseを持たないDialogueを、指定したミリ秒後に自動的に閉じます。
  -------------------------------------------------------------------------------------------------------------------------------------

各Propertyについて、以下で詳しく説明します。

------------------------------------------------------------------------

## Text

`Text` はDialogueに表示するテキストを指定します。

例:

``` json
{
  "Action": "EditData",
  "Target": "TsCore/Dialogues",
  "Entries": {
    "YourName.MyMod_Greeting": {
      "Text": "Welcome!"
    }
  }
}
```

Dialogueを開くと、プレイヤーには次のように表示されます:

``` text
Welcome!
```

Dialogueに `Responses` がない場合、通常のText Dialogueとして動作します。

`Responses` が定義されている場合、`Text`
はResponse選択肢の上に表示される質問またはPromptになります。

------------------------------------------------------------------------

<a id="conditions-and-failtext"></a>

## ConditionとFailText

`Condition` を使用して、Main
Dialogueを利用可能にする条件を制御できます。

Stardew ValleyのGame State Queryシステムを使用します。

例:

``` json
{
  "Text": "The shop is currently open.",
  "Condition": "TIME 1000 2000",
  "FailText": "The shop is currently closed."
}
```

Conditionが成功した場合:

``` text
The shop is currently open.
```

が表示されます。

Conditionが失敗した場合:

``` text
The shop is currently closed.
```

が代わりに表示されます。

Dialogue Levelの `Condition` が失敗した場合:

-   Main Dialogueは表示されません。
-   `Responses` は表示されません。
-   `AudioCue` は再生されません。
-   `AfterActions` は登録されません。
-   `FailText` が指定されていれば表示されます。
-   `FailAudioCue` が指定されていれば再生されます。

`Condition` を省略した場合、Dialogueは常に利用できます。

------------------------------------------------------------------------

<a id="multiple-conditions"></a>

## 複数のCondition

異なる失敗時動作を持つ複数の条件がDialogueに必要な場合、`Conditions`
を使用できます。

単一の `Condition`
とは異なり、各Entryにはそれぞれ次の項目を設定できます:

-   `Condition`
-   `FailText`
-   `FailAudioCue`
-   `FailActions`
-   `FailNext`

Conditionは上から順番に判定されます。

最初に失敗したConditionの失敗時動作が処理され、それ以降のConditionは判定されません。

例:

``` json
{
  "Text": "You may enter.",
  "Conditions": [
    {
      "Condition": "PLAYER_HAS_MAIL MyQuestComplete",
      "FailText": "You haven't completed the quest yet.",
      "FailAudioCue": "cancel"
    },
    {
      "Condition": "PLAYER_CURRENT_MONEY Current 2000",
      "FailText": "You need 2000G.",
      "FailAudioCue": "cancel"
    }
  ]
}
```

処理順序:

``` text
Condition 1
    ↓
┌─ Failed → its Fail behavior
│
└─ Passed
      ↓
   Condition 2
      ↓
   ┌─ Failed → its Fail behavior
   │
   └─ Passed
         ↓
      Main Dialogue
```

### FailActions

`FailActions` は、そのConditionが失敗したときに実行するTrigger
Actionを指定します。

``` json
{
  "Condition": "PLAYER_HAS_MAIL MyMail",
  "FailText": "You cannot enter yet.",
  "FailActions": [
    "AddMoney 100"
  ]
}
```

`FailText` が表示される場合、`FailActions` はそのFailure
Dialogueを閉じた後に実行されます。

Failure Textが表示されない場合、Failure Actionはすぐに処理できます。

他のTrigger Action Listと同様に、Actionは順番に実行されます。

### FailNext

`FailNext`
は、そのConditionが失敗したときに開く別のDialogueを指定します。

``` json
{
  "Condition": "PLAYER_HAS_MAIL MyMail",
  "FailText": "You cannot enter yet.",
  "FailNext": "YourName.MyMod_NotAvailable"
}
```

`FailText` が表示される場合、Failure Dialogueを閉じた後に `FailNext`
が開きます。

`FailActions` は `FailNext` より先に処理されます。

<a id="conditions-on-responses"></a>

### Response内のConditions

`Conditions` はResponse内でも使用できます。

``` json
{
  "Text": "Yes",
  "Conditions": [
    {
      "Condition": "PLAYER_CURRENT_MONEY Current 2000",
      "FailText": "You don't have enough money."
    },
    {
      "Condition": "PLAYER_HAS_MAIL MyPermit",
      "FailText": "You don't have the required permit."
    }
  ],
  "Actions": [
    "AddMoney -2000"
  ]
}
```

すべてのEntryが成功した場合にのみResponseが成功します。

最初に失敗したEntryが、そのEntry自身の
`FailText`、`FailAudioCue`、`FailActions`、`FailNext` を処理します。

> **注意:** 従来の単一 `Condition`、`FailText`、`FailAudioCue`
> Propertyも引き続き対応しています。条件ごとに異なる失敗時動作が必要な場合は
> `Conditions` を使用してください。

------------------------------------------------------------------------

## HideDialogue

`HideDialogue`
は、そのDialogueが生成するすべてのMessageを非表示にします。

デフォルト:

``` json
"HideDialogue": false
```

例:

``` json
{
  "HideDialogue": true,
  "AudioCue": "coin",
  "AfterActions": [
    "AddMoney 100"
  ]
}
```

`HideDialogue` が `true` の場合:

-   Dialogueの通常Textは表示されません。
-   そのDialogueが生成するFailure Textは表示されません。
-   Conditionは引き続き判定されます。
-   Audio Cueは引き続き再生できます。
-   Failure Actionと失敗時の継続処理も引き続き動作します。
-   成功時の `AfterActions` と `RandomActions` も引き続き実行されます。

閉じるDialogue Windowがないため、成功時の `AfterActions` と
`RandomActions` はすぐに処理されます。

非表示のDialogueに `Responses`
が定義されている場合、Response選択肢を表示できないためResponsesはスキップされます。

Message Windowを表示せず、Interaction LogicやReward処理のためにDialogue
Systemを使用する場合に便利です。

------------------------------------------------------------------------

## UsageLimit

`UsageLimit` を使用すると、Dialogueが成功できる回数を制限できます。

現在対応している設定:

  ---------------------------------------------------------------------------------------------------
  Property          対応値                         説明
  ----------------- ------------------------------ --------------------------------------------------
  `Scope`           `Building`                     個別のBuilding Instanceに使用状態を保存します。

  `Period`          `Day`                          ゲーム内1日につき1回Dialogueを成功可能にします。

  `Key`             Custom string                  Buildingの `modData`
                                                   に使用状態を保存するためのKey。
  ---------------------------------------------------------------------------------------------------

例:

``` json
{
  "UsageLimit": {
    "Scope": "Building",
    "Period": "Day",
    "Key": "YourName.MyMod/TreeReward"
  },
  "Text": "You found an item!",
  "AfterActions": [
    "AddItem (O)595 1 0"
  ]
}
```

`Scope: Building` と `Period: Day`
では、個々のBuildingごとに1日1回Dialogueを使用できます。

たとえば同じBuildingが3つ存在する場合:

``` text
Building A → available
Building B → available
Building C → available
```

Building Aを使用しても、Building BやCは使用済みになりません。

そのため、Buildingごとに使用状態を個別管理したい場合でも、複数のInstanceで同じ
`Key` を共有できます。

使用済み状態は、そのBuildingの `modData` に保存されます。

<a id="building-source-requirement"></a>

### Building Sourceの要件

`Scope: Building`
を使用するには、Dialogueが実際のBuildingから開始されている必要があります。

たとえば、T's Core Dialogueを開くBuilding
Actionと組み合わせて使用できます。

通常のMap Tile ActionなどBuilding
Sourceなしで同じDialogueを開始した場合、T's
CoreはどのBuildingに使用状態を保持すべきか判断できません。その場合Dialogueは中止され、警告がログへ出力されます。

<a id="when-the-usage-is-recorded"></a>

### 使用状態を記録するタイミング

Dialogueが使用済みとして記録されるのは、成功側の処理が完了した後だけです。

成功側のTrigger Actionが失敗した場合、使用済みとして記録されません。

これにより、Actionの失敗によってその日の使用回数を失うことなく、プレイヤーはInteractionを再試行できます。

------------------------------------------------------------------------

<a id="audio-cues"></a>

## Audio Cue

Dialogueを表示するときにAudio Cueを再生できます。

使用:

``` json
"AudioCue": "coin"
```

例:

``` json
{
  "Text": "Welcome!",
  "AudioCue": "coin"
}
```

Dialogueが `Condition` を正常に通過して開かれるときにAudio
Cueが再生されます。

Dialogueごとに異なるAudio Cueを使用できます。

これは `Next` から開かれるDialogueにも適用されます。

例:

``` json
{
  "Text": "Warp complete!",
  "AudioCue": "coin",
  "Duration": 1500
}
```

では、Textが表示されるときにAudio
Cueを再生し、1500ミリ秒後にDialogueを自動的に閉じます。

------------------------------------------------------------------------

### Fail Audio Cue

`FailAudioCue` を使用すると、Dialogue Levelの `Condition`
が失敗したときに別のAudio Cueを再生できます。

例:

``` json
{
  "Text": "You may enter.",
  "Condition": "PLAYER_HAS_MAIL MyMail",
  "FailText": "You cannot enter yet.",
  "AudioCue": "coin",
  "FailAudioCue": "cancel"
}
```

Conditionが成功した場合:

``` text
AudioCue
    ↓
Main Dialogue
```

Conditionが失敗した場合:

``` text
FailAudioCue
    ↓
FailText
```

Conditionが失敗した場合、`AudioCue` は再生されません。

------------------------------------------------------------------------

## Responses

`Responses` はDialogueへプレイヤーの選択肢を追加します。

例:

``` json
{
  "Text": "Would you like to continue?",
  "Responses": [
    {
      "Text": "Yes"
    },
    {
      "Text": "No"
    }
  ]
}
```

プレイヤーには2つのResponse選択肢が表示されます:

``` text
Yes
No
```

各Responseにはそれぞれ独自の動作を設定できます。

------------------------------------------------------------------------

<a id="response-properties"></a>

### ResponseのProperty

`Responses` の各Entryでは次のPropertyを使用できます。

  ---------------------------------------------------------------------------------------------
  Property               デフォルト          説明
  ---------------------- ------------------- --------------------------------------------------
  `Text`                 `""`                Response選択肢に表示するテキスト。

  `Condition`            ---                 このResponseが選択されたときに判定する単一のGame
                                             State Query。

  `FailText`             ---                 Responseの `Condition`
                                             が失敗した場合に表示するテキスト。

  `Conditions`           ---                 個別の失敗時動作を設定できる複数のGame State Query
                                             Condition。

  `FailAudioCue`         ---                 Responseの `Condition`
                                             が失敗した場合に再生するAudio Cue。

  `Actions`              ---                 Response成功時に実行するTrigger Action。

  `Next`                 ---                 Response成功後に開くDialogue ID。
  ---------------------------------------------------------------------------------------------

------------------------------------------------------------------------

<a id="response-conditions"></a>

## ResponseのCondition

各Responseには独自の `Condition` を設定できます。

Dialogue LevelのConditionとは異なり、ResponseのConditionは
**プレイヤーがそのResponseを選択したとき** に判定されます。

例:

``` json
{
  "Text": "Use the special fishing area? It costs 2000G.",
  "Responses": [
    {
      "Text": "Yes",
      "Condition": "PLAYER_CURRENT_MONEY Current 2000",
      "FailText": "You don't have enough money."
    },
    {
      "Text": "No"
    }
  ]
}
```

プレイヤーが `Yes`
を選択し、2000G以上所持していればResponseが成功します。

プレイヤーの所持金が不足している場合:

``` text
You don't have enough money.
```

が表示されます。

ResponseのConditionが失敗した場合、そのResponseの `Actions` と `Next`
は処理されません。

Responseでも `Conditions`
を使用して、複数の条件に個別の失敗時動作を設定できます。[複数のCondition](#multiple-conditions)を参照してください。

------------------------------------------------------------------------

<a id="response-fail-audio-cue"></a>

### ResponseのFail Audio Cue

Responseにも `FailAudioCue` を指定できます。

例:

``` json
{
  "Text": "Yes",
  "Condition": "PLAYER_CURRENT_MONEY Current 2000",
  "FailText": "You don't have enough money.",
  "FailAudioCue": "cancel"
}
```

所持金が不足した状態でプレイヤーがこのResponseを選択すると:

``` text
FailAudioCue
    ↓
FailText
```

Response Actionは実行されません。

------------------------------------------------------------------------

<a id="response-actions"></a>

## Response Action

`Actions` はResponse成功後に実行するStardew Valley Trigger
Actionを指定します。

例:

``` json
{
  "Text": "Yes",
  "Actions": [
    "AddMoney -2000"
  ]
}
```

複数のTrigger Actionを指定できます:

``` json
{
  "Text": "Yes",
  "Actions": [
    "AddMoney -2000",
    "TsCoreWarp BusStop 20 22"
  ]
}
```

Actionは順番に実行されます。

T's CoreのWarp Actionも使用できます:

``` text
TsCoreWarp
TsCoreMagicWarp
TsCoreMagicWarp_Simple
```

例:

``` json
{
  "Text": "Yes",
  "Actions": [
    "TsCoreMagicWarp Farm 64 15 Down"
  ]
}
```

T's CoreのNotification ActionもTrigger Actionとして使用できます:

``` text
TsCoreNotification
```

Stardew ValleyバニラのTrigger Actionも使用できます。

> **重要:** Response
> Actionは順番に実行されます。前のActionが成功した後で後続のActionが失敗しても、T's
> Coreはすでに成功したActionを元に戻しません。

------------------------------------------------------------------------

## Next Dialogue

`Next`
を使用すると、あるDialogueから別のDialogueへ続けることができます。

例:

``` json
{
  "Text": "Would you like to warp?",
  "Responses": [
    {
      "Text": "Yes",
      "Actions": [
        "TsCoreMagicWarp Farm 64 15 Down"
      ],
      "Next": "YourName.MyMod_Warped"
    },
    {
      "Text": "No",
      "Next": "YourName.MyMod_NotWarped"
    }
  ]
}
```

移動先のDialogueは同じData Assetへ登録できます:

``` json
{
  "YourName.MyMod_Warped": {
    "Text": "Warping!",
    "Duration": 1500
  },
  "YourName.MyMod_NotWarped": {
    "Text": "Warp cancelled."
  }
}
```

これにより2つの分岐が作成されます:

``` text
Yes
 ↓
Actions
 ↓
YourName.MyMod_Warped
```

もう一方:

``` text
No
 ↓
YourName.MyMod_NotWarped
```

`Next` は、選択したResponseの `Actions` が成功した後に処理されます。

ResponseのConditionが失敗した場合、`Next` は開かれません。

Actionが失敗した場合は処理が停止し、`Next` は開かれません。

------------------------------------------------------------------------

<a id="chaining-multiple-dialogues"></a>

### 複数Dialogueの連結

`Next` で開いたDialogue自体にResponsesと別の `Next` を設定できます。

これにより、複数のDialogueを連結できます。

例:

``` text
Dialogue A
    ↓
Response
    ↓
Dialogue B
    ↓
Response
    ↓
Dialogue C
```

各DialogueはDialogue IDを使用して `TsCore/Dialogues` から取得されます。

------------------------------------------------------------------------

## AfterActions

`AfterActions` はDialogueを閉じた後に実行するTrigger
Actionを指定します。

例:

``` json
{
  "Text": "The Dialogue will perform an Action when closed.",
  "AfterActions": [
    "AddMoney 100"
  ]
}
```

Responseの `Actions`
とは異なり、これらのActionはすぐには実行されません。

処理順序:

``` text
Dialogue displayed
    ↓
Player closes Dialogue
    ↓
AfterActions
```

Multi-page Dialogueでも同じように動作します。

`AfterActions` は個々のText
Pageごとではなく、Dialogue全体を閉じた後に実行されます。

------------------------------------------------------------------------

### AfterActionsとResponses

Responsesを含むDialogueでも `AfterActions` を使用できます。

例:

``` json
{
  "Text": "Choose an option.",
  "Responses": [
    {
      "Text": "Yes"
    },
    {
      "Text": "No"
    }
  ],
  "AfterActions": [
    "AddMoney 100"
  ]
}
```

`AfterActions` は、Question Dialogueとそこから続くDialogue
Chainがすべて終了した後に実行されます。

------------------------------------------------------------------------

### AfterActionsとNext

Responseから `Next` を通して別のDialogueへ続く場合も、`AfterActions`
は保持されます。

例:

``` text
Dialogue A
AfterActions: Action A
    ↓
Next
    ↓
Dialogue B
AfterActions: Action B
    ↓
Dialogue closes
    ↓
Action A
    ↓
Action B
```

Dialogue Chain内の複数のDialogueに `AfterActions`
が定義されている場合、それらのActionは蓄積され、最後のDialogueを閉じた後に登録順で実行されます。

------------------------------------------------------------------------

## RandomActions

`RandomActions` は重み付きRandom
Selectionを使用してAction候補を1つ選択します。

各候補には次のPropertyがあります:

  --------------------------------------------------------------------------------------------
  Property               デフォルト          説明
  ---------------------- ------------------- -------------------------------------------------
  `Weight`               `1`                 この候補が選択される相対的な確率。`0`
                                             より大きい値である必要があります。

  `Actions`              ---                 この候補が選択されたときに順番に実行するTrigger
                                             Action。
  --------------------------------------------------------------------------------------------

例:

``` json
{
  "Text": "You found a flower!",
  "RandomActions": [
    {
      "Weight": 7,
      "Actions": [
        "AddItem (O)595 1 0"
      ]
    },
    {
      "Weight": 2,
      "Actions": [
        "AddItem (O)595 2 0"
      ]
    },
    {
      "Weight": 1,
      "Actions": [
        "AddItem (O)595 3 0"
      ]
    }
  ]
}
```

Weightの合計:

``` text
7 + 2 + 1 = 10
```

そのため選択確率は:

``` text
Weight 7 → 70%
Weight 2 → 20%
Weight 1 → 10%
```

選択される候補は1つだけです。

選択された候補に複数の `Actions`
が含まれている場合、上から順番に実行されます。

<a id="when-randomactions-run"></a>

### RandomActionsを実行するタイミング

表示されるDialogueでは、`RandomActions` は `AfterActions`
と同じ成功時タイミングを使用し、Dialogueを閉じた後に処理されます。

両方が定義されている場合、成功側の処理順序は次のとおりです:

``` text
Dialogue closes
    ↓
AfterActions
    ↓
Selected RandomActions
```

`HideDialogue` が `true` の場合は閉じるDialogue
Windowがないため、これらの成功側Actionはすぐに処理されます。

<a id="stable-random-selection"></a>

### 安定したRandom Selection

`RandomActions` は決定論的なSelectionを使用します。

Selectionには現在のSave、ゲーム内日付、Dialogue IDが使用されます。

DialogueにBuilding Sourceがある場合、その個別BuildingのPersistent
IDも使用されます。

つまり:

``` text
same day + same Building + same Dialogue
→ same selected candidate

same day + different Building
→ independently selected candidate

next in-game day
→ new daily selection
```

同じBuildingが複数存在するときに、すべてが同じRandom結果にならないようにしたいBuilding
Rewardなどで特に便利です。

Building Sourceがない場合もRandom Selectionは機能しますが、Building
InstanceではなくDialogueと現在の日付を基準に選択されます。

### RandomActionsとUsageLimit

`RandomActions` は `UsageLimit` と組み合わせて使用できます。

``` json
{
  "UsageLimit": {
    "Scope": "Building",
    "Period": "Day",
    "Key": "YourName.MyMod/TreeReward"
  },
  "Text": "You found a flower!",
  "RandomActions": [
    {
      "Weight": 8,
      "Actions": [
        "AddItem (O)595 1 0"
      ]
    },
    {
      "Weight": 2,
      "Actions": [
        "AddItem (O)595 2 0"
      ]
    }
  ]
}
```

これにより、Building
Instanceごとに独自の日次Rewardが選択され、そのRewardを1日1回受け取れるようになります。

選択された候補内のActionが失敗した場合、処理は停止し、Usage
Limitは完了済みとして記録されません。

> **重要:** `Weight` の値は `0`
> より大きい必要があります。不正な重みの候補は拒否され、警告がログへ出力されます。

------------------------------------------------------------------------

## Duration

`Duration` は通常のText Dialogueを指定時間後に自動的に閉じます。

値は **ミリ秒** 単位で指定します。

例:

``` json
{
  "Text": "Warping!",
  "Duration": 1500
}
```

これによりDialogueを約1.5秒間表示し、その後自動的に閉じます。

`Duration` を省略、または `0`
以下にした場合、Dialogueは通常どおり手動で閉じる動作になります。

------------------------------------------------------------------------

### DurationとAfterActions

`Duration` は `AfterActions` と組み合わせて使用できます。

例:

``` json
{
  "Text": "Warp complete!",
  "Duration": 1500,
  "AfterActions": [
    "AddMoney 100"
  ]
}
```

処理順序:

``` text
Dialogue displayed
    ↓
1500 milliseconds
    ↓
Dialogue automatically closes
    ↓
AfterActions
```

自動終了でも通常のDialogue終了処理が使用されるため、`AfterActions`
は通常どおり実行されます。

------------------------------------------------------------------------

### DurationとResponses

`Duration` は `Responses` を持たない通常のDialogue向けです。

DialogueにResponsesが含まれている場合、`Duration` は無視されます。

これにより、プレイヤーがResponseを選んでいる最中にQuestion
Dialogueが自動的に閉じることを防ぎます。

------------------------------------------------------------------------

## Multi-page Dialogue

通常のText
Dialogueでは、次の記号でTextを区切ることで複数Pageを含められます:

``` text
#
```

例:

``` json
{
  "Text": "This is page 1.#This is page 2.#This is page 3."
}
```

Pageは順番に表示されます。

``` text
Page 1
    ↓
Page 2
    ↓
Page 3
```

`AfterActions`
が指定されている場合、Dialogue全体を閉じた後に実行されます。

> **注意:** この `#` Page Separatorは通常のText
> Dialogueに適用されます。`Responses` を持つQuestion Dialogueでは `Text`
> を質問Promptとして使用し、同じMulti-page動作は使用しません。

------------------------------------------------------------------------

<a id="complete-example"></a>

## 完全な使用例

次の例では、Dialogue Systemの主な機能をまとめて使用しています。

この例では、次の機能を持つ特別な釣りエリアを作成します:

-   10:00～20:00の間だけ利用できます。
-   2000Gの料金がかかります。
-   プレイヤーの現在の所持金を確認します。
-   条件を満たしていない場合にFailure Messageを表示します。
-   Audio Cueを再生します。
-   Trigger Actionを実行します。
-   プレイヤーをワープさせます。
-   別のDialogueへ続きます。
-   到着時のMessageを自動的に閉じます。

### Dialogue Data

``` json
{
  "Action": "EditData",
  "Target": "TsCore/Dialogues",
  "Entries": {
    "YourName.MyMod_FishingArea": {
      "Text": "Would you like to use the special fishing area? The fee is 2000G.",
      "Condition": "TIME 1000 2000",
      "FailText": "The special fishing area is currently closed. (Open 10:00–20:00)",
      "AudioCue": "coin",
      "FailAudioCue": "cancel",
      "Responses": [
        {
          "Text": "Yes",
          "Condition": "PLAYER_CURRENT_MONEY Current 2000",
          "FailText": "You don't have enough money.",
          "FailAudioCue": "cancel",
          "Actions": [
            "AddMoney -2000",
            "TsCoreWarp BusStop 20 22"
          ],
          "Next": "YourName.MyMod_FishingAreaArrival"
        },
        {
          "Text": "No",
          "Next": "YourName.MyMod_FishingAreaCancelled"
        }
      ]
    },

    "YourName.MyMod_FishingAreaArrival": {
      "Text": "Welcome to the special fishing area!",
      "AudioCue": "coin",
      "Duration": 1500
    },

    "YourName.MyMod_FishingAreaCancelled": {
      "Text": "You decided not to use the special fishing area."
    }
  }
}
```

------------------------------------------------------------------------

### Map Tile Action

その後、MapからDialogueを開けます:

``` json
{
  "Action": "EditMap",
  "Target": "Maps/Farm",
  "MapTiles": [
    {
      "Position": {
        "X": 68,
        "Y": 9
      },
      "Layer": "Buildings",
      "SetProperties": {
        "Action": "TsCoreDialogue YourName.MyMod_FishingArea"
      }
    }
  ]
}
```

処理の流れ:

``` text
Player interacts with tile
    ↓
TIME condition
    ↓
┌─ Failed
│     ↓
│  FailAudioCue
│     ↓
│  FailText
│
└─ Passed
      ↓
   AudioCue
      ↓
   Question
      ↓
   Yes / No
```

プレイヤーが `Yes` を選択した場合:

``` text
Money condition
    ↓
┌─ Failed
│     ↓
│  Response FailAudioCue
│     ↓
│  Response FailText
│
└─ Passed
      ↓
   AddMoney -2000
      ↓
   TsCoreWarp
      ↓
   Next
      ↓
   Arrival Dialogue
      ↓
   Duration
      ↓
   Auto-close
```

プレイヤーが `No` を選択した場合:

``` text
No
 ↓
Next
 ↓
Cancellation Dialogue
```

------------------------------------------------------------------------

<a id="debugging"></a>

## デバッグ

T's
Coreでは、登録されているDialogueの確認とテストを行うためのコマンドを提供しています。

<a id="listing-dialogues"></a>

### Dialogue一覧の確認

使用:

``` text
tscore_debug_dialogue
```

現在登録されているDialogue IDを次のData Assetから表示します:

``` text
TsCore/Dialogues
```

Content
Patcherから追加したDialogueが正しく登録されているか確認するために使用できます。

------------------------------------------------------------------------

<a id="testing-a-dialogue"></a>

### Dialogueのテスト

使用:

``` text
tscore_debug_dialogue <DialogueId>
```

例:

``` text
tscore_debug_dialogue YourName.MyMod_FishingArea
```

指定したDialogueを直接開きます。

次の機能を含む通常のDialogue動作が使用されます:

-   ConditionとConditions
-   FailTextとConditionごとの失敗時動作
-   HideDialogue
-   UsageLimit
-   Audio Cue
-   Responses
-   ResponseのConditionとConditions
-   Response Action
-   Next
-   AfterActions
-   RandomActions
-   Duration

MapのTile ActionやTouch
Actionを何度も発動させずにDialogueをテストする場合に便利です。

------------------------------------------------------------------------

<a id="reloading-content-patcher-content-packs"></a>

## Content Patcher Content Packの再読み込み

`TsCore/Dialogues` を通して登録したDialogueは、そのData
Assetを編集しているContent Patcher Content
Packを再読み込みすることで、ゲーム実行中に更新できます。

使用:

``` text
tscore_cp_reload <ContentPackId>
```

例:

``` text
tscore_cp_reload YourName.MyDialoguePack
```

開発中に次の変更を反映できます:

-   Dialogueの追加
-   Dialogueの削除
-   Dialogue Textの変更
-   ConditionまたはConditionsと、その失敗時動作の変更
-   HideDialogueの動作変更
-   UsageLimit設定の変更
-   Responsesの変更
-   Trigger Actionの変更
-   Next Dialogueの移動先変更
-   AfterActionsの変更
-   RandomActionsとWeightの変更
-   Durationの変更
-   Audio Cueの変更

Stardew Valleyを再起動する必要はありません。

Content Packを再読み込みすると、次にDialogueを開いたときから更新後の
`TsCore/Dialogues` Dataが使用されます。

T's CoreのContent Patcher Reload Systemでは、ConfigSchema、Config
Token、GMCM設定、DynamicTokensの再読み込みにも対応しています。

`tscore_cp_reload` の詳細については、[Content Patcher
Integration](ModderGuide_ContentPatcherIntegration.md)
ガイドを参照してください。

------------------------------------------------------------------------

<a id="notes"></a>

## 注意事項

Dialogueは次のData Assetを通して登録します:

``` text
TsCore/Dialogues
```

`Entries` で使用するDictionary KeyがDialogue IDです。

カスタムDialogueでは、全体で一意なIDを使用することを推奨します。

可能な場合は、自分のModのUniqueIDを基にしたIDを使用することを推奨します:

``` text
YourName.MyMod_DialogueName
```

Dialogueは次のActionから開始できます:

``` text
TsCoreDialogue <DialogueId>
```

Tile ActionまたはTouch Actionとして使用できます。

Dialogue Levelの `Condition` はMain Dialogueを表示する前に判定されます。

複数の条件に個別の失敗時動作が必要な場合、DialogueとResponseの両方で
`Conditions`
を使用できます。上から順番に判定され、最初の失敗で停止します。

Response Levelの `Condition`
は、そのResponseが選択された後にのみ判定されます。

Responseの主な処理順序:

``` text
Response selected
    ↓
Response Condition
    ↓
Response Actions
    ↓
Next Dialogue
```

`Actions` はResponseのConditionが成功した直後に実行されます。

`HideDialogue`
はそのDialogueが生成するMessageを非表示にしますが、Condition、Audio
Cue、Actionは無効にしません。

`UsageLimit` は現在 `Scope: Building` と `Period: Day`
に対応しており、個々のBuilding
Instanceごとにゲーム内1日につき1回成功できます。

`AfterActions` は異なり、Dialogue
Chain全体が完全に閉じた後に実行されます。

`Next` で接続された複数のDialogueが `AfterActions`
を登録した場合、それらのActionは蓄積され、最後のDialogueを閉じた後に登録順で実行されます。

`RandomActions` は重み付き候補を1つ選択し、その候補のTrigger
Actionを実行します。Buildingから開始されたDialogueでは、決定論的な日次Selectionに個別のBuildingも含まれます。

`Duration` はミリ秒単位で指定し、Responsesを持たない通常のText
Dialogueに適用されます。

`AudioCue` はMain
DialogueがConditionを正常に通過したときに再生されます。

`FailAudioCue` は次の失敗に対して個別に指定できます:

-   Dialogue LevelのCondition失敗
-   Response LevelのCondition失敗

通常のText Dialogueでは `#` を使用して複数Pageに分割できます。

開発中は次のコマンドを使用できます:

``` text
tscore_debug_dialogue
tscore_debug_dialogue <DialogueId>
tscore_cp_reload <ContentPackId>
```

ゲームを何度も再起動せずにDialogueの確認、テスト、再読み込みを行えます。

今後のT's Coreで、Dialogue Systemの機能が追加される場合があります。

------------------------------------------------------------------------

## Modder Guide

-   ← [BigCraftable Extension](ModderGuide_BigCraftableExtension.md)
-   ↑ [Guide Index](#top)
-   → [Migration System](ModderGuide_MigrationSystem.md)

← [READMEに戻る](../../README.md)

← [Modder Guideに戻る](ModderGuide.md)
