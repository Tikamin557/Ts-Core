using StardewModdingAPI;
using Ts_Core.Models;

namespace Ts_Core.Services.Migration
{
    /// <summary>
    /// 登録済みMigrationの情報です。
    /// </summary>
    internal sealed class RegisteredMigrationInfo
    {
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

        /// <summary>
        /// Migrationを登録したModまたはContent Packです。
        /// </summary>
        public string Owner { get; init; } = "";

        /// <summary>
        /// Migration定義ファイルのパスです。
        /// </summary>
        public string SourceFile { get; init; } = "";
    }

    /// <summary>
    /// ID Migrationを管理します。
    /// </summary>
    public static class MigrationService
    {
        //----------------------------------------
        // アセットフォルダ
        //----------------------------------------

        private const string AssetFolder = "migration";

        //----------------------------------------
        // 登録済みMigration
        //----------------------------------------

        private static readonly List<MigrationModel>
            Migrations =
                new();

        //----------------------------------------
        // 登録済みMigration情報
        //----------------------------------------

        private static readonly List<RegisteredMigrationInfo>
            RegisteredMigrations =
                new();

        //----------------------------------------
        // Migration取得
        //----------------------------------------

        /// <summary>
        /// 現在登録されているMigrationを取得します。
        /// </summary>
        internal static IReadOnlyList<MigrationModel>
            GetMigrations()
        {
            return Migrations;
        }

