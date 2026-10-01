using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using Ts_Core.Services.Relationship;

namespace Ts_Core.Services.PolyamorySweetRoomsRelated
{
    /// <summary>
    /// PSR配偶者部屋Preset設定画面の利用可否判定と画面表示をまとめるサービスです。
    /// Shortcut PanelとGMCMの両方から同じ判定・画面を利用します。
    /// </summary>
    internal static class PsrRoomPresetUiService
    {
        //----------------------------------------
        // PSR / 依存サービス
        //----------------------------------------

        /// <summary>Polyamory Sweet RoomsのSMAPI UniqueIDです。</summary>
        private const string PsrModId = "ApryllForever.PolyamorySweetRooms";
        private static ITranslationHelper translation = null!;
        private static PartnerService partnerService = null!;
        private static IModRegistry modRegistry = null!;

        //----------------------------------------
        // 初期化
        //----------------------------------------

        /// <summary>設定画面で使用する翻訳・配偶者情報・Mod Registryを保持します。</summary>
        internal static void Initialize(ITranslationHelper translationHelper, PartnerService service, IModRegistry registry)
        {
            translation = translationHelper;
            partnerService = service;
            modRegistry = registry;
        }

        //----------------------------------------
        // 利用可否判定
        //----------------------------------------

        /// <summary>Polyamory Sweet Roomsが現在ロードされているかを返します。</summary>
        internal static bool IsPsrInstalled()
        {
            return modRegistry.IsLoaded(PsrModId);
        }

        /// <summary>現在のData Assetに有効なPresetが1件以上あるかを返します。</summary>
        internal static bool HasAvailablePresets()
        {
            return PsrRoomContentService.GetPresets().Count > 0;
        }

        /// <summary>PSR導入済みかつ利用可能なPresetがあるかを返します。</summary>
        internal static bool IsAvailable()
        {
            return IsPsrInstalled()
                && HasAvailablePresets();
        }

        //----------------------------------------
        // 設定画面を開く
        //----------------------------------------

        /// <summary>通常の独立メニューとしてPSR設定画面を開きます。Shortcut Panel用です。</summary>
        internal static void Open()
        {
            if (!Context.IsWorldReady
                || !IsAvailable())
            {
                return;
            }

            Game1.activeClickableMenu = new PsrRoomPresetMenu(translation, partnerService, modRegistry);
        }

        /// <summary>
        /// 現在のメニューの子メニューとしてPSR設定画面を開きます。
        /// GMCMの内部状態を壊さず設定画面へ遷移するために使用します。
        /// </summary>
        internal static void OpenAsChildMenu(IClickableMenu parentMenu)
        {
            if (!Context.IsWorldReady
                || !IsAvailable())
            {
                return;
            }

            parentMenu.SetChildMenu(
                new PsrRoomPresetMenu(
                    translation,
                    partnerService,
                    modRegistry));
        }
    }
}
