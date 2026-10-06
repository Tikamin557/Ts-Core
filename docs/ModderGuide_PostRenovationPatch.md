# 📖 Modder Guide

This guide explains how to use the public features provided by **T's Core** in Content Patcher.

<a id="top"></a>

## Guide Index

- 📄 [Relationship Services](ModderGuide_RelationshipServices.md)
- 📄 [Location Services](ModderGuide_LocationServices.md)
- 📄 [Warp Services](ModderGuide_WarpServices.md)
- 📄 [Map Properties](ModderGuide_MapProperties.md)
- 📄 [Building Services](ModderGuide_BuildingServices.md)
- 📄 [BigCraftable Extension](ModderGuide_BigCraftableExtension.md)
- 📄 [Dialogue System](ModderGuide_DialogueSystem.md)
- 📄 [Migration System](ModderGuide_MigrationSystem.md)
- 📄 [Notification System](ModderGuide_NotificationSystem.md)
- 📄 [Content Patcher Integration](ModderGuide_ContentPatcherIntegration.md)
- 📄 [Position Picker](ModderGuide_PositionPicker.md)
- ✅ **Post Renovation Patch** *(Current Page)*
- 📄 [Polyamory Sweet Rooms Integration](ModderGuide_PolyamorySweetRoomsIntegration.md)
- 📄 [Other Features](ModderGuide_OtherFeatures.md)

← [Back to README](../README.md)

← [Back to Modder Guide](ModderGuide.md)

---

# Post Renovation Patch

Post Renovation Patch is a data-driven map patching system for FarmHouse maps.

Normal Content Patcher map edits can be overwritten when Stardew Valley later rebuilds the FarmHouse layout and applies runtime changes such as renovations. T's Core can apply registered map patches after `FarmHouse.updateFarmLayout()` has completed, allowing those patches to be applied to the resulting FarmHouse map.

Post Renovation Patches are registered through:

```text
TsCore/PostRenovationPatches
```

No C# code is required.

---

## Contents

