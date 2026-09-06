# 📖 Modder Guide

This guide explains how to use the public features provided by **T's Core** in Content Patcher.

<a id="top"></a>

## Guide Index

- 📄 [Relationship Services](ModderGuide_RelationshipServices.md)
- 📄 [Location Services](ModderGuide_LocationServices.md)
- 📄 [Warp Services](ModderGuide_WarpServices.md)
- ✅ **Map Properties** *(Current Page)*
- 📄 [Building Services](ModderGuide_BuildingServices.md)
- 📄 [Migration System](ModderGuide_MigrationSystem.md)
- 📄 [Notification System](ModderGuide_NotificationSystem.md)
- 📄 [Content Patcher Integration](ModderGuide_ContentPatcherIntegration.md)

← [Back to README](../README.md)

← [Back to Modder Guide](ModderGuide.md)

---

# Map Properties

T's Core provides custom map properties which can add additional behavior to locations without requiring any C# code.

These properties can be added directly to maps and are intended for use with Content Patcher and other map-based mods.

This guide explains the custom map properties currently supported by T's Core.

---

## Contents

- [Timed Exit](#timed-exit)
- [TsCoreTimedExit](#tscoretimedexit)
- [TsCoreTimedExitMessage](#tscoretimedexitmessage)
- [TsCoreTimedExitSound](#tscoretimedexitsound)
- [Using Translated Messages](#using-translated-messages)
- [Examples](#examples)
- [Behavior](#behavior)
- [Error Handling](#error-handling)
- [Notes](#notes)

---

## Timed Exit

Timed Exit allows a location to automatically warp the player out at or after a specified in-game time.

It is configured using the following map properties:

| Property | Required | Description |
|----------|----------|-------------|
| `TsCoreTimedExit` | ✅ | Specifies the time and T's Core Warp Action used to exit the location. |
| `TsCoreTimedExitMessage` | ✅ | Specifies the message displayed before the warp, or disables the message. |
| `TsCoreTimedExitSound` | Optional | Specifies an audio cue played when the Timed Exit is triggered. |

Timed Exit uses the existing T's Core Warp Actions, so the destination can use:

- Warp Providers
- Location names
- Direct coordinates
- Facing directions
- Warp audio cues
- Audio repeat settings
- Blackout timing
- Audio start delays

For full information about Warp Actions and Warp Providers, see the [Warp Services](ModderGuide_WarpServices.md) guide.

---

## TsCoreTimedExit

`TsCoreTimedExit` specifies the in-game time when the Timed Exit becomes active and the Warp Action which should be performed.

### Syntax

```text
<Time> <WarpAction> <WarpAction arguments...>
```

For example:

```text
2100 TsCoreWarp Farm 64 15
```

or:

```text
2100 TsCoreMagicWarp FarmHouseFront Down
```

The first value is the in-game time.

Everything after the time uses the normal syntax of an existing T's Core Warp Action.

Supported Warp Actions:

- `TsCoreWarp`
- `TsCoreMagicWarp`
- `TsCoreMagicWarp_Simple`

Examples:

```text
2100 TsCoreWarp Farm 64 15
2100 TsCoreMagicWarp Farm 64 15
2100 TsCoreMagicWarp_Simple FarmHouseFront
2100 TsCoreMagicWarp FarmHouseFront Down wand
2100 TsCoreMagicWarp FarmHouseFront Auto wand 3 100 200 250
```

Timed Exit does not use a separate warp system.

The Warp Action is passed to the same T's Core Warp Services used by normal Tile Actions and Touch Actions.

This means that Warp Providers and optional Warp Action arguments behave the same way as described in the [Warp Services](ModderGuide_WarpServices.md) guide.

---

### Time Behavior

Timed Exit becomes active when the current in-game time is equal to or later than the configured time.

For example:

```text
2100 TsCoreWarp Farm 64 15
```

becomes active at:

```text
21:00
```

If the player is already inside the location when the configured time is reached, the Timed Exit is triggered.

If the player enters the location after the configured time, the Timed Exit is also triggered immediately.

For example, with:

```text
2100 TsCoreWarp Farm 64 15
```

the Timed Exit will trigger both when:

```text
20:50 → 21:00
```

and when the player enters the location at:

```text
22:00
```

This behavior prevents the player from avoiding the Timed Exit simply by entering the location after the configured time.

---

## TsCoreTimedExitMessage

`TsCoreTimedExitMessage` controls the message displayed when a Timed Exit is triggered.

The message is shown before the Warp Action is performed.

There are three supported formats.

---

### Direct Message

A normal value is displayed directly as dialogue.

```text
TsCoreTimedExitMessage = The facility is now closed.
```

The Warp Action is performed after the player closes the dialogue.

---

### No Message

Use:

```text
TsCoreTimedExitMessage = false
```

to disable the dialogue.

The Warp Action will be performed immediately when the Timed Exit is triggered.

The value is case-insensitive, so values such as:

```text
false
FALSE
False
```

are treated the same way.

Only the actual map property value is checked for `false`.

If a translated message happens to contain the text:

```text
false
```

it is displayed normally as dialogue.

---

### Strings/StringsFromMaps Key

If the entire value is enclosed in double quotes, T's Core treats it as a key in:

```text
Strings/StringsFromMaps
```

For example:

```text
TsCoreTimedExitMessage = "MyMod_TimedExitMessage"
```

T's Core will load:

```text
Strings/StringsFromMaps:MyMod_TimedExitMessage
```

and display the resulting text.

This format is useful for translated messages.

If the entire value is not enclosed in double quotes, it is treated as normal message text.

For example:

```text
The shop closes at "21:00".
```

is displayed directly because the entire value is not enclosed in double quotes.

---

### Dialogue Formatting

Timed Exit messages support the same basic dialogue formatting used by Stardew Valley map messages.

Use:

```text
^
```

for a line break.

Example:

```text
The facility is now closed.^Please come again tomorrow.
```

Displays approximately as:

```text
The facility is now closed.
Please come again tomorrow.
```

Use:

```text
#
```

to move to the next dialogue page.

Example:

```text
The facility is now closed.#Thank you for visiting.
```

The Warp Action is not performed when moving between pages.

It is performed only after the final dialogue is closed.

The two symbols can also be combined:

```text
The facility is now closed.^Please prepare to leave.#Thank you for visiting.
```

---

## TsCoreTimedExitSound

`TsCoreTimedExitSound` optionally plays a Stardew Valley audio cue when the Timed Exit is triggered.

Example:

```text
TsCoreTimedExitSound = crystal
```

The audio cue is played when the Timed Exit begins.

If a message is enabled, the sound is played when the message is displayed.

If:

```text
TsCoreTimedExitMessage = false
```

the sound is played immediately before the Warp Action begins.

Example:

```text
TsCoreTimedExit = 2100 TsCoreWarp Farm 64 15
TsCoreTimedExitMessage = The facility is now closed.
TsCoreTimedExitSound = crystal
```

The sequence is:

```text
Timed Exit triggered
    ↓
Play "crystal"
    ↓
Display message
    ↓
Player closes message
    ↓
Perform Warp Action
```

If no `TsCoreTimedExitSound` property is specified, no additional Timed Exit sound is played.

An empty value is also treated as no additional sound.

> **Note:** `TsCoreTimedExitSound` is separate from the audio cue configured in the Warp Action itself.
>
> This means a Timed Exit can play one sound when the exit message appears, while the Warp Action can use another sound during the actual warp.

For example:

```text
TsCoreTimedExit = 2100 TsCoreMagicWarp Farm 64 15 Auto wand
TsCoreTimedExitMessage = The facility is now closed.
TsCoreTimedExitSound = crystal
```

plays:

```text
crystal
```

when the Timed Exit is triggered, and:

```text
wand
```

as part of the Magic Warp.

---

## Using Translated Messages

For translated dialogue, add the message to:

```text
Strings/StringsFromMaps
```

using Content Patcher.

For example:

```json
{
    "Action": "EditData",
    "Target": "Strings/StringsFromMaps",
    "Entries": {
        "MyMod_TimedExitMessage": "{{i18n:MyMod_TimedExitMessage}}"
    }
}
```

Then add the corresponding entry to your Content Pack's i18n file.

Example:

```json
{
    "MyMod_TimedExitMessage": "The facility is now closed.^Please prepare to leave.#Thank you for visiting."
}
```

The map property can then reference that key:

```text
TsCoreTimedExitMessage = "MyMod_TimedExitMessage"
```

Content Patcher resolves the i18n token when editing `Strings/StringsFromMaps`, and T's Core reads the resulting translated text when the Timed Exit is triggered.

This allows the same map property to display the appropriate language for the current player.

> **Note:** Writing a Content Patcher i18n token directly into the map property is not supported.
>
> For example:
>
> ```text
> TsCoreTimedExitMessage = {{i18n:MyMod_TimedExitMessage}}
> ```
>
> will not be resolved by T's Core.
>
> Use `Strings/StringsFromMaps` as shown above instead.

---

## Examples

### Basic Timed Exit

The following properties display a message at 21:00 and warp the player to the farm after the dialogue is closed.

```text
TsCoreTimedExit = 2100 TsCoreWarp Farm 64 15
TsCoreTimedExitMessage = The facility is now closed.
```

---

### Timed Exit Without a Message

```text
TsCoreTimedExit = 2100 TsCoreWarp Farm 64 15
TsCoreTimedExitMessage = false
```

At or after 21:00, the Warp Action is performed immediately.

---

### Timed Exit With Sound

```text
TsCoreTimedExit = 2100 TsCoreWarp Farm 64 15
TsCoreTimedExitMessage = The facility is now closed.
TsCoreTimedExitSound = crystal
```

When the Timed Exit is triggered:

1. `crystal` is played.
2. The message is displayed.
3. The Warp Action is performed after the message is closed.

---

### Timed Exit With a Warp Provider

```text
TsCoreTimedExit = 2100 TsCoreMagicWarp FarmHouseFront Down
TsCoreTimedExitMessage = The facility is now closed.
TsCoreTimedExitSound = crystal
```

This uses the built-in:

```text
FarmHouseFront
```

Warp Provider instead of hardcoded destination coordinates.

---

### Timed Exit With a Translated Message

Map properties:

```text
TsCoreTimedExit = 2100 TsCoreWarp Farm 64 15
TsCoreTimedExitMessage = "MyMod_TimedExitMessage"
TsCoreTimedExitSound = crystal
```

Content Patcher:

```json
{
    "Action": "EditData",
    "Target": "Strings/StringsFromMaps",
    "Entries": {
        "MyMod_TimedExitMessage": "{{i18n:MyMod_TimedExitMessage}}"
    }
}
```

i18n:

```json
{
    "MyMod_TimedExitMessage": "The facility is now closed.^Please prepare to leave.#Thank you for visiting."
}
```

---

### Content Patcher EditMap Example

Timed Exit properties can also be added to a map using Content Patcher.

For example:

```json
{
    "Action": "EditMap",
    "Target": "Maps/Custom_MyLocation",
    "MapProperties": {
        "TsCoreTimedExit": "2100 TsCoreMagicWarp FarmHouseFront Down",
        "TsCoreTimedExitMessage": "\"MyMod_TimedExitMessage\"",
        "TsCoreTimedExitSound": "crystal"
    }
}
```

The translated text can be registered separately:

```json
{
    "Action": "EditData",
    "Target": "Strings/StringsFromMaps",
    "Entries": {
        "MyMod_TimedExitMessage": "{{i18n:MyMod_TimedExitMessage}}"
    }
}
```

---

## Behavior

Timed Exit is checked in two main situations:

- when the in-game time changes;
- when the local player enters a location.

This allows the feature to handle both players who are already inside the location and players who enter after the configured exit time.

Once a Timed Exit begins processing, T's Core waits for its dialogue to finish before performing the Warp Action.

If the message contains multiple pages separated by:

```text
#
```

the player can move through those pages normally.

The Warp Action begins only after the complete dialogue has been closed.

When:

```text
TsCoreTimedExitMessage = false
```

there is no dialogue delay and the Warp Action begins immediately.

---

## Error Handling

T's Core validates the Timed Exit properties before performing the exit.

Invalid configuration is written to the SMAPI log.

Examples include:

- missing `TsCoreTimedExitMessage`;
- empty `TsCoreTimedExitMessage`;
- invalid time values;
- unsupported Warp Actions;
- missing `Strings/StringsFromMaps` keys;
- empty translation keys;
- invalid Warp Action arguments.

For configuration errors which prevent T's Core from determining how the Timed Exit should work, the Warp Action is not performed.

For example, if:

```text
TsCoreTimedExitMessage = "MissingMessageKey"
```

references a key which does not exist in:

```text
Strings/StringsFromMaps
```

T's Core logs an error and does not perform the Timed Exit.

---

### Invalid Audio Cues

An invalid value in:

```text
TsCoreTimedExitSound
```

does not prevent the Timed Exit from continuing.

For example:

```text
TsCoreTimedExitSound = InvalidAudioCue
```

causes T's Core to log an error, but the normal Timed Exit sequence continues.

If a message is enabled:

```text
Invalid audio cue
    ↓
Error logged
    ↓
Display message
    ↓
Perform Warp Action
```

If the message is disabled:

```text
Invalid audio cue
    ↓
Error logged
    ↓
Perform Warp Action
```

This prevents an optional sound configuration error from blocking the actual exit.

---

## Notes

Map Properties are designed for map-based mods and Content Patcher Content Packs and require no custom C# code.

Timed Exit currently supports the following properties:

```text
TsCoreTimedExit
TsCoreTimedExitMessage
TsCoreTimedExitSound
```

`TsCoreTimedExit` uses the existing T's Core Warp Actions rather than implementing a separate warp system.

For this reason, all normal Warp Action features remain available, including Warp Providers and optional warp effects.

When possible, Warp Providers are recommended instead of hardcoded coordinates because they can adapt more easily to compatible custom maps and location changes.

For detailed Warp Action syntax and available Warp Providers, see the [Warp Services](ModderGuide_WarpServices.md) guide.

Additional custom map properties may be added in future versions of T's Core.

---

## Modder Guide

- ← [Warp Services](ModderGuide_WarpServices.md)
- ↑ [Guide Index](#top)
- → [Building Services](ModderGuide_BuildingServices.md)

← [Back to README](../README.md)

← [Back to Modder Guide](ModderGuide.md)
