using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Menus;
using Ts_Core.Services.BuildingRelated;

namespace Ts_Core.Patches
{
    /// <summary>
    /// Building描画時にTsCoreの条件付きDrawLayerを追加します。
    /// </summary>
    internal static class BuildingDrawLayerPatch
    {
        //----------------------------------------
        // Patch適用
        //----------------------------------------

        /// <summary>
        /// Building描画処理にHarmonyパッチを適用します。
        /// </summary>
        public static void Apply(
            Harmony harmony)
        {
            PatchDraw(
                harmony);

            PatchDrawBackground(
                harmony);

            PatchDrawInMenu(
                harmony);
        }

        //----------------------------------------
        // Foreground DrawLayer
        //----------------------------------------

        private static void PatchDraw(
            Harmony harmony)
        {
            var method =
                AccessTools.Method(
                    typeof(Building),
                    nameof(Building.draw),
                    new[]
                    {
                        typeof(SpriteBatch)
                    });

            if (method == null)
                return;

            harmony.Patch(
                method,
                postfix: new HarmonyMethod(
                    typeof(BuildingDrawLayerPatch),
                    nameof(DrawPostfix)));
        }

        /// <summary>
        /// Building本体の描画後に前面DrawLayerを追加します。
        /// </summary>
        private static void DrawPostfix(
            Building __instance,
            SpriteBatch b)
        {
            BuildingDrawLayerService.DrawLayers(
                __instance,
                b,
                drawInBackground: false);
        }

        //----------------------------------------
        // Background DrawLayer
        //----------------------------------------

        private static void PatchDrawBackground(
            Harmony harmony)
        {
            var method =
                AccessTools.Method(
                    typeof(Building),
                    nameof(Building.drawBackground),
                    new[]
                    {
                        typeof(SpriteBatch)
                    });

            if (method == null)
                return;

            harmony.Patch(
                method,
                postfix: new HarmonyMethod(
                    typeof(BuildingDrawLayerPatch),
                    nameof(DrawBackgroundPostfix)));
        }

        /// <summary>
        /// Building背景の描画後に背景DrawLayerを追加します。
        /// </summary>
        private static void DrawBackgroundPostfix(
            Building __instance,
            SpriteBatch b)
        {
            BuildingDrawLayerService.DrawLayers(
                __instance,
                b,
                drawInBackground: true);
        }

        //----------------------------------------
        // 建設メニューDrawLayer
        //----------------------------------------

        private static void PatchDrawInMenu(
            Harmony harmony)
        {
            var method =
                AccessTools.Method(
                    typeof(Building),
                    nameof(Building.drawInMenu),
                    new[]
                    {
                        typeof(SpriteBatch),
                        typeof(int),
                        typeof(int)
                    });

            if (method == null)
                return;

            harmony.Patch(
                method,
                postfix: new HarmonyMethod(
                    typeof(BuildingDrawLayerPatch),
                    nameof(DrawInMenuPostfix)));
        }

        /// <summary>
        /// 建設メニューのBuilding描画後に
        /// TsCore DrawLayerを追加します。
        /// </summary>
        private static void DrawInMenuPostfix(
            Building __instance,
            SpriteBatch b,
            int x,
            int y)
        {
            if (Game1.activeClickableMenu
                is not CarpenterMenu carpenterMenu)
            {
                return;
            }

            BuildingDrawLayerService.DrawLayersInMenu(
                __instance,
                b,
                x,
                y,
                carpenterMenu.TargetLocation);
        }
    }
}