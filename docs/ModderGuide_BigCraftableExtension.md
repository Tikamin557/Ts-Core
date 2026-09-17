# 📖 Modder Guide

This guide explains how to use the public features provided by **T's Core** in Content Patcher.

<a id="top"></a>

## Guide Index

- 📄 [Relationship Services](ModderGuide_RelationshipServices.md)
- 📄 [Location Services](ModderGuide_LocationServices.md)
- 📄 [Warp Services](ModderGuide_WarpServices.md)
- 📄 [Map Properties](ModderGuide_MapProperties.md)
- 📄 [Building Services](ModderGuide_BuildingServices.md)
- ✅ **BigCraftable Extension** *(Current Page)*
- 📄 [Dialogue System](ModderGuide_DialogueSystem.md)
- 📄 [Migration System](ModderGuide_MigrationSystem.md)
- 📄 [Notification System](ModderGuide_NotificationSystem.md)
- 📄 [Content Patcher Integration](ModderGuide_ContentPatcherIntegration.md)
- 📄 [Other Features](ModderGuide_OtherFeatures.md)

← [Back to README](../README.md)

← [Back to Modder Guide](ModderGuide.md)

---

# BigCraftable Extension

BigCraftable Extension allows Content Patcher packs to add additional features to Big Craftables.

It uses Stardew Valley's `Data/BigCraftables` system together with the custom T's Core asset:

```text
TsCore/BigCraftableExtension
```

A BigCraftable Extension can provide:

- custom collision sizes;
- placement conditions using Game State Queries;
- custom texture sizes;
- custom normal textures and animations;
- Tile Actions;
- interaction conditions using Game State Queries;
- required held items;
- Machine Effects;
- temporary Action lights;
- Action wobble effects;
- Action textures and animations;
- Idle effects;
- Idle wobble effects;
- normal lights.

BigCraftable Extension can be used with normal Big Craftables and does not require the Big Craftable to have a `Data/Machines` entry for its general BigCraftable Extension features.

No C# code is required.

---

## Contents

