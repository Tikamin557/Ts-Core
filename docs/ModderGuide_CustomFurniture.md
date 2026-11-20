# Custom Furniture (v2.0.7)

← [Back to Modder Guide](ModderGuide.md)

T's Core extends furniture defined in Stardew Valley's `Data/Furniture` using the Content Patcher data asset `TsCore/CustomFurniture`. Definitions are keyed by the **exact furniture Item ID**, not by their display names. Requires T's Core 2.0.7 or later.

## Content Patcher example

Add a normal `EditData` patch to your Content Pack's `content.json`:

```json
{
  "Action": "EditData",
  "Target": "TsCore/CustomFurniture",
  "Entries": {
    "YourModId_CustomTV": {
      "Type": "TV",
      "Description": "A custom television.\nRight-click to change the stand.",
      "ScreenArea": { "X": 4, "Y": 3, "Width": 16, "Height": 12 },
      "ScreenFit": "Contain",
      "RotationVariants": {
        "Enabled": true,
        "Count": 3,
        "Columns": 3,
        "ScreenAreas": {
          "2": { "X": 5, "Y": 3, "Width": 16, "Height": 12 }
        }
      },
      "TileProperties": {
        "0,0": { "Interaction": "Table", "ItemOffset": "0, -2" },
        "1,0": { "Interaction": "TV" }
      }
    }
  }
}
```

The example assumes that `YourModId_CustomTV` is already registered in `Data/Furniture` with a compatible TV definition and that its texture contains three adjacent same-sized sprites. **Adjust all pixel positions, tile coordinates, and IDs for your actual furniture.** The example does not itself create the `Data/Furniture` entry or texture.

## Properties

| Property | Meaning |
| --- | --- |
| `Type` | `TV` or `Furniture` (default: `TV`). Use `TV` for television-screen rendering. |
| `Description` | Optional furniture tooltip description. Supports `\n`; text wraps automatically. |
| `ScreenArea` | For `TV`, required screen rectangle: `X`, `Y`, `Width`, `Height`, measured in **source texture pixels** from the sprite's upper-left corner. Positive dimensions required. |
| `ScreenFit` | `Stretch` (default; fill without preserving aspect ratio), `Contain` (preserve aspect ratio; fit inside), or `Cover` (preserve aspect ratio; fill and crop). |
| `RotationVariants` | Optional appearance changes using the furniture rotate action, without changing the collision area. |
| `TileProperties` | Optional interactions and actions assigned to individual collision tiles. |

### RotationVariants

- `Enabled`: `true` enables the feature.
- `Count`: number of variants, from **2 to 64** when enabled.
- `Columns`: number of sprite columns per row, from 1 to `Count`; defaults to `Count` (a single row).
- `ScreenAreas`: optional per-variant TV screen rectangles keyed by **1-based variant numbers** (`"1"`, `"2"`, etc.). Unspecified variants use the shared `ScreenArea`.
- Sprites must be the same size and arranged in the specified row/column grid. Rotation changes the displayed sprite, not the furniture's collision dimensions. The selected variant is stored with the furniture.

### TileProperties

Keys are **relative collision tile coordinates** in the form `"X,Y"` (for example, `"0,0"` or `"1,0"`). Values can include:

- `Action`: a map-style action string to execute when interacting with that tile, such as an action opening a custom shop. Use an action supported by the game or your installed mods.
- `Interaction`: `TV` for normal television interaction or `Table` to allow placing/retrieving an item on a tile of a TV. The table interaction uses the game's single `heldObject` slot, not an independent inventory per tile.
- `ItemOffset`: optional `"X, Y"` source-pixel adjustment to the position of the item drawn on a `Table` tile (default `"0, 0"`).

> **Important:** These settings extend existing `Data/Furniture` definitions. `TV` functionality depends on a compatible vanilla TV furniture instance. Do not assume that every furniture type supports TV behavior. When a TV table interaction is disabled, remove any placed item first so it remains retrievable.

## Related guide

To replace older furniture IDs in existing saves, see [Migration System — Furniture Migration](ModderGuide_MigrationSystem.md#furniture-migration-v207).
