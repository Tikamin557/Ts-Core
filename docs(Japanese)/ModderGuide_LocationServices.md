# 📖 Modder Guide

このガイドでは、**T's Core** がContent
Patcher向けに提供している公開機能の使用方法を説明します。

<a id="top"></a>

## Guide Index

-   📄 [Relationship Services](ModderGuide_RelationshipServices.md)
-   ✅ **Location Services** *(現在のページ)*
-   📄 [Warp Services](ModderGuide_WarpServices.md)
-   📄 [Map Properties](ModderGuide_MapProperties.md)
-   📄 [Building Services](ModderGuide_BuildingServices.md)
-   📄 [BigCraftable Extension](ModderGuide_BigCraftableExtension.md)
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

# Location Services

Location Servicesは、プレイヤーの現在地や移動に関する**Content Patcher
Token**を提供します。

独自のC#コードを必要とせず、Locationに応じたContent
Packを作成する場合に役立ちます。

------------------------------------------------------------------------

## 目次

-   [利用可能なToken](#利用可能なtoken)
-   [利用可能なTokenの詳細](#利用可能なtokenの詳細)
-   [実装予定のToken](#実装予定のtoken)
-   [一般的な使用例](#一般的な使用例)
-   [デバッグ](#デバッグ)
-   [注意事項](#注意事項)

------------------------------------------------------------------------

## 利用可能なToken

すべてのLocation Services Tokenは、次の形式で使用します。

``` text
{{Tikamin557.TsCore/<TokenName>}}
```

  ------------------------------------------------------------------------------------------------------------------
  Token                 戻り値                                                 例                  状態
  --------------------- ------------------------------------------------------ ------------------- -----------------
  `LocationElapsed`     現在のLocationに入ってから経過したゲーム内時間（分）   `60`                ✅ 利用可能

  `PreviousLocation`    直前にいたLocation名                                   `Farm`              ✅ 利用可能

  `VisitCount`          現在のLocationへの累計訪問回数                         `7`                 🚧 実装予定

  `SessionVisitCount`   現在のゲームセッション中の訪問回数                     `3`                 🚧 実装予定

  `EnteredToday`        現在のLocationに今日すでに入ったことがあるか           `true`              🚧 実装予定

  `IsOutdoors`          現在のLocationが屋外か                                 `false`             ✅ 利用可能

  `IsIndoors`           現在のLocationが屋内か                                 `true`              ✅ 利用可能
  ------------------------------------------------------------------------------------------------------------------

> **注意:** 🚧 実装予定と記載されているTokenは、T's
> Coreの将来のバージョン向けに予約されています。現在の動作は確定していないため、公開するContent
> Packでは依存しないでください。

------------------------------------------------------------------------

## 利用可能なTokenの詳細

### LocationElapsed

プレイヤーが現在のLocationに入ってから経過したゲーム内時間を分単位で返します。

``` json
"When": {
    "Query: {{Tikamin557.TsCore/LocationElapsed}} >= 60": true
},
"Update": "OnTimeChange"
```

この条件は、プレイヤーが現在のLocationに**ゲーム内時間で60分以上**滞在するとtrueになります。

> **注意:** `LocationElapsed`
> が返すのは経過時間であり、現在の時刻ではありません。時間の経過に応じてPatchを反応させる必要がある場合は、`Update: OnTimeChange`
> を使用してください。

------------------------------------------------------------------------

### PreviousLocation

現在のLocationに入る直前にプレイヤーがいたLocationの内部名を返します。

``` json
"When": {
    "Tikamin557.TsCore/PreviousLocation": "Farm"
},
"Update": "OnLocationChange"
```

この条件は、プレイヤーが `Farm`
から現在のLocationへ移動してきた場合にtrueになります。

> **注意:**
> セーブデータを読み込んだ後、プレイヤーが最初にLocationを移動するまでは、このTokenが空の値を返す場合があります。

------------------------------------------------------------------------

### IsOutdoors / IsIndoors

これらのTokenは、現在のLocationが屋外または屋内として扱われているかを示します。

  Token          `true` を返す条件
  -------------- ----------------------------
  `IsOutdoors`   現在のLocationが屋外である
  `IsIndoors`    現在のLocationが屋内である

#### 屋外の例

``` json
"When": {
    "Tikamin557.TsCore/IsOutdoors": "true"
},
"Update": "OnLocationChange"
```

#### 屋内の例

``` json
"When": {
    "Tikamin557.TsCore/IsIndoors": "true"
},
"Update": "OnLocationChange"
```

通常、`IsIndoors` は `IsOutdoors` と反対の値になります。

> **注意:**
> 結果は、カスタムLocationを含め、そのLocationがどのように設定されているかによって決まります。

------------------------------------------------------------------------

## 実装予定のToken

次のTokenは登録されていますが、**まだ完全には実装されていません**。

  ------------------------------------------------------------------------------------------------------------------------
  Token                 想定されている用途                                                 推奨されるUpdate
  --------------------- ------------------------------------------------------------------ -------------------------------
  `VisitCount`          現在のLocationに入った回数を記録する                               `OnLocationChange`

  `SessionVisitCount`   現在のゲームセッション中の訪問回数を記録する                       `OnLocationChange`

  `EnteredToday`        現在のゲーム内日付で、そのLocationに入ったことがあるかを記録する   `OnLocationChange`
  ------------------------------------------------------------------------------------------------------------------------

以下の例は、将来想定されている使用方法を示しています。

### VisitCount

``` json
"When": {
    "Query: {{Tikamin557.TsCore/VisitCount}} >= 3": true
},
"Update": "OnLocationChange"
```

3回目の訪問以降にPatchを適用する用途を想定しています。

### SessionVisitCount

``` json
"When": {
    "Query: {{Tikamin557.TsCore/SessionVisitCount}} >= 2": true
},
"Update": "OnLocationChange"
```

現在のゲームセッション中に、そのLocationへ2回以上入った後でPatchを適用する用途を想定しています。

### EnteredToday

``` json
"When": {
    "Tikamin557.TsCore/EnteredToday": "true"
},
"Update": "OnLocationChange"
```

現在のゲーム内日付で、そのLocationにすでに入ったことがあるかを確認する用途を想定しています。

> **重要:**
> 実装予定のTokenは参考情報としてのみ掲載しています。利用可能として記載されるまでは、公開するContent
> Packで現在の動作に依存しないでください。

------------------------------------------------------------------------

## 一般的な使用例

Location Servicesは、次のような用途に利用できます。

-   LocationごとのMap編集
-   動的な装飾
-   屋内と屋外での表示の違い
-   Locationに応じたDialogue
-   Event固有のPatch
-   カスタムLocationへの対応
-   Locationに入ってからの経過時間に応じた動作

------------------------------------------------------------------------

### Content Patcher Content Packの再読み込み

T's Coreには、ゲームの実行中にContent Patcher Content
Packを再読み込みするための開発用ツールもあります。

ゲームを再起動せずに、Patch、ConfigSchema、Config
Token、GMCM設定、DynamicTokensを再読み込みできます。

`tscore_cp_reload` やその他のContent Patcher連携機能については、[Content
Patcher Integration](ModderGuide_ContentPatcherIntegration.md)
ガイドを参照してください。

------------------------------------------------------------------------

<a id="デバッグ"></a>

## デバッグ

Token関連の情報を確認するには、次のSMAPIコマンドを使用できます。

  Command                    表示内容
  -------------------------- -----------------------------------------
  `tscore_tokens`            T's Coreが提供するすべてのToken関連情報
  `tscore_tokens_location`   Location Servicesの情報

Location Servicesだけを確認したい場合は、`tscore_tokens_location`
を使用してください。

<details>
<summary>
出力例
</summary>
``` text
tscore_tokens_location
[T's Core] ===== Location =====
[T's Core]
[T's Core]     Current Location    : Farm
[T's Core]     Previous Location   : BusStop
[T's Core]     Location Elapsed    : 20
[T's Core]     Visit Count         : 3
[T's Core]     Session Visit Count : 3
[T's Core]     Entered Today       : True
[T's Core]     Is Outdoors         : True
[T's Core]     Is Indoors          : False
```

</details>
> **注意:**
> 実装予定のTokenもデバッグコマンドには表示されます。現在の値はテストのみを目的としており、機能が完全に実装されるまでは依存しないでください。

------------------------------------------------------------------------

## 注意事項

Location Servicesは**読み取り専用**です。

Content Patcher
Tokenを通してLocation関連の情報を公開するだけで、Location、Warp、プレイヤーの移動を変更することはありません。

Tokenによって、プレイヤーがLocationを移動したときに更新されるものと、ゲーム内時間の経過に応じて更新されるものがあります。Content
Patcherで使用する際は、適切な `Update` の値を指定してください。

------------------------------------------------------------------------

## Modder Guide

-   ← [Relationship Services](ModderGuide_RelationshipServices.md)
-   ↑ [Guide Index](#top)
-   → [Warp Services](ModderGuide_WarpServices.md)

← [READMEに戻る](../../README.md)

← [Modder Guideに戻る](ModderGuide.md)
