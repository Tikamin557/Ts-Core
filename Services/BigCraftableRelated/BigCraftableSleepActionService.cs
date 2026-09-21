using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewValley;
using Ts_Core.Models.BigCraftableRelated;

namespace Ts_Core.Services.BigCraftableRelated
{
    /// <summary>
    /// BigCraftableから使用できる
    /// 睡眠TileActionを管理するサービスです。
    /// </summary>
    public static class BigCraftableSleepActionService
    {
        //----------------------------------------
        // State
        //----------------------------------------

        /// <summary>
        /// TsCore_Sleepによる
        /// 睡眠確認中かどうか。
        /// </summary>
        public static bool IsSleepQuestionActive
        {
            get;
            private set;
        }

        /// <summary>
        /// 睡眠確認中のBigCraftableを
        /// 翌朝削除するかどうか。
        /// </summary>
        private static bool pendingRemove;

        /// <summary>
        /// 睡眠確認中のBigCraftableがある
        /// Location名です。
        /// </summary>
        private static string? pendingLocationName;

        /// <summary>
        /// 睡眠確認中のBigCraftableの
        /// Anchor Tileです。
        /// </summary>
        private static Vector2 pendingTile;

        /// <summary>
        /// 睡眠確認中のBigCraftableの
        /// Qualified Item IDです。
        /// </summary>
        private static string? pendingQualifiedItemId;

        /// <summary>
        /// 翌朝削除するBigCraftableがある
        /// Location名です。
        /// </summary>
        private static string? removeLocationName;

        /// <summary>
        /// 翌朝削除するBigCraftableの
        /// Anchor Tileです。
        /// </summary>
        private static Vector2 removeTile;

        //----------------------------------------
        // Register
        //----------------------------------------

        /// <summary>
        /// 睡眠Actionを登録します。
        /// </summary>
        public static void Register()
        {
            //----------------------------------------
            // Tile Action
            //----------------------------------------

            GameLocation.RegisterTileAction(
                "TsCore_Sleep",
                Perform);

            //----------------------------------------
            // Touch Action
            //----------------------------------------

            GameLocation.RegisterTouchAction(
                "TsCore_Sleep",
                PerformTouch);
        }

        //----------------------------------------
        // Perform
        //----------------------------------------

        /// <summary>
        /// TileActionから
        /// 睡眠処理を開始します。
        /// </summary>
        private static bool Perform(
            GameLocation location,
            string[] action,
            Farmer player,
            Point tile)
        {
            return PerformSleep(
                location,
                action,
                player,
                new Vector2(
                    tile.X,
                    tile.Y));
        }

        //----------------------------------------
        // PerformTouch
        //----------------------------------------

        /// <summary>
        /// TouchActionから
        /// 睡眠処理を開始します。
        /// </summary>
        private static void PerformTouch(
            GameLocation location,
            string[] action,
            Farmer player,
            Vector2 tile)
        {
            PerformSleep(
                location,
                action,
                player,
                tile);
        }

        //----------------------------------------
        // PerformSleep
        //----------------------------------------

        /// <summary>
        /// 一時的なベッドとして
        /// 睡眠処理を開始します。
        /// </summary>
        private static bool PerformSleep(
            GameLocation location,
            string[] action,
            Farmer player,
            Vector2 actionTile)
        {
            //----------------------------------------
            // Sleep Check
            //----------------------------------------

            if (Game1.newDay
                || !Game1.shouldTimePass()
                || !player.hasMoved
                || player.passedOut)
            {
                return false;
            }

            //----------------------------------------
            // Remove Option
            //----------------------------------------

            bool remove =
                action.Length >= 2
                && string.Equals(
                    action[1],
                    "Remove",
                    StringComparison.OrdinalIgnoreCase);

            //----------------------------------------
            // BigCraftable取得
            //----------------------------------------

            StardewValley.Object? sleepObject =
                null;

            Vector2 anchorTile =
                Vector2.Zero;

            //----------------------------------------
            // Anchor Tile
            //----------------------------------------

            if (location.objects.TryGetValue(
                actionTile,
                out StardewValley.Object? directObject)
                && directObject != null
                && directObject.bigCraftable.Value)
            {
                sleepObject =
                    directObject;

                anchorTile =
                    actionTile;
            }

            //----------------------------------------
            // 仮想Collision Tile
            //----------------------------------------

            else if (BigCraftableExtensionCollisionService
                .TryGetCollisionObject(
                    location,
                    actionTile,
                    out StardewValley.Object collisionObject,
                    out Vector2 collisionAnchor))
            {
                sleepObject =
                    collisionObject;

                anchorTile =
                    collisionAnchor;
            }

            //----------------------------------------
            // BigCraftableなし
            //----------------------------------------

            if (sleepObject == null)
            {
                return false;
            }

            //----------------------------------------
            // Sleep Question State
            //----------------------------------------

            IsSleepQuestionActive =
                true;

            pendingRemove =
                remove;

            pendingLocationName =
                location.NameOrUniqueName;

            pendingTile =
                anchorTile;

            pendingQualifiedItemId =
                sleepObject.QualifiedItemId;

            //----------------------------------------
            // Sleep Question
            //----------------------------------------

            location.createQuestionDialogue(
                Game1.content.LoadString(
                    "Strings\\Locations:FarmHouse_Bed_GoToSleep"),
                location.createYesNoResponses(),
                "SleepTent",
                null);

            return true;
        }

