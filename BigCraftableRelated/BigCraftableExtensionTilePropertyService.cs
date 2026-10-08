using Microsoft.Xna.Framework;
using StardewValley;
using Ts_Core.Models.BigCraftableRelated;

namespace Ts_Core.Services.BigCraftableRelated
{
    /// <summary>
    /// BigCraftable Extensionによる
    /// 動的Tile Propertyを処理するServiceです。
    /// </summary>
    public static class BigCraftableExtensionTilePropertyService
    {
        //----------------------------------------
        // TryGetTileProperty
        //----------------------------------------

        /// <summary>
        /// 指定TileにBigCraftable Extensionの
        /// Tile Propertyが設定されているか確認します。
        /// </summary>
        public static bool TryGetTileProperty(
            GameLocation location,
            Vector2 tile,
            string propertyName,
            string layerName,
            out string propertyValue)
        {
            propertyValue =
                string.Empty;

            //----------------------------------------
            // Tile Property Range
            //----------------------------------------

            int minX =
                BigCraftableExtensionDataService
                    .MinTilePropertyOffsetX;

            int maxX =
                BigCraftableExtensionDataService
                    .MaxTilePropertyOffsetX;

            int minY =
                BigCraftableExtensionDataService
                    .MinTilePropertyOffsetY;

            int maxY =
                BigCraftableExtensionDataService
                    .MaxTilePropertyOffsetY;

            //----------------------------------------
            // 候補Anchor検索
            //----------------------------------------

            for (int offsetX = minX;
                offsetX <= maxX;
                offsetX++)
            {
                for (int offsetY = minY;
                    offsetY <= maxY;
                    offsetY++)
                {
                    //----------------------------------------
                    // Anchor
                    //----------------------------------------

                    Vector2 anchorTile =
                        new Vector2(
                            tile.X - offsetX,
                            tile.Y - offsetY);

                    //----------------------------------------
                    // Object取得
                    //----------------------------------------

                    if (!location.objects.TryGetValue(
                        anchorTile,
                        out StardewValley.Object? obj)
                        || obj == null
                        || !obj.bigCraftable.Value)
                    {
                        continue;
                    }

                    //----------------------------------------
                    // Extension取得
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
                    // TileProperties
                    //----------------------------------------

                    if (extension.TileProperties == null
                        || extension.TileProperties.Count == 0)
                    {
                        continue;
                    }

                    //----------------------------------------
                    // Relative Offset
                    //----------------------------------------

                    Point offset =
                        new Point(
                            offsetX,
                            offsetY);

                    //----------------------------------------
                    // Property検索
                    //----------------------------------------

                    foreach (BigCraftableTilePropertyData property
                        in extension.TileProperties)
                    {
                        //----------------------------------------
                        // Property Name
                        //----------------------------------------

                        if (!string.Equals(
                            property.Name,
                            propertyName,
                            StringComparison.Ordinal))
                        {
                            continue;
                        }

                        //----------------------------------------
                        // Layer
                        //----------------------------------------

                        if (!string.Equals(
                            property.Layer,
                            layerName,
                            StringComparison.Ordinal))
                        {
                            continue;
                        }

                        //----------------------------------------
                        // Tile
                        //----------------------------------------

                        if (!property.TileOffsets.Contains(
                            offset))
                        {
                            continue;
                        }

                        //----------------------------------------
                        // Value
                        //----------------------------------------

                        propertyValue =
                            property.Value;

                        return true;
                    }
                }
            }

            return false;
        }
    }
}