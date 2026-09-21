using StardewModdingAPI;
using StardewValley;
using Ts_Core.Models;

namespace Ts_Core.Services.Migration
{
    /// <summary>
    /// 登録されているMigrationの情報です。
    /// </summary>
    internal sealed class RegisteredMigrationInfo
    {
        /// <summary>
        /// Migration IDです。
        /// </summary>
        public string Id { get; init; } = "";

        /// <summary>
        /// Migrationの種類です。
        /// </summary>
        public string Type { get; init; } = "";

        /// <summary>
        /// 移行元となる旧IDです。
        /// </summary>
        public string OldId { get; init; } = "";

        /// <summary>
        /// 移行先となる新IDです。
        /// </summary>
        public string NewId { get; init; } = "";
    }

    /// <summary>
    /// ID Migrationを管理します。
    /// </summary>
    public static class MigrationService
    {
        //----------------------------------------
        // Migration Data
        //----------------------------------------

        /// <summary>
        /// Migration用Data Assetを取得します。
        /// </summary>
        private static Dictionary<
            string,
            MigrationModel> GetMigrationData()
        {
            return Game1.content.Load<
                Dictionary<
                    string,
                    MigrationModel>>(
                        MigrationDataService.AssetName);
        }

        //----------------------------------------
        // Migration取得
        //----------------------------------------

        /// <summary>
        /// 現在登録されている
        /// 有効なMigrationを取得します。
        /// </summary>
        internal static IReadOnlyList<MigrationModel>
            GetMigrations(
                IMonitor? monitor = null)
        {
            return GetValidMigrationEntries(
                    monitor)
                .Select(entry =>
                    entry.Value)
                .ToList();
        }

        /// <summary>
        /// Building用の
        /// Migration定義を取得します。
        /// </summary>
        internal static IReadOnlyList<MigrationModel>
            GetBuildingMigrations(
                IMonitor? monitor = null)
        {
            return GetValidMigrationEntries(
                    monitor)
                .Where(entry =>
                    string.Equals(
                        entry.Value.Type,
                        "Building",
                        StringComparison.OrdinalIgnoreCase))
                .Select(entry =>
                    entry.Value)
                .ToList();
        }

        /// <summary>
        /// 現在登録されている
        /// Migration情報を取得します。
        /// </summary>
        internal static IReadOnlyList<RegisteredMigrationInfo>
            GetRegisteredMigrations(
                IMonitor? monitor = null)
        {
            return GetValidMigrationEntries(
                    monitor)
                .Select(entry =>
                    new RegisteredMigrationInfo
                    {
                        Id = entry.Key,
                        Type = entry.Value.Type,
                        OldId = entry.Value.OldId,
                        NewId = entry.Value.NewId
                    })
                .OrderBy(migration =>
                    migration.Type)
                .ThenBy(migration =>
                    migration.OldId)
                .ToList();
        }

        //----------------------------------------
        // Migration Validation
        //----------------------------------------

        /// <summary>
        /// Data Asset内のMigrationから
        /// 有効な定義のみを取得します。
        /// </summary>
        private static IReadOnlyList<
            KeyValuePair<
                string,
                MigrationModel>>
            GetValidMigrationEntries(
                IMonitor? monitor)
        {
            Dictionary<
                string,
                MigrationModel> data =
                    GetMigrationData();

            List<
                KeyValuePair<
                    string,
                    MigrationModel>> result =
                        new();

            //----------------------------------------
            // 重複確認用
            //----------------------------------------

            Dictionary<
                string,
                string> registeredKeys =
                    new(
                        StringComparer.Ordinal);

            //----------------------------------------
            // Migration確認
            //----------------------------------------

            foreach (
                KeyValuePair<
                    string,
                    MigrationModel> entry
                in data)
            {
                string id =
                    entry.Key;

                MigrationModel migration =
                    entry.Value;

                //----------------------------------------
                // Migration ID
                //----------------------------------------

                if (string.IsNullOrWhiteSpace(
                        id))
                {
                    monitor?.Log(
                        "Migration entry has no ID.",
                        LogLevel.Warn);

                    continue;
                }

                //----------------------------------------
                // Type
                //----------------------------------------

                if (string.IsNullOrWhiteSpace(
                        migration.Type))
                {
                    monitor?.Log(
                        $"Migration '{id}' has no Type.",
                        LogLevel.Warn);

                    continue;
                }

                //----------------------------------------
                // OldId
                //----------------------------------------

                if (string.IsNullOrWhiteSpace(
                        migration.OldId))
                {
                    monitor?.Log(
                        $"Migration '{id}' has no OldId.",
                        LogLevel.Warn);

                    continue;
                }

                //----------------------------------------
                // NewId
                //----------------------------------------

                if (string.IsNullOrWhiteSpace(
                        migration.NewId))
                {
                    monitor?.Log(
                        $"Migration '{id}' has no NewId.",
                        LogLevel.Warn);

                    continue;
                }

                //----------------------------------------
                // 同一ID
                //----------------------------------------

                if (string.Equals(
                        migration.OldId,
                        migration.NewId,
                        StringComparison.Ordinal))
                {
                    monitor?.Log(
                        $"Migration '{id}' has the same OldId and NewId: '{migration.OldId}'.",
                        LogLevel.Warn);

                    continue;
                }

                //----------------------------------------
                // 現在対応しているType
                //----------------------------------------

                if (!string.Equals(
                        migration.Type,
                        "Building",
                        StringComparison.OrdinalIgnoreCase))
                {
                    monitor?.Log(
                        $"Migration '{id}' has unsupported Type '{migration.Type}'.",
                        LogLevel.Warn);

                    continue;
                }

                //----------------------------------------
                // Type + OldId 重複確認
                //----------------------------------------

                string duplicateKey =
                    $"{migration.Type.ToLowerInvariant()}\0{migration.OldId}";

                if (registeredKeys.TryGetValue(
                        duplicateKey,
                        out string? existingId))
                {
                    monitor?.Log(
                        $"Duplicate Migration ignored.\n" +
                        $"Type: {migration.Type}\n" +
                        $"OldId: {migration.OldId}\n" +
                        $"Existing Migration ID: {existingId}\n" +
                        $"Ignored Migration ID: {id}",
                        LogLevel.Warn);

                    continue;
                }

                //----------------------------------------
                // 有効なMigration
                //----------------------------------------

                registeredKeys[
                    duplicateKey] =
                        id;

                result.Add(
                    entry);
            }

            return result;
        }
    }
}