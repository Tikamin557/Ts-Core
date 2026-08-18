# T's Core Modder Guide

Welcome to the T's Core developer documentation.

T's Core is a shared library for Stardew Valley SMAPI mods. It provides reusable APIs, Content Patcher tokens, Content Pack features, and shared systems to simplify mod development and improve compatibility between mods.

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
| Notification System | Customizable HUD notifications and Notification Themes |
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

Each feature has its own file format and setup requirements.

For detailed instructions, see the corresponding guide:

- **[Building Services Guide](ModderGuide_BuildingServices.md)**
- **[Warp Services Guide](ModderGuide_WarpServices.md)**
- **[Notification System Guide](ModderGuide_NotificationSystem.md)**

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

> **Note:** Reload commands only refresh data managed by T's Core. They do not reload Content Patcher patches or other SMAPI mods.

---

# 🔒 API Stability

T's Core is under active development.

Public APIs are intended to remain compatible whenever possible, but new functionality may be added over time.

When developing against T's Core:

- Use documented public APIs.
- Avoid internal implementations.
- Keep your required version up to date.
- Test your mod after updating T's Core.

Content Pack file formats may also gain new optional properties as T's Core is expanded.

Existing properties are intended to remain compatible whenever possible.

---

# 📖 Detailed Documentation

Detailed documentation for each system is available below.

- [Relationship Services](ModderGuide_RelationshipServices.md)
- [Location Services](ModderGuide_LocationServices.md)
- [Warp Services](ModderGuide_WarpServices.md)
- [Building Services](ModderGuide_BuildingServices.md)
- [Notification System](ModderGuide_NotificationSystem.md)
- [Content Patcher Tokens](ModderGuide_ContentPatcherTokens.md)

← [Back to README](../README.md)
