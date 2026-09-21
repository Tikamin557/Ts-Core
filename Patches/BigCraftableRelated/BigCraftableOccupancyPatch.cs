using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using Ts_Core.Services.BigCraftableRelated;

namespace Ts_Core.Patches.BigCraftableRelated
{
    /// <summary>
    /// BigCraftable Extensionで指定された
    /// 追加Collision範囲を占有タイルとして扱うPatchです。
    /// </summary>
    public static class BigCraftableOccupancyPatch
    {
        //----------------------------------------
        // Apply
        //----------------------------------------

        /// <summary>
        /// タイル占有判定にPatchを適用します。
        /// </summary>
        public static void Apply(
            Harmony harmony)
        {
            harmony.Patch(
                original:
                    AccessTools.Method(
                        typeof(GameLocation),
                        nameof(GameLocation.IsTileOccupiedBy),
                        new[]
                        {
                            typeof(Vector2),
                            typeof(CollisionMask),
                            typeof(CollisionMask),
                            typeof(bool)
                        }),
                postfix:
                    new HarmonyMethod(
                        typeof(BigCraftableOccupancyPatch),
                        nameof(IsTileOccupiedByPostfix)));
        }

        //----------------------------------------
        // GameLocation.IsTileOccupiedBy
        //----------------------------------------

        /// <summary>
        /// Vanillaで占有されていない場合、
        /// BigCraftable Extensionの追加Collision範囲を確認します。
        /// </summary>
        private static void IsTileOccupiedByPostfix(
            GameLocation __instance,
            Vector2 tile,
            CollisionMask collisionMask,
            CollisionMask ignorePassables,
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
            // Object判定が対象外
            //----------------------------------------

            if (!collisionMask.HasFlag(
                CollisionMask.Objects))
            {
                return;
            }

            //----------------------------------------
            // 一時除外
            //----------------------------------------

            if (BigCraftableExtensionToolService
                .IsCollisionTemporarilyIgnored(
                    tile))
            {
                return;
            }

            //----------------------------------------
            // Extension Collision
            //----------------------------------------

            if (!BigCraftableExtensionCollisionService.TryGetCollisionObject(
                __instance,
                tile,
                out StardewValley.Object obj))
            {
                return;
            }

            //----------------------------------------
            // Passable
            //----------------------------------------

            if (ignorePassables.HasFlag(
                CollisionMask.Objects)
                && obj.isPassable())
            {
                return;
            }

            __result = true;
        }
    }
}