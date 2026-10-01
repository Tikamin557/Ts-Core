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
-   ✅ **Notification System** *(現在のページ)*
-   📄 [Content Patcher
Integration](ModderGuide_ContentPatcherIntegration.md)
-   📄 [Post Renovation Patch](ModderGuide_PostRenovationPatch.md)
-   📄 [Polyamory Sweet Rooms
Integration](ModderGuide_PolyamorySweetRoomsIntegration.md)
-   📄 [Other Features](ModderGuide_OtherFeatures.md)

← [READMEに戻る](../../README.md)

← [Modder Guideに戻る](ModderGuide.md)

------------------------------------------------------------------------

# Notification System

Notification Systemを使用すると、Content Patcher Content PackからT's Coreを利用したカスタマイズ可能な画面上Notificationを表示できます。

Notificationは `TsCoreNotification` Actionを使用して、Tile Action、Touch Action、Trigger Actionから表示できます。

T's Coreでは次のカスタムData Assetも提供しています:

``` text
TsCore/NotificationThemes
```

Content Patcher Content Packでは `EditData` を使用してカスタムNotification Themeを登録できます。

Themeでは、色、Border、Textの外観、Layout、画面上の位置、NotificationをDismissするタイミングを制御できます。

T's Coreには、直接使用したりカスタムThemeから継承したりできるBuilt-in Notification Themeも複数用意されています。

------------------------------------------------------------------------

<a id="contents"></a>

## 目次

