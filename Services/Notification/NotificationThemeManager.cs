using StardewModdingAPI;
using StardewModdingAPI.Events;
using System.Text.Json;

namespace Ts_Core.Services.Notification
{
    /// <summary>
    /// 通知テーマの読み込み・登録・継承管理を行います。
    /// </summary>
    internal static class NotificationThemeManager
    {
        /// <summary>
        /// SMAPI Helper
        /// </summary>
        private static IModHelper helper = null!;

        /// <summary>
        /// ログ出力
        /// </summary>
        private static IMonitor monitor = null!;

        /// <summary>
        /// 通知テーマフォルダ監視
        /// </summary>
        private static readonly List<FileSystemWatcher> watchers = new();

        /// <summary>
        /// テーマファイルの再読み込み処理を
        /// 同期するためのロックオブジェクトです。
        /// </summary>
        private static readonly object reloadLock = new();

        /// <summary>
        /// リロード待機用タイマー
        /// </summary>
        private static Timer? reloadTimer;

        /// <summary>
        /// 次回Updateでリロードするか
        /// </summary>
        private static volatile bool reloadPending;

        //----------------------------------------
        // 登録済みテーマ一覧 (読み込み済みテーマ)
        //----------------------------------------

        private static readonly Dictionary<string, NotificationTheme> themes =
            new(StringComparer.OrdinalIgnoreCase);

        private static readonly HashSet<string> contentPackThemes =
            new(StringComparer.OrdinalIgnoreCase);

        private static readonly HashSet<string> contentPackThemeShortNames =
            new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 通知テーマ管理を初期化します。
        /// </summary>
        public static void Initialize(
            IModHelper helper,
            IMonitor monitor)
        {
            NotificationThemeManager.helper = helper;
            NotificationThemeManager.monitor = monitor;

            string path =
                Path.Combine(
                    helper.DirectoryPath,
                    "assets",
                    "notification");

            CreateWatcher(path);

            foreach (IContentPack pack in helper.ContentPacks.GetOwned())
            {
                string contentPackPath =
                    Path.Combine(
                        pack.DirectoryPath,
                        "assets",
                        "notification");

                if (Directory.Exists(contentPackPath))
                    CreateWatcher(contentPackPath);
            }

            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;

            ExportDefaultThemes();

            ReloadThemes();
        }

        /// <summary>
        /// 指定したフォルダのテーマファイルを
        /// 監視するFileWatcherを作成します。
        /// </summary>
        private static void CreateWatcher(string path)
        {
            FileSystemWatcher watcher =
                new(path)
                {
                    Filter = "*.json",
                    NotifyFilter =
                        NotifyFilters.LastWrite |
                        NotifyFilters.FileName
                };

            watcher.IncludeSubdirectories = true;

            watcher.Changed += OnThemeFileChanged;
            watcher.Created += OnThemeFileChanged;
            watcher.Deleted += OnThemeFileChanged;
            watcher.Renamed += OnThemeFileChanged;

            watcher.EnableRaisingEvents = true;

            watchers.Add(watcher);
        }

        /// <summary>
        /// 使用中のリソースを解放します。
        /// </summary>
        public static void Dispose()
        {
            helper.Events.GameLoop.UpdateTicked -= OnUpdateTicked;

            lock (reloadLock)
            {
                reloadTimer?.Dispose();
                reloadTimer = null;
            }

            foreach (FileSystemWatcher watcher in watchers)
            {
                watcher.EnableRaisingEvents = false;

                watcher.Changed -= OnThemeFileChanged;
                watcher.Created -= OnThemeFileChanged;
                watcher.Deleted -= OnThemeFileChanged;
                watcher.Renamed -= OnThemeFileChanged;

                watcher.Dispose();
            }

            watchers.Clear();
        }

        /// <summary>
        /// テーマファイル変更時
        /// （短時間に複数回発生するため遅延リロードする）
        /// </summary>
        private static void OnThemeFileChanged(
            object? sender,
            FileSystemEventArgs e)
        {
            lock (reloadLock)
            {
                reloadTimer?.Dispose();

                reloadTimer = new Timer(
                    _ =>
                    {
                        reloadPending = true;

                        lock (reloadLock)
                        {
                            reloadTimer?.Dispose();
                            reloadTimer = null;
                        }
                    },
                    null,
                    300,
                    Timeout.Infinite);
            }
        }

        /// <summary>
        /// リロード予約があればテーマを再読み込みします。
        /// </summary>
        private static void OnUpdateTicked(
            object? sender,
            UpdateTickedEventArgs e)
        {
            if (!reloadPending)
                return;

            reloadPending = false;

            ReloadThemes();
        }

        /// <summary>
        /// 全テーマを再読み込みします。
        /// </summary>
        public static void ReloadThemes()
        {
            try
            {
                Clear();

                RegisterDefaults();

                ReadBuiltinThemes();

                ReadContentPackThemes();

                ResolveInheritance();
            }

            catch (Exception ex)
            {
                monitor.Log(
                    ex.ToString(),
                    LogLevel.Error);
            }
        }

