using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System.Reflection;
using System.Reflection.Emit;
using Ts_Core.Services.BigCraftableRelated;

namespace Ts_Core.Patches.BigCraftableRelated
{
    /// <summary>
    /// BigCraftable ExtensionのTexture Widthに合わせて、
    /// GameLocationのObject描画範囲を拡張するPatchです。
    /// </summary>
    public static class BigCraftableDrawCullingPatch
    {
        private static IMonitor? Monitor;

        //----------------------------------------
        // Apply
        //----------------------------------------

        /// <summary>
        /// Patchを適用します。
        /// </summary>
        public static void Apply(
            Harmony harmony,
            IMonitor monitor)
        {
            Monitor = monitor;

            harmony.Patch(
                original:
                    AccessTools.Method(
                        typeof(GameLocation),
                        nameof(GameLocation.draw),
                        new[]
                        {
                    typeof(SpriteBatch)
                        }),
                transpiler:
                    new HarmonyMethod(
                        typeof(BigCraftableDrawCullingPatch),
                        nameof(Transpiler)));
        }

        //----------------------------------------
        // Transpiler
        //----------------------------------------

        /// <summary>
        /// Object描画時の左側探索範囲を、
        /// BigCraftable Extensionの最大Texture Widthに
        /// 合わせて拡張します。
        /// </summary>
        private static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes =
                new(instructions);

            //----------------------------------------
            // Target
            //----------------------------------------

            MethodInfo tryGetValueMethod =
                AccessTools.Method(
                    typeof(StardewValley.Network.OverlaidDictionary),
                    nameof(StardewValley.Network.OverlaidDictionary.TryGetValue),
                    new[]
                    {
                        typeof(Microsoft.Xna.Framework.Vector2),
                        typeof(StardewValley.Object).MakeByRefType()
                    });

            MethodInfo helperMethod =
                AccessTools.Method(
                    typeof(BigCraftableDrawCullingPatch),
                    nameof(GetObjectDrawStartX));

            //----------------------------------------
            // Object Draw Loop検索
            //----------------------------------------

            bool patched =
                false;

            for (int i = 0;
                i < codes.Count;
                i++)
            {
                //----------------------------------------
                // Object TryGetValueを探す
                //----------------------------------------

                if (!codes[i].Calls(
                    tryGetValueMethod))
                {
                    continue;
                }

                //----------------------------------------
                // TryGetValueより前にある
                // X Loop初期化を探す
                //----------------------------------------
                //
                // Vanilla:
                //
                // viewport.X / 64 - 1
                // stloc x
                //
                //----------------------------------------

                for (int j = i - 1;
                    j >= 5;
                    j--)
                {
                    if (!IsLoadInt(
                        codes[j - 2],
                        1))
                    {
                        continue;
                    }

                    if (codes[j - 1].opcode
                        != OpCodes.Sub)
                    {
                        continue;
                    }

                    if (!IsStoreLocal(
                        codes[j]))
                    {
                        continue;
                    }

                    if (codes[j - 3].opcode
                        != OpCodes.Div)
                    {
                        continue;
                    }

                    if (!IsLoadInt(
                        codes[j - 4],
                        64))
                    {
                        continue;
                    }

                    //----------------------------------------
                    // Vanilla X開始値の直後に
                    // TsCore補正を追加
                    //----------------------------------------
                    //
                    // 元:
                    //
                    // ... / 64 - 1
                    // stloc x
                    //
                    // ↓
                    //
                    // ... / 64 - 1
                    // call GetObjectDrawStartX
                    // stloc x
                    //
                    //----------------------------------------

                    codes.Insert(
                        j,
                        new CodeInstruction(
                            OpCodes.Call,
                            helperMethod));

                    patched =
                        true;

                    break;
                }

                break;
            }

            //----------------------------------------
            // Patch確認
            //----------------------------------------

            if (!patched)
            {
                Type? ultraSmoothType =
                    AccessTools.TypeByName(
                        "UltraSmooth.Optimizers.WorldRenderOptimizer");

                if (ultraSmoothType == null)
                {
                    Monitor?.Log(
                        "Could not patch GameLocation.draw "
                        + "for BigCraftable Extension texture culling. "
                        + "Another mod may have modified the object drawing code. "
                        + "Extended BigCraftables may be culled too early "
                        + "near the edge of the screen.",
                        LogLevel.Warn);
                }

                return codes;
            }

            return codes;
        }

        //----------------------------------------
        // GetObjectDrawStartX
        //----------------------------------------

        /// <summary>
        /// Object描画時のX探索開始位置を取得します。
        /// </summary>
        internal static int GetObjectDrawStartX(
            int vanillaStartX)
        {
            int maxTextureWidth =
                BigCraftableExtensionDataService
                    .MaxTextureWidth;

            //----------------------------------------
            // Vanilla Width
            //----------------------------------------

            if (maxTextureWidth <= 16)
            {
                return vanillaStartX;
            }

            //----------------------------------------
            // Texture Tile Width
            //----------------------------------------

            int textureTiles =
                (int)Math.Ceiling(
                    maxTextureWidth / 16f);

            //----------------------------------------
            // Additional Tiles
            //----------------------------------------

            int additionalTiles =
                Math.Max(
                    0,
                    textureTiles - 1);

            //----------------------------------------
            // 左側へ探索範囲を拡張
            //----------------------------------------

            return vanillaStartX
                - additionalTiles;
        }

        //----------------------------------------
        // IL Helpers
        //----------------------------------------

        /// <summary>
        /// Local VariableへのStore命令か確認します。
        /// </summary>
        private static bool IsStoreLocal(
            CodeInstruction instruction)
        {
            return instruction.opcode
                    == OpCodes.Stloc
                || instruction.opcode
                    == OpCodes.Stloc_S
                || instruction.opcode
                    == OpCodes.Stloc_0
                || instruction.opcode
                    == OpCodes.Stloc_1
                || instruction.opcode
                    == OpCodes.Stloc_2
                || instruction.opcode
                    == OpCodes.Stloc_3;
        }

        /// <summary>
        /// 指定された整数をLoadする命令か確認します。
        /// </summary>
        private static bool IsLoadInt(
            CodeInstruction instruction,
            int value)
        {
            if (instruction.opcode
                == OpCodes.Ldc_I4_M1)
            {
                return value == -1;
            }

            if (instruction.opcode
                == OpCodes.Ldc_I4_0)
            {
                return value == 0;
            }

            if (instruction.opcode
                == OpCodes.Ldc_I4_1)
            {
                return value == 1;
            }

            if (instruction.opcode
                == OpCodes.Ldc_I4_2)
            {
                return value == 2;
            }

            if (instruction.opcode
                == OpCodes.Ldc_I4_3)
            {
                return value == 3;
            }

            if (instruction.opcode
                == OpCodes.Ldc_I4_4)
            {
                return value == 4;
            }

            if (instruction.opcode
                == OpCodes.Ldc_I4_5)
            {
                return value == 5;
            }

            if (instruction.opcode
                == OpCodes.Ldc_I4_6)
            {
                return value == 6;
            }

            if (instruction.opcode
                == OpCodes.Ldc_I4_7)
            {
                return value == 7;
            }

            if (instruction.opcode
                == OpCodes.Ldc_I4_8)
            {
                return value == 8;
            }

            if (instruction.opcode
                    == OpCodes.Ldc_I4_S
                || instruction.opcode
                    == OpCodes.Ldc_I4)
            {
                return Convert.ToInt32(
                    instruction.operand)
                    == value;
            }

            return false;
        }
    }
}