        //----------------------------------------
        // ConfirmSleep
        //----------------------------------------

        /// <summary>
        /// 睡眠を確定し、
        /// 必要な睡眠情報を記録します。
        /// </summary>
        public static void ConfirmSleep()
        {
            //----------------------------------------
            // Remove
            //----------------------------------------

            if (pendingRemove)
            {
                //----------------------------------------
                // 以前の継続寝床を解除
                //----------------------------------------

                BigCraftableSleepPersistenceService.Remove(
                    Game1.player);

                //----------------------------------------
                // 翌朝の削除対象を記録
                //----------------------------------------

                if (!string.IsNullOrWhiteSpace(
                    pendingLocationName))
                {
                    removeLocationName =
                        pendingLocationName;

                    removeTile =
                        pendingTile;
                }

                ClearSleepQuestion();

                return;
            }

            //----------------------------------------
            // 継続寝床
            //----------------------------------------

            if (!string.IsNullOrWhiteSpace(
                pendingLocationName)
                && !string.IsNullOrWhiteSpace(
                    pendingQualifiedItemId))
            {
                BigCraftableSleepPersistenceService.Set(
                    Game1.player,
                    new BigCraftableSleepData
                    {
                        LocationName =
                            pendingLocationName,

                        WakeUpPoint =
                            Game1.player.TilePoint,

                        Anchor =
                            pendingTile,

                        QualifiedItemId =
                            pendingQualifiedItemId
                    });
            }

            ClearSleepQuestion();
        }

        //----------------------------------------
        // ClearSleepQuestion
        //----------------------------------------

        /// <summary>
        /// 睡眠確認状態を解除します。
        /// </summary>
        public static void ClearSleepQuestion()
        {
            IsSleepQuestionActive =
                false;

            pendingRemove =
                false;

            pendingLocationName =
                null;

            pendingTile =
                Vector2.Zero;

            pendingQualifiedItemId =
                null;
        }

        //----------------------------------------
        // OnDayStarted
        //----------------------------------------

        /// <summary>
        /// 翌朝になった時、
        /// Remove指定されたBigCraftableを削除します。
        /// </summary>
        public static void OnDayStarted(
            object? sender,
            DayStartedEventArgs e)
        {
            //----------------------------------------
            // 削除対象なし
            //----------------------------------------

            if (string.IsNullOrWhiteSpace(
                removeLocationName))
            {
                return;
            }

            //----------------------------------------
            // Location取得
            //----------------------------------------

            GameLocation? location =
                Game1.getLocationFromName(
                    removeLocationName);

            if (location == null)
            {
                ClearRemoveTarget();
                return;
            }

            //----------------------------------------
            // BigCraftable取得
            //----------------------------------------

            if (!location.objects.TryGetValue(
                removeTile,
                out StardewValley.Object? obj)
                || obj == null
                || !obj.bigCraftable.Value)
            {
                ClearRemoveTarget();
                return;
            }

            //----------------------------------------
            // BigCraftable削除
            //----------------------------------------

            location.objects.Remove(
                removeTile);

            ClearRemoveTarget();
        }

        //----------------------------------------
        // Clear
        //----------------------------------------

        /// <summary>
        /// 睡眠関連の一時状態を
        /// すべて解除します。
        /// </summary>
        public static void Clear()
        {
            ClearSleepQuestion();

            ClearRemoveTarget();
        }

        //----------------------------------------
        // ClearRemoveTarget
        //----------------------------------------

        /// <summary>
        /// 翌朝の削除対象を解除します。
        /// </summary>
        private static void ClearRemoveTarget()
        {
            removeLocationName =
                null;

            removeTile =
                Vector2.Zero;
        }
    }
}