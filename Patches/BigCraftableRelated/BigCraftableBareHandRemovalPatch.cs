using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Network;
using System.Reflection;
using System.Reflection.Emit;
using Ts_Core.Services.BigCraftableRelated;

namespace Ts_Core.Patches.BigCraftableRelated
{
    /// <summary>
    /// BigCraftable Extensionの仮想Collision範囲から
    /// 素手でBigCraftableを回収できるようにするPatchです。
    /// </summary>
    public static class BigCraftableBareHandRemovalPatch
    {
        //----------------------------------------
        // State
        //----------------------------------------

        /// <summary>
        /// 素手回収時に解決された
        /// 実際のObject Tileです。
        /// </summary>
        private static Vector2? resolvedObjectTile;

        //----------------------------------------
        // Apply
        //----------------------------------------

        /// <summary>
        /// Game1.pressUseToolButtonに
        /// Patchを適用します。
        /// </summary>
        public static void Apply(
            Harmony harmony)
        {
            harmony.Patch(
                original:
                    AccessTools.Method(
                        typeof(Game1),
                        nameof(Game1.pressUseToolButton)),
                transpiler:
                    new HarmonyMethod(
                        typeof(BigCraftableBareHandRemovalPatch),
                        nameof(Transpiler)));
        }

        //----------------------------------------
        // Transpiler
        //----------------------------------------

        /// <summary>
        /// 素手回収処理で使用されるObject検索と削除を、
        /// BigCraftable Extensionの仮想Collision範囲に対応させます。
        /// </summary>
        private static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes =
                instructions.ToList();

            //----------------------------------------
            // Target Methods
            //----------------------------------------

            MethodInfo tryGetValueMethod =
                AccessTools.Method(
                    typeof(OverlaidDictionary),
                    nameof(OverlaidDictionary.TryGetValue),
                    new[]
                    {
                        typeof(Vector2),
                        typeof(StardewValley.Object).MakeByRefType()
                    });

            MethodInfo removeMethod =
                AccessTools.Method(
                    typeof(OverlaidDictionary),
                    nameof(OverlaidDictionary.Remove),
                    new[]
                    {
                        typeof(Vector2)
                    });

            MethodInfo replacementTryGetValueMethod =
                AccessTools.Method(
                    typeof(BigCraftableBareHandRemovalPatch),
                    nameof(TryGetObjectForBareHandRemoval));

            MethodInfo replacementRemoveMethod =
                AccessTools.Method(
                    typeof(BigCraftableBareHandRemovalPatch),
                    nameof(RemoveObjectForBareHandRemoval));

            //----------------------------------------
            // Replace
            //----------------------------------------

            bool replacedTryGetValue =
                false;

            bool replacedRemove =
                false;

            foreach (CodeInstruction code
                in codes)
            {
                //----------------------------------------
                // Objects.TryGetValue
                //----------------------------------------

                if (!replacedTryGetValue
                    && code.Calls(
                        tryGetValueMethod))
                {
                    code.opcode =
                        OpCodes.Call;

                    code.operand =
                        replacementTryGetValueMethod;

                    replacedTryGetValue =
                        true;
                }

                //----------------------------------------
                // Objects.Remove
                //----------------------------------------

                else if (!replacedRemove
                    && code.Calls(
                        removeMethod))
                {
                    code.opcode =
                        OpCodes.Call;

                    code.operand =
                        replacementRemoveMethod;

                    replacedRemove =
                        true;
                }

                yield return code;
            }

            //----------------------------------------
            // Patch確認
            //----------------------------------------

            if (!replacedTryGetValue
                || !replacedRemove)
            {
                throw new InvalidOperationException(
                    "Failed to patch Game1.pressUseToolButton "
                    + "for BigCraftable Extension bare-hand removal.");
            }
        }

        //----------------------------------------
        // TryGetObjectForBareHandRemoval
        //----------------------------------------

        /// <summary>
        /// 素手回収対象のObjectを取得します。
        /// 実Objectが存在しない場合は、
        /// BigCraftable Extensionの仮想Collision範囲を確認します。
        /// </summary>
        private static bool TryGetObjectForBareHandRemoval(
            OverlaidDictionary objects,
            Vector2 tile,
            out StardewValley.Object obj)
        {
            //----------------------------------------
            // State初期化
            //----------------------------------------

            resolvedObjectTile =
                null;

            //----------------------------------------
            // Vanilla Object
            //----------------------------------------

            if (objects.TryGetValue(
                tile,
                out obj))
            {
                resolvedObjectTile =
                    tile;

                return true;
            }

            //----------------------------------------
            // Location
            //----------------------------------------

            GameLocation? location =
                Game1.currentLocation;

            if (location == null)
            {
                obj =
                    null!;

                return false;
            }

            //----------------------------------------
            // Virtual Collision Object
            //----------------------------------------

            if (!BigCraftableExtensionCollisionService
                .TryGetCollisionObject(
                    location,
                    tile,
                    out obj))
            {
                return false;
            }

            //----------------------------------------
            // Anchor Tile
            //----------------------------------------

            resolvedObjectTile =
                obj.TileLocation;

            return true;
        }

        //----------------------------------------
        // RemoveObjectForBareHandRemoval
        //----------------------------------------

        /// <summary>
        /// 素手回収で解決された実際のObject Tileから
        /// Objectを削除します。
        /// </summary>
        private static bool RemoveObjectForBareHandRemoval(
            OverlaidDictionary objects,
            Vector2 tile)
        {
            //----------------------------------------
            // Resolved Tile
            //----------------------------------------

            Vector2 removeTile =
                resolvedObjectTile
                ?? tile;

            //----------------------------------------
            // State初期化
            //----------------------------------------

            resolvedObjectTile =
                null;

            //----------------------------------------
            // Remove
            //----------------------------------------

            return objects.Remove(
                removeTile);
        }
    }
}