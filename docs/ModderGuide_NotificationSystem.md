# 📖 Modder Guide

This guide explains how to use the public features provided by **T's Core** in Content Patcher.

<a id="top"></a>

## Guide Index

- 📄 [Relationship Services](ModderGuide_RelationshipServices.md)
- 📄 [Location Services](ModderGuide_LocationServices.md)
- 📄 [Warp Services](ModderGuide_WarpServices.md)
- 📄 [Map Properties](ModderGuide_MapProperties.md)
- 📄 [Building Services](ModderGuide_BuildingServices.md)
- 📄 [Machine Interaction](ModderGuide_MachineInteraction.md)
- 📄 [Migration System](ModderGuide_MigrationSystem.md)
- ✅ **Notification System** *(Current Page)*
- 📄 [Content Patcher Integration](ModderGuide_ContentPatcherIntegration.md)

← [Back to README](../README.md)

← [Back to Modder Guide](ModderGuide.md)

---

# Notification System

The Notification System allows Content Patcher Content Packs to display customizable on-screen notifications using T's Core.

Notifications can be displayed from Tile Actions, Touch Actions, and Trigger Actions using the `TsCoreNotification` action.

T's Core also provides the custom Data Asset:

```text
TsCore/NotificationThemes
```

Content Patcher Content Packs can use `EditData` to register custom Notification Themes.

Themes can control colors, borders, text appearance, layout, screen position, and when a notification should be dismissed.

T's Core also provides several built-in Notification Themes which can be used directly or inherited by custom themes.

---

## Contents

