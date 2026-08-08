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

T's Core provides custom Warp Actions and reusable Warp Providers, and also allows Content Packs to register their own destinations without requiring any C# code.

This guide explains how to use Warp Actions, create custom Warp Providers, and integrate them into your Content Packs.

---

## Contents

- Warp Actions
- Warp Providers
- Content Pack Setup
- Warp Provider Types
- Built-in Warp Providers
- Examples
- Debugging
- Notes

---

## Warp Actions

T's Core provides two custom warp actions:

| Action | Description |
|--------|-------------|
| `TsCoreWarp` | Performs a standard warp without visual effects. |
| `TsCoreMagicWarp` | Performs the same warp using Stardew Valley's built-in magic warp animation and sound effect. |

Both actions support:

- Built-in Warp Providers
- Custom Warp Providers
- Location names
- Direct coordinates
- Optional facing direction

The same syntax can be used with:

- Tile Actions
- Touch Actions
- Trigger Actions *(where supported)*

> **Note:** When custom warp actions are used as Tile Actions, Stardew Valley normally logs a legacy **"unknown warp property"** warning to the SMAPI console. T's Core automatically suppresses this warning for `TsCoreWarp` and `TsCoreMagicWarp`.

---

### Syntax

`TsCoreWarp` and `TsCoreMagicWarp` use the same syntax.

| Destination | Syntax | Example |
|-------------|--------|---------|
| Provider | `<Action> <Provider>` | `TsCoreWarp FarmHouseFront` |
| Provider + facing direction | `<Action> <Provider> <FacingDirection>` | `TsCoreMagicWarp FarmHouseFront Left` |
| Location | `<Action> <LocationName>` | `TsCoreWarp BusStop` |
| Location + coordinates | `<Action> <LocationName> <X> <Y>` | `TsCoreWarp Farm 64 15` |
| Location + coordinates + facing direction | `<Action> <LocationName> <X> <Y> <FacingDirection>` | `TsCoreMagicWarp Farm 64 15 Down` |

Replace `<Action>` with either `TsCoreWarp` or `TsCoreMagicWarp`.

When only a location name is specified, T's Core automatically uses the location's default warp point.

> **Note:** Direct coordinate warps behave like the corresponding vanilla warp action. If you do not need any T's Core-specific functionality, using the vanilla warp action is recommended.

---

### Facing Direction

Facing direction can be specified using either its name or numeric value.

| Direction | Numeric Value |
|----------|--------------:|
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

can be used instead of a fixed destination such as:

```text
Warp Farm 64 15
```

Providers make Content Packs easier to maintain and improve compatibility with custom maps and mods that move buildings or change warp destinations.

For example, `FarmHouseFront` resolves the farmhouse entrance dynamically, so a Content Pack does not need to know its exact coordinates.

---

### Provider Resolution

When a destination is passed to `TsCoreWarp` or `TsCoreMagicWarp`, T's Core first attempts to resolve it as a registered Warp Provider.

If no matching provider exists, the value is treated as a location name.

For example, both of the following are valid:

```text
TsCoreWarp FarmHouseFront
TsCoreWarp BusStop
```

`FarmHouseFront` resolves to a registered provider, while `BusStop` is treated as a location name if no provider with that name exists.

---

## Content Pack Setup

In addition to the built-in providers, T's Core allows Content Packs to register custom Warp Providers.

Warp Providers are added through a standard **T's Core Content Pack**.

A single Content Pack can include multiple T's Core features, such as custom Warp Providers and Notification Themes.

### manifest.json

```json
{
  "Name": "[TsC] My T's Core Content Pack",
  "Author": "YourName",
  "Version": "1.0.0",
  "UniqueID": "YourName.MyTsCorePack",
  "ContentPackFor": {
    "UniqueID": "Tikamin557.TsCore"
  }
}
```

Replace the example values with your own information before publishing your Content Pack.

### Folder Structure

```text
[TsC] My T's Core Content Pack
├── manifest.json
└── assets
    ├── notification
    │   └── MyNotification.json
    └── warp
        └── MyWarpProvider.json
```

Currently, T's Core supports the following feature folders:

| Folder | Purpose |
|--------|---------|
| `assets/notification` | Custom Notification Themes |
| `assets/warp` | Custom Warp Providers |

Only create the folders your Content Pack actually uses.

JSON filenames may be chosen freely.

> **Note:** The folder names (`assets`, `notification`, `warp`, etc.) are fixed and must not be renamed. T's Core searches these specific folders when loading Content Packs.

---

## Warp Provider Types

Each JSON file inside `assets/warp` defines one Warp Provider.

T's Core currently supports two provider types:

- `Warp`
- `Building`

---

### Warp Provider (Type: Warp)

A **Warp** provider resolves its destination by reading an existing warp from a source location.

```json
{
  "Id": "MyFarmHouseFront",
  "Type": "Warp",
  "Source": "FarmHouse",
  "Target": "Farm"
}
```

| Property | Required | Description |
|----------|----------|-------------|
| `Id` | ✅ | Unique provider name used by `TsCoreWarp` and `TsCoreMagicWarp`. |
| `Type` | ✅ | Must be `"Warp"`. |
| `Source` | ✅ | Location containing the warp to inspect. |
| `Target` | ✅ | Destination location of the warp to resolve. |

T's Core searches `Source` for a warp leading to `Target` and uses that warp's destination coordinates.

Because the destination is resolved at runtime, this provider can adapt when another mod changes the corresponding map warp.

