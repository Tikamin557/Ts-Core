using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using Ts_Core.Debug;
using Ts_Core.Interfaces;
using Ts_Core.Models;
using Ts_Core.Services.ShortcutPanelRelated;

namespace Ts_Core.Services.GenericModConfigMenuRelated
{
    /// <summary>
    /// T's Core自身の設定を
    /// Generic Mod Config Menuへ登録します。
    /// </summary>
    internal static class GenericModConfigMenuService
    {
        private const int ShortcutDiagnosticButtonWidth = 360;
        private const int ShortcutDiagnosticButtonHeight = 64;

        private static Rectangle shortcutDiagnosticButtonBounds;
        private static int shortcutDiagnosticButtonLastDrawTick;
        private static bool shortcutDiagnosticInputRegistered;

        private const int SmapiConsoleButtonWidth = 360;
        private const int SmapiConsoleButtonHeight = 64;
        private static Rectangle smapiConsoleButtonBounds;
        private static int smapiConsoleButtonLastDrawTick;
        private static bool smapiConsoleInputRegistered;
        private static bool smapiConsoleOpenPending;
        private static int smapiConsoleOpenAfterTick;
        private static ITranslationHelper? smapiConsoleTranslation;

        //----------------------------------------
        // GMCMのMod ID
        //----------------------------------------

        private const string GenericModConfigMenuId =
            "spacechase0.GenericModConfigMenu";

        //----------------------------------------
        // Shortcut Panel 診断ボタン
        //----------------------------------------

        /// <summary>
        /// ショートカットパネル位置診断用のGMCM入力項目を登録します。
        /// </summary>
        private static void RegisterShortcutDiagnosticInput(
            IModHelper helper,
            IMonitor monitor,
            IGenericModConfigMenuApi api,
            IManifest manifest)
        {
            if (shortcutDiagnosticInputRegistered)
                return;

            shortcutDiagnosticInputRegistered = true;

            helper.Events.Input.ButtonPressed +=
                (sender, e) =>
                {
                    if (e.Button != SButton.MouseLeft)
                        return;

                    if (Game1.ticks > shortcutDiagnosticButtonLastDrawTick + 1)
                        return;

                    if (!api.TryGetCurrentMenu(
                            out IManifest currentMod,
                            out _ )
                        || currentMod.UniqueID != manifest.UniqueID)
                    {
                        return;
                    }

                    Vector2 cursorPosition =
                        Utility.ModifyCoordinatesForUIScale(
                            e.Cursor.ScreenPixels);

                    if (!shortcutDiagnosticButtonBounds.Contains(
                            cursorPosition.ToPoint()))
                    {
                        return;
                    }

                    DebugShortcutPanelLogger.Log(
                        monitor);

                    Game1.playSound(
                        "smallSelect");
                };
        }

        /// <summary>
        /// ショートカットパネル位置診断ログを出力するGMCMボタンを描画します。
        /// </summary>
        private static void DrawShortcutDiagnosticButton(
            SpriteBatch spriteBatch,
            Vector2 position,
            IModHelper helper)
        {
            shortcutDiagnosticButtonBounds =
                new Rectangle(
                    (int)position.X,
                    (int)position.Y,
                    ShortcutDiagnosticButtonWidth,
                    ShortcutDiagnosticButtonHeight);

            shortcutDiagnosticButtonLastDrawTick =
                Game1.ticks;

            Point mousePosition =
                Game1.getMousePosition();

            bool hover =
                shortcutDiagnosticButtonBounds.Contains(
                    mousePosition);

            IClickableMenu.drawTextureBox(
                spriteBatch,
                shortcutDiagnosticButtonBounds.X,
                shortcutDiagnosticButtonBounds.Y,
                shortcutDiagnosticButtonBounds.Width,
                shortcutDiagnosticButtonBounds.Height,
                hover
                    ? Color.White
                    : Color.White * 0.9f);

            string text =
                helper.Translation.Get(
                    "config.ShortcutPanelDiagnostic.button");

            Vector2 textSize =
                Game1.smallFont.MeasureString(
                    text);

            Vector2 textPosition =
                new Vector2(
                    shortcutDiagnosticButtonBounds.Center.X
                        - textSize.X / 2f,
                    shortcutDiagnosticButtonBounds.Center.Y
                        - textSize.Y / 2f);

            spriteBatch.DrawString(
                Game1.smallFont,
                text,
                textPosition,
                Game1.textColor);
        }

