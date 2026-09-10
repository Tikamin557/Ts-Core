# 📖 Modder Guide

This guide explains how to use the public features provided by **T's Core** in Content Patcher.

<a id="top"></a>

## Guide Index

- 📄 [Relationship Services](ModderGuide_RelationshipServices.md)
- 📄 [Location Services](ModderGuide_LocationServices.md)
- 📄 [Warp Services](ModderGuide_WarpServices.md)
- 📄 [Map Properties](ModderGuide_MapProperties.md)
- 📄 [Building Services](ModderGuide_BuildingServices.md)
- ✅ **Machine Interaction** *(Current Page)*
- 📄 [Migration System](ModderGuide_MigrationSystem.md)
- 📄 [Notification System](ModderGuide_NotificationSystem.md)
- 📄 [Content Patcher Integration](ModderGuide_ContentPatcherIntegration.md)

← [Back to README](../README.md)

← [Back to Modder Guide](ModderGuide.md)

---

# Machine Interaction

Machine Interaction allows Content Patcher packs to add custom right-click behavior to machines and Big Craftables.

It uses Stardew Valley's `Data/Machines` system together with the custom T's Core asset:

```text
TsCore/MachineInteraction
```

A Machine Interaction can execute a Tile Action and can optionally provide conditions, required items, Machine Effects, temporary Action effects, Idle effects, lights, wobble effects, and custom textures or animations.

No C# code is required.

---

## Contents

