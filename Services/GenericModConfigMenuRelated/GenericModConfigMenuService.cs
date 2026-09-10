using StardewModdingAPI;
using Ts_Core.Interfaces;
using Ts_Core.Models;

namespace Ts_Core.Services.GenericModConfigMenuRelated
{
    /// <summary>
    /// T's Core自身の設定を
    /// Generic Mod Config Menuへ登録します。
    /// </summary>
    internal static class GenericModConfigMenuService
    {
        //----------------------------------------
        // GMCM Mod ID
        //----------------------------------------

        private const string GenericModConfigMenuId =
            "spacechase0.GenericModConfigMenu";

        //----------------------------------------
        // Time Skip
        //----------------------------------------

        /// <summary>
        /// ゲーム内時刻をTime Skipの
        /// スライダー位置へ変換します。
        /// </summary>
        private static int TimeToSliderValue(
            int time)
        {
            int hour =
                time / 100;

            int minute =
                time % 100;

            int totalMinutes =
                hour * 60 + minute;

            return
                (totalMinutes - 6 * 60) / 10;
        }

        /// <summary>
        /// Time Skipのスライダー位置を
        /// ゲーム内時刻へ変換します。
        /// </summary>
        private static int SliderValueToTime(
            int value)
        {
            int totalMinutes =
                6 * 60 + value * 10;

            int hour =
                totalMinutes / 60;

            int minute =
                totalMinutes % 60;

            return
                hour * 100 + minute;
        }

        //----------------------------------------
        // 登録
        //----------------------------------------

