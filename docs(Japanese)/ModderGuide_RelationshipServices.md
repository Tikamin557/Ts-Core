# 📖 Modder Guide

このガイドでは、Content Patcherから **T's Core** が提供する公開機能を使用する方法について説明します。

<a id="top"></a>

## ガイド一覧

- ✅ **Relationship Services** *(現在のページ)*
- 📄 [Location Services](ModderGuide_LocationServices.md)
- 📄 [Warp Services](ModderGuide_WarpServices.md)
- 📄 [Map Properties](ModderGuide_MapProperties.md)
- 📄 [Building Services](ModderGuide_BuildingServices.md)
- 📄 [BigCraftable Extension](ModderGuide_BigCraftableExtension.md)
- 📄 [Dialogue System](ModderGuide_DialogueSystem.md)
- 📄 [Migration System](ModderGuide_MigrationSystem.md)
- 📄 [Notification System](ModderGuide_NotificationSystem.md)
- 📄 [Content Patcher Integration](ModderGuide_ContentPatcherIntegration.md)
- 📄 [Post Renovation Patch](ModderGuide_PostRenovationPatch.md)
- 📄 [Polyamory Sweet Rooms Integration](ModderGuide_PolyamorySweetRoomsIntegration.md)
- 📄 [Other Features](ModderGuide_OtherFeatures.md)

← [READMEに戻る](../../README.md)

← [Modder Guideに戻る](ModderGuide.md)

---

# Relationship Services

Relationship Servicesは、プレイヤーの現在のパートナーを取得するための **Content Patcher Token** を提供します。

T's Coreが対応しているRelationship Systemを自動的に処理するため、システムごとに個別の互換パッチを用意しなくても、同じContent Packを使用できます。

---

## 目次