- [When to Use This System](#when-to-use-this-system)
- [Data Asset](#data-asset)
- [Properties](#properties)
- [Patch Modes](#patch-modes)
- [MapTiles](#maptiles)
- [Multiple Targets](#multiple-targets)
- [Basic Example](#basic-example)
- [FromArea and ToArea](#fromarea-and-toarea)
- [Map Size and Layers](#map-size-and-layers)
- [Updating Patches](#updating-patches)
- [Notes](#notes)

---

## When to Use This System

Use Post Renovation Patch when a map edit must remain applied after Stardew Valley rebuilds the FarmHouse layout.

A common example is a staircase, doorway, or other map addition which overlaps an area affected by a FarmHouse renovation. A normal Content Patcher `EditMap` patch may be applied before the runtime FarmHouse layout changes and then be overwritten.

Post Renovation Patch is intended for this type of FarmHouse-specific runtime map editing.

---

## Data Asset

Register patches by editing:

```text
TsCore/PostRenovationPatches
```

Each entry key is a unique ID for the patch.

Example:

```json
{
  "Action": "EditData",
  "Target": "TsCore/PostRenovationPatches",
  "Entries": {
    "{{ModId}}_ExamplePatch": {
      "Target": "Maps/FarmHouse2",
      "FromFile": "Maps/ExamplePatch",
      "ToArea": {
        "X": 10,
        "Y": 20,
        "Width": 4,
        "Height": 4
      },
      "PatchMode": "Overlay"
    }
  }
}
```

The `Target` and `FromFile` values are game content asset names.

The source map can therefore be loaded into the game content pipeline with Content Patcher and referenced from the Post Renovation Patch definition.

---

## Properties

| Property | Required | Description |
|----------|----------|-------------|
| Entry key | ✅ | Unique ID for this Post Renovation Patch entry. |
| `Target` | ✅ | Game content asset name of the FarmHouse map this patch should apply to. Multiple targets can be specified as a comma-separated list. |
| `FromFile` | △ | Game content asset name of the source map used for the patch. Required when `MapTiles` is not specified. |
| `MapTiles` | △ | Changes properties on existing map tiles after the FarmHouse layout update. Required when `FromFile` is not specified. |
| `FromArea` | ❌ | Rectangle to copy from the source map. If omitted, the full source map is used. |
| `ToArea` | ❌ | Destination rectangle in the target map. If omitted, the patch starts at `0, 0` using the size of `FromArea`. |
| `PatchMode` | ❌ | Map patch mode. Defaults to `Overlay`. |

`FromFile` and `MapTiles` can also be used together. At least one of them must contain a patch operation.

`FromArea` and `ToArea` must have the same width and height.

---

## Patch Modes

The following SMAPI `PatchMapMode` values are supported:

| Value | Description |
|-------|-------------|
| `Overlay` | Copies non-empty source tiles over the target map. |
| `ReplaceByLayer` | Replaces the destination area on layers represented by the source map, including empty source tiles. |
| `Replace` | Replaces the destination area across the map, including clearing tiles on target-only layers within that area. |

If `PatchMode` is omitted, `Overlay` is used.

---

## MapTiles

`MapTiles` can be used to set properties on existing tiles after the FarmHouse layout update. This is useful when a renovation may overwrite tile properties such as `Action` or `NoFurniture`.

The following fields are supported:

| Property | Required | Description |
|----------|----------|-------------|
| `Position` | ✅ | Tile coordinates in the target map. |
| `Layer` | ✅ | Name of the layer containing the tile. |
| `SetProperties` | ❌ | Properties to set on the existing tile. |

Example:

```json
{
  "Action": "EditData",
  "Target": "TsCore/PostRenovationPatches",
  "Entries": {
    "{{ModId}}_KitchenFix": {
      "Target": "Maps/FarmHouse2",
      "MapTiles": [
        {
          "Position": { "X": 38, "Y": 23 },
          "Layer": "Buildings",
          "SetProperties": { "Action": "kitchen" }
        },
        {
          "Position": { "X": 41, "Y": 24 },
          "Layer": "Back",
          "SetProperties": { "NoFurniture": "T" }
        }
      ]
    }
  }
}
```

`MapTiles` does not create a tile. The specified layer and tile must already exist at the given position. Invalid layers, out-of-range positions, or empty tile positions are logged as patch errors.

`MapTiles` can be used by itself without `FromFile`, or together with a normal `FromFile` map patch in the same entry.

---

## Multiple Targets

`Target` can contain multiple FarmHouse map asset names separated by commas. The same Post Renovation Patch is applied when the FarmHouse's current map asset matches any of them.

Example:

```json
"Target": "Maps/FarmHouse2, Maps/FarmHouse2_marriage"
```

Spaces around each comma-separated target are ignored. A single target continues to work as before.

---

## Basic Example

First, load the source map as a game content asset:

```json
{
  "Action": "Load",
  "Target": "Maps/Example_PostRenovationPatch",
  "FromFile": "assets/ExamplePatch.tmx"
}
```

Then register the Post Renovation Patch:

```json
{
  "Action": "EditData",
  "Target": "TsCore/PostRenovationPatches",
  "Entries": {
    "{{ModId}}_ExamplePatch": {
      "Target": "Maps/FarmHouse2",
      "FromFile": "Maps/Example_PostRenovationPatch",
      "ToArea": {
        "X": 10,
        "Y": 20,
        "Width": 4,
        "Height": 4
      },
      "PatchMode": "Overlay"
    }
  }
}
```

When the target FarmHouse layout is rebuilt, T's Core applies the registered patch after the FarmHouse layout update has completed.

---

## FromArea and ToArea

`FromArea` can be used when only part of the source map should be copied.

```json
"FromArea": {
  "X": 4,
  "Y": 8,
  "Width": 6,
  "Height": 5
}
```

`ToArea` controls where that area is placed in the target map:

```json
"ToArea": {
  "X": 20,
  "Y": 12,
  "Width": 6,
  "Height": 5
}
```

The two rectangles must have matching dimensions.

If `FromArea` is omitted, the full source map is used.

If `ToArea` is omitted, the patch is placed at the top-left of the target map using the source area's width and height.

---

## Map Size and Layers

If the destination area extends beyond the current target map size, T's Core expands the target map so the patch can fit.

Layers which exist in the source map but not in the target map are added to the target map as needed.

T's Core also maps the source map's tile sheets into the target map so tiles from the source map can be used by the applied patch.

---

## Updating Patches

`TsCore/PostRenovationPatches` is a normal game content asset and can be edited through Content Patcher conditions.

When the asset is invalidated, T's Core rebuilds affected FarmHouse maps and reapplies the currently active Post Renovation Patches.

This allows patches to be added, removed, or changed when Content Patcher updates the Data Asset.

During development, `tscore_cp_reload <ContentPackId>` can be used to reload a Content Pack which edits this Data Asset.

---

## Notes

- Post Renovation Patch is specifically applied to `FarmHouse` instances.
- `Target` is matched against the FarmHouse's current map asset. Multiple targets can be separated with commas.
- At least one of `FromFile` or `MapTiles` must be specified. They can also be used together.
- `FromFile` must reference a map available through the game content pipeline.
- `MapTiles` currently supports `Position`, `Layer`, and `SetProperties`. The specified tile must already exist.
- `FromArea` must stay within the source map bounds.
- `FromArea` and `ToArea` must have the same dimensions.
- `ToArea` cannot use negative coordinates.
- The default `PatchMode` is `Overlay`.
- Errors while applying an individual patch are logged to the SMAPI console.
- Content Patcher conditions should be applied to the `EditData` patch which adds or changes the entry in `TsCore/PostRenovationPatches`.

---

## Modder Guide

- ← [Content Patcher Integration](ModderGuide_ContentPatcherIntegration.md)
- ↑ [Guide Index](#top)
- → [Polyamory Sweet Rooms Integration](ModderGuide_PolyamorySweetRoomsIntegration.md)

← [Back to README](../README.md)

← [Back to Modder Guide](ModderGuide.md)
