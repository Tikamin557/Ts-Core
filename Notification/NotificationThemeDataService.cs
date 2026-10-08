using StardewModdingAPI.Events;

namespace Ts_Core.Services.Notification
{
    /// <summary>
    /// Notification Theme用Data Assetを提供します。
    /// </summary>
    internal static class NotificationThemeDataService
    {
        /// <summary>
        /// Notification Theme Data Asset名
        /// </summary>
        public const string AssetName =
            "TsCore/NotificationThemes";

        //----------------------------------------
        // TsCore標準Theme ID
        //----------------------------------------

        private static readonly HashSet<string>
            DefaultThemeIds =
                new(StringComparer.OrdinalIgnoreCase)
                {
                    nameof(NotificationThemes.Info),
                    nameof(NotificationThemes.Success),
                    nameof(NotificationThemes.Error),
                    nameof(NotificationThemes.Warning),
                    nameof(NotificationThemes.Quest),
                    nameof(NotificationThemes.Achievement),
                    nameof(NotificationThemes.Boss),
                    nameof(NotificationThemes.Lavender),
                    nameof(NotificationThemes.Rose),
                    nameof(NotificationThemes.RetroWindow)
                };

        /// <summary>
        /// Data Asset要求時
        /// </summary>
        internal static void OnAssetRequested(
            object? sender,
            AssetRequestedEventArgs e)
        {
            if (!e.NameWithoutLocale.IsEquivalentTo(AssetName))
                return;

            //----------------------------------------
            // Data Assetには外部Themeのみ登録します。
            //----------------------------------------

            e.LoadFrom(
                () =>
                    new Dictionary<string, NotificationTheme>(),
                AssetLoadPriority.Exclusive);
        }

        /// <summary>
        /// TsCore標準Notification Themeを取得します。
        /// </summary>
        internal static Dictionary<string, NotificationTheme>
            GetDefaultThemes()
        {
            return new Dictionary<string, NotificationTheme>(
                StringComparer.OrdinalIgnoreCase)
            {
                [nameof(NotificationThemes.Info)] =
                    NotificationThemes.DefaultInfo.Clone(),

                [nameof(NotificationThemes.Success)] =
                    NotificationThemes.DefaultSuccess.Clone(),

                [nameof(NotificationThemes.Error)] =
                    NotificationThemes.DefaultError.Clone(),

                [nameof(NotificationThemes.Warning)] =
                    NotificationThemes.DefaultWarning.Clone(),

                [nameof(NotificationThemes.Quest)] =
                    NotificationThemes.DefaultQuest.Clone(),

                [nameof(NotificationThemes.Achievement)] =
                    NotificationThemes.DefaultAchievement.Clone(),

                [nameof(NotificationThemes.Boss)] =
                    NotificationThemes.DefaultBoss.Clone(),

                [nameof(NotificationThemes.Lavender)] =
                    NotificationThemes.DefaultLavender.Clone(),

                [nameof(NotificationThemes.Rose)] =
                    NotificationThemes.DefaultRose.Clone(),

                [nameof(NotificationThemes.RetroWindow)] =
                    NotificationThemes.DefaultRetroWindow.Clone()
            };
        }

        /// <summary>
        /// 指定Theme IDがTsCore標準Themeか確認します。
        /// </summary>
        internal static bool IsDefaultTheme(
            string themeId)
        {
            if (string.IsNullOrWhiteSpace(themeId))
                return false;

            return DefaultThemeIds.Contains(themeId);
        }
    }
}