        /// <summary>
        /// Building用のMigration定義を取得します。
        /// </summary>
        internal static IReadOnlyList<MigrationModel>
            GetBuildingMigrations()
        {
            return Migrations
                .Where(migration =>
                    string.Equals(
                        migration.Type,
                        "Building",
                        StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        /// <summary>
        /// 現在登録されているMigration情報を取得します。
        /// </summary>
        internal static IReadOnlyList<RegisteredMigrationInfo>
            GetRegisteredMigrations()
        {
            return RegisteredMigrations
                .OrderBy(migration => migration.Type)
                .ThenBy(migration => migration.OldId)
                .ToList();
        }

        //----------------------------------------
        // 読み込み
        //----------------------------------------

        /// <summary>
        /// Migration定義を読み込みます。
        /// </summary>
        public static void Load(
            IModHelper helper,
            IMonitor monitor)
        {
            ReadBuiltinMigrations(
                helper,
                monitor);

            ReadContentPackMigrations(
                helper,
                monitor);
        }

        //----------------------------------------
        // 再読み込み
        //----------------------------------------

        /// <summary>
        /// Migration定義をすべて再読み込みします。
        /// </summary>
        public static void Reload(
            IModHelper helper,
            IMonitor monitor)
        {
            monitor.Log(
                "Reloading Migrations...",
                LogLevel.Info);

            Clear();

            Load(
                helper,
                monitor);

            monitor.Log(
                $"Migrations reloaded successfully. Registered Migrations: {RegisteredMigrations.Count}",
                LogLevel.Info);
        }

        //----------------------------------------
        // Migration登録
        //----------------------------------------

        /// <summary>
        /// Migrationを登録します。
        /// </summary>
        private static void RegisterMigration(
            MigrationModel model,
            string owner,
            string sourceFile,
            IMonitor monitor)
        {
            //----------------------------------------
            // Type
            //----------------------------------------

            if (string.IsNullOrWhiteSpace(
                    model.Type))
            {
                monitor.Log(
                    $"Migration in '{sourceFile}' has no Type.",
                    LogLevel.Warn);

                return;
            }

            //----------------------------------------
            // OldId
            //----------------------------------------

            if (string.IsNullOrWhiteSpace(
                    model.OldId))
            {
                monitor.Log(
                    $"Migration in '{sourceFile}' has no OldId.",
                    LogLevel.Warn);

                return;
            }

            //----------------------------------------
            // NewId
            //----------------------------------------

            if (string.IsNullOrWhiteSpace(
                    model.NewId))
            {
                monitor.Log(
                    $"Migration '{model.OldId}' in '{sourceFile}' has no NewId.",
                    LogLevel.Warn);

                return;
            }

            //----------------------------------------
            // 同一ID
            //----------------------------------------

            if (string.Equals(
                    model.OldId,
                    model.NewId,
                    StringComparison.Ordinal))
            {
                monitor.Log(
                    $"Migration '{model.OldId}' in '{sourceFile}' has the same OldId and NewId.",
                    LogLevel.Warn);

                return;
            }

            //----------------------------------------
            // 現在対応しているType
            //----------------------------------------

            if (!string.Equals(
                    model.Type,
                    "Building",
                    StringComparison.OrdinalIgnoreCase))
            {
                monitor.Log(
                    $"Unsupported Migration Type '{model.Type}' in '{sourceFile}'.",
                    LogLevel.Warn);

                return;
            }

            //----------------------------------------
            // 重複確認
            //----------------------------------------

            RegisteredMigrationInfo? existing =
                RegisteredMigrations
                    .FirstOrDefault(migration =>
                        string.Equals(
                            migration.Type,
                            model.Type,
                            StringComparison.OrdinalIgnoreCase)
                        && string.Equals(
                            migration.OldId,
                            model.OldId,
                            StringComparison.Ordinal));

            if (existing != null)
            {
                monitor.Log(
                    $"Duplicate Migration ignored.\n" +
                    $"Type: {model.Type}\n" +
                    $"OldId: {model.OldId}\n" +
                    $"Already registered by: {existing.Owner}\n" +
                    $"Existing file: {existing.SourceFile}\n" +
                    $"Ignored migration owner: {owner}\n" +
                    $"Ignored file: {sourceFile}",
                    LogLevel.Warn);

                return;
            }

            //----------------------------------------
            // 登録
            //----------------------------------------

            Migrations.Add(
                model);

            RegisteredMigrations.Add(
                new RegisteredMigrationInfo
                {
                    Type = model.Type,
                    OldId = model.OldId,
                    NewId = model.NewId,
                    Owner = owner,
                    SourceFile = sourceFile
                });

            monitor.Log(
                $"Registered {model.Type} Migration '{model.OldId}' -> '{model.NewId}' from '{owner}'.",
                LogLevel.Trace);
        }

        //----------------------------------------
        // TsCore内のMigration
        //----------------------------------------

        private static void ReadBuiltinMigrations(
            IModHelper helper,
            IMonitor monitor)
        {
            string folder =
                Path.Combine(
                    helper.DirectoryPath,
                    "assets",
                    AssetFolder);

            if (!Directory.Exists(folder))
                return;

            foreach (string file in Directory.EnumerateFiles(
                folder,
                "*.json",
                SearchOption.AllDirectories))
            {
                string relative =
                    Path.GetRelativePath(
                        helper.DirectoryPath,
                        file);

                try
                {
                    List<MigrationModel>? migrations =
                        helper.Data.ReadJsonFile<List<MigrationModel>>(
                            relative);

                    if (migrations == null)
                        continue;

                    foreach (MigrationModel migration in migrations)
                    {
                        RegisterMigration(
                            migration,
                            "T's Core",
                            relative,
                            monitor);
                    }

                    monitor.Log(
                        $"Loaded builtin Migration file: {relative}",
                        LogLevel.Trace);
                }
                catch (Exception ex)
                {
                    monitor.Log(
                        $"Failed to load Migration file '{relative}': {ex.Message}",
                        LogLevel.Warn);
                }
            }
        }

        //----------------------------------------
        // Content PackのMigration
        //----------------------------------------

        private static void ReadContentPackMigrations(
            IModHelper helper,
            IMonitor monitor)
        {
            foreach (IContentPack pack
                     in helper.ContentPacks.GetOwned())
            {
                string folder =
                    Path.Combine(
                        pack.DirectoryPath,
                        "assets",
                        AssetFolder);

                if (!Directory.Exists(folder))
                    continue;

                foreach (string file in Directory.EnumerateFiles(
                    folder,
                    "*.json",
                    SearchOption.AllDirectories))
                {
                    string relativePath =
                        Path.GetRelativePath(
                            pack.DirectoryPath,
                            file);

                    try
                    {
                        monitor.Log(
                            $"Loading Migration file: {pack.Manifest.UniqueID}/{relativePath}",
                            LogLevel.Trace);

                        List<MigrationModel>? migrations =
                            pack.ReadJsonFile<List<MigrationModel>>(
                                relativePath);

                        if (migrations == null)
                            continue;

                        foreach (MigrationModel migration in migrations)
                        {
                            RegisterMigration(
                                migration,
                                pack.Manifest.UniqueID,
                                relativePath,
                                monitor);
                        }

                        monitor.Log(
                            $"Loaded Migration file: {pack.Manifest.UniqueID}/{relativePath}",
                            LogLevel.Trace);
                    }
                    catch (Exception ex)
                    {
                        monitor.Log(
                            $"Failed to load Migration file '{pack.Manifest.UniqueID}/{relativePath}': {ex.Message}",
                            LogLevel.Warn);
                    }
                }
            }
        }

        //----------------------------------------
        // Migration削除
        //----------------------------------------

        /// <summary>
        /// 登録済みMigrationをすべて削除します。
        /// </summary>
        internal static void Clear()
        {
            Migrations.Clear();
            RegisteredMigrations.Clear();
        }
    }
}