        /// <summary>
        /// 指定したテーマを取得します。
        /// 存在しない場合は Info を返します。
        /// </summary>
        public static NotificationTheme GetTheme(string name)
        {
            if (themes.TryGetValue(name, out NotificationTheme? theme))
                return theme;

            monitor.Log(
                $"Notification theme '{name}' was not found. Using 'Info' instead.",
                LogLevel.Warn);

            return themes["Info"];
        }

        /// <summary>
        /// TsCore標準テーマ名を取得します。
        /// </summary>
        public static IEnumerable<string> GetBuiltinThemeNames()
        {
            return themes.Keys
                .Except(contentPackThemes)
                .Except(contentPackThemeShortNames)
                .OrderBy(p => p);
        }

        /// <summary>
        /// Content Packのテーマ名を取得します。
        /// </summary>
        public static IEnumerable<string> GetContentPackThemeNames()
        {
            return contentPackThemes
                .OrderBy(p => p);
        }

        /// <summary>
        /// テーマを登録します。
        /// 同名テーマが存在する場合は上書きします。
        /// </summary>
        public static void Register(
            string name,
            NotificationTheme theme)
        {
            ArgumentNullException.ThrowIfNull(theme);

            themes[name] = theme;
        }

        //----------------------------------------
        // 標準テーマ一覧
        //----------------------------------------

        private static readonly (string Name, NotificationTheme Theme)[] DefaultThemes =
        {
            (nameof(NotificationThemes.Info),
                NotificationThemes.DefaultInfo),
            (nameof(NotificationThemes.Success),
                NotificationThemes.DefaultSuccess),
            (nameof(NotificationThemes.Error),
                NotificationThemes.DefaultError),
            (nameof(NotificationThemes.Warning),
                NotificationThemes.DefaultWarning),
            (nameof(NotificationThemes.Quest),
                NotificationThemes.DefaultQuest),
            (nameof(NotificationThemes.Achievement),
                NotificationThemes.DefaultAchievement),
            (nameof(NotificationThemes.Boss),
                NotificationThemes.DefaultBoss),
            (nameof(NotificationThemes.RetroWindow),
                NotificationThemes.DefaultRetroWindow),
        };

        /// <summary>
        /// デフォルトテーマJSONを初回のみ出力します。
        /// </summary>
        private static void ExportDefaultThemes()
        {
            string folder = Path.Combine(
                helper.DirectoryPath,
                "assets",
                "notification");

            Directory.CreateDirectory(folder);

            foreach (var theme in DefaultThemes)
            {
                ExportIfMissing(
                    $"{theme.Name}.json",
                    theme.Theme);
            }
        }

        /// <summary>
        /// ファイルが存在しない場合のみテーマを書き出します。
        /// </summary>
        private static void ExportIfMissing(
            string fileName,
            NotificationTheme theme)
        {
            string relative =
                Path.Combine(
                    "assets",
                    "notification",
                    fileName);

            string full =
                Path.Combine(
                    helper.DirectoryPath,
                    relative);

            if (File.Exists(full))
                return;

            helper.Data.WriteJsonFile(
                relative,
                theme.Clone());
        }

        /// <summary>
        /// 組み込みデフォルトテーマを登録します。
        /// </summary>
        private static void RegisterDefaults()
        {
            foreach (var theme in DefaultThemes)
            {
                Register(
                    theme.Name,
                    theme.Theme.Clone());
            }
        }

        /// <summary>
        /// Mod本体の通知テーマを読み込みます。
        /// </summary>
        private static void ReadBuiltinThemes()
        {
            string folder = Path.Combine(
                helper.DirectoryPath,
                "assets",
                "notification");

            if (!Directory.Exists(folder))
                return;

            foreach (string file in Directory.EnumerateFiles(
                folder,
                "*.json",
                SearchOption.AllDirectories))
            {
                //----------------------------------------
                // assets からの相対パス
                //----------------------------------------

                string relative =
                    Path.GetRelativePath(
                        helper.DirectoryPath,
                        file);

                NotificationTheme? theme =
                    helper.Data.ReadJsonFile<NotificationTheme>(relative);

                if (theme == null)
                    continue;

                string shortName =
                    Path.GetFileNameWithoutExtension(file);

                bool replaced =
                    themes.ContainsKey(shortName);

                Register(
                    shortName,
                    theme.Clone());

                monitor.Log(
                    replaced
                        ? $"Overriding builtin theme: {shortName}"
                        : $"Loaded builtin theme: {shortName}",
                    LogLevel.Trace);
            }
        }

