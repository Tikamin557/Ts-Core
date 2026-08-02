using StardewValley;
using Ts_Core.Providers;
using Ts_Core.Readers;

namespace Ts_Core.Services.Relationship
{
    /// <summary>
    /// 配偶者・ルームメイト情報を取得し、表示順を管理するサービスです。
    /// </summary>
    public class PartnerService
    {
        //----------------------------------------
        // Provider
        //----------------------------------------

        private readonly IPartnerProvider provider;

        //----------------------------------------
        // コンストラクタ
        //----------------------------------------

        public PartnerService(IPartnerProvider provider)
        {
            this.provider = provider;
        }

        //----------------------------------------
        // Provider名取得
        //----------------------------------------

        public string GetProviderName()
        {
            return provider.GetType().Name;
        }

        //----------------------------------------
        // パートナー一覧取得
        //----------------------------------------

        /// <summary>
        /// パートナー一覧を名前順で取得します。
        /// </summary>
        public IEnumerable<string>? GetPartners()
        {
            if (Game1.player == null)
                return null;

            return provider.GetPartners(Game1.player)
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Distinct()
                .OrderBy(p => p)
                .ToList();
        }

        //----------------------------------------
        // 部屋順パートナー一覧取得
        //----------------------------------------

        /// <summary>
        /// 部屋の並び順に合わせたパートナー一覧を取得します。
        /// RoomOrderが存在しない場合は取得順を返します。
        /// </summary>
        public IEnumerable<string>? GetRoomOrderedPartners()
        {
            if (Game1.player == null)
                return null;

            var partners = provider.GetPartners(Game1.player)
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Distinct()
                .ToList();

            var roomOrder = RoomOrderReader.Load();

            if (roomOrder == null)
                return partners;

            var ordered = new List<string>();

            foreach (var name in roomOrder)
            {
                if (partners.Remove(name))
                    ordered.Add(name);
            }

            ordered.AddRange(partners);

            return ordered;
        }
    }
}