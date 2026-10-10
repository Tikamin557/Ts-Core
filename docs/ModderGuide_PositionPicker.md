# 📖 Modder Guide

This guide explains how to add T's Core's visual **Position Picker** to a Content Patcher Content Pack.

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
- ✅ **Position Picker** *(Current Page)*
- 📄 [Post Renovation Patch](ModderGuide_PostRenovationPatch.md)
- 📄 [Polyamory Sweet Rooms Integration](ModderGuide_PolyamorySweetRoomsIntegration.md)
- 📄 [Other Features](ModderGuide_OtherFeatures.md)

← [Back to README](../README.md)

← [Back to Modder Guide](ModderGuide.md)

---

# Position Picker

Position Picker lets a Content Patcher Content Pack add a button to its own Generic Mod Config Menu which allows the player to select X/Y coordinates directly on a map.

The selected coordinates are written back to the Content Pack's existing ConfigSchema fields and saved through Content Patcher's normal config handling. No custom C# code is required.

Position Pickers are registered through:

```text
TsCore/PositionPickers
```

## Requirements

- T's Core;
- Content Patcher;
- Generic Mod Config Menu;
- X and Y fields already defined in the Content Pack's `ConfigSchema`;
- a map asset used as the placement preview.

The picker doesn't automatically warp the player. The player must already be in the configured `Location` before the button can be used.

## Registering a Position Picker

```json
{
  "Action": "EditData",
  "Target": "TsCore/PositionPickers",
  "Entries": {
    "{{ModId}}_ExamplePosition": {
      "ContentPackId": "{{ModId}}",
      "GMCM_Name": "{{i18n:position-picker.name}}",
      "GMCM_Description": "{{i18n:position-picker.description}}",
      "GMCM_Button": "{{i18n:position-picker.button}}",
      "GMCM_WorldRequired": "{{i18n:position-picker.world-required}}",
      "GMCM_LocationRequired": "{{i18n:position-picker.location-required}}",
      "XField": "ExamplePosition_X",
      "YField": "ExamplePosition_Y",
      "AfterField": "ExamplePosition_Y",
      "Location": "FarmHouse",
      "PreviewMap": "Mods/{{ModId}}/ExamplePosition_Preview",
      "PreviewNote": "{{i18n:position-picker.preview-note}}",
      "AnchorX": 0,
      "AnchorY": 0
    }
  }
}
```

## Properties

| Property | Required | Description |
|----------|----------|-------------|
| Entry key | ✅ | Unique ID for this Position Picker definition. |
| `ContentPackId` | ✅ | UniqueID of the Content Pack whose GMCM receives the picker. Normally use `{{ModId}}`. |
| `GMCM_Name` | ❌ | GMCM option name. Uses T's Core's default text when omitted. |
| `GMCM_Description` | ❌ | Normal description/hover text. Uses T's Core's default text when omitted. |
| `GMCM_Button` | ❌ | Text shown inside the picker button. Uses T's Core's default text when omitted. |
| `GMCM_WorldRequired` | ❌ | Button hover text when no save is loaded. |
| `GMCM_LocationRequired` | ❌ | Button hover text when the player isn't in the required location. |
| `XField` | ✅ | ConfigSchema field which receives the selected X coordinate. |
| `YField` | ✅ | ConfigSchema field which receives the selected Y coordinate. |
| `BeforeField` | ❌ | ConfigSchema field key before which the picker button should be inserted. Takes priority over `AfterField` when both are set. |
| `AfterField` | ❌ | ConfigSchema field key after which the picker button should be inserted. If it isn't found, the picker is added at the end of the GMCM page. |
| `Location` | ✅ | Location name in which the picker can be used. Both `Name` and `NameOrUniqueName` are accepted by T's Core. |
| `PreviewMap` | ✅ | Map asset drawn as the placement preview. `{{ModId}}` in this value is replaced with the Content Pack's UniqueID. |
| `PreviewNote` | ❌ | Additional note displayed below the coordinate/control guide while selecting a position. Long text is automatically wrapped. |
| `AnchorX` | ❌ | X tile inside `PreviewMap` which should align with the selected coordinate. Default `0`. |
| `AnchorY` | ❌ | Y tile inside `PreviewMap` which should align with the selected coordinate. Default `0`. |

Use `BeforeField` to place the picker immediately before a ConfigSchema field, or `AfterField` to place it immediately after one. If the target field is not found, the picker is added at the end of the GMCM page.

Since v1.9.4, clicking or holding an overlapping GMCM dropdown option does not accidentally activate the Position Picker button. Its hover tooltip is also suppressed while the dropdown is active.

`XField`, `YField`, `Location`, and `PreviewMap` are required. Invalid definitions are ignored and a warning is written to the SMAPI log.

## Preview Map and Anchor

`PreviewMap` is only a visual preview. It doesn't determine what your Content Patcher or Post Renovation Patch actually places after the coordinates are saved.

This means the preview can include surrounding walls, beams, or other context which isn't part of the final patch.

For example, a 1×4 preview map could contain:

```text
Y=0  Beam
Y=1  Wall
Y=2  Marker  ← selected coordinate
Y=3  Wall
```

Use:

```json
"AnchorX": 0,
"AnchorY": 2
```

to align the marker tile at Y=2 with the coordinate selected by the player.

Tile rotation and flip information in the preview map is respected.

## Player Controls

When the picker is active:

- move the mouse to choose a tile;
- left-click to confirm;
- right-click or press `Esc` to cancel;
- move the mouse to a screen edge to scroll the camera;
- `WASD` and arrow keys can also scroll the camera.

The selected coordinate is always clamped to the actual map bounds. The camera can scroll slightly beyond the map edges so the preview can be viewed without being hidden by the guide.

## Saving and Applying the Coordinates

When the player confirms a tile, T's Core updates `XField` and `YField`, then invokes Content Patcher's normal save/apply handling for that Content Pack.

This means existing patches which use those Config Tokens can update through the normal Content Patcher config flow.

The Position Picker doesn't place the final map patch itself. Your Content Pack remains responsible for using the saved X/Y values.

## GMCM Behavior

The picker button is disabled when:

- no save is loaded; or
- the player isn't currently in the configured `Location`.

Hovering the disabled button shows the corresponding reason. Custom text can be supplied through `GMCM_WorldRequired` and `GMCM_LocationRequired`.

`AfterField` can be used to place the picker directly after the related X/Y settings instead of at the bottom of the menu.

## Example ConfigSchema

```json
"ConfigSchema": {
  "ExamplePosition_X": {
    "Default": "20"
  },
  "ExamplePosition_Y": {
    "Default": "10"
  }
}
```

Your patches can then use the normal Config Tokens:

```json
"X": "{{ExamplePosition_X}}",
"Y": "{{ExamplePosition_Y}}"
```

---

## Modder Guide

- ← [Content Patcher Integration](ModderGuide_ContentPatcherIntegration.md)
- ↑ [Guide Index](#top)
- → [Post Renovation Patch](ModderGuide_PostRenovationPatch.md)

← [Back to README](../README.md)

← [Back to Modder Guide](ModderGuide.md)
