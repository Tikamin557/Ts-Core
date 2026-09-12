# 📖 Modder Guide

This guide explains how to use the public features provided by **T's Core** in Content Patcher.

<a id="top"></a>

## Guide Index

- 📄 [Relationship Services](ModderGuide_RelationshipServices.md)
- 📄 [Location Services](ModderGuide_LocationServices.md)
- ✅ **Warp Services** *(Current Page)*
- 📄 [Map Properties](ModderGuide_MapProperties.md)
- 📄 [Building Services](ModderGuide_BuildingServices.md)
- 📄 [Machine Interaction](ModderGuide_MachineInteraction.md)
- 📄 [Migration System](ModderGuide_MigrationSystem.md)
- 📄 [Notification System](ModderGuide_NotificationSystem.md)
- 📄 [Content Patcher Integration](ModderGuide_ContentPatcherIntegration.md)

← [Back to README](../README.md)

← [Back to Modder Guide](ModderGuide.md)

---

# Warp Services

Warp Services provide reusable warp functionality for Content Patcher.

T's Core provides custom Warp Actions and reusable Warp Providers, and allows Content Patcher packs to register their own Warp Providers through the custom data asset:

```text
TsCore/WarpProviders
```

No C# code is required.

This guide explains how to use Warp Actions, register custom Warp Providers, and integrate them into Content Patcher packs.

---

## Contents

