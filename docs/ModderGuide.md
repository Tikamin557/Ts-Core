# T's Core Modder Guide

Welcome to the T's Core developer documentation.

T's Core is a shared library for Stardew Valley SMAPI mods. It provides reusable APIs, Content Patcher tokens, Content Pack features, migration support, development tools, and shared systems to simplify mod development and improve compatibility between mods.

---

## 📚 Table of Contents

- [Getting Started](#-getting-started)
- [Using T's Core from C#](#-using-ts-core-from-c)
- [Available Systems](#-available-systems)
- [Content Packs](#-content-packs)
- [Debug Commands](#-debug-commands)
- [API Stability](#-api-stability)
- [Detailed Documentation](#-detailed-documentation)

← [Back to README](../README.md)

---

# 🚀 Getting Started

## Installing T's Core

Add T's Core as a dependency in your `manifest.json`.

```json
"Dependencies": [
  {
    "UniqueID": "Tikamin557.TsCore",
    "IsRequired": true
  }
]
```

If your mod can run without T's Core, use:

```json
"IsRequired": false
```

instead.

---

# 💻 Using T's Core from C#

T's Core is intended for SMAPI C# mods.

Only use documented **public APIs**.

> **Important**
>
> Do not rely on internal classes, internal fields, or undocumented behavior.
> Internal implementations may change without notice.

---

# 🧩 Available Systems

T's Core currently provides the following systems.

| System | Description |
|---------|-------------|
| Relationship Services | Unified partner information |
| Location Services | Player location information |
| Warp Services | Shared warp actions and reusable Warp Providers |
| Building Services | Building Providers, Building Lights, conditional Draw Layers, and building-related restrictions |
| Migration System | Migration support for IDs stored in existing save data |
| Notification System | Customizable HUD notifications and Notification Themes |
| Content Patcher Integration | Content Patcher development tools and ConfigSchema extensions, including conditional GMCM visibility |
| Content Patcher Tokens | Custom CP tokens |
| Shared Utilities | Common helper functions |

Detailed documentation for each system is available where noted below.

---

# 📦 Content Packs

T's Core supports Content Packs, allowing mods to add custom features without writing any C# code.

Currently, Content Packs can provide:

- Custom Building Providers
- Custom Notification Themes
- Custom Warp Providers
- Migration definitions

Each feature has its own file format and setup requirements.

For detailed instructions, see the corresponding guide:

- **[Building Services Guide](ModderGuide_BuildingServices.md)**
- **[Warp Services Guide](ModderGuide_WarpServices.md)**
- **[Migration System Guide](ModderGuide_MigrationSystem.md)**
- **[Notification System Guide](ModderGuide_NotificationSystem.md)**

T's Core also extends Content Patcher's `ConfigSchema` with optional features for controlling GMCM visibility based on installed mods.

The following properties are available:

- `TsCore.ShowIfMod` — shows a Config field when at least one of the specified mods is loaded.
- `TsCore.ShowIfAllMods` — shows a Config field only when all specified mods are loaded.

These properties can be used by Content Patcher Content Packs without writing C# code.

For detailed usage and examples, see the **[Content Patcher Integration Guide](ModderGuide_ContentPatcherIntegration.md)**.

---

# 🛠 Debug Commands

The following commands are available during development.

## Token Commands

| Command | Description |
|---------|-------------|
| `tscore_tokens` | Prints all available token values |
| `tscore_tokens_relationship` | Prints relationship-related tokens |
| `tscore_tokens_location` | Prints location-related tokens |

## Other Commands

| Command | Description |
|---------|-------------|
| `tscore_debug_warp` | Prints all registered Warp Providers |
| `tscore_debug_buildings` | Prints all registered Building Providers |
| `tscore_debug_buildings <ID>` | Prints detailed information for the specified Building Provider |
| `tscore_debug_farmbuildings` | Prints buildings currently placed on the main farm |
| `tscore_debug_notification_themes` | Prints all registered Notification Themes |
| `tscore_debug_notification` | Displays a test notification |
| `tscore_debug_notification_trigger` | Tests notification Trigger Actions |

## Reload Commands

| Command | Description |
|---------|-------------|
| `tscore_reload` | Reloads all supported T's Core resources |
| `tscore_reload all` | Reloads all supported T's Core resources |
| `tscore_reload warp` | Reloads all registered Warp Providers |
| `tscore_reload building` | Reloads all registered Building Providers |
| `tscore_reload notification` | Reloads all registered Notification Themes |
| `tscore_cp_reload <ContentPackId>` | Reloads the specified Content Patcher Content Pack |

`tscore_reload` commands reload resources managed directly by T's Core.

`tscore_cp_reload` is a separate development command for Content Patcher Content Packs. It reloads the specified Content Pack's patches and refreshes related Content Patcher data, including ConfigSchema, Config Tokens, GMCM settings, DynamicTokens, and T's Core conditional GMCM visibility settings.

For details, see the [Content Patcher Integration](ModderGuide_ContentPatcherIntegration.md) guide.

> **Note:** Migration definitions are loaded when T's Core initializes and Building Migrations are applied when a save is loaded. There is currently no command for reloading or manually applying Migration definitions while the game is running.

---

# 🔒 API Stability

T's Core is under active development.

Public APIs are intended to remain compatible whenever possible, but new functionality may be added over time.

When developing against T's Core:

- Use documented public APIs.
- Avoid internal implementations.
- Keep your required version up to date.
- Test your mod after updating T's Core.

Content Pack file formats and Content Patcher extensions may also gain new optional properties as T's Core is expanded.

Existing properties are intended to remain compatible whenever possible.

---

# 📖 Detailed Documentation

Detailed documentation for each system is available below.

- [Relationship Services](ModderGuide_RelationshipServices.md)
- [Location Services](ModderGuide_LocationServices.md)
- [Warp Services](ModderGuide_WarpServices.md)
- [Building Services](ModderGuide_BuildingServices.md)
- [Migration System](ModderGuide_MigrationSystem.md)
- [Notification System](ModderGuide_NotificationSystem.md)
- [Content Patcher Integration](ModderGuide_ContentPatcherIntegration.md)

← [Back to README](../README.md)