- [Notification Action](#notification-action)
- [Syntax](#syntax)
- [Built-in Notification Themes](#built-in-notification-themes)
- [Priority](#priority)
- [Duration](#duration)
- [FirstVisitToday](#firstvisittoday)
- [Content Pack Setup](#content-pack-setup)
- [Custom Notification Themes](#custom-notification-themes)
- [Theme Properties](#theme-properties)
- [Theme Inheritance](#theme-inheritance)
- [Dismissal on Location Change](#dismissal-on-location-change)
- [Examples](#examples)
- [Debugging](#debugging)
- [Reloading Content Patcher Content Packs](#reloading-content-patcher-content-packs)
- [Notes](#notes)

---

## Notification Action

T's Core provides the following custom action for displaying notifications:

```text
TsCoreNotification
```

It can be used with:

- Tile Actions
- Touch Actions
- Trigger Actions

The same notification syntax is used for all supported action types.

---

## Syntax

The standard syntax is:

```text
TsCoreNotification <TypeOrTheme> <Priority> <Duration> <Message...>
```

Example:

```text
TsCoreNotification Info Normal 180 Welcome to the farm!
```

The arguments are:

| Argument | Required | Description |
|----------|----------|-------------|
| `TypeOrTheme` | ✅ | Built-in Notification Theme ID or custom Notification Theme ID. |
| `Priority` | ✅ | Notification display priority. |
| `Duration` | ✅ | Display duration in update ticks. |
| `Message` | ✅ | Text displayed by the notification. The remaining arguments are joined together as the message. |

For example:

```text
TsCoreNotification Warning High 300 Watch out!
```

uses the built-in `Warning` theme, gives the notification `High` priority, and displays it for 300 update ticks.

Custom themes use exactly the same syntax:

```text
TsCoreNotification MyTheme Normal 240 Custom notification text
```

If `MyTheme` is registered in `TsCore/NotificationThemes`, that theme will be used to display the notification.

---

## Built-in Notification Themes

T's Core includes the following built-in Notification Themes:

| Theme | Purpose |
|------|---------|
| `Info` | General information notification. |
| `Success` | Success or completion notification. |
| `Error` | Error notification. |
| `Warning` | Warning notification. |
| `Quest` | Quest-related notification. |
| `Achievement` | Achievement-style notification. |
| `Boss` | High-importance boss-style notification. |
| `Lavender` | Lavender-colored notification theme. |
| `Rose` | Rose-colored notification theme. |
| `RetroWindow` | Retro-style message window. |

Built-in themes can be used directly with `TsCoreNotification`.

Example:

```text
TsCoreNotification Achievement Normal 240 Achievement unlocked!
```

These themes are implemented internally by T's Core.

They are **not entries in `TsCore/NotificationThemes`** and cannot be replaced or modified through Content Patcher.

They can still be used as base themes for custom Notification Themes.

For example:

```json
{
  "Action": "EditData",
  "Target": "TsCore/NotificationThemes",
  "Entries": {
    "MyInfoTheme": {
      "Base": "Info",
      "TextScale": 1.2
    }
  }
}
```

The following IDs are reserved by T's Core:

```text
Info
Success
Error
Warning
Quest
Achievement
Boss
Lavender
Rose
RetroWindow
```

If a Content Patcher pack attempts to register one of these IDs in `TsCore/NotificationThemes`, the external definition is ignored and T's Core uses its built-in theme instead.

T's Core also logs a warning such as:

```text
Notification Theme ID 'Info' is reserved by T's Core and cannot be overridden.
```

---

## Priority

Notifications have a display priority.

Priority determines which notification should be displayed first when multiple notifications are waiting.

The available priorities are:

| Priority | Description |
|----------|-------------|
| `Low` | Low-priority notification. |
| `Normal` | Standard notification priority. |
| `High` | High-priority notification. |
| `Critical` | Highest notification priority. |

Example:

```text
TsCoreNotification Warning High 180 Warning message
```

If a notification with a higher priority is received while another notification is currently displayed, the higher-priority notification can take its place.

The interrupted notification is returned to the queue and can be displayed again afterward.

Notifications waiting in the queue are also selected according to priority.

---

## Duration

`Duration` controls how long the notification remains visible.

The value is specified in **update ticks**.

Example:

```text
TsCoreNotification Info Normal 300 This notification lasts longer.
```

The duration controls the normal display lifetime of the notification.

A notification may disappear earlier if its Notification Theme defines a location-based dismissal condition.

---

## FirstVisitToday

`TsCoreNotification` provides an optional `FirstVisitToday` mode.

This is useful for notifications that should only appear the first time the player enters a specific location each day.

### Syntax

```text
TsCoreNotification FirstVisitToday <LocationName> <TypeOrTheme> <Priority> <Duration> <Message...>
```

Example:

```text
TsCoreNotification FirstVisitToday Custom_MyLocation Info Normal 300 Welcome!
```

The notification is displayed only when:

1. the player's current location matches `<LocationName>`; and
2. this is the first visit to that location for the current day.

Later visits to the same location on the same day will not display the notification again.

The first-visit state resets on a new day.

---

### Trigger Action Example

`FirstVisitToday` is particularly useful with Stardew Valley's `LocationChanged` Trigger Action.

```json
{
  "Action": "EditData",
  "Target": "Data/TriggerActions",
  "Entries": {
    "MyMod_LocationInfo": {
      "Id": "MyMod_LocationInfo",
      "Trigger": "LocationChanged",
      "Condition": "LOCATION_NAME Here Custom_MyLocation",
      "Actions": [
        "TsCoreNotification FirstVisitToday Custom_MyLocation Info High 300 {{i18n:LocationInfoText}}"
      ],
      "MarkActionApplied": false
    }
  }
}
```

`MarkActionApplied` is set to `false` so Stardew Valley can raise the Trigger Action again on later location changes.

T's Core handles the once-per-day behavior through `FirstVisitToday`.

This means the Trigger Action itself can remain repeatable while the notification is only displayed on the first visit of the day.

---

## Content Pack Setup

Custom Notification Themes are registered through Content Patcher using the following Data Asset:

```text
TsCore/NotificationThemes
```

This uses a normal Content Patcher Content Pack.

### manifest.json

```json
{
  "Name": "[CP] My Notification Pack",
  "Author": "YourName",
  "Version": "1.0.0",
  "UniqueID": "YourName.MyNotificationPack",
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

Use `EditData` to add Notification Themes:

```json
{
  "Format": "2.9.0",
  "Changes": [
    {
      "Action": "EditData",
      "Target": "TsCore/NotificationThemes",
      "Entries": {
        "MyInfoTheme": {
          "Base": "Info",
          "BackgroundColor": {
            "R": 30,
            "G": 60,
            "B": 120,
            "A": 220
          },
          "TextScale": 1.0
        }
      }
    }
  ]
}
```

The key in `Entries` becomes the Notification Theme ID.

In this example:

```text
MyInfoTheme
```

can be used as:

```text
TsCoreNotification MyInfoTheme Normal 180 Hello!
```

A single `EditData` patch can register multiple themes:

```json
{
  "Action": "EditData",
  "Target": "TsCore/NotificationThemes",
  "Entries": {
    "MyInfoTheme": {
      "Base": "Info",
      "TextScale": 1.1
    },
    "MyWarningTheme": {
      "Base": "Warning",
      "TextScale": 1.2
    }
  }
}
```

The Notification Theme ID is determined by the `Entries` key.

No special folder structure or separate Notification Theme JSON files are required by T's Core.

> **Important:** Notification Theme IDs should be globally unique. Do not use one of T's Core's reserved built-in theme IDs.

For custom themes, using an ID based on your mod's UniqueID is recommended when possible.

For example:

```text
YourName.MyMod_MyTheme
```

---

## Custom Notification Themes

A Notification Theme controls the visual appearance and some behavior of a notification.

For example:

```json
{
  "Action": "EditData",
  "Target": "TsCore/NotificationThemes",
  "Entries": {
    "MyInfoTheme": {
      "Base": "Info",
      "BackgroundColor": {
        "R": 30,
        "G": 60,
        "B": 120,
        "A": 220
      },
      "TextColor": {
        "R": 255,
        "G": 255,
        "B": 255,
        "A": 255
      },
      "TextScale": 1.0,
      "Anchor": "Bottom",
      "OffsetY": -60
    }
  }
}
```

This theme inherits unspecified properties from the built-in `Info` theme and overrides only the properties defined in the entry.

Using inheritance is recommended when only a few properties need to be changed.

A theme can also define all of its own properties without using inheritance:

```json
"Base": null
```

---

## Theme Properties

Notification Themes support the following properties.

### General

| Property | Type | Description |
|----------|------|-------------|
| `Base` | string | ID of the Notification Theme to inherit from. |

---

### Background and Border

| Property | Type | Description |
|----------|------|-------------|
| `BackgroundColor` | Color | Background color of the notification. |
| `BorderColor` | Color | Border color. |
| `BorderStyle` | enum | Border rendering style. |
| `BorderThickness` | int | Border thickness. |

Colors can be specified using RGBA values:

```json
"BackgroundColor": {
  "R": 30,
  "G": 60,
  "B": 120,
  "A": 220
}
```

---

### Text

| Property | Type | Description |
|----------|------|-------------|
| `TextColor` | Color | Text color. |
| `ShadowColor` | Color | Text shadow color. |
| `DrawShadow` | bool | Whether a text shadow is drawn. |
| `ShadowOffset` | Vector2 | Offset of the text shadow. |
| `TextScale` | float | Text size multiplier. |
| `TextAnchor` | enum | Alignment of the text inside the notification window. |

Example `ShadowOffset`:

```json
"ShadowOffset": "2, 2"
```

---

### Layout

| Property | Type | Description |
|----------|------|-------------|
| `MinHeight` | int | Minimum notification height. |
| `MinWidth` | int | Minimum notification width. |
| `PaddingX` | int | Horizontal text padding. |
| `PaddingY` | int | Vertical text padding. |
| `BorderPadding` | int | Padding used around the border. |
| `Anchor` | enum | Screen position used as the notification anchor. |
| `OffsetX` | int | Horizontal offset from the selected anchor. |
| `OffsetY` | int | Vertical offset from the selected anchor. |

---

### Dismissal

| Property | Type | Description |
|----------|------|-------------|
| `DismissOnLocationChange` | bool | Dismiss the notification whenever the player changes location. |
| `DismissOnEnterLocations` | string[] | Dismiss the notification when the player enters one of the specified locations. |

All theme properties are optional.

If a property is omitted and the theme has a `Base`, the value is inherited from the base theme.

---

## Theme Inheritance

Notification Themes can inherit from another Notification Theme using the `Base` property.

For example:

```json
{
  "Action": "EditData",
  "Target": "TsCore/NotificationThemes",
  "Entries": {
    "MyTheme": {
      "Base": "Info",
      "TextScale": 1.2,
      "OffsetY": -100
    }
  }
}
```

This creates `MyTheme` based on the built-in `Info` theme while changing only the text scale and vertical position.

Built-in themes can be used as base themes even though they are not stored as entries in `TsCore/NotificationThemes`.

Inheritance can also be used between custom themes.

For example:

```json
{
  "Action": "EditData",
  "Target": "TsCore/NotificationThemes",
  "Entries": {
    "MyBaseTheme": {
      "Base": "Info",
      "TextScale": 1.1
    },
    "MyChildTheme": {
      "Base": "MyBaseTheme",
      "TextColor": {
        "R": 255,
        "G": 220,
        "B": 100,
        "A": 255
      }
    }
  }
}
```

Any property that is not explicitly defined by the child theme is inherited from its parent.

This includes:

- background and border settings;
- text appearance;
- layout settings;
- screen positioning;
- location-based dismissal settings.

Properties explicitly defined by the child theme take priority over values inherited from its base theme.

Built-in themes are merged internally by T's Core before inheritance is resolved.

This allows custom themes to safely inherit from built-in themes such as:

```text
Info
Warning
Rose
RetroWindow
```

while preventing those built-in themes from being overridden through Content Patcher.

> **Note:** Circular theme inheritance is not supported. T's Core detects circular inheritance and logs a warning.

---

## Dismissal on Location Change

Notification Themes can control whether their notifications should disappear when the player changes location.

There are two available settings.

---

### DismissOnLocationChange

Set:

```json
"DismissOnLocationChange": true
```

to dismiss the notification whenever the player changes location.

Example:

```json
{
  "Base": "Info",
  "DismissOnLocationChange": true
}
```

A notification using this theme will disappear immediately when the local player warps to another location.

This is useful for location-specific instructions or information that should no longer remain visible after leaving the current area.

---

### DismissOnEnterLocations

`DismissOnEnterLocations` can be used when the notification should remain visible across normal location changes but disappear when the player enters a specific location.

Example:

```json
{
  "Base": "Info",
  "DismissOnEnterLocations": [
    "Farm",
    "FarmHouse"
  ]
}
```

A notification using this theme will be dismissed when the player enters either `Farm` or `FarmHouse`.

It will not be dismissed when entering other locations unless another dismissal rule applies.

Multiple location names can be specified.

---

### Combining the Settings

Both settings can be defined in the same theme:

```json
{
  "Base": "Info",
  "DismissOnLocationChange": false,
  "DismissOnEnterLocations": [
    "FarmHouse"
  ]
}
```

In this example, the notification does **not** disappear on every location change.

It disappears only when the player enters `FarmHouse`.

If `DismissOnLocationChange` is `true`, any location change dismisses the notification regardless of the `DismissOnEnterLocations` list.

---

### Queued Notifications

Location-based dismissal rules also apply to notifications waiting in the notification queue.

When the player changes location, T's Core checks:

- the notification currently being displayed; and
- notifications waiting in the queue.

Any notification whose dismissal condition matches the new location is removed.

If the currently displayed notification is dismissed, T's Core immediately attempts to display the next eligible notification from the queue.

This allows location-specific notifications to be removed cleanly without preventing unrelated queued notifications from appearing.

---

## Examples

### Basic Information Notification

```text
TsCoreNotification Info Normal 180 Welcome!
```

---

### Warning Notification

```text
TsCoreNotification Warning High 300 Watch out!
```

---

### Custom Theme

If `MyInfoTheme` has been registered in `TsCore/NotificationThemes`:

```text
TsCoreNotification MyInfoTheme Normal 240 This uses my custom theme.
```

---

### First Visit Today

```text
TsCoreNotification FirstVisitToday Custom_MyLocation MyInfoTheme High 300 Welcome to this location!
```

---

### Tile Action

The following example displays a notification when the player interacts with a tile.

```json
{
  "Action": "EditMap",
  "Target": "Maps/Town",
  "MapTiles": [
    {
      "Position": {
        "X": 26,
        "Y": 28
      },
      "Layer": "Buildings",
      "SetProperties": {
        "Action": "TsCoreNotification Info Normal 180 Hello from Pelican Town!"
      }
    }
  ]
}
```

---

### Trigger Action

The following example displays a notification whenever the player enters a specific custom location.

```json
{
  "Action": "EditData",
  "Target": "Data/TriggerActions",
  "Entries": {
    "MyMod_LocationMessage": {
      "Id": "MyMod_LocationMessage",
      "Trigger": "LocationChanged",
      "Condition": "LOCATION_NAME Here Custom_MyLocation",
      "Actions": [
        "TsCoreNotification MyInfoTheme Normal 300 {{i18n:LocationMessage}}"
      ],
      "MarkActionApplied": false
    }
  }
}
```

---

### First Visit Trigger Action

The following version only displays the notification on the first visit each day:

```json
{
  "Action": "EditData",
  "Target": "Data/TriggerActions",
  "Entries": {
    "MyMod_FirstVisitMessage": {
      "Id": "MyMod_FirstVisitMessage",
      "Trigger": "LocationChanged",
      "Condition": "LOCATION_NAME Here Custom_MyLocation",
      "Actions": [
        "TsCoreNotification FirstVisitToday Custom_MyLocation MyInfoTheme Normal 300 {{i18n:LocationMessage}}"
      ],
      "MarkActionApplied": false
    }
  }
}
```

---

## Debugging

T's Core provides several commands for testing and inspecting the Notification System during development.

---

### Inspecting Notification Themes

Use:

```text
tscore_debug_notification_themes
```

to display the currently registered Notification Themes.

The output separates themes into:

```text
TsCore Built-in Themes
External Data Asset Themes
```

These represent:

- **TsCore Built-in Themes** — fixed themes provided internally by T's Core.
- **External Data Asset Themes** — custom themes registered through `TsCore/NotificationThemes`.

This can be used to verify that built-in themes and themes added through Content Patcher are available correctly.

If an external Content Patcher pack attempts to register one of T's Core's reserved built-in theme IDs, T's Core ignores the external definition and logs a warning.

For example:

```text
[T's Core] Notification Theme ID 'Info' is reserved by T's Core and cannot be overridden.
```

---

### Testing a Notification

Use:

```text
tscore_debug_notification <TypeOrTheme>
```

Example:

```text
tscore_debug_notification Info
```

or:

```text
tscore_debug_notification MyInfoTheme
```

If the name matches a built-in Notification Theme, that theme is displayed.

Otherwise, T's Core attempts to use the value as a custom Notification Theme ID.

---

### Testing the Notification Action

The following command tests notification display through the same notification action processing used by Trigger Actions:

```text
tscore_debug_notification_trigger <TypeOrTheme> <Priority> <Duration> <Message...>
```

Example:

```text
tscore_debug_notification_trigger Warning High 500 Watch Out!
```

Custom themes can also be tested:

```text
tscore_debug_notification_trigger MyInfoTheme Normal 300 Test Message
```

This is useful for checking the arguments that will later be used with `TsCoreNotification`.

---

## Reloading Content Patcher Content Packs

Notification Themes registered through `TsCore/NotificationThemes` can be updated while the game is running by reloading the Content Patcher Content Pack that edits the Data Asset.

Use:

```text
tscore_cp_reload <ContentPackId>
```

For example:

```text
tscore_cp_reload YourName.MyNotificationPack
```

This can be used during development to:

- add a Notification Theme;
- remove a Notification Theme;
- change theme properties;
- change theme inheritance;

without restarting Stardew Valley.

Changes to a custom base theme are also reflected in custom themes which inherit from it when the Content Pack is reloaded.

Custom themes which inherit from a built-in T's Core theme continue to inherit from the fixed built-in theme after the reload.

T's Core's Content Patcher reload system also supports reloading ConfigSchema, Config Tokens, GMCM settings, and DynamicTokens.

For more details about `tscore_cp_reload`, see the [Content Patcher Integration](ModderGuide_ContentPatcherIntegration.md) guide.

---

## Notes

Custom Notification Themes are registered through:

```text
TsCore/NotificationThemes
```

The dictionary key used in `Entries` is the Notification Theme ID.

The Data Asset is intended for **external custom Notification Themes**.

T's Core's built-in themes are implemented internally and are not stored as entries in `TsCore/NotificationThemes`.

For most custom themes, inheriting from an existing built-in theme and overriding only the required properties is recommended.

For example:

```json
{
  "Base": "Info",
  "TextScale": 1.2,
  "DismissOnLocationChange": true
}
```

is generally easier to maintain than redefining every visual property.

The built-in theme IDs:

```text
Info
Success
Error
Warning
Quest
Achievement
Boss
Lavender
Rose
RetroWindow
```

are reserved by T's Core and cannot be overridden through `TsCore/NotificationThemes`.

Custom themes can still use any of these built-in themes as their `Base`.

Use `FirstVisitToday` when a notification should appear only once per location per day, and use the theme dismissal settings when a notification should only remain relevant while the player is in a particular area.

During development, use:

```text
tscore_debug_notification_themes
tscore_debug_notification <TypeOrTheme>
tscore_debug_notification_trigger <TypeOrTheme> <Priority> <Duration> <Message...>
tscore_cp_reload <ContentPackId>
```

to inspect, test, and reload custom Notification Themes without repeatedly restarting the game.

---

## Modder Guide

- ← [Migration System](ModderGuide_MigrationSystem.md)
- ↑ [Guide Index](#top)
- → [Content Patcher Integration](ModderGuide_ContentPatcherIntegration.md)

← [Back to README](../README.md)

← [Back to Modder Guide](ModderGuide.md)
