using StardewModdingAPI;
using StardewValley;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ts_Core.Models.PolyamorySweetRooms;

namespace Ts_Core.Services.PolyamorySweetRoomsRelated
{
    /// <summary>
    /// Presetに紐付いたPolyamory Sweet Rooms Content Packの
    /// <c>content.json</c>を読み書きするサービスです。
    /// </summary>
    /// <remarks>
    /// PSR内部のroom dataを直接変更せず、PSRが正式に読むContent Packファイルだけを
    /// 更新することで、PSR本来の読み込み処理へ設定を引き渡します。
    /// </remarks>
    internal static class PsrRoomContentService
    {
        //----------------------------------------
        // 依存サービス / JSON設定
        //----------------------------------------

        private static IModHelper helper = null!;
        private static IMonitor monitor = null!;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        //----------------------------------------
        // 初期化 / Preset取得
        //----------------------------------------

        /// <summary>SMAPI HelperとMonitorを保持します。</summary>
        internal static void Initialize(IModHelper modHelper, IMonitor modMonitor)
        {
            helper = modHelper;
            monitor = modMonitor;
        }

        /// <summary>現在の<see cref="PsrRoomPresetDataService.AssetName"/>を読み込みます。</summary>
        internal static IReadOnlyDictionary<string, PsrRoomPresetModel> GetPresets()
        {
            try
            {
                return Game1.content.Load<Dictionary<string, PsrRoomPresetModel>>(
                    PsrRoomPresetDataService.AssetName);
            }
            catch (Exception ex)
            {
                monitor.Log($"Failed loading {PsrRoomPresetDataService.AssetName}: {ex}", LogLevel.Error);
                return new Dictionary<string, PsrRoomPresetModel>();
            }
        }

        //----------------------------------------
        // PSR Content Pack確認 / 読み込み
        //----------------------------------------

        /// <summary>Presetが指定するPSR Content PackをSMAPIから取得できるか確認します。</summary>
        internal static bool IsContentPackAvailable(PsrRoomPresetModel preset)
        {
            if (string.IsNullOrWhiteSpace(preset.PsrContentPackId))
                return false;

            return TryGetContentPack(preset.PsrContentPackId) != null;
        }

        /// <summary>
        /// 既存のcontent.jsonから各Slotの割り当てNPCを読み込みます。
        /// Slot IDおよび旧TsCore_Unassigned形式は「未割り当て」として扱います。
        /// </summary>
        internal static List<string?> LoadAssignments(PsrRoomPresetModel preset)
        {
            List<string?> result = preset.Slots.Select(_ => (string?)null).ToList();
            string? path = GetContentPath(preset);
            if (path == null || !File.Exists(path))
                return result;

            try
            {
                string json = File.ReadAllText(path);
                PsrContentFile? content = JsonSerializer.Deserialize<PsrContentFile>(json, JsonOptions);
                if (content?.Data == null)
                    return result;

                int count = Math.Min(result.Count, content.Data.Count);
                for (int i = 0; i < count; i++)
                {
                    string? name = content.Data[i].Name;
                    string slotId = preset.Slots[i].Id;
                    if (!string.IsNullOrWhiteSpace(name)
                        && !string.Equals(name, slotId, StringComparison.OrdinalIgnoreCase)
                        && !string.Equals(name, "TsCore_Unassigned", StringComparison.OrdinalIgnoreCase)
                        && !name.StartsWith("TsCore_Unassigned_", StringComparison.OrdinalIgnoreCase))
                    {
                        result[i] = name;
                    }
                }
            }
            catch (Exception ex)
            {
                monitor.Log($"Failed reading PSR content file '{path}': {ex}", LogLevel.Warn);
            }

            return result;
        }

        //----------------------------------------
        // PSR Content Pack保存
        //----------------------------------------

