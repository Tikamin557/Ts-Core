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

T's Core also supports registering additional warp providers.

This allows other mods to expose reusable warp destinations that can be used by any Content Pack.

*(Custom provider registration will be documented in a future update.)*

See **Built-in Warp Providers** below for the list of providers included with T's Core.

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
tscore_tokens_warp
```

*(Coming Soon)*

---

## Notes

Warp Services are fully compatible with Content Patcher and require no C# code.

Additional warp providers and features may be added in future versions of T's Core without requiring changes to existing Content Packs.

---

## Modder Guide

- ← [Location Services](ModderGuide_LocationServices.md)
- ↑ [Guide Index](#top)
- → [Notification System](ModderGuide_NotificationSystem.md) *(Coming Soon)*

← [Back to README](../README.md)

← [Back to Modder Guide](ModderGuide.md)
