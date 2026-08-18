# 📖 Modder Guide

This guide explains how to use the public features provided by **T's Core** in Content Patcher.

<a id="top"></a>

## Guide Index

- 📄 [Relationship Services](ModderGuide_RelationshipServices.md)
- 📄 [Location Services](ModderGuide_LocationServices.md)
- 📄 [Warp Services](ModderGuide_WarpServices.md)
- 📄 [Building Services](ModderGuide_BuildingServices.md)
- ✅ **Notification System** *(Current Page)*
- 📄 [Content Patcher Tokens](ModderGuide_ContentPatcherTokens.md) *(Coming Soon)*

← [Back to README](../README.md)

← [Back to Modder Guide](ModderGuide.md)

---

# Notification System

The Notification System allows Content Packs to display customizable on-screen notifications using T's Core.

Notifications can be displayed from Tile Actions, Touch Actions, and Trigger Actions using the `TsCoreNotification` action.

T's Core also supports custom Notification Themes through Content Packs. Themes can control colors, borders, text appearance, layout, screen position, and when a notification should be dismissed.

---

## Contents

- [Notification Action](#notification-action)
- [Syntax](#syntax)
- [Built-in Notification Types](#built-in-notification-types)
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
- [Reloading Notification Themes](#reloading-notification-themes)
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
| `TypeOrTheme` | ✅ | Built-in notification type or custom Notification Theme name. |
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

If `MyTheme` is registered by a T's Core Content Pack, that theme will be used to display the notification.

---

## Built-in Notification Types

T's Core includes the following built-in notification types:

| Type | Purpose |
|------|---------|
| `Info` | General information notification. |
| `Success` | Success or completion notification. |
| `Error` | Error notification. |
| `Warning` | Warning notification. |
| `Quest` | Quest-related notification. |
| `Achievement` | Achievement-style notification. |
| `Boss` | High-importance boss-style notification. |
| `RetroWindow` | Retro-style message window. |

Built-in types can be used directly with `TsCoreNotification`.

Example:

```text
TsCoreNotification Achievement Normal 240 Achievement unlocked!
```

The built-in themes are stored in T's Core's:

```text
assets/notification
```

folder.

These JSON files can also be useful as examples when creating custom Notification Themes.

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
        "TsCoreNotification FirstVisitToday Custom_MyLocation MyInfoTheme High 300 {{i18n:LocationInfoText}}"
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

T's Core Content Packs can register custom Notification Themes.

A single Content Pack can include multiple T's Core features, such as Notification Themes, Warp Providers, and Building Providers.

### manifest.json

```json
{
  "Name": "[TsC] My T's Core Content Pack",
  "Author": "YourName",
  "Version": "1.0.0",
  "UniqueID": "YourName.MyTsCorePack",
  "UpdateKeys": [ "Nexus:12345" ],
  "ContentPackFor": {
    "UniqueID": "Tikamin557.TsCore"
  }
}
```

Replace the example values with your own information before publishing your Content Pack.

---

### Folder Structure

Notification Theme JSON files are placed inside:

```text
assets/notification
```

Example:

```text
[TsC] My T's Core Content Pack
├── manifest.json
└── assets
    └── notification
        ├── MyInfoTheme.json
        └── MyWarningTheme.json
```

JSON filenames may be chosen freely.

The filename without `.json` becomes the short theme name.

For example:

```text
assets/notification/MyInfoTheme.json
```

can be used as:

```text
TsCoreNotification MyInfoTheme Normal 180 Hello!
```

> **Note:** The folder names `assets` and `notification` are fixed and must not be renamed.

---

## Custom Notification Themes

A Notification Theme controls the visual appearance and some behavior of a notification.

Example:

```json
{
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
```

This theme inherits unspecified properties from the built-in `Info` theme and overrides only the properties defined in the JSON file.

Using inheritance is recommended when only a few properties need to be changed.

---

## Theme Properties

Notification Themes support the following properties.

### General

| Property | Type | Description |
|----------|------|-------------|
| `Base` | string | Name of the theme to inherit from. |

---

### Background and Border

| Property | Type | Description |
|----------|------|-------------|
| `BackgroundColor` | Color | Background color of the notification. |
| `BorderColor` | Color | Border color. |
| `BorderStyle` | enum | Border rendering style. |
| `BorderThickness` | int | Border thickness. |

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

Custom themes can inherit from another Notification Theme using:

```json
"Base": "ThemeName"
```

For example:

```json
{
  "Base": "Info",
  "TextScale": 1.2,
  "OffsetY": -100
}
```

This creates a theme based on `Info` while changing only the text scale and vertical position.

Inheritance can also be used between custom themes.

For example:

```json
{
  "Base": "MyBaseTheme",
  "TextColor": {
    "R": 255,
    "G": 220,
    "B": 100,
    "A": 255
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

This can be used to verify that themes from a Content Pack were loaded successfully.

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

If the name matches a built-in notification type, that type is displayed.

Otherwise, T's Core attempts to use the value as a custom Notification Theme name.

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

## Reloading Notification Themes

During Content Pack development, Notification Themes can be reloaded without restarting the game.

Use:

```text
tscore_reload notification
```

T's Core rescans its Notification Theme folders and reloads the registered themes.

This makes it possible to:

- edit an existing theme;
- add a new theme;
- remove a theme;
- test theme inheritance;

without restarting Stardew Valley.

You can also use:

```text
tscore_reload
```

or:

```text
tscore_reload all
```

to reload all supported T's Core resources.

> **Note:** Reloading T's Core resources does not reload Content Patcher patches or other SMAPI mods.

---

## Notes

Notification Themes are designed so that Content Packs can create reusable notification styles without requiring C# code.

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

Use `FirstVisitToday` when a notification should appear only once per location per day, and use the theme dismissal settings when a notification should only remain relevant while the player is in a particular area.

During development, `tscore_debug_notification_themes`, `tscore_debug_notification`, `tscore_debug_notification_trigger`, and `tscore_reload notification` can be used to verify themes and notification behavior without repeatedly restarting the game.

---

## Modder Guide

- ← [Building Services](ModderGuide_BuildingServices.md)
- ↑ [Guide Index](#top)
- → [Content Patcher Tokens](ModderGuide_ContentPatcherTokens.md) *(Coming Soon)*

← [Back to README](../README.md)

← [Back to Modder Guide](ModderGuide.md)
