using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using Ts_Core.Models.MapRelated;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.Tiles;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Ts_Core.Services.MapRelated.PostRenovation
{
    /// <summary>
    /// FarmHouseのRenovation適用後に
    /// Map Patchを適用します。
    /// </summary>
    internal static class PostRenovationPatchService
    {
        private static IModHelper? Helper;
        private static IMonitor? Monitor;

        /// <summary>
        /// Post Renovation Patchを適用済みの
        /// FarmHouseを記録します。
        /// </summary>
        private static readonly HashSet<FarmHouse>
            PatchedFarmHouses = new();

        /// <summary>
        /// Data Asset更新後にMap再構築が必要か。
        /// </summary>
        private static bool RefreshPending;

        internal static void Initialize(
            IModHelper helper,
            IMonitor monitor)
        {
            Helper = helper;
            Monitor = monitor;

            helper.Events.Content.AssetsInvalidated
                += OnAssetsInvalidated;

            helper.Events.GameLoop.UpdateTicked
                += OnUpdateTicked;

            helper.Events.GameLoop.ReturnedToTitle
                += OnReturnedToTitle;
        }

        /// <summary>
        /// 現在有効なPost Renovation Patchを
        /// 指定FarmHouseへ適用します。
        /// </summary>
        internal static void ApplyPatches(
            FarmHouse farmHouse)
        {
            if (Helper == null)
                return;

            if (farmHouse.Map == null)
                return;

            string? mapPath =
                farmHouse.mapPath.Value;

            if (string.IsNullOrWhiteSpace(
                    mapPath))
            {
                return;
            }

            Dictionary<string, PostRenovationPatchModel>
                patches;

            try
            {
                patches =
                    Helper.GameContent.Load<
                        Dictionary<
                            string,
                            PostRenovationPatchModel>>(
                                PostRenovationPatchDataService.AssetName);
            }
            catch (Exception ex)
            {
                Monitor?.Log(
                    $"Failed loading " +
                    $"'{PostRenovationPatchDataService.AssetName}'.\n{ex}",
                    LogLevel.Error);
                return;
            }

            bool applied = false;

            foreach ((string id, PostRenovationPatchModel patch)
                in patches)
            {
                if (!IsTargetMap(
                        patch.Target,
                        mapPath))
                {
                    continue;
                }

                try
                {
                    ApplyPatch(
                        farmHouse.Map,
                        patch);

                    applied = true;
                }
                catch (Exception ex)
                {
                    Monitor?.Log(
                        $"Failed applying Post Renovation Patch " +
                        $"'{id}'.\n{ex}",
                        LogLevel.Error);
                }
            }

            if (applied)
            {
                farmHouse.SortLayers();

                PatchedFarmHouses.Add(
                    farmHouse);
            }
        }

        private static bool IsTargetMap(
            string target,
            string mapPath)
        {
            if (Helper == null)
                return false;

            if (string.IsNullOrWhiteSpace(
                    target))
            {
                return false;
            }

            //----------------------------------------
            // 複数Target
            //----------------------------------------

            string[] targets =
                target.Split(
                    ',',
                    StringSplitOptions.RemoveEmptyEntries
                    | StringSplitOptions.TrimEntries);

            foreach (string targetName in targets)
            {
                if (Helper.GameContent
                    .ParseAssetName(targetName)
                    .IsEquivalentTo(mapPath))
                {
                    return true;
                }
            }

            return false;
        }

        private static void ApplyPatch(
            Map targetMap,
            PostRenovationPatchModel patch)
        {
            if (Helper == null)
                return;

            bool hasMapPatch =
                !string.IsNullOrWhiteSpace(
                    patch.FromFile);

            bool hasMapTiles =
                patch.MapTiles?.Count > 0;

            if (!hasMapPatch
                && !hasMapTiles)
            {
                throw new InvalidOperationException(
                    "FromFile and MapTiles are both empty.");
            }

            //----------------------------------------
            // Map Patch
            //----------------------------------------

            if (hasMapPatch)
            {
                ApplyMapPatch(
                    targetMap,
                    patch);
            }

            //----------------------------------------
            // Map Tile Property
            //----------------------------------------

            if (hasMapTiles)
            {
                ApplyMapTiles(
                    targetMap,
                    patch.MapTiles!);
            }
        }

        /// <summary>
        /// FromFileで指定されたMap Patchを適用します。
        /// </summary>
        private static void ApplyMapPatch(
            Map targetMap,
            PostRenovationPatchModel patch)
        {
            if (Helper == null)
                return;

            Map sourceMap =
                Helper.GameContent.Load<Map>(
                    patch.FromFile);

            Rectangle sourceBounds =
                GetMapBounds(sourceMap);

            Rectangle sourceArea =
                patch.FromArea
                ?? sourceBounds;

            Rectangle targetArea =
                patch.ToArea
                ?? new Rectangle(
                    0,
                    0,
                    sourceArea.Width,
                    sourceArea.Height);

            ValidateArea(
                "FromArea",
                sourceArea,
                sourceBounds);

            ValidateArea(
                "ToArea",
                targetArea);

            if (sourceArea.Size
                != targetArea.Size)
            {
                throw new InvalidOperationException(
                    "FromArea and ToArea must have " +
                    "the same width and height.");
            }

            ExtendMap(
                targetMap,
                targetArea.Right,
                targetArea.Bottom);

            Dictionary<TileSheet, TileSheet>
                tileSheets =
                    GetTileSheetMap(
                        sourceMap,
                        targetMap);

            Dictionary<Layer, Layer>
                layers =
                    GetLayerMap(
                        sourceMap,
                        targetMap);

            HashSet<Layer> sourceLayers =
                new(layers.Values);

            bool replaceAll =
                patch.PatchMode
                    == PatchMapMode.Replace;

            bool replaceByLayer =
                patch.PatchMode
                    == PatchMapMode.ReplaceByLayer;

            for (int y = 0;
                y < sourceArea.Height;
                y++)
            {
                for (int x = 0;
                    x < sourceArea.Width;
                    x++)
                {
                    int sourceX =
                        sourceArea.X + x;

                    int sourceY =
                        sourceArea.Y + y;

                    int targetX =
                        targetArea.X + x;

                    int targetY =
                        targetArea.Y + y;

                    if (replaceAll)
                    {
                        foreach (Layer targetLayer
                            in targetMap.Layers)
                        {
                            if (!sourceLayers.Contains(
                                    targetLayer))
                            {
                                targetLayer.Tiles[
                                    targetX,
                                    targetY] = null;
                            }
                        }
                    }

                    foreach ((Layer sourceLayer, Layer targetLayer)
                        in layers)
                    {
                        Tile? sourceTile =
                            sourceLayer.Tiles[
                                sourceX,
                                sourceY];

                        Tile? newTile =
                            CreateTile(
                                sourceTile,
                                targetLayer,
                                tileSheets);

                        if (newTile != null
                            || replaceByLayer
                            || replaceAll)
                        {
                            targetLayer.Tiles[
                                targetX,
                                targetY] = newTile;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 指定されたMap TileへPropertyを設定します。
        /// </summary>
        private static void ApplyMapTiles(
            Map targetMap,
            IEnumerable<PostRenovationMapTileModel> mapTiles)
        {
            foreach (PostRenovationMapTileModel mapTile
                in mapTiles)
            {
                if (string.IsNullOrWhiteSpace(
                        mapTile.Layer))
                {
                    throw new InvalidOperationException(
                        "MapTiles Layer is empty.");
                }

                if (mapTile.Position.X < 0
                    || mapTile.Position.Y < 0)
                {
                    throw new InvalidOperationException(
                        $"MapTiles Position is invalid: " +
                        $"{mapTile.Position}.");
                }

                Layer? layer =
                    targetMap.GetLayer(
                        mapTile.Layer);

                if (layer == null)
                {
                    throw new InvalidOperationException(
                        $"MapTiles Layer '{mapTile.Layer}' " +
                        "was not found.");
                }

                if (mapTile.Position.X >= layer.LayerWidth
                    || mapTile.Position.Y >= layer.LayerHeight)
                {
                    throw new InvalidOperationException(
                        $"MapTiles Position {mapTile.Position} " +
                        $"is outside Layer '{mapTile.Layer}'.");
                }

                Tile? tile =
                    layer.Tiles[
                        mapTile.Position.X,
                        mapTile.Position.Y];

                if (tile == null)
                {
                    throw new InvalidOperationException(
                        $"MapTiles Tile was not found at " +
                        $"'{mapTile.Layer}' {mapTile.Position}.");
                }

                foreach ((string key, string value)
                    in mapTile.SetProperties)
                {
                    tile.Properties[key] =
                        value;
                }
            }
        }

        private static Rectangle GetMapBounds(
            Map map)
        {
            if (map.Layers.Count == 0)
            {
                throw new InvalidOperationException(
                    "Map has no layers.");
            }

            int maxWidth =
                map.Layers.Max(
                    layer =>
                        layer.LayerWidth);

            int maxHeight =
                map.Layers.Max(
                    layer =>
                        layer.LayerHeight);

            return new Rectangle(
                0,
                0,
                maxWidth,
                maxHeight);
        }

        private static void ValidateArea(
            string name,
            Rectangle area,
            Rectangle? bounds = null)
        {
            if (area.Width <= 0
                || area.Height <= 0
                || area.X < 0
                || area.Y < 0)
            {
                throw new InvalidOperationException(
                    $"{name} is invalid: {area}.");
            }

            if (bounds.HasValue
                && (area.Right > bounds.Value.Right
                    || area.Bottom > bounds.Value.Bottom))
            {
                throw new InvalidOperationException(
                    $"{name} is outside the map bounds: " +
                    $"{area} / {bounds.Value}.");
            }
        }

        private static void ExtendMap(
            Map map,
            int minWidth,
            int minHeight)
        {
            foreach (Layer layer in map.Layers)
            {
                if (layer.LayerWidth >= minWidth
                    && layer.LayerHeight >= minHeight)
                {
                    continue;
                }

                int width =
                    Math.Max(
                        minWidth,
                        layer.LayerWidth);

                int height =
                    Math.Max(
                        minHeight,
                        layer.LayerHeight);

                layer.LayerSize =
                    new Size(
                        width,
                        height);
            }
        }

        private static Dictionary<TileSheet, TileSheet>
            GetTileSheetMap(
                Map sourceMap,
                Map targetMap)
        {
            Dictionary<TileSheet, TileSheet> result =
                new();

            foreach (TileSheet sourceSheet
                in sourceMap.TileSheets)
            {
                TileSheet? targetSheet =
                    targetMap.GetTileSheet(
                        sourceSheet.Id);

                if (targetSheet != null
                    && IsSameImageSource(
                        sourceSheet.ImageSource,
                        targetSheet.ImageSource))
                {
                    result[sourceSheet] =
                        targetSheet;
                    continue;
                }

                // 同じPost Renovation Patchが再適用された場合は、
                // 前回追加したTileSheetを再利用します。
                string generatedIdPrefix =
                    "z_" + sourceSheet.Id;

                TileSheet? generatedSheet =
                    targetMap.TileSheets
                        .FirstOrDefault(
                            sheet =>
                                (string.Equals(
                                    sheet.Id,
                                    generatedIdPrefix,
                                    StringComparison.OrdinalIgnoreCase)
                                || sheet.Id.StartsWith(
                                    generatedIdPrefix + "_",
                                    StringComparison.OrdinalIgnoreCase))
                                && IsSameImageSource(
                                    sourceSheet.ImageSource,
                                    sheet.ImageSource));

                if (generatedSheet != null)
                {
                    result[sourceSheet] =
                        generatedSheet;
                    continue;
                }

                string id =
                    generatedIdPrefix;

                string baseId = id;
                int index = 2;

                while (targetMap.GetTileSheet(id)
                    != null)
                {
                    id =
                        baseId + "_" + index++;
                }

                TileSheet newSheet = new TileSheet(
                    id,
                    targetMap,
                    sourceSheet.ImageSource,
                    sourceSheet.SheetSize,
                    sourceSheet.TileSize
                );

                CopyProperties(
                    sourceSheet.Properties,
                    newSheet.Properties);

                targetMap.AddTileSheet(
                    newSheet);

                result[sourceSheet] =
                    newSheet;
            }

            return result;
        }

        private static bool IsSameImageSource(
            string first,
            string second)
        {
            string Normalize(string value) =>
                value.Replace('\\', '/');

            return string.Equals(
                Normalize(first),
                Normalize(second),
                StringComparison.OrdinalIgnoreCase);
        }

        private static Dictionary<Layer, Layer>
            GetLayerMap(
                Map sourceMap,
                Map targetMap)
        {
            Dictionary<Layer, Layer> result =
                new();

            foreach (Layer sourceLayer
                in sourceMap.Layers)
            {
                Layer? targetLayer =
                    targetMap.GetLayer(
                        sourceLayer.Id);

                if (targetLayer == null)
                {
                    targetLayer =
                        new Layer(
                            sourceLayer.Id,
                            targetMap,
                            new Size(
                                GetMapBounds(targetMap).Width,
                                GetMapBounds(targetMap).Height),
                            sourceLayer.TileSize);

                    targetMap.AddLayer(
                        targetLayer);
                }

                CopyProperties(
                    sourceLayer.Properties,
                    targetLayer.Properties);

                result[sourceLayer] =
                    targetLayer;
            }

            return result;
        }

        private static Tile? CreateTile(
            Tile? sourceTile,
            Layer targetLayer,
            Dictionary<TileSheet, TileSheet> tileSheets)
        {
            if (sourceTile == null)
                return null;

            Tile newTile;

            if (sourceTile is AnimatedTile animatedTile)
            {
                StaticTile[] frames =
                    animatedTile.TileFrames
                        .Select(
                            frame =>
                                (StaticTile)CreateTile(
                                    frame,
                                    targetLayer,
                                    tileSheets)!)
                        .ToArray();

                newTile =
                    new AnimatedTile(
                        targetLayer,
                        frames,
                        animatedTile.FrameInterval);
            }
            else if (sourceTile is StaticTile staticTile)
            {
                newTile =
                    new StaticTile(
                        targetLayer,
                        tileSheets[staticTile.TileSheet],
                        staticTile.BlendMode,
                        staticTile.TileIndex);
            }
            else
            {
                throw new InvalidOperationException(
                    $"Unsupported tile type: " +
                    $"{sourceTile.GetType().FullName}");
            }

            CopyProperties(
                sourceTile.Properties,
                newTile.Properties);

            return newTile;
        }

        private static void CopyProperties(
            xTile.ObjectModel.IPropertyCollection source,
            xTile.ObjectModel.IPropertyCollection target)
        {
            foreach (var pair in source)
            {
                target[pair.Key] =
                    pair.Value;
            }
        }

        private static void OnAssetsInvalidated(
            object? sender,
            AssetsInvalidatedEventArgs e)
        {
            if (!e.NamesWithoutLocale.Any(
                    name =>
                        name.IsEquivalentTo(
                            PostRenovationPatchDataService.AssetName)))
            {
                return;
            }

            if (PatchedFarmHouses.Count > 0)
            {
                RefreshPending = true;
            }
        }

        private static void OnUpdateTicked(
            object? sender,
            UpdateTickedEventArgs e)
        {
            if (!RefreshPending)
                return;

            if (!Context.IsWorldReady)
                return;

            RefreshPending = false;

            FarmHouse[] farmHouses =
                PatchedFarmHouses.ToArray();

            PatchedFarmHouses.Clear();

            foreach (FarmHouse farmHouse
                in farmHouses)
            {
                try
                {
                    RefreshFarmHouse(
                        farmHouse);
                }
                catch (Exception ex)
                {
                    Monitor?.Log(
                        $"Failed refreshing FarmHouse " +
                        $"'{farmHouse.NameOrUniqueName}' after " +
                        $"Post Renovation Patch data changed.\n{ex}",
                        LogLevel.Error);
                }
            }
        }

        private static void RefreshFarmHouse(
            FarmHouse farmHouse)
        {
            if (Helper == null)
                return;

            string? mapPath =
                farmHouse.mapPath.Value;

            if (string.IsNullOrWhiteSpace(
                    mapPath))
            {
                return;
            }

            Vector2? playerPosition =
                ReferenceEquals(
                    Game1.currentLocation,
                    farmHouse)
                    ? Game1.player?.Position
                    : null;

            farmHouse.interiorDoors.Clear();

            farmHouse.loadMap(
                mapPath,
                force_reload: true);

            Helper.Reflection
                .GetField<bool>(
                    farmHouse,
                    "displayingSpouseRoom")
                .SetValue(false);

            farmHouse.MakeMapModifications(
                force: true);

            farmHouse.interiorDoors.Clear();
            farmHouse.interiorDoors.ResetSharedState();
            farmHouse.interiorDoors.ResetLocalState();
            farmHouse.updateWarps();
            farmHouse.updateDoors();

            farmHouse.fridgePosition =
                farmHouse.GetFridgePositionFromMap()
                ?? Point.Zero;

            if (playerPosition.HasValue
                && Game1.player != null)
            {
                Game1.player.Position =
                    playerPosition.Value;
            }
        }

        private static void OnReturnedToTitle(
            object? sender,
            ReturnedToTitleEventArgs e)
        {
            PatchedFarmHouses.Clear();
            RefreshPending = false;
        }
    }
}
