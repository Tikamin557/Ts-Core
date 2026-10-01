# 📖 Modder Guide

このガイドでは、**T's Core** がContent
Patcher向けに提供している公開機能の使用方法を説明します。

<a id="top"></a>

## Guide Index

-   📄 [Relationship Services](ModderGuide_RelationshipServices.md)
-   📄 [Location Services](ModderGuide_LocationServices.md)
-   📄 [Warp Services](ModderGuide_WarpServices.md)
-   ✅ **Map Properties** *(現在のページ)*
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

# Map Properties

T's
Coreでは、C#コードを必要とせずにLocationへ追加の動作を設定できるカスタムMap
Propertyを提供しています。

これらのPropertyはMapへ直接追加でき、Content
Patcherやその他のMapベースのModから使用することを想定しています。

このガイドでは、現在T's Coreが対応しているカスタムMap
Propertyについて説明します。

------------------------------------------------------------------------

## 目次

-   [Timed Exit](#timed-exit)
-   [TsCoreTimedExit](#tscoretimedexit)
-   [TsCoreTimedExitMessage](#tscoretimedexitmessage)
-   [TsCoreTimedExitSound](#tscoretimedexitsound)
-   [翻訳されたMessageを使用する](#using-translated-messages)
-   [使用例](#examples)
-   [動作](#behavior)
-   [エラー処理](#error-handling)
-   [注意事項](#notes)

------------------------------------------------------------------------

## Timed Exit

Timed
Exitを使用すると、指定したゲーム内時刻以降にプレイヤーをLocationから自動的にワープさせることができます。

次のMap Propertyを使用して設定します。

  -----------------------------------------------------------------------------------------------------------------
  Property                   必須                  説明
  -------------------------- --------------------- ----------------------------------------------------------------
  `TsCoreTimedExit`          ✅                    Locationから退出する時刻と、使用するT's Core Warp
                                                   Actionを指定します。

  `TsCoreTimedExitMessage`   ✅                    ワープ前に表示するMessageを指定するか、Messageを無効にします。

  `TsCoreTimedExitSound`     任意                  Timed Exitが発動したときに再生するAudio Cueを指定します。
  -----------------------------------------------------------------------------------------------------------------

Timed Exitは既存のT's Core Warp
Actionを使用するため、行き先には次の機能を利用できます。

-   Warp Provider
-   Location名
-   座標の直接指定
-   向きの指定
-   WarpのAudio Cue
-   Audio Cueの繰り返し設定
-   Blackoutの時間設定
-   Audio Cueの再生開始遅延

Warp ActionとWarp Providerの詳しい情報については、[Warp
Services](ModderGuide_WarpServices.md) ガイドを参照してください。

------------------------------------------------------------------------

## TsCoreTimedExit

`TsCoreTimedExit` は、Timed Exitが有効になるゲーム内時刻と、実行するWarp
Actionを指定します。

### 構文

``` text
<Time> <WarpAction> <WarpAction arguments...>
```

例:

``` text
2100 TsCoreWarp Farm 64 15
```

または:

``` text
2100 TsCoreMagicWarp FarmHouseFront Down
```

最初の値はゲーム内時刻です。

時刻より後ろの部分には、既存のT's Core Warp
Actionと同じ通常の構文を使用します。

対応しているWarp Action:

-   `TsCoreWarp`
-   `TsCoreMagicWarp`
-   `TsCoreMagicWarp_Simple`

例:

``` text
2100 TsCoreWarp Farm 64 15
2100 TsCoreMagicWarp Farm 64 15
2100 TsCoreMagicWarp_Simple FarmHouseFront
2100 TsCoreMagicWarp FarmHouseFront Down wand
2100 TsCoreMagicWarp FarmHouseFront Auto wand 3 100 200 250
```

Timed Exitは独自のワープシステムを使用しているわけではありません。

Warp Actionは、通常のTile ActionやTouch Actionと同じT's Core Warp
Servicesへ渡されます。

そのため、Warp ProviderやWarp Actionの省略可能な引数は、[Warp
Services](ModderGuide_WarpServices.md)
ガイドに記載されているものと同じように動作します。

------------------------------------------------------------------------

### 時刻による動作

現在のゲーム内時刻が設定時刻と同じか、それより後になるとTimed
Exitが有効になります。

例:

``` text
2100 TsCoreWarp Farm 64 15
```

は次の時刻に有効になります:

``` text
21:00
```

設定時刻になった時点ですでにプレイヤーがそのLocation内にいる場合、Timed
Exitが発動します。

設定時刻を過ぎてからプレイヤーがそのLocationへ入った場合も、Timed
Exitはすぐに発動します。

たとえば次の設定では:

``` text
2100 TsCoreWarp Farm 64 15
```

Timed Exitは次のどちらの場合にも発動します:

``` text
20:50 → 21:00
```

また、プレイヤーが次の時刻にLocationへ入った場合:

``` text
22:00
```

この動作により、設定時刻を過ぎてからLocationへ入るだけでTimed
Exitを回避することはできません。

------------------------------------------------------------------------

## TsCoreTimedExitMessage

`TsCoreTimedExitMessage` は、Timed
Exitが発動したときに表示するMessageを制御します。

MessageはWarp Actionを実行する前に表示されます。

3種類の形式に対応しています。

------------------------------------------------------------------------

### Messageを直接指定する

通常の値を指定すると、その内容がDialogueとして直接表示されます。

``` text
TsCoreTimedExitMessage = The facility is now closed.
```

プレイヤーがDialogueを閉じた後にWarp Actionが実行されます。

------------------------------------------------------------------------

### Messageを表示しない

次のように指定します:

``` text
TsCoreTimedExitMessage = false
```

これによりDialogueを無効にできます。

Timed Exitが発動すると、Warp Actionがすぐに実行されます。

大文字と小文字は区別されないため、次の値はすべて同じように扱われます:

``` text
false
FALSE
False
```

これらはすべて同じ扱いになります。

`false` として判定されるのは、実際のMap Propertyの値だけです。

翻訳されたMessage内にたまたま次の文字列が含まれていても:

``` text
false
```

通常どおりDialogueとして表示されます。

------------------------------------------------------------------------

### Strings/StringsFromMapsのKey

値全体がダブルクォーテーションで囲まれている場合、T's
Coreはその値を次のData AssetのKeyとして扱います:

``` text
Strings/StringsFromMaps
```

例:

``` text
TsCoreTimedExitMessage = "MyMod_TimedExitMessage"
```

T's Coreは次の値を読み込み:

``` text
Strings/StringsFromMaps:MyMod_TimedExitMessage
```

取得したテキストを表示します。

この形式は翻訳されたMessageを使用する場合に便利です。

値全体がダブルクォーテーションで囲まれていない場合は、通常のMessage本文として扱われます。

例:

``` text
The shop closes at "21:00".
```

は、値全体がダブルクォーテーションで囲まれていないため、そのまま直接表示されます。

------------------------------------------------------------------------

### Dialogueの書式

Timed ExitのMessageでは、Stardew ValleyのMap
Messageと同じ基本的なDialogue書式を使用できます。

次のように指定します:

``` text
^
```

で改行します。

例:

``` text
The facility is now closed.^Please come again tomorrow.
```

おおよそ次のように表示されます:

``` text
The facility is now closed.
Please come again tomorrow.
```

次のように指定します:

``` text
#
```

で次のDialogueページへ進みます。

例:

``` text
The facility is now closed.#Thank you for visiting.
```

ページ間を移動している間はWarp Actionは実行されません。

最後のDialogueを閉じた後にのみ実行されます。

2つの記号を組み合わせて使用することもできます:

``` text
The facility is now closed.^Please prepare to leave.#Thank you for visiting.
```

------------------------------------------------------------------------

## TsCoreTimedExitSound

`TsCoreTimedExitSound` は、Timed Exitが発動したときにStardew
ValleyのAudio Cueを任意で再生します。

例:

``` text
TsCoreTimedExitSound = crystal
```

Audio CueはTimed Exitの処理開始時に再生されます。

Messageが有効な場合、Messageを表示するときにSoundが再生されます。

次の場合:

``` text
TsCoreTimedExitMessage = false
```

Warp Actionが始まる直前にSoundが再生されます。

例:

``` text
TsCoreTimedExit = 2100 TsCoreWarp Farm 64 15
TsCoreTimedExitMessage = The facility is now closed.
TsCoreTimedExitSound = crystal
```

処理の流れは次のとおりです:

``` text
Timed Exit triggered
    ↓
Play "crystal"
    ↓
Display message
    ↓
Player closes message
    ↓
Perform Warp Action
```

`TsCoreTimedExitSound` Propertyが指定されていない場合、Timed
Exit用の追加Soundは再生されません。

空の値も、追加Soundなしとして扱われます。

> **注意:** `TsCoreTimedExitSound` は、Warp Action自体に設定するAudio
> Cueとは別のものです。
>
> そのため、Timed
> Exitでは退出Messageの表示時に1つのSoundを再生し、実際のワープ中にはWarp
> Action側で別のSoundを使用できます。

例:

``` text
TsCoreTimedExit = 2100 TsCoreMagicWarp Farm 64 15 Auto wand
TsCoreTimedExitMessage = The facility is now closed.
TsCoreTimedExitSound = crystal
```

では次のSoundが再生されます:

``` text
crystal
```

Timed Exitの発動時に再生され、

``` text
wand
```

はMagic Warpの一部として再生されます。

------------------------------------------------------------------------

<a id="using-translated-messages"></a>

## 翻訳されたMessageを使用する

翻訳されたDialogueを使用する場合は、Messageを次のData Assetへ追加します:

``` text
Strings/StringsFromMaps
```

Content Patcherを使用して追加します。

例:

``` json
{
    "Action": "EditData",
    "Target": "Strings/StringsFromMaps",
    "Entries": {
        "MyMod_TimedExitMessage": "{{i18n:MyMod_TimedExitMessage}}"
    }
}
```

次に、Content Packのi18nファイルへ対応するEntryを追加します。

例:

``` json
{
    "MyMod_TimedExitMessage": "The facility is now closed.^Please prepare to leave.#Thank you for visiting."
}
```

その後、Map PropertyからそのKeyを参照できます:

``` text
TsCoreTimedExitMessage = "MyMod_TimedExitMessage"
```

Content Patcherは `Strings/StringsFromMaps` を編集する際にi18n
Tokenを解決し、Timed Exitが発動したときにT's
Coreがその翻訳済みテキストを読み込みます。

これにより、同じMap
Propertyから現在のプレイヤーに適した言語のMessageを表示できます。

> **注意:** Content Patcherのi18n TokenをMap
> Propertyへ直接記述する方法には対応していません。
>
> 例:
>
> ``` text
> TsCoreTimedExitMessage = {{i18n:MyMod_TimedExitMessage}}
> ```
>
> この形式はT's Coreでは解決されません。
>
> 代わりに、上記のように `Strings/StringsFromMaps` を使用してください。

------------------------------------------------------------------------

<a id="examples"></a>

## 使用例

### 基本的なTimed Exit

次のPropertyでは21:00にMessageを表示し、Dialogueを閉じた後にプレイヤーをFarmへワープさせます。

``` text
TsCoreTimedExit = 2100 TsCoreWarp Farm 64 15
TsCoreTimedExitMessage = The facility is now closed.
```

------------------------------------------------------------------------

### MessageなしのTimed Exit

``` text
TsCoreTimedExit = 2100 TsCoreWarp Farm 64 15
TsCoreTimedExitMessage = false
```

21:00以降になると、Warp Actionがすぐに実行されます。

------------------------------------------------------------------------

### Sound付きのTimed Exit

``` text
TsCoreTimedExit = 2100 TsCoreWarp Farm 64 15
TsCoreTimedExitMessage = The facility is now closed.
TsCoreTimedExitSound = crystal
```

Timed Exitが発動すると:

1.  `crystal` が再生されます。
2.  Messageが表示されます。
3.  Messageを閉じた後にWarp Actionが実行されます。

------------------------------------------------------------------------

### Warp Providerを使用するTimed Exit

``` text
TsCoreTimedExit = 2100 TsCoreMagicWarp FarmHouseFront Down
TsCoreTimedExitMessage = The facility is now closed.
TsCoreTimedExitSound = crystal
```

この例では組み込みの:

``` text
FarmHouseFront
```

Warp Providerを、固定された行き先座標の代わりに使用します。

------------------------------------------------------------------------

### 翻訳されたMessageを使用するTimed Exit

Map Property:

``` text
TsCoreTimedExit = 2100 TsCoreWarp Farm 64 15
TsCoreTimedExitMessage = "MyMod_TimedExitMessage"
TsCoreTimedExitSound = crystal
```

Content Patcher:

``` json
{
    "Action": "EditData",
    "Target": "Strings/StringsFromMaps",
    "Entries": {
        "MyMod_TimedExitMessage": "{{i18n:MyMod_TimedExitMessage}}"
    }
}
```

i18n:

``` json
{
    "MyMod_TimedExitMessage": "The facility is now closed.^Please prepare to leave.#Thank you for visiting."
}
```

------------------------------------------------------------------------

### Content Patcher EditMapの例

Timed Exit Propertyは、Content
Patcherを使用してMapへ追加することもできます。

例:

``` json
{
    "Action": "EditMap",
    "Target": "Maps/Custom_MyLocation",
    "MapProperties": {
        "TsCoreTimedExit": "2100 TsCoreMagicWarp FarmHouseFront Down",
        "TsCoreTimedExitMessage": "\"MyMod_TimedExitMessage\"",
        "TsCoreTimedExitSound": "crystal"
    }
}
```

翻訳テキストは別のPatchで登録できます:

``` json
{
    "Action": "EditData",
    "Target": "Strings/StringsFromMaps",
    "Entries": {
        "MyMod_TimedExitMessage": "{{i18n:MyMod_TimedExitMessage}}"
    }
}
```

------------------------------------------------------------------------

<a id="behavior"></a>

## 動作

Timed Exitは主に次の2つの状況で確認されます:

-   ゲーム内時刻が変化したとき
-   ローカルプレイヤーがLocationへ入ったとき

これにより、設定した退出時刻の時点ですでにLocation内にいる場合と、退出時刻を過ぎてからLocationへ入った場合の両方に対応できます。

Timed Exitの処理が開始されると、T's
CoreはDialogueが終了するまで待ってからWarp Actionを実行します。

Messageに次の記号で区切られた複数ページが含まれている場合:

``` text
#
```

プレイヤーは通常どおり各ページを進めることができます。

Warp Actionは、Dialogue全体を閉じた後にのみ開始されます。

次の場合:

``` text
TsCoreTimedExitMessage = false
```

Dialogueによる待機はなく、Warp Actionがすぐに開始されます。

------------------------------------------------------------------------

<a id="error-handling"></a>

## エラー処理

T's Coreは退出処理を実行する前にTimed Exit Propertyを検証します。

不正な設定はSMAPIログへ出力されます。

例:

-   `TsCoreTimedExitMessage` が存在しない
-   `TsCoreTimedExitMessage` が空
-   時刻の値が不正
-   対応していないWarp Action
-   `Strings/StringsFromMaps` のKeyが存在しない
-   翻訳Keyが空
-   Warp Actionの引数が不正

T's CoreがTimed Exitの動作を判断できなくなる設定エラーの場合、Warp
Actionは実行されません。

たとえば次の場合:

``` text
TsCoreTimedExitMessage = "MissingMessageKey"
```

が次のData Assetに存在しないKeyを参照している場合:

``` text
Strings/StringsFromMaps
```

T's Coreはエラーをログへ出力し、Timed Exitを実行しません。

------------------------------------------------------------------------

### 不正なAudio Cue

次のPropertyに不正な値が指定されていても:

``` text
TsCoreTimedExitSound
```

Timed Exitの処理自体は継続されます。

例:

``` text
TsCoreTimedExitSound = InvalidAudioCue
```

T's Coreはエラーをログへ出力しますが、通常のTimed Exit処理は継続します。

Messageが有効な場合:

``` text
Invalid audio cue
    ↓
Error logged
    ↓
Display message
    ↓
Perform Warp Action
```

Messageが無効な場合:

``` text
Invalid audio cue
    ↓
Error logged
    ↓
Perform Warp Action
```

これにより、任意設定であるSoundの設定エラーによって実際の退出処理が妨げられることを防ぎます。

------------------------------------------------------------------------

<a id="notes"></a>

## 注意事項

Map PropertiesはMapベースのModやContent Patcher Content
Packから使用することを想定しており、独自のC#コードは必要ありません。

現在、Timed Exitでは次のPropertyに対応しています:

``` text
TsCoreTimedExit
TsCoreTimedExitMessage
TsCoreTimedExitSound
```

`TsCoreTimedExit` は独自のワープシステムを実装するのではなく、既存のT's
Core Warp Actionを使用します。

そのため、Warp Providerや任意のワープエフェクトを含む通常のWarp
Action機能をすべて利用できます。

可能な場合は固定座標よりWarp
Providerの使用を推奨します。対応するカスタムMapやLocationの変更へ、より柔軟に追従できるためです。

Warp Actionの詳しい構文と利用可能なWarp Providerについては、[Warp
Services](ModderGuide_WarpServices.md) ガイドを参照してください。

今後のT's Coreで、カスタムMap Propertyが追加される場合があります。

------------------------------------------------------------------------

## Modder Guide

-   ← [Warp Services](ModderGuide_WarpServices.md)
-   ↑ [Guide Index](#top)
-   → [Building Services](ModderGuide_BuildingServices.md)

← [READMEに戻る](../../README.md)

← [Modder Guideに戻る](ModderGuide.md)
