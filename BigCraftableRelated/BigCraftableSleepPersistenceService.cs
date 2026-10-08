using StardewModdingAPI;
using StardewValley;
using Ts_Core.Models.BigCraftableRelated;

namespace Ts_Core.Services.BigCraftableRelated
{
    /// <summary>
    /// TsCore_Sleepで使用した
    /// 継続寝床の保存と読込を管理するサービスです。
    /// </summary>
    public static class BigCraftableSleepPersistenceService
    {
        //----------------------------------------
        // Constants
        //----------------------------------------

        /// <summary>
        /// Save Dataのキーです。
        /// </summary>
        private const string SaveDataKey =
            "BigCraftableSleepData";

        //----------------------------------------
        // State
        //----------------------------------------

        /// <summary>
        /// SMAPI Helperです。
        /// </summary>
        private static IModHelper? helper;

        /// <summary>
        /// プレイヤーごとの
        /// 継続寝床情報です。
        /// </summary>
        private static Dictionary<long, BigCraftableSleepData>
            sleepDataByPlayer = new();

        //----------------------------------------
        // Initialize
        //----------------------------------------

        /// <summary>
        /// サービスを初期化します。
        /// </summary>
        public static void Initialize(
            IModHelper modHelper)
        {
            helper =
                modHelper;
        }

        //----------------------------------------
        // Load
        //----------------------------------------

        /// <summary>
        /// Save Dataから
        /// 継続寝床情報を読み込みます。
        /// </summary>
        public static void Load()
        {
            if (helper == null)
            {
                return;
            }

            sleepDataByPlayer =
                helper.Data.ReadSaveData<
                    Dictionary<long, BigCraftableSleepData>>(
                    SaveDataKey)
                ?? new Dictionary<long, BigCraftableSleepData>();
        }

        //----------------------------------------
        // Save
        //----------------------------------------

        /// <summary>
        /// 継続寝床情報を
        /// Save Dataへ保存します。
        /// </summary>
        public static void Save()
        {
            if (helper == null)
            {
                return;
            }

            helper.Data.WriteSaveData(
                SaveDataKey,
                sleepDataByPlayer);
        }

        //----------------------------------------
        // Set
        //----------------------------------------

        /// <summary>
        /// プレイヤーの継続寝床を
        /// 設定します。
        /// </summary>
        public static void Set(
            Farmer player,
            BigCraftableSleepData data)
        {
            sleepDataByPlayer[
                player.UniqueMultiplayerID] =
                data;
        }

        //----------------------------------------
        // TryGet
        //----------------------------------------

        /// <summary>
        /// プレイヤーの継続寝床を取得します。
        /// </summary>
        public static bool TryGet(
            Farmer player,
            out BigCraftableSleepData? data)
        {
            if (sleepDataByPlayer.TryGetValue(
                player.UniqueMultiplayerID,
                out BigCraftableSleepData? found))
            {
                data =
                    found;

                return true;
            }

            data =
                null;

            return false;
        }

        //----------------------------------------
        // TryGetValid
        //----------------------------------------

        /// <summary>
        /// プレイヤーの継続寝床を取得し、
        /// 対象BigCraftableが現在も存在する場合のみ
        /// trueを返します。
        /// </summary>
        public static bool TryGetValid(
            Farmer player,
            out BigCraftableSleepData? data)
        {
            //----------------------------------------
            // 保存データ取得
            //----------------------------------------

            if (!TryGet(
                player,
                out data)
                || data == null)
            {
                return false;
            }

            //----------------------------------------
            // Location確認
            //----------------------------------------

            if (string.IsNullOrWhiteSpace(
                data.LocationName))
            {
                return false;
            }

            GameLocation? location =
                Game1.getLocationFromName(
                    data.LocationName);

            if (location == null)
            {
                return false;
            }

            //----------------------------------------
            // AnchorのBigCraftable確認
            //----------------------------------------

            if (!location.objects.TryGetValue(
                data.Anchor,
                out StardewValley.Object? obj)
                || obj == null
                || !obj.bigCraftable.Value)
            {
                return false;
            }

            //----------------------------------------
            // Qualified Item ID確認
            //----------------------------------------

            if (!string.Equals(
                obj.QualifiedItemId,
                data.QualifiedItemId,
                StringComparison.Ordinal))
            {
                return false;
            }

            return true;
        }

        //----------------------------------------
        // Remove
        //----------------------------------------

        /// <summary>
        /// プレイヤーの継続寝床を
        /// 削除します。
        /// </summary>
        public static void Remove(
            Farmer player)
        {
            sleepDataByPlayer.Remove(
                player.UniqueMultiplayerID);
        }

        //----------------------------------------
        // Clear
        //----------------------------------------

        /// <summary>
        /// 読み込まれている
        /// 継続寝床情報を解除します。
        /// </summary>
        public static void Clear()
        {
            sleepDataByPlayer.Clear();
        }
    }
}