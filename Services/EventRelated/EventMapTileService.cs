using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using xTile.Tiles;

namespace Ts_Core.Services.EventRelated
{
    /// <summary>
    /// イベントコマンド tsCoreChangeMapTile を登録し、
    /// AnimatedTileを含むタイルをイベント中だけStaticTileに差し替えます。
    /// </summary>
    internal static class EventMapTileService
    {
        //----------------------------------------
        // 変更前のタイル
        //----------------------------------------

        private sealed record TileChange(
            Event Event,
            GameLocation Location,
            string LayerName,
            int X,
            int Y,
            Tile OriginalTile);

        private static readonly List<TileChange> Changes = new();
        private static IMonitor monitor = null!;

        //----------------------------------------
        // 初期化
        //----------------------------------------

        /// <summary>
        /// イベントコマンドと終了検出用の更新イベントを登録します。
        /// </summary>
        public static void Initialize(IModHelper helper, IMonitor log)
        {
            monitor = log;
            Event.RegisterCommand("tsCoreChangeMapTile", ChangeMapTile);
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            helper.Events.GameLoop.ReturnedToTitle += OnReturnedToTitle;
        }

        //----------------------------------------
        // イベントコマンド
        //----------------------------------------

        /// <summary>
        /// changeMapTileと同じ引数を受け取り、元のタイルを保存して置き換えます。
        /// </summary>
        private static void ChangeMapTile(Event evt, string[] args, EventContext context)
        {
            try
            {
                if (args.Length != 5
                    || !int.TryParse(args[2], out int x)
                    || !int.TryParse(args[3], out int y)
                    || !int.TryParse(args[4], out int tileIndex))
                {
                    monitor.Log("tsCoreChangeMapTile: expected layer, x, y and tile index.", LogLevel.Warn);
                    return;
                }

                var location = context.Location;
                var layer = location.Map.GetLayer(args[1]);
                if (layer == null)
                {
                    monitor.Log($"tsCoreChangeMapTile: layer '{args[1]}' was not found.", LogLevel.Warn);
                    return;
                }

                x = evt.OffsetTileX(x);
                y = evt.OffsetTileY(y);
                if (x < 0 || y < 0 || x >= layer.LayerWidth || y >= layer.LayerHeight)
                {
                    monitor.Log($"tsCoreChangeMapTile: tile ({x}, {y}) is outside layer '{args[1]}'.", LogLevel.Warn);
                    return;
                }

                var original = layer.Tiles[x, y];
                if (original == null)
                {
                    monitor.Log($"tsCoreChangeMapTile: tile ({x}, {y}) on '{args[1]}' is empty.", LogLevel.Warn);
                    return;
                }

                // 同じイベントで同じタイルを繰り返し変更しても、最初の状態を復元します。
                if (!Changes.Any(change => ReferenceEquals(change.Event, evt)
                    && ReferenceEquals(change.Location, location)
                    && change.LayerName == args[1] && change.X == x && change.Y == y))
                {
                    Changes.Add(new TileChange(evt, location, args[1], x, y, original));
                }

                var replacement = new StaticTile(layer, original.TileSheet, original.BlendMode, tileIndex);
                foreach (var property in original.Properties)
                {
                    replacement.Properties[property.Key] = property.Value;
                }
                layer.Tiles[x, y] = replacement;
            }
            catch (Exception ex)
            {
                monitor.Log($"tsCoreChangeMapTile failed: {ex}", LogLevel.Error);
            }
            finally
            {
                // 通常のchangeMapTileと同様に次のイベントコマンドへ進みます。
                evt.CurrentCommand++;
            }
        }

        //----------------------------------------
        // イベント終了時の復元
        //----------------------------------------

        /// <summary>
        /// 通常終了・スキップ・ロケーション移動でイベントが外れた後に復元します。
        /// </summary>
        private static void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            if (Changes.Count == 0)
            {
                return;
            }

            for (int i = Changes.Count - 1; i >= 0; i--)
            {
                var change = Changes[i];
                if (ReferenceEquals(change.Location.currentEvent, change.Event))
                {
                    continue;
                }

                Restore(change);
                Changes.RemoveAt(i);
            }
        }

        /// <summary>
        /// タイトルへ戻った際も変更を残さないようにします。
        /// </summary>
        private static void OnReturnedToTitle(object? sender, ReturnedToTitleEventArgs e)
        {
            foreach (var change in Changes)
            {
                Restore(change);
            }
            Changes.Clear();
        }

        private static void Restore(TileChange change)
        {
            try
            {
                var layer = change.Location.Map.GetLayer(change.LayerName);
                if (layer != null && change.X >= 0 && change.Y >= 0
                    && change.X < layer.LayerWidth && change.Y < layer.LayerHeight)
                {
                    layer.Tiles[change.X, change.Y] = change.OriginalTile;
                }
            }
            catch (Exception ex)
            {
                monitor.Log($"tsCoreChangeMapTile restore failed: {ex}", LogLevel.Warn);
            }
        }
    }
}
