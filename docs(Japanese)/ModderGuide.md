# T's Core Modder Guide

T's CoreのMod開発者向けドキュメントへようこそ。

T's Coreは、Stardew ValleyのSMAPI
Mod向けの共通ライブラリ兼フレームワークです。再利用可能なAPI、Content
Patcher連携、カスタムAction、データ駆動型システム、移行機能、開発用ツールなどを提供し、Mod開発を簡略化するとともにMod間の互換性向上を支援します。

------------------------------------------------------------------------

## 📚 目次

-   [はじめに](#-はじめに)
-   [C#からT's Coreを使用する](#-cからts-coreを使用する)
-   [利用可能なシステム](#-利用可能なシステム)
-   [Content PatcherからT's
    Coreを使用する](#-content-patcherからts-coreを使用する)
-   [デバッグコマンド](#-デバッグコマンド)
-   [APIの安定性](#-apiの安定性)
-   [詳細ドキュメント](#-詳細ドキュメント)

← [READMEに戻る](../../README.md)

------------------------------------------------------------------------

# 🚀 はじめに

## T's Coreの導入

`manifest.json` にT's Coreを依存Modとして追加します。

``` json
"Dependencies": [
  {
    "UniqueID": "Tikamin557.TsCore",
    "IsRequired": true
  }
]
```

T's Coreがなくても動作できるModの場合は、代わりに次のように設定します。

``` json
"IsRequired": false
```

------------------------------------------------------------------------

# 💻 C#からT's Coreを使用する

SMAPI C# Modでは、ドキュメントに記載された公開APIを通してT's
Coreを利用できます。

また、C# ModからT's CoreのShortcut
Panelへ独自のActionを登録し、プレイヤーがゲーム内パネルからその機能を直接利用できるようにすることもできます。

> **重要**
>
> 内部クラス、内部フィールド、ドキュメントに記載されていない動作には依存しないでください。
> 内部実装は予告なく変更される場合があります。

------------------------------------------------------------------------

# 🧩 利用可能なシステム

現在、T's Coreでは以下のシステムを提供しています。

  ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
  System                                     説明
  ------------------------------------------ ---------------------------------------------------------------------------------------------------------------------------------------------------------------------
  Relationship Services                      対応している結婚Modのサポートを含む、関係性やパートナーに関する情報を提供します。

  Location Services                          プレイヤーの現在地に関する情報や関連機能を提供します。

  Warp Services                              Warp Actionと再利用可能なWarp Providerを提供します。

  Map Properties                             Locationに追加の動作を設定するためのカスタムMap Propertyを追加します。

  Building Services                          Buildingにカスタム設定や表示機能を追加します。

  BigCraftable Extension                     `TsCore/BigCraftableExtension` を通して登録したBigCraftable
                                             Extensionにより、BigCraftableへ設定可能なインタラクション、当たり判定サイズ、設置条件、テクスチャ、アニメーション、エフェクト、ライトなどの任意機能を追加できます。

  Dialogue System                            条件、選択肢、Action、Dialogueの連結などの任意機能に対応した、データ駆動型のカスタムDialogueを追加します。

  Migration System                           既存のセーブデータに保存されているIDを移行するための機能を提供します。

  Notification System                        カスタマイズ可能な画面上のNotificationとNotification Themeを提供します。

  Content Patcher Integration                Content Patcherでの開発や設定に関する追加機能を提供します。

  Position Picker                            Content PackのGMCMからMap上でX/Y座標を直接選択できる機能を提供します。

  Post Renovation Patch                      FarmHouseのRenovationやその他の実行時のFarmHouseレイアウト変更後に、データ駆動型のMap Patchを適用します。

  Polyamory Sweet Rooms Integration          対応するFarmhouse Modから、Polyamory Sweet Rooms用の設定可能なSpouse Room Presetを提供できるようにします。

  Other Features                             カスタム睡眠ActionやLocationカテゴリ判定など、追加のTile ActionとGame State Queryを提供します。

  Content Patcher Tokens                     Content Patcherで使用できるカスタムTokenを提供します。

  Shortcut Panel                             カスタマイズ可能なゲーム内Shortcut Panelと、C# Modから独自のShortcut Actionを登録できる公開APIを提供します。

  Shared Utilities                           T's Mods間で共有して使用する共通機能を提供します。
  ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

各機能の個別ガイドについては、[詳細ドキュメント](#-詳細ドキュメント)を参照してください。

------------------------------------------------------------------------

# 📦 Content PatcherからT's Coreを使用する

T's Coreの多くの機能は、C#コードを書かずにContent
Patcherから直接利用できます。

Content PatcherのContent Packでは、T's CoreのカスタムAction、Game State
Query、Map Property、各種連携機能、カスタムData Assetを使用できます。

以下のカスタム定義を登録できます。

-   Building Provider
-   Warp Provider
-   Notification Theme
-   Migration定義
-   BigCraftable Extension
-   Dialogue
-   Post Renovation Patch
-   Polyamory Sweet RoomsのSpouse Room Preset
-   Position Picker定義
-   Polyamory Sweet Rooms設定画面を開くGMCMボタン

`TsCore/BigCraftableExtension` を通して登録したBigCraftable
Extensionでは、BigCraftableへ設定可能なインタラクション、当たり判定サイズ、テクスチャ、アニメーション、エフェクト、ライトなどの任意機能を追加できます。

`TsCore/Dialogues` を通して登録したDialogueは、`TsCoreDialogue`
を使用してMapのTile ActionやTouch Actionから表示できます。

T's Coreでは、ゲームプレイ上の追加動作や条件に使用できるカスタムTile
ActionとGame State
Queryも提供しています。これらの比較的小規模な機能については、**[Other
Features Guide](ModderGuide_OtherFeatures.md)** を参照してください。

さらに、カスタムToken、開発時に使用できる拡張Content
Packリロード機能、導入済みModに応じてGMCM項目の表示を切り替える機能、Map上でX/Y座標を直接選択できるPosition Pickerなど、Content
Patcher向けの追加機能も提供しています。

各システムには、それぞれ設定方法と利用可能なオプションがあります。詳細や使用例については、[詳細ドキュメント](#-詳細ドキュメント)内の対応するガイドを参照してください。

------------------------------------------------------------------------

# 🛠 デバッグコマンド

T's
Coreでは、開発中に各システムの状態確認やテストを行うためのコマンドを提供しています。

## Tokenコマンド

  Command                        説明
  ------------------------------ -----------------------------------------
  `tscore_tokens`                利用可能なすべてのToken値を表示します。
  `tscore_tokens_relationship`   Relationship関連のTokenを表示します。
  `tscore_tokens_location`       Location関連のTokenを表示します。

## デバッグコマンド

  -----------------------------------------------------------------------------------------------------
  Command                                    説明
  ------------------------------------------ ----------------------------------------------------------
  `tscore_debug_warp`                        登録されているすべてのWarp Providerを表示します。

  `tscore_debug_buildings`                   登録されているすべてのBuilding Providerを表示します。

  `tscore_debug_buildings <ID>`              指定したBuilding Providerの詳細情報を表示します。

  `tscore_debug_farmbuildings`               現在メインファームに設置されているBuildingを表示します。

  `tscore_debug_dialogue`                    登録されているすべてのDialogueを表示します。

  `tscore_debug_dialogue <ID>`               テスト用に指定したDialogueを表示します。

  `tscore_debug_notification_themes`         利用可能なすべてのNotification Themeを表示します。

  `tscore_debug_notification <ID>`           テスト用のNotificationを表示します。

  `tscore_debug_notification_trigger`        NotificationのTrigger Actionをテストします。
  -----------------------------------------------------------------------------------------------------

## Content Patcher Reload

``` text
tscore_cp_reload <ContentPackId>
```

この開発用コマンドは、ゲームを再起動せずに指定したContent Patcher
Content Packを再読み込みし、対応しているContent
Patcher関連データを更新します。

T's CoreのカスタムData Asset、ConfigSchema、Config
Token、GMCM設定、DynamicTokensの変更にも対応しています。

詳細については、**[Content Patcher Integration
Guide](ModderGuide_ContentPatcherIntegration.md)** を参照してください。

------------------------------------------------------------------------

# 🔒 APIの安定性

T's Coreは現在も継続して開発されています。

公開APIおよびドキュメントに記載された機能については、可能な限り互換性を維持する方針ですが、今後新しい機能や任意プロパティが追加される場合があります。

T's Coreを利用してModを開発する場合は、以下を推奨します。

-   ドキュメントに記載された公開APIを使用してください。
-   内部実装への依存は避けてください。
-   必須とするT's Coreのバージョンを適切に更新してください。
-   T's Coreを更新した後は、自分のModの動作をテストしてください。

------------------------------------------------------------------------

# 📖 詳細ドキュメント

詳しい設定方法、プロパティ、使用例、利用方法については、各ガイドを参照してください。

-   [Relationship Services](ModderGuide_RelationshipServices.md)
-   [Location Services](ModderGuide_LocationServices.md)
-   [Warp Services](ModderGuide_WarpServices.md)
-   [Map Properties](ModderGuide_MapProperties.md)
-   [Building Services](ModderGuide_BuildingServices.md)
-   [BigCraftable Extension](ModderGuide_BigCraftableExtension.md)
-   [Dialogue System](ModderGuide_DialogueSystem.md)
-   [Migration System](ModderGuide_MigrationSystem.md)
-   [Notification System](ModderGuide_NotificationSystem.md)
-   [Content Patcher
    Integration](ModderGuide_ContentPatcherIntegration.md)
-   [Position Picker](ModderGuide_PositionPicker.md)
-   [Post Renovation Patch](ModderGuide_PostRenovationPatch.md)
-   [Polyamory Sweet Rooms
    Integration](ModderGuide_PolyamorySweetRoomsIntegration.md)
-   [Other Features](ModderGuide_OtherFeatures.md)

← [READMEに戻る](../../README.md)


ショートカットパネルの自由配置・CJB Cheats Menu連携については、[ショートカットパネルガイド](ModderGuide_ShortcutPanel.md)を参照してください。
