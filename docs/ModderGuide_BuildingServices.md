# 📖 Modder Guide

This guide explains how to use the public features provided by **T's Core** in Content Patcher.

<a id="top"></a>

## Guide Index

- 📄 [Relationship Services](ModderGuide_RelationshipServices.md)
- 📄 [Location Services](ModderGuide_LocationServices.md)
- 📄 [Warp Services](ModderGuide_WarpServices.md)
- 📄 [Map Properties](ModderGuide_MapProperties.md)
- ✅ **Building Services** *(Current Page)*
- 📄 [Machine Interaction](ModderGuide_MachineInteraction.md)
- 📄 [Migration System](ModderGuide_MigrationSystem.md)
- 📄 [Notification System](ModderGuide_NotificationSystem.md)
- 📄 [Content Patcher Integration](ModderGuide_ContentPatcherIntegration.md)

← [Back to README](../README.md)

← [Back to Modder Guide](ModderGuide.md)

---

# Building Services

Building Services provide additional functionality for buildings defined in `Data/Buildings`.

T's Core allows Content Patcher Content Packs to add Building Providers through the custom `TsCore/BuildingProviders` data asset.

Building Providers can add features such as nighttime light sources, conditional Draw Layers, feature enable/disable controls, and construction location restrictions without requiring any C# code.

This guide explains how to register Building Providers through Content Patcher and configure the available building-related features.

---

## Contents

