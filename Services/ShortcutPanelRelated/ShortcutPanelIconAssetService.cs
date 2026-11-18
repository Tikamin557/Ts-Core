using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace Ts_Core.Services.ShortcutPanelRelated
{
    /// <summary>
    /// ショートカットパネルの標準アイコンをゲームアセットとして公開します。
    /// Content Patcherなどによる差し替え後のテクスチャはGameContent経由で取得します。
    /// </summary>
    internal static class ShortcutPanelIconAssetService
    {
        private const string AssetPrefix =
            "Mods/Tikamin557.TsCore/ShortcutPanel/";

        private static IModHelper helper = null!;

        private static readonly HashSet<string> IconNames = new(StringComparer.OrdinalIgnoreCase)
        {
            "CjbCheatsMenu", "GameMenu", "GameMenuAnimals", "GameMenuCollections",
            "GameMenuCrafting", "GameMenuExit", "GameMenuInventory", "GameMenuMap",
            "GameMenuOptions", "GameMenuSkills", "GameMenuSocial", "GameMenuSpecialItems",
            "GmcmSelectMod", "GmcmTopMenu", "MissingShortcut", "PsrRoomPresets",
            "ScreenshotCurrentScreen", "ScreenshotFullMap", "SmapiConsole",
            "TerrainFeatures", "TimeSkip", "TimeSkipDuration"
        };

        /// <summary>アセット要求イベントを登録する前に呼び出します。</summary>
        internal static void Initialize(IModHelper modHelper)
        {
            helper = modHelper;
        }

        //----------------------------------------
        // Asset Requested
        //----------------------------------------

        /// <summary>既存のPNGを各ゲームアセットの標準画像として提供します。</summary>
        internal static void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            // 言語別アセットは提供せず、CPのLoadが適用される共通アセットへフォールバックさせます。
            if (e.Name.LocaleCode != null)
                return;

            string name = e.Name.Name;
            if (!name.StartsWith(AssetPrefix, StringComparison.OrdinalIgnoreCase))
                return;

            string iconName = name.Substring(AssetPrefix.Length);
            if (!IconNames.Contains(iconName))
                return;

            e.LoadFromModFile<Texture2D>(
                "assets/ShortcutPanel/" + iconName + ".png",
                AssetLoadPriority.Low);
        }

        //----------------------------------------
        // Icon Texture
        //----------------------------------------

        /// <summary>外部Modの変更を含む現在のアイコンを取得します。</summary>
        internal static Texture2D GetTexture(string iconName)
        {
            return helper.GameContent.Load<Texture2D>(AssetPrefix + iconName);
        }
    }
}
