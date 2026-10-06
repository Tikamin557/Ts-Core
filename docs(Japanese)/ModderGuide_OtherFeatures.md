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
-   ✅ **Other Features** *(現在のページ)*

← [READMEに戻る](../../README.md)

← [Modder Guideに戻る](ModderGuide.md)

------------------------------------------------------------------------

# Other Features

このページでは、現在は個別のガイドを必要としないT's Coreの追加機能について説明します。

これらの機能は、独自のC#コードを必要とせずContent Patcher Packから使用できます。

------------------------------------------------------------------------

<a id="contents"></a>

## 目次

-   [Tile Actions](#tile-actions)
    -   [TsCore_Sleep](#tscore_sleep)
    -   [TsCore_Sleep Remove](#tscore_sleep-remove)
-   [Game State Queries](#game-state-queries)
    -   [TsCore_LOCATION_CATEGORY](#tscore_location_category)
-   [Compatibility and Game Fixes](#compatibility-and-game-fixes)
-   [Notes](#notes)

------------------------------------------------------------------------

<a id="tile-actions"></a>

# Tile Action

T's Coreでは、Stardew ValleyのTile Actionに対応している場所で使用できるカスタムTile Actionを提供しています。

T's Core BigCraftable Extensionの `TileAction` Propertyでも使用できます。

------------------------------------------------------------------------

## TsCore_Sleep

`TsCore_Sleep` を使用すると、対象のBig Craftableをプレイヤーの睡眠場所として使用できます。

BigCraftable Extensionでの例:

``` json
{
    "TileAction": "TsCore_Sleep"
}
```

プレイヤーがBig Craftableを操作すると、T's Coreは通常の睡眠判定を行い、睡眠確認Dialogueを表示します。

プレイヤーが睡眠を選択すると、現在のLocationがその夜の一時的な睡眠場所として使用されます。

さらに、T's Coreは後でPass Outした場合に使用する睡眠場所としてそのLocationを記憶します。

この睡眠場所はGameをSaveして再読み込みした場合も維持されます。

例:

``` text
Sleep using TsCore_Sleep
        ↓
Wake up at that location
        ↓
Later pass out outside at 2:00 AM
        ↓
Wake up at the previously used TsCore_Sleep location
```

保存される睡眠場所では、次の情報を使用してBig Craftableを識別します:

``` text
Location
    +
Anchor position
    +
Big Craftable type
```

Big CraftableのTypeはQualified Item IDによって識別されます。

Big Craftableが削除されたり別の位置へ移動された場合、保存されている位置は有効な睡眠場所とはみなされなくなります。

その後、同じTypeのBig Craftableを同じ位置へ再び設置すると、その睡眠場所は再び有効になります。

別のBig Craftableで `TsCore_Sleep` を使用すると、それまで記憶されていたT's Coreの睡眠場所が置き換えられます。

> **注意:** Locationを起床場所として使用できるかどうかを判定するStardew Valleyの通常の制限は引き続き適用されます。

------------------------------------------------------------------------

## TsCore_Sleep Remove

`TsCore_Sleep Remove` は `TsCore_Sleep` と同様に動作しますが、使い捨てまたは1回限りの睡眠Object向けです。

例:

``` json
{
    "TileAction": "TsCore_Sleep Remove"
}
```

プレイヤーはBig Craftableを操作し、そこで通常どおり睡眠できます。

睡眠後、そのActionに使用したBig Craftableは削除されます。

`TsCore_Sleep` とは異なり、このActionではBig Craftableを永続的な睡眠場所として保存しません。

また、`TsCore_Sleep Remove` を使用すると、それまで記憶されていたT's Coreの睡眠場所も消去されます。

次のようなObjectに便利です:

``` text
Single-use sleeping bags
Temporary camps
Disposable sleeping objects
```

------------------------------------------------------------------------

<a id="tscore_sleep-comparison"></a>

### TsCore_Sleepの比較

  -------------------------------------------------------------------------------------------
  Action                  対象Locationで睡眠     睡眠後にObjectを削除   睡眠場所を記憶
  ----------------------- ---------------------- ---------------------- ---------------------
  `TsCore_Sleep`          はい                   いいえ                 はい

  `TsCore_Sleep Remove`   はい                   はい                   いいえ
  -------------------------------------------------------------------------------------------

どちらのActionもStardew Valleyの通常の睡眠判定と睡眠確認Dialogueを使用します。

------------------------------------------------------------------------

<a id="game-state-queries"></a>

# Game State Query

T's Coreでは、対応するGame State Query Conditionを使用できる場所で利用可能なカスタムGame State Queryを提供しています。

BigCraftable Extensionの次のようなPropertyでも使用できます:

``` text
Condition
PlacementCondition
```

------------------------------------------------------------------------

## TsCore_LOCATION_CATEGORY

`TsCore_LOCATION_CATEGORY` は、LocationがT's CoreのLocation Categoryに属しているかを確認します。

形式:

``` text
TsCore_LOCATION_CATEGORY <Location> <Category>
```

Location引数には、Stardew Valley標準のGame State Query Location引数形式を使用します。

例:

``` text
TsCore_LOCATION_CATEGORY Target Dungeon
```

は、対象Locationが `Dungeon` Categoryに属しているかを確認します。

Categoryは複数指定できます:

``` text
TsCore_LOCATION_CATEGORY Target Category1 Category2
```

対象Locationが指定したCategoryのいずれかに一致するとQueryは成功します。

------------------------------------------------------------------------

### Dungeon

現在対応しているCategory:

``` text
Dungeon
```

`Dungeon` は、Stardew ValleyのMineおよびVolcano DungeonのLocation Typeで表されるDungeon形式のLocationを識別します。

これには次のようなLocationが含まれます:

``` text
The Mines
Skull Cavern
Quarry Mine
Volcano Dungeon
```

例:

``` text
TsCore_LOCATION_CATEGORY Target Dungeon
```

は、対象LocationがこれらのDungeon Locationのいずれかである場合にtrueを返します。

通常のGame State Query構文を使用してQueryを否定することもできます:

``` text
!TsCore_LOCATION_CATEGORY Target Dungeon
```

機能をDungeon Location以外でのみ利用可能にしたい場合に便利です。

たとえばBigCraftable Extensionでは、Dungeon内へのBig Craftable設置を禁止できます:

``` json
{
    "PlacementCondition": "!TsCore_LOCATION_CATEGORY Target Dungeon"
}
```

------------------------------------------------------------------------

<a id="compatibility-and-game-fixes"></a>

# Compatibility and Game Fixes

T's Coreには、T's Modsで使用するための内部的な互換性修正やゲーム動作の補完も含まれています。

これらは個別の公開APIを提供する機能ではなく、該当するFarmHouse、Map、天候、配偶者部屋の動作を必要に応じて自動的に補正します。

## FarmHouse Warp Fix

通常の玄関ではなくカスタムWarpからFarmHouseへ入った場合、Stardew ValleyがLocationの初期化時にプレイヤーをFarmHouseの玄関位置へ移動させることがあります。

T's Coreはこの場合に本来のカスタムWarp先を維持します。通常の玄関からのWarpやCellarからFarmHouseへのWarpでは、Stardew Valley標準の動作を維持します。

## Rain Totem Location Context Fix

Stardew ValleyのLocationでは、`RainTotemAffectsContext` を使用してレイントーテムの効果を別のLocation Contextへ向けることができます。

T's Coreは、Default以外のContextが指定された場合に、翌日の雨が対象Contextへ正しく反映されないことがある問題を補正します。

## Cellar Entrance Fix

FarmHouse Mapの再構築や再読み込み後に、Cellarへの入口やWarp状態が消える場合があります。

T's CoreはCellarが開放済みのFarmHouseで必要に応じてStardew Valley標準のCellarタイル、Cellar Warp、関連する床状態を再適用します。 v1.9.1ではFarmHouseのMap Assetが無効化された場合にも再適用を予約し、Content PatcherなどによるMap再読み込み後にFarmHouse Mapが再構築された場合でもCellar入口を復元できるようになりました。

## Spouse Room Tile Fix

Stardew ValleyのFarmHouse Renovation処理によって、配偶者部屋付近のカスタムFront LayerタイルがVanillaタイルで上書きされる場合があります。

T's Coreは、その位置がVanillaのタイル状態ではない場合に既存のカスタムタイルを維持できます。

この修正はT's Coreの設定にある **配偶者部屋のタイル修正** で切り替えられます。使用しているFarmHouse Modによっては、この設定は必要ありません。

## Custom Spouse Room Window Fix

Content Patcherから読み込まれたカスタム配偶者部屋では、通常の `indoor` の窓タイルを使用していても、配偶者部屋のMap Override処理によって別のTileSheet IDが割り当てられる場合があります。その結果、Stardew Valley標準の `DayTiles`、`NightTiles`、`WindowLight` による窓の昼夜切り替えが正しく生成・維持されないことがあります。

T's Coreは、このような配偶者部屋の窓でもStardew Valley標準の昼夜切り替え処理を利用できるよう補正します。また、すでに夜または雨天の状態でカスタム配偶者部屋が再読み込みされた場合は、夜用タイル状態を再適用します。

------------------------------------------------------------------------

<a id="notes"></a>

## 注意事項

T's CoreのTile ActionはBigCraftable Extensionで使用できます:

``` json
{
    "TileAction": "TsCore_Sleep"
}
```

または:

``` json
{
    "TileAction": "TsCore_Sleep Remove"
}
```

`TsCore_Sleep` は最後に使用した永続的なT's Core睡眠場所を記憶します。

`TsCore_Sleep Remove` は睡眠場所を保存せず、それまで記憶されていたT's Core睡眠場所を消去します。

永続的なT's Core睡眠場所は次の情報を基に識別されます:

``` text
Location + Anchor position + Big Craftable type
```

個々の物理的なObject Instanceそのものを識別するものではありません。

T's CoreのGame State Queryでは、否定を含む標準のGame State Query構文を使用できます:

``` text
!TsCore_LOCATION_CATEGORY Target Dungeon
```

現在対応しているT's CoreのLocation Category:

``` text
Dungeon
```

今後のT's Core Versionでは、追加のTile Action、Game State Query、その他の小規模な公開機能がこのページに記載される場合があります。

------------------------------------------------------------------------

## Modder Guide

-   ← [Polyamory Sweet Rooms Integration](ModderGuide_PolyamorySweetRoomsIntegration.md)
-   ↑ [Guide Index](#top)
-   → *(ガイド終了)*

← [READMEに戻る](../../README.md)

← [Modder Guideに戻る](ModderGuide.md)
