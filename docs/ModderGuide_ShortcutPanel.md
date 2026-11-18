# Shortcut Panel

T's Core provides an optional in-game Shortcut Panel for players and a shortcut registration API for C# mod authors.

## Position and controls (v1.9.2)

Open T's Core's Generic Mod Config Menu settings and set **Shortcut Panel Position** to **Free**. The default **Fixed** mode keeps the panel in its original position beside the money UI. In Free mode, drag the move handle above the panel's open/close tab to reposition it. The offset is saved and reused after restarting the game. The panel is kept within the visible screen area.

The open/close tab and panel have separate size settings. Free positioning works with these display scales.

## CJB Cheats Menu warp shortcuts (v1.9.2)

When **CJB Cheats Menu** is installed, open an empty Shortcut Panel slot, choose **Mod Features**, then select **CJB Cheats Menu** and a warp destination from its grouped list. The saved shortcut opens the selected destination. Available destinations follow the warp list provided by CJB Cheats Menu, including its configuration and content edits.

This integration depends on CJB Cheats Menu's internal warp implementation rather than a public API, so future CJB updates may affect compatibility. If warp names appear incorrectly immediately after changing the game language, fully restarting the game may be necessary; the same issue can affect CJB's own warp page.


## Terrain Features shortcut (v1.9.5)

Under **Mod Features**, the Shortcut Panel can open T's Core's **Terrain Features** management menu. The same menu can be opened with the configurable key (default `L`). See the [Terrain Features Guide](ModderGuide_TerrainFeatures.md).

## For C# mod authors

C# mods can register custom actions for the Shortcut Panel using T's Core's public `ITsCoreApi` shortcut registration API. See the API interface and existing Modder Guide for the registration contract.

## Default visibility (v2.0.4)

The Shortcut Panel is **disabled by default on new installations**. Players can enable it through GMCM. Existing enabled/disabled preferences saved in `config.json` are preserved.

## Icon customization (v2.0.6)

T's Core exposes 22 built-in shortcut icons as individual game assets. UI recolors and other Content Patcher content packs can customize them using `Load` or `EditImage`. If no patch is applied, T's Core uses its default PNGs.

### Available assets

| Asset name | Used for | Default image size (px) |
| --- | --- | --- |
| `Mods/Tikamin557.TsCore/ShortcutPanel/CjbCheatsMenu` | CJB Cheats Menu warp | 47x47 |
| `Mods/Tikamin557.TsCore/ShortcutPanel/GameMenu` | Game menu selector | 48x48 |
| `Mods/Tikamin557.TsCore/ShortcutPanel/GameMenuAnimals` | Animals tab | 48x48 |
| `Mods/Tikamin557.TsCore/ShortcutPanel/GameMenuCollections` | Collections tab | 48x48 |
| `Mods/Tikamin557.TsCore/ShortcutPanel/GameMenuCrafting` | Crafting tab | 48x48 |
| `Mods/Tikamin557.TsCore/ShortcutPanel/GameMenuExit` | Exit tab | 48x48 |
| `Mods/Tikamin557.TsCore/ShortcutPanel/GameMenuInventory` | Inventory tab | 48x48 |
| `Mods/Tikamin557.TsCore/ShortcutPanel/GameMenuMap` | Map tab | 48x48 |
| `Mods/Tikamin557.TsCore/ShortcutPanel/GameMenuOptions` | Options tab | 48x48 |
| `Mods/Tikamin557.TsCore/ShortcutPanel/GameMenuSkills` | Skills tab | 48x48 |
| `Mods/Tikamin557.TsCore/ShortcutPanel/GameMenuSocial` | Social tab | 48x48 |
| `Mods/Tikamin557.TsCore/ShortcutPanel/GameMenuSpecialItems` | Special Items & Powers tab | 48x48 |
| `Mods/Tikamin557.TsCore/ShortcutPanel/GmcmSelectMod` | GMCM mod selection | 48x48 |
| `Mods/Tikamin557.TsCore/ShortcutPanel/GmcmTopMenu` | GMCM main menu | 48x48 |
| `Mods/Tikamin557.TsCore/ShortcutPanel/MissingShortcut` | Missing shortcut | 16x16 |
| `Mods/Tikamin557.TsCore/ShortcutPanel/PsrRoomPresets` | Spouse room presets | 48x48 |
| `Mods/Tikamin557.TsCore/ShortcutPanel/ScreenshotCurrentScreen` | Screenshot (current screen) | 16x16 |
| `Mods/Tikamin557.TsCore/ShortcutPanel/ScreenshotFullMap` | Screenshot (full map) | 16x16 |
| `Mods/Tikamin557.TsCore/ShortcutPanel/SmapiConsole` | SMAPI console | 48x48 |
| `Mods/Tikamin557.TsCore/ShortcutPanel/TerrainFeatures` | Terrain Features management | 48x48 |
| `Mods/Tikamin557.TsCore/ShortcutPanel/TimeSkip` | Time skip | 32x32 |
| `Mods/Tikamin557.TsCore/ShortcutPanel/TimeSkipDuration` | Time skip (duration) | 32x32 |

The default PNGs are located in T's Core's `assets/ShortcutPanel/` folder. Transparent PNGs matching the default image dimensions are recommended. Do not append a `.png` extension or a locale suffix to the asset name. These assets are separate from the icons used by the game's own menu.

### Content Patcher examples

To replace the entire Skills shortcut icon using `Load`, add the following patch to the `Changes` array in your `content.json`:

```json
{
    "Action": "Load",
    "Target": "Mods/Tikamin557.TsCore/ShortcutPanel/GameMenuSkills",
    "FromFile": "assets/GameMenuSkills.png"
}
```

Alternatively, use `EditImage`. `PatchMode: Replace` replaces pixels including transparent areas.

```json
{
    "Action": "EditImage",
    "Target": "Mods/Tikamin557.TsCore/ShortcutPanel/GameMenuSkills",
    "FromFile": "assets/GameMenuSkills.png",
    "PatchMode": "Replace"
}
```

These are alternative examples for the same icon; use one or the other. `FromFile` refers to an image in your content pack. This feature requires T's Core v2.0.6 or later. If T's Core is an optional dependency, you can add `When: { "HasMod": "Tikamin557.TsCore" }` to the patch (assuming v2.0.6 or later is installed).

## Symbol Color (v2.0.6)

The Shortcut Panel settings in GMCM include a **Symbol Color** option.

- **Default**: uses the existing game text color (`Game1.textColor`).
- **White**: draws symbols such as the open/close arrow, move handle, and selection-menu scroll arrows in white. Use this option if symbols are difficult to see with a dark UI theme.

This setting does not change normal text or PNG icon colors. Normal text continues to respect UI recolor mods.

The setting is saved in `config.json` as `ShortcutPanelForegroundColor` (default: `Default`).
