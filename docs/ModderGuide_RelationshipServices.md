<a id="top"></a>

# 📖 Modder Guide

This guide explains how to use the public features provided by **T's Core** in Content Patcher.

## Guide Index

- ✅ **Relationship Services** *(Current Page)*
- 📄 [Location Services](ModderGuide_LocationServices.md)
- 📄 [Warp Services](ModderGuide_WarpServices.md) *(Coming Soon)*
- 📄 [Notification System](ModderGuide_NotificationSystem.md) *(Coming Soon)*
- 📄 [Content Patcher Tokens](ModderGuide_ContentPatcherTokens.md) *(Coming Soon)*

← [Back to Guide Index](ModderGuide.md)

---

# Relationship Services

Relationship Services provide **Content Patcher tokens** for working with the player's current partners.

They automatically support both **vanilla Stardew Valley** and supported relationship systems, allowing Content Packs to work without creating separate compatibility patches.

---

## Supported Relationship Systems

Relationship Services currently support the following relationship systems:

- Vanilla marriage
- Vanilla roommate (Krobus)
- FreeLove
- PolyamorySweetLove

T's Core automatically detects which relationship system is currently active and provides a consistent set of Content Patcher tokens.

This means your Content Pack can simply use:

```text
{{Tikamin557.TsCore/Partners}}
{{Tikamin557.TsCore/OrderedPartners}}
```

without creating separate compatibility patches for each supported relationship mod.

As support for additional relationship mods is added to T's Core, existing Content Packs can automatically gain compatibility without requiring any changes.

---

## Available Tokens

### Partners

Returns the names of all current partners.

#### Example

```text
{{Tikamin557.TsCore/Partners}}
```

#### Example Value

```text
Abigail, Emily, Sebastian
```

Because this token returns a **list of partner names**, it works naturally with list-aware Content Patcher features such as:

- `Count`
- `HasValue`

---

#### Count Example

Returns the number of current partners.

```text
{{Count:{{Tikamin557.TsCore/Partners}}}}
```

Result:

```text
3
```

---

#### HasValue Example

Checks whether the player has a specific partner.

```json
"When": {
    "HasValue:{{Tikamin557.TsCore/Partners}}": "Abigail"
}
```

If the token value is:

```text
Abigail, Emily, Sebastian
```

the condition evaluates to:

```text
true
```

`HasValue` checks whether the specified partner exists in the list, so you don't need to worry about the order of the partners.

---

### OrderedPartners

Returns the names of all current partners in **spouse room order**.

Unlike `Partners`, this token follows the room order provided by the currently active spouse room system.

#### Example

```text
{{Tikamin557.TsCore/OrderedPartners}}
```

#### Example Value

```text
Sebastian, Abigail, Emily
```

`OrderedPartners` can be used in the same way as the `Partners` token.

The only difference is that the returned list follows the current spouse room order instead of the partner list order.

---

## When should I use `OrderedPartners`?

Use **`Partners`** when you only need to know who the player's current partners are.

Use **`OrderedPartners`** when the order of the partners is important, such as:

- Assigning spouse rooms
- Applying room-specific map patches
- Matching furniture or decorations to spouse room positions
- Creating hallway or room layouts based on spouse order

For most Content Packs, `Partners` is the recommended token.

Use `OrderedPartners` only when you need to match the spouse room order.

#### Example — Check the second spouse

The following condition evaluates to `true` if **Abigail** is the second partner in the spouse room order.

```json
"When": {
    "Tikamin557.TsCore/OrderedPartners |valueAt=1": "Abigail"
}
```

This is useful when applying patches based on a partner's position in the spouse room order, such as assigning custom spouse rooms or room-specific decorations.

> **Note:** `valueAt` uses zero-based indexing. For example, `valueAt=0` is the first partner, `valueAt=1` is the second, and `valueAt=2` is the third.

---

## Practical Examples

### Example 1 — Check whether the player has a partner

```json
"When": {
    "Query": "Count:{{Tikamin557.TsCore/Partners}} > 0"
}
```

---

### Example 2 — Check for a specific partner

```json
"When": {
    "HasValue:{{Tikamin557.TsCore/Partners}}": "Abigail"
}
```

---

### Example 3 — Polyamory support

```json
"When": {
    "Query": "Count:{{Tikamin557.TsCore/Partners}} >= 2"
}
```

This is useful for applying different patches based on the player's current partner count.

The same Content Pack works for:

- Vanilla marriage
- FreeLove
- PolyamorySweetLove

without any additional compatibility code.

---

## Common Use Cases

Relationship Services are useful for:

- Multi-spouse compatible Content Packs
- Custom spouse rooms
- Marriage events
- Dialogue conditions
- Furniture visibility
- Conditional map edits
- Conditional data patches

---

## Debugging

To inspect the current values provided by the Relationship Services, enter the following command into the SMAPI console:

```text
tscore_tokens_relationship
```

Example output:

```text
tscore_tokens_relationship
[T's Core] ===== Relationship =====
[T's Core] Provider: ApiMarriageProvider (MarriageMod: ApryllForever.PolyamorySweetLove)
[T's Core] Partners (3): Abigail, Emily, Sebastian
[T's Core] RoomMod: Polyamory Sweet Rooms
[T's Core] OrderedPartners (3): Sebastian, Abigail, Emily
[T's Core] OrderedPartners (room index):
[T's Core] [0] Sebastian
[T's Core] [1] Abigail
[T's Core] [2] Emily
```

The room index shown in the debug output corresponds directly to the `valueAt` index used by the `OrderedPartners` token.

---

## Notes

Relationship Services are **read-only**.

They do **not** modify:

- marriages
- friendships
- dating
- roommate status

They simply expose relationship information through Content Patcher tokens.

---

## Modder Guide

- ← Previous *(None)*
- ↑ [Guide Index](#top)
- → [Location Services](ModderGuide_LocationServices.md)

← [Back to README](../README.md)

← [Back to Guide Index](ModderGuide.md)
