using Microsoft.Xna.Framework;
using StardewValley;

namespace Ts_Core.Services.BigCraftableRelated
{
    /// <summary>
    /// BigCraftableExtensionの仮想Collisionに対する
    /// Tool処理を補完するServiceです。
    /// </summary>
    public static class BigCraftableExtensionToolService
    {
        private static readonly HashSet<Vector2>
            temporarilyIgnoredCollisionTiles =
                new HashSet<Vector2>();

        /// <summary>
        /// 指定タイルの仮想Collisionを
        /// 一時的に占有判定から除外します。
        /// </summary>
        public static void BeginIgnoreCollision(
            IEnumerable<Vector2> tiles)
        {
            temporarilyIgnoredCollisionTiles.Clear();

            foreach (Vector2 tile in tiles)
            {
                temporarilyIgnoredCollisionTiles.Add(
                    tile);
            }
        }

        /// <summary>
        /// 仮想Collisionの一時除外を終了します。
        /// </summary>
        public static void EndIgnoreCollision()
        {
            temporarilyIgnoredCollisionTiles.Clear();
        }

        /// <summary>
        /// 指定タイルの仮想Collisionが
        /// 一時的に除外されているか取得します。
        /// </summary>
        public static bool IsCollisionTemporarilyIgnored(
            Vector2 tile)
        {
            return temporarilyIgnoredCollisionTiles.Contains(
                tile);
        }

        /// <summary>
        /// 仮想Collision上のBigCraftableに対して
        /// Tool処理を実行します。
        /// </summary>
        /// <returns>
        /// 仮想Collision上のBigCraftableを処理した場合はtrue。
        /// それ以外はfalse。
        /// </returns>
        public static bool TryPerformToolAction(
            GameLocation location,
            Vector2 targetTile,
            Tool tool,
            Farmer who)
        {
            //----------------------------------------
            // 実ObjectがあるタイルはVanillaに任せる
            //----------------------------------------

            if (location.Objects.ContainsKey(targetTile))
                return false;

            //----------------------------------------
            // 仮想Collisionから所有Objectを取得
            //----------------------------------------

            if (!BigCraftableExtensionCollisionService
                .TryGetCollisionObject(
                    location,
                    targetTile,
                    out StardewValley.Object obj))
            {
                return false;
            }

            //----------------------------------------
            // Tool Action
            //----------------------------------------

            if (!obj.performToolAction(tool))
                return true;

            //----------------------------------------
            // Vanilla相当の回収処理
            //----------------------------------------

            if (obj.Type == "Crafting"
                && obj.fragility.Value != 2)
            {
                location.debris.Add(
                    new Debris(
                        obj.QualifiedItemId,
                        who.GetToolLocation(),
                        Utility.PointToVector2(
                            who.StandingPixel)));
            }

            obj.performRemoveAction();

            //----------------------------------------
            // 実際のAnchorから削除
            //----------------------------------------

            location.Objects.Remove(
                obj.TileLocation);

            return true;
        }
    }
}