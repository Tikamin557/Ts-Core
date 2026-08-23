# 📖 Modder Guide

This guide explains how to use the public features provided by **T's Core** in Content Patcher.

<a id="top"></a>

## Guide Index

- ✅ **Relationship Services** *(Current Page)*
- 📄 [Location Services](ModderGuide_LocationServices.md)
- 📄 [Warp Services](ModderGuide_WarpServices.md)
- 📄 [Building Services](ModderGuide_BuildingServices.md)
- 📄 [Notification System](ModderGuide_NotificationSystem.md)
- 📄 [Content Patcher Tokens](ModderGuide_ContentPatcherTokens.md) *(Coming Soon)*

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

Both tokens return a **list of partner names** and can be used with Content Patcher features such as `Count`, `HasValue`, and `valueAt`.

For example:

| Token | Example Value |
|-------|---------------|
| `Partners` | `Abigail, Emily, Sebastian` |
| `OrderedPartners` | `Sebastian, Abigail, Emily` |

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
| Check for a partner | `HasValue:{{Tikamin557.TsCore/Partners}}` |
| Check whether any partner exists | `Count:{{Tikamin557.TsCore/Partners}} > 0` |
| Check for multiple partners | `Count:{{Tikamin557.TsCore/Partners}} >= 2` |

#### Check for a specific partner

```json
"When": {
    "HasValue:{{Tikamin557.TsCore/Partners}}": "Abigail"
}
```

`HasValue` checks whether the specified name exists anywhere in the list, so partner order does not matter.

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

## Common Examples

The same conditions work with vanilla relationships and supported relationship mods.

### Check whether the player has any partner

```json
"When": {
    "Query": "Count:{{Tikamin557.TsCore/Partners}} > 0"
}
```

### Check for a specific partner

```json
"When": {
    "HasValue:{{Tikamin557.TsCore/Partners}}": "Abigail"
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

> **Recommendation:** Use `Partners` unless your Content Pack specifically depends on spouse room order.

---

## Common Use Cases

Relationship Services are useful for Content Packs involving:

- Multi-spouse compatibility
- Custom spouse rooms
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

They do not modify marriages, friendships, dating relationships, or roommate status. They only expose existing relationship information through Content Patcher tokens.

---

## Modder Guide

- ← Previous *(None)*
- ↑ [Guide Index](#top)
- → [Location Services](ModderGuide_LocationServices.md)

← [Back to README](../README.md)

← [Back to Modder Guide](ModderGuide.md)
