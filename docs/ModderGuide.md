# T's Core Modder Guide

Welcome to the T's Core developer documentation.

T's Core is a shared library for Stardew Valley SMAPI mods. It provides reusable APIs, Content Patcher tokens, and shared systems to simplify mod development and improve compatibility between mods.

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
| Warp Services | Shared warp utilities |
| Notification System | HUD notifications |
| Content Patcher Tokens | Custom CP tokens |
| Shared Utilities | Common helper functions |

Detailed documentation for each system will be added over time.

---

# 📦 Content Packs

T's Core supports Content Packs, allowing mods to add custom features without writing any C# code.

Currently, Content Packs can provide:

- Custom Notification Themes
- Custom Warp Providers

Each feature has its own file format and setup requirements.

For detailed instructions, see the corresponding guide:

- **Notification Guide (Coming Soon)**
- **[Warp Guide](ModderGuide_WarpServices.md)**

---

# 🛠 Debug Commands

The following commands are available during development.

## Token Commands

| Command | Description |
|---------|-------------|
| `tscore_tokens` | Prints all token values |
| `tscore_tokens_relationship` | Prints relationship tokens |
| `tscore_tokens_location` | Prints location tokens |

## Other Commands

| Command | Description |
|---------|-------------|
| `tscore_debug_warp` | Prints warp providers information |
| `tscore_debug_buildings` | Prints building information |
| `tscore_debug_notification` | Tests notifications |
| `tscore_debug_notification_trigger` | Tests Trigger Actions |

---

# 🔒 API Stability

T's Core is under active development.

Public APIs are intended to remain compatible whenever possible, but new functionality may be added over time.

When developing against T's Core:

- Use documented public APIs.
- Avoid internal implementations.
- Keep your required version up to date.
- Test your mod after updating T's Core.

---

# 📖 Detailed Documentation

Detailed documentation for each system is available below.

- [Relationship Services](ModderGuide_RelationshipServices.md)
- [Location Services](ModderGuide_LocationServices.md)
- [Warp Services](ModderGuide_WarpServices.md)
- [Notification System](ModderGuide_NotificationSystem.md) *(Coming Soon)*
- [Content Patcher Tokens](ModderGuide_ContentPatcherTokens.md) *(Coming Soon)*

← [Back to README](../README.md)
