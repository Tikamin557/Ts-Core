using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System.Reflection;
using Ts_Core.Interfaces;
using Ts_Core.Models.ContentPatcherRelated;
using Ts_Core.Services.PolyamorySweetRoomsRelated;

namespace Ts_Core.Services.ContentPatcherRelated
{
    /// <summary>
    /// <c>TsCore/PsrRoomPresetButtons</c> に登録された定義を使用して、
    /// Content Patcher Content PackのGMCMへPSR Room Presets設定画面を開くボタンを追加します。
    /// </summary>
    internal static class ContentPatcherPsrRoomPresetButtonService
    {
        private const int ButtonWidth = 360;
        private const int ButtonHeight = 64;

        private static IModHelper? Helper;
        private static IGenericModConfigMenuApi? GmcmApi;
        private static bool Initialized;

        private static IManifest? RegisteringManifest;
        private static readonly List<(string Id, PsrRoomPresetButtonModel Model)> PendingDefinitions = new();
        private static bool IsAddingOption;

        private static Rectangle ButtonBounds;
        private static long ButtonLastDrawTick;
        private static string? ButtonContentPackId;
        private static PsrRoomPresetButtonModel? ButtonModel;
        private static string? ButtonHoverText;
        private static long ButtonHoverDrawTick;
        private static bool OpenPending;

        /// <summary>入力監視を初期化します。</summary>
        internal static void Initialize(IModHelper helper)
        {
            if (Initialized)
                return;

            Initialized = true;
            Helper = helper;

            helper.Events.Input.ButtonPressed += OnButtonPressed;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            helper.Events.Display.RenderedActiveMenu += OnRenderedActiveMenu;
        }

        /// <summary>Content Patcherが対象Content PackのGMCMを登録する直前にボタン定義を準備します。</summary>
        internal static void BeginConfigMenuRegistration(
            object rawContentPack,
            IModHelper helper,
            IMonitor monitor)
        {
            ClearConfigMenuRegistration();

            if (rawContentPack is not IContentPack contentPack)
                return;

            GmcmApi ??= helper.ModRegistry.GetApi<IGenericModConfigMenuApi>(
                "spacechase0.GenericModConfigMenu");

            if (GmcmApi == null)
                return;

            Dictionary<string, PsrRoomPresetButtonModel>? definitions = LoadDefinitions(helper, monitor);
            if (definitions == null)
                return;

            foreach ((string id, PsrRoomPresetButtonModel model) in definitions)
            {
                if (string.Equals(
                    model.ContentPackId,
                    contentPack.Manifest.UniqueID,
                    StringComparison.OrdinalIgnoreCase))
                {
                    PendingDefinitions.Add((id, model));
                }
            }

            if (PendingDefinitions.Count > 0)
                RegisteringManifest = contentPack.Manifest;
        }

        /// <summary>BeforeField / AfterFieldを見つけられなかったボタンをGMCM末尾へ追加します。</summary>
        internal static void CompleteConfigMenuRegistration(IModHelper helper, IMonitor monitor)
        {
            try
            {
                foreach ((string id, PsrRoomPresetButtonModel model) in PendingDefinitions.ToArray())
                {
                    AddGmcmOption(id, model, helper);

                    string targetField = !string.IsNullOrWhiteSpace(model.BeforeField)
                        ? model.BeforeField
                        : model.AfterField;

                    if (!string.IsNullOrWhiteSpace(targetField))
                    {
                        monitor.Log(
                            $"Could not find GMCM field '{targetField}' for PSR Room Presets button '{id}'. " +
                            "The button was added at the end of the menu.",
                            LogLevel.Trace);
                    }
                }
            }
            finally
            {
                ClearConfigMenuRegistration();
            }
        }

