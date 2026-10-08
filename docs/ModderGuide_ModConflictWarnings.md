# Mod Conflict Warnings

T's Core v1.9.3 adds a data-driven way for Content Patcher packs to warn players when incompatible mod versions are installed together. It **only displays warnings**; it does not disable mods or prevent the game from loading.

← [Modder Guide](ModderGuide.md) | [README](../README.md)

## Data Asset

Register definitions with Content Patcher's `EditData` action, targeting `TsCore/ModConflictWarnings`. Each `Entries` key identifies one conflict definition. Choose a unique key (for example, prefix it with `{{ModId}}`).

```json
{
    "Action": "EditData",
    "Target": "TsCore/ModConflictWarnings",
    "Entries": {
        "{{ModId}}_CP_PIF": {
            "ModIds": [
                "Tikamin557.CP.FarmhouseAnnex",
                "Tikamin557.CP.FarmhouseAnnex_PIF"
            ]
        }
    }
}
```

The example warns if both versions of T's Farmhouse Annex are loaded. You can register the definition in just one Content Pack; it does not need to appear in both versions.

## Properties

| Property | Required | Description |
|---|---|---|
| `ModIds` | Yes | Array of mod UniqueIDs. A warning is triggered when **at least two distinct IDs** in this list are loaded. Matching is case-insensitive. |
| `Message` | No | Custom text for the title-screen warning. If omitted or empty, T's Core generates a localized default message listing the detected mod names. |

You may specify more than two IDs. This is a **group rule**: any two or more installed mods from the group trigger a warning; it is not a requirement that all listed mods be installed.

## Custom Message and Translations

```json
{
    "Action": "EditData",
    "Target": "TsCore/ModConflictWarnings",
    "Entries": {
        "{{ModId}}_CP_PIF": {
            "ModIds": [
                "Tikamin557.CP.FarmhouseAnnex",
                "Tikamin557.CP.FarmhouseAnnex_PIF"
            ],
            "Message": "{{i18n:modConflict.warning}}"
        }
    }
}
```

Define `modConflict.warning` in the **Content Pack's** `i18n/default.json` and `i18n/ja.json` (or other supported language files). Content Patcher resolves the i18n token before T's Core reads the data asset. A message may contain JSON-escaped `\n` for a new line or `\n\n` for a blank line.

For example, an entry in `i18n/default.json`:

```json
{
    "modConflict.warning": "Both versions are installed.\nPlease keep only one version."
}
```

`Message` replaces the default **title-screen message** for that definition; it does not replace the SMAPI console warning format.

## When and How Warnings Appear

- T's Core checks the registered definitions once, shortly after the initial title screen appears (after a short delay to allow startup and language initialization).
- Each detected conflict is logged at SMAPI's `Warn` level, including the definition key, installed mod names, and UniqueIDs.
- If any conflicts are found, one dismissible warning window appears on the title screen. Multiple messages are combined in that window, separated by blank lines.
- The window supports mouse-wheel scrolling and a draggable scrollbar when the text is too long. Close it with **OK**.
- The warning is based on the language available at startup; closing it and then changing the title-screen language does not display it again during that launch.

## Notes

- Use installed mod **UniqueIDs**, not display names, in `ModIds`.
- This feature checks whether mods are **loaded**, not whether their individual Content Patcher patches are active or have been skipped.
- Avoid defining the same conflict group repeatedly in multiple packs, as each matching definition generates its own warning entry.
- Since the check happens once at startup, editing definitions during the same game session does not automatically re-display the warning.
- This is a warning system only; it does not resolve compatibility problems automatically.
