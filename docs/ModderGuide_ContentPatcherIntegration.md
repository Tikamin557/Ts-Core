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
- 📄 [Notification System](ModderGuide_NotificationSystem.md)
- ✅ **Content Patcher Integration** *(Current Page)*

← [Back to README](../README.md)

← [Back to Modder Guide](ModderGuide.md)

---

# Content Patcher Integration

Content Patcher Integration provides additional features and development tools for Content Patcher Content Packs.

T's Core can reload a Content Pack's `content.json` together with related ConfigSchema, Config Tokens, GMCM settings, Dynamic Tokens, and patches without restarting the game.

T's Core also extends Content Patcher's `ConfigSchema` with optional properties for controlling GMCM field visibility based on installed mods.

---

## Contents

- [Reloading a Content Patcher Content Pack](#reloading-a-content-patcher-content-pack)
- [What Is Reloaded](#what-is-reloaded)
- [ConfigSchema](#configschema)
- [Config Tokens](#config-tokens)
- [Generic Mod Config Menu](#generic-mod-config-menu)
- [Dynamic Tokens](#dynamic-tokens)
- [Content Patcher Patches](#content-patcher-patches)
- [Disabled Patch Records](#disabled-patch-records)
- [T's Core Custom Data Assets](#ts-core-custom-data-assets)
- [Development Workflow](#development-workflow)
- [Limitations](#limitations)
- [Notes](#notes)

---

## Reloading a Content Patcher Content Pack

Use:

```text
tscore_cp_reload <ContentPackId>
```

where `<ContentPackId>` is the `UniqueID` of the Content Patcher Content Pack you want to reload.

For example:

```text
tscore_cp_reload YourName.MyContentPack
```

T's Core searches the Content Packs currently loaded by Content Patcher and reloads the Content Pack with the specified `UniqueID`.

If the Content Pack cannot be found, T's Core logs a warning and does not perform the reload.

> **Note:** This command is intended for Content Patcher Content Packs. It does not reload arbitrary SMAPI mods.

---

## What Is Reloaded

`tscore_cp_reload` performs more than a normal Content Patcher patch reload.

T's Core reloads or rebuilds the following data:

| Data | Reloaded |
|------|:--------:|
| `content.json` | ✅ |
| Content Patcher patches | ✅ |
| `ConfigSchema` | ✅ |
| `config.json` | ✅ Rebuilt and saved |
| Config Tokens | ✅ |
| GMCM settings | ✅ |
| `DynamicTokens` | ✅ |
| Dynamic Token conditions | ✅ |
| Dynamic Token dependencies | ✅ |
| Old permanently disabled patch records | ✅ Cleared for the target Content Pack |

The general reload process is:

```text
tscore_cp_reload <ContentPackId>
        ↓
Reload content.json
        ↓
Rebuild Config from ConfigSchema
        ↓
Save config.json
        ↓
Rebuild Config Tokens
        ↓
Re-register GMCM settings
        ↓
Rebuild Dynamic Tokens
        ↓
Clear old disabled patch records
        ↓
Reload Content Patcher patches
```

This makes it possible to test many common Content Pack changes without restarting Stardew Valley.

Changes made by the Content Pack to T's Core custom Data Assets are also refreshed through the reloaded Content Patcher patches.

---

## ConfigSchema

When a Content Pack is reloaded, T's Core reads the latest `ConfigSchema` from the reloaded `content.json`.

The Content Pack's Config is then rebuilt using Content Patcher's normal config handling.

This allows changes such as:

- adding Config fields;
- removing Config fields;
- renaming Config fields;
- changing Config field definitions;

to be reflected during development without restarting the game.

For example, if the original schema contains:

```json
"ConfigSchema": {
  "OldSetting": {
    "AllowValues": "A, B, C",
    "Default": "A"
  }
}
```

and it is changed to:

```json
"ConfigSchema": {
  "NewSetting": {
    "AllowValues": "A, B, C, D",
    "Default": "B"
  }
}
```

running:

```text
tscore_cp_reload YourName.MyContentPack
```

causes T's Core to rebuild the Content Pack's Config using the updated schema.

The rebuilt Config is then saved to the Content Pack's `config.json`.

> **Note:** Existing values are preserved or defaulted according to Content Patcher's normal config handling.

---

## Config Tokens

After rebuilding the Config, T's Core also rebuilds the Content Pack's Config Tokens.

Old Config Tokens associated with the previous ConfigSchema are removed before the current Config fields are registered again.

This is important when Config fields are added, removed, or renamed.

For example, changing:

```json
"ConfigSchema": {
  "OldSetting": {
    "Default": true
  }
}
```

to:

```json
"ConfigSchema": {
  "NewSetting": {
    "Default": true
  }
}
```

will remove the old `OldSetting` Config Token and register the new `NewSetting` Config Token when the Content Pack is reloaded.

The Content Patcher Token Context is then updated so the rebuilt Config Tokens can be used by patches and Dynamic Tokens.

---

## Generic Mod Config Menu

If the Content Pack uses ConfigSchema with Generic Mod Config Menu, T's Core re-registers its GMCM configuration after rebuilding the Config.

This allows changes to ConfigSchema to be reflected in GMCM without restarting the game.

For example:

1. Add or modify a ConfigSchema field.
2. Save `content.json`.
3. Run:

```text
tscore_cp_reload YourName.MyContentPack
```

The updated configuration can then be registered with GMCM using the new Config structure.

> **Note:** Generic Mod Config Menu must already be available for Content Patcher's GMCM integration to be used.

### Conditional GMCM Visibility

T's Core extends Content Patcher's `ConfigSchema` with optional properties for controlling whether individual Config fields are shown in Generic Mod Config Menu based on which mods are currently loaded.

The following properties are supported:

| Property | Behavior |
|----------|----------|
| `TsCore.ShowIfMod` | Shows the field if **at least one** of the specified mods is loaded. |
| `TsCore.ShowIfAllMods` | Shows the field only if **all** specified mods are loaded. |

These properties only control whether the Config field is shown in GMCM.

They do **not** remove the Config field itself. Hidden fields remain in `config.json`, retain their values, and can still be used normally by Content Patcher.

#### TsCore.ShowIfMod

Use `TsCore.ShowIfMod` when a Config field should only be shown if one or more specified mods are loaded.

For example:

```json
"ConfigSchema": {
  "CompatibilityOption": {
    "AllowValues": "true, false",
    "Default": "true",
    "TsCore.ShowIfMod": "Example.AuthorMod"
  }
}
```

`CompatibilityOption` will only be shown in GMCM when `Example.AuthorMod` is loaded.

Multiple mod IDs can be specified as a comma-separated list:

```json
"TsCore.ShowIfMod": "Example.ModA, Example.ModB"
```

Multiple IDs use **OR logic**.

In this example, the field is shown when either `Example.ModA` **or** `Example.ModB` is loaded.

Conceptually:

```text
Example.ModA OR Example.ModB
```

#### TsCore.ShowIfAllMods

Use `TsCore.ShowIfAllMods` when all specified mods must be loaded for the Config field to be shown.

For example:

```json
"ConfigSchema": {
  "CompatibilityOption": {
    "AllowValues": "true, false",
    "Default": "true",
    "TsCore.ShowIfAllMods": "Example.ModA, Example.ModB"
  }
}
```

The field is only shown when both mods are loaded.

Multiple IDs use **AND logic**.

Conceptually:

```text
Example.ModA AND Example.ModB
```

If either mod is missing, the field is hidden from GMCM.

#### Combining Both Conditions

`TsCore.ShowIfMod` and `TsCore.ShowIfAllMods` can be used together on the same ConfigSchema field.

For example:

```json
"ConfigSchema": {
  "CompatibilityOption": {
    "AllowValues": "true, false",
    "Default": "true",
    "TsCore.ShowIfMod": "Example.ModA, Example.ModB",
    "TsCore.ShowIfAllMods": "Example.FrameworkA, Example.FrameworkB"
  }
}
```

When both properties are specified, **both conditions must pass**.

The example above is equivalent to:

```text
(Example.ModA OR Example.ModB)
AND
(Example.FrameworkA AND Example.FrameworkB)
```

#### Reloading Visibility Conditions

The visibility conditions are re-evaluated when using:

```text
tscore_cp_reload <ContentPackId>
```

This allows the properties to be added, removed, or changed during development and tested without restarting the game.

#### Notes

- Mod IDs are checked against mods currently loaded by SMAPI.
- Multiple mod IDs must be separated by commas.
- Whitespace around each mod ID is ignored.
- `TsCore.ShowIfMod` uses OR logic when multiple mod IDs are specified.
- `TsCore.ShowIfAllMods` uses AND logic when multiple mod IDs are specified.
- When both properties are used on the same field, both conditions must pass.
- These properties only affect GMCM visibility.
- Hidden Config fields can still be used by Content Patcher Config Tokens, Dynamic Tokens, and patches.

---

## Dynamic Tokens

T's Core can fully rebuild the Content Pack's `DynamicTokens` from the latest `content.json`.

For example:

```json
"DynamicTokens": [
  {
    "Name": "MyToken",
    "Value": "{{MyConfig}}"
  }
]
```

After editing the Dynamic Token, run:

```text
tscore_cp_reload YourName.MyContentPack
```

T's Core removes the previous Dynamic Token state and rebuilds it using the current definition.

### Supported Changes

The reload process supports changes such as:

- adding or removing a Dynamic Token;
- changing its `Value`;
- changing its `When` conditions;
- changing dependencies between Dynamic Tokens;
- changing references to Config Tokens.

Before registering the current Dynamic Tokens, T's Core clears the previous Dynamic Token state associated with the Content Pack.

The current Dynamic Tokens are then parsed and registered again, and the Token Context is updated.

This allows Dynamic Tokens and their dependencies to be changed during development without restarting the game.

---

## Content Patcher Patches

After the Content Pack data has been rebuilt, T's Core tells Content Patcher to reload the target Content Pack's patches.

For example:

```text
tscore_cp_reload YourName.MyContentPack
```

reloads the patches belonging to:

```text
YourName.MyContentPack
```

after ConfigSchema, Config Tokens, GMCM settings, and Dynamic Tokens have been updated.

This ordering allows the reloaded patches to use the newly rebuilt configuration and token state.

---

## Disabled Patch Records

Content Patcher may permanently disable a patch when it cannot be loaded correctly.

During development, this can happen when a patch temporarily contains invalid data.

Before reloading the target Content Pack, T's Core removes old permanently disabled patch records belonging to that Content Pack.

Records belonging to other Content Packs are not affected.

This allows corrected patches to be evaluated again as part of the reload.

---

## T's Core Custom Data Assets

Several T's Core systems use custom Data Assets which can be edited through normal Content Patcher `EditData` patches.

These include:

```text
TsCore/BuildingProviders
TsCore/WarpProviders
TsCore/NotificationThemes
TsCore/Migrations
TsCore/MachineInteraction
```

When developing a Content Patcher Content Pack which edits these assets, use:

```text
tscore_cp_reload <ContentPackId>
```

after changing the Content Pack.

The target Content Pack's patches are reloaded, allowing changes to these Data Assets to be refreshed without using a separate T's Core resource reload command.

For detailed information about each Data Asset, see its corresponding system guide.

---

## Development Workflow

A typical development workflow can use `tscore_cp_reload` to avoid restarting the game after many common Content Pack changes.

For example:

1. Start Stardew Valley and load your save.
2. Edit your Content Pack files.
3. Save the changes.
4. In the SMAPI console, run:

```text
tscore_cp_reload YourName.MyContentPack
```

5. Return to the game and test the changes.

You can repeat this process while developing the Content Pack.

This is useful when working on:

- Content Patcher patches;
- ConfigSchema;
- Config Tokens;
- GMCM options;
- Dynamic Tokens;
- T's Core custom Data Assets.

---

## Limitations

Content Patcher Integration is intended as a **development convenience**.

It should not be treated as a replacement for restarting the game when testing a Content Pack for release.

Some game state or changes made by a patch may not be completely reversible simply by reloading that patch.

For final testing, restarting Stardew Valley and testing the Content Pack from a clean game session is still recommended.

### Content Patcher Internal Implementation

The extended reload functionality uses Content Patcher's internal runtime implementation in order to rebuild data which isn't normally exposed through its public API.

This includes internal systems related to:

- loaded Content Packs;
- Config handling;
- Config Tokens;
- GMCM registration;
- Dynamic Tokens;
- Token Context;
- patch management.

Because these are internal Content Patcher systems, changes to Content Patcher itself may require corresponding updates to T's Core.

> **Important:** Compatibility with future Content Patcher versions is not guaranteed until the corresponding T's Core version has been tested with them.

### Reload Errors

If part of the reload cannot be completed, T's Core logs the error to the SMAPI console.

For example, a reload may fail if:

- the specified Content Pack ID does not exist;
- the updated `content.json` cannot be loaded;
- a Dynamic Token contains invalid data;
- Content Patcher's internal structure has changed in an incompatible way.

Check the SMAPI console when a reload does not behave as expected.

---

## Notes

Content Patcher Integration is designed primarily to improve the Content Pack development workflow.

For supported development changes, use:

```text
tscore_cp_reload <ContentPackId>
```

after saving the Content Pack files.

The command can reload the target Content Pack's `content.json`, rebuild its ConfigSchema-related state, rebuild Config Tokens, re-register GMCM settings, rebuild Dynamic Tokens and their dependencies, clear old disabled patch records, and reload its Content Patcher patches.

Content Packs which edit T's Core custom Data Assets can use the same `tscore_cp_reload` command. A separate `tscore_reload` command is no longer required.

Although `tscore_cp_reload` can significantly reduce the number of game restarts needed during development, a full restart is still recommended when performing final compatibility and release testing.

---

## Modder Guide

- ← [Notification System](ModderGuide_NotificationSystem.md)
- ↑ [Guide Index](#top)
- → *(End of Guide)*

← [Back to README](../README.md)

← [Back to Modder Guide](ModderGuide.md)
