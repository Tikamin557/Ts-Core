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
-   ✅ **Content Patcher Integration** *(現在のページ)*
-   📄 [Other Features](ModderGuide_OtherFeatures.md)

← [READMEに戻る](../../README.md)

← [Modder Guideに戻る](ModderGuide.md)

------------------------------------------------------------------------

# Content Patcher Integration

Content Patcher Integrationは、Content Patcher Content Pack向けの追加機能と開発ツールを提供します。

T's Coreでは、Gameを再起動せずにContent Packの `content.json` と、それに関連するConfigSchema、Config Token、GMCM設定、Dynamic Token、Patchをまとめて再読み込みできます。

またT's CoreはContent Patcherの `ConfigSchema` を拡張し、導入されているModに応じてGMCM Fieldの表示を制御する任意Propertyを追加します。

------------------------------------------------------------------------

<a id="contents"></a>

## 目次

-   [Reloading a Content Patcher Content Pack](#reloading-a-content-patcher-content-pack)
-   [What Is Reloaded](#what-is-reloaded)
-   [ConfigSchema](#configschema)
-   [Config Tokens](#config-tokens)
-   [Generic Mod Config Menu](#generic-mod-config-menu)
-   [Dynamic Tokens](#dynamic-tokens)
-   [Content Patcher Patches](#content-patcher-patches)
-   [Disabled Patch Records](#disabled-patch-records)
-   [T's Core Custom Data Assets](#ts-core-custom-data-assets)
-   [Development Workflow](#development-workflow)
-   [Limitations](#limitations)
-   [Notes](#notes)

------------------------------------------------------------------------

<a id="reloading-a-content-patcher-content-pack"></a>

## Content Patcher Content Packの再読み込み

使用:

``` text
tscore_cp_reload <ContentPackId>
```

`<ContentPackId>` には、再読み込みしたいContent Patcher Content Packの `UniqueID` を指定します。

例:

``` text
tscore_cp_reload YourName.MyContentPack
```

T's CoreはContent Patcherが現在読み込んでいるContent Packを検索し、指定した `UniqueID` のContent Packを再読み込みします。

Content Packが見つからない場合、T's Coreは警告をログへ出力し、再読み込みを実行しません。

> **注意:** このコマンドはContent Patcher Content Pack向けです。任意のSMAPI Modを再読み込みするものではありません。

------------------------------------------------------------------------

<a id="what-is-reloaded"></a>

## 再読み込みされる内容

`tscore_cp_reload` は通常のContent Patcher Patch再読み込みより多くの処理を行います。

T's Coreは次のDataを再読み込みまたは再構築します:

  Data                                         再読み込み
  ----------------------------------- -----------------------------
  `content.json`                                   ✅
  Content Patcher patches                          ✅
  `ConfigSchema`                                   ✅
  `config.json`                             ✅ 再構築して保存
  Config Tokens                                    ✅
  GMCM settings                                    ✅
  `DynamicTokens`                                  ✅
  Dynamic Token conditions                         ✅
  Dynamic Token dependencies                       ✅
  古い永続的なDisabled Patch Record    ✅ 対象Content Pack分を消去

基本的な再読み込み処理の流れ:

``` text
tscore_cp_reload <ContentPackId>
        ↓
Reload content.json
        ↓
Rebuild Config from ConfigSchema
        ↓
Save config.json
        ↓
Rebuild Config Tokens
        ↓
Re-register GMCM settings
        ↓
Rebuild Dynamic Tokens
        ↓
Clear old disabled patch records
        ↓
Reload Content Patcher patches
```

これにより、Stardew Valleyを再起動せずに一般的なContent Pack変更の多くをテストできます。

Content PackがT's CoreのカスタムData Assetへ行った変更も、Content Patcher Patchの再読み込みを通して更新されます。

------------------------------------------------------------------------

## ConfigSchema

Content Packを再読み込みすると、T's Coreは再読み込み後の `content.json` から最新の `ConfigSchema` を読み取ります。

その後、Content Patcherの通常のConfig処理を使用してContent PackのConfigを再構築します。

これにより、次のような変更を:

-   Config Fieldの追加
-   Config Fieldの削除
-   Config Field名の変更
-   Config Field定義の変更

Gameを再起動せず、開発中に反映できます。

たとえば、元のSchemaが次の内容で:

``` json
"ConfigSchema": {
  "OldSetting": {
    "AllowValues": "A, B, C",
    "Default": "A"
  }
}
```

次のように変更した場合:

``` json
"ConfigSchema": {
  "NewSetting": {
    "AllowValues": "A, B, C, D",
    "Default": "B"
  }
}
```

次を実行すると:

``` text
tscore_cp_reload YourName.MyContentPack
```

T's Coreが更新後のSchemaを使用してContent PackのConfigを再構築します。

再構築したConfigはContent Packの `config.json` へ保存されます。

> **注意:** 既存値の維持またはDefault値への設定は、Content Patcherの通常のConfig処理に従います。

------------------------------------------------------------------------

<a id="config-tokens"></a>

## Config Token

Configを再構築した後、T's CoreはContent PackのConfig Tokenも再構築します。

以前のConfigSchemaに関連付けられた古いConfig Tokenを削除してから、現在のConfig Fieldを再登録します。

これはConfig Fieldを追加、削除、または名前変更した場合に重要です。

たとえば次の内容を:

``` json
"ConfigSchema": {
  "OldSetting": {
    "Default": true
  }
}
```

次のように変更すると:

``` json
"ConfigSchema": {
  "NewSetting": {
    "Default": true
  }
}
```

Content Pack再読み込み時に古い `OldSetting` Config Tokenが削除され、新しい `NewSetting` Config Tokenが登録されます。

その後Content PatcherのToken Contextが更新され、再構築したConfig TokenをPatchやDynamic Tokenから使用できるようになります。

------------------------------------------------------------------------

## Generic Mod Config Menu

Content PackがConfigSchemaをGeneric Mod Config Menuと共に使用している場合、T's CoreはConfig再構築後にGMCM設定を再登録します。

これにより、Gameを再起動せずにConfigSchemaへの変更をGMCMへ反映できます。

例:

1.  ConfigSchema Fieldを追加または変更します。
2.  `content.json` を保存します。
3.  次を実行します:

``` text
tscore_cp_reload YourName.MyContentPack
```

その後、更新された設定を新しいConfig構造でGMCMへ登録できます。

> **注意:** Content PatcherのGMCM連携を使用するには、Generic Mod Config Menuが利用可能な状態である必要があります。

<a id="conditional-gmcm-visibility"></a>

### GMCMの条件付き表示

T's CoreはContent Patcherの `ConfigSchema` を拡張し、現在読み込まれているModに応じて個々のConfig FieldをGeneric Mod Config Menuに表示するか制御する任意Propertyを追加します。

次のPropertyに対応しています:

  -----------------------------------------------------------------------------------------------------------------
  Property                            動作
  ----------------------------------- -----------------------------------------------------------------------------
  `TsCore.ShowIfMod`                  指定したModの **少なくとも1つ** が読み込まれている場合にFieldを表示します。

  `TsCore.ShowIfAllMods`              指定したModが **すべて** 読み込まれている場合にのみFieldを表示します。
  -----------------------------------------------------------------------------------------------------------------

これらのPropertyが制御するのは、Config FieldをGMCMに表示するかどうかだけです。

Config Field自体を削除するものでは **ありません**。非表示のFieldも `config.json` に残って値を保持し、Content Patcherから通常どおり使用できます。

#### TsCore.ShowIfMod

指定したModのうち1つ以上が読み込まれている場合だけConfig Fieldを表示したいときは、`TsCore.ShowIfMod` を使用します。

例:

``` json
"ConfigSchema": {
  "CompatibilityOption": {
    "AllowValues": "true, false",
    "Default": "true",
    "TsCore.ShowIfMod": "Example.AuthorMod"
  }
}
```

`CompatibilityOption` は `Example.AuthorMod` が読み込まれている場合だけGMCMに表示されます。

複数のMod IDはカンマ区切りのListとして指定できます:

``` json
"TsCore.ShowIfMod": "Example.ModA, Example.ModB"
```

複数IDでは **OR Logic** を使用します。

この例では、`Example.ModA` **または** `Example.ModB` のどちらかが読み込まれている場合にFieldが表示されます。

概念的には:

``` text
Example.ModA OR Example.ModB
```

#### TsCore.ShowIfAllMods

指定したModがすべて読み込まれている場合だけConfig Fieldを表示したいときは、`TsCore.ShowIfAllMods` を使用します。

例:

``` json
"ConfigSchema": {
  "CompatibilityOption": {
    "AllowValues": "true, false",
    "Default": "true",
    "TsCore.ShowIfAllMods": "Example.ModA, Example.ModB"
  }
}
```

両方のModが読み込まれている場合にのみFieldが表示されます。

複数IDでは **AND Logic** を使用します。

概念的には:

``` text
Example.ModA AND Example.ModB
```

どちらかのModが存在しない場合、FieldはGMCMで非表示になります。

<a id="combining-both-conditions"></a>

#### 両方のConditionを組み合わせる

`TsCore.ShowIfMod` と `TsCore.ShowIfAllMods` は、同じConfigSchema Fieldで併用できます。

例:

``` json
"ConfigSchema": {
  "CompatibilityOption": {
    "AllowValues": "true, false",
    "Default": "true",
    "TsCore.ShowIfMod": "Example.ModA, Example.ModB",
    "TsCore.ShowIfAllMods": "Example.FrameworkA, Example.FrameworkB"
  }
}
```

両方のPropertyを指定した場合、**両方のConditionを満たす必要があります**。

上の例は次の条件と同等です:

``` text
(Example.ModA OR Example.ModB)
AND
(Example.FrameworkA AND Example.FrameworkB)
```

<a id="reloading-visibility-conditions"></a>

#### 表示Conditionの再読み込み

次を使用すると、表示Conditionが再評価されます:

``` text
tscore_cp_reload <ContentPackId>
```

これにより、開発中にPropertyを追加、削除、変更して、Gameを再起動せずにテストできます。

#### 注意事項

-   Mod IDはSMAPIが現在読み込んでいるModに対して確認されます。
-   複数のMod IDはカンマで区切る必要があります。
-   各Mod ID前後の空白は無視されます。
-   `TsCore.ShowIfMod` は複数のMod IDを指定した場合にOR Logicを使用します。
-   `TsCore.ShowIfAllMods` は複数のMod IDを指定した場合にAND Logicを使用します。
-   両方のPropertyを同じFieldで使用する場合、両方のConditionを満たす必要があります。
-   これらのPropertyが影響するのはGMCMでの表示だけです。
-   非表示のConfig FieldもContent PatcherのConfig Token、Dynamic Token、Patchから引き続き使用できます。

------------------------------------------------------------------------

<a id="dynamic-tokens"></a>

## Dynamic Token

T's Coreは最新の `content.json` からContent Packの `DynamicTokens` を完全に再構築できます。

例:

``` json
"DynamicTokens": [
  {
    "Name": "MyToken",
    "Value": "{{MyConfig}}"
  }
]
```

Dynamic Tokenを編集した後、次を実行します:

``` text
tscore_cp_reload YourName.MyContentPack
```

T's Coreは以前のDynamic Token Stateを削除し、現在の定義を使用して再構築します。

<a id="supported-changes"></a>

### 対応している変更

再読み込み処理では、次のような変更に対応しています:

-   Dynamic Tokenの追加または削除
-   `Value` の変更
-   `When` Conditionの変更
-   Dynamic Token間のDependency変更
-   Config Tokenへの参照変更

現在のDynamic Tokenを登録する前に、T's CoreはそのContent Packに関連する以前のDynamic Token Stateを消去します。

その後、現在のDynamic Tokenを再度Parseして登録し、Token Contextを更新します。

これにより、Gameを再起動せずに開発中のDynamic TokenとそのDependencyを変更できます。

------------------------------------------------------------------------

<a id="content-patcher-patches"></a>

## Content Patcher Patch

Content PackのDataを再構築した後、T's CoreはContent Patcherに対象Content PackのPatchを再読み込みさせます。

例:

``` text
tscore_cp_reload YourName.MyContentPack
```

は次のContent Packに属するPatchを再読み込みします:

``` text
YourName.MyContentPack
```

ConfigSchema、Config Token、GMCM設定、Dynamic Tokenを更新した後に実行されます。

この順序により、再読み込みしたPatchは新しく再構築されたConfigとToken Stateを使用できます。

------------------------------------------------------------------------

<a id="disabled-patch-records"></a>

## Disabled Patch Record

Content PatcherはPatchを正しく読み込めない場合、そのPatchを永続的にDisableすることがあります。

開発中、一時的にPatchへ不正なDataが含まれた場合などに発生することがあります。

対象Content Packを再読み込みする前に、T's CoreはそのContent Packに属する古い永続的なDisabled Patch Recordを削除します。

他のContent Packに属するRecordには影響しません。

これにより、修正したPatchを再読み込み処理の一部として再評価できます。

------------------------------------------------------------------------

<a id="ts-core-custom-data-assets"></a>

## T's CoreのカスタムData Asset

T's Coreの複数のSystemでは、通常のContent Patcher `EditData` Patchから編集できるカスタムData Assetを使用しています。

対象には次のData Assetがあります:

``` text
TsCore/BuildingProviders
TsCore/WarpProviders
TsCore/NotificationThemes
TsCore/Migrations
TsCore/MachineInteraction
TsCore/PostRenovationPatches
TsCore/PsrRoomPresets
```

これらのAssetを編集するContent Patcher Content Packを開発する場合は、変更後に次を使用します:

``` text
tscore_cp_reload <ContentPackId>
```

対象Content PackのPatchが再読み込みされるため、T's Core専用の別Resource Reload Commandを使用せずに、これらのData Assetへの変更を更新できます。

各Data Assetの詳細については、対応するSystem Guideを参照してください。

------------------------------------------------------------------------

<a id="development-workflow"></a>

## 開発手順

一般的な開発手順では `tscore_cp_reload` を使用することで、多くの一般的なContent Pack変更のたびにGameを再起動する必要をなくせます。

例:

1.  Stardew Valleyを起動してSaveを読み込みます。
2.  Content PackのFileを編集します。
3.  変更を保存します。
4.  SMAPI Consoleで次を実行します:

``` text
tscore_cp_reload YourName.MyContentPack
```

5.  Gameへ戻って変更をテストします。

Content Packの開発中はこの手順を繰り返せます。

次の項目を編集する場合に便利です:

-   Content Patcher Patch
-   ConfigSchema
-   Config Token
-   GMCM Option
-   Dynamic Token
-   T's CoreのカスタムData Asset

------------------------------------------------------------------------

<a id="limitations"></a>

## 制限事項

Content Patcher Integrationは **開発を便利にするための機能** です。

Release用Content Packをテストする際に、Gameの再起動を完全に置き換えるものとして扱うべきではありません。

Game StateやPatchによって行われた変更の一部は、そのPatchを再読み込みするだけでは完全に元へ戻せない場合があります。

最終テストでは、Stardew Valleyを再起動し、CleanなGame SessionからContent Packをテストすることを引き続き推奨します。

<a id="content-patcher-internal-implementation"></a>

### Content Patcherの内部実装

拡張再読み込み機能では、通常Public APIから公開されていないDataを再構築するため、Content Patcherの内部Runtime実装を使用しています。

これには次の内部Systemが含まれます:

-   読み込み済みContent Pack
-   Config処理
-   Config Token
-   GMCM登録
-   Dynamic Token
-   Token Context
-   Patch管理

これらはContent Patcherの内部Systemであるため、Content Patcher本体の変更に応じてT's Core側の更新が必要になる場合があります。

> **重要:** 将来のContent Patcher Versionとの互換性は、対応するT's Core Versionでテストされるまで保証されません。

<a id="reload-errors"></a>

### 再読み込みError

再読み込み処理の一部を完了できない場合、T's CoreはErrorをSMAPI Consoleへ出力します。

たとえば、次の場合に再読み込みが失敗することがあります:

-   指定したContent Pack IDが存在しない
-   更新後の `content.json` を読み込めない
-   Dynamic Tokenに不正なDataが含まれている
-   Content Patcherの内部構造が互換性のない形で変更されている

再読み込みが想定どおり動作しない場合は、SMAPI Consoleを確認してください。

------------------------------------------------------------------------

<a id="notes"></a>

## 注意事項

Content Patcher Integrationは主にContent Packの開発手順を改善するために設計されています。

対応している開発中の変更では、Content Pack Fileを保存した後に:

``` text
tscore_cp_reload <ContentPackId>
```

を使用してください。

このコマンドは、対象Content Packの `content.json` の再読み込み、ConfigSchema関連Stateの再構築、Config Tokenの再構築、GMCM設定の再登録、Dynamic TokenとそのDependencyの再構築、古いDisabled Patch Recordの消去、Content Patcher Patchの再読み込みを行えます。

T's CoreのカスタムData Assetを編集するContent Packでも、同じ `tscore_cp_reload` コマンドを使用できます。別の `tscore_reload` コマンドは不要になりました。

`tscore_cp_reload` によって開発中に必要なGame再起動回数を大幅に減らせますが、最終的な互換性テストとRelease Testでは完全な再起動を引き続き推奨します。

------------------------------------------------------------------------

## Modder Guide

-   ← [Notification System](ModderGuide_NotificationSystem.md)
-   ↑ [Guide Index](#top)
-   → [Post Renovation Patch](ModderGuide_PostRenovationPatch.md)

← [READMEに戻る](../../README.md)

← [Modder Guideに戻る](ModderGuide.md)
