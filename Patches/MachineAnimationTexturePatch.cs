using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;
using System.Reflection.Emit;
using Ts_Core.Services.MachineRelated;

namespace Ts_Core.Patches
{
    /// <summary>
    /// Machine Interactionの
    /// Animation Textureを描画するPatchです。
    /// </summary>
    public static class MachineAnimationTexturePatch
    {
        //----------------------------------------
        // Apply
        //----------------------------------------

        /// <summary>
        /// Patchを適用します。
        /// </summary>
        public static void Apply(
            Harmony harmony)
        {
            MethodInfo? targetMethod =
                AccessTools.Method(
                    typeof(StardewValley.Object),
                    nameof(StardewValley.Object.draw),
                    new[]
                    {
                        typeof(SpriteBatch),
                        typeof(int),
                        typeof(int),
                        typeof(float)
                    });

            if (targetMethod == null)
            {
                throw new InvalidOperationException(
                    "Object.draw target method was not found.");
            }

            harmony.Patch(
                targetMethod,
                transpiler:
                    new HarmonyMethod(
                        typeof(
                            MachineAnimationTexturePatch),
                        nameof(Transpiler)));
        }

        //----------------------------------------
        // Transpiler
        //----------------------------------------

        /// <summary>
        /// 通常BigCraftable本体の描画処理を
        /// TsCore対応描画へ差し替えます。
        /// </summary>
        private static IEnumerable<CodeInstruction>
            Transpiler(
                IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes =
                new(
                    instructions);

            MethodInfo? getSourceRectMethod =
                AccessTools.Method(
                    typeof(
                        StardewValley.ItemTypeDefinitions
                            .ParsedItemData),
                    nameof(
                        StardewValley.ItemTypeDefinitions
                            .ParsedItemData.GetSourceRect),
                    new[]
                    {
                typeof(int),
                typeof(int?)
                    });

            if (getSourceRectMethod == null)
            {
                //----------------------------------------
                // int?で取得できない場合
                //----------------------------------------

                getSourceRectMethod =
                    AccessTools.Method(
                        typeof(
                            StardewValley.ItemTypeDefinitions
                                .ParsedItemData),
                        nameof(
                            StardewValley.ItemTypeDefinitions
                                .ParsedItemData.GetSourceRect),
                        new[]
                        {
                    typeof(int),
                    typeof(int)
                        });
            }

            MethodInfo? vanillaDrawMethod =
                AccessTools.Method(
                    typeof(SpriteBatch),
                    nameof(SpriteBatch.Draw),
                    new[]
                    {
                typeof(Texture2D),
                typeof(Rectangle),
                typeof(Rectangle?),
                typeof(Color),
                typeof(float),
                typeof(Vector2),
                typeof(SpriteEffects),
                typeof(float)
                    });

            MethodInfo? customDrawMethod =
                AccessTools.Method(
                    typeof(
                        MachineAnimationTexturePatch),
                    nameof(DrawMachine));

            if (getSourceRectMethod == null
                || vanillaDrawMethod == null
                || customDrawMethod == null)
            {
                throw new InvalidOperationException(
                    "Machine animation draw methods were not found.");
            }

            //----------------------------------------
            // 通常BigCraftableのSourceRectを検索
            //----------------------------------------

            for (int i = 0;
                i < codes.Count;
                i++)
            {
                if (!codes[i].Calls(
                    getSourceRectMethod))
                {
                    continue;
                }

                /*
                 * 通常Machineの場合:
                 *
                 * itemData.GetSourceRect(
                 *     offset,
                 *     ParentSheetIndex)
                 *
                 * offsetはローカル変数なので、
                 * GetSourceRect直前付近に
                 * ldloc が存在します。
                 */

                bool usesLocalOffset =
                    false;

                int start =
                    Math.Max(
                        0,
                        i - 4);

                for (int j = start;
                    j < i;
                    j++)
                {
                    OpCode opcode =
                        codes[j].opcode;

                    if (opcode == OpCodes.Ldloc
                        || opcode == OpCodes.Ldloc_0
                        || opcode == OpCodes.Ldloc_1
                        || opcode == OpCodes.Ldloc_2
                        || opcode == OpCodes.Ldloc_3
                        || opcode == OpCodes.Ldloc_S)
                    {
                        usesLocalOffset =
                            true;

                        break;
                    }
                }

                if (!usesLocalOffset)
                    continue;

                //----------------------------------------
                // このSourceRectの後にある
                // SpriteBatch.Drawを検索
                //----------------------------------------

                for (int j = i + 1;
                    j < codes.Count;
                    j++)
                {
                    if (!codes[j].Calls(
                        vanillaDrawMethod))
                    {
                        continue;
                    }

                    //----------------------------------------
                    // Object自身を追加
                    //----------------------------------------

                    codes.Insert(
                        j,
                        new CodeInstruction(
                            OpCodes.Ldarg_0));

                    j++;

                    //----------------------------------------
                    // vanilla Drawを
                    // TsCore対応Drawへ差し替え
                    //----------------------------------------

                    codes[j].opcode =
                        OpCodes.Call;

                    codes[j].operand =
                        customDrawMethod;

                    return codes;
                }
            }

            throw new InvalidOperationException(
                "Normal BigCraftable draw call was not found.");
        }

        //----------------------------------------
        // Draw
        //----------------------------------------

        /// <summary>
        /// Machine本体を描画します。
        /// </summary>
        private static void DrawMachine(
            SpriteBatch spriteBatch,
            Texture2D texture,
            Rectangle destinationRectangle,
            Rectangle? sourceRectangle,
            Color color,
            float rotation,
            Vector2 origin,
            SpriteEffects effects,
            float layerDepth,
            StardewValley.Object machine)
        {
            //----------------------------------------
            // TsCore Action Animation
            //----------------------------------------

            if (MachineAnimationTextureService
                .TryGetActionDrawData(
                    machine,
                    out Texture2D? actionTexture,
                    out Rectangle actionSourceRect)
                && actionTexture != null)
            {
                texture =
                    actionTexture;

                sourceRectangle =
                    actionSourceRect;
            }

            //----------------------------------------
            // vanilla Working
            //----------------------------------------

            else if (machine.minutesUntilReady.Value > 0
                && !machine.readyForHarvest.Value)
            {
                /*
                 * vanilla側で既に計算された
                 * Texture / SourceRectangleをそのまま使用
                 */
            }

            //----------------------------------------
            // TsCore Idle Animation
            //----------------------------------------

            else if (MachineAnimationTextureService
                .TryGetIdleDrawData(
                    machine,
                    out Texture2D? idleTexture,
                    out Rectangle idleSourceRect)
                && idleTexture != null)
            {
                texture =
                    idleTexture;

                sourceRectangle =
                    idleSourceRect;
            }

            //----------------------------------------
            // Draw
            //----------------------------------------

            spriteBatch.Draw(
                texture,
                destinationRectangle,
                sourceRectangle,
                color,
                rotation,
                origin,
                effects,
                layerDepth);
        }
    }
}