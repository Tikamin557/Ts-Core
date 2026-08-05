# 📖 Modder Guide

This guide explains how to use the public features provided by **T's Core** in Content Patcher.

<a id="top"></a>

## Guide Index

- 📄 [Relationship Services](ModderGuide_RelationshipServices.md)
- 📄 [Location Services](ModderGuide_LocationServices.md)
- ✅ **Warp Services** *(Current Page)*
- 📄 [Notification System](ModderGuide_NotificationSystem.md) *(Coming Soon)*
- 📄 [Content Patcher Tokens](ModderGuide_ContentPatcherTokens.md) *(Coming Soon)*

← [Back to README](../README.md)

← [Back to Modder Guide](ModderGuide.md)

---

# Warp Services

Warp Services provide reusable warp functionality for Content Patcher.

They allow Content Packs to perform flexible warps using map names, registered warp providers, or magic warps without requiring C# code.

---

## Contents

- Warp Actions
- Warp Providers
- Built-in Warp Providers
- Examples
- Debugging
- Notes

---

## Warp Actions

Warp Services are built around two custom warp actions:

- `TsCoreWarp`
- `TsCoreMagicWarp`

Both actions support the following destination types:

- Built-in warp providers
- Registered custom warp providers
- Location names
- Location coordinates

The same syntax can be used with:

- Tile Actions
- Touch Actions
- Trigger Actions *(where supported)*

The only difference between the two actions is how the player is warped:

| Action | Description |
|--------|-------------|
| `TsCoreWarp` | Performs a standard warp without visual effects. |
| `TsCoreMagicWarp` | Performs the same warp using Stardew Valley's built-in magic warp animation and sound effect. |

> **Note:** When custom warp actions are used as Tile Actions, Stardew Valley normally logs a legacy **"unknown warp property"** warning to the SMAPI console. T's Core automatically suppresses this warning for `TsCoreWarp` and `TsCoreMagicWarp`, allowing Content Packs to use these actions without generating unnecessary console messages.

---

### Syntax

#### Warp using a provider

```text
TsCoreWarp <Provider>
TsCoreMagicWarp <Provider>
```

Example:

```text
TsCoreWarp FarmHouseFront
TsCoreMagicWarp FarmHouseFront
```

---

#### Warp using a provider and set the player's facing direction

```text
TsCoreWarp <Provider> <FacingDirection>
TsCoreMagicWarp <Provider> <FacingDirection>
```

Example:

```text
TsCoreWarp FarmHouseFront Left
TsCoreMagicWarp FarmHouseFront Left
```

---

#### Warp to a map

```text
TsCoreWarp <LocationName>
TsCoreMagicWarp <LocationName>
```

Example:

```text
TsCoreWarp BusStop
TsCoreMagicWarp BusStop
```

T's Core automatically uses the map's default warp point.

---

#### Warp to specific coordinates

```text
TsCoreWarp <LocationName> <X> <Y>
TsCoreMagicWarp <LocationName> <X> <Y>
```

Example:

```text
TsCoreWarp Farm 64 15
TsCoreMagicWarp Farm 64 15
```

This behaves exactly the same as the corresponding vanilla warp action.

If you do not need any T's Core-specific features, it is recommended to use the vanilla warp action instead.

---

#### Warp to specific coordinates and set the player's facing direction

```text
TsCoreWarp <LocationName> <X> <Y> <FacingDirection>
TsCoreMagicWarp <LocationName> <X> <Y> <FacingDirection>
```

Example:

```text
TsCoreWarp Farm 64 15 Down
TsCoreMagicWarp Farm 64 15 Down
```

---

### Facing Direction

The facing direction can be specified using either text or the corresponding numeric value.

| Direction | Numeric Value |
|----------|------:|
| `Up` | `0` |
| `Right` | `1` |
| `Down` | `2` |
| `Left` | `3` |

---

## Warp Providers

Warp Providers allow Content Packs to reference destinations by **provider name** instead of hardcoding map names and coordinates.

For example:

```text
TsCoreWarp FarmHouseFront
```

Instead of:

```text
Warp Farm 64 15
```

This makes Content Packs easier to maintain and improves compatibility with custom maps and other mods that change warp destinations.

T's Core automatically resolves the provider name to the correct destination.

---

### Why use Warp Providers?

Using warp providers has several advantages:

- No hardcoded coordinates
- Better compatibility with custom maps
- Easier maintenance
- More readable Content Packs

For example, if another mod changes the farmhouse entrance position, a provider such as `FarmHouseFront` can automatically resolve the correct destination without requiring changes to your Content Pack.

---

### Provider Resolution

When `TsCoreWarp` or `TsCoreMagicWarp` is executed, T's Core attempts to resolve the destination in the following order:

1. Built-in warp providers
2. Registered custom warp providers
3. Location name (fallback)

This means the following examples are both valid:

```text
TsCoreWarp FarmHouseFront
```

```text
TsCoreWarp BusStop
```

If no provider named `BusStop` exists, T's Core automatically treats it as a location name.

---

