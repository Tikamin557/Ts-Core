using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using Ts_Core.Services.BigCraftableRelated;

namespace Ts_Core.Patches.BigCraftableRelated
{
    /// <summary>
    /// AndroidのTap-to-Moveで、
    /// BigCraftable ExtensionのCollisionと
    /// PlayerPassableTilesを正しく経路探索へ反映するための互換パッチ。
    ///
    /// Android専用のAStarNodeおよび
    /// GameLocation.isTileOccupiedIgnoreFloorsAndHorseは
    /// PC版には存在しないため、Reflectionで取得する。
    /// </summary>
    internal static class BigCraftableAndroidTapToMovePatch
    {
        private static IMonitor? Monitor;

        private static MethodInfo? IsTileOccupiedIgnoreFloorsAndHorseMethod;

        //----------------------------------------
        // Apply
        //----------------------------------------

        /// <summary>
        /// パッチを適用する。
        /// Android専用クラスまたはメソッドが存在しない環境では
        /// 何もせず終了する。
        /// </summary>
        public static void Apply(
            Harmony harmony,
            IMonitor monitor)
        {
            Monitor = monitor;

            try
            {
                Assembly gameAssembly =
                    typeof(Game1).Assembly;

                //----------------------------------------
                // Android AStarNode
                //----------------------------------------

                Type? aStarNodeType =
                    gameAssembly.GetType(
                        "StardewValley.Mobile.AStarNode",
                        throwOnError: false);

                //----------------------------------------
                // PC版では存在しない
                //----------------------------------------

                if (aStarNodeType == null)
                {
                    return;
                }

                //----------------------------------------
                // Android専用
                // isTileOccupiedIgnoreFloorsAndHorse
                //----------------------------------------

                IsTileOccupiedIgnoreFloorsAndHorseMethod =
                    typeof(GameLocation).GetMethod(
                        "isTileOccupiedIgnoreFloorsAndHorse",
                        BindingFlags.Instance
                        | BindingFlags.Public
                        | BindingFlags.NonPublic,
                        binder: null,
                        types:
                        new[]
                        {
                            typeof(Vector2)
                        },
                        modifiers: null);

                if (IsTileOccupiedIgnoreFloorsAndHorseMethod == null)
                {
                    monitor.Log(
                        "Android Tap-to-Move compatibility patch was skipped: "
                        + "GameLocation.isTileOccupiedIgnoreFloorsAndHorse(Vector2) "
                        + "could not be found.",
                        LogLevel.Warn);

                    return;
                }

                //----------------------------------------
                // AStarNode.TileClear getter
                //----------------------------------------

                PropertyInfo? tileClearProperty =
                    aStarNodeType.GetProperty(
                        "TileClear",
                        BindingFlags.Instance
                        | BindingFlags.Public
                        | BindingFlags.NonPublic);

                MethodInfo? tileClearGetter =
                    tileClearProperty?
                        .GetGetMethod(
                            nonPublic: true);

                if (tileClearGetter == null)
                {
                    monitor.Log(
                        "Android Tap-to-Move compatibility patch was skipped: "
                        + "StardewValley.Mobile.AStarNode.TileClear getter "
                        + "could not be found.",
                        LogLevel.Warn);

                    return;
                }

                //----------------------------------------
                // Harmony Patch
                //----------------------------------------

                harmony.Patch(
                    original: tileClearGetter,
                    transpiler:
                    new HarmonyMethod(
                        typeof(BigCraftableAndroidTapToMovePatch),
                        nameof(Transpiler)));
            }
            catch (Exception ex)
            {
                monitor.Log(
                    "Failed to apply the Android Tap-to-Move compatibility "
                    + $"patch. The game will continue without this patch.\n{ex}",
                    LogLevel.Warn);
            }
        }

        //----------------------------------------
        // Transpiler
        //----------------------------------------

        /// <summary>
        /// AStarNode.TileClear内の
        /// GameLocation.isTileOccupiedIgnoreFloorsAndHorse(Vector2)
        /// 呼び出しだけをTsCoreの補完メソッドへ置き換える。
        /// </summary>
        private static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes =
                instructions.ToList();

            MethodInfo? originalMethod =
                IsTileOccupiedIgnoreFloorsAndHorseMethod;

            MethodInfo? replacementMethod =
                AccessTools.Method(
                    typeof(BigCraftableAndroidTapToMovePatch),
                    nameof(IsTileOccupiedForTapToMove));

            //----------------------------------------
            // Method確認
            //----------------------------------------

