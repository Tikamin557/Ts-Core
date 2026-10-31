using StardewValley;
using xTile.Tiles;

namespace Ts_Core.Services.MapRelated.TsCoreDecoratableCellar
{
    /// <summary>
    /// 壁紙・床材の複数タイル更新をまとめて実行し、例外時は変更前の状態へ戻します。
    /// GameLocation.setMapTileは既存StaticTileを直接変更する場合があるため、
    /// 変更前のタイルを別インスタンスとして複製しておきます。
    /// </summary>
    internal static class TsCoreDecoratableCellarTileTransaction
    {
        internal readonly record struct Replacement(int X, int Y, int Index, string LayerName);

        internal static bool TryApply(GameLocation location, string tileSheetId, IEnumerable<Replacement> replacements)
        {
            var snapshots = new List<(xTile.Layers.Layer Layer, int X, int Y, StaticTile Tile)>();
            try
            {
                foreach (var item in replacements)
                {
                    var layer = location.Map?.GetLayer(item.LayerName);
                    if (layer == null || layer.Tiles[item.X, item.Y] is not StaticTile original)
                        continue;

                    var copy = new StaticTile(layer, original.TileSheet, original.BlendMode, original.TileIndex);
                    foreach (var property in original.Properties)
                        copy.Properties[property.Key] = property.Value;
                    snapshots.Add((layer, item.X, item.Y, copy));
                    location.setMapTile(item.X, item.Y, item.Index, item.LayerName, tileSheetId, null, true);
                }
                return snapshots.Count > 0;
            }
            catch
            {
                // 既存タイルを直接変更する場合もあるため、全スナップショットを戻します。
                foreach (var snapshot in snapshots)
                    snapshot.Layer.Tiles[snapshot.X, snapshot.Y] = snapshot.Tile;
                throw;
            }
        }
    }
}