- [対応しているRelationship System](#supported-relationship-systems)
- [利用可能なToken](#available-tokens)
- [一般的な使用例](#common-examples)
- [どのTokenを使用すればいいですか？](#which-token-should-i-use)
- [一般的な用途](#common-use-cases)
- [デバッグ](#debugging)
- [注意事項](#notes)

---

<a id="supported-relationship-systems"></a>

## 対応しているRelationship System

| System | 対応 |
|--------|:---------:|
| Vanillaの結婚 | ✅ |
| Vanillaのルームメイト（Krobus） | ✅ |
| FreeLove | ✅ |
| PolyamorySweetLove | ✅ |

T's Coreは現在使用されているRelationship Systemを自動的に検出し、どのシステムが使用されている場合でも同じTokenを提供します。

今後T's Coreが他のRelationship Modへ対応した場合も、既存のContent Pack側を変更することなく自動的に互換性を得られます。

---

<a id="available-tokens"></a>

## 利用可能なToken

| Token | 戻り値 | 推奨用途 |
|-------|---------|-----------------|
| `{{Tikamin557.TsCore/Partners}}` | 現在のパートナー | 一般的なパートナー判定 |
| `{{Tikamin557.TsCore/OrderedPartners}}` | Spouse Roomの順序に並べた現在のパートナー | 部屋や順序に依存するPatch |
| `{{Tikamin557.TsCore/CanBeRomanced:<NPC>}}` | 指定したNPCが恋愛対象に設定されているか | NPCの恋愛可否判定 |

`Partners` と `OrderedPartners` は **パートナー名のリスト** を返し、Content Patcherの `Count`、`contains`、`valueAt` などの機能と組み合わせて使用できます。

`CanBeRomanced` はNPC名を入力として受け取り、`true` または `false` を返します。

例：

| Token | 値の例 |
|-------|---------------|
| `Partners` | `Abigail, Emily, Sebastian` |
| `OrderedPartners` | `Sebastian, Abigail, Emily` |
| `CanBeRomanced:Abigail` | `true` |

---

### Partners

**プレイヤーの現在のパートナーが誰なのか** だけを確認したい場合は、`Partners` を使用します。

```text
{{Tikamin557.TsCore/Partners}}
```

ほとんどのContent Packでは、このRelationship Tokenの使用を推奨します。

結果はリストとして返されるため、Content Patcherのリスト対応機能を直接使用できます。

| 用途 | 例 |
|---------|---------|
| パートナー数を数える | `{{Count:{{Tikamin557.TsCore/Partners}}}}` |
| パートナーが1人以上いるか確認する | `HasValue:{{Tikamin557.TsCore/Partners}}` |
| 特定のパートナーがいるか確認する | `Tikamin557.TsCore/Partners \|contains=Abigail` |
| 複数のパートナーがいるか確認する | `Count:{{Tikamin557.TsCore/Partners}} >= 2` |

#### プレイヤーにパートナーがいるか確認する

```json
"When": {
    "HasValue:{{Tikamin557.TsCore/Partners}}": "true"
}
```

`HasValue` は、そのTokenに現在何らかの値があるかを確認します。

パートナーが誰であるかに関係なく、プレイヤーに1人以上のパートナーがいる場合、この条件はtrueになります。

#### 特定のパートナーがいるか確認する

```json
"When": {
    "Tikamin557.TsCore/Partners |contains=Abigail": "true"
}
```

`contains` は指定した名前がリスト内のどこかに存在するかを確認するため、パートナーの順序は関係ありません。

---

### OrderedPartners

**Spouse Roomの順序における各パートナーの位置** が重要な場合は、`OrderedPartners` を使用します。

```text
{{Tikamin557.TsCore/OrderedPartners}}
```

`Partners` と同じリスト操作に対応していますが、返されるリストは現在使用されているSpouse Room Systemが提供する部屋の順序に従います。

これは、次のような用途に役立ちます。

- Spouse Roomの割り当て
- 特定の部屋に対するMap Patch
- Spouse Roomの位置に合わせた家具や装飾の配置
- 配偶者の順序に基づいたレイアウトの作成

#### 部屋の位置からパートナーを確認する

例えば、**Abigail** が2番目のパートナーかどうかを確認する場合：

```json
"When": {
    "Tikamin557.TsCore/OrderedPartners |valueAt=1": "Abigail"
}
```

`valueAt` は0から始まるインデックスを使用します。

| 位置 | Index |
|----------|------:|
| 1番目のパートナー | `0` |
| 2番目のパートナー | `1` |
| 3番目のパートナー | `2` |

---

### CanBeRomanced

特定のNPCが現在恋愛対象に設定されているかを確認するには、`CanBeRomanced` を使用します。

```text
{{Tikamin557.TsCore/CanBeRomanced:<NPC>}}
```

NPC名はTokenの入力値として指定します。

例：

```text
{{Tikamin557.TsCore/CanBeRomanced:Abigail}}
```

このTokenは次の値を返します。

| 値 | 説明 |
|-------|-------------|
| `true` | 指定したNPCは恋愛対象に設定されています。 |
| `false` | 指定したNPCが恋愛対象に設定されていない、またはNPCが見つかりませんでした。 |

この値は、Stardew Valleyの次のデータにあるNPCの `CanBeRomanced` 設定に基づきます。

```text
Data/Characters
```

つまり、このTokenにはContent Patcherによる `Data/Characters` の変更も含めた、現在のCharacter Dataが反映されます。

#### Content Patcherの条件

例：

```json
"When": {
    "Tikamin557.TsCore/CanBeRomanced:Willy": "true"
}
```

このPatchは、Willyが現在恋愛対象に設定されている場合に適用されます。

そのため、どのModがNPCの恋愛可否を変更したかを把握していなくても、同じContent Packから他Modによる変更に対応できます。

#### Dynamic Tokenの条件

`CanBeRomanced` はContent PatcherのDynamic Tokenの条件にも使用できます。

例：

```json
"DynamicTokens": [
    {
        "Name": "WillyFriendshipRequirement",
        "Value": "2500"
    },
    {
        "Name": "WillyFriendshipRequirement",
        "Value": "2000",
        "When": {
            "Tikamin557.TsCore/CanBeRomanced:Willy": "true"
        }
    }
]
```

この例では、Willyが恋愛対象に設定されている場合にDynamic Tokenで別の値を使用できます。

> **注意:** `CanBeRomanced` はNPCの現在のCharacter Dataを確認します。プレイヤーが現在そのNPCと交際中または結婚しているかどうかを確認するものではありません。

---

<a id="common-examples"></a>

## 一般的な使用例

同じ条件をVanillaのRelationshipと対応しているRelationship Modの両方で使用できます。

### プレイヤーにパートナーがいるか確認する

```json
"When": {
    "HasValue:{{Tikamin557.TsCore/Partners}}": "true"
}
```

### 特定のパートナーがいるか確認する

```json
"When": {
    "Tikamin557.TsCore/Partners |contains=Abigail": "true"
}
```

### 複数のパートナーがいるか確認する

```json
"When": {
    "Query": "Count:{{Tikamin557.TsCore/Partners}} >= 2"
}
```

Vanilla、FreeLove、PolyamorySweetLoveごとに個別の互換条件を用意する必要はありません。

---

<a id="which-token-should-i-use"></a>

## どのTokenを使用すればいいですか？

| 必要な処理 | 使用するToken |
|-------------------|-----|
| プレイヤーにパートナーがいるか確認する | `Partners` |
| 特定のパートナーがいるか確認する | `Partners` |
| 現在のパートナー数を数える | `Partners` |
| 複数配偶者に対応する | `Partners` |
| Spouse Roomの順序を判定する | `OrderedPartners` |
| 部屋の位置に応じてPatchを適用する | `OrderedPartners` |
| 部屋のIndexからパートナーを取得する | `OrderedPartners` |
| NPCが恋愛対象に設定されているか確認する | `CanBeRomanced:<NPC>` |

> > **推奨:** 一般的なパートナー判定には `Partners` を使用し、Content PackがSpouse Roomの順序に明確に依存する場合のみ `OrderedPartners` を使用してください。特定のNPCの恋愛可否設定を確認する必要がある場合は `CanBeRomanced` を使用してください。

> **推奨:** Content PackがSpouse Roomの順序に明確に依存する場合を除き、`Partners` を使用してください。

---

<a id="common-use-cases"></a>

## 一般的な用途

Relationship Servicesは、次のようなContent Packで役立ちます。

- 複数配偶者への互換対応
- カスタムSpouse Room
- NPCの恋愛可否に関する互換対応
- 結婚イベント
- Dialogueの条件
- 家具の表示条件
- 条件付きMap編集
- 条件付きData Patch

---

<a id="debugging"></a>

## デバッグ

次のSMAPIコマンドを使用して、Token関連の情報を確認できます。

| Command | 表示内容 |
|---------|----------|
| `tscore_tokens` | T's Coreが提供するすべてのToken関連情報 |
| `tscore_tokens_relationship` | Relationship Servicesの情報 |

Relationship Servicesだけを確認したい場合は、`tscore_tokens_relationship` を使用してください。

<details>
<summary>出力例</summary>

```text
tscore_tokens_relationship
[T's Core] ===== Relationship =====
[T's Core]
[T's Core]     Provider            : ApiMarriageProvider
[T's Core]     Description         : MarriageMod: ApryllForever.PolyamorySweetLove
[T's Core]     Room Mod            : Polyamory Sweet Rooms
[T's Core]     Partners (3)        : Abigail, Emily, Sebastian
[T's Core]     OrderedPartners (3) : Sebastian, Abigail, Emily
[T's Core]
[T's Core] ----- OrderedPartners Index -----
[T's Core]
[T's Core]     [0] Sebastian
[T's Core]     [1] Abigail
[T's Core]     [2] Emily
```

</details>

`OrderedPartners Index` に表示されるIndexは、`OrderedPartners` Tokenで使用する `valueAt` のIndexに直接対応しています。

---

### Content Patcher Content Packの再読み込み

T's Coreには、ゲーム実行中にContent Patcher Content Packを再読み込みするための開発用ツールも用意されています。

ゲームを再起動せずに、Patch、ConfigSchema、Config Token、GMCM設定、DynamicTokensを再読み込みできます。

`tscore_cp_reload` やその他のContent Patcher連携機能については、[Content Patcher Integration](ModderGuide_ContentPatcherIntegration.md)ガイドを参照してください。

---

<a id="notes"></a>

## 注意事項

Relationship Servicesは **読み取り専用** です。

結婚、友好度、交際関係、ルームメイト状態、NPCの恋愛可否を変更することはありません。

`Partners` と `OrderedPartners` は、プレイヤーの既存のRelationship情報を取得します。

`CanBeRomanced` は、`Data/Characters` から指定したNPCの現在の `CanBeRomanced` 設定を読み取ります。

`Data/Characters` はContent Patcherによって変更できるため、`CanBeRomanced` を使用することでNPCの恋愛可否を変更するModとの互換対応ができます。

---

## Modder Guide

- ← 前のページ *(なし)*
- ↑ [ガイド一覧](#top)
- → [Location Services](ModderGuide_LocationServices.md)

← [READMEに戻る](../../README.md)

← [Modder Guideに戻る](ModderGuide.md)
