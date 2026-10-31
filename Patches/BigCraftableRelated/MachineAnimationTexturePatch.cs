using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using System.Reflection;
using System.Reflection.Emit;
using Ts_Core.Services.BigCraftableRelated;

namespace Ts_Core.Patches.BigCraftableRelated
{
    /// <summary>
    /// BigCraftable Extensionの
    /// Animation Textureを描画するPatchです。
    /// </summary>
    public static class MachineAnimationTexturePatch
    {
        private static IMonitor? monitor;

        //----------------------------------------
        // Apply
        //----------------------------------------

        /// <summary>
        /// Patchを適用します。
        /// </summary>
        public static void Apply(
            Harmony harmony,
            IMonitor modMonitor)
        {
            monitor = modMonitor;
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
                monitor?.Log(
                    "Machine animation drawing patch was skipped: required methods were not found.",
                    LogLevel.Warn);

                return codes;
            }

            //----------------------------------------
            // 通常BigCraftableの描画候補を検索
            //----------------------------------------

            /*
             * まず従来の条件（直前4命令以内のldloc）で検索します。
             * 他Modによる命令挿入で条件から外れた場合のみ、
             * 直前12命令まで検索範囲を広げます。
             *
             * 誤ったDrawの差し替えを防ぐため、候補が
             * 一意に特定できる場合にだけPatchを適用します。
             */

            List<int> strictCandidates =
                FindDrawCandidates(
                    codes,
                    getSourceRectMethod,
                    vanillaDrawMethod,
                    4);

            List<int> candidates =
                strictCandidates.Count > 0
                    ? strictCandidates
                    : FindDrawCandidates(
                        codes,
                        getSourceRectMethod,
                        vanillaDrawMethod,
                        12);

            if (candidates.Count != 1)
            {
                monitor?.Log(
                    $"Machine animation drawing patch was skipped: expected one BigCraftable draw call, found {candidates.Count}. Another mod may have modified Object.draw().",
                    LogLevel.Warn);

                return codes;
            }

            //----------------------------------------
            // Object自身を描画呼び出しへ追加
            //----------------------------------------

            int drawIndex = candidates[0];
            CodeInstruction originalDraw = codes[drawIndex];
            CodeInstruction loadMachine =
                new(OpCodes.Ldarg_0);

            // 分岐先ラベルは追加命令に移し、
            // Object引数の読み込みを飛ばさないようにします。
            loadMachine.labels.AddRange(originalDraw.labels);
            originalDraw.labels.Clear();

            codes.Insert(
                drawIndex,
                loadMachine);

            originalDraw.opcode =
                OpCodes.Call;

            originalDraw.operand =
                customDrawMethod;

            return codes;
        }

        //----------------------------------------
        // FindDrawCandidates
        //----------------------------------------

        /// <summary>
        /// SourceRect取得から近い通常BigCraftable描画候補を探します。
        /// 別のSourceRect取得をまたいでDrawを検索しません。
        /// </summary>
        private static List<int> FindDrawCandidates(
            List<CodeInstruction> codes,
            MethodInfo getSourceRectMethod,
            MethodInfo vanillaDrawMethod,
            int localSearchRange)
        {
            HashSet<int> candidates =
                new();

            for (int i = 0;
                i < codes.Count;
                i++)
            {
                if (!codes[i].Calls(getSourceRectMethod))
                    continue;

                bool usesLocalOffset =
                    false;

                int start =
                    Math.Max(0, i - localSearchRange);

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

                // 次のSourceRect取得、または一定の範囲で検索を終了します。
                int end =
                    Math.Min(codes.Count, i + 80);

                for (int j = i + 1;
                    j < end;
                    j++)
                {
                    if (codes[j].Calls(getSourceRectMethod))
                        break;

                    if (codes[j].Calls(vanillaDrawMethod))
                    {
                        candidates.Add(j);
                        break;
                    }
                }
            }

            return candidates.OrderBy(index => index).ToList();
        }

        //----------------------------------------
        // Draw
        //----------------------------------------

        /// <summary>
        /// BigCraftable本体を描画します。
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
            // Extension Data
            //----------------------------------------

            if (!BigCraftableExtensionDataService
                .TryGetExtensionData(
                    machine,
                    out string extensionId,
                    out var extension))
            {
                //----------------------------------------
                // Extension未使用
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

                return;
            }

            //----------------------------------------
            // Texture Size
            //----------------------------------------

            BigCraftableExtensionTextureService
                .GetTextureSize(
                    machine,
                    extension,
                    out int textureWidth,
                    out int textureHeight);

            //----------------------------------------
            // TsCore Action Animation
            //----------------------------------------

            if (MachineAnimationTextureService
                .TryGetActionDrawData(
                    machine,
                    extensionId,
                    extension,
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
                 * Texture / SourceRectangleを使用します。
                 *
                 * Texture Sizeのみ
                 * Extension設定を適用します。
                 */

                if (sourceRectangle.HasValue)
                {
                    Rectangle source =
                        sourceRectangle.Value;

                    source.Width =
                        textureWidth;

                    source.Height =
                        textureHeight;

                    sourceRectangle =
                        source;
                }
            }

            //----------------------------------------
            // Extension / Default Texture
            //----------------------------------------

            else
            {
                texture =
                    BigCraftableExtensionTextureService
                        .GetTexture(
                            machine,
                            extension);

                sourceRectangle =
                    BigCraftableExtensionTextureService
                        .GetSourceRect(
                            machine,
                            extension);

                //----------------------------------------
                // Normal Animation
                //----------------------------------------

                if (sourceRectangle.HasValue
                    && extension.Frames != null
                    && extension.Frames.Count > 0
                    && MachineAnimationTextureService
                        .TryGetNormalFrame(
                            extensionId,
                            extension,
                            out int frame))
                {
                    Rectangle source =
                        sourceRectangle.Value;

                    source.X +=
                        frame
                        * textureWidth;

                    sourceRectangle =
                        source;
                }
            }

            //----------------------------------------
            // Destination Width
            //----------------------------------------

            int widthDifference =
                (textureWidth - 16) * 4;

            destinationRectangle.Width +=
                widthDifference;

            //----------------------------------------
            // Destination Height
            //----------------------------------------

            int heightDifference =
                (textureHeight - 32) * 4;

            destinationRectangle.Y -=
                heightDifference;

            destinationRectangle.Height +=
                heightDifference;

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

            //----------------------------------------
            // Draw Layers
            //----------------------------------------

            BigCraftableDrawLayerService
                .DrawLayers(
                    machine,
                    spriteBatch,
                    extension,
                    texture,
                    color);
        }
    }
}