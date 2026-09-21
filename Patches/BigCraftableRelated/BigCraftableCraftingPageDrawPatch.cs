using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;
using Ts_Core.Services.BigCraftableRelated;

namespace Ts_Core.Patches.BigCraftableRelated
{
    /// <summary>
    /// Crafting PageのカスタムサイズBigCraftableを
    /// 表示領域の横中央・下揃えで描画するPatchです。
    /// </summary>
    public static class BigCraftableCraftingPageDrawPatch
    {
        //----------------------------------------
        // 対象Component
        //----------------------------------------

        private static readonly Dictionary<
            ClickableTextureComponent,
            StardewValley.Object> TargetComponents =
                new();

        /// <summary>
        /// Patchを適用します。
        /// </summary>
        public static void Apply(
            Harmony harmony)
        {
            harmony.Patch(
                original:
                    AccessTools.Method(
                        typeof(ClickableTextureComponent),
                        nameof(ClickableTextureComponent.draw),
                        new[]
                        {
                    typeof(SpriteBatch),
                    typeof(Color),
                    typeof(float),
                    typeof(int),
                    typeof(int),
                    typeof(int)
                        }),
                prefix:
                    new HarmonyMethod(
                        typeof(BigCraftableCraftingPageDrawPatch),
                        nameof(DrawPrefix)),
                postfix:
                    new HarmonyMethod(
                        typeof(BigCraftableCraftingPageDrawPatch),
                        nameof(DrawPostfix)));
        }

        /// <summary>
        /// Crafting Pageの再配置時に
        /// 対象Componentをリセットします。
        /// </summary>
        public static void ClearTargets()
        {
            TargetComponents.Clear();
        }

        /// <summary>
        /// 中央配置の対象Componentを登録します。
        /// </summary>
        public static void RegisterTarget(
            ClickableTextureComponent component,
            StardewValley.Object obj)
        {
            TargetComponents[component] =
                obj;
        }

        /// <summary>
        /// 描画位置を横中央・下揃えに補正します。
        /// </summary>
        private static void DrawPrefix(
            ClickableTextureComponent __instance,
            SpriteBatch b,
            Color c,
            float layerDepth,
            ref int xOffset,
            ref int yOffset)
        {
            //----------------------------------------
            // 対象確認
            //----------------------------------------

            if (!TargetComponents.TryGetValue(
                __instance,
                out StardewValley.Object? obj))
            {
                return;
            }

            if (!__instance.visible
                || __instance.texture == null)
            {
                return;
            }

            //----------------------------------------
            // 描画サイズ確認
            //----------------------------------------

            Rectangle sourceRect =
                __instance.sourceRect;

            float baseScale =
                __instance.baseScale;

            float drawWidth =
                sourceRect.Width
                * baseScale;

            float drawHeight =
                sourceRect.Height
                * baseScale;

            //----------------------------------------
            // 横中央・下配置
            //----------------------------------------

            xOffset +=
                (int)((64f - drawWidth) / 2f);

            yOffset +=
                (int)(128f - drawHeight);

            //----------------------------------------
            // Extension Data
            //----------------------------------------

            if (!BigCraftableExtensionDataService
                .TryGetExtensionData(
                    obj,
                    out _,
                    out var extension))
            {
                return;
            }

            //----------------------------------------
            // Draw Position
            //----------------------------------------

            Vector2 drawPosition =
                new(
                    __instance.bounds.X
                        + xOffset
                        + sourceRect.Width
                        / 2f
                        * baseScale,
                    __instance.bounds.Y
                        + yOffset
                        + sourceRect.Height
                        / 2f
                        * baseScale);

            //----------------------------------------
            // Back Draw Layers
            //----------------------------------------

            BigCraftableDrawLayerService
                .DrawLayersInMenu(
                    obj,
                    b,
                    extension,
                    __instance.texture,
                    drawPosition,
                    __instance.scale,
                    layerDepth - 0.0001f,
                    c,
                    "Back");
        }

        private static void DrawPostfix(
            ClickableTextureComponent __instance,
            SpriteBatch b,
            Color c,
            float layerDepth,
            int xOffset,
            int yOffset)
        {
            //----------------------------------------
            // 対象確認
            //----------------------------------------

            if (!TargetComponents.TryGetValue(
                __instance,
                out StardewValley.Object? obj))
            {
                return;
            }

            if (!__instance.visible
                || __instance.texture == null)
            {
                return;
            }

            //----------------------------------------
            // Extension Data
            //----------------------------------------

            if (!BigCraftableExtensionDataService
                .TryGetExtensionData(
                    obj,
                    out _,
                    out var extension))
            {
                return;
            }

            //----------------------------------------
            // Source Rectangle
            //----------------------------------------

            Rectangle sourceRect =
                __instance.sourceRect;

            //----------------------------------------
            // Draw Position
            //----------------------------------------

            Vector2 drawPosition =
                new(
                    __instance.bounds.X
                        + xOffset
                        + sourceRect.Width
                        / 2f
                        * __instance.baseScale,
                    __instance.bounds.Y
                        + yOffset
                        + sourceRect.Height
                        / 2f
                        * __instance.baseScale);

            //----------------------------------------
            // Front Draw Layers
            //----------------------------------------

            BigCraftableDrawLayerService
                .DrawLayersInMenu(
                    obj,
                    b,
                    extension,
                    __instance.texture,
                    drawPosition,
                    __instance.scale,
                    layerDepth + 0.0001f,
                    c,
                    "Front");
        }
    }
}