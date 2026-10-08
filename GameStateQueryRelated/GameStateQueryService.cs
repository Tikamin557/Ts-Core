using StardewValley;
using StardewValley.Delegates;
using StardewValley.Locations;

namespace Ts_Core.Services.GameStateQueryRelated
{
    /// <summary>
    /// T's Core独自のGame State Queryを
    /// 登録・処理するサービスです。
    /// </summary>
    public static class GameStateQueryService
    {
        //----------------------------------------
        // Query Keys
        //----------------------------------------

        private const string LocationCategoryQuery =
            "TsCore_LOCATION_CATEGORY";

        //----------------------------------------
        // Register
        //----------------------------------------

        /// <summary>
        /// T's Core独自のGame State Queryを
        /// 登録します。
        /// </summary>
        public static void Register()
        {
            GameStateQuery.Register(
                LocationCategoryQuery,
                LocationCategory);
        }

        //----------------------------------------
        // LOCATION_CATEGORY
        //----------------------------------------

        /// <summary>
        /// 指定されたLocationが
        /// 指定カテゴリーに該当するか確認します。
        /// </summary>
        private static bool LocationCategory(
            string[] query,
            GameStateQueryContext context)
        {
            //----------------------------------------
            // Location取得
            //----------------------------------------

            GameLocation location =
                context.Location;

            if (!GameStateQuery.Helpers.TryGetLocationArg(
                query,
                1,
                ref location,
                out _))
            {
                return false;
            }

            //----------------------------------------
            // Category取得
            //----------------------------------------

            if (query.Length < 3)
            {
                return false;
            }

            //----------------------------------------
            // Category判定
            //----------------------------------------

            for (int i = 2; i < query.Length; i++)
            {
                if (IsLocationCategory(
                    location,
                    query[i]))
                {
                    return true;
                }
            }

            return false;
        }

        //----------------------------------------
        // Category判定
        //----------------------------------------

        /// <summary>
        /// Locationが指定カテゴリーに
        /// 該当するか確認します。
        /// </summary>
        private static bool IsLocationCategory(
            GameLocation location,
            string category)
        {
            return category.ToLowerInvariant() switch
            {
                "dungeon" =>
                    location is MineShaft
                    || location is VolcanoDungeon,

                _ => false
            };
        }
    }
}