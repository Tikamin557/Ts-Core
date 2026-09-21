using Microsoft.Xna.Framework;
using System.Globalization;

namespace Ts_Core.Models.BigCraftableRelated
{
    /// <summary>
    /// BigCraftable Extensionによって追加する
    /// Tile Propertyのデータです。
    /// </summary>
    public sealed class BigCraftableTilePropertyData
    {
        //----------------------------------------
        // Id
        //----------------------------------------

        /// <summary>
        /// Tile Propertyの識別IDです。
        /// </summary>
        public string Id
        {
            get;
            set;
        } = string.Empty;

        //----------------------------------------
        // Name
        //----------------------------------------

        /// <summary>
        /// Tile Property名です。
        /// </summary>
        public string Name
        {
            get;
            set;
        } = string.Empty;

        //----------------------------------------
        // Value
        //----------------------------------------

        /// <summary>
        /// Tile Propertyの値です。
        /// </summary>
        public string Value
        {
            get;
            set;
        } = string.Empty;

        //----------------------------------------
        // Layer
        //----------------------------------------

        /// <summary>
        /// Tile Propertyを設定するLayer名です。
        /// </summary>
        public string Layer
        {
            get;
            set;
        } = "Back";

        //----------------------------------------
        // Tiles
        //----------------------------------------

        private List<string>? tiles;

        private readonly HashSet<Point> tileOffsets =
            new();

        /// <summary>
        /// Tile Propertyを設定するTileです。
        /// BigCraftableの設置タイルを基準に、
        /// "X, Y"形式の相対座標で指定します。
        /// </summary>
        public List<string>? Tiles
        {
            get
            {
                return tiles;
            }

            set
            {
                tiles =
                    value;

                tileOffsets.Clear();

                if (value == null)
                    return;

                foreach (string tile in value)
                {
                    if (!TryParseTile(
                        tile,
                        out Point offset))
                    {
                        continue;
                    }

                    tileOffsets.Add(
                        offset);
                }
            }
        }

        /// <summary>
        /// Tile Propertyを設定するTileの
        /// 相対座標です。
        /// </summary>
        public IReadOnlySet<Point> TileOffsets =>
            tileOffsets;

        //----------------------------------------
        // TryParseTile
        //----------------------------------------

        /// <summary>
        /// "X, Y"形式の相対座標を解析します。
        /// </summary>
        private static bool TryParseTile(
            string? value,
            out Point offset)
        {
            offset =
                Point.Zero;

            if (string.IsNullOrWhiteSpace(
                value))
            {
                return false;
            }

            string[] parts =
                value.Split(
                    ',');

            if (parts.Length != 2)
                return false;

            if (!int.TryParse(
                parts[0].Trim(),
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out int x))
            {
                return false;
            }

            if (!int.TryParse(
                parts[1].Trim(),
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out int y))
            {
                return false;
            }

            offset =
                new Point(
                    x,
                    y);

            return true;
        }
    }
}