# 📖 Modder Guide

This guide explains how to use the public features provided by **T's Core** in Content Patcher.

<a id="top"></a>

## Guide Index

- 📄 [Relationship Services](ModderGuide_RelationshipServices.md)
- 📄 [Location Services](ModderGuide_LocationServices.md)
- 📄 [Warp Services](ModderGuide_WarpServices.md)
- 📄 [Building Services](ModderGuide_BuildingServices.md)
- 📄 [Notification System](ModderGuide_NotificationSystem.md)
- ✅ [Content Patcher Integration] *(Current Page)*

← [Back to README](../README.md)

← [Back to Modder Guide](ModderGuide.md)

---

# Content Patcher Integration

Content Patcher Integration provides development tools for reloading Content Patcher Content Packs while Stardew Valley is running.

T's Core can reload a Content Pack's `content.json` together with related ConfigSchema, Config Tokens, GMCM settings, Dynamic Tokens, and patches without restarting the game.

This is primarily intended to make Content Patcher Content Pack development and testing faster.

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
- [T's Core Reload vs Content Patcher Reload](#ts-core-reload-vs-content-patcher-reload)
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

For example, during development you can:

1. add a new ConfigSchema field;
2. save `content.json`;
3. run:

```text
tscore_cp_reload YourName.MyContentPack
```

The updated configuration can then be registered with GMCM using the new Config structure.

The GMCM registration uses the Content Pack's current Config and Content Patcher's normal configuration handling.

> **Note:** Generic Mod Config Menu must already be available for Content Patcher's GMCM integration to be used.

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

The reload process supports common Dynamic Token changes such as:

- adding a Dynamic Token;
- removing a Dynamic Token;
- changing its `Value`;
- changing its `When` conditions;
- changing dependencies between Dynamic Tokens;
- changing references to Config Tokens.

For example:

```json
"DynamicTokens": [
  {
    "Name": "TokenA",
    "Value": "{{MyConfig}}"
  },
  {
    "Name": "TokenB",
    "Value": "{{TokenA}}"
  }
]
```

The dependency between `TokenA` and `TokenB` is rebuilt when the Content Pack is reloaded.

### Dynamic Token State

Before registering the current Dynamic Tokens, T's Core clears the previous Dynamic Token state associated with the Content Pack.

This includes:

- registered Dynamic Tokens;
- Dynamic Token values;
- Dynamic Token dependencies;
- Dynamic Token dependents;
- interdependent Token information.

The current Dynamic Tokens are then parsed and registered again.

Their `When` conditions and `Value` fields are parsed using Content Patcher's normal parsing logic.

Finally, the Token Context is updated.

This means Dynamic Tokens can be removed entirely and later added again without requiring a game restart.

---

## Content Patcher Patches

After the Content Pack data has been rebuilt, T's Core tells Content Patcher to reload the target Content Pack's patches.

This uses Content Patcher's own reload handling for the specified Content Pack.

For example:

```text
tscore_cp_reload YourName.MyContentPack
```

ultimately reloads the patches belonging to:

```text
YourName.MyContentPack
```

after ConfigSchema, Config Tokens, GMCM settings, and Dynamic Tokens have been updated.

This ordering allows the reloaded patches to use the newly rebuilt configuration and token state.

---

## Disabled Patch Records

Content Patcher may permanently disable a patch when it cannot be loaded correctly.

During Content Pack development, this can happen when a patch temporarily contains invalid data.

After the patch is fixed, old disabled-patch state from the previous version may still exist in Content Patcher's current runtime state.

Before reloading the patches, T's Core removes old permanently disabled patch records belonging to the target Content Pack.

Only records associated with the Content Pack specified in:

```text
tscore_cp_reload <ContentPackId>
```

are removed.

Records belonging to other Content Packs are not affected.

This allows corrected patches to be evaluated again as part of the reload.

---

## T's Core Reload vs Content Patcher Reload

T's Core provides two different reload commands for development.

| Command | Purpose |
|---------|-------------|
| `tscore_reload` | Reloads resources managed directly by T's Core. |
| `tscore_cp_reload <ContentPackId>` | Reloads a Content Patcher Content Pack and its supported related state. |

### tscore_reload

Use:

```text
tscore_reload
```

or:

```text
tscore_reload all
```

to reload all supported T's Core resources.

Individual T's Core resource types can also be reloaded.

For example:

```text
tscore_reload warp
```

```text
tscore_reload building
```

```text
tscore_reload notification
```

These commands reload resources provided through T's Core Content Packs, such as:

- Warp Providers;
- Building Providers;
- Notification Themes.

They do not reload Content Patcher patches.

### tscore_cp_reload

Use:

```text
tscore_cp_reload <ContentPackId>
```

when developing a Content Patcher Content Pack.

This command reloads the target Content Patcher Content Pack's supported data, including its patches, ConfigSchema, Config Tokens, GMCM configuration, and Dynamic Tokens.

The two commands serve different purposes and can be used independently.

---

## Development Workflow

A typical Content Patcher development workflow can use `tscore_cp_reload` to avoid restarting the game after many common changes.

For example:

1. Start Stardew Valley and load your save.
2. Edit your Content Pack's `content.json`.
3. Save the file.
4. In the SMAPI console, run:

```text
tscore_cp_reload YourName.MyContentPack
```

5. Return to the game and test the changes.

You can repeat this process while developing the Content Pack.

This is particularly useful when working on:

- patches;
- ConfigSchema;
- Config Tokens;
- GMCM options;
- Dynamic Tokens;
- Dynamic Token conditions;
- interactions between Config and Dynamic Tokens.

### Example

Suppose a Content Pack initially contains:

```json
{
  "Format": "2.9.0",

  "ConfigSchema": {
    "Variant": {
      "AllowValues": "A, B",
      "Default": "A"
    }
  },

  "DynamicTokens": [
    {
      "Name": "SelectedVariant",
      "Value": "{{Variant}}"
    }
  ],

  "Changes": [
    {
      "Action": "EditData",
      "Target": "Data/Objects",
      "When": {
        "SelectedVariant": "A"
      },
      "Entries": {
        "MyExampleEntry": {
          "Name": "Example"
        }
      }
    }
  ]
}
```

During development, you could change the ConfigSchema, Dynamic Token, or patch and then run:

```text
tscore_cp_reload YourName.MyContentPack
```

T's Core will rebuild the supported Content Patcher state before the patches are reloaded.

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

For most development changes, use:

```text
tscore_cp_reload <ContentPackId>
```

after saving the Content Pack files.

The command can reload the target Content Pack's `content.json`, rebuild its ConfigSchema-related state, rebuild Config Tokens, re-register GMCM settings, rebuild Dynamic Tokens and their dependencies, clear old disabled patch records, and reload its Content Patcher patches.

For T's Core Content Pack resources such as Warp Providers, Building Providers, and Notification Themes, use `tscore_reload` instead.

Although `tscore_cp_reload` can significantly reduce the number of game restarts needed during development, a full restart is still recommended when performing final compatibility and release testing.

---

## Modder Guide

- ← [Notification System](ModderGuide_NotificationSystem.md)
- ↑ [Guide Index](#top)
- → *(End of Guide)*

← [Back to README](../README.md)

← [Back to Modder Guide](ModderGuide.md)