        /// <summary>Content PatcherがConfig項目を追加する直前に、BeforeFieldで指定されたボタンを登録します。</summary>
        internal static void BeforeContentPatcherFieldAdded(string fieldName)
        {
            if (IsAddingOption
                || RegisteringManifest == null
                || PendingDefinitions.Count == 0
                || Helper == null)
            {
                return;
            }

            foreach ((string id, PsrRoomPresetButtonModel model) in PendingDefinitions.ToArray())
            {
                if (!string.Equals(model.BeforeField, fieldName, StringComparison.OrdinalIgnoreCase))
                    continue;

                AddGmcmOption(id, model, Helper);
                PendingDefinitions.Remove((id, model));
            }
        }

        /// <summary>Position Pickerが監視しているContent Patcher AddField()から項目追加を通知されます。</summary>
        internal static void AfterContentPatcherFieldAdded(string fieldName)
        {
            if (IsAddingOption
                || RegisteringManifest == null
                || PendingDefinitions.Count == 0
                || Helper == null)
            {
                return;
            }

            foreach ((string id, PsrRoomPresetButtonModel model) in PendingDefinitions.ToArray())
            {
                if (!string.IsNullOrWhiteSpace(model.BeforeField)
                    || !string.Equals(model.AfterField, fieldName, StringComparison.OrdinalIgnoreCase))
                    continue;

                AddGmcmOption(id, model, Helper);
                PendingDefinitions.Remove((id, model));
            }
        }

        /// <summary>対象Content PackのGMCMへPSR Room Presetsボタンを追加します。</summary>
        private static void AddGmcmOption(string id, PsrRoomPresetButtonModel model, IModHelper helper)
        {
            if (GmcmApi == null || RegisteringManifest == null)
                return;

            try
            {
                IsAddingOption = true;

                GmcmApi.AddComplexOption(
                    RegisteringManifest,
                    name: () => GetText(model.GMCM_Name, "config.PsrRoomPresets.name", helper),
                    draw: (spriteBatch, position) => DrawButton(spriteBatch, position, model),
                    tooltip: () => GetText(model.GMCM_Description, "config.PsrRoomPresets.description", helper),
                    height: () => ButtonHeight,
                    fieldId: $"TsCore.PsrRoomPresets.{id}");
            }
            finally
            {
                IsAddingOption = false;
            }
        }

        /// <summary>GMCM内のボタンを描画します。</summary>
        private static void DrawButton(SpriteBatch spriteBatch, Vector2 position, PsrRoomPresetButtonModel model)
        {
            ButtonBounds = new Rectangle((int)position.X, (int)position.Y, ButtonWidth, ButtonHeight);
            ButtonLastDrawTick = Game1.ticks;
            ButtonContentPackId = model.ContentPackId;
            ButtonModel = model;

            bool available = Context.IsWorldReady && PsrRoomPresetUiService.IsAvailable();
            bool hover = ButtonBounds.Contains(Game1.getMousePosition());
            float alpha = available ? (hover ? 1f : 0.9f) : 0.45f;

            IClickableMenu.drawTextureBox(
                spriteBatch,
                ButtonBounds.X,
                ButtonBounds.Y,
                ButtonBounds.Width,
                ButtonBounds.Height,
                available ? Color.White * alpha : Color.White);

            if (!available)
            {
                spriteBatch.Draw(
                    Game1.fadeToBlackRect,
                    new Rectangle(
                        ButtonBounds.X + 4,
                        ButtonBounds.Y + 4,
                        ButtonBounds.Width - 8,
                        ButtonBounds.Height - 8),
                    Color.White * 0.45f);
            }

            string text = Helper != null
                ? GetText(model.GMCM_Button, "config.PsrRoomPresets.button", Helper)
                : "Open";

            Vector2 textSize = Game1.smallFont.MeasureString(text);
            spriteBatch.DrawString(
                Game1.smallFont,
                text,
                new Vector2(
                    ButtonBounds.Center.X - textSize.X / 2f,
                    ButtonBounds.Center.Y - textSize.Y / 2f),
                Game1.textColor * alpha);

            // GMCM標準Tooltipとは別に、ボタン上では現在の利用不可理由を表示します。
            // 無効状態のComplexOptionでもホバー説明を確認できるよう、最前面描画を予約します。
            if (hover && Helper != null && !IsGmcmDropdownActiveOrRecentlyClosed())
            {
                ButtonHoverText = GetButtonTooltip(model, Helper);
                ButtonHoverDrawTick = Game1.ticks;
            }
        }

