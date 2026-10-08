using Microsoft.Xna.Framework;
using StardewValley;
using Ts_Core.Models.BigCraftableRelated;

namespace Ts_Core.Services.BigCraftableRelated
{
    /// <summary>
    /// BigCraftable Extensionで指定された
    /// Collision範囲を処理します。
    /// </summary>
    public static class BigCraftableExtensionCollisionService
    {
        //----------------------------------------
        // TryGetCollisionObject
        //----------------------------------------

        /// <summary>
        /// 指定タイルがBigCraftable Extensionの
        /// 追加Collision範囲に含まれている場合、
        /// そのBigCraftableを取得します。
        /// </summary>
        public static bool TryGetCollisionObject(
            GameLocation location,
            Vector2 tile,
            out StardewValley.Object collisionObject)
        {
            return TryGetCollisionObject(
                location,
                tile,
                out collisionObject,
                out _);
        }

        //----------------------------------------
        // TryGetCollisionObject
        // Anchor取得
        //----------------------------------------

        /// <summary>
        /// 指定タイルがBigCraftable Extensionの
        /// 追加Collision範囲に含まれている場合、
        /// そのBigCraftableとAnchor Tileを取得します。
        /// </summary>
        public static bool TryGetCollisionObject(
            GameLocation location,
            Vector2 tile,
            out StardewValley.Object collisionObject,
            out Vector2 anchorTile)
        {
            collisionObject =
                null!;

            anchorTile =
                Vector2.Zero;

            //----------------------------------------
            // 最大Collision Size
            //----------------------------------------

            int maxWidth =
                BigCraftableExtensionDataService
                    .MaxCollisionWidth;

            int maxHeight =
                BigCraftableExtensionDataService
                    .MaxCollisionHeight;

            //----------------------------------------
            // 追加Collisionなし
            //----------------------------------------

            if (maxWidth <= 1
                && maxHeight <= 1)
            {
                return false;
            }

            //----------------------------------------
            // Anchor候補確認
            //----------------------------------------

            for (int offsetX = 0;
                offsetX < maxWidth;
                offsetX++)
            {
                for (int offsetY = 0;
                    offsetY < maxHeight;
                    offsetY++)
                {
                    //----------------------------------------
                    // Anchor Tile自身は対象外
                    //----------------------------------------

                    if (offsetX == 0
                        && offsetY == 0)
                    {
                        continue;
                    }

                    //----------------------------------------
                    // Anchor候補
                    //----------------------------------------

                    Vector2 candidateAnchor =
                        new Vector2(
                            tile.X - offsetX,
                            tile.Y + offsetY);

                    //----------------------------------------
                    // Object取得
                    //----------------------------------------

                    if (!location.objects.TryGetValue(
                        candidateAnchor,
                        out StardewValley.Object obj))
                    {
                        continue;
                    }

                    //----------------------------------------
                    // BigCraftable確認
                    //----------------------------------------

                    if (!obj.bigCraftable.Value)
                    {
                        continue;
                    }

                    //----------------------------------------
                    // Extension Data取得
                    //----------------------------------------

                    if (!BigCraftableExtensionDataService
                        .TryGetExtensionData(
                            obj,
                            out _,
                            out BigCraftableExtensionData extension))
                    {
                        continue;
                    }

                    //----------------------------------------
                    // Collision Size
                    //----------------------------------------

                    int width =
                        Math.Max(
                            1,
                            extension.CollisionWidth);

                    int height =
                        Math.Max(
                            1,
                            extension.CollisionHeight);

                    //----------------------------------------
                    // Collision範囲確認
                    //----------------------------------------

                    if (offsetX >= width
                        || offsetY >= height)
                    {
                        continue;
                    }

                    collisionObject =
                        obj;

                    anchorTile =
                        candidateAnchor;

                    return true;
                }
            }

            return false;
        }

        //----------------------------------------
        // IsPlayerPassableTile
        //----------------------------------------

        /// <summary>
        /// 指定されたCollision Tileが
        /// Farmerに対して通行可能か確認します。
        /// </summary>
        public static bool IsPlayerPassableTile(
            StardewValley.Object obj,
            Vector2 anchorTile,
            Vector2 collisionTile)
        {
            //----------------------------------------
            // BigCraftable確認
            //----------------------------------------

            if (!obj.bigCraftable.Value)
            {
                return false;
            }

            //----------------------------------------
            // Extension Data取得
            //----------------------------------------

            if (!BigCraftableExtensionDataService
                .TryGetExtensionData(
                    obj,
                    out _,
                    out BigCraftableExtensionData extension))
            {
                return false;
            }

            //----------------------------------------
            // PlayerPassableTiles
            //----------------------------------------

            if (extension.PlayerPassableTileOffsets.Count == 0)
            {
                return false;
            }

            //----------------------------------------
            // Anchorからの相対座標
            //----------------------------------------

            Point offset =
                new Point(
                    (int)(collisionTile.X - anchorTile.X),
                    (int)(collisionTile.Y - anchorTile.Y));

            //----------------------------------------
            // 通行可能Tile確認
            //----------------------------------------

            return extension
                .PlayerPassableTileOffsets
                .Contains(
                    offset);
        }

        //----------------------------------------
        // IsPlayerPassableAnchor
        //----------------------------------------

        /// <summary>
        /// BigCraftableのAnchor Tileが
        /// Farmerに対して通行可能か確認します。
        /// </summary>
        public static bool IsPlayerPassableAnchor(
            StardewValley.Object obj)
        {
            //----------------------------------------
            // BigCraftable確認
            //----------------------------------------

            if (!obj.bigCraftable.Value)
            {
                return false;
            }

            //----------------------------------------
            // Extension Data取得
            //----------------------------------------

            if (!BigCraftableExtensionDataService
                .TryGetExtensionData(
                    obj,
                    out _,
                    out BigCraftableExtensionData extension))
            {
                return false;
            }

            //----------------------------------------
            // Anchor
            //----------------------------------------

            return extension
                .PlayerPassableTileOffsets
                .Contains(
                    Point.Zero);
        }
    }
}