- [Overview](#overview)
- [Basic Setup](#basic-setup)
- [Interaction Data](#interaction-data)
- [Tile Action](#tile-action)
- [Conditions](#conditions)
- [Required Items](#required-items)
- [Action Effects](#action-effects)
- [Action Light](#action-light)
- [Action Wobble](#action-wobble)
- [Action Texture and Animation](#action-texture-and-animation)
- [Idle Effects](#idle-effects)
- [Idle Wobble](#idle-wobble)
- [Idle Texture and Animation](#idle-texture-and-animation)
- [Machine Light](#machine-light)
- [Texture Priority](#texture-priority)
- [Complete Example](#complete-example)
- [Notes](#notes)

---

## Overview

Machine Interaction extends machines defined in Stardew Valley's `Data/Machines`.

A machine is linked to a T's Core Machine Interaction through the following Custom Field:

```text
TsCore/MachineInteraction
```

For example:

```json
"CustomFields": {
    "TsCore/MachineInteraction": "MyMod/MyInteraction"
}
```

The value identifies an entry in:

```text
TsCore/MachineInteraction
```

The basic flow is:

```text
Player right-clicks the machine
        ↓
Data/Machines
        ↓
TsCore/MachineInteraction Custom Field
        ↓
T's Core Machine Interaction
        ↓
Condition
        ↓
Required Item
        ↓
Tile Action
        ↓
Action Effects / Light / Wobble
        ↓
Action Texture / Animation
```

Machine Interaction is used when the machine does not currently have output ready to harvest.

Normal machine output collection continues to use Stardew Valley's normal machine behavior.

---

## Basic Setup

A Machine Interaction requires two main steps:

1. Link a machine to an Interaction ID through `Data/Machines`.
2. Add the corresponding interaction to `TsCore/MachineInteraction`.

### Step 1: Link the Machine

Add the following Custom Field to the machine's `Data/Machines` entry:

```json
{
    "Action": "EditData",
    "Target": "Data/Machines",
    "TargetField": [
        "(BC)MyMod_MyMachine",
        "CustomFields"
    ],
    "Entries": {
        "TsCore/MachineInteraction": "MyMod/MyInteraction"
    }
}
```

In this example:

```text
MyMod/MyInteraction
```

is the Machine Interaction ID.

> **Note:** The machine must have a valid `Data/Machines` entry.

T's Core uses Stardew Valley's `InteractMethod` system to handle the interaction.

If the machine already defines another `InteractMethod`, T's Core does not overwrite it.

---

### Step 2: Add the Interaction

Add an entry to:

```text
TsCore/MachineInteraction
```

For example:

```json
{
    "Action": "EditData",
    "Target": "TsCore/MachineInteraction",
    "Entries": {
        "MyMod/MyInteraction": {
            "TileAction": "Message \"Hello!\""
        }
    }
}
```

Right-clicking the linked machine now executes the specified Tile Action.

---

## Interaction Data

The following properties are supported.

| Property | Default | Description |
|----------|---------|-------------|
| `TileAction` | — | Tile Action executed when the interaction succeeds. |
| `Condition` | — | Game State Query which must be satisfied before the interaction can continue. |
| `InvalidConditionMessage` | — | Message displayed when `Condition` is not satisfied. |
| `RequiredItem` | — | Item which must be held by the player. |
| `RequiredItemCount` | `1` | Required number of the held item. |
| `ConsumeRequiredItem` | `true` | Whether the required item is consumed after a successful interaction. |
| `InvalidItemMessage` | — | Message displayed when the required item is not being held. |
| `InvalidCountMessage` | — | Message displayed when the player is holding too few of the required item. |
| `ActionEffects` | — | Machine Effects played after a successful Tile Action. |
| `ActionEffectChance` | `1` | Chance that `ActionEffects` are played. |
| `ActionLight` | `false` | Whether to display a temporary light after a successful Tile Action. |
| `ActionLightOffset` | `"0, 0"` | Action Light offset from the machine, in tiles. |
| `ActionLightRadius` | `2` | Radius of the Action Light. |
| `ActionLightColor` | `"White"` | Color of the Action Light. |
| `ActionLightDurationMs` | `1000` | Duration of the Action Light in milliseconds. |
| `ActionWobble` | `false` | Whether the machine wobbles after a successful Tile Action. |
| `ActionWobbleDurationMs` | `1000` | Action Wobble duration in milliseconds. |
| `ActionTexture` | — | Texture asset displayed during the Action state. |
| `ActionTexturePosition` | `"0, 0"` | Base pixel position of the Action texture. |
| `ActionFrames` | — | Frame numbers used by the Action animation. |
| `ActionFrameDurationMs` | — | Action animation frame duration or durations. |
| `ActionDurationMs` | `1000` | Duration of a static Action texture. |
| `IdleEffects` | — | Machine Effects which may play while the machine is idle. |
| `IdleEffectChance` | `1` | Chance that `IdleEffects` are played. |
| `IdleWobble` | `false` | Whether the machine uses a wobble effect while idle. |
| `IdleTexture` | — | Texture asset displayed while the machine is idle. |
| `IdleTexturePosition` | `"0, 0"` | Base pixel position of the Idle texture. |
| `IdleFrames` | — | Frame numbers used by the Idle animation. |
| `IdleFrameDurationMs` | — | Idle animation frame duration or durations. |
| `Light` | `false` | Whether the machine has a normal T's Core light. |
| `LightOffset` | `"0, 0"` | Normal light offset from the machine, in tiles. |
| `LightRadius` | `2` | Radius of the normal light. |
| `LightColor` | `"White"` | Color of the normal light. |

Each group of properties is explained in more detail below.

---

## Tile Action

`TileAction` specifies the action executed when the player successfully interacts with the machine.

Example:

```json
{
    "TileAction": "Message \"Hello!\""
}
```

The action is executed at the machine's tile position.

Because T's Core uses Stardew Valley's Tile Action system, both vanilla Tile Actions and Tile Actions registered by compatible mods can be used.

For example, a vanilla warp can be used:

```json
{
    "TileAction": "Warp 10 10 FarmHouse"
}
```

T's Core custom Tile Actions can also be used.

For example:

```json
{
    "TileAction": "TsCoreMagicWarp FarmHouseFront"
}
```

If `TileAction` is missing or empty, the Machine Interaction is not executed.

---

## Conditions

`Condition` can be used to restrict when the Machine Interaction is available.

It uses Stardew Valley's Game State Query system.

Example:

```json
{
    "TileAction": "Message \"The condition was met!\"",
    "Condition": "PLAYER_HAS_MAIL MyMail"
}
```

If the condition is not satisfied, the Tile Action is not executed.

An optional message can be displayed using:

```json
{
    "Condition": "PLAYER_HAS_MAIL MyMail",
    "InvalidConditionMessage": "You can't use this yet."
}
```

The condition is checked before Required Item requirements.

The interaction context includes the current location, player, and held item.

---

## Required Items

A Machine Interaction can require the player to hold a specific item.

Example:

```json
{
    "TileAction": "Message \"The machine accepted the wood.\"",
    "RequiredItem": "(O)388"
}
```

`(O)388` is Wood.

Qualified and unqualified item IDs are supported.

### Required Item Count

Use `RequiredItemCount` to require more than one item.

```json
{
    "RequiredItem": "(O)388",
    "RequiredItemCount": 5
}
```

The player must be holding at least 5 Wood.

The required item is checked against the player's currently held object.

---

### Consuming the Required Item

By default, required items are consumed after the interaction succeeds.

```json
{
    "RequiredItem": "(O)388",
    "RequiredItemCount": 5,
    "ConsumeRequiredItem": true
}
```

To check for an item without consuming it:

```json
{
    "RequiredItem": "(O)388",
    "ConsumeRequiredItem": false
}
```

Required items are only consumed after the Tile Action succeeds.

---

### Invalid Item Message

`InvalidItemMessage` is displayed when the player is not holding the required item.

```json
{
    "RequiredItem": "(O)388",
    "InvalidItemMessage": "You need to hold Wood."
}
```

---

### Invalid Count Message

`InvalidCountMessage` is displayed when the correct item is being held, but the stack is too small.

```json
{
    "RequiredItem": "(O)388",
    "RequiredItemCount": 5,
    "InvalidCountMessage": "You need at least [ItemCount] Wood."
}
```

The following T's Core token is available in this message:

```text
[ItemCount]
```

It is replaced with the configured `RequiredItemCount`.

For the example above, the displayed message becomes:

```text
You need at least 5 Wood.
```

---

### Required Item and Machine Output Rules

Machine Interaction can coexist with Stardew Valley's normal machine `OutputRules`.

This means the same held item may be used by both systems depending on the machine configuration.

For example, a machine may accept an item through its normal `OutputRules` while its Machine Interaction also displays a message or plays an Action Effect.

If `RequiredItem` is only being used as an interaction check alongside normal machine input behavior, consider using:

```json
"ConsumeRequiredItem": false
```

to avoid consuming the same item through both systems.

---

## Action Effects

`ActionEffects` can play Stardew Valley Machine Effects after a successful Tile Action.

Example:

```json
{
    "ActionEffects": [
        {
            "Id": "MyActionEffect",
            "Sounds": [
                {
                    "Id": "coin"
                }
            ]
        }
    ]
}
```

The effect uses Stardew Valley's `MachineEffects` data structure.

Machine Effects can be used for effects such as sounds, shake effects, and temporary sprites supported by Stardew Valley's machine effect system.

T's Core attempts the configured effects in order and stops after the first effect which successfully plays.

> **Note:** The `Frames` field in Stardew Valley's `MachineEffects` data is not used by T's Core's Action/Idle effect playback. Use the T's Core Action or Idle animation properties described below when you want to animate the machine itself.

---

### Action Effect Chance

`ActionEffectChance` controls the chance that `ActionEffects` are played.

The value is between `0` and `1`.

For example:

```json
{
    "ActionEffectChance": 0.5
}
```

gives the Action Effects a 50% chance to play.

Default:

```text
1
```

---

## Action Light

A temporary light can be displayed after a successful Tile Action.

Example:

```json
{
    "ActionLight": true,
    "ActionLightOffset": "0, -0.5",
    "ActionLightRadius": 2.0,
    "ActionLightColor": "White",
    "ActionLightDurationMs": 1000
}
```

### Action Light Offset

`ActionLightOffset` uses the following format:

```text
"X, Y"
```

The values are measured in **tiles** relative to the center of the machine's placement tile.

Decimal and negative values are supported.

Examples:

| Value | Result |
|-------|--------|
| `"0, 0"` | Center of the machine tile. |
| `"0, -0.5"` | Half a tile upward. |
| `"0.5, 0"` | Half a tile to the right. |
| `"-1, 1"` | One tile left and one tile down. |

Since one Stardew Valley tile is 64 pixels:

```text
0.5 tile = 32 pixels
```

---

### Action Light Radius

`ActionLightRadius` controls the light radius.

Default:

```text
2
```

---

### Action Light Color

`ActionLightColor` specifies the light color.

Example:

```json
"ActionLightColor": "DarkCyan"
```

Color names supported by Stardew Valley / MonoGame can be used.

Default:

```text
White
```

---

### Action Light Duration

`ActionLightDurationMs` controls how long the Action Light remains visible.

The value is specified in milliseconds.

Example:

```json
"ActionLightDurationMs": 2000
```

displays the light for approximately 2 seconds.

Default:

```text
1000
```

If the interaction is triggered again before the previous Action Light expires, the duration is restarted.

---

## Action Wobble

`ActionWobble` temporarily applies the machine wobble effect used by Stardew Valley machines.

Example:

```json
{
    "ActionWobble": true,
    "ActionWobbleDurationMs": 1000
}
```

`ActionWobbleDurationMs` is specified in milliseconds.

Default:

```text
1000
```

Action Wobble has priority over Idle Wobble.

It does not change the machine's processing state.

---

## Action Texture and Animation

A custom texture can be displayed temporarily after a successful Machine Interaction.

The Action texture has the highest T's Core machine texture priority.

### Loading the Texture

First load the texture asset through Content Patcher.

For example:

```json
{
    "Action": "Load",
    "Target": "MyMod/MyMachineTexture",
    "FromFile": "assets/MyMachineTexture.png"
}
```

The same texture asset can be used for both Action and Idle textures.

---

### Static Action Texture

To display a static texture:

```json
{
    "ActionTexture": "MyMod/MyMachineTexture",
    "ActionTexturePosition": "0, 32",
    "ActionDurationMs": 2000
}
```

`ActionTexturePosition` specifies the top-left pixel coordinate of the first frame inside the texture.

The format is:

```text
"X, Y"
```

For example:

```text
"0, 32"
```

starts at pixel X `0`, Y `32`.

A standard Big Craftable frame is:

```text
16 × 32 pixels
```

If `ActionFrames` is not specified, frame `0` is displayed for `ActionDurationMs`.

---

### Animated Action Texture

To create an Action animation:

```json
{
    "ActionTexture": "MyMod/MyMachineTexture",
    "ActionTexturePosition": "0, 32",
    "ActionFrames": [ 0, 1, 2, 3, 2, 1 ],
    "ActionFrameDurationMs": [ 300, 300, 450, 1200, 450, 300 ]
}
```

Frames are arranged horizontally.

The source position for each frame is calculated as:

```text
X = ActionTexturePosition.X + (Frame × 16)
Y = ActionTexturePosition.Y
```

For example, with:

```text
ActionTexturePosition = "0, 32"
```

the frame positions are:

```text
Frame 0 → (0, 32)
Frame 1 → (16, 32)
Frame 2 → (32, 32)
Frame 3 → (48, 32)
```

Action animations play once from beginning to end.

When the animation finishes, the machine returns to the next applicable visual state.

---

### Action Frame Duration

`ActionFrameDurationMs` supports either a single value:

```json
"ActionFrameDurationMs": 300
```

or one value for each frame:

```json
"ActionFrameDurationMs": [ 300, 300, 450, 1200, 450, 300 ]
```

When a single value is used, every frame uses the same duration.

When an array is used, the number of duration values must exactly match the number of entries in `ActionFrames`.

If the counts do not match, the Action animation is disabled and T's Core logs a warning.

When `ActionFrames` is specified, `ActionDurationMs` is ignored.

The total Action duration is determined by the frame durations.

---

## Idle Effects

`IdleEffects` can play Machine Effects while the machine is idle.

A machine is considered idle for this feature when:

```text
readyForHarvest = false
and
minutesUntilReady <= 0
```

This means Idle Effects do not run while the machine is actively processing an item or has output ready to harvest.

Example:

```json
{
    "IdleEffects": [
        {
            "Id": "MyIdleEffect",
            "Sounds": [
                {
                    "Id": "bubbles"
                }
            ]
        }
    ],
    "IdleEffectChance": 0.5
}
```

Idle Effects are checked when the in-game time changes.

T's Core attempts the configured effects in order and stops after the first effect which successfully plays.

---

### Idle Effect Chance

`IdleEffectChance` controls the chance that `IdleEffects` are played when checked.

The value is between `0` and `1`.

Default:

```text
1
```

For example:

```json
"IdleEffectChance": 0.25
```

gives the configured Idle Effects a 25% chance to play each time they are checked.

---

## Idle Wobble

`IdleWobble` applies Stardew Valley's machine wobble effect while the machine is idle.

Example:

```json
{
    "IdleWobble": true
}
```

Idle Wobble is active only when:

```text
readyForHarvest = false
and
minutesUntilReady <= 0
```

It is disabled while the machine is processing or has output ready.

If Action Wobble is currently active, Action Wobble takes priority.

---

## Idle Texture and Animation

A custom texture can be displayed while the machine is idle.

Idle textures are only used when the machine is not processing and does not have output ready.

### Static Idle Texture

Example:

```json
{
    "IdleTexture": "MyMod/MyMachineTexture",
    "IdleTexturePosition": "0, 64"
}
```

`IdleTexturePosition` specifies the top-left pixel coordinate of the first frame.

The format is:

```text
"X, Y"
```

A standard Big Craftable frame is:

```text
16 × 32 pixels
```

If `IdleFrames` is not specified, frame `0` remains displayed for as long as the machine is idle.

---

### Animated Idle Texture

Example:

```json
{
    "IdleTexture": "MyMod/MyMachineTexture",
    "IdleTexturePosition": "0, 64",
    "IdleFrames": [ 0, 1, 2, 1 ],
    "IdleFrameDurationMs": 300
}
```

Idle animations loop continuously while the machine remains idle.

Frames are arranged horizontally.

The source position for each frame is calculated as:

```text
X = IdleTexturePosition.X + (Frame × 16)
Y = IdleTexturePosition.Y
```

For example:

```text
IdleTexturePosition = "0, 64"

Frame 0 → (0, 64)
Frame 1 → (16, 64)
Frame 2 → (32, 64)
```

---

### Idle Frame Duration

Like Action animations, `IdleFrameDurationMs` can be specified as a single value:

```json
"IdleFrameDurationMs": 300
```

or as one value for each frame:

```json
"IdleFrameDurationMs": [ 300, 500, 300, 500 ]
```

When an array is used, the number of values must exactly match the number of entries in `IdleFrames`.

If the counts do not match, the Idle animation is disabled and T's Core logs a warning.

---

## Machine Light

`Light` adds a normal T's Core light to the machine.

Example:

```json
{
    "Light": true,
    "LightOffset": "0, -0.5",
    "LightRadius": 2.0,
    "LightColor": "White"
}
```

Unlike `ActionLight`, this light is not limited to a specific duration.

It remains associated with the placed machine while the T's Core normal light is applicable.

If the machine is removed, the corresponding T's Core light is also removed.

---

### Light Offset

`LightOffset` uses the same tile-based format as `ActionLightOffset`:

```text
"X, Y"
```

Decimal and negative values are supported.

Examples:

```json
"LightOffset": "0, 0"
```

places the light at the center of the machine tile.

```json
"LightOffset": "0, -0.5"
```

moves the light half a tile upward.

```json
"LightOffset": "0.5, -1"
```

moves it half a tile to the right and one tile upward.

---

### Light Radius

`LightRadius` controls the radius of the normal light.

Default:

```text
2
```

---

### Light Color

`LightColor` specifies the normal light color.

Example:

```json
"LightColor": "DarkCyan"
```

Default:

```text
White
```

---

### Light Priority

T's Core respects temporary Action Lights and Stardew Valley's vanilla `LightWhileWorking`.

The priority is:

```text
Action Light
    ↓
Vanilla LightWhileWorking
    ↓
T's Core Machine Light
```

When an Action Light is active, the normal T's Core Machine Light is temporarily suppressed.

If the machine has vanilla `LightWhileWorking` data and is currently processing, the vanilla working light is used instead of the normal T's Core Machine Light.

When those higher-priority lights are no longer applicable, the normal T's Core Machine Light is restored.

---

## Texture Priority

T's Core Machine Interaction textures are designed to coexist with Stardew Valley's normal machine animation system.

The visual priority is:

```text
T's Core Action Texture / Animation
        ↓
Vanilla Working Texture / Animation
        ↓
T's Core Idle Texture / Animation
        ↓
Vanilla Big Craftable Texture
```

### Action

Action Texture / Animation has the highest priority.

It can temporarily override the machine's current visual state after a successful Machine Interaction.

When the Action state ends, the next applicable state is displayed.

---

### Working

Working textures and animations are handled entirely by Stardew Valley.

T's Core does not replace or modify the vanilla `WorkingTexture` or Working animation system.

While the machine is processing, the vanilla Working state takes priority over the T's Core Idle state.

---

### Idle

Idle Texture / Animation is displayed only when:

```text
readyForHarvest = false
and
minutesUntilReady <= 0
```

It is therefore suppressed while the machine is processing and when output is ready to harvest.

---

### Normal Texture

If no Action, Working, or Idle texture applies, Stardew Valley displays the machine's normal Big Craftable texture.

---

## Complete Example

The following example demonstrates the main Machine Interaction features together.

### Machine Data

First, link the machine to the Machine Interaction:

```json
{
    "Action": "EditData",
    "Target": "Data/Machines",
    "TargetField": [
        "(BC)MyMod_MyMachine",
        "CustomFields"
    ],
    "Entries": {
        "TsCore/MachineInteraction": "MyMod/MyMachineInteraction"
    }
}
```

---

### Texture

Load the texture used by the Action and Idle states:

```json
{
    "Action": "Load",
    "Target": "MyMod/MyMachineTexture",
    "FromFile": "assets/MyMachineTexture.png"
}
```

---

### Machine Interaction

```json
{
    "Action": "EditData",
    "Target": "TsCore/MachineInteraction",
    "Entries": {
        "MyMod/MyMachineInteraction": {
            "TileAction": "Message \"Machine Interaction Test\"",

            "Condition": "PLAYER_HAS_MAIL TestMail",
            "InvalidConditionMessage": "The condition has not been met.",

            "RequiredItem": "(O)388",
            "RequiredItemCount": 5,
            "ConsumeRequiredItem": true,
            "InvalidItemMessage": "You need to hold Wood.",
            "InvalidCountMessage": "You need at least [ItemCount] Wood.",

            "ActionEffects": [
                {
                    "Id": "ActionEffect1",
                    "Sounds": [
                        {
                            "Id": "coin"
                        }
                    ]
                }
            ],
            "ActionEffectChance": 1.0,

            "ActionLight": true,
            "ActionLightOffset": "0, -0.5",
            "ActionLightRadius": 2.0,
            "ActionLightColor": "White",
            "ActionLightDurationMs": 1000,

            "ActionWobble": true,
            "ActionWobbleDurationMs": 1000,

            "ActionTexture": "MyMod/MyMachineTexture",
            "ActionTexturePosition": "0, 32",
            "ActionFrames": [ 0, 1, 2, 3, 2, 1 ],
            "ActionFrameDurationMs": [ 300, 300, 450, 1200, 450, 300 ],
            "ActionDurationMs": 1000,

            "IdleEffects": [
                {
                    "Id": "IdleEffect1",
                    "Sounds": [
                        {
                            "Id": "bubbles"
                        }
                    ]
                }
            ],
            "IdleEffectChance": 1.0,

            "IdleWobble": true,

            "IdleTexture": "MyMod/MyMachineTexture",
            "IdleTexturePosition": "0, 64",
            "IdleFrames": [ 0, 1, 2, 1 ],
            "IdleFrameDurationMs": 300,

            "Light": true,
            "LightOffset": "0, -0.5",
            "LightRadius": 2.0,
            "LightColor": "White"
        }
    }
}
```

This example:

- requires the `PLAYER_HAS_MAIL TestMail` condition;
- requires the player to hold at least 5 Wood;
- consumes 5 Wood after a successful interaction;
- executes the configured Tile Action;
- plays an Action Machine Effect;
- displays a temporary Action Light;
- applies Action Wobble;
- plays a custom Action animation;
- plays Idle Effects while the machine is idle;
- applies Idle Wobble;
- loops a custom Idle animation;
- displays a normal T's Core Machine Light when applicable.

> **Note:** In this example, `ActionDurationMs` is included to show the available property, but it has no effect because `ActionFrames` is specified. The Action animation duration is determined by `ActionFrameDurationMs`.

---

## Notes

Machine Interaction is designed for Content Patcher packs and does not require custom C# code.

The interaction is linked to a machine through:

```text
Data/Machines
    CustomFields
        TsCore/MachineInteraction
```

The Custom Field value must match an entry in:

```text
TsCore/MachineInteraction
```

The main interaction order is:

```text
Condition
    ↓
Required Item
    ↓
Tile Action
    ↓
Action Effects / Light / Wobble
    ↓
Action Texture / Animation
    ↓
Required Item consumption
```

If the Condition fails, Required Item checks and the Tile Action are not performed.

If the Required Item check fails, the Tile Action is not performed.

Required items are consumed only after a successful Tile Action.

Machine Interaction can coexist with normal Stardew Valley machine `OutputRules`.

Action and Idle Machine Effects use Stardew Valley's `MachineEffects` data structure.

Action and Idle custom machine frames use standard Big Craftable frame dimensions:

```text
16 × 32 pixels
```

Action and Idle frames are arranged horizontally from their configured texture position.

Light offsets use **tile coordinates** and support decimal values:

```text
"0, -0.5"
```

Texture positions use **pixel coordinates**:

```text
"0, 64"
```

These values intentionally use different units:

| Property | Unit | Example |
|----------|------|---------|
| `ActionLightOffset` | Tiles | `"0, -0.5"` |
| `LightOffset` | Tiles | `"0.5, -1"` |
| `ActionTexturePosition` | Pixels | `"0, 32"` |
| `IdleTexturePosition` | Pixels | `"0, 64"` |

T's Core does not modify Stardew Valley's vanilla Working texture or Working animation behavior.

Additional Machine Interaction features may be added in future versions of T's Core.

---

## Modder Guide

- ← [Building Services](ModderGuide_BuildingServices.md)
- ↑ [Guide Index](#top)
- → [Migration System](ModderGuide_MigrationSystem.md)

← [Back to README](../README.md)

← [Back to Modder Guide](ModderGuide.md)
