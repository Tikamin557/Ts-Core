# Shortcut Panel

T's Core provides an optional in-game Shortcut Panel for players and a shortcut registration API for C# mod authors.

## Position and controls (v1.9.2)

Open T's Core's Generic Mod Config Menu settings and set **Shortcut Panel Position** to **Free**. The default **Fixed** mode keeps the panel in its original position beside the money UI. In Free mode, drag the move handle above the panel's open/close tab to reposition it. The offset is saved and reused after restarting the game. The panel is kept within the visible screen area.

The open/close tab and panel have separate size settings. Free positioning works with these display scales.

## CJB Cheats Menu warp shortcuts (v1.9.2)

When **CJB Cheats Menu** is installed, open an empty Shortcut Panel slot, choose **Mod Features**, then select **CJB Cheats Menu** and a warp destination from its grouped list. The saved shortcut opens the selected destination. Available destinations follow the warp list provided by CJB Cheats Menu, including its configuration and content edits.

This integration depends on CJB Cheats Menu's internal warp implementation rather than a public API, so future CJB updates may affect compatibility. If warp names appear incorrectly immediately after changing the game language, fully restarting the game may be necessary; the same issue can affect CJB's own warp page.

## For C# mod authors

C# mods can register custom actions for the Shortcut Panel using T's Core's public `ITsCoreApi` shortcut registration API. See the API interface and existing Modder Guide for the registration contract.
