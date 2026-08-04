# 📖 Modder Guide

This guide explains how to use the public features provided by **T's Core** in Content Patcher.

## Guide Index

- 📄 [Relationship Services](ModderGuide_RelationshipServices.md)
- ✅ **Location Services** *(Current Page)*
- 📄 [Warp Services](ModderGuide_WarpServices.md) *(Coming Soon)*
- 📄 [Notification System](ModderGuide_NotificationSystem.md) *(Coming Soon)*
- 📄 [Content Patcher Tokens](ModderGuide_ContentPatcherTokens.md) *(Coming Soon)*

← [Back to Guide Index](ModderGuide.md)

---

# Location Services

Location Services provide Content Patcher tokens related to the player's current location and movement.

They are useful for creating location-aware Content Packs using dynamic location information provided by T's Core.

---

## Available Tokens

| Token | Returns | Example Value | Status |
|-------|---------|---------------|--------|
| `LocationElapsed` | Time elapsed since entering the current location | `60` | ✅ Available |
| `PreviousLocation` | Previous location name | `Farm` | ✅ Available |
| `VisitCount` | Total visits to the current location | `7` | 🚧 Planned |
| `SessionVisitCount` | Visits to the current location during the current game session | `3` | 🚧 Planned |
| `EnteredToday` | Whether the current location has already been entered today | `true` | 🚧 Planned |
| `IsOutdoors` | Whether the current location is outdoors | `false` | ✅ Available |
| `IsIndoors` | Whether the current location is indoors | `true` | ✅ Available |

All of these tokens can be used in Content Patcher using the following format:

```text
{{Tikamin557.TsCore/<TokenName>}}
```

> **Note:** Tokens marked as 🚧 Planned are reserved for future versions of T's Core. They are included for forward compatibility but are not yet recommended for use in released Content Packs. Their behavior may change before they become fully supported.

The sections below describe each available token in more detail.

---

## LocationElapsed

Returns the elapsed in-game minutes since the player entered the current location.

### Example — Apply a patch after staying for 60 minutes

```json
"When": {
    "Query: {{Tikamin557.TsCore/LocationElapsed}} >= 60": true
},
"Update": "OnTimeChange"
```

This patch is applied after the player has spent at least **60 in-game minutes** in the current location.

> **Note:** `LocationElapsed` returns elapsed minutes, not the current time of day. Since the value changes as time passes, remember to set `Update` to `OnTimeChange`.

---

## PreviousLocation

Returns the name of the location the player was in immediately before entering the current location.

The returned value is the internal location name used by Stardew Valley (the same name used by Content Patcher).

### Example — Check whether the player came from the Farm

```json
"When": {
    "Tikamin557.TsCore/PreviousLocation": "Farm"
},
"Update": "OnLocationChange"
```

This patch is applied when the player entered the current location from the **Farm**.

> **Note:** The token may return an empty value until the player changes locations for the first time after loading the save.

---

## VisitCount *(Not yet implemented)*

Returns the number of times the player has entered the current location.

> **Status:** This token is planned for a future version of T's Core and is **not yet fully implemented**.
>
> The example below demonstrates the intended usage once the feature becomes available. It should not be relied upon in released Content Packs at this time.

### Planned Example — Apply a patch from the third visit onward

```json
"When": {
    "Query: {{Tikamin557.TsCore/VisitCount}} >= 3": true
},
"Update": "OnLocationChange"
```

When fully implemented, this patch will be applied after the player has entered the current location at least **three times**.

> **Note:** Since this value changes when entering a location, `Update` should be set to `OnLocationChange`.

---

## SessionVisitCount *(Not yet implemented)*

Returns the number of times the player has entered the current location during the current game session.

> **Status:** This token is planned for a future version of T's Core and is **not yet fully implemented**.
>
> The example below demonstrates the intended usage once the feature becomes available. It should not be relied upon in released Content Packs at this time.

### Planned Example — Apply a patch after entering twice in the current session

```json
"When": {
    "Query: {{Tikamin557.TsCore/SessionVisitCount}} >= 2": true
},
"Update": "OnLocationChange"
```

When fully implemented, this patch will be applied after the player has entered the current location at least **two times during the current game session**.

> **Note:** Since this value changes when entering a location, `Update` should be set to `OnLocationChange`.

---

## EnteredToday *(Not yet implemented)*

Returns whether the current location has already been entered during the current in-game day.

> **Status:** This token is planned for a future version of T's Core and is **not yet fully implemented**.
>
> The example below demonstrates the intended usage once the feature becomes available. It should not be relied upon in released Content Packs at this time.

### Planned Example — Check whether the location has already been visited today

```json
"When": {
    "Tikamin557.TsCore/EnteredToday": "true"
},
"Update": "OnLocationChange"
```

When fully implemented, this patch will be applied if the current location has already been entered during the current in-game day.

> **Note:** Since this value changes when entering a location or when a new day begins, `Update` should be set to `OnLocationChange`.

---

## IsOutdoors

Returns `true` if the player's current location is considered outdoors; otherwise, it returns `false`.

### Example — Apply a patch only in outdoor locations

```json
"When": {
    "Tikamin557.TsCore/IsOutdoors": "true"
},
"Update": "OnLocationChange"
```

This patch is applied only while the player is in an **outdoor location**.

This can be useful for:

- Outdoor visual effects
- Weather-related changes
- Exterior decorations
- Location-specific ambient changes

> **Note:** The result depends on how the location is configured. This also applies to custom locations.

---

## IsIndoors

Returns `true` if the player's current location is considered indoors; otherwise, it returns `false`.

### Example — Apply a patch only in indoor locations

```json
"When": {
    "Tikamin557.TsCore/IsIndoors": "true"
},
"Update": "OnLocationChange"
```

This patch is applied only while the player is in an **indoor location**.

This can be useful for:

- Interior decorations
- Indoor lighting changes
- Furniture-related patches
- Effects that should not appear outdoors

`IsIndoors` is the opposite of `IsOutdoors`. When one token returns `true`, the other normally returns `false`.

> **Note:** The result depends on how the location is configured. This also applies to custom locations.

---

## Common Use Cases

Location Services are useful for:

- Location-specific map edits
- Dynamic decorations
- Seasonal area changes
- Interior / exterior differences
- Dialogue based on location
- Event-specific patches
- Support for custom locations

---

## Debugging

To inspect the current values provided by the Location Services, enter the following command into the SMAPI console:

```text
tscore_tokens_location
```

Example output:

```text
tscore_tokens_location
[T's Core] ===== Location =====
[T's Core] CurrentLocation: Farm
[T's Core] PreviousLocation: BusStop
[T's Core] LocationElapsed: 20
[T's Core] VisitCount: 3
[T's Core] SessionVisitCount: 3
[T's Core] EnteredToday: True
[T's Core] IsOutdoors: True
[T's Core] IsIndoors: False
```

> **Note:** Planned tokens are also displayed by the debug command. Their current values are intended for testing only and should not be relied upon until the features are fully implemented.

---

## Notes

Location Services are **read-only**.

They simply expose location-related information through Content Patcher tokens.

Some tokens update when the player changes locations, while others update as in-game time passes. Make sure to specify an appropriate `Update` value when using these tokens in Content Patcher.

Location Services are designed to provide simple location-aware conditions for Content Patcher without requiring custom tokens or C# code.

---

## Modder Guide

- ← [Relationship Services](ModderGuide_RelationshipServices.md)
- ↑ [Guide Index](ModderGuide.md)
- → [Warp Services](ModderGuide_WarpServices.md)
