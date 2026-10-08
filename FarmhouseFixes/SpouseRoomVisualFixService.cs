using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using Ts_Core.Services.ContentPatcherRelated.ContentPatcherOption;
using xTile.Layers;
using xTile.Tiles;

namespace Ts_Core.Services.FarmhouseFixes
{
    /// <summary>
    /// 配偶者部屋に追加される特殊な表示物を
    /// T's Core Option設定に応じて非表示にします。
    /// </summary>
    internal static class SpouseRoomVisualFixService
    {
        //----------------------------------------
        // Emily 足場
        //----------------------------------------

        private const int EmilyParrotStandTopTileIndex =
            2141;

        private const int EmilyParrotStandBottomTileIndex =
            2173;

        private const string EmilyParrotStandTextureName =
            "Maps/townInterior";

        //----------------------------------------
        // Service状態
        //----------------------------------------

        private static IModHelper? helper;

        private static FarmHouse? pendingFarmHouse;

        private static bool updatePending;

        //----------------------------------------
        // Emily 補完足場保存
        //----------------------------------------

        private static FarmHouse? emilyInjectedStandFarmHouse;

        private static Layer? emilyInjectedStandLayer;

        /// <summary>
        /// TsCoreが補完したEmily足場の
        /// 上側描画Sprite。
        /// </summary>
        private static TemporaryAnimatedSprite?
            emilyInjectedStandTopSprite;

        /// <summary>
        /// TsCoreが補完したEmily足場の
        /// 下側Buildings Tile。
        /// </summary>
        private static Tile?
            emilyInjectedStandBottomTile;

        /// <summary>
        /// TsCoreが補完したEmily足場の
        /// 下側Tile位置。
        /// </summary>
        private static Point
            emilyInjectedStandBottomPosition;

        /// <summary>
        /// TsCoreがEmily足場の下側を補完する前に
        /// その位置に存在していたTile。
        /// </summary>
        private static Tile?
            emilyInjectedStandOriginalBottomTile;

        //----------------------------------------
        // Emily 非表示足場保存
        //----------------------------------------

        private static FarmHouse? emilyStandFarmHouse;

        private static Layer? emilyStandLayer;

        private static readonly Dictionary<
            Point,
            Tile>
            EmilyStandTiles = new();

        /// <summary>
        /// Emilyのオウム用足場を削除したことで
        /// 進入可能になった上側タイルの座標。
        /// </summary>
        private static readonly HashSet<Point>
            EmilyStandCollisionTiles = new();

        //----------------------------------------
        // Sebastian 水槽衝突判定
        //----------------------------------------

        private static FarmHouse? sebastianTankFarmHouse;

        /// <summary>
        /// Sebastianのカエル用水槽が表示されている時に
        /// 進入不可にするタイルの座標。
        /// </summary>
        private static readonly HashSet<Point>
            SebastianTankCollisionTiles = new();

        //----------------------------------------
        // 更新要求
        //----------------------------------------

        /// <summary>
        /// 指定したFarmHouseの配偶者部屋表示物を
        /// 次回Update時に更新するよう要求します。
        /// </summary>
        internal static void RequestRefresh(
            FarmHouse farmHouse)
        {
            pendingFarmHouse =
                farmHouse;

            //----------------------------------------
            // 既にUpdate待ち
            //----------------------------------------

            if (updatePending)
                return;

            if (helper == null)
                return;

            //----------------------------------------
            // 次回Updateを予約
            //----------------------------------------

            updatePending = true;

            helper.Events.GameLoop.UpdateTicked +=
                OnUpdateTicked;
        }

        //----------------------------------------
        // Update
        //----------------------------------------

        /// <summary>
        /// 予約された配偶者部屋表示更新を実行します。
        /// </summary>
        private static void OnUpdateTicked(
            object? sender,
            UpdateTickedEventArgs e)
        {
            //----------------------------------------
            // Event解除
            //----------------------------------------

            if (helper != null)
            {
                helper.Events.GameLoop.UpdateTicked -=
                    OnUpdateTicked;
            }

            updatePending = false;

            //----------------------------------------
            // 更新実行
            //----------------------------------------

            Update();
        }

        //----------------------------------------
        // 更新
        //----------------------------------------

