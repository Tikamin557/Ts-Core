# T's Core Modder Guide

Welcome to the T's Core developer documentation.

T's Core is a shared library and framework for Stardew Valley SMAPI mods. It provides reusable APIs, Content Patcher integration, custom actions, data-driven systems, migration support, and development tools to simplify mod development and improve compatibility between mods.

---

## 📚 Table of Contents

- [Getting Started](#-getting-started)
- [Using T's Core from C#](#-using-ts-core-from-c)
- [Available Systems](#-available-systems)
- [Using T's Core with Content Patcher](#-using-ts-core-with-content-patcher)
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

T's Core can be used by SMAPI C# mods through its documented public APIs.

> **Important**
>
> Do not rely on internal classes, internal fields, or undocumented behavior.
> Internal implementations may change without notice.

---

# 🧩 Available Systems

T's Core currently provides the following systems.

| System | Description |
|---------|-------------|
| Relationship Services | Provides relationship and partner information, including support for compatible marriage mods. |
| Location Services | Provides player location information and related features. |
| Warp Services | Provides Warp Actions and reusable Warp Providers. |
| Map Properties | Adds custom map properties for additional location behavior. |
| Building Services | Adds custom settings and visual features to buildings. |
| Machine Interaction | Adds configurable interactions and visual behavior to machines. |
| Migration System | Provides migration support for IDs stored in existing save data. |
| Notification System | Provides customizable on-screen notifications and Notification Themes. |
| Content Patcher Integration | Provides additional Content Patcher development and configuration features. |
| Content Patcher Tokens | Provides custom tokens for use with Content Patcher. |
| Shared Utilities | Provides common functionality shared between T's Mods. |

See the [Detailed Documentation](#-detailed-documentation) section for individual guides.

---

# 📦 Using T's Core with Content Patcher

Many T's Core features can be used directly from Content Patcher without writing C# code.

Content Patcher Content Packs can use T's Core custom actions, map properties, integrations, and custom Data Assets.

Custom definitions can be registered for:

- Building Providers
- Warp Providers
- Notification Themes
- Migration definitions
- Machine Interactions

T's Core also provides additional Content Patcher features such as custom tokens, Content Pack reload tools, and conditional GMCM field visibility.

Each system has its own setup and available options. See the corresponding guide in the [Detailed Documentation](#-detailed-documentation) section for details and examples.

---

# 🛠 Debug Commands

T's Core provides several commands for inspecting and testing its systems during development.

## Token Commands

| Command | Description |
|---------|-------------|
| `tscore_tokens` | Prints all available token values. |
| `tscore_tokens_relationship` | Prints relationship-related tokens. |
| `tscore_tokens_location` | Prints location-related tokens. |

## Debug Commands

| Command | Description |
|---------|-------------|
| `tscore_debug_warp` | Prints all registered Warp Providers. |
| `tscore_debug_buildings` | Prints all registered Building Providers. |
| `tscore_debug_buildings <ID>` | Prints detailed information for the specified Building Provider. |
| `tscore_debug_farmbuildings` | Prints buildings currently placed on the main farm. |
| `tscore_debug_notification_themes` | Prints all available Notification Themes. |
| `tscore_debug_notification` | Displays a test notification. |
| `tscore_debug_notification_trigger` | Tests notification Trigger Actions. |

## Content Patcher Reload

```text
tscore_cp_reload <ContentPackId>
```

This development command reloads the specified Content Patcher Content Pack and refreshes supported Content Patcher data without restarting the game.

This includes support for changes to T's Core custom Data Assets, ConfigSchema, Config Tokens, GMCM settings, and DynamicTokens.

For details, see the **[Content Patcher Integration Guide](ModderGuide_ContentPatcherIntegration.md)**.

---

# 🔒 API Stability

T's Core is under active development.

Public APIs and documented features are intended to remain compatible whenever possible, but new functionality and optional properties may be added over time.

When developing against T's Core:

- Use documented public APIs.
- Avoid relying on internal implementations.
- Keep your required T's Core version up to date.
- Test your mod after updating T's Core.

---

# 📖 Detailed Documentation

For detailed setup, properties, examples, and usage instructions, see the individual guides:

- [Relationship Services](ModderGuide_RelationshipServices.md)
- [Location Services](ModderGuide_LocationServices.md)
- [Warp Services](ModderGuide_WarpServices.md)
- [Map Properties](ModderGuide_MapProperties.md)
- [Building Services](ModderGuide_BuildingServices.md)
- [Machine Interaction](ModderGuide_MachineInteraction.md)
- [Migration System](ModderGuide_MigrationSystem.md)
- [Notification System](ModderGuide_NotificationSystem.md)
- [Content Patcher Integration](ModderGuide_ContentPatcherIntegration.md)

← [Back to README](../README.md)