            if (originalMethod == null
                || replacementMethod == null)
            {
                Monitor?.Log(
                    "Android Tap-to-Move compatibility patch could not find "
                    + "the required methods. AStarNode.TileClear was left "
                    + "unchanged.",
                    LogLevel.Warn);

                return codes;
            }

            //----------------------------------------
            // 呼び出し置換
            //----------------------------------------

            int replacedCount = 0;

            foreach (CodeInstruction code in codes)
            {
                if (!code.Calls(originalMethod))
                {
                    continue;
                }

                /*
                 * 元のスタック:
                 *
                 *   GameLocation
                 *   Vector2
                 *
                 * Android版:
                 *
                 *   callvirt bool
                 *   GameLocation.isTileOccupiedIgnoreFloorsAndHorse(Vector2)
                 *
                 * これを:
                 *
                 *   call bool
                 *   IsTileOccupiedForTapToMove(GameLocation, Vector2)
                 *
                 * に置き換える。
                 */

                code.opcode =
                    OpCodes.Call;

                code.operand =
                    replacementMethod;

                replacedCount++;
            }

            //----------------------------------------
            // 結果
            //----------------------------------------

            if (replacedCount == 0)
            {
                Monitor?.Log(
                    "Android Tap-to-Move compatibility patch could not find "
                    + "GameLocation.isTileOccupiedIgnoreFloorsAndHorse in "
                    + "AStarNode.TileClear. The method was left unchanged.",
                    LogLevel.Warn);
            }
            else if (replacedCount > 1)
            {
                Monitor?.Log(
                    "Android Tap-to-Move compatibility patch found "
                    + $"{replacedCount} calls to "
                    + "GameLocation.isTileOccupiedIgnoreFloorsAndHorse in "
                    + "AStarNode.TileClear. All matching calls were patched.",
                    LogLevel.Trace);
            }

            return codes;
        }

        //----------------------------------------
        // IsTileOccupiedForTapToMove
        //----------------------------------------

        /// <summary>
        /// Android Tap-to-Move用の占有判定。
        ///
        /// BigCraftable Extensionの仮想Collision Tileを
        /// AndroidのA*経路探索でも障害物として扱う。
        ///
        /// PlayerPassableTilesに指定されているTileは
        /// プレイヤー用の経路探索では通行可能として扱う。
        ///
        /// TsCoreの仮想Collision Tileでない場合は、
        /// Android版のisTileOccupiedIgnoreFloorsAndHorseを
        /// Reflectionで呼び出してVanillaの判定を使用する。
        /// </summary>
        private static bool IsTileOccupiedForTapToMove(
            GameLocation location,
            Vector2 tile)
        {
            //----------------------------------------
            // TsCore 仮想Collision Tile
            //----------------------------------------

            if (BigCraftableExtensionCollisionService
                .TryGetCollisionObject(
                    location,
                    tile,
                    out StardewValley.Object collisionObject,
                    out Vector2 anchorTile))
            {
                //----------------------------------------
                // PlayerPassableTiles
                //----------------------------------------

                if (BigCraftableExtensionCollisionService
                    .IsPlayerPassableTile(
                        collisionObject,
                        anchorTile,
                        tile))
                {
                    return false;
                }

                //----------------------------------------
                // 仮想Collision Tile
                //----------------------------------------

                return true;
            }

            //----------------------------------------
            // Android Vanilla Method
            //----------------------------------------

            MethodInfo? method =
                IsTileOccupiedIgnoreFloorsAndHorseMethod;

            if (method == null)
            {
                return true;
            }

            bool occupied;

            try
            {
                object? result =
                    method.Invoke(
                        location,
                        new object[]
                        {
                            tile
                        });

                if (result is not bool boolResult)
                {
                    return true;
                }

                occupied =
                    boolResult;
            }
            catch
            {
                //----------------------------------------
                // Vanilla判定を取得できない場合
                // 安全側として通行不可
                //----------------------------------------

                return true;
            }

            //----------------------------------------
            // Vanilla側で通行可能
            //----------------------------------------

            if (!occupied)
            {
                return false;
            }

            //----------------------------------------
            // 実体Object取得
            //----------------------------------------

            if (!location.objects.TryGetValue(
                tile,
                out StardewValley.Object? obj))
            {
                return true;
            }

            //----------------------------------------
            // BigCraftable確認
            //----------------------------------------

            if (!obj.bigCraftable.Value)
            {
                return true;
            }

            //----------------------------------------
            // Anchor PlayerPassable
            //----------------------------------------

            if (!BigCraftableExtensionCollisionService
                .IsPlayerPassableAnchor(obj))
            {
                return true;
            }

            //----------------------------------------
            // PlayerPassable Anchor
            //----------------------------------------

            return false;
        }
    }
}