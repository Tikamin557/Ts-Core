using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System.Reflection;
using System.Reflection.Emit;

namespace Ts_Core.Patches.BigCraftableRelated
{
    /// <summary>
    /// Adds BigCraftable Extension texture culling support
    /// to Ultra Smooth's optimized object renderer.
    /// </summary>
    internal static class BigCraftableUltraSmoothCompatibilityPatch
    {
        private const string TargetTypeName =
            "UltraSmooth.Optimizers.WorldRenderOptimizer";

        private const string TargetMethodName =
            "DrawObjectsOptimized";

        private static IMonitor? Monitor;

        public static void Apply(
            Harmony harmony,
            IMonitor monitor)
        {
            Monitor = monitor;

            Type? targetType =
                AccessTools.TypeByName(TargetTypeName);

            if (targetType == null)
            {
                return;
            }

            MethodInfo? targetMethod =
                AccessTools.Method(
                    targetType,
                    TargetMethodName,
                    new[]
                    {
                        typeof(GameLocation),
                        typeof(SpriteBatch)
                    });

            if (targetMethod == null)
            {
                Monitor.Log(
                    "Ultra Smooth was detected, but "
                    + "WorldRenderOptimizer.DrawObjectsOptimized "
                    + "could not be found. "
                    + "BigCraftable Extension texture culling "
                    + "compatibility could not be applied.",
                    LogLevel.Warn);

                return;
            }

            try
            {
                harmony.Patch(
                    original: targetMethod,
                    transpiler: new HarmonyMethod(
                        typeof(BigCraftableUltraSmoothCompatibilityPatch),
                        nameof(Transpiler)));
            }
            catch (Exception ex)
            {
                Monitor.Log(
                    "Failed to apply BigCraftable Extension "
                    + "texture culling compatibility for Ultra Smooth.\n"
                    + ex,
                    LogLevel.Warn);
            }
        }

        private static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes =
                new(instructions);

            MethodInfo helper =
                AccessTools.Method(
                    typeof(BigCraftableDrawCullingPatch),
                    nameof(BigCraftableDrawCullingPatch.GetObjectDrawStartX));

            bool patched = false;

            for (int i = 0; i < codes.Count; i++)
            {
                if (!IsStoreLocal(
                    codes[i].opcode,
                    out _))
                {
                    continue;
                }

                int storeIndex = i;

                int subIndex = storeIndex - 1;

                if (subIndex < 0
                    || codes[subIndex].opcode != OpCodes.Sub)
                {
                    continue;
                }

                int oneIndex = subIndex - 1;

                if (oneIndex < 0
                    || !IsLoadInt(
                        codes[oneIndex],
                        1))
                {
                    continue;
                }

                int divIndex = oneIndex - 1;

                if (divIndex < 0
                    || codes[divIndex].opcode != OpCodes.Div)
                {
                    continue;
                }

                int sixtyFourIndex = divIndex - 1;

                if (sixtyFourIndex < 0
                    || !IsLoadInt(
                        codes[sixtyFourIndex],
                        64))
                {
                    continue;
                }

                /*
                 * Ultra Smooth has both:
                 *
                 * minY = viewport.Y / 64 - 1;
                 * minX = viewport.X / 64 - 1;
                 *
                 * They have the same arithmetic IL pattern.
                 *
                 * minY appears first, so skip the first match
                 * and patch the second one.
                 */

                if (!patched)
                {
                    patched = true;
                    continue;
                }

                codes.Insert(
                    storeIndex,
                    new CodeInstruction(
                        OpCodes.Call,
                        helper));

                return codes;
            }

            Monitor?.Log(
                "Could not patch Ultra Smooth's "
                + "DrawObjectsOptimized method for "
                + "BigCraftable Extension texture culling. "
                + "Extended BigCraftables may be culled too early "
                + "near the edge of the screen.",
                LogLevel.Warn);

            return codes;
        }

        private static bool IsStoreLocal(
            OpCode opcode,
            out int localIndex)
        {
            localIndex = -1;

            if (opcode == OpCodes.Stloc_0)
            {
                localIndex = 0;
                return true;
            }

            if (opcode == OpCodes.Stloc_1)
            {
                localIndex = 1;
                return true;
            }

            if (opcode == OpCodes.Stloc_2)
            {
                localIndex = 2;
                return true;
            }

            if (opcode == OpCodes.Stloc_3)
            {
                localIndex = 3;
                return true;
            }

            if (opcode == OpCodes.Stloc
                || opcode == OpCodes.Stloc_S)
            {
                return true;
            }

            return false;
        }

        private static bool IsLoadInt(
            CodeInstruction instruction,
            int value)
        {
            if (value == 1
                && instruction.opcode == OpCodes.Ldc_I4_1)
            {
                return true;
            }

            if (instruction.opcode == OpCodes.Ldc_I4
                && instruction.operand is int intValue
                && intValue == value)
            {
                return true;
            }

            if (instruction.opcode == OpCodes.Ldc_I4_S)
            {
                if (instruction.operand is sbyte sbyteValue
                    && sbyteValue == value)
                {
                    return true;
                }

                if (instruction.operand is byte byteValue
                    && byteValue == value)
                {
                    return true;
                }
            }

            return false;
        }
    }
}