        //----------------------------------------
        // Android用SMAPIコンソール
        //----------------------------------------

        /// <summary>
        /// 現在の実行環境がAndroid版SMAPIかを判定します。
        /// </summary>
        private static bool IsAndroidEnvironment()
        {
            try
            {
                Type game1Type = typeof(Game1);

                return game1Type.GetProperty(
                           "DateTimeScale",
                           System.Reflection.BindingFlags.Public
                           | System.Reflection.BindingFlags.NonPublic
                           | System.Reflection.BindingFlags.Static) != null
                    || game1Type.GetField(
                           "DateTimeScale",
                           System.Reflection.BindingFlags.Public
                           | System.Reflection.BindingFlags.NonPublic
                           | System.Reflection.BindingFlags.Static) != null;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// ゲーム内SMAPIコンソールを開くGMCM項目を登録します。
        /// </summary>
        private static void RegisterSmapiConsoleInput(
            IModHelper helper,
            IMonitor monitor,
            IGenericModConfigMenuApi api,
            IManifest manifest)
        {
            if (smapiConsoleInputRegistered)
                return;

            smapiConsoleInputRegistered = true;

            helper.Events.Input.ButtonPressed +=
                (sender, e) =>
                {
                    if (e.Button != SButton.MouseLeft)
                        return;

                    if (Game1.ticks > smapiConsoleButtonLastDrawTick + 1)
                        return;

                    if (!api.TryGetCurrentMenu(
                            out IManifest currentMod,
                            out _ )
                        || currentMod.UniqueID != manifest.UniqueID)
                    {
                        return;
                    }

                    Vector2 cursorPosition =
                        Utility.ModifyCoordinatesForUIScale(
                            e.Cursor.ScreenPixels);

                    if (!smapiConsoleButtonBounds.Contains(
                            cursorPosition.ToPoint()))
                    {
                        return;
                    }

                    // PCでは案内表示だけにし、クリックしても何もしません。
                    if (!IsAndroidEnvironment())
                        return;

                    Game1.playSound("smallSelect");

                    // GMCMはメニューを閉じた後も、終了処理用の状態を一時的に保持します。
                    // すぐにactiveClickableMenuを置き換えると、Android版GMCMの
                    // UpdateTicking処理がTitleMenuの
                    // 「titleInPosition」フィールドをSMAPIコンソールメニューから取得しようとしてしまいます。
                    // そのため、まずGMCMを通常どおり閉じ、
                    // GMCMの終了処理に必要なtickが経過してからコンソールを開きます。
                    smapiConsoleTranslation = helper.Translation;
                    smapiConsoleOpenPending = true;
                    smapiConsoleOpenAfterTick = Game1.ticks + 2;

                    Game1.activeClickableMenu?.exitThisMenu();
                };

            helper.Events.GameLoop.UpdateTicked +=
                (sender, e) =>
                {
                    if (!smapiConsoleOpenPending
                        || Game1.ticks < smapiConsoleOpenAfterTick)
                    {
                        return;
                    }

                    // AndroidではGMCMがTitleMenuのサブメニューとして表示されるため、
                    // そのサブメニューが実際に閉じるまで待ってから
                    // activeClickableMenuを置き換えます。必要なら次のtickまで待機を続けます。
                    if (TitleMenu.subMenu != null)
                        return;

                    smapiConsoleOpenPending = false;

                    if (smapiConsoleTranslation == null)
                        return;

                    ShortcutPanelSmapiConsoleMenu consoleMenu =
                        new ShortcutPanelSmapiConsoleMenu(
                            smapiConsoleTranslation);

                    // タイトル画面でも TitleMenu.subMenu には入れません。
                    // GMCM の終了処理と subMenu のクローズを待った後、通常の
                    // activeClickableMenu として開くことで、TitleMenu.subMenu 固有の
                    // 描画スケール変換を受けないようにします。
                    Game1.activeClickableMenu = consoleMenu;
                };
        }

        /// <summary>
        /// ゲーム内SMAPIコンソールを開くGMCMボタンを描画します。PCではAndroid専用であることが分かる無効表示にします。
        /// </summary>
        private static void DrawSmapiConsoleButton(
            SpriteBatch spriteBatch,
            Vector2 position,
            IModHelper helper)
        {
            smapiConsoleButtonBounds =
                new Rectangle(
                    (int)position.X,
                    (int)position.Y,
                    SmapiConsoleButtonWidth,
                    SmapiConsoleButtonHeight);

            smapiConsoleButtonLastDrawTick = Game1.ticks;

            Point mousePosition = Game1.getMousePosition();
            bool hover = smapiConsoleButtonBounds.Contains(mousePosition);

            bool isAvailable =
                IsAndroidEnvironment();

            float alpha =
                isAvailable
                    ? (hover ? 1f : 0.9f)
                    : 0.45f;

            // 無効時にボタン全体へalphaを掛けると、GMCMの影まで透けて
            // ボタン内部に見えてしまいます。背景は不透明で描画し、
            // 上から白い膜を重ねて「無効」の薄い見た目にします。
            IClickableMenu.drawTextureBox(
                spriteBatch,
                smapiConsoleButtonBounds.X,
                smapiConsoleButtonBounds.Y,
                smapiConsoleButtonBounds.Width,
                smapiConsoleButtonBounds.Height,
                isAvailable
                    ? Color.White * alpha
                    : Color.White);

            if (!isAvailable)
            {
                spriteBatch.Draw(
                    Game1.fadeToBlackRect,
                    new Rectangle(
                        smapiConsoleButtonBounds.X + 4,
                        smapiConsoleButtonBounds.Y + 4,
                        smapiConsoleButtonBounds.Width - 8,
                        smapiConsoleButtonBounds.Height - 8),
                    Color.White * 0.45f);
            }

            string text = helper.Translation.Get(
                "config.SmapiConsole.button");

            Vector2 textSize = Game1.smallFont.MeasureString(text);
            Vector2 textPosition =
                new Vector2(
                    smapiConsoleButtonBounds.Center.X - textSize.X / 2f,
                    smapiConsoleButtonBounds.Center.Y - textSize.Y / 2f);

            spriteBatch.DrawString(
                Game1.smallFont,
                text,
                textPosition,
                Game1.textColor * alpha);

            // PCではボタン自体にマウスを乗せた場合も、
            // 左側の項目名と同じAndroid専用の案内を表示します。
            if (!isAvailable && hover)
            {
                IClickableMenu.drawHoverText(
                    spriteBatch,
                    helper.Translation.Get(
                        "config.SmapiConsole.androidOnly"),
                    Game1.smallFont);
            }
        }

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
            IMonitor monitor,
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

            RegisterShortcutDiagnosticInput(
                helper,
                monitor,
                api,
                manifest);

            if (IsAndroidEnvironment())
            {
                RegisterSmapiConsoleInput(
                    helper,
                    monitor,
                    api,
                    manifest);
            }

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
            // Shortcut Panel
            //----------------------------------------

            // 改行追加
            api.AddParagraph(
                manifest,
                text: () => " ");
            api.AddParagraph(
                manifest,
                text: () => " ");

            api.AddSectionTitle(
                manifest,
                text: () =>
                    helper.Translation.Get(
                        "config.ShortcutPanel.name"),
                tooltip: () =>
                    helper.Translation.Get(
                        "config.ShortcutPanel.description"));

            //----------------------------------------
            // Shortcut Panel - 表示
            //----------------------------------------

            api.AddBoolOption(
                manifest,
                getValue: () =>
                    getConfig()
                        .ShortcutPanelEnabled,
                setValue: value =>
                    getConfig()
                        .ShortcutPanelEnabled =
                            value,
                name: () =>
                    helper.Translation.Get(
                        "config.ShortcutPanelEnabled.name"),
                tooltip: () =>
                    helper.Translation.Get(
                        "config.ShortcutPanelEnabled.description"),
                fieldId:
                    "ShortcutPanelEnabled");

            //----------------------------------------
            // Shortcut Panel - 開閉タブサイズ
            //----------------------------------------

            api.AddNumberOption(
                manifest,
                getValue: () =>
                    getConfig()
                        .ShortcutPanelTabScale,
                setValue: value =>
                    getConfig()
                        .ShortcutPanelTabScale =
                            value,
                name: () =>
                    helper.Translation.Get(
                        "config.ShortcutPanelTabScale.name"),
                tooltip: () =>
                    helper.Translation.Get(
                        "config.ShortcutPanelTabScale.description"),
                min:
                    100,
                max:
                    200,
                interval:
                    5,
                formatValue: value =>
                    helper.Translation.Get(
                        "config.ShortcutPanelOpacity.format",
                        new
                        {
                            Value =
                                value
                        }),
                fieldId:
                    "ShortcutPanelTabScale");

            //----------------------------------------
            // Shortcut Panel - パネルサイズ
            //----------------------------------------

            api.AddNumberOption(
                manifest,
                getValue: () =>
                    getConfig()
                        .ShortcutPanelScale,
                setValue: value =>
                    getConfig()
                        .ShortcutPanelScale =
                            value,
                name: () =>
                    helper.Translation.Get(
                        "config.ShortcutPanelScale.name"),
                tooltip: () =>
                    helper.Translation.Get(
                        "config.ShortcutPanelScale.description"),
                min:
                    100,
                max:
                    200,
                interval:
                    5,
                formatValue: value =>
                    helper.Translation.Get(
                        "config.ShortcutPanelOpacity.format",
                        new
                        {
                            Value =
                                value
                        }),
                fieldId:
                    "ShortcutPanelScale");

            //----------------------------------------
            // Shortcut Panel - タブ不透明度
            //----------------------------------------

            api.AddNumberOption(
                manifest,
                getValue: () =>
                    getConfig()
                        .ShortcutPanelTabOpacity,
                setValue: value =>
                    getConfig()
                        .ShortcutPanelTabOpacity =
                            value,
                name: () =>
                    helper.Translation.Get(
                        "config.ShortcutPanelTabOpacity.name"),
                tooltip: () =>
                    helper.Translation.Get(
                        "config.ShortcutPanelTabOpacity.description"),
                min:
                    0,
                max:
                    100,
                interval:
                    5,
                formatValue: value =>
                    helper.Translation.Get(
                        "config.ShortcutPanelOpacity.format",
                        new
                        {
                            Value =
                                value
                        }),
                fieldId:
                    "ShortcutPanelTabOpacity");

            //----------------------------------------
            // Shortcut Panel - メインパネル不透明度
            //----------------------------------------

            api.AddNumberOption(
                manifest,
                getValue: () =>
                    getConfig()
                        .ShortcutPanelOpacity,
                setValue: value =>
                    getConfig()
                        .ShortcutPanelOpacity =
                            value,
                name: () =>
                    helper.Translation.Get(
                        "config.ShortcutPanelOpacity.name"),
                tooltip: () =>
                    helper.Translation.Get(
                        "config.ShortcutPanelOpacity.description"),
                min:
                    0,
                max:
                    100,
                interval:
                    5,
                formatValue: value =>
                    helper.Translation.Get(
                        "config.ShortcutPanelOpacity.format",
                        new
                        {
                            Value =
                                value
                        }),
                fieldId:
                    "ShortcutPanelOpacity");

            //----------------------------------------
            // Shortcut Panel - 位置診断ログ
            //----------------------------------------

            api.AddComplexOption(
                manifest,
                name: () =>
                    helper.Translation.Get(
                        "config.ShortcutPanelDiagnostic.name"),
                draw: (spriteBatch, position) =>
                    DrawShortcutDiagnosticButton(
                        spriteBatch,
                        position,
                        helper),
                tooltip: () =>
                    helper.Translation.Get(
                        "config.ShortcutPanelDiagnostic.description"),
                height: () =>
                    ShortcutDiagnosticButtonHeight + 8,
                fieldId:
                    "ShortcutPanelDiagnostic");

            //----------------------------------------
            // スクリーンショット
            //----------------------------------------

            api.AddComplexOption(
                manifest,
                name: () =>
                    helper.Translation.Get(
                        "config.Screenshot.heading"),
                draw: (spriteBatch, position) =>
                {
                },
                height: () => 36,
                fieldId:
                    "ScreenshotHeading");

            //----------------------------------------
            // スクリーンショット - UI
            //----------------------------------------

            api.AddBoolOption(
                manifest,
                getValue: () =>
                    getConfig().ScreenshotIncludeUi,
                setValue: value =>
                    getConfig().ScreenshotIncludeUi = value,
                name: () =>
                    helper.Translation.Get(
                        "config.ScreenshotIncludeUi.name"),
                tooltip: () =>
                    helper.Translation.Get(
                        "config.ScreenshotIncludeUi.description"),
                fieldId:
                    "ScreenshotIncludeUi");

            //----------------------------------------
            // スクリーンショット - Shortcut Panel
            //----------------------------------------

            api.AddBoolOption(
                manifest,
                getValue: () =>
                    getConfig().ScreenshotIncludeShortcutPanel,
                setValue: value =>
                    getConfig().ScreenshotIncludeShortcutPanel = value,
                name: () =>
                    helper.Translation.Get(
                        "config.ScreenshotIncludeShortcutPanel.name"),
                tooltip: () =>
                    helper.Translation.Get(
                        "config.ScreenshotIncludeShortcutPanel.description"),
                fieldId:
                    "ScreenshotIncludeShortcutPanel");

            //----------------------------------------
            // スクリーンショット - Mouse Cursor
            //----------------------------------------

            api.AddBoolOption(
                manifest,
                getValue: () =>
                    getConfig().ScreenshotIncludeMouseCursor,
                setValue: value =>
                    getConfig().ScreenshotIncludeMouseCursor = value,
                name: () =>
                    helper.Translation.Get(
                        "config.ScreenshotIncludeMouseCursor.name"),
                tooltip: () =>
                    helper.Translation.Get(
                        "config.ScreenshotIncludeMouseCursor.description"),
                fieldId:
                    "ScreenshotIncludeMouseCursor");

            //----------------------------------------
            // デバッグ支援機能
            //----------------------------------------

            // 改行追加
            api.AddParagraph(
                manifest,
                text: () => " ");
            api.AddParagraph(
                manifest,
                text: () => " ");

            api.AddSectionTitle(
                manifest,
                text: () =>
                    helper.Translation.Get(
                        "config.DebugSupport.name"),
                tooltip: () =>
                    helper.Translation.Get(
                        "config.DebugSupport.description"));

            //----------------------------------------
            // SMAPIコンソール
            //----------------------------------------

            api.AddComplexOption(
                manifest,
                name: () =>
                    helper.Translation.Get(
                        "config.SmapiConsole.name"),
                draw: (spriteBatch, position) =>
                    DrawSmapiConsoleButton(
                        spriteBatch,
                        position,
                        helper),
                tooltip: () =>
                    IsAndroidEnvironment()
                        ? helper.Translation.Get(
                            "config.SmapiConsole.description")
                        : helper.Translation.Get(
                            "config.SmapiConsole.androidOnly"),
                height: () =>
                    SmapiConsoleButtonHeight + 8,
                fieldId:
                    "SmapiConsole");

            api.AddParagraph(
                manifest,
                text: () => " ");

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

            // 改行追加
            api.AddParagraph(
                manifest,
                text: () => " ");

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
