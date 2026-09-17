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
- ✅ **Other Features** *(Current Page)*

← [Back to README](../README.md)

← [Back to Modder Guide](ModderGuide.md)

---

# Other Features

This page covers additional T's Core features which don't currently require a separate guide.

These features can be used by Content Patcher packs without requiring custom C# code.

---

## Contents

- [Tile Actions](#tile-actions)
  - [TsCore_Sleep](#tscore_sleep)
  - [TsCore_Sleep Remove](#tscore_sleep-remove)
- [Game State Queries](#game-state-queries)
  - [TsCore_LOCATION_CATEGORY](#tscore_location_category)
- [Notes](#notes)

---

# Tile Actions

T's Core provides custom Tile Actions which can be used anywhere Stardew Valley Tile Actions are supported.

They can also be used with the `TileAction` property of T's Core BigCraftable Extension.

---

## TsCore_Sleep

`TsCore_Sleep` allows the player to use the target Big Craftable as a sleeping location.

Example with BigCraftable Extension:

```json
{
    "TileAction": "TsCore_Sleep"
}
```

When the player interacts with the Big Craftable, T's Core performs the normal sleep checks and displays the sleep confirmation dialogue.

If the player chooses to sleep, the current location is used as a temporary sleeping location for that night.

In addition, T's Core remembers the sleeping location for future pass-outs.

This sleeping location is preserved when the game is saved and reloaded.

For example:

```text
Sleep using TsCore_Sleep
        ↓
Wake up at that location
        ↓
Later pass out outside at 2:00 AM
        ↓
Wake up at the previously used TsCore_Sleep location
```

The saved sleeping location identifies the Big Craftable using:

```text
Location
    +
Anchor position
    +
Big Craftable type
```

The Big Craftable type is identified by its Qualified Item ID.

If the Big Craftable is removed or moved to another position, the saved position is no longer considered a valid sleeping location.

If the same type of Big Craftable is later placed again at the same position, the sleeping location becomes valid again.

Using `TsCore_Sleep` at another Big Craftable replaces the previously remembered T's Core sleeping location.

> **Note:** Stardew Valley's normal restrictions for determining whether a location can be used as a wake-up location still apply.

---

## TsCore_Sleep Remove

`TsCore_Sleep Remove` works similarly to `TsCore_Sleep`, but is intended for disposable or single-use sleeping objects.

Example:

```json
{
    "TileAction": "TsCore_Sleep Remove"
}
```

The player can interact with the Big Craftable and sleep there normally.

After sleeping, the Big Craftable used for the action is removed.

Unlike `TsCore_Sleep`, this action doesn't preserve the Big Craftable as a persistent sleeping location.

Using `TsCore_Sleep Remove` also clears any previously remembered T's Core sleeping location.

This makes it useful for objects such as:

```text
Single-use sleeping bags
Temporary camps
Disposable sleeping objects
```

---

### TsCore_Sleep Comparison

| Action | Sleeps at the target location | Object removed after sleeping | Sleeping location remembered |
|--------|-------------------------------|-------------------------------|------------------------------|
| `TsCore_Sleep` | Yes | No | Yes |
| `TsCore_Sleep Remove` | Yes | Yes | No |

Both actions use Stardew Valley's normal sleep checks and sleep confirmation dialogue.

---

# Game State Queries

T's Core provides custom Game State Queries which can be used anywhere compatible Game State Query conditions are supported.

They can also be used with properties such as BigCraftable Extension's:

```text
Condition
PlacementCondition
```

---

## TsCore_LOCATION_CATEGORY

`TsCore_LOCATION_CATEGORY` checks whether a location belongs to a T's Core location category.

The format is:

```text
TsCore_LOCATION_CATEGORY <Location> <Category>
```

The location argument uses Stardew Valley's standard Game State Query location argument format.

For example:

```text
TsCore_LOCATION_CATEGORY Target Dungeon
```

checks whether the target location belongs to the `Dungeon` category.

Multiple categories can be specified:

```text
TsCore_LOCATION_CATEGORY Target Category1 Category2
```

The query succeeds if the target location matches any of the specified categories.

---

### Dungeon

The following category is currently supported:

```text
Dungeon
```

`Dungeon` identifies dungeon-style locations represented by Stardew Valley's mine and volcano dungeon location types.

This includes locations such as:

```text
The Mines
Skull Cavern
Quarry Mine
Volcano Dungeon
```

For example:

```text
TsCore_LOCATION_CATEGORY Target Dungeon
```

returns true when the target location is one of these dungeon locations.

The query can also be negated using normal Game State Query syntax:

```text
!TsCore_LOCATION_CATEGORY Target Dungeon
```

This is useful when a feature should only be available outside dungeon locations.

For example, BigCraftable Extension can prevent a Big Craftable from being placed in dungeons:

```json
{
    "PlacementCondition": "!TsCore_LOCATION_CATEGORY Target Dungeon"
}
```

---

## Notes

T's Core Tile Actions can be used with BigCraftable Extension:

```json
{
    "TileAction": "TsCore_Sleep"
}
```

or:

```json
{
    "TileAction": "TsCore_Sleep Remove"
}
```

`TsCore_Sleep` remembers the most recently used persistent T's Core sleeping location.

`TsCore_Sleep Remove` doesn't preserve a sleeping location and clears a previously remembered T's Core sleeping location.

The identity of a persistent T's Core sleeping location is based on:

```text
Location + Anchor position + Big Craftable type
```

It doesn't identify an individual physical object instance.

T's Core Game State Queries can be used with standard Game State Query syntax, including negation:

```text
!TsCore_LOCATION_CATEGORY Target Dungeon
```

The currently supported T's Core location category is:

```text
Dungeon
```

Additional Tile Actions, Game State Queries, and other smaller public features may be documented on this page in future versions of T's Core.

---

## Modder Guide

- ← [Content Patcher Integration](ModderGuide_ContentPatcherIntegration.md)
- ↑ [Guide Index](#top)
- → *(End of Guide)*

← [Back to README](../README.md)

← [Back to Modder Guide](ModderGuide.md)
