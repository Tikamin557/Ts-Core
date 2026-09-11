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
- ✅ **Migration System** *(Current Page)*
- 📄 [Notification System](ModderGuide_NotificationSystem.md)
- 📄 [Content Patcher Integration](ModderGuide_ContentPatcherIntegration.md)

← [Back to README](../README.md)

← [Back to Modder Guide](ModderGuide.md)

---

# Migration System

The Migration System allows Content Patcher Content Packs to migrate IDs used by existing save data when a mod changes its internal IDs.

This is useful when an update changes an ID that may already be stored in a player's save.

Migration definitions are registered through the custom T's Core Data Asset:

```text
TsCore/Migrations