        /// <summary>PSR Room Presetsボタンの現在状態に応じたHover Textを返します。</summary>
        private static string GetButtonTooltip(PsrRoomPresetButtonModel model, IModHelper helper)
        {
            if (!PsrRoomPresetUiService.IsPsrInstalled())
            {
                return GetText(
                    model.GMCM_PsrRequired,
                    "shortcutPanel.PsrRoomPresets.psrRequired",
                    helper);
            }

            if (!Context.IsWorldReady)
            {
                return GetText(
                    model.GMCM_WorldRequired,
                    "config.PsrRoomPresets.worldRequired",
                    helper);
            }

            if (!PsrRoomPresetUiService.HasAvailablePresets())
            {
                return GetText(
                    model.GMCM_NoPreset,
                    "config.PsrRoomPresets.noPreset",
                    helper);
            }

            return GetText(
                model.GMCM_Description,
                "config.PsrRoomPresets.description",
                helper);
        }

        /// <summary>ボタンの左クリックを検出し、マウスを離した後にPSR画面を開く予約をします。</summary>
        private static void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (e.Button != SButton.MouseLeft
                || ButtonModel == null
                || string.IsNullOrWhiteSpace(ButtonContentPackId)
                || Game1.ticks > ButtonLastDrawTick + 1
                || GmcmApi == null)
            {
                return;
            }

