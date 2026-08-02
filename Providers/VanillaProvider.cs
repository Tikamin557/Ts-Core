using StardewValley;

namespace Ts_Core.Providers
{
    /// <summary>
    /// バニラ環境のパートナー情報を取得します。
    /// </summary>
    public class VanillaProvider : IPartnerProvider
    {
        //----------------------------------------
        // プロパティ
        //----------------------------------------

        /// <summary>
        /// Providerの説明を取得します。
        /// </summary>
        public string Description => "MarriageMod: Vanilla";

        //----------------------------------------
        // パートナー一覧取得
        //----------------------------------------

        /// <summary>
        /// バニラ環境のパートナー一覧を取得します。
        /// </summary>
        public IEnumerable<string> GetPartners(Farmer farmer)
        {
            var result = new List<string>();

            if (farmer == null)
                return result;

            //----------------------------------------
            // 配偶者
            //----------------------------------------

            if (!string.IsNullOrEmpty(farmer.spouse))
                result.Add(farmer.spouse);

            //----------------------------------------
            // ルームメイト候補
            //----------------------------------------

            foreach (var entry in farmer.friendshipData.Pairs)
            {
                string npcName = entry.Key;
                Friendship friendship = entry.Value;

                //----------------------------------------
                // 無効なNPC名
                //----------------------------------------

                if (string.IsNullOrEmpty(npcName))
                    continue;

                //----------------------------------------
                // 配偶者は除外
                //----------------------------------------

                if (npcName == farmer.spouse)
                    continue;

                //----------------------------------------
                // 最大友好度NPC（ルームメイト候補として扱う）
                //----------------------------------------

                if (friendship.Points >= 2500)
                {
                    // 特別扱い候補（ルームメイト相当）
                    result.Add(npcName);
                }
            }

            return result;
        }
    }
}