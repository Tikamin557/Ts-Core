using StardewValley;
using StardewValley.GameData.Characters;
using StardewValley.GameData.Objects;

namespace Ts_Core.Services.Relationship
{
    /// <summary>
    /// 現在ロードされているゲームデータから、恋愛・結婚・ルームメイト候補NPCを判定します。
    /// PSR設定画面とCanBeRomanced Tokenで共通利用する候補判定の基礎サービスです。
    /// </summary>
    internal static class RomanceCandidateService
    {
        //----------------------------------------
        // 候補判定
        //----------------------------------------

        private const string RoommateProposalTagPrefix = "propose_roommate_";

        /// <summary>現在のData/CharactersにNPC内部名が存在するか確認します。</summary>
        internal static bool Exists(string npcName)
        {
            if (string.IsNullOrWhiteSpace(npcName))
                return false;

            Dictionary<string, CharacterData> data =
                Game1.content.Load<Dictionary<string, CharacterData>>("Data/Characters");

            return data.ContainsKey(npcName.Trim());
        }

        /// <summary>現在のData/CharactersでCanBeRomancedがtrueか確認します。</summary>
        internal static bool CanBeRomanced(string npcName)
        {
            if (string.IsNullOrWhiteSpace(npcName))
                return false;

            Dictionary<string, CharacterData> data =
                Game1.content.Load<Dictionary<string, CharacterData>>("Data/Characters");

            return data.TryGetValue(npcName.Trim(), out CharacterData? character)
                && character.CanBeRomanced;
        }

        /// <summary>
        /// CanBeRomanced NPC、ルームメイト候補、明示的な追加候補を統合して返します。
        /// 戻り値は表示名順ですが、値自体はNPC内部名です。
        /// </summary>
        internal static IReadOnlyList<string> GetCandidates(IEnumerable<string>? additionalCandidates = null)
        {
            Dictionary<string, CharacterData> characters =
                Game1.content.Load<Dictionary<string, CharacterData>>("Data/Characters");

            HashSet<string> result = new(StringComparer.OrdinalIgnoreCase);

            foreach ((string name, CharacterData character) in characters)
            {
                if (!string.IsNullOrWhiteSpace(name) && character.CanBeRomanced)
                    result.Add(name);
            }

            AddRoommateCandidates(result, characters);

            if (additionalCandidates != null)
            {
                foreach (string name in additionalCandidates)
                {
                    if (!string.IsNullOrWhiteSpace(name))
                        result.Add(name.Trim());
                }
            }

            return result
                .OrderBy(GetDisplayName, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
        }

        /// <summary>
        /// Data/Objectsのpropose_roommate_* Context Tagからルームメイト候補を検出します。
        /// KrobusのようにCanBeRomancedだけでは拾えない候補を補完します。
        /// </summary>
        private static void AddRoommateCandidates(
            HashSet<string> result,
            IReadOnlyDictionary<string, CharacterData> characters)
        {
            Dictionary<string, ObjectData> objects =
                Game1.content.Load<Dictionary<string, ObjectData>>("Data/Objects");

            HashSet<string> roommateKeys = new(StringComparer.OrdinalIgnoreCase);

            foreach (ObjectData data in objects.Values)
            {
                if (data.ContextTags == null)
                    continue;

                foreach (string tag in data.ContextTags)
                {
                    if (tag != null && tag.StartsWith(RoommateProposalTagPrefix, StringComparison.OrdinalIgnoreCase))
                    {
                        string key = tag[RoommateProposalTagPrefix.Length..].Trim();
                        if (key.Length > 0)
                            roommateKeys.Add(key);
                    }
                }
            }

            foreach (string name in characters.Keys)
            {
                if (roommateKeys.Contains(GetSafeNpcName(name)))
                    result.Add(name);
            }
        }

        /// <summary>roommate Context Tagとの比較用にNPC名を安全名へ変換します。</summary>
        private static string GetSafeNpcName(string npcName)
        {
            return npcName.Trim().ToLowerInvariant().Replace(' ', '_');
        }

        //----------------------------------------
        // 表示名
        //----------------------------------------

        /// <summary>NPC内部名から現在のローカライズ済み表示名を取得します。</summary>
        internal static string GetDisplayName(string npcName)
        {
            NPC? npc = Game1.getCharacterFromName(npcName, mustBeVillager: false);
            return npc?.displayName ?? npcName;
        }
    }
}
