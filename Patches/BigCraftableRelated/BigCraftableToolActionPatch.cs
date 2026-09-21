using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Tools;
using System.Reflection;
using Ts_Core.Services.BigCraftableRelated;

namespace Ts_Core.Patches.BigCraftableRelated
{
    /// <summary>
    /// BigCraftableExtensionの仮想Collisionに対する
    /// Tool処理を補完するPatchです。
    /// </summary>
    internal static class BigCraftableToolActionPatch
    {
        private static readonly MethodInfo? TilesAffectedMethod =
            AccessTools.Method(
                typeof(Tool),
                "tilesAffected",
                new[]
                {
                    typeof(Vector2),
                    typeof(int),
                    typeof(Farmer)
                });

        /// <summary>
        /// Patchを適用します。
        /// </summary>
        public static void Apply(Harmony harmony)
        {
            //----------------------------------------
            // Pickaxe
            //----------------------------------------

            PatchSingleTileTool(
                harmony,
                typeof(Pickaxe));

            //----------------------------------------
            // Axe
            //----------------------------------------

            PatchSingleTileTool(
                harmony,
                typeof(Axe));

            //----------------------------------------
            // Hoe
            //----------------------------------------

            MethodInfo? hoeMethod =
                AccessTools.Method(
                    typeof(Hoe),
                    nameof(Hoe.DoFunction),
                    new[]
                    {
                        typeof(GameLocation),
                        typeof(int),
                        typeof(int),
                        typeof(int),
                        typeof(Farmer)
                    });

            if (hoeMethod != null)
            {
                harmony.Patch(
                    hoeMethod,
                    prefix: new HarmonyMethod(
                        typeof(BigCraftableToolActionPatch),
                        nameof(HoePrefix)),
                    postfix: new HarmonyMethod(
                        typeof(BigCraftableToolActionPatch),
                        nameof(HoePostfix)));
            }
        }

        /// <summary>
        /// 単一タイルを対象とするToolにPatchを適用します。
        /// </summary>
        private static void PatchSingleTileTool(
            Harmony harmony,
            System.Type toolType)
        {
            MethodInfo? method =
                AccessTools.Method(
                    toolType,
                    nameof(Tool.DoFunction),
                    new[]
                    {
                        typeof(GameLocation),
                        typeof(int),
                        typeof(int),
                        typeof(int),
                        typeof(Farmer)
                    });

            if (method == null)
                return;

            harmony.Patch(
                method,
                prefix: new HarmonyMethod(
                    typeof(BigCraftableToolActionPatch),
                    nameof(SingleTilePrefix)),
                postfix: new HarmonyMethod(
                    typeof(BigCraftableToolActionPatch),
                    nameof(SingleTilePostfix)));
        }

        /// <summary>
        /// 単一タイルToolの使用前に
        /// 仮想Collisionか確認します。
        /// </summary>
        private static void SingleTilePrefix(
            GameLocation location,
            int x,
            int y,
            out Vector2? __state)
        {
            __state = null;

            Vector2 targetTile =
                new Vector2(
                    x / 64,
                    y / 64);

            //----------------------------------------
            // 実Objectがある場合はVanillaに任せる
            //----------------------------------------

            if (location.Objects.ContainsKey(targetTile))
                return;

            //----------------------------------------
            // 仮想Collisionか確認
            //----------------------------------------

            if (!BigCraftableExtensionCollisionService
                .TryGetCollisionObject(
                    location,
                    targetTile,
                    out _))
            {
                return;
            }

            __state = targetTile;
        }

        /// <summary>
        /// 単一タイルToolでVanillaが処理できない
        /// 仮想CollisionにTool処理を適用します。
        /// </summary>
        private static void SingleTilePostfix(
            Tool __instance,
            GameLocation location,
            Farmer who,
            Vector2? __state)
        {
            if (!__state.HasValue)
                return;

            BigCraftableExtensionToolService
                .TryPerformToolAction(
                    location,
                    __state.Value,
                    __instance,
                    who);
        }

        /// <summary>
        /// Hoe使用前に対象となる仮想Collisionを取得し、
        /// Vanilla処理中の占有判定から一時的に除外します。
        /// </summary>
        private static void HoePrefix(
            Hoe __instance,
            GameLocation location,
            int x,
            int y,
            Farmer who,
            out List<Vector2>? __state)
        {
            __state = null;

            //----------------------------------------
            // 前回の一時状態を解除
            //----------------------------------------

            BigCraftableExtensionToolService
                .EndIgnoreCollision();

            if (TilesAffectedMethod == null)
                return;

            Vector2 initialTile =
                new Vector2(
                    x / 64,
                    y / 64);

            int power =
                who.toolPower.Value;

            object? result =
                TilesAffectedMethod.Invoke(
                    __instance,
                    new object[]
                    {
                        initialTile,
                        power,
                        who
                    });

            if (result is not List<Vector2> tileLocations)
                return;

            List<Vector2> virtualTiles =
                new List<Vector2>();

            foreach (Vector2 tileLocation in tileLocations)
            {
                //----------------------------------------
                // 実ObjectはVanillaに任せる
                //----------------------------------------

                if (location.Objects.ContainsKey(tileLocation))
                    continue;

                //----------------------------------------
                // 仮想Collisionか確認
                //----------------------------------------

                if (!BigCraftableExtensionCollisionService
                    .TryGetCollisionObject(
                        location,
                        tileLocation,
                        out _))
                {
                    continue;
                }

                virtualTiles.Add(
                    tileLocation);
            }

            if (virtualTiles.Count == 0)
                return;

            //----------------------------------------
            // Hoe処理中だけ占有判定から除外
            //----------------------------------------

            BigCraftableExtensionToolService
                .BeginIgnoreCollision(
                    virtualTiles);

            __state =
                virtualTiles;
        }

        /// <summary>
        /// HoeでVanillaが処理できない仮想Collisionに
        /// Tool処理を適用します。
        /// </summary>
        private static void HoePostfix(
            Hoe __instance,
            GameLocation location,
            Farmer who,
            List<Vector2>? __state)
        {
            try
            {
                if (__state == null)
                    return;

                HashSet<StardewValley.Object> processedObjects =
                    new HashSet<StardewValley.Object>();

                foreach (Vector2 tileLocation in __state)
                {
                    //----------------------------------------
                    // Vanilla処理中に状態が変化した場合
                    //----------------------------------------

                    if (location.Objects.ContainsKey(tileLocation))
                        continue;

                    if (!BigCraftableExtensionCollisionService
                        .TryGetCollisionObject(
                            location,
                            tileLocation,
                            out StardewValley.Object obj))
                    {
                        continue;
                    }

                    //----------------------------------------
                    // 同じBigCraftableは1回だけ処理
                    //----------------------------------------

                    if (!processedObjects.Add(obj))
                        continue;

                    BigCraftableExtensionToolService
                        .TryPerformToolAction(
                            location,
                            tileLocation,
                            __instance,
                            who);
                }
            }
            finally
            {
                //----------------------------------------
                // 一時除外を必ず解除
                //----------------------------------------

                BigCraftableExtensionToolService
                    .EndIgnoreCollision();
            }
        }
    }
}