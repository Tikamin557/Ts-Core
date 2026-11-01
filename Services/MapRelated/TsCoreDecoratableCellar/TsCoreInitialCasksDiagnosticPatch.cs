using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;

namespace Ts_Core.Services.MapRelated.TsCoreDecoratableCellar
{
    /// <summary>
    /// 新規Cellarに熟成ダル、または熟成ダル入りチェストを初期配置します。
    /// セーブデータ読み込み中はカスタム配置を行いません。
    /// </summary>
    internal static class TsCoreInitialCasksDiagnosticPatch
    {
        private static IMonitor? monitor;

        //----------------------------------------
        // Harmony Registration
        //----------------------------------------
        internal static void Apply(Harmony harmony, IMonitor monitor)
        {
            TsCoreInitialCasksDiagnosticPatch.monitor = monitor;
            var method = AccessTools.Method(typeof(Cellar), "setUpAgingBoards");
            if (method == null)
            {
                monitor.Log("Initial casks diagnostic: target method not found.", LogLevel.Warn);
                return;
            }
            try
            {
                harmony.Patch(method,
                    prefix: new HarmonyMethod(typeof(TsCoreInitialCasksDiagnosticPatch), nameof(Prefix))
);
                monitor.Log("Initial casks custom placement enabled.", LogLevel.Trace);
            }
            catch (Exception ex)
            {
                monitor.Log($"Initial casks diagnostic patch failed: {ex}", LogLevel.Warn);
            }
        }

        //----------------------------------------
        // Initial Placement
        //----------------------------------------
        private static bool Prefix(Cellar __instance)
        {
            // SaveGame.IsProcessing is set during save/load operations.
            // During loading, saved object placement is restored later; avoid
            // constructing temporary custom casks in those Cellar instances.
            if (SaveGame.IsProcessing)
                return true;

            try
            {
                if (__instance.Map?.Properties == null ||
                    !__instance.Map.Properties.TryGetValue("TsCoreInitialCasks", out var property))
                    return true;

                // Only replace the initial vanilla setup when the location is empty.
                // This method is called during construction; never remove existing objects.
                if (__instance.objects.Count() != 0)
                    return true;

                // Parse and validate the entire property before changing the location.
                var entries = new Dictionary<Vector2, (string? ChestId, int Count, bool NoStack)>();
                foreach (string part in (property?.ToString() ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    string[] fields = part.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if ((fields.Length != 2 && fields.Length != 4 && fields.Length != 5) ||
                        !int.TryParse(fields[0], out int x) ||
                        !int.TryParse(fields[1], out int y) ||
                        x < 0 || y < 0 ||
                        x >= __instance.Map.Layers[0].LayerWidth ||
                        y >= __instance.Map.Layers[0].LayerHeight)
                    {
                        monitor?.Log($"Invalid TsCoreInitialCasks entry: '{part}'. Using vanilla placement.", LogLevel.Warn);
                        return true;
                    }

                    string? chestId = null;
                    int count = 1;
                    bool noStack = false;
                    if (fields.Length >= 4)
                    {
                        chestId = fields[2] switch
                        {
                            "Chest" => "130",
                            "StoneChest" => "232",
                            "JunimoChest" => "256",
                            "BigChest" => "BigChest",
                            "BigStoneChest" => "BigStoneChest",
                            _ => null
                        };
                        noStack = fields.Length == 5 && fields[4] == "NoStack";
                        if ((fields.Length == 5 && !noStack) || chestId == null || !int.TryParse(fields[3], out count) || count < 1 || count > 999)
                        {
                            monitor?.Log($"Invalid TsCoreInitialCasks chest or quantity: '{part}'. Using vanilla placement.", LogLevel.Warn);
                            return true;
                        }
                    }

                    // Match the existing behavior: repeated coordinates only place one object.
                    entries.TryAdd(new Vector2(x, y), (chestId, count, noStack));
                }

                // Prepare everything before adding anything to the location.
                var objectsToPlace = new Dictionary<Vector2, StardewValley.Object>();
                foreach (var entry in entries)
                {
                    if (entry.Value.ChestId is string chestId)
                    {
                        var chest = new Chest(true, entry.Key, chestId);
                        if (entry.Value.NoStack)
                        {
                            // Add directly to individual slots: addItem merges identical items.
                            // Respect the chest's actual slot capacity before inserting anything.
                            if (entry.Value.Count > chest.GetActualCapacity())
                            {
                                monitor?.Log($"Initial casks: chest cannot hold {entry.Value.Count} separate casks at {entry.Key}. Using vanilla placement.", LogLevel.Warn);
                                return true;
                            }
                            for (int i = 0; i < entry.Value.Count; i++)
                                chest.Items.Add(ItemRegistry.Create("(BC)163"));
                        }
                        else
                        {
                            for (int i = 0; i < entry.Value.Count; i++)
                            {
                                if (chest.addItem(ItemRegistry.Create("(BC)163")) != null)
                                {
                                    monitor?.Log($"Initial casks: chest cannot hold {entry.Value.Count} casks at {entry.Key}. Using vanilla placement.", LogLevel.Warn);
                                    return true;
                                }
                            }
                        }
                        objectsToPlace.Add(entry.Key, chest);
                    }
                    else
                    {
                        objectsToPlace.Add(entry.Key, new Cask(entry.Key));
                    }
                }

                foreach (var entry in objectsToPlace)
                    __instance.objects.Add(entry.Key, entry.Value);

                monitor?.Log($"Initial casks: placed {objectsToPlace.Count} custom objects.", LogLevel.Trace);
                return false;
            }
            catch (Exception ex)
            {
                monitor?.Log($"Initial casks: custom placement failed: {ex}", LogLevel.Error);
                return true;
            }
        }


    }
}
