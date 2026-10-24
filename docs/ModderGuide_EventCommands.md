# Event Commands — `tsCoreChangeMapTile` (v2.0.0)

← [Back to Modder Guide](ModderGuide.md)

T's Core adds `tsCoreChangeMapTile` for Stardew Valley events. Like the vanilla `changeMapTile` command, it changes a map tile during an event, but it also supports tiles that were originally **animated**.

## Syntax

```text
tsCoreChangeMapTile <Layer> <X> <Y> <TileIndex>
```

Example:

```text
tsCoreChangeMapTile Buildings7 5 7 41
```

- `Layer`: an existing map layer (for example, `Buildings7`).
- `X`, `Y`: tile coordinates; the event's tile offsets are applied.
- `TileIndex`: the replacement tile index **in the original tile's tilesheet**.

## Behavior

1. The command saves the original tile object and replaces it with a **static tile** using the specified index.
2. The replacement keeps the original tile's tilesheet, blend mode, and tile properties.
3. **When the event ends, the original tile object is automatically restored**, including its animation if it was an `AnimatedTile`. Restoration also runs after event skipping or leaving the location, once the event is no longer active.
4. If the same event changes the same tile multiple times, T's Core retains the original tile from before the first change for restoration.

This is a temporary visual/map change **during an event**, not a persistent map edit. The target tile must already exist; the command does not create tiles in empty positions. If the layer, coordinates, or arguments are invalid, T's Core logs a warning.

> **Tip:** Use this command instead of vanilla `changeMapTile` when an event needs to temporarily replace an animated map tile. The original animation resumes after restoration.