- [Warp Actions](#warp-actions)
- [Warp Providers](#warp-providers)
- [Content Pack Setup](#content-pack-setup)
- [Registering Warp Providers](#registering-warp-providers)
- [Warp Provider Types](#warp-provider-types)
- [Default Warp Providers](#default-warp-providers)
- [Special Built-in Warp Providers](#special-built-in-warp-providers)
- [Example](#example)
- [Debugging](#debugging)
- [Notes](#notes)

---

## Warp Actions

T's Core provides three custom warp actions:

| Action | Description |
|--------|-------------|
| `TsCoreWarp` | Performs a standard warp without magic warp visual effects. |
| `TsCoreMagicWarp` | Performs a magic warp using effects similar to Stardew Valley's built-in magic warp. |
| `TsCoreMagicWarp_Simple` | Performs a simplified magic warp. Some of the standard magic warp effects are omitted, and the player remains visible during the warp animation. |

All three actions support:

- Warp Providers
- Location names
- Direct coordinates
- Optional facing directions
- Custom audio cues
- Repeated audio playback
- Custom audio intervals
- Custom blackout durations
- Delayed audio playback

They can be used with:

- Tile Actions
- Touch Actions

> **Note:** When custom warp actions are used as Tile Actions, Stardew Valley normally logs a legacy **"unknown warp property"** warning to the SMAPI console. T's Core automatically suppresses this warning for T's Core warp actions.

---

### Syntax

All T's Core Warp Actions use the same syntax.

#### Provider / Location

```text
<Action> <ProviderOrLocation> [FacingDirection] [AudioCue] [RepeatCount] [IntervalMs] [BlackoutDurationMs] [AudioStartDelayMs]
```

#### Direct Coordinates

```text
<Action> <LocationName> <X> <Y> [FacingDirection] [AudioCue] [RepeatCount] [IntervalMs] [BlackoutDurationMs] [AudioStartDelayMs]
```

Replace `<Action>` with:

- `TsCoreWarp`
- `TsCoreMagicWarp`
- `TsCoreMagicWarp_Simple`

Examples:

```text
TsCoreWarp FarmHouseFront
TsCoreMagicWarp FarmHouseFront Down
TsCoreMagicWarp_Simple FarmHouseFront Auto wand
TsCoreMagicWarp FarmHouseFront Auto wand 3 100
TsCoreMagicWarp FarmHouseFront Auto wand 3 100 200
TsCoreMagicWarp FarmHouseFront Auto wand 3 100 200 250

TsCoreWarp Farm 64 15
TsCoreMagicWarp Farm 64 15 Down
TsCoreMagicWarp_Simple Farm 64 15 Auto wand
TsCoreMagicWarp Farm 64 15 Auto wand 3 100 200 250
```

When only a location name is specified, T's Core automatically uses the location's default warp point.

> **Note:** Optional arguments are positional. To specify a later argument, all preceding arguments must also be specified.

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

`Auto` is useful when a later optional argument needs to be specified but you don't want to change the player's current facing direction.

For example:

```text
TsCoreMagicWarp FarmHouseFront Auto wand
```

---

### Custom Audio Cue

T's Core Warp Actions can optionally play an audio cue when the warp is performed.

The audio cue is specified immediately after `FacingDirection`.

```text
TsCoreMagicWarp FarmHouseFront Down wand
TsCoreMagicWarp_Simple FarmHouseFront Auto wand
TsCoreWarp Farm 64 15 Left doorClose
```

If you want to specify an audio cue without changing the player's facing direction, use `Auto`.

```text
TsCoreMagicWarp_Simple FarmHouseFront Auto MyAudioCue
```

For `TsCoreMagicWarp` and `TsCoreMagicWarp_Simple`, the default audio cue is:

```text
wand
```

If a custom audio cue is specified, it replaces the default `wand` sound.

`TsCoreWarp` does not play a warp sound by default, but a custom audio cue can still be specified.

---

### Audio Repeat Count

`RepeatCount` controls how many times the audio cue is played.

```text
TsCoreMagicWarp FarmHouseFront Auto wand 3
```

In this example, `wand` is played three times.

The value must be `1` or greater.

Default:

```text
1
```

---

### Audio Interval

`IntervalMs` controls the interval between repeated audio cues in milliseconds.

```text
TsCoreMagicWarp FarmHouseFront Auto wand 3 200
```

This plays the audio cue three times with a 200 ms interval between each playback.

The value must be `0` or greater.

Default:

```text
100
```

`IntervalMs` only has a noticeable effect when `RepeatCount` is greater than `1`.

---

### Blackout Duration

`BlackoutDurationMs` controls how long the screen remains completely black before the actual warp is performed.

```text
TsCoreMagicWarp FarmHouseFront Auto wand 1 100 500
```

In this example, the screen remains fully black for 500 ms before the warp occurs.

The value must be `0` or greater.

If this argument is omitted, T's Core uses the default blackout duration for the selected Warp Action.

Current default:

```text
100 ms
```

A value of `0` performs the warp immediately after the screen becomes fully black.

---

### Audio Start Delay

`AudioStartDelayMs` controls how long T's Core waits before starting audio playback.

```text
TsCoreMagicWarp FarmHouseFront Auto wand 1 100 100 300
```

In this example, playback of `wand` begins after a 300 ms delay.

The value must be `0` or greater.

Default:

```text
0
```

When combined with repeated playback, the timing is calculated from the start delay.

For example:

```text
TsCoreMagicWarp FarmHouseFront Auto wand 3 200 100 300
```

plays the audio cue at approximately:

```text
300 ms
500 ms
700 ms
```

after the Warp Action begins.

---

### Optional Argument Order

The complete optional argument order is:

| Argument | Default | Description |
|----------|---------|-------------|
| `FacingDirection` | Current direction | Direction the player faces after warping. |
| `AudioCue` | `wand` for Magic types, none for normal Warp | Audio cue played during the warp. |
| `RepeatCount` | `1` | Number of times the audio cue is played. |
| `IntervalMs` | `100` | Interval between repeated audio cues. |
| `BlackoutDurationMs` | `100` | Time the screen remains completely black before warping. |
| `AudioStartDelayMs` | `0` | Delay before audio playback begins. |

Because these arguments are positional, preceding values must be included when specifying later options.

For example, to specify only a custom blackout duration while keeping the other settings at their normal values:

```text
TsCoreMagicWarp FarmHouseFront Auto wand 1 100 500
```

---

## Warp Providers

Warp Providers allow Content Patcher packs to reference destinations by **provider ID** instead of hardcoding map names and coordinates.

For example:

```text
TsCoreWarp FarmHouseFront
```

can be used instead of a fixed destination such as:

```text
Warp Farm 64 15
```

Providers make Content Patcher packs easier to maintain and improve compatibility with custom maps and mods that move buildings or change warp destinations.

For example, `FarmHouseFront` resolves the farmhouse entrance dynamically, so a Content Patcher pack does not need to know its exact coordinates.

Custom Warp Providers are registered through the T's Core data asset:

```text
TsCore/WarpProviders
```

---

### Provider Resolution

When a destination is passed to a T's Core Warp Action, T's Core first attempts to resolve it as a Warp Provider.

If no matching provider exists, the value is treated as a location name.

For example:

```text
TsCoreWarp FarmHouseFront
TsCoreWarp BusStop
```

`FarmHouseFront` resolves to a Warp Provider, while `BusStop` is treated as a location name if no provider with that ID exists.

When a location name is used directly, T's Core uses Stardew Valley's default warp location for that map.

---

## Content Pack Setup

Custom Warp Providers are registered from a normal **Content Patcher Content Pack**.

The Content Pack should depend on both Content Patcher and T's Core.

### manifest.json

```json
{
  "Name": "[CP] My Warp Pack",
  "Author": "YourName",
  "Version": "1.0.0",
  "UniqueID": "YourName.MyWarpPack",
  "UpdateKeys": [ "Nexus:12345" ],
  "ContentPackFor": {
    "UniqueID": "Pathoschild.ContentPatcher"
  },
  "Dependencies": [
    {
      "UniqueID": "Tikamin557.TsCore",
      "IsRequired": true
    }
  ]
}
```

Replace the example values with your own information before publishing your Content Pack.

### Folder Structure

A simple Content Pack may look like this:

```text
[CP] My Warp Pack
├── manifest.json
└── content.json
```

Warp Provider definitions do not need a special folder.

They are added through Content Patcher's `EditData` action in `content.json`.

---

## Registering Warp Providers

Use `EditData` with:

```text
Target: TsCore/WarpProviders
```

Each entry key becomes the Warp Provider ID.

For example:

```json
{
  "Action": "EditData",
  "Target": "TsCore/WarpProviders",
  "Entries": {
    "MyWarpProvider": {
      "Type": "Warp",
      "Source": "Mine",
      "Target": "Mountain"
    }
  }
}
```

The provider ID in this example is:

```text
MyWarpProvider
```

Unlike older T's Core Warp Provider definitions, the provider data itself does **not** contain an `Id` property.

The ID is taken from the `Entries` key:

```json
"Entries": {
  "MyWarpProvider": {
```

A single `EditData` patch can register multiple providers.

For example:

```json
{
  "Action": "EditData",
  "Target": "TsCore/WarpProviders",
  "Entries": {
    "MyWarpProvider": {
      "Type": "Warp",
      "Source": "Mine",
      "Target": "Mountain"
    },
    "MyBuildingWarp": {
      "Type": "Building",
      "BuildingType": "YourName.MyMod_MyBuilding",
      "OffsetX": 1,
      "OffsetY": 3
    }
  }
}
```

> **Note:** Provider IDs should be globally unique. Using an ID already present in `TsCore/WarpProviders` may modify or replace that entry depending on the Content Patcher patches affecting the asset.

For custom providers, using an ID based on your mod's UniqueID is recommended when possible.

For example:

```text
YourName.MyMod_MyWarp
```

---

## Warp Provider Types

T's Core currently supports three data asset Warp Provider types:

| Type | Purpose |
|------|---------|
| `Warp` | Resolves the destination of an existing map warp. |
| `MapEntry` | Resolves the source position of an existing map warp. |
| `Building` | Resolves a position relative to a building placed on the farm. |

---

### Warp Provider (Type: Warp)

A **Warp** provider resolves its destination by reading an existing warp from a source location.

```json
{
  "Action": "EditData",
  "Target": "TsCore/WarpProviders",
  "Entries": {
    "MyFarmHouseFront": {
      "Type": "Warp",
      "Source": "FarmHouse",
      "Target": "Farm",
      "Fallback": "FarmHouseFront"
    }
  }
}
```

| Property | Required | Description |
|----------|----------|-------------|
| Entry key | ✅ | Unique provider ID used by T's Core Warp Actions. |
| `Type` | ✅ | Must be `"Warp"`. |
| `Source` | ✅ | Location containing the warp to inspect. |
| `Target` | ✅ | Destination location of the warp to resolve. |
| `Fallback` | Optional | Provider used if the destination cannot be resolved. |

T's Core searches `Source` for a warp leading to `Target`.

If a matching warp is found, the provider returns that warp's **destination location and destination coordinates**.

For example:

```text
FarmHouse
    Warp at (4, 11)
        → Farm (64, 15)
```

A `Warp` provider configured with:

```json
{
  "Type": "Warp",
  "Source": "FarmHouse",
  "Target": "Farm"
}
```

resolves to:

```text
Farm (64, 15)
```

Because the destination is resolved at runtime, this provider can adapt when another mod changes the corresponding map warp.

If the warp cannot be found and `Fallback` is specified, T's Core attempts to resolve the fallback provider instead.

---

### Warp Provider (Type: MapEntry)

A **MapEntry** provider resolves the position of an existing warp **inside the map containing that warp**.

Unlike a `Warp` provider, which returns where an existing warp leads, a `MapEntry` provider returns where that warp is located.

```json
{
  "Action": "EditData",
  "Target": "TsCore/WarpProviders",
  "Entries": {
    "MyFarmHouseEntry": {
      "Type": "MapEntry",
      "Map": "FarmHouse",
      "Target": "Farm",
      "OffsetX": 0,
      "OffsetY": -1,
      "Fallback": "FarmHouseFront"
    }
  }
}
```

| Property | Required | Description |
|----------|----------|-------------|
| Entry key | ✅ | Unique provider ID used by T's Core Warp Actions. |
| `Type` | ✅ | Must be `"MapEntry"`. |
| `Map` | ✅ | Location containing the warp to inspect. |
| `Target` | ✅ | Destination location used to identify the warp. |
| `OffsetX` | Optional | Horizontal offset from the warp's source tile. Defaults to `0`. |
| `OffsetY` | Optional | Vertical offset from the warp's source tile. Defaults to `0`. |
| `Fallback` | Optional | Provider used if the destination cannot be resolved. |

T's Core searches `Map` for a warp leading to `Target`.

If a matching warp is found, T's Core returns the **source position of that warp inside `Map`**, optionally adjusted by `OffsetX` and `OffsetY`.

For example:

```text
FarmHouse
    Warp at (4, 11)
        → Farm (64, 15)
```

A `MapEntry` provider configured with:

```json
{
  "Type": "MapEntry",
  "Map": "FarmHouse",
  "Target": "Farm"
}
```

resolves to:

```text
FarmHouse (4, 11)
```

By comparison, a `Warp` provider using the same map relationship would resolve to:

```text
Farm (64, 15)
```

This makes `MapEntry` useful when you need a destination near a map's entrance or exit without hardcoding the tile coordinates.

Offsets can be used to move the destination relative to the detected warp.

For example:

```json
{
  "Type": "MapEntry",
  "Map": "FarmHouse",
  "Target": "Farm",
  "OffsetX": 0,
  "OffsetY": -1
}
```

If the matching warp is located at `(4, 11)`, this provider resolves to:

```text
FarmHouse (4, 10)
```

If the map or matching warp cannot be found and `Fallback` is specified, T's Core attempts to resolve the fallback provider instead.

---

### Warp Provider (Type: Building)

A **Building** provider calculates its destination from the position of a building placed on the player's farm.

The following example shows a Building Provider for a custom building.

```json
{
  "Action": "EditData",
  "Target": "TsCore/WarpProviders",
  "Entries": {
    "MonsterHouseFront": {
      "Type": "Building",
      "BuildingType": "Tikamin557.SF.MonsterHouse.Buildings_MonsterHouse",
      "OffsetX": 0,
      "OffsetY": 1,
      "Fallback": "FarmHouseFront"
    }
  }
}
```

| Property | Required | Description |
|----------|----------|-------------|
| Entry key | ✅ | Unique provider ID used by T's Core Warp Actions. |
| `Type` | ✅ | Must be `"Building"`. |
| `BuildingType` | ✅ | Internal building type to search for on the player's farm. |
| `OffsetX` | Optional | Horizontal offset from the building's top-left tile. Defaults to `0`. |
| `OffsetY` | Optional | Vertical offset from the building's top-left tile. Defaults to `0`. |
| `Fallback` | Optional | Provider used if the building cannot be found. |

T's Core searches the farm for the specified `BuildingType` and calculates the destination from the building's top-left tile plus `OffsetX` and `OffsetY`.

For example, a building occupying a **2 × 1** area may use:

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

## Default Warp Providers

`TsCore/WarpProviders` includes four default Warp Providers provided by T's Core.

| Provider | Source | Target | Description |
|----------|--------|--------|-------------|
| `FarmHouseFront` | `FarmHouse` | `Farm` | Resolves the tile outside the farmhouse entrance. |
| `GreenhouseFront` | `Greenhouse` | `Farm` | Resolves the tile outside the greenhouse entrance. |
| `FarmCaveFront` | `FarmCave` | `Farm` | Resolves the tile outside the farm cave entrance. |
| `IslandFarmHouseFront` | `IslandFarmHouse` | `IslandWest` | Resolves the tile outside the Island Farmhouse. |

These providers are standard entries in:

```text
TsCore/WarpProviders
```

and can be used with any T's Core Warp Action.

For example:

```text
TsCoreWarp FarmHouseFront
TsCoreMagicWarp GreenhouseFront Down
```

Because these destinations are resolved from the active map warps, they can adapt to compatible custom maps and mods that move or modify their corresponding entrances.

> **Note:** A provider requires a valid warp between its configured source and target locations. If another mod removes that warp entirely, the provider may not be able to resolve its destination.

Additional default providers may be added in future versions of T's Core.

---

## Special Built-in Warp Providers

T's Core also provides three special Warp Providers implemented directly by T's Core.

Unlike the providers in `TsCore/WarpProviders`, these are not data asset entries.

| Provider | Description |
|----------|-------------|
| `PlayerHome` | Resolves the entrance of the current player's own FarmHouse or Cabin. |
| `PreviousHome` | Resolves the entrance of the FarmHouse or Cabin the player most recently exited. |
| `CurrentHome` | Resolves the entrance of the FarmHouse or Cabin in which the action is currently being used. |

These providers can be used directly with T's Core Warp Actions.

For example:

```text
TsCoreWarp PlayerHome
TsCoreMagicWarp PreviousHome
TsCoreMagicWarp_Simple CurrentHome
```

---

### PlayerHome

`PlayerHome` resolves the current player's own home.

This works with both the main FarmHouse and player cabins.

For example:

```text
TsCoreWarp PlayerHome
```

warps the player to the entrance position of their own home.

---

### PreviousHome

`PreviousHome` remembers the FarmHouse or Cabin the local player most recently exited.

For example:

```text
TsCoreWarp PreviousHome
```

can be used to return the player to the entrance of that home.

`PreviousHome` becomes available after T's Core has recorded a home the player exited.

If no previous home has been recorded yet, the provider cannot resolve a destination.

---

### CurrentHome

`CurrentHome` resolves the entrance position of the FarmHouse or Cabin where the Warp Action is currently being used.

For example:

```text
TsCoreWarp CurrentHome
```

This provider is intended for actions used from inside a FarmHouse or Cabin.

If it is used from another type of location, it cannot resolve a destination.

---

### Reserved Provider IDs

The following IDs are reserved for T's Core's special built-in providers:

```text
PlayerHome
PreviousHome
CurrentHome
```

Custom entries using these IDs in `TsCore/WarpProviders` are not treated as custom providers.

Use a different ID for your own Warp Providers.

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

For example:

```json
{
    "Action": "EditMap",
    "Target": "Maps/Town",
    "MapTiles": [
        {
            "Position": { "X": 26, "Y": 28 },
            "Layer": "Buildings",
            "SetProperties": {
                "Action": "TsCoreMagicWarp_Simple FarmHouseFront Auto wand 2 150 100 0"
            }
        }
    ]
}
```

This example:

- uses the `FarmHouseFront` provider;
- keeps the player's current facing direction;
- uses the `wand` audio cue;
- plays the cue twice;
- uses a 150 ms audio interval;
- keeps the screen completely black for 100 ms before warping;
- starts audio playback immediately.

---

## Debugging

T's Core provides debug commands for inspecting Warp Providers and farm buildings.

Warp Providers registered through `TsCore/WarpProviders` are managed through Content Patcher, so changes to them should be reloaded using T's Core's Content Patcher reload command.

---

### Reloading Warp Provider Changes

When developing a Content Patcher pack, changes to Warp Provider patches can be reloaded without restarting the game.

Use:

```text
tscore_cp_reload <ContentPackId>
```

For example:

```text
tscore_cp_reload YourName.MyWarpPack
```

After the Content Patcher pack is reloaded, changes to its `TsCore/WarpProviders` entries are reflected immediately.

This includes:

- adding a Warp Provider;
- removing a Warp Provider;
- changing the provider type;
- changing source or target locations;
- changing offsets;
- changing the fallback provider.

For details about `tscore_cp_reload`, ConfigSchema reloading, Config Tokens, GMCM integration, and DynamicTokens, see the [Content Patcher Integration](ModderGuide_ContentPatcherIntegration.md) guide.

---

### Reloading T's Core Resources

`tscore_reload` is used for resources loaded directly by T's Core.

Current commands are:

| Command | Reloads |
|---------|---------|
| `tscore_reload`<br>`tscore_reload all` | All supported directly loaded T's Core resources |
| `tscore_reload notification` | Notification Themes |

Running `tscore_reload` without an argument is equivalent to:

```text
tscore_reload all
```

Warp Providers are **not** reloaded through `tscore_reload`.

Use `tscore_cp_reload` for Warp Provider changes made by a Content Patcher pack.

---

### Inspecting Warp Providers

Use the following command to display all currently available Warp Providers:

```text
tscore_debug_warp
```

The output separates providers into three groups:

```text
Built-in Providers
TsCore Data Asset Providers
External Data Asset Providers
```

These represent:

- **Built-in Providers** — special providers implemented directly by T's Core.
- **TsCore Data Asset Providers** — default entries supplied by T's Core in `TsCore/WarpProviders`.
- **External Data Asset Providers** — providers added through Content Patcher or other edits to the data asset.

The output includes information such as:

- Provider ID
- Provider type
- Source / target locations
- Map used by `MapEntry`
- Building type
- Coordinate offsets
- Fallback provider

<details>
<summary>Example output</summary>

```text
tscore_debug_warp
[T's Core] ===== Warp Providers =====
[T's Core]
[T's Core] ----- Built-in Providers -----
[T's Core]
[T's Core] PlayerHome
[T's Core]     Type                : Built-in
[T's Core]     Destination         : Player's own home
[T's Core]
[T's Core] PreviousHome
[T's Core]     Type                : Built-in
[T's Core]     Destination         : Previously exited home
[T's Core]
[T's Core] CurrentHome
[T's Core]     Type                : Built-in
[T's Core]     Destination         : Current FarmHouse/Cabin
[T's Core]
[T's Core] ----- TsCore Data Asset Providers -----
[T's Core]
[T's Core] FarmCaveFront
[T's Core]     Type                : Warp
[T's Core]     Source              : FarmCave
[T's Core]     Target              : Farm
[T's Core]     Fallback            : (none)
[T's Core]
[T's Core] FarmHouseFront
[T's Core]     Type                : Warp
[T's Core]     Source              : FarmHouse
[T's Core]     Target              : Farm
[T's Core]     Fallback            : (none)
[T's Core]
[T's Core] GreenhouseFront
[T's Core]     Type                : Warp
[T's Core]     Source              : Greenhouse
[T's Core]     Target              : Farm
[T's Core]     Fallback            : (none)
[T's Core]
[T's Core] IslandFarmHouseFront
[T's Core]     Type                : Warp
[T's Core]     Source              : IslandFarmHouse
[T's Core]     Target              : IslandWest
[T's Core]     Fallback            : (none)
[T's Core]
[T's Core] ----- External Data Asset Providers -----
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

Use:

- `Warp` when you want the **destination of an existing warp**.
- `MapEntry` when you want the **position where an existing warp is located**.
- `Building` when you want a position relative to a **building placed on the farm**.

Warp destinations are resolved at runtime, allowing compatible Content Patcher packs to adapt to map changes without hardcoding coordinates.

Custom Warp Providers should be added through:

```text
TsCore/WarpProviders
```

using Content Patcher's `EditData` action.

The provider ID is the dictionary entry key and is not stored inside the provider model.

The special providers:

```text
PlayerHome
PreviousHome
CurrentHome
```

are implemented directly by T's Core and are not entries in `TsCore/WarpProviders`.

Additional providers and features may be added in future versions of T's Core.

---

## Modder Guide

- ← [Location Services](ModderGuide_LocationServices.md)
- ↑ [Guide Index](#top)
- → [Map Properties](ModderGuide_MapProperties.md)

← [Back to README](../README.md)

← [Back to Modder Guide](ModderGuide.md)