- [Overview](#overview)
- [Basic Setup](#basic-setup)
- [BigCraftable Extension Data](#bigcraftable-extension-data)
- [Collision Size](#collision-size)
- [Placement Condition](#placement-condition)
- [Texture Size](#texture-size)
- [Normal Texture and Animation](#normal-texture-and-animation)
- [Tile Action](#tile-action)
- [Conditions](#conditions)
- [Required Items](#required-items)
- [Action Effects](#action-effects)
- [Action Light](#action-light)
- [Action Wobble](#action-wobble)
- [Action Texture and Animation](#action-texture-and-animation)
- [Idle Effects](#idle-effects)
- [Idle Wobble](#idle-wobble)
- [Normal Light](#normal-light)
- [Texture Priority](#texture-priority)
- [Complete Example](#complete-example)
- [Notes](#notes)

---

## Overview

A Big Craftable is linked to a T's Core BigCraftable Extension through the following Custom Field in `Data/BigCraftables`:

```text
TsCore/BigCraftableExtension
```

For example:

```json
"CustomFields": {
    "TsCore/BigCraftableExtension": "MyMod/MyExtension"
}
```

The value identifies an entry in:

```text
TsCore/BigCraftableExtension
```

The basic relationship is:

```text
Data/BigCraftables
        ↓
CustomFields
        ↓
TsCore/BigCraftableExtension
        ↓
BigCraftable Extension Data
```

Interaction features use the following general flow:

```text
Player interacts with the Big Craftable
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
        ↓
Required Item consumption
```

BigCraftable Extension also provides features which don't require interaction, such as:

```text
Collision Size
Texture Size
Normal Texture / Animation
Idle Effects
Idle Wobble
Normal Light
```

---

## Basic Setup

A BigCraftable Extension requires two main steps:

1. Link a Big Craftable to an Extension ID through `Data/BigCraftables`.
2. Add the corresponding entry to `TsCore/BigCraftableExtension`.

### Step 1: Link the Big Craftable

Add the following Custom Field to the Big Craftable's `Data/BigCraftables` entry:

```json
{
    "Action": "EditData",
    "Target": "Data/BigCraftables",
    "TargetField": [
        "MyMod_MyBigCraftable",
        "CustomFields"
    ],
    "Entries": {
        "TsCore/BigCraftableExtension": "MyMod/MyExtension"
    }
}
```

In this example:

```text
MyMod/MyExtension
```

is the BigCraftable Extension ID.

You can also include the Custom Field directly when adding the Big Craftable:

```json
{
    "Action": "EditData",
    "Target": "Data/BigCraftables",
    "Entries": {
        "MyMod_MyBigCraftable": {
            "Name": "MyBigCraftable",
            "DisplayName": "My Big Craftable",
            "Description": "An example Big Craftable.",
            "Price": 500,
            "Fragility": 0,
            "CanBePlacedOutdoors": true,
            "CanBePlacedIndoors": true,
            "Texture": "MyMod/MyBigCraftableTexture",
            "SpriteIndex": 0,
            "CustomFields": {
                "TsCore/BigCraftableExtension": "MyMod/MyExtension"
            }
        }
    }
}
```

---

### Step 2: Add the Extension

Add an entry to:

```text
TsCore/BigCraftableExtension
```

For example:

```json
{
    "Action": "EditData",
    "Target": "TsCore/BigCraftableExtension",
    "Entries": {
        "MyMod/MyExtension": {
            "TileAction": "Message \"Hello!\""
        }
    }
}
```

Interacting with the linked Big Craftable now executes the specified Tile Action.

---

## BigCraftable Extension Data

The following properties are supported.

| Property | Default | Description |
|----------|---------|-------------|
| `CollisionSize` | `"1, 1"` | Collision width and height in tiles. |
| `PlacementCondition` | — | Game State Query which must be satisfied for the Big Craftable to be placed. |
| `Texture` | — | Optional normal texture override. |
| `TexturePosition` | — | Optional base pixel position for the normal texture. |
| `TextureSize` | `"16, 32"` | Width and height of one texture frame in pixels. |
| `Frames` | — | Frame numbers used by the normal animation. |
| `FrameDurationMs` | — | Normal animation frame duration or durations. |
| `TileAction` | — | Tile Action executed when the interaction succeeds. |
| `Condition` | — | Game State Query which must be satisfied before interaction can continue. |
| `InvalidConditionMessage` | — | Message displayed when `Condition` is not satisfied. |
| `RequiredItem` | — | Item which must be held by the player. |
| `RequiredItemCount` | `1` | Required number of the held item. |
| `ConsumeRequiredItem` | `true` | Whether the required item is consumed after a successful interaction. |
| `InvalidItemMessage` | — | Message displayed when the required item is not being held. |
| `InvalidCountMessage` | — | Message displayed when too few of the required item are held. |
| `ActionEffects` | — | Machine Effects played after a successful Tile Action. |
| `ActionEffectChance` | `1` | Chance that `ActionEffects` are played. |
| `ActionLight` | `false` | Whether to display a temporary light after a successful Tile Action. |
| `ActionLightOffset` | `"0, 0"` | Action Light offset in tiles. |
| `ActionLightRadius` | `2` | Radius of the Action Light. |
| `ActionLightColor` | `"White"` | Color of the Action Light. |
| `ActionLightDurationMs` | `1000` | Duration of the Action Light in milliseconds. |
| `ActionWobble` | `false` | Whether the Big Craftable wobbles after a successful Tile Action. |
| `ActionWobbleDurationMs` | `1000` | Action Wobble duration in milliseconds. |
| `ActionTexture` | — | Texture asset displayed during the Action state. |
| `ActionTexturePosition` | `"0, 0"` | Base pixel position of the Action texture. |
| `ActionFrames` | — | Frame numbers used by the Action animation. |
| `ActionFrameDurationMs` | — | Action animation frame duration or durations. |
| `ActionDurationMs` | `1000` | Duration of a static Action texture. |
| `IdleEffects` | — | Machine Effects which may play while the Big Craftable is idle. |
| `IdleEffectChance` | `1` | Chance that `IdleEffects` are played. |
| `IdleWobble` | `false` | Whether the Big Craftable uses a wobble effect while idle. |
| `Light` | `false` | Whether the Big Craftable has a normal T's Core light. |
| `LightOffset` | `"0, 0"` | Normal light offset in tiles. |
| `LightRadius` | `2` | Radius of the normal light. |
| `LightColor` | `"White"` | Color of the normal light. |

Each group of properties is explained below.

---

## Collision Size

`CollisionSize` changes the collision footprint of the Big Craftable.

The format is:

```text
"Width, Height"
```

The values are measured in tiles.

Example:

```json
{
    "CollisionSize": "1, 2"
}
```

This creates a collision footprint which is 1 tile wide and 2 tiles high.

The default is:

```text
"1, 1"
```

### Collision Direction

The actual placed Big Craftable object is used as the bottom-left anchor of the collision footprint.

Width extends to the right.

Height extends upward.

For example:

```json
"CollisionSize": "1, 2"
```

produces:

```text
[X]
[A]
```

where:

```text
A = actual Big Craftable placement tile
X = additional collision tile
```

A 3 × 3 collision footprint:

```json
"CollisionSize": "3, 3"
```

produces:

```text
[X][X][X]
[X][X][X]
[A][X][X]
```

Only the anchor tile contains the actual Stardew Valley Object.

The additional footprint tiles are handled virtually by T's Core.

This allows a Big Craftable to occupy a larger area without creating duplicate Objects on the additional tiles.

The extended footprint is used by T's Core for features including placement, collision, interaction, and object removal handling.

---

## Placement Condition

`PlacementCondition` can restrict where the Big Craftable can be placed.

It uses Stardew Valley's Game State Query system.

For example, T's Core provides the `TsCore_LOCATION_CATEGORY` Game State Query, which can be used to prevent placement in dungeon locations:

```json
{
    "PlacementCondition": "!TsCore_LOCATION_CATEGORY Target Dungeon"
}
```

In this example, the Big Craftable can be placed normally outside dungeon locations, but can't be placed in locations categorized as `Dungeon`.

The condition is checked when the player attempts to place the Big Craftable.

If the condition isn't satisfied, the Big Craftable can't be placed at that location.

The Game State Query context includes the target location and the current player.

> **Note:** `PlacementCondition` only controls whether the Big Craftable can be newly placed. It doesn't remove or disable an already placed Big Craftable if the condition later becomes false.

`PlacementCondition` is separate from `Condition`.

```text
PlacementCondition
    ↓
Checked when placing the Big Craftable

Condition
    ↓
Checked when interacting with the placed Big Craftable
```

If `PlacementCondition` is omitted, T's Core doesn't add any additional placement condition.

---

## Texture Size

`TextureSize` specifies the size of one Big Craftable texture frame.

The format is:

```text
"Width, Height"
```

The values are measured in source texture pixels.

Example:

```json
{
    "TextureSize": "17, 32"
}
```

means:

```text
Width  = 17 pixels
Height = 32 pixels
```

The default is:

```text
"16, 32"
```

This is the standard Stardew Valley Big Craftable frame size.

`TextureSize` is also used as the frame size for T's Core normal and Action animations.

For example, with:

```json
"TextureSize": "17, 32"
```

horizontally arranged frames begin every 17 pixels.

---

## Normal Texture and Animation

BigCraftable Extension can override the normal texture used by a Big Craftable and can optionally animate it.

The relevant properties are:

```text
Texture
TexturePosition
TextureSize
Frames
FrameDurationMs
```

### Normal Texture

`Texture` specifies an optional texture asset.

Example:

```json
{
    "Texture": "MyMod/MyBigCraftableTexture"
}
```

If `Texture` is omitted, T's Core uses the normal texture associated with the Big Craftable.

This makes it possible to use BigCraftable Extension features without defining a separate texture.

---

### Texture Position

`TexturePosition` specifies the top-left pixel position of frame 0.

The format is:

```text
"X, Y"
```

Example:

```json
{
    "TexturePosition": "0, 32"
}
```

If `TexturePosition` is omitted, T's Core preserves the normal source position determined by the Big Craftable's Stardew Valley data.

This means a simple extension can use:

```json
{
    "TextureSize": "17, 32"
}
```

without needing to repeat the normal texture position.

---

### Normal Animation

Use `Frames` to create a continuously looping normal animation.

Example:

```json
{
    "TextureSize": "17, 32",
    "Frames": [ 0, 1, 2, 3 ],
    "FrameDurationMs": 300
}
```

Frames are arranged horizontally.

The source X position is calculated using the configured frame width:

```text
X = BasePosition.X + (Frame × TextureWidth)
Y = BasePosition.Y
```

For example, with:

```text
TextureSize = "17, 32"
BasePosition = "0, 0"
```

the frame positions are:

```text
Frame 0 → (0, 0)
Frame 1 → (17, 0)
Frame 2 → (34, 0)
Frame 3 → (51, 0)
```

Normal animations loop continuously.

---

### Normal Frame Duration

`FrameDurationMs` supports either one duration for all frames:

```json
{
    "Frames": [ 0, 1, 2, 3 ],
    "FrameDurationMs": 300
}
```

or one duration for each frame:

```json
{
    "Frames": [ 0, 1, 2, 3 ],
    "FrameDurationMs": [ 300, 300, 600, 300 ]
}
```

Values are specified in milliseconds.

When an array is used, the number of duration values must exactly match the number of entries in `Frames`.

If the values are invalid or the counts do not match, the animation is disabled and T's Core logs a warning.

---

## Tile Action

`TileAction` specifies the action executed when the player successfully interacts with the Big Craftable.

Example:

```json
{
    "TileAction": "Message \"Hello!\""
}
```

The action is executed at the Big Craftable's anchor tile position.

Because T's Core uses Stardew Valley's Tile Action system, both vanilla Tile Actions and Tile Actions registered by compatible mods can be used.

For example:

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

If `TileAction` is missing or empty, the interaction is not executed.

---

## Conditions

`Condition` can restrict when the interaction is available.

It uses Stardew Valley's Game State Query system.

Example:

```json
{
    "TileAction": "Message \"The condition was met!\"",
    "Condition": "PLAYER_HAS_MAIL MyMail"
}
```

If the condition isn't satisfied, the Tile Action isn't executed.

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

A BigCraftable Extension can require the player to hold a specific item.

Example:

```json
{
    "TileAction": "Message \"The Big Craftable accepted the Wood.\"",
    "RequiredItem": "(O)388"
}
```

`(O)388` is Wood.

Qualified and unqualified item IDs are supported.

The item is checked against the player's currently held object.

Having the item elsewhere in the inventory isn't sufficient.

---

### Required Item Count

Use `RequiredItemCount` to require more than one item.

```json
{
    "RequiredItem": "(O)388",
    "RequiredItemCount": 5
}
```

The player must be holding at least 5 Wood.

The default is:

```text
1
```

---

### Consuming the Required Item

By default, required items are consumed after the Tile Action succeeds.

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

Required items are only consumed after the Tile Action successfully executes.

---

### Invalid Item Message

`InvalidItemMessage` is displayed when the player isn't holding the required item.

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

> **Important:** `[ItemCount]` uses Stardew Valley's tokenizable string syntax. It should not be written as a Content Patcher token such as `{{ItemCount}}`.

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

Machine Effects can be used for effects supported by Stardew Valley's machine effect system, such as sounds and other visual effects.

T's Core attempts the configured effects in order and stops after the first effect which successfully plays.

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

The values are measured in tiles relative to the center of the Big Craftable's anchor tile.

Decimal and negative values are supported.

Examples:

| Value | Result |
|-------|--------|
| `"0, 0"` | Center of the anchor tile. |
| `"0, -0.5"` | Half a tile upward. |
| `"0.5, 0"` | Half a tile to the right. |
| `"-1, 1"` | One tile left and one tile down. |

Machine light positions use world coordinates, where one tile is 64 × 64 world pixels.

```text
0.5 tile = 32 world pixels
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

`ActionWobble` temporarily applies a wobble effect to the Big Craftable after a successful Tile Action.

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

It does not change the Big Craftable's processing state.

---

## Action Texture and Animation

A custom texture can be displayed temporarily after a successful Tile Action.

### Loading the Texture

First load the texture asset through Content Patcher.

For example:

```json
{
    "Action": "Load",
    "Target": "MyMod/MyBigCraftableTexture",
    "FromFile": "assets/MyBigCraftableTexture.png"
}
```

---

### Static Action Texture

To display a static Action texture:

```json
{
    "TextureSize": "17, 32",
    "ActionTexture": "MyMod/MyBigCraftableTexture",
    "ActionTexturePosition": "17, 0",
    "ActionDurationMs": 2000
}
```

`ActionTexturePosition` specifies the top-left pixel coordinate of frame 0 inside the Action texture.

The format is:

```text
"X, Y"
```

If `ActionFrames` isn't specified, Action frame `0` is displayed for `ActionDurationMs`.

After the duration expires, the next applicable texture state is restored.

---

### Animated Action Texture

To create an Action animation:

```json
{
    "TextureSize": "17, 32",
    "ActionTexture": "MyMod/MyBigCraftableTexture",
    "ActionTexturePosition": "0, 0",
    "ActionFrames": [ 0, 1, 2, 3, 2, 1 ],
    "ActionFrameDurationMs": [ 300, 300, 450, 1200, 450, 300 ]
}
```

Frames are arranged horizontally.

The source position for each frame uses the width defined by `TextureSize`:

```text
X = ActionTexturePosition.X + (Frame × TextureWidth)
Y = ActionTexturePosition.Y
```

For example, with:

```text
TextureSize = "17, 32"
ActionTexturePosition = "0, 0"
```

the frame positions are:

```text
Frame 0 → (0, 0)
Frame 1 → (17, 0)
Frame 2 → (34, 0)
Frame 3 → (51, 0)
```

Action animations play once from beginning to end.

When the animation finishes, the next applicable visual state is displayed.

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

If the counts don't match, the Action animation is disabled and T's Core logs a warning.

When `ActionFrames` is specified, `ActionDurationMs` is ignored.

The total Action duration is determined by the frame durations.

---

## Idle Effects

`IdleEffects` can play Machine Effects while the Big Craftable is idle.

A Big Craftable is considered idle for this feature when:

```text
readyForHarvest = false
and
minutesUntilReady <= 0
```

This means Idle Effects don't run while a machine-type Big Craftable is actively processing an item or has output ready to harvest.

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

gives the configured Idle Effects a 25% chance to play each time they're checked.

---

## Idle Wobble

`IdleWobble` applies Stardew Valley's machine-style wobble effect while the Big Craftable is idle.

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

It's disabled while a machine-type Big Craftable is processing or has output ready.

If Action Wobble is currently active, Action Wobble takes priority.

---

## Normal Light

`Light` adds a normal T's Core light to the Big Craftable.

Example:

```json
{
    "Light": true,
    "LightOffset": "0, -0.5",
    "LightRadius": 2.0,
    "LightColor": "White"
}
```

Unlike `ActionLight`, this light isn't limited to a specific duration.

It remains associated with the placed Big Craftable while the T's Core normal light is applicable.

If the Big Craftable is removed, the corresponding T's Core light is also removed.

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

places the light at the center of the anchor tile.

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

### Light Priority for Machines

When the Big Craftable also has Stardew Valley machine data, T's Core respects temporary Action Lights and Stardew Valley's vanilla `LightWhileWorking`.

The priority is:

```text
Action Light
    ↓
Vanilla LightWhileWorking
    ↓
T's Core Normal Light
```

When an Action Light is active, the normal T's Core light is temporarily suppressed.

If the Big Craftable has vanilla `LightWhileWorking` data and is currently processing, the vanilla working light is used instead of the normal T's Core light.

When those higher-priority lights are no longer applicable, the normal T's Core light is restored.

For a normal Big Craftable without machine data, the vanilla `LightWhileWorking` step doesn't apply.

---

## Texture Priority

BigCraftable Extension textures are designed to coexist with Stardew Valley's normal machine animation system.

The visual priority is:

```text
T's Core Action Texture / Animation
        ↓
Vanilla Working Texture / Animation
        ↓
T's Core Normal Texture / Animation
```

### Action

Action Texture / Animation has the highest T's Core texture priority.

It temporarily overrides the current visual state after a successful Tile Action.

When the Action state ends, the next applicable state is displayed.

---

### Working

Working textures and animations are handled by Stardew Valley.

T's Core doesn't replace or modify the vanilla `WorkingTexture` or Working animation system.

When a Big Craftable is operating as a machine and is currently processing, its vanilla Working state takes priority over the T's Core normal texture state.

---

### Normal

The T's Core normal Texture / Animation is the default BigCraftable Extension visual state when no higher-priority Action or Working state applies.

If the BigCraftable Extension `Texture` property is omitted, the Big Craftable's normal texture is used.

If `TexturePosition` is also omitted, the normal Stardew Valley source position is preserved.

This allows normal animation to be added without requiring a separate texture asset.

---

## Complete Example

The following example demonstrates the main BigCraftable Extension features together.

### Big Craftable Data

First, add or edit the Big Craftable and link it to the Extension:

```json
{
    "Action": "EditData",
    "Target": "Data/BigCraftables",
    "Entries": {
        "MyMod_MyBigCraftable": {
            "Name": "MyBigCraftable",
            "DisplayName": "My Big Craftable",
            "Description": "An example Big Craftable.",
            "Price": 500,
            "Fragility": 0,
            "CanBePlacedOutdoors": true,
            "CanBePlacedIndoors": true,
            "Texture": "MyMod/MyBigCraftableTexture",
            "SpriteIndex": 0,
            "CustomFields": {
                "TsCore/BigCraftableExtension": "MyMod/MyExtension"
            }
        }
    }
}
```

---

### Texture

Load the texture:

```json
{
    "Action": "Load",
    "Target": "MyMod/MyBigCraftableTexture",
    "FromFile": "assets/MyBigCraftableTexture.png"
}
```

---

### BigCraftable Extension

```json
{
    "Action": "EditData",
    "Target": "TsCore/BigCraftableExtension",
    "Entries": {
        "MyMod/MyExtension": {
            "CollisionSize": "1, 2",

            "TextureSize": "17, 32",
            "Frames": [ 0, 1, 2, 3 ],
            "FrameDurationMs": 300,

            "TileAction": "Message \"BigCraftable Extension Test\"",

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

            "ActionTexture": "MyMod/MyBigCraftableTexture",
            "ActionTexturePosition": "0, 0",
            "ActionFrames": [ 3, 2, 1, 0 ],
            "ActionFrameDurationMs": 300,
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

            "Light": true,
            "LightOffset": "0, -0.5",
            "LightRadius": 2.0,
            "LightColor": "White"
        }
    }
}
```

This example:

- creates a 1 × 2 collision footprint;
- uses 17 × 32 pixel texture frames;
- loops a normal four-frame animation;
- requires the `PLAYER_HAS_MAIL TestMail` condition;
- requires the player to hold at least 5 Wood;
- consumes 5 Wood after a successful interaction;
- executes the configured Tile Action;
- plays an Action Machine Effect;
- displays a temporary Action Light;
- applies Action Wobble;
- plays a custom Action animation;
- plays Idle Effects while idle;
- applies Idle Wobble;
- displays a normal T's Core light when applicable.

> **Note:** `ActionDurationMs` is included above to show the available property, but it has no effect while `ActionFrames` is specified. The Action animation duration is determined by `ActionFrameDurationMs`.

---

## Notes

BigCraftable Extension is designed for Content Patcher packs and doesn't require custom C# code.

The extension is linked to a Big Craftable through:

```text
Data/BigCraftables
    CustomFields
        TsCore/BigCraftableExtension
```

The Custom Field value must match an entry in:

```text
TsCore/BigCraftableExtension
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

If the Condition fails, Required Item checks and the Tile Action aren't performed.

If the Required Item check fails, the Tile Action isn't performed.

Required items are consumed only after a successful Tile Action.

Action and Idle Machine Effects use Stardew Valley's `MachineEffects` data structure.

Texture frame dimensions are controlled by:

```json
"TextureSize": "Width, Height"
```

For example:

```json
"TextureSize": "17, 32"
```

Normal and Action animation frames are arranged horizontally using the configured Texture Width.

Collision dimensions are controlled by:

```json
"CollisionSize": "Width, Height"
```

For example:

```json
"CollisionSize": "1, 2"
```

Collision Width extends to the right from the anchor tile.

Collision Height extends upward from the anchor tile.

Light offsets use **tile coordinates** and support decimal values:

```text
"0, -0.5"
```

Texture positions and texture sizes use **source texture pixel coordinates**:

```text
TexturePosition       = "0, 32"
ActionTexturePosition = "0, 64"
TextureSize           = "17, 32"
```

These values intentionally use different units:

| Property | Unit | Example |
|----------|------|---------|
| `CollisionSize` | Tiles | `"1, 2"` |
| `TextureSize` | Source pixels | `"17, 32"` |
| `TexturePosition` | Source pixels | `"0, 32"` |
| `ActionTexturePosition` | Source pixels | `"0, 64"` |
| `ActionLightOffset` | Tiles | `"0, -0.5"` |
| `LightOffset` | Tiles | `"0.5, -1"` |

T's Core doesn't modify Stardew Valley's vanilla Working texture or Working animation behavior.

Additional BigCraftable Extension features may be added in future versions of T's Core.

---

## Modder Guide

- ← [Building Services](ModderGuide_BuildingServices.md)
- ↑ [Guide Index](#top)
- → [Dialogue System](ModderGuide_DialogueSystem.md)

← [Back to README](../README.md)

← [Back to Modder Guide](ModderGuide.md)