        /// <summary>
        /// 更新要求されているFarmHouseの
        /// 配偶者部屋表示物を処理します。
        /// </summary>
        internal static void Update()
        {
            FarmHouse? farmHouse =
                pendingFarmHouse;

            if (farmHouse == null)
                return;

            pendingFarmHouse = null;

            //----------------------------------------
            // Emily
            //----------------------------------------

            if (ContentPatcherOptionService.IsEnabled(
                    ContentPatcherOptionIds.HideEmilyParrot))
            {
                //----------------------------------------
                // TsCoreが補完していた足場がある場合は
                // 元の状態へ戻してから非表示処理
                //----------------------------------------

                RemoveInjectedEmilyParrotStand(
                    farmHouse);

                RemoveEmilyParrot(
                    farmHouse);

                HideEmilyParrotStand(
                    farmHouse);
            }
            else
            {
                RestoreEmilyParrotStand(
                    farmHouse);

                EnsureEmilyParrotStand(
                    farmHouse);
            }

            //----------------------------------------
            // Sebastian
            //----------------------------------------

            if (ContentPatcherOptionService.IsEnabled(
                    ContentPatcherOptionIds.HideSebastianFrog))
            {
                ClearSebastianTankCollision();

                RemoveSebastianFrogs(
                    farmHouse);
            }
            else
            {
                UpdateSebastianTankCollision(
                    farmHouse);
            }
        }

        //----------------------------------------
        // Emily オウム
        //----------------------------------------

        /// <summary>
        /// Emilyの配偶者部屋のオウムを削除します。
        /// </summary>
        private static void RemoveEmilyParrot(
            FarmHouse farmHouse)
        {
            for (int i =
                     farmHouse.TemporarySprites.Count - 1;
                 i >= 0;
                 i--)
            {
                TemporaryAnimatedSprite sprite =
                    farmHouse.TemporarySprites[i];

                if (sprite is EmilysParrot)
                {
                    farmHouse.TemporarySprites.RemoveAt(
                        i);
                }
            }
        }

        //----------------------------------------
        // Emily 足場非表示
        //----------------------------------------

        /// <summary>
        /// Emilyのオウム用足場を非表示にします。
        /// </summary>
        private static void HideEmilyParrotStand(
            FarmHouse farmHouse)
        {
            Layer? layer =
                farmHouse.Map.GetLayer(
                    "Buildings");

            if (layer == null)
                return;

            //----------------------------------------
            // Mapが変更されていた場合は
            // 古い保存情報を破棄
            //----------------------------------------

            if (emilyStandLayer != null
                && !ReferenceEquals(
                    emilyStandLayer,
                    layer))
            {
                ClearEmilyParrotStandState();
            }

            //----------------------------------------
            // 既にこのLayerの足場を保存済み
            //----------------------------------------

            if (ReferenceEquals(
                    emilyStandFarmHouse,
                    farmHouse)
                && ReferenceEquals(
                    emilyStandLayer,
                    layer)
                && EmilyStandTiles.Count > 0)
            {
                return;
            }

            //----------------------------------------
            // 2141 / 2173 の上下ペアを検索
            //----------------------------------------

            for (int x = 0;
                 x < layer.LayerWidth;
                 x++)
            {
                for (int y = 0;
                     y < layer.LayerHeight - 1;
                     y++)
                {
                    Tile? topTile =
                        layer.Tiles[x, y];

                    Tile? bottomTile =
                        layer.Tiles[x, y + 1];

                    if (topTile?.TileIndex
                            != EmilyParrotStandTopTileIndex
                        || bottomTile?.TileIndex
                            != EmilyParrotStandBottomTileIndex)
                    {
                        continue;
                    }

                    //----------------------------------------
                    // 元Tile保存
                    //----------------------------------------

                    EmilyStandTiles[
                        new Point(
                            x,
                            y)] =
                        topTile;

                    EmilyStandTiles[
                        new Point(
                            x,
                            y + 1)] =
                        bottomTile;

                    //----------------------------------------
                    // 上側は壁部分なので進入禁止
                    //----------------------------------------

                    EmilyStandCollisionTiles.Add(
                        new Point(
                            x,
                            y));

                    //----------------------------------------
                    // 足場非表示
                    //----------------------------------------

                    layer.Tiles[x, y] =
                        null;

                    layer.Tiles[x, y + 1] =
                        null;
                }
            }

            if (EmilyStandTiles.Count == 0)
                return;

            emilyStandFarmHouse =
                farmHouse;

            emilyStandLayer =
                layer;
        }

        //----------------------------------------
        // Emily 足場復元
        //----------------------------------------

