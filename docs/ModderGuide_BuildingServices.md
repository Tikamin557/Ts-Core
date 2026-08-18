# 📖 Modder Guide

This guide explains how to use the public features provided by **T's Core** in Content Patcher.

<a id="top"></a>

## Guide Index

- 📄 [Relationship Services](ModderGuide_RelationshipServices.md)
- 📄 [Location Services](ModderGuide_LocationServices.md)
- 📄 [Warp Services](ModderGuide_WarpServices.md)
- ✅ **Building Services** *(Current Page)*
- 📄 [Notification System](ModderGuide_NotificationSystem.md)
- 📄 [Content Patcher Tokens](ModderGuide_ContentPatcherTokens.md) *(Coming Soon)*

← [Back to README](../README.md)

← [Back to Modder Guide](ModderGuide.md)

---

# Building Services

Building Services provide additional functionality for buildings defined in `Data/Buildings`.

T's Core allows Content Packs to register Building Providers that add features such as nighttime light sources, conditional Draw Layers, feature enable/disable controls, and construction location restrictions without requiring any C# code.

This guide explains how to create Building Providers and configure the available building-related features.

---

## Contents

- [Content Pack Setup](#content-pack-setup)
- [Building Provider](#building-provider)
- [Enabling and Disabling Features](#enabling-and-disabling-features)
- [Building Lights](#building-lights)
- [Building Draw Layers](#building-draw-layers)
- [Valley Farm Only](#valley-farm-only)
- [Example](#example)
- [Debugging](#debugging)
- [Notes](#notes)

---

## Content Pack Setup

T's Core Content Packs can register custom Building Providers.

A single Content Pack can include multiple T's Core features, such as Building Providers, Warp Providers, and Notification Themes.

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

### Folder Structure

```text
[TsC] My T's Core Content Pack
├── manifest.json
└── assets
    └── buildings
        └── MyBuilding.json
```

Building Provider JSON files must be placed inside:

```text
assets/buildings
```

Subfolders are supported, and JSON filenames may be chosen freely.

> **Note:** The folder names (`assets` and `buildings`) are fixed and must not be renamed. T's Core searches this folder when loading Building Providers.

---

## Building Provider

Each JSON file inside `assets/buildings` defines one Building Provider.

A Building Provider targets a building type from `Data/Buildings` and can add T's Core-specific functionality to it.

Basic example:

```json
{
  "Id": "MyBuildingProvider",
  "BuildingType": "YourModId_MyBuilding"
}
```

| Property | Required | Description |
|----------|----------|-------------|
| `Id` | ✅ | Unique ID for the Building Provider. |
| `BuildingType` | ✅ | Building type ID from `Data/Buildings` targeted by this provider. |
| `BuildingsEnabledField` | Optional | `CustomFields` key used to enable or disable the entire Building Provider. |
| `LightsEnabledField` | Optional | `CustomFields` key used to enable or disable Building Lights. |
| `DrawLayersEnabledField` | Optional | `CustomFields` key used to enable or disable Building Draw Layers. |
| `ValleyFarmOnly` | Optional | If `true`, the building is shown in the construction menu only for the main Valley farm. Defaults to `false`. |
| `Lights` | Optional | Building Lights added by this provider. |
| `DrawLayers` | Optional | Conditional Building Draw Layers added by this provider. |

Provider IDs should be unique.

If another Building Provider with the same ID has already been registered, the duplicate provider will not be registered.

Multiple Building Providers may target the same `BuildingType`.

This can be useful when different configurations of the same building need different T's Core features.

---

## Enabling and Disabling Features

Building Providers can use fields in `Data/Buildings` → `CustomFields` to enable or disable their features.

Three optional properties are available:

```json
{
  "BuildingsEnabledField": "MyBuilding/BuildingsEnabled",
  "LightsEnabledField": "MyBuilding/LightsEnabled",
  "DrawLayersEnabledField": "MyBuilding/DrawLayersEnabled"
}
```

The values specified here are the names of keys which T's Core will read from the target building's `CustomFields`.

For example:

```json
"CustomFields": {
  "MyBuilding/BuildingsEnabled": "true",
  "MyBuilding/LightsEnabled": "false",
  "MyBuilding/DrawLayersEnabled": "true"
}
```

### BuildingsEnabledField

`BuildingsEnabledField` controls the entire Building Provider.

If its value is `false`, all features defined by that provider are disabled, including Building Lights and Building Draw Layers.

```json
"BuildingsEnabledField": "MyBuilding/BuildingsEnabled"
```

### LightsEnabledField

`LightsEnabledField` controls only the Building Lights defined by the provider.

```json
"LightsEnabledField": "MyBuilding/LightsEnabled"
```

### DrawLayersEnabledField

`DrawLayersEnabledField` controls only the Building Draw Layers defined by the provider.

```json
"DrawLayersEnabledField": "MyBuilding/DrawLayersEnabled"
```

If an Enabled Field is not specified, the corresponding feature is enabled by default.

If the specified `CustomFields` key does not exist, the feature is also treated as enabled.

> **Tip:** These fields can be edited through Content Patcher, which makes it possible to enable or disable T's Core building features dynamically based on config options or other conditions.

### Multiple Providers for the Same Building

Different Building Providers targeting the same `BuildingType` can use different `BuildingsEnabledField` values.

For example:

```json
{
  "Id": "MyBuilding_VariantA",
  "BuildingType": "YourModId_MyBuilding",
  "BuildingsEnabledField": "MyBuilding/VariantAEnabled"
}
```

and:

```json
{
  "Id": "MyBuilding_VariantB",
  "BuildingType": "YourModId_MyBuilding",
  "BuildingsEnabledField": "MyBuilding/VariantBEnabled"
}
```

Content Patcher can then switch between them:

```json
"CustomFields": {
  "MyBuilding/VariantAEnabled": "true",
  "MyBuilding/VariantBEnabled": "false"
}
```

This allows multiple T's Core configurations to be prepared for a single `Data/Buildings` entry and switched dynamically.

---

## Building Lights

Building Lights add light sources to buildings.

Lights are positioned relative to the building's top-left tile and automatically follow the building when it is moved.

They are also removed immediately when the building is demolished.

Example:

```json
{
  "Id": "MyBuildingProvider",
  "BuildingType": "YourModId_MyBuilding",

  "Lights": [
    {
      "Id": "LeftLamp",
      "OffsetX": 0,
      "OffsetY": -2,
      "Radius": 2,
      "Color": "0,0,0"
    }
  ]
}
```

| Property | Required | Description |
|----------|----------|-------------|
| `Id` | ✅ | Unique Light ID within the provider. |
| `OffsetX` | ✅ | Horizontal tile offset from the building's top-left tile. Negative values are supported. |
| `OffsetY` | ✅ | Vertical tile offset from the building's top-left tile. Negative values are supported. |
| `Radius` | Optional | Radius of the light source. Defaults to `4`. |
| `Color` | Optional | Light color in `"R,G,B"` format. |

The light position is calculated relative to the current position of each matching building.

If multiple buildings of the same `BuildingType` exist, each building receives its own Light instances.

Building Lights are active only when the game considers the location dark.

### Controlling Lights with CustomFields

To allow Content Patcher to enable or disable the Lights independently:

```json
{
  "Id": "MyBuildingProvider",
  "BuildingType": "YourModId_MyBuilding",

  "LightsEnabledField": "MyBuilding/LightsEnabled",

  "Lights": [
    {
      "Id": "LeftLamp",
      "OffsetX": 0,
      "OffsetY": -2,
      "Radius": 2,
      "Color": "0,0,0"
    }
  ]
}
```

Then edit the target building's `CustomFields`:

```json
"CustomFields": {
  "MyBuilding/LightsEnabled": "false"
}
```

When the value changes to `false`, existing T's Core Lights for that provider are removed.

---

## Building Draw Layers

Building Draw Layers add extra graphical layers to a building.

They are drawn in addition to any Draw Layers already defined by the building's original `Data/Buildings` entry.

T's Core does not replace the building's existing `DrawLayers`.

Example:

```json
{
  "Id": "MyBuildingProvider",
  "BuildingType": "YourModId_MyBuilding",

  "DrawLayers": [
    {
      "Id": "Animal",
      "SourceRect": {
        "X": 0,
        "Y": 144,
        "Width": 16,
        "Height": 16
      },
      "DrawPosition": "48, 8",
      "FrameDuration": 1500,
      "FrameCount": 6,
      "Condition": "TIME 600 1530, WEATHER Here Sun Wind"
    }
  ]
}
```

### Draw Layer Properties

| Property | Required | Default | Description |
|----------|----------|---------|-------------|
| `Id` | ✅ | — | Unique Draw Layer ID within the provider. |
| `Texture` | Optional | Building texture | Texture asset used for the layer. |
| `SourceRect` | ✅ | — | Pixel area of the texture to draw. |
| `DrawPosition` | ✅ | — | Draw position relative to the building. |
| `DrawInBackground` | Optional | `false` | Draw the layer behind the building instead of in front of it. |
| `SortTileOffset` | Optional | `0` | Y tile offset used when calculating the render order. |
| `OnlyDrawIfChestHasContents` | Optional | — | Draw only if the specified Building Chest contains an item. |
| `FrameDuration` | Optional | `90` | Duration of each animation frame in milliseconds. |
| `FrameCount` | Optional | `1` | Number of animation frames. |
| `FramesPerRow` | Optional | `-1` | Number of animation frames per spritesheet row. |
| `AnimalDoorOffset` | Optional | `0, 0` | Pixel offset applied based on the building's animal door open state. |
| `Condition` | Optional | — | Game State Query condition which must match for the layer to be drawn. |

### Texture

If `Texture` is omitted, T's Core uses the target building's original texture.

```json
{
  "SourceRect": {
    "X": 0,
    "Y": 144,
    "Width": 16,
    "Height": 16
  }
}
```

A custom texture can also be specified:

```json
{
  "Texture": "Mods/YourModId/MyBuildingTexture",
  "SourceRect": {
    "X": 0,
    "Y": 0,
    "Width": 16,
    "Height": 16
  }
}
```

### Season Offset

T's Core applies the target building's `SeasonOffset` from `Data/Buildings` to Draw Layer source rectangles.

This means Draw Layers using the building's original texture automatically follow the same seasonal texture offsets as the building itself.

For example, if the building has:

```json
"SeasonOffset": {
  "X": 160,
  "Y": 0
}
```

the Draw Layer's `SourceRect` is adjusted automatically based on the current season.

### Animation

Animated Draw Layers use the same general frame layout behavior as vanilla Building Draw Layers.

Example:

```json
{
  "Id": "Animal",
  "SourceRect": {
    "X": 0,
    "Y": 144,
    "Width": 16,
    "Height": 16
  },
  "DrawPosition": "48, 8",
  "FrameDuration": 1500,
  "FrameCount": 6,
  "FramesPerRow": -1
}
```

With `FramesPerRow: -1`, frames are read horizontally from the spritesheet.

### Conditional Draw Layers

The `Condition` property accepts a Stardew Valley Game State Query.

For example:

```json
"Condition": "TIME 600 1530, WEATHER Here Sun Wind"
```

This layer is shown only between 6:00 AM and 3:30 PM when the current weather is `Sun` or `Wind`.

Conditions may use normal Game State Query syntax.

For example:

```json
"Condition": "TIME 600 1530, WEATHER Here Sun Wind, !SEASON Winter"
```

or:

```json
"Condition": "TIME 600 1530, WEATHER Here Sun, SEASON Winter"
```

If `Condition` is omitted, the Draw Layer is always eligible to be drawn.

### Foreground and Background

By default, T's Core Draw Layers are drawn in front of the building.

Set:

```json
"DrawInBackground": true
```

to draw the layer behind the building instead.

### Controlling Draw Layers with CustomFields

To allow Content Patcher to enable or disable all Draw Layers in a provider:

```json
{
  "DrawLayersEnabledField": "MyBuilding/DrawLayersEnabled"
}
```

Then set the corresponding `CustomFields` value:

```json
"CustomFields": {
  "MyBuilding/DrawLayersEnabled": "false"
}
```

When disabled, Draw Layers defined by that provider are not drawn.

---

## Valley Farm Only

`ValleyFarmOnly` can restrict a building to the main Valley farm's construction menu.

Example:

```json
{
  "Id": "MyBuildingProvider",
  "BuildingType": "YourModId_MyBuilding",
  "ValleyFarmOnly": true
}
```

When set to `true`, the building appears in the construction menu only when the main Valley farm is selected as the construction location.

It will not appear when the construction menu targets another buildable location, such as a custom buildable map.

```text
ValleyFarmOnly: false / omitted
→ Normal Stardew Valley construction rules are used.

ValleyFarmOnly: true
→ The building is available only when constructing on the main Valley farm.
```

This restriction is applied while the corresponding Building Provider is enabled.

If the provider is disabled through `BuildingsEnabledField`, its `ValleyFarmOnly` restriction is not applied.

> **Note:** `ValleyFarmOnly` controls whether the building appears in the construction menu for the selected target location. It does not modify the building's `Data/Buildings` entry.

---

## Example

The following example combines several Building Services features in a single provider:

```json
{
  "Id": "MyBuildingProvider",
  "BuildingType": "YourModId_MyBuilding",

  "ValleyFarmOnly": true,

  "BuildingsEnabledField": "MyBuilding/BuildingsEnabled",
  "LightsEnabledField": "MyBuilding/LightsEnabled",
  "DrawLayersEnabledField": "MyBuilding/DrawLayersEnabled",

  "Lights": [
    {
      "Id": "LeftLamp",
      "OffsetX": 0,
      "OffsetY": -2,
      "Radius": 2,
      "Color": "0,0,0"
    }
  ],

  "DrawLayers": [
    {
      "Id": "Animal",
      "SourceRect": {
        "X": 0,
        "Y": 144,
        "Width": 16,
        "Height": 16
      },
      "DrawPosition": "48, 8",
      "FrameDuration": 1500,
      "FrameCount": 6,
      "Condition": "TIME 600 1530, WEATHER Here Sun Wind"
    }
  ]
}
```

This provider:

- Targets `YourModId_MyBuilding`.
- Restricts the building's construction menu entry to the main Valley farm.
- Allows the entire provider to be enabled or disabled through a `CustomFields` value.
- Adds a nighttime Building Light.
- Adds an animated Draw Layer with a Game State Query condition.
- Allows the Light and Draw Layers to be enabled or disabled independently.

---

## Debugging

T's Core provides debug commands for inspecting Building Providers and buildings currently placed on the farm.

---

### Reloading Building Providers

Building Providers can be reloaded without restarting the game:

```text
tscore_reload building
```

You can also reload all supported T's Core resources:

```text
tscore_reload
```

or:

```text
tscore_reload all
```

After reloading, T's Core rescans `assets/buildings` and registers the current Building Provider JSON files.

> **Note:** Reloading Building Providers only reloads data managed by T's Core. It does not reload Content Patcher patches.

---

### Inspecting Building Providers

Use:

```text
tscore_debug_buildings
```

to display all currently registered Building Providers.

The output includes:

- Total registered Provider count.
- Number of Providers registered by each Content Pack.
- Provider IDs.

To display detailed information for a specific Provider, enter its ID:

```text
tscore_debug_buildings MyBuildingProvider
```

The detailed output includes the Provider's target `BuildingType`, Enabled Fields, `ValleyFarmOnly`, Lights, Draw Layers, and their settings.

---

### Inspecting Farm Buildings

Use:

```text
tscore_debug_farmbuildings
```

to inspect buildings currently placed on the main farm.

The output includes each building's:

- Internal building type.
- Tile position.
- Size.
- Interior location.

This is useful when checking the `BuildingType` value used by Building Providers.

> **Note:** A save must be loaded before using `tscore_debug_farmbuildings`.

---

## Notes

Building Services are designed for use with T's Core Content Packs and require no C# code.

Building Provider features are applied in addition to the building's normal `Data/Buildings` behavior.

Whenever possible, use unique Provider IDs and unique `CustomFields` keys to avoid conflicts with other Content Packs.

Additional Building Services features may be added in future versions of T's Core.

---

## Modder Guide

- ← [Warp Services](ModderGuide_WarpServices.md)
- ↑ [Guide Index](#top)
- → [Notification System](ModderGuide_NotificationSystem.md)

← [Back to README](../README.md)

← [Back to Modder Guide](ModderGuide.md)




