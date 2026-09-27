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
- ✅ **Dialogue System** *(Current Page)*
- 📄 [Migration System](ModderGuide_MigrationSystem.md)
- 📄 [Notification System](ModderGuide_NotificationSystem.md)
- 📄 [Content Patcher Integration](ModderGuide_ContentPatcherIntegration.md)
- 📄 [Other Features](ModderGuide_OtherFeatures.md)

← [Back to README](../README.md)

← [Back to Modder Guide](ModderGuide.md)

---

# Dialogue System

The Dialogue System allows Content Patcher packs to create custom data-driven dialogues using T's Core.

Dialogues are registered through the custom Data Asset:

```text
TsCore/Dialogues
```

They can be opened from map Tile Actions and Touch Actions using:

```text
TsCoreDialogue <DialogueId>
```

A Dialogue can display text, provide player responses, use single or multiple Game State Query conditions, execute Trigger Actions, continue to another Dialogue, limit usage per Building, run weighted random Actions, hide its messages when needed, automatically close after a specified duration, and play Audio Cues.

No C# code is required.

---

## Contents

- [Overview](#overview)
- [Content Pack Setup](#content-pack-setup)
- [Dialogue Action](#dialogue-action)
- [Dialogue Properties](#dialogue-properties)
- [Text](#text)
- [Conditions and FailText](#conditions-and-failtext)
- [Multiple Conditions](#multiple-conditions)
- [HideDialogue](#hidedialogue)
- [UsageLimit](#usagelimit)
- [Audio Cues](#audio-cues)
- [Responses](#responses)
- [Response Conditions](#response-conditions)
- [Response Actions](#response-actions)
- [Next Dialogue](#next-dialogue)
- [AfterActions](#afteractions)
- [RandomActions](#randomactions)
- [Duration](#duration)
- [Multi-page Dialogue](#multi-page-dialogue)
- [Complete Example](#complete-example)
- [Debugging](#debugging)
- [Reloading Content Patcher Content Packs](#reloading-content-patcher-content-packs)
- [Notes](#notes)

---

## Overview

T's Core Dialogues are stored in:

```text
TsCore/Dialogues
```

Each entry in the Data Asset represents one Dialogue.

For example:

```json
{
  "Action": "EditData",
  "Target": "TsCore/Dialogues",
  "Entries": {
    "MyMod/MyDialogue": {
      "Text": "Hello!"
    }
  }
}
```

The key in `Entries` becomes the Dialogue ID.

In this example:

```text
MyMod/MyDialogue
```

is the Dialogue ID.

It can then be displayed from a map using:

```text
TsCoreDialogue MyMod/MyDialogue
```

The basic flow is:

```text
TsCoreDialogue
    ↓
Dialogue ID
    ↓
TsCore/Dialogues
    ↓
Condition / Conditions
    ↓
UsageLimit
    ↓
Dialogue Text / Responses
    ↓
Response Condition / Conditions
    ↓
Response Actions
    ↓
Next Dialogue
    ↓
AfterActions / RandomActions
```

Depending on the configured properties, not every step is required.

---

## Content Pack Setup

Dialogues are registered through Content Patcher using:

```text
TsCore/Dialogues
```

This uses a normal Content Patcher Content Pack.

### manifest.json

```json
{
  "Name": "[CP] My Dialogue Pack",
  "Author": "YourName",
  "Version": "1.0.0",
  "UniqueID": "YourName.MyDialoguePack",
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

---

### content.json

Use `EditData` to add Dialogues:

```json
{
  "Format": "2.9.0",
  "Changes": [
    {
      "Action": "EditData",
      "Target": "TsCore/Dialogues",
      "Entries": {
        "YourName.MyMod_Greeting": {
          "Text": "Hello!"
        }
      }
    }
  ]
}
```

The key in `Entries` becomes the Dialogue ID.

For custom Dialogues, using an ID based on your mod's UniqueID is recommended when possible.

For example:

```text
YourName.MyMod_Greeting
```

A single `EditData` patch can register multiple Dialogues.

```json
{
  "Action": "EditData",
  "Target": "TsCore/Dialogues",
  "Entries": {
    "YourName.MyMod_Dialogue1": {
      "Text": "This is the first Dialogue."
    },
    "YourName.MyMod_Dialogue2": {
      "Text": "This is the second Dialogue."
    }
  }
}
```

No special folder structure or separate Dialogue JSON files are required by T's Core.

---

## Dialogue Action

T's Core provides the following custom action:

```text
TsCoreDialogue
```

Syntax:

```text
TsCoreDialogue <DialogueId>
```

Example:

```text
TsCoreDialogue YourName.MyMod_Greeting
```

The action can be used as either a Tile Action or Touch Action.

---

### Tile Action

For example, the following map patch opens a Dialogue when the player interacts with a tile:

```json
{
  "Action": "EditMap",
  "Target": "Maps/Farm",
  "MapTiles": [
    {
      "Position": {
        "X": 68,
        "Y": 9
      },
      "Layer": "Buildings",
      "SetProperties": {
        "Action": "TsCoreDialogue YourName.MyMod_Greeting"
      }
    }
  ]
}
```

---

### Touch Action

The same syntax can be used with a Touch Action:

```json
{
  "Action": "EditMap",
  "Target": "Maps/Farm",
  "MapTiles": [
    {
      "Position": {
        "X": 68,
        "Y": 9
      },
      "Layer": "Back",
      "SetProperties": {
        "TouchAction": "TsCoreDialogue YourName.MyMod_Greeting"
      }
    }
  ]
}
```

The Dialogue is opened when the Touch Action is activated.

---

## Dialogue Properties

A Dialogue entry supports the following properties.

| Property | Default | Description |
|----------|---------|-------------|
| `Text` | `""` | Text displayed by the Dialogue. |
| `HideDialogue` | `false` | Hides all messages generated by this Dialogue while allowing its logic and Actions to continue. |
| `Condition` | — | Single Game State Query which must be satisfied before the Dialogue is displayed. |
| `FailText` | — | Text displayed when `Condition` is not satisfied. |
| `Conditions` | — | Multiple Game State Query conditions with individual failure behavior. |
| `UsageLimit` | — | Optional usage limit. Currently supports one use per Building per day. |
| `AudioCue` | — | Audio Cue played when the Dialogue successfully passes its conditions. |
| `FailAudioCue` | — | Audio Cue played when the single `Condition` fails. |
| `Responses` | — | Player response choices. |
| `AfterActions` | — | Trigger Actions executed after the Dialogue chain is closed. |
| `RandomActions` | — | Weighted Action candidates. One candidate is selected and executed after a successful Dialogue. |
| `Duration` | `0` | Automatically closes a non-response Dialogue after the specified number of milliseconds. |

Each property is explained in more detail below.

---

## Text

`Text` specifies the text displayed by the Dialogue.

Example:

```json
{
  "Action": "EditData",
  "Target": "TsCore/Dialogues",
  "Entries": {
    "YourName.MyMod_Greeting": {
      "Text": "Welcome!"
    }
  }
}
```

When the Dialogue is opened, the player sees:

```text
Welcome!
```

If the Dialogue has no `Responses`, it behaves as a normal text Dialogue.

If `Responses` are defined, `Text` becomes the question or prompt shown above the response choices.

---

## Conditions and FailText

`Condition` can be used to control whether the main Dialogue is available.

It uses Stardew Valley's Game State Query system.

Example:

```json
{
  "Text": "The shop is currently open.",
  "Condition": "TIME 1000 2000",
  "FailText": "The shop is currently closed."
}
```

If the condition succeeds:

```text
The shop is currently open.
```

is displayed.

If the condition fails:

```text
The shop is currently closed.
```

is displayed instead.

When the Dialogue-level `Condition` fails:

- the main Dialogue is not displayed;
- `Responses` are not displayed;
- `AudioCue` is not played;
- `AfterActions` are not registered;
- `FailText` is displayed if specified;
- `FailAudioCue` is played if specified.

If `Condition` is omitted, the Dialogue is always available.

---

## Multiple Conditions

`Conditions` can be used when a Dialogue needs multiple requirements with different failure behavior.

Unlike a single `Condition`, each entry can define its own:

- `Condition`;
- `FailText`;
- `FailAudioCue`;
- `FailActions`;
- `FailNext`.

The conditions are checked from top to bottom.

The first condition which fails handles its failure behavior, and the remaining conditions are not checked.

Example:

```json
{
  "Text": "You may enter.",
  "Conditions": [
    {
      "Condition": "PLAYER_HAS_MAIL MyQuestComplete",
      "FailText": "You haven't completed the quest yet.",
      "FailAudioCue": "cancel"
    },
    {
      "Condition": "PLAYER_CURRENT_MONEY Current 2000",
      "FailText": "You need 2000G.",
      "FailAudioCue": "cancel"
    }
  ]
}
```

The processing order is:

```text
Condition 1
    ↓
┌─ Failed → its Fail behavior
│
└─ Passed
      ↓
   Condition 2
      ↓
   ┌─ Failed → its Fail behavior
   │
   └─ Passed
         ↓
      Main Dialogue
```

### FailActions

`FailActions` specifies Trigger Actions to execute when that condition fails.

```json
{
  "Condition": "PLAYER_HAS_MAIL MyMail",
  "FailText": "You cannot enter yet.",
  "FailActions": [
    "AddMoney 100"
  ]
}
```

If `FailText` is displayed, the `FailActions` run after that failure Dialogue is closed.

If no failure text is displayed, the failure Actions can be processed immediately.

As with other Trigger Action lists, Actions are executed in order.

### FailNext

`FailNext` specifies another Dialogue to open when that condition fails.

```json
{
  "Condition": "PLAYER_HAS_MAIL MyMail",
  "FailText": "You cannot enter yet.",
  "FailNext": "YourName.MyMod_NotAvailable"
}
```

If `FailText` is displayed, `FailNext` is opened after the failure Dialogue is closed.

`FailActions` are processed before `FailNext`.

### Conditions on Responses

`Conditions` can also be used inside a Response.

```json
{
  "Text": "Yes",
  "Conditions": [
    {
      "Condition": "PLAYER_CURRENT_MONEY Current 2000",
      "FailText": "You don't have enough money."
    },
    {
      "Condition": "PLAYER_HAS_MAIL MyPermit",
      "FailText": "You don't have the required permit."
    }
  ],
  "Actions": [
    "AddMoney -2000"
  ]
}
```

The Response succeeds only if all entries pass.

The first failed entry handles its own `FailText`, `FailAudioCue`, `FailActions`, and `FailNext`.

> **Note:** The original single `Condition`, `FailText`, and `FailAudioCue` properties remain supported. Use `Conditions` when different requirements need different failure behavior.

---

## HideDialogue

`HideDialogue` hides all messages generated by that Dialogue.

Default:

```json
"HideDialogue": false
```

Example:

```json
{
  "HideDialogue": true,
  "AudioCue": "coin",
  "AfterActions": [
    "AddMoney 100"
  ]
}
```

When `HideDialogue` is `true`:

- the Dialogue's normal text is not displayed;
- failure text generated by that Dialogue is not displayed;
- conditions are still checked;
- Audio Cues can still play;
- failure Actions and failure continuation behavior still work;
- successful `AfterActions` and `RandomActions` still run.

Because there is no Dialogue window to close, successful `AfterActions` and `RandomActions` are processed immediately.

If a hidden Dialogue defines `Responses`, the response choices cannot be displayed, so the Responses are skipped.

This is useful when the Dialogue System is being used for interaction logic or rewards without showing a message window.

---

## UsageLimit

`UsageLimit` can restrict how often a Dialogue succeeds.

Currently supported:

| Property | Supported value | Description |
|----------|-----------------|-------------|
| `Scope` | `Building` | Stores the usage state on the individual Building instance. |
| `Period` | `Day` | Allows the Dialogue to succeed once per in-game day. |
| `Key` | Custom string | Key used to store the usage state in the Building's `modData`. |

Example:

```json
{
  "UsageLimit": {
    "Scope": "Building",
    "Period": "Day",
    "Key": "YourName.MyMod/TreeReward"
  },
  "Text": "You found an item!",
  "AfterActions": [
    "AddItem (O)595 1 0"
  ]
}
```

With `Scope: Building` and `Period: Day`, each individual Building can use the Dialogue once per day.

For example, if three copies of the same Building exist:

```text
Building A → available
Building B → available
Building C → available
```

Using Building A does not mark Buildings B or C as used.

The same `Key` can therefore be shared by multiple instances when the usage state should be tracked independently for each Building.

The used state is stored in that Building's `modData`.

### Building source requirement

`Scope: Building` requires the Dialogue to have been started from an actual Building.

For example, it can be used with a Building action which opens a T's Core Dialogue.

If the same Dialogue is started without a Building source, such as from a normal map Tile Action, T's Core cannot determine which Building should own the usage state. In that case the Dialogue is aborted and a warning is logged.

### When the usage is recorded

The Dialogue is marked as used only after its successful processing completes.

If a success-side Trigger Action fails, the usage is not marked as completed.

This allows the player to try the interaction again instead of losing that day's use because an Action failed.

---

## Audio Cues

A Dialogue can play an Audio Cue when it is displayed.

Use:

```json
"AudioCue": "coin"
```

For example:

```json
{
  "Text": "Welcome!",
  "AudioCue": "coin"
}
```

The Audio Cue is played when the Dialogue successfully passes its `Condition` and is opened.

Different Dialogues can use different Audio Cues.

This also applies to Dialogues opened through `Next`.

For example:

```json
{
  "Text": "Warp complete!",
  "AudioCue": "coin",
  "Duration": 1500
}
```

plays the Audio Cue when the text appears and automatically closes the Dialogue after 1500 milliseconds.

---

### Fail Audio Cue

`FailAudioCue` can be used to play a different Audio Cue when the Dialogue-level `Condition` fails.

Example:

```json
{
  "Text": "You may enter.",
  "Condition": "PLAYER_HAS_MAIL MyMail",
  "FailText": "You cannot enter yet.",
  "AudioCue": "coin",
  "FailAudioCue": "cancel"
}
```

If the condition succeeds:

```text
AudioCue
    ↓
Main Dialogue
```

If the condition fails:

```text
FailAudioCue
    ↓
FailText
```

`AudioCue` is not played when the condition fails.

---

## Responses

`Responses` adds player choices to a Dialogue.

Example:

```json
{
  "Text": "Would you like to continue?",
  "Responses": [
    {
      "Text": "Yes"
    },
    {
      "Text": "No"
    }
  ]
}
```

The player is shown two response choices:

```text
Yes
No
```

Each Response can define its own behavior.

---

### Response Properties

Each entry in `Responses` supports the following properties.

| Property | Default | Description |
|----------|---------|-------------|
| `Text` | `""` | Text displayed for the response choice. |
| `Condition` | — | Single Game State Query checked when this response is selected. |
| `FailText` | — | Text displayed when the Response `Condition` fails. |
| `Conditions` | — | Multiple Game State Query conditions with individual failure behavior. |
| `FailAudioCue` | — | Audio Cue played when the Response `Condition` fails. |
| `Actions` | — | Trigger Actions executed when the response succeeds. |
| `Next` | — | Dialogue ID opened after the response succeeds. |

---

## Response Conditions

Each Response can have its own `Condition`.

Unlike the Dialogue-level condition, a Response condition is checked **when the player selects that response**.

For example:

```json
{
  "Text": "Use the special fishing area? It costs 2000G.",
  "Responses": [
    {
      "Text": "Yes",
      "Condition": "PLAYER_CURRENT_MONEY Current 2000",
      "FailText": "You don't have enough money."
    },
    {
      "Text": "No"
    }
  ]
}
```

If the player selects `Yes` and has at least 2000G, the Response succeeds.

If the player does not have enough money:

```text
You don't have enough money.
```

is displayed.

The Response's `Actions` and `Next` are not processed when its condition fails.

A Response can also use `Conditions` for multiple requirements with separate failure behavior. See [Multiple Conditions](#multiple-conditions).

---

### Response Fail Audio Cue

A Response can also specify `FailAudioCue`.

Example:

```json
{
  "Text": "Yes",
  "Condition": "PLAYER_CURRENT_MONEY Current 2000",
  "FailText": "You don't have enough money.",
  "FailAudioCue": "cancel"
}
```

When the player selects this response without enough money:

```text
FailAudioCue
    ↓
FailText
```

The Response Actions are not executed.

---

## Response Actions

`Actions` specifies Stardew Valley Trigger Actions to execute after a Response succeeds.

Example:

```json
{
  "Text": "Yes",
  "Actions": [
    "AddMoney -2000"
  ]
}
```

Multiple Trigger Actions can be specified:

```json
{
  "Text": "Yes",
  "Actions": [
    "AddMoney -2000",
    "TsCoreWarp BusStop 20 22"
  ]
}
```

The actions are executed in order.

T's Core Warp actions can also be used:

```text
TsCoreWarp
TsCoreMagicWarp
TsCoreMagicWarp_Simple
```

For example:

```json
{
  "Text": "Yes",
  "Actions": [
    "TsCoreMagicWarp Farm 64 15 Down"
  ]
}
```

T's Core Notification actions can also be used through Trigger Actions:

```text
TsCoreNotification
```

Vanilla Stardew Valley Trigger Actions can be used as well.

> **Important:** Response Actions are executed sequentially. If an earlier Action succeeds and a later Action fails, T's Core does not undo the Actions which already succeeded.

---

## Next Dialogue

`Next` can continue from one Dialogue to another Dialogue.

Example:

```json
{
  "Text": "Would you like to warp?",
  "Responses": [
    {
      "Text": "Yes",
      "Actions": [
        "TsCoreMagicWarp Farm 64 15 Down"
      ],
      "Next": "YourName.MyMod_Warped"
    },
    {
      "Text": "No",
      "Next": "YourName.MyMod_NotWarped"
    }
  ]
}
```

The target Dialogues can be registered in the same Data Asset:

```json
{
  "YourName.MyMod_Warped": {
    "Text": "Warping!",
    "Duration": 1500
  },
  "YourName.MyMod_NotWarped": {
    "Text": "Warp cancelled."
  }
}
```

This creates two branches:

```text
Yes
 ↓
Actions
 ↓
YourName.MyMod_Warped
```

and:

```text
No
 ↓
YourName.MyMod_NotWarped
```

`Next` is processed after the selected Response's `Actions` succeed.

If the Response condition fails, `Next` is not opened.

If an Action fails, processing stops and `Next` is not opened.

---

### Chaining Multiple Dialogues

A `Next` Dialogue can itself contain Responses with another `Next`.

This allows multiple Dialogues to be chained together.

For example:

```text
Dialogue A
    ↓
Response
    ↓
Dialogue B
    ↓
Response
    ↓
Dialogue C
```

Each Dialogue is retrieved from `TsCore/Dialogues` using its Dialogue ID.

---

## AfterActions

`AfterActions` specifies Trigger Actions which are executed after the Dialogue is closed.

Example:

```json
{
  "Text": "The Dialogue will perform an Action when closed.",
  "AfterActions": [
    "AddMoney 100"
  ]
}
```

Unlike Response `Actions`, these Actions are not executed immediately.

The order is:

```text
Dialogue displayed
    ↓
Player closes Dialogue
    ↓
AfterActions
```

This also works with multi-page Dialogues.

`AfterActions` are executed after the entire Dialogue has been closed, not after each individual text page.

---

### AfterActions and Responses

A Dialogue containing Responses can also use `AfterActions`.

Example:

```json
{
  "Text": "Choose an option.",
  "Responses": [
    {
      "Text": "Yes"
    },
    {
      "Text": "No"
    }
  ],
  "AfterActions": [
    "AddMoney 100"
  ]
}
```

The `AfterActions` run after the question Dialogue and its resulting Dialogue chain are finished.

---

### AfterActions and Next

`AfterActions` remain active when a Response continues to another Dialogue through `Next`.

For example:

```text
Dialogue A
AfterActions: Action A
    ↓
Next
    ↓
Dialogue B
AfterActions: Action B
    ↓
Dialogue closes
    ↓
Action A
    ↓
Action B
```

If multiple Dialogues in a chain define `AfterActions`, their Actions are accumulated and executed in registration order after the final Dialogue closes.

---

## RandomActions

`RandomActions` selects one Action candidate using weighted random selection.

Each candidate contains:

| Property | Default | Description |
|----------|---------|-------------|
| `Weight` | `1` | Relative chance for this candidate to be selected. Must be greater than `0`. |
| `Actions` | — | Trigger Actions executed in order when this candidate is selected. |

Example:

```json
{
  "Text": "You found a flower!",
  "RandomActions": [
    {
      "Weight": 7,
      "Actions": [
        "AddItem (O)595 1 0"
      ]
    },
    {
      "Weight": 2,
      "Actions": [
        "AddItem (O)595 2 0"
      ]
    },
    {
      "Weight": 1,
      "Actions": [
        "AddItem (O)595 3 0"
      ]
    }
  ]
}
```

The total weight is:

```text
7 + 2 + 1 = 10
```

so the selection chances are:

```text
Weight 7 → 70%
Weight 2 → 20%
Weight 1 → 10%
```

Only one candidate is selected.

If that candidate contains multiple `Actions`, they are executed from top to bottom.

### When RandomActions run

For a visible Dialogue, `RandomActions` use the same success timing as `AfterActions` and are processed after the Dialogue closes.

When both are defined, the success-side order is:

```text
Dialogue closes
    ↓
AfterActions
    ↓
Selected RandomActions
```

When `HideDialogue` is `true`, there is no Dialogue window to close, so these success Actions are processed immediately.

### Stable random selection

`RandomActions` use deterministic selection.

The selection includes the current save, in-game day, and Dialogue ID.

When the Dialogue has a Building source, the individual Building's persistent ID is also included.

This means:

```text
same day + same Building + same Dialogue
→ same selected candidate

same day + different Building
→ independently selected candidate

next in-game day
→ new daily selection
```

This is especially useful for Building rewards where multiple copies of the same Building should not all receive the same random result.

When there is no Building source, the random selection still works, but it is based on the Dialogue and current day rather than a Building instance.

### RandomActions and UsageLimit

`RandomActions` can be combined with `UsageLimit`.

```json
{
  "UsageLimit": {
    "Scope": "Building",
    "Period": "Day",
    "Key": "YourName.MyMod/TreeReward"
  },
  "Text": "You found a flower!",
  "RandomActions": [
    {
      "Weight": 8,
      "Actions": [
        "AddItem (O)595 1 0"
      ]
    },
    {
      "Weight": 2,
      "Actions": [
        "AddItem (O)595 2 0"
      ]
    }
  ]
}
```

This gives each Building instance its own daily reward selection and allows that reward to be received once per day.

If an Action in the selected candidate fails, processing stops and the usage limit is not marked as completed.

> **Important:** `Weight` values must be greater than `0`. Invalid weighted candidates are rejected and a warning is logged.

---

## Duration

`Duration` automatically closes a normal text Dialogue after a specified amount of time.

The value is specified in **milliseconds**.

Example:

```json
{
  "Text": "Warping!",
  "Duration": 1500
}
```

This displays the Dialogue for approximately 1.5 seconds and then closes it automatically.

If `Duration` is omitted or is `0` or less, the Dialogue uses the normal manual-close behavior.

---

### Duration and AfterActions

`Duration` can be combined with `AfterActions`.

Example:

```json
{
  "Text": "Warp complete!",
  "Duration": 1500,
  "AfterActions": [
    "AddMoney 100"
  ]
}
```

The order is:

```text
Dialogue displayed
    ↓
1500 milliseconds
    ↓
Dialogue automatically closes
    ↓
AfterActions
```

The automatic close uses the normal Dialogue closing behavior, so `AfterActions` are executed normally.

---

### Duration and Responses

`Duration` is intended for normal Dialogues without `Responses`.

If a Dialogue contains Responses, `Duration` is ignored.

This prevents a question Dialogue from automatically closing while the player is choosing a response.

---

## Multi-page Dialogue

A normal text Dialogue can contain multiple pages by separating the text with:

```text
#
```

Example:

```json
{
  "Text": "This is page 1.#This is page 2.#This is page 3."
}
```

The pages are displayed in order.

```text
Page 1
    ↓
Page 2
    ↓
Page 3
```

`AfterActions`, if specified, run after the entire Dialogue is closed.

> **Note:** This `#` page separator applies to normal text Dialogues. A question Dialogue with `Responses` uses its `Text` as the question prompt and does not use the same multi-page behavior.

---

## Complete Example

The following example demonstrates the main Dialogue System features together.

It creates a special fishing area which:

- is available only between 10:00 and 20:00;
- costs 2000G;
- checks the player's current money;
- displays failure messages when requirements are not met;
- plays Audio Cues;
- executes Trigger Actions;
- warps the player;
- continues to another Dialogue;
- automatically closes the arrival message.

### Dialogue Data

```json
{
  "Action": "EditData",
  "Target": "TsCore/Dialogues",
  "Entries": {
    "YourName.MyMod_FishingArea": {
      "Text": "Would you like to use the special fishing area? The fee is 2000G.",
      "Condition": "TIME 1000 2000",
      "FailText": "The special fishing area is currently closed. (Open 10:00–20:00)",
      "AudioCue": "coin",
      "FailAudioCue": "cancel",
      "Responses": [
        {
          "Text": "Yes",
          "Condition": "PLAYER_CURRENT_MONEY Current 2000",
          "FailText": "You don't have enough money.",
          "FailAudioCue": "cancel",
          "Actions": [
            "AddMoney -2000",
            "TsCoreWarp BusStop 20 22"
          ],
          "Next": "YourName.MyMod_FishingAreaArrival"
        },
        {
          "Text": "No",
          "Next": "YourName.MyMod_FishingAreaCancelled"
        }
      ]
    },

    "YourName.MyMod_FishingAreaArrival": {
      "Text": "Welcome to the special fishing area!",
      "AudioCue": "coin",
      "Duration": 1500
    },

    "YourName.MyMod_FishingAreaCancelled": {
      "Text": "You decided not to use the special fishing area."
    }
  }
}
```

---

### Map Tile Action

The Dialogue can then be opened from the map:

```json
{
  "Action": "EditMap",
  "Target": "Maps/Farm",
  "MapTiles": [
    {
      "Position": {
        "X": 68,
        "Y": 9
      },
      "Layer": "Buildings",
      "SetProperties": {
        "Action": "TsCoreDialogue YourName.MyMod_FishingArea"
      }
    }
  ]
}
```

The resulting flow is:

```text
Player interacts with tile
    ↓
TIME condition
    ↓
┌─ Failed
│     ↓
│  FailAudioCue
│     ↓
│  FailText
│
└─ Passed
      ↓
   AudioCue
      ↓
   Question
      ↓
   Yes / No
```

If the player selects `Yes`:

```text
Money condition
    ↓
┌─ Failed
│     ↓
│  Response FailAudioCue
│     ↓
│  Response FailText
│
└─ Passed
      ↓
   AddMoney -2000
      ↓
   TsCoreWarp
      ↓
   Next
      ↓
   Arrival Dialogue
      ↓
   Duration
      ↓
   Auto-close
```

If the player selects `No`:

```text
No
 ↓
Next
 ↓
Cancellation Dialogue
```

---

## Debugging

T's Core provides a command for inspecting and testing registered Dialogues.

### Listing Dialogues

Use:

```text
tscore_debug_dialogue
```

to display the currently registered Dialogue IDs from:

```text
TsCore/Dialogues
```

This can be used to verify that Dialogues added through Content Patcher have been registered correctly.

---

### Testing a Dialogue

Use:

```text
tscore_debug_dialogue <DialogueId>
```

For example:

```text
tscore_debug_dialogue YourName.MyMod_FishingArea
```

This directly opens the specified Dialogue.

The normal Dialogue behavior is used, including:

- Condition and Conditions;
- FailText and per-condition failure behavior;
- HideDialogue;
- UsageLimit;
- Audio Cues;
- Responses;
- Response Condition and Conditions;
- Response Actions;
- Next;
- AfterActions;
- RandomActions;
- Duration.

This is useful for testing a Dialogue without repeatedly activating its map Tile Action or Touch Action.

---

## Reloading Content Patcher Content Packs

Dialogues registered through `TsCore/Dialogues` can be updated while the game is running by reloading the Content Patcher Content Pack which edits the Data Asset.

Use:

```text
tscore_cp_reload <ContentPackId>
```

For example:

```text
tscore_cp_reload YourName.MyDialoguePack
```

This can be used during development to:

- add a Dialogue;
- remove a Dialogue;
- change Dialogue text;
- change Condition or Conditions and their failure behavior;
- change HideDialogue behavior;
- change UsageLimit settings;
- change Responses;
- change Trigger Actions;
- change Next Dialogue targets;
- change AfterActions;
- change RandomActions and weights;
- change Duration;
- change Audio Cues;

without restarting Stardew Valley.

After the Content Pack is reloaded, the updated `TsCore/Dialogues` data is used the next time the Dialogue is opened.

T's Core's Content Patcher reload system also supports reloading ConfigSchema, Config Tokens, GMCM settings, and DynamicTokens.

For more details about `tscore_cp_reload`, see the [Content Patcher Integration](ModderGuide_ContentPatcherIntegration.md) guide.

---

## Notes

Dialogues are registered through:

```text
TsCore/Dialogues
```

The dictionary key used in `Entries` is the Dialogue ID.

For custom Dialogues, globally unique IDs are recommended.

Using an ID based on your mod's UniqueID is recommended when possible:

```text
YourName.MyMod_DialogueName
```

Dialogues can be started through:

```text
TsCoreDialogue <DialogueId>
```

using either a Tile Action or Touch Action.

Dialogue-level `Condition` is checked before the main Dialogue is displayed.

`Conditions` can be used on both Dialogues and Responses when multiple requirements need separate failure behavior. They are checked from top to bottom and stop at the first failure.

Response-level `Condition` is checked only after that Response is selected.

The main processing order for a Response is:

```text
Response selected
    ↓
Response Condition
    ↓
Response Actions
    ↓
Next Dialogue
```

`Actions` run immediately after the Response condition succeeds.

`HideDialogue` suppresses messages generated by that Dialogue but does not disable its conditions, Audio Cues, or Actions.

`UsageLimit` currently supports `Scope: Building` with `Period: Day`, allowing each individual Building instance to succeed once per in-game day.

`AfterActions` are different: they run after the Dialogue chain has completely closed.

If multiple Dialogues connected through `Next` register `AfterActions`, those Actions are accumulated and executed in registration order after the final Dialogue closes.

`RandomActions` selects one weighted candidate and executes that candidate's Trigger Actions. Building-started Dialogues include the individual Building in the deterministic daily selection.

`Duration` is specified in milliseconds and applies to normal text Dialogues without Responses.

`AudioCue` is played when the main Dialogue successfully passes its Condition.

`FailAudioCue` can be specified separately for:

- a Dialogue-level Condition failure;
- a Response-level Condition failure.

Normal text Dialogues can use `#` to separate multiple pages.

During development, use:

```text
tscore_debug_dialogue
tscore_debug_dialogue <DialogueId>
tscore_cp_reload <ContentPackId>
```

to inspect, test, and reload Dialogues without repeatedly restarting the game.

Additional Dialogue System features may be added in future versions of T's Core.

---

## Modder Guide

- ← [BigCraftable Extension](ModderGuide_BigCraftableExtension.md)
- ↑ [Guide Index](#top)
- → [Migration System](ModderGuide_MigrationSystem.md)

← [Back to README](../README.md)

← [Back to Modder Guide](ModderGuide.md)
