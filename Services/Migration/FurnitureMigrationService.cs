using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using Ts_Core.Models.Migration;

namespace Ts_Core.Services.Migration
{
    /// <summary>
    /// セーブ済みの家具を、移行先の家具定義に従って安全に置き換えます。
    /// 設置済み家具と所持品・収納内の家具を対象にします。
    /// </summary>
    internal static class FurnitureMigrationService
    {
        //----------------------------------------
        // 家具の移行
        //----------------------------------------

        /// <summary>
        /// セーブ読込時に、設置済み家具とアイテム一覧内の家具を移行します。
        /// </summary>
        internal static void ApplyMigrations(IMonitor monitor)
        {
            if (!Context.IsWorldReady || !Context.IsMainPlayer)
                return;

            IReadOnlyList<MigrationModel> migrations = MigrationService.GetFurnitureMigrations(monitor);
            if (migrations.Count == 0)
                return;

            Dictionary<string, MigrationModel> byOldId = migrations.ToDictionary(m => m.OldId, StringComparer.Ordinal);
            HashSet<string> furnitureIds = DataLoader.Furniture(Game1.content).Keys.ToHashSet(StringComparer.Ordinal);
            int migrated = 0;

            foreach (GameLocation location in Game1.locations)
            {
                for (int i = 0; i < location.furniture.Count; i++)
                {
                    Furniture oldFurniture = location.furniture[i];
                    if (TryMigrate(oldFurniture, byOldId, furnitureIds, monitor, out Furniture? replacement))
                    {
                        location.furniture[i] = replacement!;
                        migrated++;
                        monitor.Log($"Migrated Furniture '{oldFurniture.ItemId}' -> '{replacement!.ItemId}' at {location.NameOrUniqueName} ({oldFurniture.TileLocation.X}, {oldFurniture.TileLocation.Y}).", LogLevel.Info);
                    }
                }

                // 冷蔵庫は通常の設置オブジェクト一覧とは別に保持されるため、個別に確認します。
                if (location is FarmHouse farmHouse && farmHouse.fridge.Value != null)
                    migrated += MigrateItems(farmHouse.fridge.Value.Items, byOldId, furnitureIds, monitor);
                else if (location is IslandFarmHouse islandFarmHouse && islandFarmHouse.fridge.Value != null)
                    migrated += MigrateItems(islandFarmHouse.fridge.Value.Items, byOldId, furnitureIds, monitor);

                // 通常のチェストと、収納機能を持つ家具の内容を確認します。
                foreach (var obj in location.Objects.Values)
                {
                    if (obj is Chest chest)
                        migrated += MigrateItems(chest.Items, byOldId, furnitureIds, monitor);
                }

            }

            // ジュニモの箱は全設置場所で共通のグローバル収納を使用します。
            // location.Objects のChest.Itemsとは別のため、一度だけ処理します。
            migrated += MigrateItems(Game1.player.team.GetOrCreateGlobalInventory(FarmerTeam.GlobalInventoryId_JunimoChest), byOldId, furnitureIds, monitor);

            foreach (Farmer farmer in Game1.getAllFarmers())
                migrated += MigrateItems(farmer.Items, byOldId, furnitureIds, monitor);

            if (migrated > 0)
                monitor.Log($"Furniture Migration completed. Migrated Furniture: {migrated}", LogLevel.Info);
        }

        /// <summary>
        /// インベントリまたは収納の各スロットを確認します。
        /// </summary>
        private static int MigrateItems(IList<Item> items, Dictionary<string, MigrationModel> byOldId,
            HashSet<string> furnitureIds, IMonitor monitor)
        {
            int migrated = 0;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] is not Furniture oldFurniture ||
                    !TryMigrate(oldFurniture, byOldId, furnitureIds, monitor, out Furniture? replacement))
                    continue;

