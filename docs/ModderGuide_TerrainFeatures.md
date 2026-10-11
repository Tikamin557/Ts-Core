# Terrain Features Management (v1.9.5)

T's Core includes an in-game **Terrain Features** management menu. This is a player-facing utility, not a new Content Patcher data asset or public API.

## Opening the menu

- Load a save and press the **Terrain Features Menu** key (default: `L`). The key can be changed or disabled in T's Core's Generic Mod Config Menu.
- Alternatively, assign **Terrain Features** from **Mod Features** in T's Core's Shortcut Panel.
- The menu is available to the host/main player while a save is loaded and no other menu is open.

## Selecting an area and targets

Choose either the **current location** or **all outdoor locations plus the current location**. The second option can affect many maps, so check the scope before proceeding.

You can select individual targets or use the **All**, **Resource Clumps**, **Objects**, and **Terrain Features** group options. The 17 target types are:

| Group | Targets |
|---|---|
| Terrain features | Bush, Tree, Grass, Path, Crop, Tilled Soil |
| Objects | Weeds, Twig, Rock, Forage, Fence |
| Resource clumps | Stump, Log, Boulder |
| Group selections | Resource Clumps, Objects, Terrain Features |

**All** selects all 17 entries, including the group selections.

The selected targets and area scope are saved in T's Core's `TerrainFeaturesConfig.json` and restored when the menu is reopened.

## Operations

| Operation | Behavior |
|---|---|
| **Clear** | Removes selected features and objects from the chosen location(s). |
| **Generate** | Attempts to add selected features from a newly generated source location, only where placement is possible. |
| **Reset** | Clears selected targets, then restores their initial/generated state from a newly generated source location. |

Each operation requires confirmation before it runs. A result message reports successful and failed locations.

> **Caution:** Clear and Reset can remove existing terrain features, objects, crops, or placed items depending on the targets selected. Save your game before using them. Generate and Reset depend on whether T's Core can create a suitable source location; they may fail for some custom maps. This feature does not guarantee restoration of manually placed items.

## For mod authors

This feature is intended for players and is not currently exposed as a documented Content Patcher registration asset or public C# API. Content Packs don't need to register anything to use the menu.

← [Back to Modder Guide](ModderGuide.md)

← [Back to README](../README.md)
