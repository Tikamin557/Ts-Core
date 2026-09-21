using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace Ts_Core.Services.Notification
{
    /// <summary>
    /// Notification Themeの取得・継承管理を行います。
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

        //----------------------------------------
        // 解決済みTheme
        //----------------------------------------

        private static Dictionary<string, NotificationTheme>? resolvedThemes;

        /// <summary>
        /// Notification Theme管理を初期化します。
        /// </summary>
        public static void Initialize(
            IModHelper helper,
            IMonitor monitor)
        {
            NotificationThemeManager.helper = helper;
            NotificationThemeManager.monitor = monitor;

            //----------------------------------------
            // Data Asset更新監視
            //----------------------------------------

            helper.Events.Content.AssetsInvalidated
                += OnAssetsInvalidated;
        }

        /// <summary>
        /// 使用中のイベントを解除します。
        /// </summary>
        public static void Dispose()
        {
            helper.Events.Content.AssetsInvalidated
                -= OnAssetsInvalidated;

            resolvedThemes = null;
        }

        /// <summary>
        /// Data Assetが更新された時に
        /// 解決済みThemeキャッシュを破棄します。
        /// </summary>
        private static void OnAssetsInvalidated(
            object? sender,
            AssetsInvalidatedEventArgs e)
        {
            if (!e.NamesWithoutLocale.Any(
                name =>
                    name.IsEquivalentTo(
                        NotificationThemeDataService.AssetName)))
            {
                return;
            }

            resolvedThemes = null;
        }

        //----------------------------------------
        // Theme取得
        //----------------------------------------

        /// <summary>
        /// 指定したThemeを取得します。
        /// 存在しない場合はInfoを返します。
        /// </summary>
        public static NotificationTheme GetTheme(
            string name)
        {
            Dictionary<string, NotificationTheme> themes =
                GetResolvedThemes();

            if (themes.TryGetValue(
                name,
                out NotificationTheme? theme))
            {
                return theme;
            }

            monitor.Log(
                $"Notification theme '{name}' was not found. Using 'Info' instead.",
                LogLevel.Warn);

            //----------------------------------------
            // InfoはTsCore標準Themeなので
            // 通常は必ず存在します。
            //----------------------------------------

            if (themes.TryGetValue(
                nameof(NotificationThemes.Info),
                out NotificationTheme? info))
            {
                return info;
            }

            //----------------------------------------
            // 万一Data AssetからInfoまで削除された場合
            // 組み込みDefaultを最後のFallbackにします。
            //----------------------------------------

            monitor.Log(
                "Default notification theme 'Info' was not found in the data asset. Using the built-in fallback.",
                LogLevel.Warn);

            return NotificationThemes.DefaultInfo.Clone();
        }

        /// <summary>
        /// 継承解決済みの全Themeを取得します。
        /// </summary>
        private static Dictionary<string, NotificationTheme>
            GetResolvedThemes()
        {
            if (resolvedThemes != null)
                return resolvedThemes;

            resolvedThemes =
                LoadAndResolveThemes();

            return resolvedThemes;
        }

        /// <summary>
        /// Notification Themeを読み込み、継承を解決します。
        /// </summary>
        private static Dictionary<string, NotificationTheme>
            LoadAndResolveThemes()
        {
            //----------------------------------------
            // TsCore標準Theme
            //----------------------------------------

            Dictionary<string, NotificationTheme> themes =
                NotificationThemeDataService.GetDefaultThemes();

            //----------------------------------------
            // 外部Data Asset Theme
            //----------------------------------------

            Dictionary<string, NotificationTheme> assetThemes;

            try
            {
                assetThemes =
                    helper.GameContent.Load<
                        Dictionary<string, NotificationTheme>>(
                            NotificationThemeDataService.AssetName);
            }
            catch (Exception ex)
            {
                monitor.Log(
                    $"Failed loading Notification Themes from " +
                    $"'{NotificationThemeDataService.AssetName}'.\n{ex}",
                    LogLevel.Error);

                assetThemes =
                    new Dictionary<string, NotificationTheme>();
            }

            foreach (var pair in assetThemes)
            {
                if (string.IsNullOrWhiteSpace(pair.Key)
                    || pair.Value == null)
                {
                    continue;
                }

                //----------------------------------------
                // TsCore標準IDは予約済み
                //----------------------------------------

                if (NotificationThemeDataService
                    .IsDefaultTheme(pair.Key))
                {
                    monitor.Log(
                        $"Notification Theme ID '{pair.Key}' is " +
                        $"reserved by T's Core and cannot be overridden.",
                        LogLevel.Warn);

                    continue;
                }

                themes[pair.Key] =
                    pair.Value.Clone();
            }

            //----------------------------------------
            // Theme継承を解決
            //----------------------------------------

            ResolveInheritance(themes);

            return themes;
        }

        //----------------------------------------
        // Theme一覧
        //----------------------------------------

        /// <summary>
        /// 登録されているNotification Theme名を取得します。
        /// </summary>
        public static IEnumerable<string> GetThemeNames()
        {
            return GetResolvedThemes()
                .Keys
                .OrderBy(p => p);
        }

        /// <summary>
        /// TsCore標準Data Asset Theme名を取得します。
        /// </summary>
        public static IEnumerable<string>
            GetDefaultThemeNames()
        {
            return GetResolvedThemes()
                .Keys
                .Where(
                    NotificationThemeDataService
                        .IsDefaultTheme)
                .OrderBy(p => p);
        }

        /// <summary>
        /// 外部Data Asset Theme名を取得します。
        /// </summary>
        public static IEnumerable<string>
            GetExternalThemeNames()
        {
            return GetResolvedThemes()
                .Keys
                .Where(
                    name =>
                        !NotificationThemeDataService
                            .IsDefaultTheme(name))
                .OrderBy(p => p);
        }

        /// <summary>
        /// Theme情報を直接取得します。
        /// Debug表示用です。
        /// </summary>
        internal static bool TryGetTheme(
            string name,
            out NotificationTheme? theme)
        {
            return GetResolvedThemes()
                .TryGetValue(
                    name,
                    out theme);
        }

        //----------------------------------------
        // Theme継承
        //----------------------------------------

        /// <summary>
        /// 全Themeの継承を解決します。
        /// </summary>
        private static void ResolveInheritance(
            Dictionary<string, NotificationTheme> themes)
        {
            HashSet<string> resolved =
                new(StringComparer.OrdinalIgnoreCase);

            foreach (
                string name in themes.Keys.ToList())
            {
                ResolveTheme(
                    name,
                    themes,
                    resolved,
                    new HashSet<string>(
                        StringComparer.OrdinalIgnoreCase));
            }
        }

        /// <summary>
        /// 指定Themeの継承を解決します。
        /// </summary>
        private static void ResolveTheme(
            string name,
            Dictionary<string, NotificationTheme> themes,
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

            if (!themes.TryGetValue(
                name,
                out NotificationTheme? theme))
            {
                resolving.Remove(name);
                return;
            }

            //----------------------------------------
            // Baseあり
            //----------------------------------------

            if (!string.IsNullOrWhiteSpace(theme.Base))
            {
                string baseName =
                    theme.Base;

                //----------------------------------------
                // 親Theme存在確認
                //----------------------------------------

                if (themes.TryGetValue(
                    baseName,
                    out NotificationTheme? parent))
                {
                    //----------------------------------------
                    // まず親を解決
                    //----------------------------------------

                    ResolveTheme(
                        baseName,
                        themes,
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
                        $"Base theme '{baseName}' was not found for notification theme '{name}'.",
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
        /// 親Themeの未設定項目を
        /// 子Themeへ継承します。
        /// </summary>
        private static void ApplyInheritance(
            NotificationTheme parent,
            NotificationTheme child)
        {
            child.BackgroundColor ??=
                parent.BackgroundColor;

            child.BorderColor ??=
                parent.BorderColor;

            child.BorderStyle ??=
                parent.BorderStyle;

            child.BorderThickness ??=
                parent.BorderThickness;

            child.TextColor ??=
                parent.TextColor;

            child.ShadowColor ??=
                parent.ShadowColor;

            child.DrawShadow ??=
                parent.DrawShadow;

            child.ShadowOffset ??=
                parent.ShadowOffset;

            child.TextAnchor ??=
                parent.TextAnchor;

            child.TextScale ??=
                parent.TextScale;

            child.MinHeight ??=
                parent.MinHeight;

            child.MinWidth ??=
                parent.MinWidth;

            child.PaddingX ??=
                parent.PaddingX;

            child.PaddingY ??=
                parent.PaddingY;

            child.BorderPadding ??=
                parent.BorderPadding;

            child.Anchor ??=
                parent.Anchor;

            child.OffsetX ??=
                parent.OffsetX;

            child.OffsetY ??=
                parent.OffsetY;

            //----------------------------------------
            // 表示終了条件
            //----------------------------------------

            child.DismissOnLocationChange ??=
                parent.DismissOnLocationChange;

            child.DismissOnEnterLocations ??=
                parent.DismissOnEnterLocations?
                    .ToList();
        }
    }
}