using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;

namespace Ts_Core.Services.WarpRelated
{
    /// <summary>
    /// プレイヤーが最後に離れた
    /// FarmHouse/Cabinを記録します。
    /// </summary>
    internal static class PreviousHomeService
    {
        //----------------------------------------
        // ModData Key
        //----------------------------------------

        private const string LocationKey =
            "Tikamin557.TsCore/PreviousHomeLocation";

        //----------------------------------------
        // 初期化
        //----------------------------------------

        internal static void Initialize(
            IModHelper helper)
        {
            helper.Events.Player.Warped +=
                OnWarped;
        }

        //----------------------------------------
        // Warped
        //----------------------------------------

        private static void OnWarped(
            object? sender,
            WarpedEventArgs e)
        {
            //----------------------------------------
            // ローカルプレイヤーのみ
            //----------------------------------------

            if (!e.Player.IsLocalPlayer)
                return;

            //----------------------------------------
            // 移動元がFarmHouse/Cabinか確認
            //----------------------------------------

            if (e.OldLocation
                is not FarmHouse home)
            {
                return;
            }

            //----------------------------------------
            // 同じHome内のWarpは無視
            //----------------------------------------

            if (ReferenceEquals(
                    e.OldLocation,
                    e.NewLocation))
            {
                return;
            }

            //----------------------------------------
            // Previous Home保存
            //----------------------------------------

            e.Player.modData[LocationKey] =
                home.NameOrUniqueName;
        }

        //----------------------------------------
        // Previous Home取得
        //----------------------------------------

        internal static bool TryGetPreviousHome(
            out FarmHouse? home)
        {
            home = null;

            if (!Context.IsWorldReady)
                return false;

            Farmer player =
                Game1.player;

            //----------------------------------------
            // Previous Home情報取得
            //----------------------------------------

            if (!player.modData.TryGetValue(
                    LocationKey,
                    out string? location)
                || string.IsNullOrWhiteSpace(
                    location))
            {
                return false;
            }

            //----------------------------------------
            // 各プレイヤーのHomeを検索
            //----------------------------------------

            foreach (Farmer farmer
                in Game1.getAllFarmers())
            {
                FarmHouse farmerHome =
                    Utility.getHomeOfFarmer(
                        farmer);

                if (!string.Equals(
                        farmerHome.NameOrUniqueName,
                        location,
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                home =
                    farmerHome;

                return true;
            }

            return false;
        }
    }
}