### Registering Custom Providers

In addition to the built-in providers, T's Core allows Content Packs to register their own custom Warp Providers.

This makes it possible to expose reusable warp destinations that can be shared across multiple Content Packs without writing any C# code.

---

### Content Pack Setup

Warp Providers can be added through a standard **T's Core Content Pack**.

A single Content Pack can include multiple T's Core features, such as custom Warp Providers and Notification Themes.

To create a T's Core Content Pack, configure your `manifest.json` as follows:

```json
{
  "Name": "My T's Core Content Pack",
  "Author": "YourName",
  "Version": "1.0.0",
  "UniqueID": "YourName.MyTsCorePack",
  "ContentPackFor": {
    "UniqueID": "Tikamin557.TsCore"
  }
}
```

Replace the example values with your own information before publishing your Content Pack.

---

### Folder Structure

```text
My T's Core Content Pack
├── manifest.json
└── assets
    ├── notification
    │   └── MyNotification.json
    └── warp
        └── MyWarpProvider.json
```

The `assets` folder can contain one or more feature-specific subfolders.

Currently, T's Core supports the following folders:

- `notification` — Custom Notification Themes
- `warp` — Custom Warp Providers

Only create the folders that your Content Pack actually uses.

The filenames are completely optional and may be chosen freely.

> **Note:** The folder names (`assets`, `notification`, `warp`, etc.) are fixed and must not be renamed. T's Core looks for JSON files in these specific folders when loading Content Packs. Renaming the folders will prevent the files from being detected.

---

## Warp Provider Definition

Each JSON file defines one Warp Provider.

The available provider types and their properties are explained in the following sections.

---

### Warp Provider (Type: Warp)

A **Warp** provider resolves its destination by reading an existing warp from a source location.

```json
{
  "Id": "FarmHouseFront",
  "Type": "Warp",
  "Source": "FarmHouse",
  "Target": "Farm"
}
```

| Property | Required | Description |
|----------|----------|-------------|
| `Id` | ✅ | Unique provider name used by `TsCoreWarp` and `TsCoreMagicWarp`. |
| `Type` | ✅ | Must be `"Warp"`. |
| `Source` | ✅ | The location that contains the warp to inspect. |
| `Target` | ✅ | The destination location of the warp that should be resolved. |

When this provider is used, T's Core searches the specified source location for a warp leading to the target location and uses its destination coordinates.

This allows the provider to automatically adapt when another mod changes the warp destination.

---

### Building Provider (Type: Building)

A **Building** provider resolves its destination based on the position of a building placed on the player's farm.

