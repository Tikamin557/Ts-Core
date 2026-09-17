# 📖 Modder Guide

This guide explains how to use the public features provided by **T's Core** in Content Patcher.

<a id="top"></a>

## Guide Index

- ✅ **Relationship Services** *(Current Page)*
- 📄 [Location Services](ModderGuide_LocationServices.md)
- 📄 [Warp Services](ModderGuide_WarpServices.md)
- 📄 [Map Properties](ModderGuide_MapProperties.md)
- 📄 [Building Services](ModderGuide_BuildingServices.md)
- 📄 [BigCraftable Extension](ModderGuide_BigCraftableExtension.md)
- 📄 [Dialogue System](ModderGuide_DialogueSystem.md)
- 📄 [Migration System](ModderGuide_MigrationSystem.md)
- 📄 [Notification System](ModderGuide_NotificationSystem.md)
- 📄 [Content Patcher Integration](ModderGuide_ContentPatcherIntegration.md)

← [Back to README](../README.md)

← [Back to Modder Guide](ModderGuide.md)

---

# Relationship Services

Relationship Services provide **Content Patcher tokens** for accessing the player's current partners.

T's Core automatically handles supported relationship systems, allowing the same Content Pack to work without separate compatibility patches.

---

## Contents

- [Supported Relationship Systems](#supported-relationship-systems)
- [Available Tokens](#available-tokens)
- [Common Examples](#common-examples)
- [Which Token Should I Use?](#which-token-should-i-use)
- [Common Use Cases](#common-use-cases)
- [Debugging](#debugging)
- [Notes](#notes)

---

## Supported Relationship Systems

| System | Supported |
|--------|:---------:|
| Vanilla marriage | ✅ |
| Vanilla roommate (Krobus) | ✅ |
| FreeLove | ✅ |
| PolyamorySweetLove | ✅ |

T's Core automatically detects the active relationship system and provides the same tokens regardless of which system is being used.

As support for additional relationship mods is added to T's Core, existing Content Packs can automatically gain compatibility without requiring changes.

---

## Available Tokens

| Token | Returns | Recommended Use |
|-------|---------|-----------------|
| `{{Tikamin557.TsCore/Partners}}` | Current partners | General relationship checks |
| `{{Tikamin557.TsCore/OrderedPartners}}` | Current partners in spouse room order | Room/order-dependent patches |
| `{{Tikamin557.TsCore/CanBeRomanced:<NPC>}}` | Whether the specified NPC can be romanced | NPC romanceability checks |

`Partners` and `OrderedPartners` return a **list of partner names** and can be used with Content Patcher features such as `Count`, `contains`, and `valueAt`.

`CanBeRomanced` accepts an NPC name as input and returns `true` or `false`.

For example:

| Token | Example Value |
|-------|---------------|
| `Partners` | `Abigail, Emily, Sebastian` |
| `OrderedPartners` | `Sebastian, Abigail, Emily` |
| `CanBeRomanced:Abigail` | `true` |

---

### Partners

Use `Partners` when you only need to know **who the player's current partners are**.

```text
{{Tikamin557.TsCore/Partners}}
```

For most Content Packs, this is the recommended relationship token.

Because the result is a list, you can directly use Content Patcher's list-aware features.

| Purpose | Example |
|---------|---------|
| Count partners | `{{Count:{{Tikamin557.TsCore/Partners}}}}` |
| Check whether any partner exists | `HasValue:{{Tikamin557.TsCore/Partners}}` |
| Check for a specific partner | `Tikamin557.TsCore/Partners \|contains=Abigail` |
| Check for multiple partners | `Count:{{Tikamin557.TsCore/Partners}} >= 2` |

#### Check whether the player has any partner

```json
"When": {
    "HasValue:{{Tikamin557.TsCore/Partners}}": "true"
}
```

`HasValue` checks whether the token currently has any value.

This condition is true when the player has at least one partner, regardless of who the partner is.

#### Check for a specific partner

```json
"When": {
    "Tikamin557.TsCore/Partners |contains=Abigail": "true"
}
```

`contains` checks whether the specified name exists anywhere in the list, so partner order does not matter.

---

### OrderedPartners

Use `OrderedPartners` when the **position of each partner in the spouse room order** is important.

```text
{{Tikamin557.TsCore/OrderedPartners}}
```

It supports the same list operations as `Partners`, but the returned list follows the room order provided by the active spouse room system.

This is useful for:

- Assigning spouse rooms
- Room-specific map patches
- Matching furniture or decorations to spouse room positions
- Creating layouts based on spouse order

#### Check a partner by room position

For example, to check whether **Abigail** is the second partner:

```json
"When": {
    "Tikamin557.TsCore/OrderedPartners |valueAt=1": "Abigail"
}
```

`valueAt` uses zero-based indexing:

| Position | Index |
|----------|------:|
| First partner | `0` |
| Second partner | `1` |
| Third partner | `2` |

---

### CanBeRomanced

Use `CanBeRomanced` to check whether a specific NPC is currently configured as romanceable.

```text
{{Tikamin557.TsCore/CanBeRomanced:<NPC>}}
```

The NPC name is specified as token input.

For example:

```text
{{Tikamin557.TsCore/CanBeRomanced:Abigail}}
```

The token returns:

| Value | Description |
|-------|-------------|
| `true` | The specified NPC is configured as romanceable. |
| `false` | The specified NPC is not configured as romanceable, or the NPC was not found. |

The value is based on the NPC's `CanBeRomanced` setting in Stardew Valley's:

```text
Data/Characters
```

This means the token reflects the current character data, including changes made to `Data/Characters` by Content Patcher.

#### Content Patcher condition

For example:

```json
"When": {
    "Tikamin557.TsCore/CanBeRomanced:Willy": "true"
}
```

This patch is applied when Willy is currently configured as romanceable.

The same Content Pack can therefore respond to changes made by other mods without needing to know which mod changed the NPC's romanceability.

#### Dynamic Token condition

`CanBeRomanced` can also be used in conditions for Content Patcher Dynamic Tokens.

For example:

```json
"DynamicTokens": [
    {
        "Name": "WillyFriendshipRequirement",
        "Value": "2500"
    },
    {
        "Name": "WillyFriendshipRequirement",
        "Value": "2000",
        "When": {
            "Tikamin557.TsCore/CanBeRomanced:Willy": "true"
        }
    }
]
```

In this example, the Dynamic Token can use a different value when Willy is configured as romanceable.

> **Note:** `CanBeRomanced` checks the NPC's current character data. It does not check whether the player is currently dating or married to that NPC.

---

## Common Examples

The same conditions work with vanilla relationships and supported relationship mods.

### Check whether the player has any partner

```json
"When": {
    "HasValue:{{Tikamin557.TsCore/Partners}}": "true"
}
```

### Check for a specific partner

```json
"When": {
    "Tikamin557.TsCore/Partners |contains=Abigail": "true"
}
```

### Check for multiple partners

```json
"When": {
    "Query": "Count:{{Tikamin557.TsCore/Partners}} >= 2"
}
```

No separate compatibility conditions are required for Vanilla, FreeLove, or PolyamorySweetLove.

---

## Which Token Should I Use?

| If you need to... | Use |
|-------------------|-----|
| Check whether the player has a partner | `Partners` |
| Check for a specific partner | `Partners` |
| Count current partners | `Partners` |
| Support multiple spouses | `Partners` |
| Determine spouse room order | `OrderedPartners` |
| Apply patches based on room position | `OrderedPartners` |
| Access a partner by room index | `OrderedPartners` |
| Check whether an NPC is configured as romanceable | `CanBeRomanced:<NPC>` |

> > **Recommendation:** Use `Partners` for general partner checks and `OrderedPartners` only when your Content Pack specifically depends on spouse room order. Use `CanBeRomanced` when you need to check the romanceability setting of a specific NPC.

> **Recommendation:** Use `Partners` unless your Content Pack specifically depends on spouse room order.

---

## Common Use Cases

Relationship Services are useful for Content Packs involving:

- Multi-spouse compatibility
- Custom spouse rooms
- NPC romanceability compatibility
- Marriage events
- Dialogue conditions
- Furniture visibility
- Conditional map edits
- Conditional data patches

---

## Debugging

The following SMAPI commands can be used to inspect token-related information:

| Command | Displays |
|---------|----------|
| `tscore_tokens` | All token-related information provided by T's Core |
| `tscore_tokens_relationship` | Relationship Services information |

Use `tscore_tokens_relationship` when you only need to inspect Relationship Services.

<details>
<summary>Example output</summary>

```text
tscore_tokens_relationship
[T's Core] ===== Relationship =====
[T's Core]
[T's Core]     Provider            : ApiMarriageProvider
[T's Core]     Description         : MarriageMod: ApryllForever.PolyamorySweetLove
[T's Core]     Room Mod            : Polyamory Sweet Rooms
[T's Core]     Partners (3)        : Abigail, Emily, Sebastian
[T's Core]     OrderedPartners (3) : Sebastian, Abigail, Emily
[T's Core]
[T's Core] ----- OrderedPartners Index -----
[T's Core]
[T's Core]     [0] Sebastian
[T's Core]     [1] Abigail
[T's Core]     [2] Emily
```

</details>

The index shown under `OrderedPartners Index` corresponds directly to the `valueAt` index used by the `OrderedPartners` token.

---

### Reloading Content Patcher Content Packs

T's Core also provides development tools for reloading Content Patcher Content Packs while the game is running.

This includes support for reloading patches, ConfigSchema, Config Tokens, GMCM settings, and DynamicTokens without restarting the game.

For details about `tscore_cp_reload` and other Content Patcher integration features, see the [Content Patcher Integration](ModderGuide_ContentPatcherIntegration.md) guide.

---

## Notes

Relationship Services are **read-only**.

They do not modify marriages, friendships, dating relationships, roommate status, or NPC romanceability.

`Partners` and `OrderedPartners` expose the player's existing relationship information.

`CanBeRomanced` reads the specified NPC's current `CanBeRomanced` setting from `Data/Characters`.

Because `Data/Characters` can be modified by Content Patcher, `CanBeRomanced` can be used for compatibility with mods which change whether an NPC is romanceable.

---

## Modder Guide

- ← Previous *(None)*
- ↑ [Guide Index](#top)
- → [Location Services](ModderGuide_LocationServices.md)

← [Back to README](../README.md)

← [Back to Modder Guide](ModderGuide.md)
