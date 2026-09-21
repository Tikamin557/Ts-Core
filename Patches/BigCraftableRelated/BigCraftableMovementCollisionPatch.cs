using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using System.Reflection;
using System.Reflection.Emit;
using Ts_Core.Services.BigCraftableRelated;

namespace Ts_Core.Patches.BigCraftableRelated
{
    /// <summary>
    /// BigCraftable Extensionで指定された
    /// Collision範囲を移動Collisionに適用するPatchです。
    /// </summary>
    public static class BigCraftableMovementCollisionPatch
    {
        //----------------------------------------
        // Fields
        //----------------------------------------

        /// <summary>
        /// Object Collision Lambdaが保持している
        /// character Fieldです。
        /// </summary>
        private static FieldInfo? CharacterField;

        //----------------------------------------
        // Apply
        //----------------------------------------

        /// <summary>
        /// GameLocationの移動Collision判定に
        /// Patchを適用します。
        /// </summary>
        public static void Apply(
            Harmony harmony)
        {
            //----------------------------------------
            // isCollidingPosition
            //----------------------------------------

            MethodInfo? collisionMethod =
                AccessTools.Method(
                    typeof(GameLocation),
                    nameof(GameLocation.isCollidingPosition),
                    new[]
                    {
                        typeof(Microsoft.Xna.Framework.Rectangle),
                        typeof(xTile.Dimensions.Rectangle),
                        typeof(bool),
                        typeof(int),
                        typeof(bool),
                        typeof(Character),
                        typeof(bool),
                        typeof(bool),
                        typeof(bool),
                        typeof(bool)
                    });

            if (collisionMethod == null)
            {
                throw new InvalidOperationException(
                    "Could not find GameLocation.isCollidingPosition.");
            }

            harmony.Patch(
                original:
                    collisionMethod,
                postfix:
                    new HarmonyMethod(
                        typeof(BigCraftableMovementCollisionPatch),
                        nameof(IsCollidingPositionPostfix)));

            //----------------------------------------
            // Object Collision Lambda
            //----------------------------------------

            MethodInfo? objectCollisionMethod =
               FindObjectCollisionMethod(
                   out FieldInfo? characterField);

            if (objectCollisionMethod == null
                || characterField == null)
            {
                throw new InvalidOperationException(
                    "Could not find GameLocation Object collision lambda.");
            }

            CharacterField =
                characterField;

            harmony.Patch(
                original:
                    objectCollisionMethod,
                transpiler:
                    new HarmonyMethod(
                        typeof(BigCraftableMovementCollisionPatch),
                        nameof(ObjectCollisionTranspiler)));
        }

        //----------------------------------------
        // FindObjectCollisionMethod
        //----------------------------------------

