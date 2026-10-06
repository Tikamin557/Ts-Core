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
-   📄 [Position Picker](ModderGuide_PositionPicker.md)
-   📄 [Post Renovation Patch](ModderGuide_PostRenovationPatch.md)
-   ✅ **Polyamory Sweet Rooms Integration** *(現在のページ)*
-   📄 [Other Features](ModderGuide_OtherFeatures.md)

← [READMEに戻る](../../README.md)

← [Modder Guideに戻る](ModderGuide.md)

------------------------------------------------------------------------

# Polyamory Sweet Rooms Integration

T's Coreでは、対応するFarmhouse Modから **Polyamory Sweet Rooms (PSR)** 用のRoom Presetを提供できます。

Farmhouse Content Packでは、提供するSpouse Room Slotを次のData Assetで定義できます:

``` text
TsCore/PsrRoomPresets
```

プレイヤーはT's Coreの **Spouse Room Setup** 画面を使用して、それらのSlotへキャラクターを割り当てられます。

Farmhouse Mod側に独自のC#コードは必要ありません。

------------------------------------------------------------------------

<a id="contents"></a>

## 目次

-   [Requirements](#requirements)
-   [Data Asset](#data-asset)
-   [Preset Properties](#preset-properties)
-   [Slot Properties](#slot-properties)
-   [Additional Candidates](#additional-candidates)
-   [Complete Example](#complete-example)
-   [How Assignments Are Saved](#how-assignments-are-saved)
-   [Player Access](#player-access)
-   [Content PackのGMCMへ設定ボタンを追加する](#adding-a-setup-button-to-a-content-pack-gmcm)
-   [Important Notes](#important-notes)

------------------------------------------------------------------------

<a id="requirements"></a>

## 必要条件

この連携機能には次のものが必要です:

-   T's Core
-   Polyamory Sweet Rooms
-   対応するPresetを登録するFarmhouse Mod
-   PresetでSMAPI Mod IDを指定したPolyamory Sweet Rooms Content Pack

Preset自体は通常、Farmhouse ModからContent Patcherを通して登録します。

------------------------------------------------------------------------

## Data Asset

次のData Assetを編集してPresetを登録します:

``` text
TsCore/PsrRoomPresets
```

各Entryが1つのRoom Presetを表します。

構造の例:

``` json
{
  "Action": "EditData",
  "Target": "TsCore/PsrRoomPresets",
  "Entries": {
    "{{ModId}}_Main": {
      "PsrContentPackId": "Example.Author.PSRContentPack",
      "Slots": [
        {
          "Id": "Room1",
          "DisplayName": "Room 1",
          "StartPosition": {
            "X": 10,
            "Y": 20
          },
          "SpousePositionOffset": {
            "X": 3,
            "Y": 3
          },
          "ShellType": "Normal"
        }
      ]
    }
  }
}
```

------------------------------------------------------------------------

<a id="preset-properties"></a>

## PresetのProperty

  ---------------------------------------------------------------------------------------------------------------------------------------------------------
  Property                 必須                  説明
  ------------------------ --------------------- ----------------------------------------------------------------------------------------------------------
  Entry key                ✅                    Presetの一意なID。

  `PsrContentPackId`       ✅                    T's Coreが `content.json` を書き込むPolyamory Sweet Rooms Content PackのSMAPI Mod ID。

  `AdditionalCandidates`   ❌                    通常のRelationship/Romanceability判定では検出できない場合でも、割り当て候補として利用可能にする追加NPC。

  `Slots`                  ✅                    T's Coreで表示・保存されるSpouse Room Slotの順序付きList。
  ---------------------------------------------------------------------------------------------------------------------------------------------------------

`Slots` の順序は、T's CoreがPolyamory Sweet RoomsのRoom Dataを書き込む際の順序としても使用されます。

------------------------------------------------------------------------

<a id="slot-properties"></a>

## SlotのProperty

`Slots` 内の各Entryが1つのSpouse Roomを定義します。

  --------------------------------------------------------------------------------------------------------------------------------------------------------
  Property                 必須                  説明
  ------------------------ --------------------- ---------------------------------------------------------------------------------------------------------
  `Id`                     ✅                    Slotの内部ID。Roomが未割り当ての場合はPSRの `name` としても使用されます。Preset内で一意にしてください。

  `DisplayName`            ❌                    T's CoreのSetup画面に表示するRoom名。空の場合は代わりに `Id` が表示されます。

  `HoverText`              ❌                    プレイヤーがRoom名へHoverしたときに表示する任意Text。

  `StartPosition`          ✅                    PSRへ `startPos` として書き込まれます。

  `SpousePositionOffset`   ❌                    PSRへ `spousePosOffset` として書き込まれます。Defaultは `3, 3`。

  `ShellType`              ✅                    PSRへ `shellType` として書き込まれます。
  --------------------------------------------------------------------------------------------------------------------------------------------------------

例:

``` json
{
  "Id": "UpperRoom",
  "DisplayName": "Upper Room",
  "HoverText": "The upper spouse room.",
  "StartPosition": {
    "X": 40,
    "Y": 8
  },
  "SpousePositionOffset": {
    "X": 3,
    "Y": 3
  },
  "ShellType": "Normal"
}
```

------------------------------------------------------------------------

## Additional Candidates

T's Coreは通常、Relationship ServiceとRomanceability Serviceから利用可能なキャラクターを取得して割り当て候補Listを作成します。

通常の判定では検出できない場合でも選択可能にしたいNPCには、`AdditionalCandidates` を使用できます。

各KeyにはNPCの内部名を指定し、`ModId` にはその候補を利用可能にするために読み込まれている必要があるModを指定します。

例:

``` json
"AdditionalCandidates": {
  "ExampleNPC": {
    "ModId": "Example.Author.NPCMod"
  }
}
```

プレイヤーの現在の進行状況ではNPCがまだ `Data/Characters` に存在していなくても、そのNPCをRoom割り当て候補として利用可能にしたい場合に便利です。

------------------------------------------------------------------------

<a id="complete-example"></a>

## 完全な例

``` json
{
  "Action": "EditData",
  "Target": "TsCore/PsrRoomPresets",
  "Entries": {
    "{{ModId}}_Main": {
      "PsrContentPackId": "Example.Author.PSRContentPack",
      "AdditionalCandidates": {
        "ExampleNPC": {
          "ModId": "Example.Author.NPCMod"
        }
      },
      "Slots": [
        {
          "Id": "UpperRoom",
          "DisplayName": "Upper Room",
          "HoverText": "The upper spouse room.",
          "StartPosition": {
            "X": 40,
            "Y": 8
          },
          "SpousePositionOffset": {
            "X": 3,
            "Y": 3
          },
          "ShellType": "Normal"
        },
        {
          "Id": "LowerRoom",
          "DisplayName": "Lower Room",
          "HoverText": "The lower spouse room.",
          "StartPosition": {
            "X": 40,
            "Y": 16
          },
          "SpousePositionOffset": {
            "X": 3,
            "Y": 3
          },
          "ShellType": "Normal"
        }
      ]
    }
  }
}
```

------------------------------------------------------------------------

<a id="how-assignments-are-saved"></a>

## 割り当ての保存方法

プレイヤーがSetupを確定すると、T's Coreは選択されたRoom割り当てを `PsrContentPackId` で指定されたContent Packの `content.json` へ書き込みます。

各Slotについて、T's Coreは次の内容を書き込みます:

-   選択したキャラクター名をPSRの `name` として書き込みます。
-   `StartPosition` を `startPos` として書き込みます。
-   `SpousePositionOffset` を `spousePosOffset` として書き込みます。
-   `ShellType` を `shellType` として書き込みます。

Slotを未割り当てのままにした場合、そのSlotの `Id` がPSRの `name` として書き込まれます。これにより、そのRoomをキャラクターへ割り当てずにRoom定義を維持できます。

生成されるRoomの順序はPreset内の `Slots` の順序に従います。

> **重要**
>
> Spouse Room Setup画面から保存すると、対象Polyamory Sweet Rooms Content Packの `content.json` が上書きされます。
>
> 保存したPolyamory Sweet Roomsの設定を反映するには、Gameを再起動する必要があります。

------------------------------------------------------------------------

<a id="player-access"></a>

## プレイヤーからのアクセス

必要なModと対応するPresetが利用可能な場合、プレイヤーはT's CoreからSpouse Room Setup画面を開けます。

次の場所から利用できます:

-   T's CoreのGeneric Mod Config Menu設定
-   Shortcut Panelに組み込まれているT's Core機能
-   `TsCore/PsrRoomPresetButtons` を登録した対応Content Pack自身のGMCM

Setup画面を使用するには、Saveを読み込んでいる必要があります。

------------------------------------------------------------------------

<a id="adding-a-setup-button-to-a-content-pack-gmcm"></a>

## Content PackのGMCMへ設定ボタンを追加する

Content PatcherのContent Packから、そのMod自身のGeneric Mod Config MenuへT's Coreの配偶者部屋設定画面を開くボタンを追加できます。C#コードは必要ありません。

次のData Assetへボタン定義を登録します。

``` text
TsCore/PsrRoomPresetButtons
```

例:

``` json
{
  "Action": "EditData",
  "Target": "TsCore/PsrRoomPresetButtons",
  "Entries": {
    "{{ModId}}_PsrRoomPresets": {
      "ContentPackId": "{{ModId}}",
      "GMCM_Name": "{{i18n:psr-preset.name}}",
      "GMCM_Description": "{{i18n:psr-preset.description}}",
      "GMCM_Button": "{{i18n:psr-preset.button}}",
      "GMCM_WorldRequired": "{{i18n:psr-preset.world-required}}",
      "GMCM_PsrRequired": "{{i18n:psr-preset.psr-required}}",
      "GMCM_NoPreset": "{{i18n:psr-preset.no-preset}}",
      "AfterField": "ExampleConfigField"
    }
  }
}
```

### ボタンのProperty

| Property | 必須 | 説明 |
|----------|------|------|
| Entry key | ✅ | ボタン定義の一意なIDです。 |
| `ContentPackId` | ✅ | ボタンを追加するContent PackのUniqueIDです。通常は `{{ModId}}` を推奨します。 |
| `GMCM_Name` | 任意 | GMCMに表示する項目名です。未指定時はT's Core標準の翻訳を使用します。 |
| `GMCM_Description` | 任意 | 通常時の説明・Hover Textです。未指定時はT's Core標準の翻訳を使用します。 |
| `GMCM_Button` | 任意 | ボタン内に表示する文字列です。未指定時はT's Core標準の翻訳を使用します。 |
| `GMCM_WorldRequired` | 任意 | セーブ未読込時にボタン上へ表示するHover Textです。 |
| `GMCM_PsrRequired` | 任意 | Polyamory Sweet Rooms未導入時にボタン上へ表示するHover Textです。 |
| `GMCM_NoPreset` | 任意 | 利用可能なRoom Presetが無い時にボタン上へ表示するHover Textです。 |
| `AfterField` | 任意 | このConfigSchema項目の直後へボタンを挿入します。対象が見つからない場合はGMCMの末尾へ追加されます。 |

設定画面を利用できない状態ではボタンは無効になりますが、無効状態でもボタンへマウスを乗せると利用できない理由が表示されます。

T's Core自身のGMCM、Shortcut Panel、Content Pack側のGMCMボタンのどこから開いても、同じ配偶者部屋設定画面を使用します。

------------------------------------------------------------------------

<a id="important-notes"></a>

## 重要な注意事項

-   `PsrContentPackId` は対象Polyamory Sweet Rooms Content PackのSMAPI Mod IDと一致する必要があります。
-   対象PSR Content Packが導入され、読み込まれている必要があります。
-   `Id` の値はPreset内で一意にしてください。
-   `Slots` の順序によって、PSR設定へ書き込まれる順序が決まります。
-   T's CoreのSetup画面では、1人のキャラクターを複数のSlotへ割り当てることはできません。
-   保存すると、対象PSR Content Packの `content.json` が上書きされます。
-   保存後、新しいRoom割り当てをテストする前にStardew Valleyを再起動してください。
-   `AdditionalCandidates` は通常の候補検出では不十分な場合にのみ使用してください。

------------------------------------------------------------------------

## Modder Guide

-   ← [Post Renovation Patch](ModderGuide_PostRenovationPatch.md)
-   ↑ [Guide Index](#top)
-   → [Other Features](ModderGuide_OtherFeatures.md)

← [READMEに戻る](../../README.md)

← [Modder Guideに戻る](ModderGuide.md)
