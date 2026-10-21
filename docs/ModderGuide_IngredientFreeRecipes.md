# Ingredient-Free Recipes

← [Modder Guide](ModderGuide.md) | [README](../README.md)

T's Core 1.9.9 supports crafting and cooking recipes with **no required ingredients**. This works with the vanilla crafting/cooking menus and includes Better Crafting tooltip compatibility.

## Recipe data

In a `Data/CraftingRecipes` or `Data/CookingRecipes` recipe value, replace the **ingredient field (the part before the first `/`)** with one of the following markers:

| Marker | Behavior |
|---|---|
| `TsCoreNullRecipeIngredients_Text` | No ingredients are consumed; display a "No ingredients required" message in the ingredient area. |
| `TsCoreNullRecipeIngredients_Hide` | No ingredients are consumed; hide the ingredient area. |
| `TsCoreNullRecipeIngredients` | Same display behavior as `_Text`. |

Keep the remaining slash-separated recipe fields (output item, unlock rules, etc.) in their usual game data format. These markers are **ingredient-field values**, not recipe IDs and not additional JSON keys.

## Example

A schematic Content Patcher edit looks like this:

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

This shows the intended change to the first slash-separated field; adapt the patch to your Content Patcher version and the actual recipe data. Do not overwrite the other recipe fields. You can also provide a complete recipe value with the marker as its first field.

For these recipes, T's Core clears the recipe's ingredient list and makes the crafting UI treat the recipe as craftable. Normal output and inventory handling still apply. In `_Text` mode, the message is translated by T's Core; `_Hide` mode omits the ingredient section. Better Crafting receives compatible ingredient-free tooltip presentation.