- [Content Patcher Setup](#content-patcher-setup)
- [Building Provider](#building-provider)
- [Enabling and Disabling Features](#enabling-and-disabling-features)
- [Building Lights](#building-lights)
- [Building Draw Layers](#building-draw-layers)
- [Valley Farm Only](#valley-farm-only)
- [Example](#example)
- [Debugging](#debugging)
- [Notes](#notes)

---

## Content Patcher Setup

Building Providers are registered by editing the custom data asset:

```text
TsCore/BuildingProviders
```

This means you can use Building Services directly from a normal Content Patcher Content Pack.

Your Content Pack should depend on both Content Patcher and T's Core.

### manifest.json

```json
{
  "Name": "[CP] My Building Mod",
  "Author": "YourName",
  "Version": "1.0.0",
  "UniqueID": "YourName.MyBuildingMod",
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

### Registering a Building Provider

Use Content Patcher's `EditData` action to add entries to `TsCore/BuildingProviders`.

```json
{
  "Action": "EditData",
  "Target": "TsCore/BuildingProviders",
  "Entries": {
    "YourModId_MyBuildingProvider": {
      "BuildingType": "YourModId_MyBuilding"
    }
  }
}
```

Each entry in `TsCore/BuildingProviders` represents one Building Provider.

The entry key is the Provider ID:

```json
"YourModId_MyBuildingProvider": {
  "BuildingType": "YourModId_MyBuilding"
}
```

Provider IDs should be unique. Using your mod's UniqueID or another unique prefix is recommended to avoid conflicts with other Content Packs.

Multiple Building Providers may target the same `BuildingType`.

---

## Building Provider

A Building Provider targets a building type from `Data/Buildings` and adds T's Core-specific functionality to it.

Basic example:

```json
{
  "Action": "EditData",
  "Target": "TsCore/BuildingProviders",
  "Entries": {
    "YourModId_MyBuildingProvider": {
      "BuildingType": "YourModId_MyBuilding"
    }
  }
}
```

### Building Provider Properties

| Property | Required | Default | Description |
|----------|----------|---------|-------------|
| `BuildingType` | ✅ | — | Building type ID from `Data/Buildings` targeted by this provider. |
| `BuildingsEnabled` | Optional | `true` | Enables or disables the entire Building Provider. |
| `BuildingsEnabledField` | Optional | — | `CustomFields` key used to enable or disable the entire Building Provider. |
| `LightsEnabled` | Optional | `true` | Enables or disables all Building Lights in this provider. |
| `LightsEnabledField` | Optional | — | `CustomFields` key used to enable or disable all Building Lights in this provider. |
| `DrawLayersEnabled` | Optional | `true` | Enables or disables all Building Draw Layers in this provider. |
| `DrawLayersEnabledField` | Optional | — | `CustomFields` key used to enable or disable all Building Draw Layers in this provider. |
| `ValleyFarmOnly` | Optional | `false` | If `true`, the building is shown in the construction menu only for the main Valley farm. |
| `Lights` | Optional | — | Building Lights added by this provider. |
| `DrawLayers` | Optional | — | Conditional Building Draw Layers added by this provider. |

The Provider ID is not specified inside the provider data. It is the entry key in `TsCore/BuildingProviders`.

For example:

```json
"YourModId_MyBuildingProvider": {
  "BuildingType": "YourModId_MyBuilding"
}
```

Here, `YourModId_MyBuildingProvider` is the Provider ID.

Multiple Building Providers may target the same `BuildingType`.

This can be useful when different configurations of the same building need different T's Core features.

---

## Enabling and Disabling Features

Building Services provide two ways to enable or disable features:

1. Direct `Enabled` properties in `TsCore/BuildingProviders`.
2. `EnabledField` properties linked to the target building's `Data/Buildings` → `CustomFields`.

Both methods can be used together.

When both are used, **both must be enabled** for the corresponding feature to be active.

---

### Direct Enabled Properties

The provider-level direct controls are:

```json
{
  "BuildingsEnabled": true,
  "LightsEnabled": true,
  "DrawLayersEnabled": true
}
```

All three default to `true` when omitted.

#### BuildingsEnabled

`BuildingsEnabled` controls the entire Building Provider.

```json
"BuildingsEnabled": false
```

When `false`, features controlled by that provider are disabled.

This also disables its Building Lights, Building Draw Layers, and `ValleyFarmOnly` restriction.

#### LightsEnabled

`LightsEnabled` controls all Building Lights defined by the provider.

```json
"LightsEnabled": false
```

#### DrawLayersEnabled

`DrawLayersEnabled` controls all Building Draw Layers defined by the provider.

```json
"DrawLayersEnabled": false
```

Because these values are part of `TsCore/BuildingProviders`, they can be edited directly through Content Patcher patches.

For example:

```json
{
  "Action": "EditData",
  "Target": "TsCore/BuildingProviders",
  "TargetField": [
    "YourModId_MyBuildingProvider",
    "LightsEnabled"
  ],
  "Value": false
}
```

Content Patcher conditions can therefore be used to change Building Services features dynamically.

---

### EnabledField Properties

Building Providers can also use fields in the target building's `Data/Buildings` → `CustomFields`.

Three provider-level properties are available:

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

#### BuildingsEnabledField

`BuildingsEnabledField` controls the entire Building Provider through a `CustomFields` value.

```json
"BuildingsEnabledField": "MyBuilding/BuildingsEnabled"
```

#### LightsEnabledField

`LightsEnabledField` controls all Building Lights defined by the provider.

```json
"LightsEnabledField": "MyBuilding/LightsEnabled"
```

#### DrawLayersEnabledField

`DrawLayersEnabledField` controls all Building Draw Layers defined by the provider.

```json
"DrawLayersEnabledField": "MyBuilding/DrawLayersEnabled"
```

If an Enabled Field is not specified, the corresponding Enabled Field check is treated as enabled.

If the specified `CustomFields` key does not exist, the feature is also treated as enabled.

---

### Combining Enabled and EnabledField

Direct `Enabled` properties and `EnabledField` properties use AND behavior.

For example:

```json
{
  "LightsEnabled": true,
  "LightsEnabledField": "MyBuilding/LightsEnabled"
}
```

with:

```json
"CustomFields": {
  "MyBuilding/LightsEnabled": "false"
}
```

results in the Lights being disabled.

Likewise:

```json
{
  "LightsEnabled": false,
  "LightsEnabledField": "MyBuilding/LightsEnabled"
}
```

remains disabled even if the `CustomFields` value is `true`.

The effective hierarchy for Lights is:

```text
BuildingsEnabled
→ BuildingsEnabledField
→ LightsEnabled
→ LightsEnabledField
→ individual Light Enabled
→ individual Light EnabledField
```

The effective hierarchy for Draw Layers is:

```text
BuildingsEnabled
→ BuildingsEnabledField
→ DrawLayersEnabled
→ DrawLayersEnabledField
→ individual Draw Layer Enabled
→ individual Draw Layer EnabledField
```

---

### Multiple Providers for the Same Building

Different Building Providers targeting the same `BuildingType` can be enabled or disabled independently.

For example:

```json
{
  "Action": "EditData",
  "Target": "TsCore/BuildingProviders",
  "Entries": {
    "YourModId_MyBuilding_VariantA": {
      "BuildingType": "YourModId_MyBuilding",
      "BuildingsEnabled": true
    },
    "YourModId_MyBuilding_VariantB": {
      "BuildingType": "YourModId_MyBuilding",
      "BuildingsEnabled": false
    }
  }
}
```

Content Patcher can edit these provider entries independently based on config options or other conditions.

---

## Building Lights

Building Lights add light sources to buildings.

Lights are positioned relative to the building's top-left tile and automatically follow the building when it is moved.

They are also removed when the building is demolished.

Example:

```json
{
  "Action": "EditData",
  "Target": "TsCore/BuildingProviders",
  "Entries": {
    "YourModId_MyBuildingProvider": {
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
  }
}
```

### Building Light Properties

| Property | Required | Default | Description |
|----------|----------|---------|-------------|
| `Id` | ✅ | — | Unique Light ID within the provider. |
| `Enabled` | Optional | `true` | Enables or disables this individual Light. |
| `EnabledField` | Optional | — | `CustomFields` key used to enable or disable this individual Light. |
| `OffsetX` | Optional | `0` | Horizontal tile offset from the building's top-left tile. Negative values are supported. |
| `OffsetY` | Optional | `0` | Vertical tile offset from the building's top-left tile. Negative values are supported. |
| `Radius` | Optional | `4` | Radius of the light source. |
| `Color` | Optional | `"0,0,0"` | Light color in `"R,G,B"` format. |

The light position is calculated relative to the current position of each matching building.

If multiple buildings of the same `BuildingType` exist, each building receives its own Light instances.

Building Lights are active only when the game considers the location dark.

---

### Controlling All Lights

All Lights in a provider can be controlled directly:

```json
{
  "LightsEnabled": false
}
```

or through a `CustomFields` key:

```json
{
  "LightsEnabledField": "MyBuilding/LightsEnabled"
}
```

with:

```json
"CustomFields": {
  "MyBuilding/LightsEnabled": "false"
}
```

Both methods can be used together.

---

### Controlling Individual Lights

Each Light also has its own `Enabled` and `EnabledField` properties.

Direct control:

```json
{
  "Id": "LeftLamp",
  "Enabled": false,
  "OffsetX": 0,
  "OffsetY": -2
}
```

Using `CustomFields`:

```json
{
  "Id": "LeftLamp",
  "EnabledField": "MyBuilding/LeftLampEnabled",
  "OffsetX": 0,
  "OffsetY": -2
}
```

with:

```json
"CustomFields": {
  "MyBuilding/LeftLampEnabled": "true"
}
```

`Enabled` defaults to `true`.

If both `Enabled` and `EnabledField` are used, both must be enabled for the Light to appear.

---

## Building Draw Layers

Building Draw Layers add extra graphical layers to a building.

They are drawn in addition to any Draw Layers already defined by the building's original `Data/Buildings` entry.

T's Core does not replace the building's existing `DrawLayers`.

Example:

```json
{
  "Action": "EditData",
  "Target": "TsCore/BuildingProviders",
  "Entries": {
    "YourModId_MyBuildingProvider": {
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
  }
}
```

### Draw Layer Properties

| Property | Required | Default | Description |
|----------|----------|---------|-------------|
| `Id` | ✅ | — | Unique Draw Layer ID within the provider. |
| `Enabled` | Optional | `true` | Enables or disables this individual Draw Layer. |
| `EnabledField` | Optional | — | `CustomFields` key used to enable or disable this individual Draw Layer. |
| `Texture` | Optional | Building texture | Texture asset used for the layer. |
| `SourceRect` | ✅ | — | Pixel area of the texture to draw. |
| `DrawPosition` | Optional | `0, 0` | Draw position relative to the building. |
| `DrawInBackground` | Optional | `false` | Draw the layer behind the building instead of in front of it. |
| `SortTileOffset` | Optional | `0` | Y tile offset used when calculating the render order. |
| `OnlyDrawIfChestHasContents` | Optional | — | Draw only if the specified Building Chest contains an item. |
| `FrameDuration` | Optional | `90` | Duration of each animation frame in milliseconds. A single value or per-frame values can be used. |
| `FrameCount` | Optional | `1` | Number of animation frames. |
| `FramesPerRow` | Optional | `-1` | Number of animation frames per spritesheet row. |
| `AnimalDoorOffset` | Optional | `0, 0` | Pixel offset applied based on the building's animal door open state. |
| `Condition` | Optional | — | Game State Query condition which must match for the layer to be drawn. |

---

### Texture

If `Texture` is omitted, T's Core uses the target building's texture.

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

---

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

---

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

`FrameDuration` can also specify a separate duration for each frame:

```json
{
  "FrameDuration": [ 800, 80, 80, 80 ],
  "FrameCount": 4
}
```

When using per-frame durations, the number of values should match `FrameCount`.

---

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

---

### Foreground and Background

By default, T's Core Draw Layers are drawn in front of the building.

Set:

```json
"DrawInBackground": true
```

to draw the layer behind the building instead.

---

### Controlling All Draw Layers

All Draw Layers in a provider can be controlled directly:

```json
{
  "DrawLayersEnabled": false
}
```

or through a `CustomFields` key:

```json
{
  "DrawLayersEnabledField": "MyBuilding/DrawLayersEnabled"
}
```

with:

```json
"CustomFields": {
  "MyBuilding/DrawLayersEnabled": "false"
}
```

Both methods can be used together.

---

### Controlling Individual Draw Layers

Each Draw Layer also has its own `Enabled` and `EnabledField` properties.

Direct control:

```json
{
  "Id": "Animal",
  "Enabled": false,
  "SourceRect": {
    "X": 0,
    "Y": 144,
    "Width": 16,
    "Height": 16
  }
}
```

Using `CustomFields`:

```json
{
  "Id": "Animal",
  "EnabledField": "MyBuilding/AnimalEnabled",
  "SourceRect": {
    "X": 0,
    "Y": 144,
    "Width": 16,
    "Height": 16
  }
}
```

with:

```json
"CustomFields": {
  "MyBuilding/AnimalEnabled": "true"
}
```

`Enabled` defaults to `true`.

If both `Enabled` and `EnabledField` are used, both must be enabled for the Draw Layer to appear.

---

## Valley Farm Only

`ValleyFarmOnly` can restrict a building to the main Valley farm's construction menu.

Example:

```json
{
  "Action": "EditData",
  "Target": "TsCore/BuildingProviders",
  "Entries": {
    "YourModId_MyBuildingProvider": {
      "BuildingType": "YourModId_MyBuilding",
      "ValleyFarmOnly": true
    }
  }
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

This restriction is applied only while the corresponding Building Provider is enabled.

If the provider is disabled through either `BuildingsEnabled` or `BuildingsEnabledField`, its `ValleyFarmOnly` restriction is not applied.

> **Note:** `ValleyFarmOnly` controls whether the building appears in the construction menu for the selected target location. It does not modify the building's `Data/Buildings` entry.

---

## Example

The following example combines several Building Services features in a single Content Patcher patch:

```json
{
  "Action": "EditData",
  "Target": "TsCore/BuildingProviders",
  "Entries": {
    "YourModId_MyBuildingProvider": {
      "BuildingType": "YourModId_MyBuilding",

      "ValleyFarmOnly": true,

      "BuildingsEnabled": true,
      "BuildingsEnabledField": "MyBuilding/BuildingsEnabled",

      "LightsEnabled": true,
      "LightsEnabledField": "MyBuilding/LightsEnabled",

      "DrawLayersEnabled": true,
      "DrawLayersEnabledField": "MyBuilding/DrawLayersEnabled",

      "Lights": [
        {
          "Id": "LeftLamp",
          "Enabled": true,
          "EnabledField": "MyBuilding/LeftLampEnabled",
          "OffsetX": 0,
          "OffsetY": -2,
          "Radius": 2,
          "Color": "0,0,0"
        }
      ],

      "DrawLayers": [
        {
          "Id": "Animal",
          "Enabled": true,
          "EnabledField": "MyBuilding/AnimalEnabled",
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
  }
}
```

This provider:

- Targets `YourModId_MyBuilding`.
- Restricts the building's construction menu entry to the main Valley farm.
- Supports both direct and `CustomFields`-based enable/disable controls.
- Adds a nighttime Building Light.
- Allows the individual Light to be enabled or disabled.
- Adds an animated Draw Layer with a Game State Query condition.
- Allows the individual Draw Layer to be enabled or disabled.
- Allows all Lights or all Draw Layers to be controlled independently.

---

## Debugging

T's Core provides debug commands for inspecting Building Providers and buildings currently placed on the farm.

### Reloading Content Patcher Content Packs

Since Building Providers are registered through the `TsCore/BuildingProviders` data asset, they can be updated by reloading the Content Patcher Content Pack which edits that asset.

T's Core provides:

```text
tscore_cp_reload <ContentPackId>
```

for reloading a Content Patcher Content Pack while the game is running.

For details about `tscore_cp_reload` and other Content Patcher integration features, see the [Content Patcher Integration](ModderGuide_ContentPatcherIntegration.md) guide.

---

### Inspecting Building Providers

Use:

```text
tscore_debug_buildings
```

to inspect the currently registered Building Providers.

To inspect a specific Provider:

```text
tscore_debug_buildings YourModId_MyBuildingProvider
```

---

### Inspecting Farm Buildings

Use:

```text
tscore_debug_farmbuildings
```

to inspect buildings currently placed on the main farm.

This can be useful when checking the `BuildingType` used by a Building Provider.

> **Note:** A save must be loaded before using `tscore_debug_farmbuildings`.

---

## Notes

Building Services are designed for use with Content Patcher Content Packs and require no C# code.

Building Providers are registered through the custom `TsCore/BuildingProviders` data asset.

Building Provider features are applied in addition to the building's normal `Data/Buildings` behavior.

Provider IDs are the entry keys in `TsCore/BuildingProviders`. Using a unique prefix, such as your mod's UniqueID, is recommended to avoid conflicts with other Content Packs.

Direct `Enabled` properties can be edited through Content Patcher, while `EnabledField` properties can be used when you want T's Core to read the enabled state from the target building's `CustomFields`.

Additional Building Services features may be added in future versions of T's Core.

---

## Modder Guide

- ← [Map Properties](ModderGuide_MapProperties.md)
- ↑ [Guide Index](#top)
- → [Machine Interaction](ModderGuide_MachineInteraction.md)

← [Back to README](../README.md)

← [Back to Modder Guide](ModderGuide.md)