        /// <summary>
        /// Content Pack の通知テーマを読み込みます。
        /// </summary>
        private static void ReadContentPackThemes()
        {
            foreach (IContentPack pack in helper.ContentPacks.GetOwned())
            {
                string folder = Path.Combine(
                    pack.DirectoryPath,
                    "assets",
                    "notification");

                if (!Directory.Exists(folder))
                    continue;

                foreach (string file in Directory.EnumerateFiles(
                    folder,
                    "*.json",
                    SearchOption.AllDirectories))
                {
                    //----------------------------------------
                    // ContentPack内からの相対パス
                    //----------------------------------------

                    string relative =
                        Path.GetRelativePath(
                            pack.DirectoryPath,
                            file);

                    NotificationTheme? theme =
                        pack.ReadJsonFile<NotificationTheme>(relative);

                    if (theme == null)
                        continue;

                    string shortName =
                        Path.GetFileNameWithoutExtension(file);

                    bool replaced =
                        themes.ContainsKey(shortName);

                    Register(
                        shortName,
                        theme.Clone());

                    Register(
                        $"{pack.Manifest.UniqueID}.{shortName}",
                        theme.Clone());

                    contentPackThemes.Add(
                        $"{pack.Manifest.UniqueID}.{shortName}");

                    contentPackThemeShortNames.Add(
                        shortName);

                    monitor.Log(
                        replaced
                            ? $"Overriding notification theme: {shortName} ({pack.Manifest.UniqueID})"
                            : $"Loaded notification theme: {shortName} ({pack.Manifest.UniqueID})",
                        LogLevel.Trace);
                }
            }
        }

        /// <summary>
        /// 全テーマの継承を解決します。
        /// </summary>
        private static void ResolveInheritance()
        {
            HashSet<string> resolved =
                new(StringComparer.OrdinalIgnoreCase);

            foreach (string name in themes.Keys.ToList())
            {
                ResolveTheme(
                    name,
                    resolved,
                    new HashSet<string>(StringComparer.OrdinalIgnoreCase));
            }
        }

        /// <summary>
        /// 指定テーマの継承を解決します。
        /// </summary>
        private static void ResolveTheme(
            string name,
            HashSet<string> resolved,
            HashSet<string> resolving)
        {
            //----------------------------------------
            // 解決済み
            //----------------------------------------

            if (resolved.Contains(name))
                return;

            //----------------------------------------
            // 循環継承
            //----------------------------------------

            if (!resolving.Add(name))
            {
                monitor.Log(
                    $"Circular inheritance detected for notification theme '{name}'.",
                    LogLevel.Warn);

                return;
            }

            //----------------------------------------
            // Theme取得
            //----------------------------------------

            NotificationTheme theme = themes[name];

            //----------------------------------------
            // Baseなし
            //----------------------------------------

            if (!string.IsNullOrWhiteSpace(theme.Base))
            {
                //----------------------------------------
                // 親Theme存在確認
                //----------------------------------------

                if (themes.TryGetValue(theme.Base, out NotificationTheme? parent))
                {
                    //----------------------------------------
                    // まず親を解決
                    //----------------------------------------

                    ResolveTheme(
                        theme.Base,
                        resolved,
                        resolving);

                    //----------------------------------------
                    // 継承
                    //----------------------------------------

                    ApplyInheritance(
                        parent,
                        theme);
                }
                else
                {
                    monitor.Log(
                        $"Base theme '{theme.Base}' was not found.",
                        LogLevel.Warn);
                }
            }

            //----------------------------------------
            // 解決完了
            //----------------------------------------

            resolving.Remove(name);

            resolved.Add(name);
        }

        /// <summary>
        /// 親テーマの未設定項目を子テーマへ継承します。
        /// </summary>
        private static void ApplyInheritance(
            NotificationTheme parent,
            NotificationTheme child)
        {
            child.BackgroundColor ??= parent.BackgroundColor;

            child.BorderColor ??= parent.BorderColor;
            child.BorderStyle ??= parent.BorderStyle;
            child.BorderThickness ??= parent.BorderThickness;

            child.TextColor ??= parent.TextColor;
            child.ShadowColor ??= parent.ShadowColor;
            child.DrawShadow ??= parent.DrawShadow;
            child.ShadowOffset ??= parent.ShadowOffset;
            child.TextAnchor ??= parent.TextAnchor;
            child.TextScale ??= parent.TextScale;

            child.MinHeight ??= parent.MinHeight;
            child.MinWidth ??= parent.MinWidth;

            child.PaddingX ??= parent.PaddingX;
            child.PaddingY ??= parent.PaddingY;
            child.BorderPadding ??= parent.BorderPadding;

            child.Anchor ??= parent.Anchor;
            child.OffsetX ??= parent.OffsetX;
            child.OffsetY ??= parent.OffsetY;
        }

        /// <summary>
        /// 登録されている通知テーマ名を取得します。
        /// </summary>
        public static IEnumerable<string> GetThemeNames()
        {
            return themes.Keys.OrderBy(p => p);
        }

        /// <summary>
        /// 登録済みテーマをすべて削除します。
        /// </summary>
        private static void Clear()
        {
            themes.Clear();
            contentPackThemes.Clear();
            contentPackThemeShortNames.Clear();
        }
    }
}