> **Note:** Provider IDs should be unique. If another provider with the same ID is already registered, the duplicate provider will not be registered.

---

### Warp Provider (Type: Building)

A **Building** provider calculates its destination from the position of a building placed on the player's farm.

The following example shows the actual Building Provider used by the **[(SF) Monster House](https://www.nexusmods.com/stardewvalley/mods/20586)** mod.

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
| `BuildingType` | ✅ | Internal building type to search for on the player's farm. |
| `OffsetX` | ✅ | Horizontal offset from the building's top-left tile. |
| `OffsetY` | ✅ | Vertical offset from the building's top-left tile. |
| `Fallback` | Optional | Provider used if the building cannot be found. Defaults to `FarmHouseFront`. |

T's Core searches the farm for the specified `BuildingType` and calculates the destination from the building's top-left tile plus `OffsetX` and `OffsetY`.

For example, the Monster House occupies a **2 × 1** area:

```text
■■
□
↑ Warp destination
```

With:

```json
"OffsetX": 0,
"OffsetY": 1
```

the destination is one tile directly below the building's top-left tile.

If the building cannot be found, the provider specified by `Fallback` is used instead.

> **Tip:** Use `tscore_debug_buildings` to inspect building types, positions, and sizes currently present on the farm.

---

## Built-in Warp Providers

T's Core includes several built-in providers that resolve their destinations dynamically from existing map warps.

| Provider | Source | Target | Description |
|----------|--------|--------|-------------|
| `FarmHouseFront` | `FarmHouse` | `Farm` | Resolves the tile outside the farmhouse entrance. |
| `GreenhouseFront` | `Greenhouse` | `Farm` | Resolves the tile outside the greenhouse entrance. |
| `FarmCaveFront` | `FarmCave` | `Farm` | Resolves the tile outside the farm cave entrance. |
| `IslandFarmHouseFront` | `IslandFarmHouse` | `IslandWest` | Resolves the tile outside the Island Farmhouse. |

All built-in providers can be used with either `TsCoreWarp` or `TsCoreMagicWarp`.

Example:

```text
TsCoreWarp FarmHouseFront
```

Because these destinations are resolved from the active map warps, they can adapt to compatible custom maps and mods that move or modify their corresponding entrances.

> **Note:** A provider requires a valid warp between its configured source and target locations. If another mod removes that warp entirely, the provider may not be able to resolve its destination.

Additional built-in providers may be added in future versions of T's Core.

---

## Examples

The following example adds a Tile Action to the front of the fountain in **Pelican Town**.

Clicking the fountain uses the `FarmHouseFront` provider to magic-warp the player to the farmhouse entrance and face **down**.

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

To perform the same warp without the magic warp effect, replace `TsCoreMagicWarp` with `TsCoreWarp`.

---

## Debugging

T's Core provides several debug commands for inspecting registered Warp Providers, farm buildings, and reloading Warp Providers during development.

---

### Reloading T's Core Resources

When developing a T's Core Content Pack, you can reload supported resources without restarting the game.

| Command | Reloads |
|---------|---------|
| `tscore_reload`<br>`tscore_reload all` | All supported T's Core resources |
| `tscore_reload warp` | Warp Providers |
| `tscore_reload notification` | Notification Themes |

Running `tscore_reload` without an argument is equivalent to `tscore_reload all`.

After running a reload command, T's Core automatically rescans the corresponding folders.

Any JSON files that have been added, modified, or removed are detected and applied immediately without restarting the game.

This allows you to test changes, add new Warp Providers or Notification Themes, and iterate on your Content Pack much faster while the game is running.

> **Note:** Reloading only refreshes data loaded by T's Core. It does not reload Content Patcher patches or other SMAPI mods.

---

### Inspecting Warp Providers

Use the following SMAPI command to display all currently registered Warp Providers:

```text
tscore_debug_warp
```

The following example shows both the built-in providers included with T's Core and a custom provider registered by the T's Core Content Pack included with the **[(SF) Monster House](https://www.nexusmods.com/stardewvalley/mods/20586)** mod.

<details>
<summary>Example output</summary>

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

</details>

---

### Inspecting Farm Buildings

When creating a Warp Provider with `Type: "Building"`, you can inspect the buildings currently placed on the farm using the following SMAPI command:

```text
tscore_debug_buildings
```

This command displays each building's internal type, tile position, size, and interior location.

The value shown for `Building` can be used as the `BuildingType` property in a Building Provider definition.

<details>
<summary>Example output</summary>

```text
tscore_debug_buildings
[T's Core] ===== Farm Buildings =====
[T's Core] Registered Buildings: 2
[T's Core]
[T's Core] Farmhouse
[T's Core]     Tile                : (59, 12)
[T's Core]     Size                : 9 x 5
[T's Core]     Indoors             : FarmHouse
[T's Core]
[T's Core] Tikami557.SF.MonsterHouse.Buildings_MonsterHouse
[T's Core]     Tile                : (56, 12)
[T's Core]     Size                : 2 x 1
[T's Core]     Indoors             : (none)
```

</details>

In this example, the value used for the Monster House provider is:

```json
"BuildingType": "Tikami557.SF.MonsterHouse.Buildings_MonsterHouse"
```

The `Tile` value represents the building's top-left tile and can be used to verify the position calculated from `OffsetX` and `OffsetY`.

> **Note:** A save must be loaded before using `tscore_debug_buildings`.

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
