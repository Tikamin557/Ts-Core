using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using xTile.Dimensions;
using xTile.Tiles;

namespace Ts_Core.Services.MapRelated.TsCoreDecoratableCellar
{
    /// <summary>
    /// Data/AdditionalWallpaperFlooring に登録された追加素材のタイルシートを、
    /// DecoratableLocation.GetWallAndFloorTilesheet と同じ形式で Cellar マップへ登録します。
    /// </summary>
    internal static class TsCoreDecoratableCellarAdditionalTilesheets
    {
        //----------------------------------------
        // Additional Material Registration
        //----------------------------------------

        /// <summary>
        /// 既存シートを優先し、未登録の場合だけ追加素材の定義とテクスチャを確認して登録します。
        /// 未定義やロード失敗時は通常の壁紙・床材処理に影響を与えず false を返します。
        /// </summary>
        internal static bool TryResolve(GameLocation location, string materialSetId, out string tileSheetId)
        {
            tileSheetId = "x_WallsAndFloors_" + materialSetId;
            var map = location.Map;
            if (map == null || string.IsNullOrWhiteSpace(materialSetId))
                return false;

            if (map.GetTileSheet(tileSheetId) != null)
                return true;

            try
            {
                foreach (var material in DataLoader.AdditionalWallpaperFlooring(Game1.content))
                {
                    if (!string.Equals(material.Id, materialSetId, StringComparison.Ordinal))
                        continue;

                    Texture2D texture = Game1.content.Load<Texture2D>(material.Texture);
                    if (texture.Width != 256 || texture.Height < 16 || texture.Height % 16 != 0)
                        return false;

                    var sheet = new TileSheet(tileSheetId, map, material.Texture,
                        new Size(texture.Width / 16, texture.Height / 16), new Size(16, 16));
                    map.AddTileSheet(sheet);
                    try
                    {
                        map.LoadTileSheets(Game1.mapDisplayDevice);
                        return true;
                    }
                    catch
                    {
                        // 読み込み失敗時は追加したシートをマップから取り除きます。
                        map.RemoveTileSheet(sheet);
                        throw;
                    }
                }
            }
            catch (Exception)
            {
                // 未登録の素材やロードに失敗した素材は安全にスキップします。
            }

            return false;
        }
    }
}