        /// <summary>
        /// 非表示にしていたEmilyのオウム用足場を
        /// 元のTileへ復元します。
        /// </summary>
        private static void RestoreEmilyParrotStand(
            FarmHouse farmHouse)
        {
            if (!ReferenceEquals(
                    emilyStandFarmHouse,
                    farmHouse))
            {
                return;
            }

            if (emilyStandLayer == null)
            {
                ClearEmilyParrotStandState();
                return;
            }

            //----------------------------------------
            // 現在のMapと保存時のLayerが異なる場合は
            // 新Map側には復元しない
            //----------------------------------------

            Layer? currentLayer =
                farmHouse.Map.GetLayer(
                    "Buildings");

            if (!ReferenceEquals(
                    currentLayer,
                    emilyStandLayer))
            {
                ClearEmilyParrotStandState();
                return;
            }

            //----------------------------------------
            // Tile復元
            //----------------------------------------

            foreach (
                KeyValuePair<Point, Tile> pair
                in EmilyStandTiles)
            {
                Point position =
                    pair.Key;

                emilyStandLayer.Tiles[
                    position.X,
                    position.Y] =
                    pair.Value;
            }

            ClearEmilyParrotStandState();
        }

        //----------------------------------------
        // Emily 足場状態破棄
        //----------------------------------------

        /// <summary>
        /// 保存しているEmily足場情報を破棄します。
        /// </summary>
        private static void ClearEmilyParrotStandState()
        {
            EmilyStandTiles.Clear();

            EmilyStandCollisionTiles.Clear();

            emilyStandFarmHouse =
                null;

            emilyStandLayer =
                null;
        }

        //----------------------------------------
        // Emily 足場衝突判定
        //----------------------------------------

