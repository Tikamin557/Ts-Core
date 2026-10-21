# Custom Recipe Pages

← [Modder Guide](ModderGuide.md) | [README](../README.md)

T's Core 1.9.9 adds `TsCore/CustomRecipePages`, a Content Patcher data asset that places selected **crafting and cooking recipes on a separate T's Core page**. Unassigned recipes remain on the vanilla page. A **V/T toggle** switches between the pages when registered recipes are available. Better Crafting integration also supports its recipe display, workbenches, and search.

## Content Patcher example

Add this entry to the `Changes` array in `content.json`:

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

The strings in `RecipeIds` must be **actual recipe IDs** from `Data/CraftingRecipes` or `Data/CookingRecipes`, not localized display names. The examples above are illustrative: use the IDs from your own data. The `Entries` keys are unique **group IDs**.

## Properties

| Property | Default | Description |
|---|---|---|
| `RecipeType` | `Crafting` | `Crafting` or `Cooking` (case-insensitive). |
| `DisplayOrder` | `10` | Group order, clamped to 0–10; lower values appear first. |
| `DedicatedRows` | `false` | Start and end this group on dedicated recipe rows. |
| `RecipeIds` | `[]` | Recipe IDs in the order they should appear within the group. |

Groups are processed in ascending `DisplayOrder`, then **ordinal group ID order** when tied. The first group to claim a recipe ID wins; duplicate IDs in later groups are skipped. Unknown IDs are ignored and logged as warnings. Recipes the player has not learned are not shown, but assigned IDs are still reserved for the custom page rather than shown on the vanilla page.

The V/T toggle is a player-facing control. T's Core uses the vanilla crafting interface for inventory and crafting actions; the feature is not a new crafting engine. Better Crafting support is implemented as compatibility with that mod, not a separate Content Patcher setting.

> **Version note:** The old experimental `TsCore/RecipeLayouts` asset is **not supported**. Use `TsCore/CustomRecipePages` in 1.9.9.