                replacement!.Stack = oldFurniture.Stack;
                items[i] = replacement;
                migrated++;
                monitor.Log($"Migrated Furniture '{oldFurniture.ItemId}' -> '{replacement.ItemId}' in an inventory.", LogLevel.Info);
            }
            return migrated;
        }

        /// <summary>
        /// 新しい家具の生成と状態の引き継ぎに成功した場合だけ置き換えを許可します。
        /// </summary>
        private static bool TryMigrate(Furniture oldFurniture, Dictionary<string, MigrationModel> byOldId,
            HashSet<string> furnitureIds,
            IMonitor monitor, out Furniture? replacement)
        {
            replacement = null;
            if (!byOldId.TryGetValue(oldFurniture.ItemId, out MigrationModel? migration))
                return false;
            if (!furnitureIds.Contains(migration.NewId))
            {
                monitor.Log($"Furniture Migration target '{migration.NewId}' was not found in Data/Furniture.", LogLevel.Warn);
                return false;
            }
            try
            {
                Furniture candidate = Furniture.GetFurnitureInstance(migration.NewId, oldFurniture.TileLocation);
                if (candidate == null || (candidate.GetType() != oldFurniture.GetType() &&
                    oldFurniture.GetType() != typeof(Furniture)))
                {
                    monitor.Log($"Furniture Migration skipped '{migration.OldId}': incompatible furniture class.", LogLevel.Warn);
                    return false;
                }
                // 収納家具は専用の保存状態を持つため、内容を移せる実装ができるまで対象外とします。
                if (oldFurniture is StorageFurniture)
                {
                    monitor.Log($"Furniture Migration skipped '{migration.OldId}': storage furniture is not supported.", LogLevel.Warn);
                    return false;
                }

                StardewValley.Object? heldObject = oldFurniture.heldObject.Value;
                if (heldObject != null)
                {
                    // 外部Modが家具のheldObjectにダミーの空Chestを保持させる場合があります。
                    // 同一クラスかつ空のChest (O)-1 に限り、移行を許可します。
                    // ダミーを新家具へ引き継ぐと、家具の上にエラーアイテムとして描画されるため、
                    // 内容がないことを確認して移行先には設定しません。
                    if (oldFurniture.GetType() != candidate.GetType() ||
                        heldObject is not Chest heldChest ||
                        heldChest.ItemId != "-1" ||
                        heldChest.Items.Any(item => item != null) ||
                        heldChest.modData.Count() != 0 ||
                        candidate.heldObject.Value != null)
                    {
                        monitor.Log($"Furniture Migration skipped '{migration.OldId}': held object or storage state cannot be safely transferred. " +
                            $"FurnitureClass={oldFurniture.GetType().FullName}; HeldObject={heldObject.GetType().FullName} " +
                            $"(ItemId={heldObject.ItemId}); TargetClass={candidate.GetType().FullName}.", LogLevel.Warn);
                        return false;
                    }

                    // 空のダミーChestは新家具に引き継ぎません。
                    // 移行先のheldObjectはnullのままにし、エラーアイテムの描画を防ぎます。
                }

                // currentRotationだけを変更すると、描画・当たり判定が更新されません。
                // 家具本来の回転処理を使い、移行元と同じ回転状態まで進めます。
                int desiredRotation = oldFurniture.currentRotation.Value;
                int rotationCount = Math.Max(1, candidate.rotations.Value);
                if (desiredRotation < 0 || desiredRotation >= rotationCount)
                {
                    monitor.Log($"Furniture Migration skipped '{migration.OldId}': target doesn't support rotation {desiredRotation}.", LogLevel.Warn);
                    return false;
                }
                for (int rotation = 0; rotation < desiredRotation; rotation++)
                    candidate.rotate();

                candidate.modData.CopyFrom(oldFurniture.modData);
                replacement = candidate;
                return true;
            }
            catch (Exception ex)
            {
                monitor.Log($"Furniture Migration failed for '{migration.OldId}': {ex.Message}", LogLevel.Warn);
                return false;
            }
        }
    }
}