        /// <summary>
        /// 非表示にしたEmilyのオウム用足場の
        /// 上側へプレイヤーが進入しようとしているか
        /// 判定します。
        /// </summary>
        internal static bool IsEmilyParrotStandCollision(
            FarmHouse farmHouse,
            Rectangle position,
            Farmer farmer)
        {
            //----------------------------------------
            // 対象FarmHouse以外
            //----------------------------------------

            if (!ReferenceEquals(
                    emilyStandFarmHouse,
                    farmHouse))
            {
                return false;
            }

            if (EmilyStandCollisionTiles.Count == 0)
                return false;

            //----------------------------------------
            // 現在位置
            //----------------------------------------

            Rectangle currentBounds =
                farmer.GetBoundingBox();

            //----------------------------------------
            // 進入禁止Tileとの衝突確認
            //----------------------------------------

            foreach (Point tilePosition
                     in EmilyStandCollisionTiles)
            {
                Rectangle tileBounds =
                    new Rectangle(
                        tilePosition.X * 64,
                        tilePosition.Y * 64,
                        64,
                        64);

                //----------------------------------------
                // 次の位置が対象Tileに入らない
                //----------------------------------------

                if (!position.Intersects(
                        tileBounds))
                {
                    continue;
                }

                //----------------------------------------
                // 既に対象Tile内にいる場合は
                // 外へ脱出できるよう妨げない
                //----------------------------------------

                if (currentBounds.Intersects(
                        tileBounds))
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        //----------------------------------------
        // Emily 足場補完
        //----------------------------------------

        /// <summary>
        /// Emilyのオウムが表示されている場合、
        /// 対応する足場が存在しなければ補完します。
        ///
        /// 上側は描画専用Spriteとして追加し、
        /// Buildingsの元Tileを変更しません。
        ///
        /// 下側はBuildingsへ追加し、
        /// 通常のMap衝突判定を利用します。
        /// </summary>
        private static void EnsureEmilyParrotStand(
            FarmHouse farmHouse)
        {
            Layer? layer =
                farmHouse.Map.GetLayer(
                    "Buildings");

            if (layer == null)
            {
                RemoveInjectedEmilyParrotStand(
                    farmHouse);

                return;
            }

            //----------------------------------------
            // Mapが再構築された場合
            //----------------------------------------

            if (emilyInjectedStandLayer != null
                && !ReferenceEquals(
                    emilyInjectedStandLayer,
                    layer))
            {
                if (ReferenceEquals(
                        emilyInjectedStandFarmHouse,
                        farmHouse)
                    && emilyInjectedStandTopSprite != null)
                {
                    farmHouse.TemporarySprites.Remove(
                        emilyInjectedStandTopSprite);
                }

                ClearInjectedEmilyParrotStandState();
            }

            //----------------------------------------
            // Emilyのオウムを検索
            //----------------------------------------

            EmilysParrot? parrot =
                null;

            foreach (TemporaryAnimatedSprite sprite
                     in farmHouse.TemporarySprites)
            {
                if (sprite is EmilysParrot emilysParrot)
                {
                    parrot =
                        emilysParrot;

                    break;
                }
            }

            //----------------------------------------
            // オウム自体が存在しない場合
            //----------------------------------------

            if (parrot == null)
            {
                RemoveInjectedEmilyParrotStand(
                    farmHouse);

                return;
            }

            //----------------------------------------
            // オウムの初期位置から足場位置を取得
            //----------------------------------------

            Vector2 parrotPosition =
                parrot.initialPosition;

            int tileX =
                (int)(parrotPosition.X / 64f);

            int tileY =
                (int)(parrotPosition.Y / 64f) + 1;

            //----------------------------------------
            // Map範囲外
            //----------------------------------------

            if (tileX < 0
                || tileY < 0
                || tileX >= layer.LayerWidth
                || tileY + 1 >= layer.LayerHeight)
            {
                return;
            }

            Point topPosition =
                new Point(
                    tileX,
                    tileY);

            Point bottomPosition =
                new Point(
                    tileX,
                    tileY + 1);

            Tile? topTile =
                layer.Tiles[
                    topPosition.X,
                    topPosition.Y];

            Tile? bottomTile =
                layer.Tiles[
                    bottomPosition.X,
                    bottomPosition.Y];

            //----------------------------------------
            // Vanilla形式の正しい足場が
            // Buildingsに既に存在する場合
            //----------------------------------------

            if (topTile?.TileIndex
                    == EmilyParrotStandTopTileIndex
                && bottomTile?.TileIndex
                    == EmilyParrotStandBottomTileIndex)
            {
                //----------------------------------------
                // 以前TsCoreが補完していたものが
                // 残っていれば削除
                //----------------------------------------

                RemoveInjectedEmilyParrotStand(
                    farmHouse);

                return;
            }

            //----------------------------------------
            // TileSheet取得
            //----------------------------------------

            TileSheet? tileSheet =
                farmHouse.Map.GetTileSheet(
                    "indoor");

            if (tileSheet == null)
                return;

            //----------------------------------------
            // 既に同じ位置へTsCoreが補完済みなら
            // そのまま使用
            //----------------------------------------

            if (ReferenceEquals(
                    emilyInjectedStandFarmHouse,
                    farmHouse)
                && ReferenceEquals(
                    emilyInjectedStandLayer,
                    layer)
                && emilyInjectedStandTopSprite != null
                && emilyInjectedStandBottomTile != null
                && emilyInjectedStandBottomPosition
                    == bottomPosition
                && farmHouse.TemporarySprites.Contains(
                    emilyInjectedStandTopSprite)
                && ReferenceEquals(
                    layer.Tiles[
                        bottomPosition.X,
                        bottomPosition.Y],
                    emilyInjectedStandBottomTile))
            {
                return;
            }

            //----------------------------------------
            // 古い補完が存在する場合は
            // 先に元の状態へ戻す
            //----------------------------------------

            RemoveInjectedEmilyParrotStand(
                farmHouse);

            //----------------------------------------
            // Remove後に下側Tileを再取得
            //----------------------------------------

            bottomTile =
                layer.Tiles[
                    bottomPosition.X,
                    bottomPosition.Y];

            //----------------------------------------
            // 上側SpriteのSourceRect取得
            //----------------------------------------

            xTile.Dimensions.Rectangle tileBounds =
                tileSheet.GetTileImageBounds(
                    EmilyParrotStandTopTileIndex);

            Rectangle sourceRect =
                new Rectangle(
                    tileBounds.X,
                    tileBounds.Y,
                    tileBounds.Width,
                    tileBounds.Height);

            //----------------------------------------
            // 上側Sprite位置
            //----------------------------------------

            Vector2 topSpritePosition =
                new Vector2(
                    topPosition.X * 64f,
                    topPosition.Y * 64f);

            //----------------------------------------
            // 上側Sprite作成
            //----------------------------------------

            TemporaryAnimatedSprite topSprite =
                new TemporaryAnimatedSprite(
                    EmilyParrotStandTextureName,
                    sourceRect,
                    999999f,
                    1,
                    9999,
                    topSpritePosition,
                    false,
                    false,
                    (parrotPosition.Y - 1f)
                        / 10000f,
                    0f,
                    Color.White,
                    4f,
                    0f,
                    0f,
                    0f);

            //----------------------------------------
            // 下側Buildings Tile作成
            //----------------------------------------

            StaticTile newBottomTile =
                new StaticTile(
                    layer,
                    tileSheet,
                    BlendMode.Alpha,
                    EmilyParrotStandBottomTileIndex);

            //----------------------------------------
            // 下側の元Tileを保存
            //----------------------------------------

            emilyInjectedStandOriginalBottomTile =
                bottomTile;

            //----------------------------------------
            // 下側だけBuildingsへ配置
            //----------------------------------------

            layer.Tiles[
                bottomPosition.X,
                bottomPosition.Y] =
                newBottomTile;

            //----------------------------------------
            // 上側はSpriteとして追加
            //----------------------------------------

            farmHouse.TemporarySprites.Add(
                topSprite);

            //----------------------------------------
            // TsCoreが追加したものを保存
            //----------------------------------------

            emilyInjectedStandFarmHouse =
                farmHouse;

            emilyInjectedStandLayer =
                layer;

            emilyInjectedStandTopSprite =
                topSprite;

            emilyInjectedStandBottomTile =
                newBottomTile;

            emilyInjectedStandBottomPosition =
                bottomPosition;
        }

        //----------------------------------------
        // Emily 補完足場削除
        //----------------------------------------

        /// <summary>
        /// TsCoreが補完したEmilyの足場を削除します。
        ///
        /// 上側Spriteを削除し、
        /// 下側Buildings Tileは補完前のTileへ戻します。
        /// </summary>
        private static void RemoveInjectedEmilyParrotStand(
            FarmHouse farmHouse)
        {
            if (!ReferenceEquals(
                    emilyInjectedStandFarmHouse,
                    farmHouse))
            {
                ClearInjectedEmilyParrotStandState();
                return;
            }

            //----------------------------------------
            // 上側Sprite削除
            //----------------------------------------

            if (emilyInjectedStandTopSprite != null)
            {
                farmHouse.TemporarySprites.Remove(
                    emilyInjectedStandTopSprite);
            }

            //----------------------------------------
            // 下側Buildings Tile復元
            //----------------------------------------

            Layer? layer =
                emilyInjectedStandLayer;

            if (layer != null)
            {
                Layer? currentLayer =
                    farmHouse.Map.GetLayer(
                        "Buildings");

                if (ReferenceEquals(
                        currentLayer,
                        layer))
                {
                    Point position =
                        emilyInjectedStandBottomPosition;

                    if (position.X >= 0
                        && position.Y >= 0
                        && position.X < layer.LayerWidth
                        && position.Y < layer.LayerHeight)
                    {
                        //----------------------------------------
                        // TsCoreが配置したTileが
                        // まだその位置にある場合だけ復元
                        //----------------------------------------

                        if (emilyInjectedStandBottomTile != null
                            && ReferenceEquals(
                                layer.Tiles[
                                    position.X,
                                    position.Y],
                                emilyInjectedStandBottomTile))
                        {
                            layer.Tiles[
                                position.X,
                                position.Y] =
                                emilyInjectedStandOriginalBottomTile;
                        }
                    }
                }
            }

            ClearInjectedEmilyParrotStandState();
        }

        //----------------------------------------
        // Emily 補完足場状態破棄
        //----------------------------------------

        /// <summary>
        /// TsCoreが補完したEmily足場の
        /// 保存情報を破棄します。
        /// </summary>
        private static void ClearInjectedEmilyParrotStandState()
        {
            emilyInjectedStandFarmHouse =
                null;

            emilyInjectedStandLayer =
                null;

            emilyInjectedStandTopSprite =
                null;

            emilyInjectedStandBottomTile =
                null;

            emilyInjectedStandBottomPosition =
                default;

            emilyInjectedStandOriginalBottomTile =
                null;
        }

        //----------------------------------------
        // Sebastian
        //----------------------------------------

        /// <summary>
        /// Sebastianの配偶者部屋の
        /// カエルと水槽を削除します。
        /// </summary>
        private static void RemoveSebastianFrogs(
            FarmHouse farmHouse)
        {
            for (int i =
                     farmHouse.TemporarySprites.Count - 1;
                 i >= 0;
                 i--)
            {
                TemporaryAnimatedSprite sprite =
                    farmHouse.TemporarySprites[i];

                if (sprite is SebsFrogs
                    || IsSebastianFrogTank(
                        sprite))
                {
                    farmHouse.TemporarySprites.RemoveAt(
                        i);
                }
            }
        }

        //----------------------------------------
        // Sebastian 水槽判定
        //----------------------------------------

        /// <summary>
        /// Sebastianの配偶者部屋に表示される
        /// カエル用水槽Spriteか判定します。
        /// </summary>
        private static bool IsSebastianFrogTank(
            TemporaryAnimatedSprite sprite)
        {
            return sprite.texture
                    == Game1.mouseCursors
                && sprite.sourceRect
                    == new Rectangle(
                        641,
                        1534,
                        48,
                        37)
                && sprite.sourceRectStartingPos
                    == new Vector2(
                        641f,
                        1534f);
        }

        //----------------------------------------
        // Sebastian 水槽衝突位置更新
        //----------------------------------------

        /// <summary>
        /// 実際に表示されているSebastianの水槽位置から
        /// 進入不可タイルを設定します。
        /// </summary>
        private static void UpdateSebastianTankCollision(
            FarmHouse farmHouse)
        {
            //----------------------------------------
            // 古い情報を破棄
            //----------------------------------------

            ClearSebastianTankCollision();

            //----------------------------------------
            // 水槽Spriteを検索
            //----------------------------------------

            foreach (TemporaryAnimatedSprite sprite
                     in farmHouse.TemporarySprites)
            {
                if (!IsSebastianFrogTank(
                        sprite))
                {
                    continue;
                }

                //----------------------------------------
                // 水槽Spriteのpositionは
                // 基準Tileより20px上に配置される
                //----------------------------------------

                int tileX =
                    (int)(sprite.position.X / 64f);

                int tileY =
                    (int)((sprite.position.Y + 20f)
                        / 64f);

                //----------------------------------------
                // 水槽基準位置から3x2Tileを
                // 進入不可にする
                //----------------------------------------

                for (int x = 0;
                     x < 3;
                     x++)
                {
                    for (int y = 0;
                         y < 2;
                         y++)
                    {
                        SebastianTankCollisionTiles.Add(
                            new Point(
                                tileX + x,
                                tileY + y));
                    }
                }

                //----------------------------------------
                // 対象FarmHouseを保存
                //----------------------------------------

                sebastianTankFarmHouse =
                    farmHouse;

                //----------------------------------------
                // Sebastianの水槽は1つだけ
                //----------------------------------------

                return;
            }
        }

        //----------------------------------------
        // Sebastian 水槽衝突状態破棄
        //----------------------------------------

        /// <summary>
        /// 保存しているSebastian水槽の
        /// 衝突情報を破棄します。
        /// </summary>
        private static void ClearSebastianTankCollision()
        {
            SebastianTankCollisionTiles.Clear();

            sebastianTankFarmHouse =
                null;
        }

        //----------------------------------------
        // Sebastian 水槽衝突判定
        //----------------------------------------

        /// <summary>
        /// Sebastianのカエル用水槽へ
        /// プレイヤーが進入しようとしているか判定します。
        /// </summary>
        internal static bool IsSebastianFrogTankCollision(
            FarmHouse farmHouse,
            Rectangle position,
            Farmer farmer)
        {
            //----------------------------------------
            // 対象FarmHouse以外
            //----------------------------------------

            if (!ReferenceEquals(
                    sebastianTankFarmHouse,
                    farmHouse))
            {
                return false;
            }

            if (SebastianTankCollisionTiles.Count == 0)
                return false;

            //----------------------------------------
            // 現在位置
            //----------------------------------------

            Rectangle currentBounds =
                farmer.GetBoundingBox();

            //----------------------------------------
            // 進入禁止Tileとの衝突確認
            //----------------------------------------

            foreach (Point tilePosition
                     in SebastianTankCollisionTiles)
            {
                Rectangle tileBounds =
                    new Rectangle(
                        tilePosition.X * 64,
                        tilePosition.Y * 64,
                        64,
                        64);

                //----------------------------------------
                // 次の位置が対象Tileに入らない
                //----------------------------------------

                if (!position.Intersects(
                        tileBounds))
                {
                    continue;
                }

                //----------------------------------------
                // 既に対象Tile内にいる場合は
                // 外へ脱出できるよう妨げない
                //----------------------------------------

                if (currentBounds.Intersects(
                        tileBounds))
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        //----------------------------------------
        // 初期化
        //----------------------------------------

        /// <summary>
        /// Serviceを初期化します。
        /// </summary>
        internal static void Initialize(
            IModHelper modHelper)
        {
            helper =
                modHelper;
        }
    }
}