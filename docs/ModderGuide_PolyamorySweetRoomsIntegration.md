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
- 📄 [Post Renovation Patch](ModderGuide_PostRenovationPatch.md)
- ✅ **Polyamory Sweet Rooms Integration** *(Current Page)*
- 📄 [Other Features](ModderGuide_OtherFeatures.md)

← [Back to README](../README.md)

← [Back to Modder Guide](ModderGuide.md)

---

# Polyamory Sweet Rooms Integration

T's Core allows supported Farmhouse mods to provide room presets for **Polyamory Sweet Rooms (PSR)**.

A Farmhouse Content Pack can describe the spouse room slots it provides through:

```text
TsCore/PsrRoomPresets
```

Players can then use T's Core's **Spouse Room Setup** screen to assign characters to those slots.

No custom C# code is required for the Farmhouse mod.

---

## Contents

- [Requirements](#requirements)
- [Data Asset](#data-asset)
- [Preset Properties](#preset-properties)
- [Slot Properties](#slot-properties)
- [Additional Candidates](#additional-candidates)
- [Complete Example](#complete-example)
- [How Assignments Are Saved](#how-assignments-are-saved)
- [Player Access](#player-access)
- [Important Notes](#important-notes)

---

## Requirements

This integration requires:

- T's Core;
- Polyamory Sweet Rooms;
- a Farmhouse mod which registers a compatible preset;
- a Polyamory Sweet Rooms Content Pack whose SMAPI Mod ID is specified by the preset.

The preset itself is normally registered by the Farmhouse mod through Content Patcher.

---

## Data Asset

Register presets by editing:

```text
TsCore/PsrRoomPresets
```

Each entry represents one room preset.

Example structure:

```json
{
  "Action": "EditData",
  "Target": "TsCore/PsrRoomPresets",
  "Entries": {
    "{{ModId}}_Main": {
      "PsrContentPackId": "Example.Author.PSRContentPack",
      "Slots": [
        {
          "Id": "Room1",
          "DisplayName": "Room 1",
          "StartPosition": {
            "X": 10,
            "Y": 20
          },
          "SpousePositionOffset": {
            "X": 3,
            "Y": 3
          },
          "ShellType": "Normal"
        }
      ]
    }
  }
}
```

---

## Preset Properties

| Property | Required | Description |
|----------|----------|-------------|
| Entry key | ✅ | Unique ID for the preset. |
| `PsrContentPackId` | ✅ | SMAPI Mod ID of the Polyamory Sweet Rooms Content Pack whose `content.json` will be written by T's Core. |
| `AdditionalCandidates` | ❌ | Additional NPCs which should be available as assignment candidates when they can't be detected through the normal relationship/romanceability checks. |
| `Slots` | ✅ | Ordered list of spouse room slots displayed and saved by T's Core. |

The order of `Slots` is also the order used when T's Core writes the Polyamory Sweet Rooms room data.

---

## Slot Properties

Each entry in `Slots` describes one spouse room.

| Property | Required | Description |
|----------|----------|-------------|
| `Id` | ✅ | Internal ID for the slot. Also used as the PSR `name` when the room is left unassigned. IDs should be unique within the preset. |
| `DisplayName` | ❌ | Room name shown in the T's Core setup screen. If empty, `Id` is shown instead. |
| `HoverText` | ❌ | Optional text shown when the player hovers over the room name. |
| `StartPosition` | ✅ | Written to PSR as `startPos`. |
| `SpousePositionOffset` | ❌ | Written to PSR as `spousePosOffset`. Defaults to `3, 3`. |
| `ShellType` | ✅ | Written to PSR as `shellType`. |

Example:

```json
{
  "Id": "UpperRoom",
  "DisplayName": "Upper Room",
  "HoverText": "The upper spouse room.",
  "StartPosition": {
    "X": 40,
    "Y": 8
  },
  "SpousePositionOffset": {
    "X": 3,
    "Y": 3
  },
  "ShellType": "Normal"
}
```

---

## Additional Candidates

T's Core normally builds the assignment list from characters available through its relationship and romanceability services.

`AdditionalCandidates` can be used for NPCs which should be selectable even when they can't be detected through those normal checks.

Each key is the NPC's internal name, and `ModId` specifies the mod which must be loaded for that candidate to be available.

Example:

```json
"AdditionalCandidates": {
  "ExampleNPC": {
    "ModId": "Example.Author.NPCMod"
  }
}
```

This can be useful when an NPC should be available as a room assignment candidate before that NPC is currently present in `Data/Characters` for the player's current progression state.

---

## Complete Example

```json
{
  "Action": "EditData",
  "Target": "TsCore/PsrRoomPresets",
  "Entries": {
    "{{ModId}}_Main": {
      "PsrContentPackId": "Example.Author.PSRContentPack",
      "AdditionalCandidates": {
        "ExampleNPC": {
          "ModId": "Example.Author.NPCMod"
        }
      },
      "Slots": [
        {
          "Id": "UpperRoom",
          "DisplayName": "Upper Room",
          "HoverText": "The upper spouse room.",
          "StartPosition": {
            "X": 40,
            "Y": 8
          },
          "SpousePositionOffset": {
            "X": 3,
            "Y": 3
          },
          "ShellType": "Normal"
        },
        {
          "Id": "LowerRoom",
          "DisplayName": "Lower Room",
          "HoverText": "The lower spouse room.",
          "StartPosition": {
            "X": 40,
            "Y": 16
          },
          "SpousePositionOffset": {
            "X": 3,
            "Y": 3
          },
          "ShellType": "Normal"
        }
      ]
    }
  }
}
```

---

## How Assignments Are Saved

When the player confirms the setup, T's Core writes the selected room assignments to the `content.json` of the Content Pack specified by `PsrContentPackId`.

For each slot, T's Core writes:

- the selected character name as PSR `name`;
- `StartPosition` as `startPos`;
- `SpousePositionOffset` as `spousePosOffset`;
- `ShellType` as `shellType`.

If a slot is left unassigned, the slot's `Id` is written as its PSR `name`. This preserves the room definition without assigning that room to a character.

The generated room order follows the order of `Slots` in the preset.

> **Important**
>
> Saving from the Spouse Room Setup screen overwrites the target Polyamory Sweet Rooms Content Pack's `content.json`.
>
> The game must be restarted for the saved Polyamory Sweet Rooms configuration to take effect.

---

## Player Access

When the required mods and a compatible preset are available, players can open the Spouse Room Setup screen through T's Core.

It is available from:

- T's Core's Generic Mod Config Menu settings;
- the built-in T's Core function in the Shortcut Panel.

A save must be loaded before the setup screen can be used.

---

## Important Notes

- `PsrContentPackId` must match the SMAPI Mod ID of the target Polyamory Sweet Rooms Content Pack.
- The target PSR Content Pack must be installed and loaded.
- `Id` values should be unique within a preset.
- The order of `Slots` controls the order written to the PSR configuration.
- A character can't be assigned to multiple slots through the T's Core setup screen.
- Saving overwrites the target PSR Content Pack's `content.json`.
- Restart Stardew Valley after saving before testing the new room assignments.
- `AdditionalCandidates` should only be used when the normal candidate detection isn't sufficient.

---

## Modder Guide

- ← [Post Renovation Patch](ModderGuide_PostRenovationPatch.md)
- ↑ [Guide Index](#top)
- → [Other Features](ModderGuide_OtherFeatures.md)

← [Back to README](../README.md)

← [Back to Modder Guide](ModderGuide.md)
