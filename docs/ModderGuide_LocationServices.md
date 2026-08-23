# 📖 Modder Guide

This guide explains how to use the public features provided by **T's Core** in Content Patcher.

<a id="top"></a>

## Guide Index

- 📄 [Relationship Services](ModderGuide_RelationshipServices.md)
- ✅ **Location Services** *(Current Page)*
- 📄 [Warp Services](ModderGuide_WarpServices.md)
- 📄 [Notification System](ModderGuide_NotificationSystem.md)
- 📄 [Content Patcher Integration](ModderGuide_ContentPatcherIntegration.md)

← [Back to README](../README.md)

← [Back to Modder Guide](ModderGuide.md)

---

# Location Services

Location Services provide **Content Patcher tokens** related to the player's current location and movement.

They are useful for creating location-aware Content Packs without requiring custom C# code.

---

## Contents

- [Available Tokens](#available-tokens)
- [Available Tokens in Detail](#available-tokens-in-detail)
- [Planned Tokens](#planned-tokens)
- [Common Use Cases](#common-use-cases)
- [Debugging](#debugging)
- [Notes](#notes)

---

## Available Tokens

All Location Services tokens use the following format:

```text
{{Tikamin557.TsCore/<TokenName>}}
```

| Token | Returns | Example | Status |
|-------|---------|---------|--------|
| `LocationElapsed` | Elapsed in-game minutes since entering the current location | `60` | ✅ Available |
| `PreviousLocation` | Previous location name | `Farm` | ✅ Available |
| `VisitCount` | Total visits to the current location | `7` | 🚧 Planned |
| `SessionVisitCount` | Visits during the current game session | `3` | 🚧 Planned |
| `EnteredToday` | Whether the current location has already been entered today | `true` | 🚧 Planned |
| `IsOutdoors` | Whether the current location is outdoors | `false` | ✅ Available |
| `IsIndoors` | Whether the current location is indoors | `true` | ✅ Available |

> **Note:** Tokens marked as 🚧 Planned are reserved for future versions of T's Core. Their current behavior is not finalized and should not be relied upon in released Content Packs.

---

## Available Tokens in Detail

### LocationElapsed

Returns the elapsed in-game minutes since the player entered the current location.

```json
"When": {
    "Query: {{Tikamin557.TsCore/LocationElapsed}} >= 60": true
},
"Update": "OnTimeChange"
```

This condition becomes true after the player has remained in the current location for at least **60 in-game minutes**.

> **Note:** `LocationElapsed` returns elapsed minutes, not the current time of day. Use `Update: OnTimeChange` when the patch needs to react as time passes.

---

### PreviousLocation

Returns the internal name of the location the player was in immediately before entering the current location.

```json
"When": {
    "Tikamin557.TsCore/PreviousLocation": "Farm"
},
"Update": "OnLocationChange"
```

This condition is true when the player entered the current location from `Farm`.

> **Note:** The token may return an empty value until the player changes locations for the first time after loading the save.

---

### IsOutdoors / IsIndoors

These tokens indicate whether the current location is considered outdoors or indoors.

| Token | Returns `true` when... |
|-------|-------------------------|
| `IsOutdoors` | The current location is outdoors |
| `IsIndoors` | The current location is indoors |

#### Outdoor example

```json
"When": {
    "Tikamin557.TsCore/IsOutdoors": "true"
},
"Update": "OnLocationChange"
```

#### Indoor example

```json
"When": {
    "Tikamin557.TsCore/IsIndoors": "true"
},
"Update": "OnLocationChange"
```

`IsIndoors` is normally the opposite of `IsOutdoors`.

> **Note:** The result depends on how the location is configured, including custom locations.

---

## Planned Tokens

The following tokens are registered but are **not yet fully implemented**:

| Token | Intended Purpose | Recommended Update |
|-------|------------------|--------------------|
| `VisitCount` | Track how many times the current location has been entered | `OnLocationChange` |
| `SessionVisitCount` | Track visits during the current game session | `OnLocationChange` |
| `EnteredToday` | Track whether the current location has been entered during the current in-game day | `OnLocationChange` |

The examples below show their intended future usage.

### VisitCount

```json
"When": {
    "Query: {{Tikamin557.TsCore/VisitCount}} >= 3": true
},
"Update": "OnLocationChange"
```

Intended to apply a patch from the third visit onward.

### SessionVisitCount

```json
"When": {
    "Query: {{Tikamin557.TsCore/SessionVisitCount}} >= 2": true
},
"Update": "OnLocationChange"
```

Intended to apply a patch after entering the location at least twice during the current game session.

### EnteredToday

```json
"When": {
    "Tikamin557.TsCore/EnteredToday": "true"
},
"Update": "OnLocationChange"
```

Intended to check whether the current location has already been entered during the current in-game day.

> **Important:** Planned tokens are shown for reference only. Do not rely on their current behavior in released Content Packs until they are marked as available.

---

## Common Use Cases

Location Services are useful for:

- Location-specific map edits
- Dynamic decorations
- Interior / exterior differences
- Dialogue based on location
- Event-specific patches
- Custom location support
- Time-based behavior after entering a location

---

### Reloading Content Patcher Content Packs

T's Core also provides development tools for reloading Content Patcher Content Packs while the game is running.

This includes support for reloading patches, ConfigSchema, Config Tokens, GMCM settings, and DynamicTokens without restarting the game.

For details about `tscore_cp_reload` and other Content Patcher integration features, see the [Content Patcher Integration](ModderGuide_ContentPatcherIntegration.md) guide.

---

The following SMAPI commands can be used to inspect token-related information:

| Command | Displays |
|---------|----------|
| `tscore_tokens` | All token-related information provided by T's Core |
| `tscore_tokens_location` | Location Services information |

Use `tscore_tokens_location` when you only need to inspect Location Services.

<details>
<summary>Example output</summary>

```text
tscore_tokens_location
[T's Core] ===== Location =====
[T's Core]
[T's Core]     Current Location    : Farm
[T's Core]     Previous Location   : BusStop
[T's Core]     Location Elapsed    : 20
[T's Core]     Visit Count         : 3
[T's Core]     Session Visit Count : 3
[T's Core]     Entered Today       : True
[T's Core]     Is Outdoors         : True
[T's Core]     Is Indoors          : False
```

</details>

> **Note:** Planned tokens are also displayed by the debug command. Their current values are intended for testing only and should not be relied upon until the features are fully implemented.

---

## Notes

Location Services are **read-only**.

They only expose location-related information through Content Patcher tokens and do not modify locations, warps, or player movement.

Some tokens update when the player changes locations, while others update as in-game time passes. Make sure to specify an appropriate `Update` value when using them in Content Patcher.

---

## Modder Guide

- ← [Relationship Services](ModderGuide_RelationshipServices.md)
- ↑ [Guide Index](#top)
- → [Warp Services](ModderGuide_WarpServices.md)

← [Back to README](../README.md)

← [Back to Modder Guide](ModderGuide.md)
