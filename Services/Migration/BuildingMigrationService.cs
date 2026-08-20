using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.GameData.Buildings;
using Ts_Core.Models;

namespace Ts_Core.Services.Migration
{
    /// <summary>
    /// Building Type IDのMigrationを処理します。
    /// </summary>
    internal static class BuildingMigrationService
    {
        //----------------------------------------
        // Building Migration
        //----------------------------------------

        /// <summary>
        /// 設置済みBuildingのBuilding Type IDを
        /// Migration定義に従って更新します。
        /// </summary>
        internal static void ApplyMigrations(
            IMonitor monitor)
        {
            if (!Context.IsWorldReady)
                return;

            IReadOnlyList<MigrationModel> migrations =
                MigrationService.GetBuildingMigrations();

            if (migrations.Count == 0)
                return;

            //----------------------------------------
            // 現在のData/Buildings
            //----------------------------------------

            Dictionary<string, BuildingData> buildingData =
                DataLoader.Buildings(
                    Game1.content);

            int migratedCount = 0;

            //----------------------------------------
            // Location一覧
            //----------------------------------------

            foreach (GameLocation location in Game1.locations)
            {
                if (location.buildings == null)
                    continue;

                //----------------------------------------
                // Building一覧
                //----------------------------------------

                foreach (Building building in location.buildings)
                {
                    string currentId =
                        building.buildingType.Value;

                    MigrationModel? migration =
                        migrations.FirstOrDefault(
                            entry =>
                                string.Equals(
                                    entry.OldId,
                                    currentId,
                                    StringComparison.Ordinal));

                    if (migration == null)
                        continue;

                    //----------------------------------------
                    // 新ID存在確認
                    //----------------------------------------

                    if (!buildingData.ContainsKey(
                            migration.NewId))
                    {
                        monitor.Log(
                            $"Building Migration target '{migration.NewId}' was not found in Data/Buildings. " +
                            $"Building '{currentId}' at {location.NameOrUniqueName} " +
                            $"({building.tileX.Value}, {building.tileY.Value}) was not migrated.",
                            LogLevel.Warn);

                        continue;
                    }

                    //----------------------------------------
                    // Building Type更新
                    //----------------------------------------

                    building.buildingType.Value =
                        migration.NewId;

                    migratedCount++;

                    monitor.Log(
                        $"Migrated Building Type '{currentId}' -> '{migration.NewId}' " +
                        $"at {location.NameOrUniqueName} " +
                        $"({building.tileX.Value}, {building.tileY.Value}).",
                        LogLevel.Info);
                }
            }

            //----------------------------------------
            // 結果
            //----------------------------------------

            if (migratedCount > 0)
            {
                monitor.Log(
                    $"Building Migration completed. Migrated Buildings: {migratedCount}",
                    LogLevel.Info);
            }
        }
    }
}