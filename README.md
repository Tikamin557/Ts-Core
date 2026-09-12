# T's Core

> **This mod is primarily intended for other mods.**
> Most players only need to install it when it is required by another mod.

A shared library and framework for Stardew Valley SMAPI mods.

It provides reusable APIs, Content Patcher integration, custom actions, data-driven systems, migration support, and development tools that other mods can use to improve compatibility and reduce duplicated code.

---

> [!IMPORTANT]
> **🚧 Work in Progress**
>
> The documentation and guides are currently being written and expanded. Additional examples, API references, and tutorials will be added over time.

---

## ✨ Features

- Relationship services and support for third-party marriage APIs
- Location tracking and location-related services
- Warp Actions and reusable Warp Providers
- Custom Map Properties for additional location behavior
  - Timed exits with optional dialogue and sound
- Building Services for adding custom settings and visual features to buildings
- Machine Interaction for adding custom right-click behavior to machines
- Migration System for updating IDs stored in existing save data
- Customizable Notification System and Notification Themes
- Content Patcher tokens and integration
  - Extended Content Pack reload tools for development
  - Conditional GMCM field visibility based on installed mods
- Custom Data Assets for registering definitions through Content Patcher
- Common utilities shared between T's Mods

---

## 📦 Installation

1. Install [SMAPI](https://www.nexusmods.com/stardewvalley/mods/2400).
2. Install [Content Patcher](https://www.nexusmods.com/stardewvalley/mods/1915).
3. Download the latest version of T's Core.
4. Extract it into your `Mods` folder.

> **Players:** You only need to install T's Core if another mod lists it as a requirement.

---

## 🛠 For Mod Authors

If you want to integrate your mod with T's Core, please see the **[Modder Guide](docs/ModderGuide.md)**.

T's Core can be used from C# mods and Content Patcher mods depending on the feature. Many features can be configured directly through Content Patcher without writing C# code.

The Modder Guide covers:

- Relationship Services
- Location Services
- Warp Services and custom Warp Providers
- Map Properties
- Building Services and custom Building Providers
- Machine Interaction
- Migration System and custom migration definitions
- Notification System and custom Notification Themes
- Content Patcher Integration and Tokens
- Debug and reload commands

Custom Warp Providers, Building Providers, Notification Themes, and migration definitions can be registered through T's Core custom Data Assets using Content Patcher.

Custom Map Properties can also add location-specific behavior without C# code, including Timed Exit properties which can automatically warp the player out of a location at or after a specified in-game time, with optional dialogue and sound.

See the **[Map Properties Guide](docs/ModderGuide_MapProperties.md)** for details.

Additional Content Patcher integration features include extended Content Pack reloading for development and conditional GMCM field visibility based on installed mods.

See the **[Content Patcher Integration Guide](docs/ModderGuide_ContentPatcherIntegration.md)** for details.

---

## 💬 Feedback & Bug Reports

Suggestions, feedback, and feature requests can be posted on the **[Nexus Mods Posts page](https://www.nexusmods.com/stardewvalley/mods/50043?tab=posts)**.

Bug reports can be submitted on the **[Nexus Mods Bugs page](https://www.nexusmods.com/stardewvalley/mods/50043?tab=bugs)**.

When reporting a bug, please include:

- SMAPI log
- T's Core version
- Stardew Valley version
- Steps to reproduce the issue

---

## 🔗 Links

- [Nexus Mods - T's Core](https://www.nexusmods.com/stardewvalley/mods/50043)
- [All Mods by Tikamin557](https://www.nexusmods.com/profile/tikamin557/mods)
