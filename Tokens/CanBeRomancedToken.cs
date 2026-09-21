using StardewValley;
using StardewValley.GameData.Characters;

namespace Ts_Core.Tokens
{
    /// <summary>
    /// 指定したNPCが恋愛・結婚可能かを返す
    /// Content Patcherトークンです。
    /// </summary>
    internal sealed class CanBeRomancedToken
    {
        //----------------------------------------
        // Token情報
        //----------------------------------------

        /// <summary>
        /// 入力値を受け取るかどうかを返します。
        /// </summary>
        public bool AllowsInput()
        {
            return true;
        }

        /// <summary>
        /// 複数の値を返す可能性があるかどうかを返します。
        /// </summary>
        public bool CanHaveMultipleValues(
            string? input = null)
        {
            return false;
        }

        //----------------------------------------
        // Token状態
        //----------------------------------------

        /// <summary>
        /// Tokenのコンテキストを更新します。
        /// </summary>
        public bool UpdateContext()
        {
            // Data/CharactersはContent Patcherによって
            // 変更される可能性があるため、
            // コンテキスト更新時に再評価させます。
            return true;
        }

        /// <summary>
        /// Tokenが使用可能かどうかを返します。
        /// </summary>
        public bool IsReady()
        {
            return true;
        }

        //----------------------------------------
        // Token値
        //----------------------------------------

        /// <summary>
        /// 指定したNPCが恋愛・結婚可能かどうかを返します。
        /// </summary>
        public IEnumerable<string> GetValues(
            string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                yield break;

            Dictionary<string, CharacterData> data =
                Game1.content.Load<
                    Dictionary<string, CharacterData>>(
                        "Data/Characters");

            if (!data.TryGetValue(
                    input.Trim(),
                    out CharacterData? character))
            {
                yield return "false";
                yield break;
            }

            yield return character.CanBeRomanced
                ? "true"
                : "false";
        }
    }
}