-   [Notification Action](#notification-action)
-   [Syntax](#syntax)
-   [Built-in Notification Themes](#built-in-notification-themes)
-   [Priority](#priority)
-   [Duration](#duration)
-   [FirstVisitToday](#firstvisittoday)
-   [Content Pack Setup](#content-pack-setup)
-   [Custom Notification Themes](#custom-notification-themes)
-   [Theme Properties](#theme-properties)
-   [Theme Inheritance](#theme-inheritance)
-   [Dismissal on Location Change](#dismissal-on-location-change)
-   [Examples](#examples)
-   [Debugging](#debugging)
-   [Reloading Content Patcher Content
Packs](#reloading-content-patcher-content-packs)
-   [Notes](#notes)

------------------------------------------------------------------------

## Notification Action

T's CoreではNotificationを表示するための次のカスタムActionを提供しています:

``` text
TsCoreNotification
```

次のActionで使用できます:

-   Tile Action
-   Touch Action
-   Trigger Action

対応するすべてのAction Typeで同じNotification構文を使用します。

------------------------------------------------------------------------

<a id="syntax"></a>

## 構文

標準構文:

``` text
TsCoreNotification <TypeOrTheme> <Priority> <Duration> <Message...>
```

例:

``` text
TsCoreNotification Info Normal 180 Welcome to the farm!
```

引数:

------------------------------------------------------------------------------------------------------------------- 引数                  必須                  説明 --------------------- --------------------- ----------------------------------------------------------------------- `TypeOrTheme`         ✅                    Built-in Notification Theme IDまたはカスタムNotification Theme ID。

`Priority`            ✅                    Notificationの表示Priority。

`Duration`            ✅                    update tick単位の表示時間。

`Message`             ✅                    Notificationに表示するText。残りの引数は結合されてMessageになります。 -------------------------------------------------------------------------------------------------------------------

例:

``` text
TsCoreNotification Warning High 300 Watch out!
```

ではBuilt-in `Warning` Themeを使用し、Notificationに `High` Priorityを設定して300 update tickの間表示します。

カスタムThemeでもまったく同じ構文を使用します:

``` text
TsCoreNotification MyTheme Normal 240 Custom notification text
```

`MyTheme` が `TsCore/NotificationThemes` に登録されている場合、そのThemeを使用してNotificationを表示します。

------------------------------------------------------------------------

<a id="built-in-notification-themes"></a>

## Built-in Notification Theme

T's Coreには次のBuilt-in Notification Themeがあります:

Theme           用途 --------------- ---------------------------------------- `Info`          一般的な情報Notification。 `Success`       成功または完了を知らせるNotification。 `Error`         Error Notification。 `Warning`       Warning Notification。 `Quest`         Quest関連のNotification。 `Achievement`   Achievement風のNotification。 `Boss`          重要度の高いBoss風Notification。 `Lavender`      Lavender色のNotification Theme。 `Rose`          Rose色のNotification Theme。 `RetroWindow`   Retro風のMessage Window。

Built-in Themeは `TsCoreNotification` から直接使用できます。

例:

``` text
TsCoreNotification Achievement Normal 240 Achievement unlocked!
```

これらのThemeはT's Core内部で実装されています。

これらは **`TsCore/NotificationThemes` のEntryではなく**、Content Patcherから置き換えたり変更したりすることはできません。

ただし、カスタムNotification ThemeのBase Themeとして使用できます。

例:

``` json
{
  "Action": "EditData",
  "Target": "TsCore/NotificationThemes",
  "Entries": {
    "MyInfoTheme": {
      "Base": "Info",
      "TextScale": 1.2
    }
  }
}
```

次のIDはT's Coreによって予約されています:

``` text
Info
Success
Error
Warning
Quest
Achievement
Boss
Lavender
Rose
RetroWindow
```

Content Patcher PackがこれらのIDのいずれかを `TsCore/NotificationThemes` に登録しようとした場合、外部定義は無視され、代わりにT's CoreのBuilt-in Themeが使用されます。

また、T's Coreは次のような警告をログへ出力します:

``` text
Notification Theme ID 'Info' is reserved by T's Core and cannot be overridden.
```

------------------------------------------------------------------------

## Priority

Notificationには表示Priorityがあります。

Priorityは複数のNotificationが待機している場合に、どのNotificationを先に表示するかを決定します。

利用可能なPriority:

Priority     説明 ------------ ------------------------------ `Low`        低PriorityのNotification。 `Normal`     標準PriorityのNotification。 `High`       高PriorityのNotification。 `Critical`   最高PriorityのNotification。

例:

``` text
TsCoreNotification Warning High 180 Warning message
```

別のNotificationを表示中に、より高いPriorityのNotificationを受け取った場合、高PriorityのNotificationがその表示を引き継ぐことがあります。

中断されたNotificationはQueueへ戻され、その後再び表示されることがあります。

Queueで待機しているNotificationもPriorityに従って選択されます。

------------------------------------------------------------------------

## Duration

`Duration` はNotificationを表示し続ける時間を制御します。

値は **update tick** 単位で指定します。

例:

``` text
TsCoreNotification Info Normal 300 This notification lasts longer.
```

DurationはNotificationの通常の表示時間を制御します。

Notification ThemeにLocationに基づくDismiss条件が設定されている場合、Notificationがそれより早く消えることがあります。

------------------------------------------------------------------------

## FirstVisitToday

`TsCoreNotification` には任意で使用できる `FirstVisitToday` Modeがあります。

プレイヤーが特定のLocationへその日に初めて入ったときだけNotificationを表示したい場合に便利です。

### 構文

``` text
TsCoreNotification FirstVisitToday <LocationName> <TypeOrTheme> <Priority> <Duration> <Message...>
```

例:

``` text
TsCoreNotification FirstVisitToday Custom_MyLocation Info Normal 300 Welcome!
```

Notificationが表示されるのは、次の両方を満たす場合だけです:

1.  プレイヤーの現在Locationが `<LocationName>` と一致する。
2.  そのLocationへの訪問が現在の日付で初回である。

同じ日に同じLocationへ再訪しても、そのNotificationは再表示されません。

初回訪問の状態は新しい日になるとリセットされます。

------------------------------------------------------------------------

### Trigger Actionの例

`FirstVisitToday` はStardew Valleyの `LocationChanged` Trigger Actionと組み合わせると特に便利です。

``` json
{
  "Action": "EditData",
  "Target": "Data/TriggerActions",
  "Entries": {
    "MyMod_LocationInfo": {
      "Id": "MyMod_LocationInfo",
      "Trigger": "LocationChanged",
      "Condition": "LOCATION_NAME Here Custom_MyLocation",
      "Actions": [
        "TsCoreNotification FirstVisitToday Custom_MyLocation Info High 300 {{i18n:LocationInfoText}}"
      ],
      "MarkActionApplied": false
    }
  }
}
```

`MarkActionApplied` を `false` にすることで、その後Locationが変わったときにもStardew ValleyがTrigger Actionを再び発生させられるようにします。

1日1回の動作はT's Coreが `FirstVisitToday` を通して処理します。

これにより、Trigger Action自体は繰り返し実行可能なまま、Notificationだけをその日の初回訪問時に表示できます。

------------------------------------------------------------------------

<a id="content-pack-setup"></a>

## Content Packのセットアップ

カスタムNotification Themeは、Content Patcherから次のData Assetへ登録します:

``` text
TsCore/NotificationThemes
```

通常のContent Patcher Content Packを使用します。

### manifest.json

``` json
{
  "Name": "[CP] My Notification Pack",
  "Author": "YourName",
  "Version": "1.0.0",
  "UniqueID": "YourName.MyNotificationPack",
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

`EditData` を使用してNotification Themeを追加します:

``` json
{
  "Format": "2.9.0",
  "Changes": [
    {
      "Action": "EditData",
      "Target": "TsCore/NotificationThemes",
      "Entries": {
        "MyInfoTheme": {
          "Base": "Info",
          "BackgroundColor": {
            "R": 30,
            "G": 60,
            "B": 120,
            "A": 220
          },
          "TextScale": 1.0
        }
      }
    }
  ]
}
```

`Entries` のKeyがNotification Theme IDになります。

この例では:

``` text
MyInfoTheme
```

は次のように使用できます:

``` text
TsCoreNotification MyInfoTheme Normal 180 Hello!
```

1つの `EditData` Patchで複数のThemeを登録できます:

``` json
{
  "Action": "EditData",
  "Target": "TsCore/NotificationThemes",
  "Entries": {
    "MyInfoTheme": {
      "Base": "Info",
      "TextScale": 1.1
    },
    "MyWarningTheme": {
      "Base": "Warning",
      "TextScale": 1.2
    }
  }
}
```

Notification Theme IDは `Entries` のKeyによって決まります。

T's Coreでは、特別なFolder構成やNotification Themeごとの個別JSONファイルは必要ありません。

> **重要:** Notification Theme IDは全体で一意にしてください。T's
> Coreが予約しているBuilt-in Theme IDは使用しないでください。

カスタムThemeでは、可能な場合は自分のModのUniqueIDを基にしたIDを使用することを推奨します。

例:

``` text
YourName.MyMod_MyTheme
```

------------------------------------------------------------------------

<a id="custom-notification-themes"></a>

## カスタムNotification Theme

Notification ThemeはNotificationの見た目と一部の動作を制御します。

例:

``` json
{
  "Action": "EditData",
  "Target": "TsCore/NotificationThemes",
  "Entries": {
    "MyInfoTheme": {
      "Base": "Info",
      "BackgroundColor": {
        "R": 30,
        "G": 60,
        "B": 120,
        "A": 220
      },
      "TextColor": {
        "R": 255,
        "G": 255,
        "B": 255,
        "A": 255
      },
      "TextScale": 1.0,
      "Anchor": "Bottom",
      "OffsetY": -60
    }
  }
}
```

このThemeは未指定のPropertyをBuilt-in `Info` Themeから継承し、Entry内で定義したPropertyだけを上書きします。

変更するPropertyが少ない場合は継承の使用を推奨します。

Themeは継承を使用せず、すべてのPropertyを独自に定義することもできます:

``` json
"Base": null
```

------------------------------------------------------------------------

<a id="theme-properties"></a>

## ThemeのProperty

Notification Themeでは次のPropertyを使用できます。

### General

Property   Type     説明 ---------- -------- --------------------------------- `Base`     string   継承元のNotification Theme ID。

------------------------------------------------------------------------

### BackgroundとBorder

Property            Type    説明 ------------------- ------- ------------------------ `BackgroundColor`   Color   Notificationの背景色。 `BorderColor`       Color   Borderの色。 `BorderStyle`       enum    Borderの描画Style。 `BorderThickness`   int     Borderの太さ。

ColorはRGBA値で指定できます:

``` json
"BackgroundColor": {
  "R": 30,
  "G": 60,
  "B": 120,
  "A": 220
}
```

------------------------------------------------------------------------

### Text

Property         Type      説明 ---------------- --------- --------------------------------------- `TextColor`      Color     Textの色。 `ShadowColor`    Color     Text Shadowの色。 `DrawShadow`     bool      Text Shadowを描画するか。 `ShadowOffset`   Vector2   Text ShadowのOffset。 `TextScale`      float     Text Sizeの倍率。 `TextAnchor`     enum      Notification Window内でのTextの配置。

`ShadowOffset` の例:

``` json
"ShadowOffset": "2, 2"
```

------------------------------------------------------------------------

### Layout

Property          Type   説明 ----------------- ------ -------------------------------------------------- `MinHeight`       int    Notificationの最小Height。 `MinWidth`        int    Notificationの最小Width。 `PaddingX`        int    Textの水平方向Padding。 `PaddingY`        int    Textの垂直方向Padding。 `BorderPadding`   int    Border周辺に使用するPadding。 `Anchor`          enum   NotificationのAnchorとして使用する画面上の位置。 `OffsetX`         int    選択したAnchorからの水平方向Offset。 `OffsetY`         int    選択したAnchorからの垂直方向Offset。

------------------------------------------------------------------------

### Dismissal

-------------------------------------------------------------------------------------------------------------------------- Property                    Type           説明 --------------------------- -------------- ------------------------------------------------------------------------------- `DismissOnLocationChange`   bool           プレイヤーがLocationを変更するたびにNotificationをDismissします。

`DismissOnEnterLocations`   string\[\]     プレイヤーが指定Locationのいずれかへ入ったときにNotificationをDismissします。 --------------------------------------------------------------------------------------------------------------------------

ThemeのPropertyはすべて任意です。

Propertyを省略し、そのThemeに `Base` が設定されている場合、値はBase Themeから継承されます。

------------------------------------------------------------------------

<a id="theme-inheritance"></a>

## Themeの継承

Notification Themeは `Base` Propertyを使用して別のNotification Themeを継承できます。

例:

``` json
{
  "Action": "EditData",
  "Target": "TsCore/NotificationThemes",
  "Entries": {
    "MyTheme": {
      "Base": "Info",
      "TextScale": 1.2,
      "OffsetY": -100
    }
  }
}
```

これによりBuilt-in `Info` Themeを基に、Text Scaleと垂直位置だけを変更した `MyTheme` が作成されます。

Built-in Themeは `TsCore/NotificationThemes` のEntryとして保存されていませんが、Base Themeとして使用できます。

カスタムTheme同士でも継承を使用できます。

例:

``` json
{
  "Action": "EditData",
  "Target": "TsCore/NotificationThemes",
  "Entries": {
    "MyBaseTheme": {
      "Base": "Info",
      "TextScale": 1.1
    },
    "MyChildTheme": {
      "Base": "MyBaseTheme",
      "TextColor": {
        "R": 255,
        "G": 220,
        "B": 100,
        "A": 255
      }
    }
  }
}
```

Child Themeで明示的に定義されていないPropertyはParentから継承されます。

これには次の設定が含まれます:

-   BackgroundとBorderの設定
-   Textの外観
-   Layout設定
-   画面上の位置
-   Locationに基づくDismiss設定

Child Themeで明示的に定義したPropertyは、Base Themeから継承した値より優先されます。

Built-in Themeは継承を解決する前にT's Core内部でMergeされます。

これによりカスタムThemeは、次のようなBuilt-in Themeを安全に継承できます:

``` text
Info
Warning
Rose
RetroWindow
```

同時に、それらのBuilt-in ThemeがContent Patcherから上書きされることを防ぎます。

> **注意:** Themeの循環継承には対応していません。T's
> Coreが循環継承を検出すると警告をログへ出力します。

------------------------------------------------------------------------

<a id="dismissal-on-location-change"></a>

## Location変更時のDismiss

Notification Themeでは、プレイヤーがLocationを変更したときにNotificationを消すかどうかを制御できます。

利用できる設定は2つあります。

------------------------------------------------------------------------

### DismissOnLocationChange

次のように設定します:

``` json
"DismissOnLocationChange": true
```

これにより、プレイヤーがLocationを変更するたびにNotificationをDismissします。

例:

``` json
{
  "Base": "Info",
  "DismissOnLocationChange": true
}
```

このThemeを使用するNotificationは、Local Playerが別のLocationへWarpするとすぐに消えます。

現在のエリアを離れた後は表示する必要がない、Location固有の案内や情報に便利です。

------------------------------------------------------------------------

### DismissOnEnterLocations

通常のLocation変更ではNotificationを表示したままにし、特定のLocationへ入ったときだけ消したい場合は `DismissOnEnterLocations` を使用できます。

例:

``` json
{
  "Base": "Info",
  "DismissOnEnterLocations": [
    "Farm",
    "FarmHouse"
  ]
}
```

このThemeを使用するNotificationは、プレイヤーが `Farm` または `FarmHouse` へ入るとDismissされます。

別のDismiss Ruleが適用されない限り、他のLocationへ入ってもDismissされません。

Location名は複数指定できます。

------------------------------------------------------------------------

### 設定の組み合わせ

両方の設定を同じThemeに定義できます:

``` json
{
  "Base": "Info",
  "DismissOnLocationChange": false,
  "DismissOnEnterLocations": [
    "FarmHouse"
  ]
}
```

この例では、Locationが変わるたびにNotificationが消えるわけでは **ありません**。

プレイヤーが `FarmHouse` へ入ったときだけ消えます。

`DismissOnLocationChange` が `true` の場合、`DismissOnEnterLocations` Listに関係なく、Locationが変わるとNotificationはDismissされます。

------------------------------------------------------------------------

### Queue内のNotification

Locationに基づくDismiss Ruleは、Notification Queueで待機中のNotificationにも適用されます。

プレイヤーがLocationを変更すると、T's Coreは次を確認します:

-   現在表示中のNotification
-   Queueで待機中のNotification

新しいLocationがDismiss条件に一致するNotificationはすべて削除されます。

現在表示中のNotificationがDismissされた場合、T's CoreはQueueから次に表示可能なNotificationをすぐに表示しようとします。

これにより、関係のないQueue内Notificationの表示を妨げずに、Location固有のNotificationだけを適切に削除できます。

------------------------------------------------------------------------

<a id="examples"></a>

## 使用例

### 基本的なInformation Notification

``` text
TsCoreNotification Info Normal 180 Welcome!
```

------------------------------------------------------------------------

### Warning Notification

``` text
TsCoreNotification Warning High 300 Watch out!
```

------------------------------------------------------------------------

### カスタムTheme

`MyInfoTheme` が `TsCore/NotificationThemes` に登録されている場合:

``` text
TsCoreNotification MyInfoTheme Normal 240 This uses my custom theme.
```

------------------------------------------------------------------------

### First Visit Today

``` text
TsCoreNotification FirstVisitToday Custom_MyLocation MyInfoTheme High 300 Welcome to this location!
```

------------------------------------------------------------------------

### Tile Action

次の例では、プレイヤーがTileを操作したときにNotificationを表示します。

``` json
{
  "Action": "EditMap",
  "Target": "Maps/Town",
  "MapTiles": [
    {
      "Position": {
        "X": 26,
        "Y": 28
      },
      "Layer": "Buildings",
      "SetProperties": {
        "Action": "TsCoreNotification Info Normal 180 Hello from Pelican Town!"
      }
    }
  ]
}
```

------------------------------------------------------------------------

### Trigger Action

次の例では、プレイヤーが特定のCustom Locationへ入るたびにNotificationを表示します。

``` json
{
  "Action": "EditData",
  "Target": "Data/TriggerActions",
  "Entries": {
    "MyMod_LocationMessage": {
      "Id": "MyMod_LocationMessage",
      "Trigger": "LocationChanged",
      "Condition": "LOCATION_NAME Here Custom_MyLocation",
      "Actions": [
        "TsCoreNotification MyInfoTheme Normal 300 {{i18n:LocationMessage}}"
      ],
      "MarkActionApplied": false
    }
  }
}
```

------------------------------------------------------------------------

### First Visit Trigger Action

次のVersionでは、その日の初回訪問時だけNotificationを表示します:

``` json
{
  "Action": "EditData",
  "Target": "Data/TriggerActions",
  "Entries": {
    "MyMod_FirstVisitMessage": {
      "Id": "MyMod_FirstVisitMessage",
      "Trigger": "LocationChanged",
      "Condition": "LOCATION_NAME Here Custom_MyLocation",
      "Actions": [
        "TsCoreNotification FirstVisitToday Custom_MyLocation MyInfoTheme Normal 300 {{i18n:LocationMessage}}"
      ],
      "MarkActionApplied": false
    }
  }
}
```

------------------------------------------------------------------------

<a id="debugging"></a>

## デバッグ

T's Coreでは、開発中にNotification Systemのテストや確認を行うためのコマンドを複数提供しています。

------------------------------------------------------------------------

### Notification Themeの確認

使用:

``` text
tscore_debug_notification_themes
```

現在登録されているNotification Themeを表示します。

出力ではThemeが次の2種類に分けて表示されます:

``` text
TsCore Built-in Themes
External Data Asset Themes
```

それぞれ次を表します:

-   **TsCore Built-in Themes** --- T's Core内部で提供される固定Theme。
-   **External Data Asset Themes** --- `TsCore/NotificationThemes`
を通して登録されたカスタムTheme。

Built-in ThemeとContent Patcherから追加したThemeが正しく利用可能になっているか確認できます。

外部Content Patcher PackがT's Coreの予約済みBuilt-in Theme IDを登録しようとした場合、T's Coreは外部定義を無視して警告をログへ出力します。

例:

``` text
[T's Core] Notification Theme ID 'Info' is reserved by T's Core and cannot be overridden.
```

------------------------------------------------------------------------

### Notificationのテスト

使用:

``` text
tscore_debug_notification <TypeOrTheme>
```

例:

``` text
tscore_debug_notification Info
```

または:

``` text
tscore_debug_notification MyInfoTheme
```

名前がBuilt-in Notification Themeと一致する場合、そのThemeが表示されます。

一致しない場合、T's Coreはその値をカスタムNotification Theme IDとして使用しようとします。

------------------------------------------------------------------------

### Notification Actionのテスト

次のコマンドでは、Trigger Actionと同じNotification Action処理を通してNotification表示をテストします:

``` text
tscore_debug_notification_trigger <TypeOrTheme> <Priority> <Duration> <Message...>
```

例:

``` text
tscore_debug_notification_trigger Warning High 500 Watch Out!
```

カスタムThemeもテストできます:

``` text
tscore_debug_notification_trigger MyInfoTheme Normal 300 Test Message
```

後で `TsCoreNotification` に使用する引数を確認する場合に便利です。

------------------------------------------------------------------------

<a id="reloading-content-patcher-content-packs"></a>

## Content Patcher Content Packの再読み込み

`TsCore/NotificationThemes` を通して登録したNotification Themeは、そのData Assetを編集しているContent Patcher Content Packを再読み込みすることで、ゲーム実行中に更新できます。

使用:

``` text
tscore_cp_reload <ContentPackId>
```

例:

``` text
tscore_cp_reload YourName.MyNotificationPack
```

開発中に次の変更を反映できます:

-   Notification Themeの追加
-   Notification Themeの削除
-   Theme Propertyの変更
-   Theme継承の変更

Stardew Valleyを再起動する必要はありません。

Content Packを再読み込みすると、カスタムBase Themeへの変更は、それを継承するカスタムThemeにも反映されます。

T's CoreのBuilt-in Themeを継承するカスタムThemeは、再読み込み後も固定されたBuilt-in Themeを引き続き継承します。

T's CoreのContent Patcher Reload Systemでは、ConfigSchema、Config Token、GMCM設定、DynamicTokensの再読み込みにも対応しています。

`tscore_cp_reload` の詳細については、[Content Patcher Integration](ModderGuide_ContentPatcherIntegration.md) ガイドを参照してください。

------------------------------------------------------------------------

<a id="notes"></a>

## 注意事項

カスタムNotification Themeは次のData Assetを通して登録します:

``` text
TsCore/NotificationThemes
```

`Entries` で使用するDictionary KeyがNotification Theme IDです。

このData Assetは **外部のカスタムNotification Theme** 用です。

T's CoreのBuilt-in Themeは内部実装されており、`TsCore/NotificationThemes` のEntryとして保存されていません。

多くのカスタムThemeでは、既存のBuilt-in Themeを継承し、必要なPropertyだけを上書きすることを推奨します。

例:

``` json
{
  "Base": "Info",
  "TextScale": 1.2,
  "DismissOnLocationChange": true
}
```

のように設定する方が、すべてのVisual Propertyを再定義するより一般的に保守しやすくなります。

次のBuilt-in Theme ID:

``` text
Info
Success
Error
Warning
Quest
Achievement
Boss
Lavender
Rose
RetroWindow
```

はT's Coreによって予約されており、`TsCore/NotificationThemes` から上書きすることはできません。

カスタムThemeでは、これらのBuilt-in Themeを `Base` として使用できます。

Locationごとに1日1回だけNotificationを表示したい場合は `FirstVisitToday` を使用し、特定のエリアにいる間だけNotificationを有効な情報として残したい場合はThemeのDismiss設定を使用してください。

開発中は次のコマンドを使用できます:

``` text
tscore_debug_notification_themes
tscore_debug_notification <TypeOrTheme>
tscore_debug_notification_trigger <TypeOrTheme> <Priority> <Duration> <Message...>
tscore_cp_reload <ContentPackId>
```

ゲームを何度も再起動せずにカスタムNotification Themeの確認、テスト、再読み込みを行えます。

------------------------------------------------------------------------

## Modder Guide

-   ← [Migration System](ModderGuide_MigrationSystem.md)
-   ↑ [Guide Index](#top)
-   → [Content Patcher
Integration](ModderGuide_ContentPatcherIntegration.md)

← [READMEに戻る](../../README.md)

← [Modder Guideに戻る](ModderGuide.md)