        /// <summary>
        /// GameLocation.isCollidingPosition内で
        /// Object Collision判定に使用されるLambdaを検索します。
        /// </summary>
        private static MethodInfo?
            FindObjectCollisionMethod(
                out FieldInfo? characterField)
        {
            characterField = null;

            //----------------------------------------
            // Nested Type検索
            //----------------------------------------

            Type[] nestedTypes =
                typeof(GameLocation)
                    .GetNestedTypes(
                        BindingFlags.Public
                        | BindingFlags.NonPublic);

            foreach (Type nestedType in nestedTypes)
            {
                //----------------------------------------
                // character Field
                //----------------------------------------

                FieldInfo? field =
                    nestedType.GetField(
                        "character",
                        BindingFlags.Instance
                        | BindingFlags.Public
                        | BindingFlags.NonPublic);

                if (field == null
                    || field.FieldType != typeof(Character))
                {
                    continue;
                }

                //----------------------------------------
                // Object Collision Lambda
                //----------------------------------------

                MethodInfo? method =
                    nestedType.GetMethod(
                        "<isCollidingPosition>b__1",
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

                if (method == null
                    || method.ReturnType != typeof(bool))
                {
                    continue;
                }

                //----------------------------------------
                // Found
                //----------------------------------------

                characterField =
                    field;

                return method;
            }

            return null;
        }

        //----------------------------------------
        // Object Collision Transpiler
        //----------------------------------------

        /// <summary>
        /// Vanilla Object Collision判定に
        /// PlayerPassableTilesのAnchor判定を追加します。
        /// </summary>
        private static IEnumerable<CodeInstruction>
            ObjectCollisionTranspiler(
                IEnumerable<CodeInstruction> instructions,
                ILGenerator generator)
        {
            List<CodeInstruction> codes =
                new List<CodeInstruction>(
                    instructions);

            //----------------------------------------
            // 対象Method / Field
            //----------------------------------------

            MethodInfo? isPassableMethod =
                AccessTools.Method(
                    typeof(StardewValley.Object),
                    nameof(StardewValley.Object.isPassable));

            MethodInfo? playerPassableMethod =
                AccessTools.Method(
                    typeof(BigCraftableMovementCollisionPatch),
                    nameof(IsPlayerPassableAnchor));

            FieldInfo? characterField =
                CharacterField;

            if (isPassableMethod == null
                || playerPassableMethod == null
                || characterField == null)
            {
                throw new InvalidOperationException(
                    "Could not resolve Object Collision members.");
            }

            //----------------------------------------
            // Pattern検索
            //
            // Vanilla:
            //
            // ldloc.0
            // callvirt Object.isPassable
            // brfalse ...
            // ldc.i4.0
            // ret
            //----------------------------------------

            for (int i = 0;
                i < codes.Count - 4;
                i++)
            {
                if (codes[i].opcode != OpCodes.Ldloc_0)
                {
                    continue;
                }

                if (!codes[i + 1].Calls(
                    isPassableMethod))
                {
                    continue;
                }

                if (codes[i + 2].opcode != OpCodes.Brfalse
                    && codes[i + 2].opcode != OpCodes.Brfalse_S)
                {
                    continue;
                }

                if (codes[i + 3].opcode != OpCodes.Ldc_I4_0
                    || codes[i + 4].opcode != OpCodes.Ret)
                {
                    continue;
                }

                //----------------------------------------
                // 新しいLabel
                //----------------------------------------

                Label vanillaCollisionCheck =
                    generator.DefineLabel();

                //----------------------------------------
                // 元のldloc.0に付いているLabelを
                // 挿入処理の先頭へ移動
                //----------------------------------------

                CodeInstruction loadObject =
                    new CodeInstruction(
                        OpCodes.Ldloc_0);

                loadObject.labels.AddRange(
                    codes[i].labels);

                codes[i].labels.Clear();

                //----------------------------------------
                // 元のVanilla処理開始位置へLabel追加
                //----------------------------------------

                codes[i].labels.Add(
                    vanillaCollisionCheck);

                //----------------------------------------
                // 挿入処理
                //
                // if (IsPlayerPassableAnchor(
                //     o,
                //     character))
                // {
                //     return false;
                // }
                //----------------------------------------

                List<CodeInstruction> injected =
                    new List<CodeInstruction>
                    {
                //----------------------------------------
                // o
                //----------------------------------------

                loadObject,

                //----------------------------------------
                // character
                //----------------------------------------

                new CodeInstruction(
                    OpCodes.Ldarg_0),

                new CodeInstruction(
                    OpCodes.Ldfld,
                    characterField),

                //----------------------------------------
                // IsPlayerPassableAnchor(
                //     o,
                //     character)
                //----------------------------------------

                new CodeInstruction(
                    OpCodes.Call,
                    playerPassableMethod),

                //----------------------------------------
                // falseならVanillaへ
                //----------------------------------------

                new CodeInstruction(
                    OpCodes.Brfalse,
                    vanillaCollisionCheck),

                //----------------------------------------
                // trueならCollisionなし
                //----------------------------------------

                new CodeInstruction(
                    OpCodes.Ldc_I4_0),

                new CodeInstruction(
                    OpCodes.Ret)
                    };

                //----------------------------------------
                // 挿入
                //----------------------------------------

                codes.InsertRange(
                    i,
                    injected);

                return codes;
            }

            //----------------------------------------
            // Patternが見つからない場合
            //----------------------------------------

            throw new InvalidOperationException(
                "Could not find the vanilla Object collision pattern.");
        }

        //----------------------------------------
        // IsPlayerPassableAnchor
        //----------------------------------------

        /// <summary>
        /// ObjectのAnchor Tileが
        /// Farmerに対して通行可能か確認します。
        /// </summary>
        private static bool IsPlayerPassableAnchor(
            StardewValley.Object obj,
            Character character)
        {
            //----------------------------------------
            // Farmerのみ
            //----------------------------------------

            if (character is not Farmer)
            {
                return false;
            }

            //----------------------------------------
            // PlayerPassableTiles
            //----------------------------------------

            return BigCraftableExtensionCollisionService
                .IsPlayerPassableAnchor(
                    obj);
        }

        //----------------------------------------
        // GameLocation.isCollidingPosition
        //----------------------------------------

        /// <summary>
        /// VanillaでCollisionが発生していない場合、
        /// BigCraftable Extensionの追加Collision範囲を確認します。
        /// </summary>
        private static void IsCollidingPositionPostfix(
            GameLocation __instance,
            Microsoft.Xna.Framework.Rectangle position,
            bool glider,
            Character character,
            ref bool __result)
        {
            //----------------------------------------
            // Vanilla判定
            //----------------------------------------

            if (__result)
            {
                return;
            }

            //----------------------------------------
            // Glider
            //----------------------------------------

            if (glider)
            {
                return;
            }

            //----------------------------------------
            // Position範囲
            //----------------------------------------

            int left =
                position.Left / 64;

            int right =
                (position.Right - 1) / 64;

            int top =
                position.Top / 64;

            int bottom =
                (position.Bottom - 1) / 64;

            //----------------------------------------
            // Collision Tile確認
            //----------------------------------------

            for (int x = left; x <= right; x++)
            {
                for (int y = top; y <= bottom; y++)
                {
                    Vector2 tile =
                        new Vector2(
                            x,
                            y);

                    if (!BigCraftableExtensionCollisionService
                        .TryGetCollisionObject(
                            __instance,
                            tile,
                            out StardewValley.Object obj,
                            out Vector2 anchorTile))
                    {
                        continue;
                    }

                    //----------------------------------------
                    // Passable
                    //----------------------------------------

                    if (obj.isPassable())
                    {
                        continue;
                    }

                    //----------------------------------------
                    // Player Passable
                    //----------------------------------------

                    if (character is Farmer
                        && BigCraftableExtensionCollisionService
                            .IsPlayerPassableTile(
                                obj,
                                anchorTile,
                                tile))
                    {
                        continue;
                    }

                    //----------------------------------------
                    // Collision Bounds
                    //----------------------------------------

                    Microsoft.Xna.Framework.Rectangle collisionBounds =
                        new Microsoft.Xna.Framework.Rectangle(
                            x * 64,
                            y * 64,
                            64,
                            64);

                    if (!collisionBounds.Intersects(
                        position))
                    {
                        continue;
                    }

                    //----------------------------------------
                    // Character Collision
                    //----------------------------------------

                    if (character != null
                        && !character.collideWith(
                            obj))
                    {
                        continue;
                    }

                    //----------------------------------------
                    // Temporary Passable
                    //----------------------------------------

                    if (character is Farmer farmer
                        && farmer.TemporaryPassableTiles.Intersects(
                            collisionBounds))
                    {
                        continue;
                    }

                    __result = true;
                    return;
                }
            }
        }
    }
}