        /// <summary>
        /// T's Coreの設定をGMCMへ登録します。
        /// </summary>
        internal static void Register(
            IModHelper helper,
            IManifest manifest,
            Func<ModConfig> getConfig,
            Action<ModConfig> setConfig)
        {
            //----------------------------------------
            // GMCM API取得
            //----------------------------------------

            IGenericModConfigMenuApi? api =
                helper.ModRegistry
                    .GetApi<IGenericModConfigMenuApi>(
                        GenericModConfigMenuId);

            if (api == null)
                return;

            //----------------------------------------
            // Mod登録
            //----------------------------------------

            api.Register(
                manifest,
                reset: () =>
                {
                    setConfig(
                        new ModConfig());
                },
                save: () =>
                {
                    helper.WriteConfig(
                        getConfig());
                });

            //----------------------------------------
            // 配偶者部屋タイル修正
            //----------------------------------------

            api.AddBoolOption(
                manifest,
                getValue: () =>
                    getConfig()
                        .EnableSpouseRoomTileFix,
                setValue: value =>
                    getConfig()
                        .EnableSpouseRoomTileFix =
                            value,
                name: () =>
                    helper.Translation.Get(
                        "config.EnableSpouseRoomTileFix.name"),
                tooltip: () =>
                    helper.Translation.Get(
                        "config.EnableSpouseRoomTileFix.description"),
                fieldId:
                    "EnableSpouseRoomTileFix");

            //----------------------------------------
            // デバッグ支援機能
            //----------------------------------------

            api.AddSectionTitle(
                manifest,
                text: () =>
                    helper.Translation.Get(
                        "config.DebugSupport.name"),
                tooltip: () =>
                    helper.Translation.Get(
                        "config.DebugSupport.description"));

            //----------------------------------------
            // Time Skip
            //----------------------------------------

            api.AddSectionTitle(
                manifest,
                text: () =>
                    helper.Translation.Get(
                        "config.TimeSkip.name"),
                tooltip: () =>
                    helper.Translation.Get(
                        "config.TimeSkip.description"));

            //----------------------------------------
            // Time Skip - 実行キー
            //----------------------------------------

            api.AddKeybindList(
                manifest,
                getValue: () =>
                    getConfig()
                        .TimeSkipKey,
                setValue: value =>
                    getConfig()
                        .TimeSkipKey =
                            value,
                name: () =>
                    helper.Translation.Get(
                        "config.TimeSkipKey.name"),
                tooltip: () =>
                    helper.Translation.Get(
                        "config.TimeSkipKey.description"),
                fieldId:
                    "TimeSkipKey");

            //----------------------------------------
            // Time Skip - 移動時刻
            //----------------------------------------

            api.AddNumberOption(
                manifest,
                getValue: () =>
                    TimeToSliderValue(
                        getConfig()
                            .TimeSkipTime),
                setValue: value =>
                    getConfig()
                        .TimeSkipTime =
                            SliderValueToTime(
                                value),
                name: () =>
                    helper.Translation.Get(
                        "config.TimeSkipTime.name"),
                tooltip: () =>
                    helper.Translation.Get(
                        "config.TimeSkipTime.description"),
                min:
                    0,
                max:
                    120,
                interval:
                    1,
                formatValue: value =>
                {
                    int time =
                        SliderValueToTime(
                            value);

                    return
                        $"{time / 100}:{time % 100:00}";
                },
                fieldId:
                    "TimeSkipTime");

            //----------------------------------------
            // Time Skip (Duration)
            //----------------------------------------

            api.AddSectionTitle(
                manifest,
                text: () =>
                    helper.Translation.Get(
                        "config.TimeSkipDuration.name"),
                tooltip: () =>
                    helper.Translation.Get(
                        "config.TimeSkipDuration.description"));

            //----------------------------------------
            // Time Skip (Duration) - 実行キー
            //----------------------------------------

            api.AddKeybindList(
                manifest,
                getValue: () =>
                    getConfig()
                        .TimeSkipDurationKey,
                setValue: value =>
                    getConfig()
                        .TimeSkipDurationKey =
                            value,
                name: () =>
                    helper.Translation.Get(
                        "config.TimeSkipDurationKey.name"),
                tooltip: () =>
                    helper.Translation.Get(
                        "config.TimeSkipDurationKey.description"),
                fieldId:
                    "TimeSkipDurationKey");

            //----------------------------------------
            // Time Skip (Duration) - 時間
            //----------------------------------------

            api.AddNumberOption(
                manifest,
                getValue: () =>
                    getConfig()
                        .TimeSkipDuration,
                setValue: value =>
                    getConfig()
                        .TimeSkipDuration =
                            value,
                name: () =>
                    helper.Translation.Get(
                        "config.TimeSkipDurationValue.name"),
                tooltip: () =>
                    helper.Translation.Get(
                        "config.TimeSkipDurationValue.description"),
                min:
                    10,
                max:
                    1200,
                interval:
                    10,
                formatValue: value =>
                    helper.Translation.Get(
                        "config.TimeSkipDurationValue.format",
                        new
                        {
                            Minutes =
                                value
                        }),
                fieldId:
                    "TimeSkipDuration");

            //----------------------------------------
            // Time Skip - スキップ速度
            //----------------------------------------

            api.AddTextOption(
                manifest,
                getValue: () =>
                    getConfig()
                        .TimeSkipSpeed
                        .ToString(),
                setValue: value =>
                {
                    if (Enum.TryParse(
                        value,
                        out TimeSkipSpeed speed))
                    {
                        getConfig()
                            .TimeSkipSpeed =
                                speed;
                    }
                },
                name: () =>
                    helper.Translation.Get(
                        "config.TimeSkipSpeed.name"),
                tooltip: () =>
                    helper.Translation.Get(
                        "config.TimeSkipSpeed.description"),
                allowedValues:
                    new[]
                    {
            nameof(
                TimeSkipSpeed.Slow),

            nameof(
                TimeSkipSpeed.Normal),

            nameof(
                TimeSkipSpeed.Fast),

            nameof(
                TimeSkipSpeed.VeryFast)
                    },
                formatAllowedValue: value =>
                    helper.Translation.Get(
                        $"config.TimeSkipSpeed.{value}"),
                fieldId:
                    "TimeSkipSpeed");

            //----------------------------------------
            // Time Skip - 完了通知
            //----------------------------------------

            api.AddBoolOption(
                manifest,
                getValue: () =>
                    getConfig()
                        .TimeSkipNotification,
                setValue: value =>
                    getConfig()
                        .TimeSkipNotification =
                            value,
                name: () =>
                    helper.Translation.Get(
                        "config.TimeSkipNotification.name"),
                tooltip: () =>
                    helper.Translation.Get(
                        "config.TimeSkipNotification.description"),
                fieldId:
                    "TimeSkipNotification");
        }
    }
}