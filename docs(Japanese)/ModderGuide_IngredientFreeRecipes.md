# Ingredient-Free Recipes（素材不要レシピ）

← [Modder Guide](ModderGuide.md) | [README](../README.md)

T's Core 1.9.9では、**必要素材が0個のクラフト・料理レシピ**を作成できます。バニラのクラフト・料理画面とBetter Craftingのツールチップ表示に対応します。

## レシピデータへの指定

`Data/CraftingRecipes` または `Data/CookingRecipes` のレシピ値のうち、**最初の `/` より前にある素材欄**を以下のいずれかに置き換えます。

| マーカー | 動作 |
|---|---|
| `TsCoreNullRecipeIngredients_Text` | 素材不要として扱い、材料欄に「素材不要」を表示。 |
| `TsCoreNullRecipeIngredients_Hide` | 素材不要として扱い、材料欄を非表示。 |
| `TsCoreNullRecipeIngredients` | `_Text` と同じ表示。 |

`/` で区切られた残りのフィールド（完成品や解放条件など）は、通常のゲームデータ形式を維持してください。これらは**レシピIDではなく素材欄の値**です。追加のJSONキーでもありません。

## 設定例

Content Patcherで既存のレシピの素材欄を変更するイメージです。

```json
{
  "Action": "EditData",
  "Target": "Data/CraftingRecipes",
  "Fields": {
    "ExampleRecipeId": {
      "0": "TsCoreNullRecipeIngredients_Text"
    }
  }
}
```

上記は先頭フィールドを変更する意図を示した例です。使用中のContent Patcherの仕様と対象レシピの実データに合わせて編集してください。残りのフィールドを消さないでください。素材欄をマーカーにした完全なレシピ値を登録する方法でも使用できます。

T's Coreは対象レシピの素材リストを空にし、クラフト画面上で作成可能として扱います。完成品や所持品の処理は通常どおりです。`_Text` はT's Coreの翻訳を使い、`_Hide` は素材欄を非表示にします。Better Craftingでも素材不要レシピのツールチップ表示に対応します。
