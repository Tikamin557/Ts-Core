# Custom Recipe Pages（専用レシピページ）

← [Modder Guide](ModderGuide.md) | [README](../README.md)

T's Core 1.9.9では、クラフト・料理レシピを**バニラとは別の専用ページ**に表示するContent Patcher用データアセット `TsCore/CustomRecipePages` を追加しました。未登録レシピはバニラページに残ります。登録レシピがある場合は**V/T切り替えボタン**を表示します。Better Craftingのレシピ画面・作業台・検索にも対応します。

## Content Patcherでの登録例

`content.json` の `Changes` に追加します。

```json
{
  "Action": "EditData",
  "Target": "TsCore/CustomRecipePages",
  "Entries": {
    "Example.Mod_Crafting": {
      "RecipeType": "Crafting",
      "DisplayOrder": 0,
      "DedicatedRows": true,
      "RecipeIds": ["Wood Fence", "Stone Fence"]
    },
    "Example.Mod_Cooking": {
      "RecipeType": "Cooking",
      "DisplayOrder": 5,
      "DedicatedRows": false,
      "RecipeIds": ["Fried Egg"]
    }
  }
}
```

`RecipeIds` には**`Data/CraftingRecipes` / `Data/CookingRecipes` に登録されている実際のレシピID**を指定します。翻訳後の表示名ではありません。上記は例なので、自分のModのIDに置き換えてください。`Entries` のキーはグループIDです。

## プロパティ

| プロパティ | 省略時 | 説明 |
|---|---|---|
| `RecipeType` | `Crafting` | `Crafting` または `Cooking`（大文字小文字を区別しない）。 |
| `DisplayOrder` | `10` | グループの順序。0～10に制限し、小さい順に表示。 |
| `DedicatedRows` | `false` | グループを専用行に配置。 |
| `RecipeIds` | `[]` | グループ内のレシピIDと表示順。 |

グループは `DisplayOrder` の昇順で処理され、同じ値の場合は**グループIDのOrdinal昇順**になります。同じレシピIDが複数グループにある場合は、最初のグループが優先されます。存在しないIDは無視され、警告ログが出ます。未習得のレシピは表示されませんが、専用ページに登録されたIDはバニラページから除外されます。

V/Tボタンはプレイヤー用のページ切り替え操作です。インベントリや作成操作はバニラの仕組みを利用します。Better Craftingへの対応は互換処理として実装されています。

> **バージョン上の注意：** 旧実験機能の `TsCore/RecipeLayouts` は**サポートされません**。1.9.9では `TsCore/CustomRecipePages` を使用してください。
