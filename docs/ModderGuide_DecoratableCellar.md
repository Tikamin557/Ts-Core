# Decoratable Cellar

T's Core 2.0.2 lets Content Patcher authors enable wallpaper and flooring customization in an existing `Cellar` location without changing its location type.

← [Back to Modder Guide](ModderGuide.md)

## Enable the feature

Set the **map property** `TsCoreDecoratableCellar` to `T` on a map used by a `Cellar`. Other location types are not supported.

## Define regions

| Tile property | Layer | Description |
|---|---|---|
| `WallID` | `Back` or `Buildings` | Wallpaper region identifier |
| `FloorID` | `Back` | Flooring region identifier |

Tiles with the same region ID are decorated together. Wallpaper uses a three-row arrangement (two rows on `Back`, one on `Buildings`), replacing only eligible tiles. Flooring uses a 2×2 pattern aligned to the top-left of the `FloorID` region.

Optionally specify region IDs and initial material IDs with the **map properties** `WallIDs` and `FloorIDs`:

```text
WallIDs = Wall_1 0, Wall_2 1
FloorIDs = Floor_1 0, Floor_2 1
```

Entries are separated by commas. Each entry contains a region ID and an optional material ID separated by whitespace. The numbers shown are examples. Regions with tile properties can be detected even without an entry in these lists.

## Materials

Players can use ordinary wallpaper and flooring items. Both vanilla materials and sets added through `Data/AdditionalWallpaperFlooring` are supported.

Material IDs use `0`, `1`, etc. for vanilla materials, or `SetId:Index` for additional sets (e.g. `Shen.Walls:0` and `Shen.Floors:0` when installed). Additional materials require their content pack to provide the corresponding textures. T's Core registers additional tilesheets on the Cellar map when needed.

## Persistence

Selected materials are stored separately for walls and floors in each Cellar's `modData`. Selections are reapplied after loading and supported map reloads. Saved selections take precedence over map-defined initial materials.

Keep region IDs consistent across map updates so saved selections remain associated with the intended regions. This feature preserves the original `Cellar` location type.

← [Back to Modder Guide](ModderGuide.md)
