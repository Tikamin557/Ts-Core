# 📖 Modder Guide

This guide explains how to use the public features provided by **T's Core** in Content Patcher.

<a id="top"></a>

## Guide Index

- 📄 [Relationship Services](ModderGuide_RelationshipServices.md)
- 📄 [Location Services](ModderGuide_LocationServices.md)
- ✅ **Warp Services** *(Current Page)*
- 📄 [Building Services](ModderGuide_BuildingServices.md)
- 📄 [Notification System](ModderGuide_NotificationSystem.md)
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

- [Warp Actions](#warp-actions)
- [Warp Providers](#warp-providers)
- [Content Pack Setup](#content-pack-setup)
- [Warp Provider Types](#warp-provider-types)
- [Built-in Warp Providers](#built-in-warp-providers)
- [Example](#example)
- [Debugging](#debugging)
- [Notes](#notes)

---

## Warp Actions

T's Core provides three custom warp actions:

| Action | Description |
|--------|-------------|
| `TsCoreWarp` | Performs a standard warp without visual effects. |
| `TsCoreMagicWarp` | Performs the same warp using Stardew Valley's built-in magic warp animation and sound effect. |
| `TsCoreMagicWarp_Simple` | Performs a simplified magic warp. Some of the standard magic warp effects are omitted, and the player remains visible during the warp animation. |

All three actions support Warp Providers, location names, direct coordinates, and optional facing directions.

All three actions also support a custom audio cue.

They can be used with:

- Tile Actions
- Touch Actions

> **Note:** When custom warp actions are used as Tile Actions, Stardew Valley normally logs a legacy **"unknown warp property"** warning to the SMAPI console. T's Core automatically suppresses this warning for T's Core warp actions.

---

### Syntax

All T's Core Warp Actions use the same syntax.

| Destination | Syntax | Example |
|-------------|--------|---------|
| Provider / Location | `<Action> <ProviderOrLocation>` | `TsCoreWarp FarmHouseFront` |
| Provider / Location + facing direction | `<Action> <ProviderOrLocation> <FacingDirection>` | `TsCoreMagicWarp FarmHouseFront Left` |
| Provider / Location + facing direction + audio cue | `<Action> <ProviderOrLocation> <FacingDirection> <AudioCue>` | `TsCoreMagicWarp_Simple FarmHouseFront Auto wand` |
| Direct coordinates | `<Action> <LocationName> <X> <Y>` | `TsCoreWarp Farm 64 15` |
| Direct coordinates + facing direction | `<Action> <LocationName> <X> <Y> <FacingDirection>` | `TsCoreMagicWarp Farm 64 15 Down` |
| Direct coordinates + facing direction + audio cue | `<Action> <LocationName> <X> <Y> <FacingDirection> <AudioCue>` | `TsCoreMagicWarp_Simple Farm 64 15 Auto wand` |

Replace `<Action>` with:

- `TsCoreWarp`
- `TsCoreMagicWarp`
- `TsCoreMagicWarp_Simple`

When only a location name is specified, T's Core automatically uses the location's default warp point.

> **Note:** A custom audio cue requires a `FacingDirection` argument. Use `Auto` if you want to keep the player's current facing direction.

---

### Facing Direction

Facing direction can be specified using either its name or numeric value.

| Direction | Numeric Value | Description |
|----------|--------------:|-------------|
| `Up` | `0` | Face upward after warping. |
| `Right` | `1` | Face right after warping. |
| `Down` | `2` | Face downward after warping. |
| `Left` | `3` | Face left after warping. |
| `Auto` | `4` | Keep the player's current facing direction. |

`Auto` is useful when a facing direction argument is required, such as when specifying a custom audio cue, but you don't want to change the player's current facing direction.

---

### Custom Audio Cue

T's Core Warp Actions can optionally play a custom audio cue when the warp is performed.

To specify a custom audio cue, a `FacingDirection` must also be specified.

For a Warp Provider or location name:

```text
<Action> <ProviderOrLocation> <FacingDirection> <AudioCue>
```

For direct coordinates:

```text
<Action> <LocationName> <X> <Y> <FacingDirection> <AudioCue>
```

Examples:

```text
TsCoreMagicWarp FarmHouseFront Down wand
TsCoreMagicWarp_Simple FarmHouseFront Auto wand
TsCoreWarp Farm 64 15 Left doorClose
```

The final argument is the audio cue name to play when the warp is performed.

If you want to specify a custom audio cue without changing the player's facing direction, use `Auto` for `FacingDirection`.

```text
TsCoreMagicWarp_Simple FarmHouseFront Auto MyAudioCue
```

> **Note:** A custom audio cue cannot be specified without `FacingDirection`, since the audio cue is always the argument immediately following it.

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

When a destination is passed to a T's Core Warp Action, T's Core first attempts to resolve it as a registered Warp Provider.

If no matching provider exists, the value is treated as a location name.

For example:

```text
TsCoreWarp FarmHouseFront
TsCoreWarp BusStop
```

`FarmHouseFront` resolves to a registered provider, while `BusStop` is treated as a location name if no provider with that name exists.

---

## Content Pack Setup

T's Core Content Packs can register custom Warp Providers.

A single Content Pack can include multiple T's Core features, such as custom Warp Providers and Notification Themes.

### manifest.json

```json
{
  "Name": "[TsC] My T's Core Content Pack",
  "Author": "YourName",
  "Version": "1.0.0",
  "UniqueID": "YourName.MyTsCorePack",
  "UpdateKeys": [ "Nexus:12345" ],
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
    ├── buildings
    │   └── MyBuilding.json
    ├── notification
    │   └── MyNotification.json
    └── warp
        └── MyWarpProvider.json
```

Currently, T's Core supports the following feature folders:

| Folder | Purpose |
|--------|---------|
| `assets/buildings` | Building Providers |
| `assets/notification` | Custom Notification Themes |
| `assets/warp` | Custom Warp Providers |

Only create the folders your Content Pack actually uses.

JSON filenames may be chosen freely.

> **Note:** The folder names (`assets`, `buildings`, `notification`, `warp`, etc.) are fixed and must not be renamed. T's Core searches these specific folders when loading Content Packs.

---

## Warp Provider Types

Each JSON file inside `assets/warp` defines one Warp Provider.

T's Core currently supports `Warp` and `Building` providers.

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
| `Id` | ✅ | Unique provider name used by T's Core Warp Actions. |
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
| `Id` | ✅ | Unique provider name used by T's Core Warp Actions. |
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

> **Tip:** Use `tscore_debug_farmbuildings` to inspect building types, positions, and sizes currently present on the farm.

---

## Built-in Warp Providers

T's Core includes several built-in providers that resolve their destinations dynamically from existing map warps.

| Provider | Source | Target | Description |
|----------|--------|--------|-------------|
| `FarmHouseFront` | `FarmHouse` | `Farm` | Resolves the tile outside the farmhouse entrance. |
| `GreenhouseFront` | `Greenhouse` | `Farm` | Resolves the tile outside the greenhouse entrance. |
| `FarmCaveFront` | `FarmCave` | `Farm` | Resolves the tile outside the farm cave entrance. |
| `IslandFarmHouseFront` | `IslandFarmHouse` | `IslandWest` | Resolves the tile outside the Island Farmhouse. |

All built-in providers can be used with any T's Core Warp Action.

Because these destinations are resolved from the active map warps, they can adapt to compatible custom maps and mods that move or modify their corresponding entrances.

> **Note:** A provider requires a valid warp between its configured source and target locations. If another mod removes that warp entirely, the provider may not be able to resolve its destination.

Additional built-in providers may be added in future versions of T's Core.

---

## Example

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

To use the simplified magic warp effect while keeping the player visible during the animation, use `TsCoreMagicWarp_Simple` instead.

---

## Debugging

T's Core provides debug commands for inspecting Warp Providers and farm buildings, as well as reloading T's Core resources during development.

---

### Reloading T's Core Resources

When developing a T's Core Content Pack, you can reload supported resources without restarting the game.

| Command | Reloads |
|---------|---------|
| `tscore_reload`<br>`tscore_reload all` | All supported T's Core resources |
| `tscore_reload warp` | Warp Providers |
| `tscore_reload building` | Building Providers |
| `tscore_reload notification` | Notification Themes |

Running `tscore_reload` without an argument is equivalent to `tscore_reload all`.

After running a reload command, T's Core rescans the corresponding folders.

Any JSON files that have been added, modified, or removed are detected and applied without restarting the game.

This makes it possible to test changes and add new Warp Providers or Notification Themes while the game is running.

> **Note:** Reloading only refreshes data loaded by T's Core. It does not reload Content Patcher patches or other SMAPI mods.

---

### Inspecting Warp Providers

Use the following command to display all currently registered Warp Providers:

```text
tscore_debug_warp
```

The following example shows the built-in providers and a custom provider registered by the T's Core Content Pack included with the **[(SF) Monster House](https://www.nexusmods.com/stardewvalley/mods/20586)** mod.

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
[T's Core]     Building            : Tikamin557.SF.MonsterHouse.Buildings_MonsterHouse
[T's Core]     Offset              : (0, 1)
[T's Core]     Fallback            : FarmHouseFront
```

</details>

---

### Inspecting Farm Buildings

When creating a `Building` provider, use the following command to inspect buildings currently placed on the farm:

```text
tscore_debug_farmbuildings
```

It displays each building's internal type, tile position, size, and interior location.

The building name shown in the output can be used as the `BuildingType` value.

<details>
<summary>Example output</summary>

```text
tscore_debug_farmbuildings
[T's Core] ===== Farm Buildings =====
[T's Core] Registered Buildings: 2
[T's Core]
[T's Core] Farmhouse
[T's Core]     Tile                : (59, 12)
[T's Core]     Size                : 9 x 5
[T's Core]     Indoors             : FarmHouse
[T's Core]
[T's Core] Tikamin557.SF.MonsterHouse.Buildings_MonsterHouse
[T's Core]     Tile                : (56, 12)
[T's Core]     Size                : 2 x 1
[T's Core]     Indoors             : (none)
```

</details>

For the Monster House example, the corresponding value is:

```json
"BuildingType": "Tikamin557.SF.MonsterHouse.Buildings_MonsterHouse"
```

The `Tile` value represents the building's top-left tile and can be used to verify the destination calculated from `OffsetX` and `OffsetY`.

> **Note:** A save must be loaded before using `tscore_debug_farmbuildings`.

---

## Notes

Warp Services are designed for use with Content Patcher and require no C# code.

Whenever possible, use Warp Providers instead of hardcoded map names or coordinates to improve compatibility with custom maps and other mods.

Additional providers and features may be added in future versions of T's Core.

---

## Modder Guide

- ← [Location Services](ModderGuide_LocationServices.md)
- ↑ [Guide Index](#top)
- → [Building Services](ModderGuide_BuildingServices.md)

← [Back to README](../README.md)

← [Back to Modder Guide](ModderGuide.md)
