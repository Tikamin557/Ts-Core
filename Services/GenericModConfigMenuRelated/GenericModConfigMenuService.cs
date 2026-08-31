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
        }
    }
}