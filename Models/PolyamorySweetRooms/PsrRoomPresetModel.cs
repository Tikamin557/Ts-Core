using Microsoft.Xna.Framework;

namespace Ts_Core.Models.PolyamorySweetRooms
{
    /// <summary>
    /// <c>TsCore/PsrRoomPresets</c> に登録される、
    /// Polyamory Sweet Rooms（PSR）用の配偶者部屋Preset定義です。
    /// </summary>
    /// <remarks>
    /// Content Patcher側のFarmhouse ModがこのモデルをData Assetへ登録し、
    /// T's Coreの設定画面はその定義を基にPSR Content Packの
    /// <c>content.json</c>を生成します。
    /// </remarks>
    public sealed class PsrRoomPresetModel
    {
        /// <summary>設定保存先となるPSR Content PackのSMAPI Mod IDです。</summary>
        public string PsrContentPackId { get; set; } = "";

        /// <summary>
        /// 通常の恋愛・ルームメイト判定だけでは検出できないNPCを追加します。
        /// KeyはNPC内部名、ValueはそのNPCを提供するModの情報です。
        /// </summary>
        public Dictionary<string, PsrRoomAdditionalCandidateModel> AdditionalCandidates { get; set; } = new();

        /// <summary>設定画面に表示する配偶者部屋スロットです。リスト順が表示・保存順になります。</summary>
        public List<PsrRoomPresetSlotModel> Slots { get; set; } = new();
    }

    /// <summary>
    /// 自動判定できない追加候補NPCの有効条件です。
    /// NPC本体がまだData/Charactersへ出現していない進行度でも、
    /// 提供Modが導入済みなら設定候補として扱えます。
    /// </summary>
    public sealed class PsrRoomAdditionalCandidateModel
    {
        /// <summary>この追加候補を有効にする提供ModのSMAPI Mod IDです。</summary>
        public string ModId { get; set; } = "";
    }

    /// <summary>Preset内の配偶者部屋1スロット分の定義です。</summary>
    /// <remarks>
    /// 未割り当て時は<see cref="Id"/>をPSRのnameとして保存するため、
    /// 同一Preset内のIdは重複しない値にしてください。
    /// </remarks>
    public sealed class PsrRoomPresetSlotModel
    {
        /// <summary>スロットの内部IDです。未割り当て時のPSR nameにも使用します。</summary>
        public string Id { get; set; } = "";

        /// <summary>設定画面に表示する部屋名です。空の場合はIdを表示します。</summary>
        public string DisplayName { get; set; } = "";

        /// <summary>DisplayName上へマウスを置いた時に表示する任意の説明文です。</summary>
        public string HoverText { get; set; } = "";

        /// <summary>PSRのstartPosへ出力する部屋の開始座標です。</summary>
        public Point StartPosition { get; set; }

        /// <summary>PSRのspousePosOffsetへ出力する配偶者位置オフセットです。</summary>
        public Point SpousePositionOffset { get; set; } = new(3, 3);

        /// <summary>PSRのshellTypeへ出力する部屋シェルの種類です。</summary>
        public string ShellType { get; set; } = "";
    }
}
