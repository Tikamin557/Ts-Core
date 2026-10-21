# Custom Flooring

← [Modder Guide](ModderGuide.md) | [README](../README.md)

T's Core 1.9.9 provides `TsCore/CustomFlooring`, a Content Patcher data asset for extending **vanilla Flooring**. A definition is keyed by the **floor ID from `Data/FloorsAndPaths`**, not the object item ID. The corresponding `Data/FloorsAndPaths` entry must point to a valid flooring item through `ItemId`.

## Registering flooring

Example Content Patcher `Changes` entry (replace the example floor ID, item definition, and texture path with your own):

```json
{
  "Action": "EditData",
  "Target": "TsCore/CustomFlooring",
  "Entries": {
    "Example.Mod_StoneFloor": {
      "PlacementHighlight": "Outline",
      "ShowRemovalDebris": false,
      "VariantChangeSound": "shwip",
      "VariantPreviewEnabled": true,
      "VariantPreviewMode": "Builder",
      "VariantPreviewColumns": 4,
      "Variants": [
        { "Id": "plain", "DisplayName": "Plain", "Texture": "Mods/Example/FloorTextures", "X": 0, "Y": 0 },
        { "Id": "moss", "DisplayName": "Mossy", "Texture": "Mods/Example/FloorTextures", "X": 16, "Y": 0 }
      ]
    }
  }
}
```

Load the `Mods/Example/FloorTextures` texture asset separately using Content Patcher. `X` and `Y` are **pixel coordinates** of a 16×16 image in that asset. Every variant needs a **unique, stable `Id`**. Its ID is stored on placed flooring; avoid renaming it when rearranging variants. At least one variant is needed for a useful definition.

## Flooring-level properties

| Property | Default | Description |
|---|---|---|
| `Variants` | `[]` | Array of texture variants. |
| `ObjectTexture` | omitted | Optional inventory item icon edit: `Texture`, `X`, `Y`, `PatchMode` (`Replace` or `Overlay`). |
| `ShowRemovalDebris` | `true` | Whether removal produces vanilla debris. |
| `AllowFlooringReplacement` | `false` | Allows replacement of this already-placed custom flooring with another floor; returns the original item when replacement succeeds. |
| `PlacementHighlight` | `Default` | `Default`, `None`, or `Outline` placement preview style. |
| `VariantChangeSound` | omitted (silent) | Sound when switching the selected variant before placement. |
| `VariantPreviewEnabled` | `false` | Show texture selection UI while holding the floor item. |
| `VariantPreviewShowName` | `false` | Show the selected variant's name in a separate window. |
| `VariantPreviewMode` | `Carousel` | `Carousel` (neighboring choices) or `Builder` (grid). |
| `VariantPreviewColumns` | `5` | Builder columns, clamped to 1–10. |
| `SelectablePreviewEmptyOrders` | `""` | Comma-separated, 1-based empty positions in Builder mode. |
| `VariantPreviewCount` | `5` | Carousel display count: 3, 5, 7, or 9. |
| `VariantPreviewScale` | `0` | UI scale adjustment, clamped to 0–5. |
| `VariantPreviewOffsetX`, `VariantPreviewOffsetY` | `0` | UI position adjustments in 10-UI-pixel steps. |
| `IdentificationMarker` | omitted | Optional marker image (`Texture`, `X`, `Y`) for identification display. |

`ObjectTexture.PatchMode` supports `Replace` (default) and `Overlay`. The inventory icon is handled separately from the placed floor texture.

## Variant properties

| Property | Default | Description |
|---|---|---|
| `Id` | `""` | Required stable variant ID; unique within this floor. |
| `DisplayName` | omitted | Optional preview label. |
| `SelectablePreviewOrder` | omitted | Optional Builder-only display order; unspecified variants follow specified ones. |
| `Texture`, `X`, `Y` | `""`, `0`, `0` | 16×16 image asset and pixel coordinates. |
| `PlacementSound`, `RemovalSound`, `FootstepSound` | inherited | Per-variant sounds; omitted uses the base floor sound, explicit `null` makes that sound silent. |
| `RandomTextures` | omitted | Weighted random texture candidates. |
| `RandomTextureChange` | `Daily` | `AMPM`, `DayNight`, `Daily`, `Weekly`, or `Seasonal`. |
| `RandomTextureShared` | `false` | Share one result across placements with the same floor ID and variant ID. |
| `Animation` | omitted | Array of frames (`X`, `Y`, `Duration` in milliseconds). |
| `AnimationId` | omitted | Optional identifier to separate animation phases. |
| `AnimationSync` | `true` | When `false`, store a per-placement animation phase. |

`RandomTextures` candidates use `Texture`, `X`, `Y`, optional `FootstepSound` and `Animation`, and `Weight` (default `1`). **Positive weights** determine relative selection probability; weights of `0` or less are excluded. If *all* candidates have non-positive weights, the implementation falls back to equal selection among the candidates.

```json
"RandomTextures": [
  { "Texture": "Mods/Example/FloorTextures", "X": 0, "Y": 16, "Weight": 3 },
  { "Texture": "Mods/Example/FloorTextures", "X": 16, "Y": 16, "Weight": 1 }
]
```

For this example, the relative chances are 3:1. Random selection is deterministic within the configured period, rather than rerolled every draw. `AMPM` switches at 12:00, `DayNight` at 18:00, `Daily` once per day, `Weekly` on days 1/8/15/22, and `Seasonal` at season changes. An individual candidate may have its own animation or footstep sound.

## Controls and compatibility

While holding custom flooring, **right-click places**, **left-click changes the selected texture**, and **Shift + left-click picks up** flooring using the configured controls. T's Core also provides configurable keybinds for previous texture and identifying placed flooring. The preview UI is optional and can be configured per floor.

Custom flooring uses the game's normal flooring objects. Definitions are supplied through Content Patcher, so remember to include T's Core as a required dependency. Test placement, replacement, saving/loading, seasons, and interactions with any texture-recolor mods your content pack supports.