            if (!GmcmApi.TryGetCurrentMenu(out IManifest currentMod, out _)
                || !string.Equals(currentMod.UniqueID, ButtonContentPackId, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            Vector2 cursorPosition = Utility.ModifyCoordinatesForUIScale(e.Cursor.ScreenPixels);
            if (!ButtonBounds.Contains(cursorPosition.ToPoint())
                || !Context.IsWorldReady
                || !PsrRoomPresetUiService.IsAvailable())
            {
                return;
            }

            // GMCMのドロップダウン操作中はPSR画面を開かない。
            if (IsGmcmDropdownActiveOrRecentlyClosed())
                return;

            Game1.playSound("smallSelect");
            OpenPending = true;
        }

        /// <summary>クリック貫通を防ぐため、左ボタンが離された後にPSR画面を子メニューとして開きます。</summary>
        private static void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            if (!OpenPending || Mouse.GetState().LeftButton != ButtonState.Released)
                return;

            OpenPending = false;

            if (IsGmcmDropdownActiveOrRecentlyClosed())
                return;

            if (Game1.activeClickableMenu == null
                || GmcmApi == null
                || string.IsNullOrWhiteSpace(ButtonContentPackId)
                || !GmcmApi.TryGetCurrentMenu(out IManifest currentMod, out _)
                || !string.Equals(currentMod.UniqueID, ButtonContentPackId, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            PsrRoomPresetUiService.OpenAsChildMenu(Game1.activeClickableMenu);
        }


        /// <summary>GMCMの通常Tooltipより後に、PSR Room Presetsボタン用Hover Textを描画します。</summary>
        private static void OnRenderedActiveMenu(object? sender, RenderedActiveMenuEventArgs e)
        {
            if (ButtonHoverDrawTick != Game1.ticks
                || string.IsNullOrWhiteSpace(ButtonHoverText)
                || GmcmApi == null
                || string.IsNullOrWhiteSpace(ButtonContentPackId))
            {
                return;
            }

            string text = ButtonHoverText;
            ButtonHoverText = null;

            if (!GmcmApi.TryGetCurrentMenu(out IManifest currentMod, out _)
                || !string.Equals(currentMod.UniqueID, ButtonContentPackId, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (IsGmcmDropdownActiveOrRecentlyClosed())
                return;

            DrawButtonHoverText(e.SpriteBatch, text);
        }

        /// <summary>
        /// GMCMのドロップダウンが開いている間と閉じた直後は、背後のボタンへの操作を防ぎます。
        /// GMCMの内部構造が変更された場合は、通常のボタン処理を維持します。
        /// </summary>
        private static bool IsGmcmDropdownActiveOrRecentlyClosed()
        {
            try
            {
                Assembly? assembly = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(item => item.GetName().Name == "GenericModConfigMenu");
                Type? dropdownType = assembly?.GetType("SpaceShared.UI.Dropdown");
                if (dropdownType == null)
                    return false;

                const BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
                object? active = dropdownType.GetField("ActiveDropdown", flags)?.GetValue(null);
                object? recent = dropdownType.GetField("SinceDropdownWasActive", flags)?.GetValue(null);

                return active != null || (recent is int ticks && ticks > 0);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>PSR Room Presetsボタン用Hover Textを画面内に収まる位置へ描画します。</summary>
        private static void DrawButtonHoverText(SpriteBatch spriteBatch, string text)
        {
            const int padding = 16;
            const int screenMargin = 8;
            const int cursorOffset = 32;
            const int maxTextWidth = 520;

            string wrappedText = Game1.parseText(text, Game1.smallFont, maxTextWidth);
            Vector2 textSize = Game1.smallFont.MeasureString(wrappedText);
            int boxWidth = (int)MathF.Ceiling(textSize.X) + padding * 2;
            int boxHeight = (int)MathF.Ceiling(textSize.Y) + padding * 2;
            Point mousePosition = Game1.getMousePosition();

            int hoverX = mousePosition.X - boxWidth - cursorOffset;
            int hoverY = mousePosition.Y + cursorOffset;

            if (hoverX < screenMargin)
                hoverX = mousePosition.X + cursorOffset;

            hoverX = Math.Max(screenMargin, hoverX);
            hoverY = Math.Max(screenMargin, hoverY);

            if (hoverX + boxWidth > Game1.uiViewport.Width - screenMargin)
                hoverX = Game1.uiViewport.Width - screenMargin - boxWidth;

            if (hoverY + boxHeight > Game1.uiViewport.Height - screenMargin)
                hoverY = mousePosition.Y - cursorOffset - boxHeight;

            hoverY = Math.Max(screenMargin, hoverY);

            IClickableMenu.drawTextureBox(
                spriteBatch,
                Game1.menuTexture,
                new Rectangle(0, 256, 60, 60),
                hoverX,
                hoverY,
                boxWidth,
                boxHeight,
                Color.White,
                1f,
                drawShadow: true);

            spriteBatch.DrawString(
                Game1.smallFont,
                wrappedText,
                new Vector2(hoverX + padding, hoverY + padding),
                Game1.textColor);
        }

        /// <summary>指定Content Pack用のPSR Room Presetsボタン定義が存在するか確認します。</summary>
        internal static bool HasDefinitionsForContentPack(string contentPackId, IModHelper helper, IMonitor monitor)
        {
            Dictionary<string, PsrRoomPresetButtonModel>? definitions = LoadDefinitions(helper, monitor);
            if (definitions == null)
                return false;

            return definitions.Values.Any(model =>
                string.Equals(
                    model.ContentPackId,
                    contentPackId,
                    StringComparison.OrdinalIgnoreCase));
        }

        private static Dictionary<string, PsrRoomPresetButtonModel>? LoadDefinitions(IModHelper helper, IMonitor monitor)
        {
            try
            {
                return helper.GameContent.Load<Dictionary<string, PsrRoomPresetButtonModel>>(
                    PsrRoomPresetButtonDataService.AssetName);
            }
            catch (Exception ex)
            {
                monitor.Log(
                    $"Failed to load '{PsrRoomPresetButtonDataService.AssetName}'.\n{ex}",
                    LogLevel.Error);
                return null;
            }
        }

        private static string GetText(string customText, string fallbackKey, IModHelper helper)
        {
            return !string.IsNullOrWhiteSpace(customText)
                ? customText
                : helper.Translation.Get(fallbackKey);
        }

        private static void ClearConfigMenuRegistration()
        {
            RegisteringManifest = null;
            PendingDefinitions.Clear();
            IsAddingOption = false;
        }
    }
}