The following example shows the actual Building Provider used by the **[(SF) MonsterHouse](https://www.nexusmods.com/stardewvalley/mods/20586)** mod.

```json
{
  "Id": "MonsterHouseFront",
  "Type": "Building",
  "BuildingType": "Tikamin557.SF.MonsterHouse.Buildings_MonsterHouse",
  "OffsetX": 0,
  "OffsetY": 1,
  "Fallback": "FarmHouseFront"
}
```

| Property | Required | Description |
|----------|----------|-------------|
| `Id` | ✅ | Unique provider name used by `TsCoreWarp` and `TsCoreMagicWarp`. |
| `Type` | ✅ | Must be `"Building"`. |
| `BuildingType` | ✅ | The building type to search for on the player's farm. |
| `OffsetX` | ✅ | Horizontal offset from the building's top-left tile. |
| `OffsetY` | ✅ | Vertical offset from the building's top-left tile. |
| `Fallback` | Optional | Provider to use if the building cannot be found. Defaults to `FarmHouseFront`. |

T's Core searches the player's farm for a building with the specified `BuildingType`.

If the building is found, the destination is calculated by adding the configured offsets to the building's top-left tile.

For example, the Monster House occupies a **2 × 1** area.

Using:

```json
"OffsetX": 0,
"OffsetY": 1
```

produces the following destination:

```text
■■
□
↑ Warp destination
```

The warp destination is one tile directly below the building's top-left tile.

If the building cannot be found, the provider specified by `Fallback` is used instead.

> **Tip:** The easiest way to find a building's internal type is by using the `tscore_debug_buildings` debug command.

---

## Built-in Warp Providers

T's Core includes several built-in warp providers.

These providers resolve their destinations dynamically at runtime, allowing Content Packs to avoid hardcoded coordinates.

Each provider resolves its destination using an existing warp from the source location.

As a result, Content Packs remain compatible with custom maps and mods that move buildings or change their entrance positions.

---

### Available Providers

| Provider | Source | Target | Status |
|----------|--------|--------|--------|
| `FarmHouseFront` | `FarmHouse` | `Farm` | ✅ Available |
| `GreenhouseFront` | `Greenhouse` | `Farm` | ✅ Available |
| `FarmCaveFront` | `FarmCave` | `Farm` | ✅ Available |
| `IslandFarmHouseFront` | `IslandFarmHouse` | `IslandWest` | ✅ Available |

---

### FarmHouseFront

Resolves the destination of the warp from `FarmHouse` to `Farm`.

This normally corresponds to the tile directly outside the player's farmhouse entrance.

Because the destination is read from the active map warp, it can adapt to:

- Vanilla farm layouts
- Custom farm maps
- Mods that move the farmhouse
- Mods that change the farmhouse entrance warp

#### Example

```text
TsCoreWarp FarmHouseFront
```

```text
TsCoreMagicWarp FarmHouseFront
```

---

### GreenhouseFront

Resolves the destination of the warp from `Greenhouse` to `Farm`.

This normally corresponds to the tile directly outside the greenhouse entrance.

Because the destination is read from the active map warp, it can adapt to custom farm layouts and mods that move or replace the greenhouse.

#### Example

```text
TsCoreWarp GreenhouseFront
```

```text
TsCoreMagicWarp GreenhouseFront
```

---

### FarmCaveFront

Resolves the destination of the warp from `FarmCave` to `Farm`.

This normally corresponds to the tile directly outside the farm cave entrance.

Because the destination is read from the active map warp, it can adapt to custom farm layouts and mods that move the farm cave entrance.

#### Example

```text
TsCoreWarp FarmCaveFront
```

```text
TsCoreMagicWarp FarmCaveFront
```

---

### IslandFarmHouseFront

Resolves the destination of the warp from `IslandFarmHouse` to `IslandWest`.

This normally corresponds to the tile directly outside the Island Farmhouse entrance on Ginger Island.

Because the destination is read from the active map warp, it can adapt to mods that change the Island Farmhouse or the surrounding `IslandWest` map.

#### Example

```text
TsCoreWarp IslandFarmHouseFront
```

```text
TsCoreMagicWarp IslandFarmHouseFront
```

---

> **Note:** These providers depend on the corresponding source and destination locations having a valid warp between them. If another mod removes or replaces that warp without providing an equivalent destination, the provider may not be able to resolve its target.

Additional built-in warp providers may be added in future versions of T's Core.


---

## Examples

### Warp to the farmhouse entrance

The following example adds a Tile Action to the front of the fountain in **Pelican Town**.

```json
{
    "Action": "EditMap",
    "Target": "Maps/Town",
    "MapTiles": [
        {
            "Position": { "X": 26, "Y": 28 },
            "Layer": "Buildings",
            "SetProperties": {
                "Action": "TsCoreWarp FarmHouseFront"
            }
        }
    ]
}
```

---

### Magic warp to the farmhouse entrance and face down

The following example performs the same warp using **TsCoreMagicWarp**.

After arriving at the farmhouse entrance, the player will automatically face **down**.

```json
{
    "Action": "EditMap",
    "Target": "Maps/Town",
    "MapTiles": [
        {
            "Position": { "X": 26, "Y": 28 },
            "Layer": "Buildings",
            "SetProperties": {
                "Action": "TsCoreMagicWarp FarmHouseFront Down"
            }
        }
    ]
}
```

This example also demonstrates how to specify the player's facing direction after warping.

---

## Debugging

Warp Services can be inspected using the following SMAPI command:

```text
tscore_debug_warp
```

The following example shows the built-in providers included with T's Core along with a custom provider registered by a Content Pack.

Example output:

```text
tscore_debug_warp
[T's Core] ===== Warp Providers =====
[T's Core] Registered Providers: 5
[T's Core]
[T's Core] ----- T's Core -----
[T's Core]
[T's Core] FarmCaveFront
[T's Core]     Type                : Warp
[T's Core]     Source              : FarmCave
[T's Core]     Target              : Farm
[T's Core]
[T's Core] FarmHouseFront
[T's Core]     Type                : Warp
[T's Core]     Source              : FarmHouse
[T's Core]     Target              : Farm
[T's Core]
[T's Core] GreenhouseFront
[T's Core]     Type                : Warp
[T's Core]     Source              : Greenhouse
[T's Core]     Target              : Farm
[T's Core]
[T's Core] IslandFarmHouseFront
[T's Core]     Type                : Warp
[T's Core]     Source              : IslandFarmHouse
[T's Core]     Target              : IslandWest
[T's Core]
[T's Core] ----- Tikamin557.TsC.MonsterHouse -----
[T's Core]
[T's Core] MonsterHouseFront
[T's Core]     Type                : Building
[T's Core]     Building            : Tikami557.SF.MonsterHouse.Buildings_MonsterHouse
[T's Core]     Offset              : (0, 1)
[T's Core]     Fallback            : FarmHouseFront
```

---

## Notes

Warp Services are fully compatible with Content Patcher and require no C# code.

Whenever possible, it is recommended to use warp providers instead of hardcoded map names or coordinates to maximize compatibility with other mods.

Additional warp providers and features may be added in future versions of T's Core without requiring changes to existing Content Packs.

---

## Modder Guide

- ← [Location Services](ModderGuide_LocationServices.md)
- ↑ [Guide Index](#top)
- → [Notification System](ModderGuide_NotificationSystem.md) *(Coming Soon)*

← [Back to README](../README.md)

← [Back to Modder Guide](ModderGuide.md)