        /// <summary>
        /// 現在の割り当てをPSR形式のcontent.jsonとして上書き保存します。
        /// 未割り当てSlotは重複nameを避けるため、そのSlot自身のIdをnameへ出力します。
        /// </summary>
        internal static bool Save(PsrRoomPresetModel preset, IReadOnlyList<string?> assignments, out string? error)
        {
            error = null;
            string? path = GetContentPath(preset);
            if (path == null)
            {
                error = "content-pack-not-found";
                return false;
            }

            try
            {
                PsrContentFile content = new();
                for (int i = 0; i < preset.Slots.Count; i++)
                {
                    PsrRoomPresetSlotModel slot = preset.Slots[i];
                    string? assigned = i < assignments.Count ? assignments[i] : null;
                    string name = string.IsNullOrWhiteSpace(assigned)
                        ? slot.Id
                        : assigned;

                    content.Data.Add(new PsrRoomData
                    {
                        Name = name,
                        StartPos = new PsrPoint(slot.StartPosition.X, slot.StartPosition.Y),
                        SpousePosOffset = new PsrPoint(slot.SpousePositionOffset.X, slot.SpousePositionOffset.Y),
                        ShellType = slot.ShellType
                    });
                }

                string json = JsonSerializer.Serialize(content, JsonOptions);
                File.WriteAllText(path, json + Environment.NewLine);
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                monitor.Log($"Failed writing PSR content file '{path}': {ex}", LogLevel.Error);
                return false;
            }
        }

        //----------------------------------------
        // Content Packパス取得
        //----------------------------------------

        /// <summary>Presetが指定するPSR Content Packのcontent.jsonパスを返します。</summary>
        private static string? GetContentPath(PsrRoomPresetModel preset)
        {
            IContentPack? contentPack = TryGetContentPack(preset.PsrContentPackId);
            if (contentPack == null)
                return null;

            return Path.Combine(contentPack.DirectoryPath, "content.json");
        }

        /// <summary>
        /// SMAPIのIModInfoから内部ContentPackを防御的に取得します。
        /// IModInfoの公開APIにはIContentPackが公開されていないためReflectionを使用します。
        /// </summary>
        private static IContentPack? TryGetContentPack(string modId)
        {
            IModInfo? modInfo = helper.ModRegistry.Get(modId);
            if (modInfo == null)
                return null;

            // IModInfo doesn't expose the loaded IContentPack through the public API.
            // SMAPI's internal ModMetadata implementation does, so access that value
            // defensively and only use it when it is an IContentPack.
            try
            {
                return modInfo.GetType()
                    .GetProperty("ContentPack")?
                    .GetValue(modInfo) as IContentPack;
            }
            catch (Exception ex)
            {
                monitor.Log(
                    $"Failed accessing content pack '{modId}': {ex}",
                    LogLevel.Warn);
                return null;
            }
        }

        //----------------------------------------
        // PSR content.json シリアライズ用モデル
        //----------------------------------------

        /// <summary>PSR content.jsonのルート要素です。</summary>
        private sealed class PsrContentFile
        {
            [JsonPropertyName("data")]
            public List<PsrRoomData> Data { get; set; } = new();
        }

        /// <summary>PSR content.json内の配偶者部屋1件分です。</summary>
        private sealed class PsrRoomData
        {
            [JsonPropertyName("name")]
            public string Name { get; set; } = "";

            [JsonPropertyName("startPos")]
            public PsrPoint StartPos { get; set; } = new();

            [JsonPropertyName("spousePosOffset")]
            public PsrPoint SpousePosOffset { get; set; } = new();

            [JsonPropertyName("shellType")]
            public string ShellType { get; set; } = "";
        }

        /// <summary>PSR JSONのX/Y座標を出力するための単純な座標モデルです。</summary>
        private sealed class PsrPoint
        {
            public PsrPoint() { }
            public PsrPoint(int x, int y) { X = x; Y = y; }
            public int X { get; set; }
            public int Y { get; set; }
        }
    }
}
