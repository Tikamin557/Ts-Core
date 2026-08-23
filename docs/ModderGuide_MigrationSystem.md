# 📖 Modder Guide

This guide explains how to use the public features provided by **T's Core** in Content Patcher.

<a id="top"></a>

## Guide Index

- 📄 [Relationship Services](ModderGuide_RelationshipServices.md)
- 📄 [Location Services](ModderGuide_LocationServices.md)
- 📄 [Warp Services](ModderGuide_WarpServices.md)
- 📄 [Building Services](ModderGuide_BuildingServices.md)
- ✅ **Migration System** *(Current Page)*
- 📄 [Notification System](ModderGuide_NotificationSystem.md)
- 📄 [Content Patcher Integration](ModderGuide_ContentPatcherIntegration.md)

← [Back to README](../README.md)

← [Back to Modder Guide](ModderGuide.md)

---

# Migration System

The Migration System allows T's Core Content Packs to migrate IDs used by existing save data when a mod changes its internal IDs.

This is useful when an update changes an ID that may already be stored in a player's save.

Currently, the Migration System supports migrating **Building Type IDs** for buildings that have already been constructed.

---

## Contents

- [Content Pack Setup](#content-pack-setup)
- [Migration Definitions](#migration-definitions)
- [Migration Properties](#migration-properties)
- [Building Migration](#building-migration)
- [When Migrations Are Applied](#when-migrations-are-applied)
- [Multiple Migrations](#multiple-migrations)
- [Duplicate Migrations](#duplicate-migrations)
- [Examples](#examples)
- [Notes](#notes)

---

## Content Pack Setup

T's Core Content Packs can register Migration definitions.

A single Content Pack can include multiple T's Core features, such as Migration definitions, Building Providers, Warp Providers, and Notification Themes.

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

Migration JSON files must be placed inside:

```text
assets/migration
```

Example:

```text
[TsC] My T's Core Content Pack
├── manifest.json
└── assets
    └── migration
        └── MyMigrations.json
```

Subfolders are supported, and JSON filenames may be chosen freely.

For example:

```text
assets/migration/buildings/MyBuildingMigrations.json
```

is also valid.

> **Note:** The folder names (`assets` and `migration`) are fixed and must not be renamed. T's Core searches this folder when loading Migration definitions.

---

## Migration Definitions

Each JSON file inside `assets/migration` contains an array of Migration definitions.

Basic example:

```json
[
  {
    "Type": "Building",
    "OldId": "YourModId_OldBuilding",
    "NewId": "YourModId_NewBuilding"
  }
]
```

This tells T's Core that existing save data using:

```text
YourModId_OldBuilding
```

should be migrated to:

```text
YourModId_NewBuilding
```

when the corresponding Migration type is processed.

Currently, the only supported Migration type is:

```text
Building
```

Additional Migration types may be added in future versions of T's Core.

---

## Migration Properties

Each Migration definition supports the following properties:

| Property | Required | Description |
|----------|----------|-------------|
| `Type` | ✅ | Type of Migration. Currently only `Building` is supported. |
| `OldId` | ✅ | Old ID stored in existing save data. |
| `NewId` | ✅ | New ID that should replace the old ID. |

All three properties are required.

For example:

```json
[
  {
    "Type": "Building",
    "OldId": "YourModId_OldBarn",
    "NewId": "YourModId_NewBarn"
  }
]
```

`OldId` and `NewId` must be different.

A Migration with the same value for both IDs is invalid and will not be registered.

> **Note:** IDs should be written using their exact internal values. Building ID matching uses the exact `BuildingType` ID stored by the building.

---

## Building Migration

Building Migration updates the Building Type ID of buildings that have already been constructed and stored in the player's save.

This is useful when a mod changes a building ID in `Data/Buildings`.

For example, an older version of a mod may have registered:

```text
YourModId_OldBuilding
```

but a newer version may use:

```text
YourModId_NewBuilding
```

Without migration, buildings already constructed in an existing save may still reference the old ID.

A Building Migration can update those buildings automatically:

```json
[
  {
    "Type": "Building",
    "OldId": "YourModId_OldBuilding",
    "NewId": "YourModId_NewBuilding"
  }
]
```

When the save is loaded, T's Core searches placed buildings for:

```text
YourModId_OldBuilding
```

and changes the stored Building Type ID to:

```text
YourModId_NewBuilding
```

### Data/Buildings Validation

Before changing a Building Type ID, T's Core verifies that `NewId` currently exists in:

```text
Data/Buildings
```

For example:

```json
[
  {
    "Type": "Building",
    "OldId": "YourModId_OldBuilding",
    "NewId": "YourModId_NewBuilding"
  }
]
```

will only be applied if:

```text
YourModId_NewBuilding
```

exists in the current `Data/Buildings` asset.

If the target ID does not exist, the building is not migrated and T's Core logs a warning.

This prevents an existing building from being migrated to a Building Type that is not currently available.

### Existing Buildings

Building Migration changes the stored Building Type ID of the existing building.

It does not create a replacement building or move the existing building.

The building keeps its existing placement while its Building Type ID is changed from `OldId` to `NewId`.

### Locations

T's Core checks buildings in the loaded game locations.

This is not limited to buildings placed on the main farm.

If a location contains buildings and one of those buildings uses a matching `OldId`, the Migration can be applied there as well.

---

## When Migrations Are Applied

Migration definitions are loaded when T's Core initializes.

Building Migrations are applied when a save is loaded.

The general process is:

```text
T's Core starts
↓
Migration definitions are loaded from assets/migration
↓
A save is loaded
↓
T's Core checks placed buildings
↓
A building matches OldId
↓
T's Core verifies that NewId exists in Data/Buildings
↓
The Building Type ID is changed to NewId
```

Once a building has been migrated, its Building Type ID no longer matches `OldId`.

The updated Building Type is then stored normally when the game saves.

> **Note:** Migration definitions are intended to handle existing save data when loading a save. T's Core does not currently provide a console command for reloading or manually applying Migration definitions while the game is running.

---

## Multiple Migrations

A single Migration JSON file can contain multiple definitions.

For example:

```json
[
  {
    "Type": "Building",
    "OldId": "YourModId_OldBarn",
    "NewId": "YourModId_NewBarn"
  },
  {
    "Type": "Building",
    "OldId": "YourModId_OldCoop",
    "NewId": "YourModId_NewCoop"
  },
  {
    "Type": "Building",
    "OldId": "YourModId_OldShed",
    "NewId": "YourModId_NewShed"
  }
]
```

You may also split Migration definitions across multiple JSON files.

For example:

```text
assets
└── migration
    ├── BarnMigrations.json
    ├── CoopMigrations.json
    └── OldVersions
        └── LegacyBuildingMigrations.json
```

T's Core searches `assets/migration` and its subfolders for Migration JSON files.

---

## Duplicate Migrations

Only one Migration can be registered for the same combination of:

```text
Type + OldId
```

For example, these two definitions conflict:

```json
[
  {
    "Type": "Building",
    "OldId": "YourModId_OldBuilding",
    "NewId": "YourModId_NewBuildingA"
  },
  {
    "Type": "Building",
    "OldId": "YourModId_OldBuilding",
    "NewId": "YourModId_NewBuildingB"
  }
]
```

Both definitions attempt to migrate the same `Building` ID:

```text
YourModId_OldBuilding
```

The first registered Migration is kept, and the duplicate Migration is ignored.

T's Core logs a warning containing information about both definitions when a duplicate is detected.

> **Recommendation:** Use only one destination for each old ID. Avoid defining multiple possible `NewId` values for the same `Type` and `OldId`.

---

## Examples

### Rename a Building Type

Suppose an older version of your mod used:

```text
YourModId_MyBarn
```

and the new version changes the ID in `Data/Buildings` to:

```text
YourModId_LargeBarn
```

Create a Migration definition:

```json
[
  {
    "Type": "Building",
    "OldId": "YourModId_MyBarn",
    "NewId": "YourModId_LargeBarn"
  }
]
```

Players who already constructed `YourModId_MyBarn` can then have the stored Building Type updated when their save is loaded.

---

### Migrate Several Old Building IDs

If an update renames several buildings:

```json
[
  {
    "Type": "Building",
    "OldId": "YourModId_OldBarn",
    "NewId": "YourModId_Barn"
  },
  {
    "Type": "Building",
    "OldId": "YourModId_OldCoop",
    "NewId": "YourModId_Coop"
  },
  {
    "Type": "Building",
    "OldId": "YourModId_OldShed",
    "NewId": "YourModId_Shed"
  }
]
```

Each placed building is checked independently when the save is loaded.

Only buildings whose current Building Type matches one of the `OldId` values are changed.

---

### Preserve Compatibility with Older Versions

Migration definitions can remain in the Content Pack after the update that introduced the new ID.

For example:

```json
[
  {
    "Type": "Building",
    "OldId": "YourModId_Barn_V1",
    "NewId": "YourModId_Barn"
  },
  {
    "Type": "Building",
    "OldId": "YourModId_Barn_V2",
    "NewId": "YourModId_Barn"
  }
]
```

This allows buildings stored using either older ID to migrate to the current ID.

Once an existing building has already been migrated, these definitions no longer match that building because it now uses the new ID.

---

## Notes

The Migration System is designed for updating IDs already stored in existing save data when a mod changes its internal IDs.

Currently, only Building Type migration is supported.

Building Migrations:

- apply to buildings that have already been constructed;
- run when a save is loaded;
- change the building's stored Building Type ID;
- verify that the target `NewId` exists in `Data/Buildings` before applying the change;
- can apply to buildings in locations other than the main farm;
- do not move or recreate the building.

Migration definitions should generally remain available for as long as your mod intends to support saves created with the old IDs.

Use exact internal IDs for `OldId` and `NewId`, and avoid defining more than one Migration for the same `Type` and `OldId`.

Additional Migration types may be added in future versions of T's Core.

---

## Modder Guide

- ← [Building Services](ModderGuide_BuildingServices.md)
- ↑ [Guide Index](#top)
- → [Notification System](ModderGuide_NotificationSystem.md)

← [Back to README](../README.md)

← [Back to Modder Guide](ModderGuide.md)
