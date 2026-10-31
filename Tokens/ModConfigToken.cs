using StardewModdingAPI;
using System.Text.Json;

namespace Ts_Core.Tokens
{
    /// <summary>
    /// 他のCP/SMAPI Modのconfig.jsonに保存された設定をCPトークンとして公開します。
    /// CP Modについては、未保存の設定に限りConfigSchemaのDefaultも参照します。
    /// </summary>
    internal sealed class ModConfigToken
    {
        private readonly IModHelper helper;
        private readonly Dictionary<string, string?> modDirectories = new(StringComparer.OrdinalIgnoreCase);

        public ModConfigToken(IModHelper helper)
        {
            this.helper = helper;
        }

        //----------------------------------------
        // Token情報と更新
        //----------------------------------------

        public bool AllowsInput() => true;

        public bool CanHaveMultipleValues(string? input = null) => false;

        // GMCMなどが設定を保存した後も、CPのコンテキスト更新で再評価します。
        public bool UpdateContext() => true;

        public bool IsReady() => true;

        //----------------------------------------
        // 設定値取得
        //----------------------------------------

        /// <summary>
        /// 「ModUniqueID/Setting.Path」を読み取ります。
        /// 不正な指定、未導入Mod、未定義の値は空の結果を返します。
        /// </summary>
        public IEnumerable<string> GetValues(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                yield break;

            int separator = input.IndexOf('/');
            if (separator <= 0 || separator >= input.Length - 1)
                yield break;

            string modId = input[..separator].Trim();
            string settingPath = input[(separator + 1)..].Trim();
            if (modId.Length == 0 || settingPath.Length == 0)
                yield break;

            string? value = ReadValue(modId, settingPath);
            if (value != null)
                yield return value;
        }

        private string? ReadValue(string modId, string settingPath)
        {
            var mod = helper.ModRegistry.GetAll()
                .FirstOrDefault(entry => string.Equals(entry.Manifest.UniqueID, modId, StringComparison.OrdinalIgnoreCase));
            if (mod == null)
                return null;

            // 入力はファイルパスには使用せず、ModRegistryが返すディレクトリだけを参照します。
            string? directory = FindModDirectory(modId);
            if (directory == null)
                return null;

            string configPath = Path.Combine(directory, "config.json");
            string? value = ReadJsonValue(configPath, settingPath);
            if (value != null)
                return value;

            if (!mod.IsContentPack)
                return null;

            // CPのConfigSchemaでは、config.jsonにない設定はDefaultが有効です。
            string contentPath = Path.Combine(directory, "content.json");
            try
            {
                if (!File.Exists(contentPath))
                    return null;

                using var document = JsonDocument.Parse(File.ReadAllText(contentPath),
                    new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true });
                if (!TryGetProperty(document.RootElement, "ConfigSchema", out var schema)
                    || !TryGetProperty(schema, settingPath, out var setting)
                    || !TryGetProperty(setting, "Default", out var defaultValue))
                    return null;

                return ConvertValue(defaultValue);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
            {
                return null;
            }
        }

        /// <summary>
        /// 読み込まれたModのManifestを照合し、対応するフォルダを探します。
        /// Modのインストール先はSMAPI ModでもCP Modでも任意の階層になり得ます。
        /// </summary>
        private string? FindModDirectory(string modId)
        {
            if (modDirectories.TryGetValue(modId, out string? cached))
                return cached;

            // フォルダ名「Mods」を前提とせず、T's Coreの配置先から親階層を探索します。
            // Stardropなどの--mods-path指定にも対応します。
            string? result = null;
            DirectoryInfo? directory = new DirectoryInfo(helper.DirectoryPath).Parent;
            for (int depth = 0; directory != null && depth < 5 && result == null; depth++, directory = directory.Parent)
            {
                // ドライブ全体やゲーム本体のフォルダまで再帰探索しないようにします。
                if (directory.Parent == null || File.Exists(Path.Combine(directory.FullName, "Stardew Valley.dll")))
                    break;

                try
                {
                    foreach (string manifestPath in Directory.EnumerateFiles(directory.FullName, "manifest.json", SearchOption.AllDirectories))
                    {
                        try
                        {
                            using var document = JsonDocument.Parse(File.ReadAllText(manifestPath),
                                new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true });
                            if (TryGetProperty(document.RootElement, "UniqueID", out var id)
                                && string.Equals(id.GetString(), modId, StringComparison.OrdinalIgnoreCase))
                            {
                                result = Path.GetDirectoryName(manifestPath);
                                break;
                            }
                        }
                        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or InvalidOperationException)
                        {
                            // 読めないManifestは無視します。
                        }
                    }
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                    // この階層を探索できない場合は、上位階層を確認します。
                }
            }

            modDirectories[modId] = result;
            return result;
        }

        private static string? ReadJsonValue(string path, string settingPath)
        {
            try
            {
                if (!File.Exists(path))
                    return null;

                using var document = JsonDocument.Parse(File.ReadAllText(path),
                    new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true });
                JsonElement current = document.RootElement;
                foreach (string part in settingPath.Split('.'))
                {
                    if (part.Length == 0 || !TryGetProperty(current, part, out current))
                        return null;
                }
                return ConvertValue(current);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
            {
                return null;
            }
        }

        private static bool TryGetProperty(JsonElement element, string name, out JsonElement value)
        {
            value = default;
            if (element.ValueKind != JsonValueKind.Object)
                return false;
            foreach (var property in element.EnumerateObject())
            {
                if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    value = property.Value;
                    return true;
                }
            }
            return false;
        }

        private static string? ConvertValue(JsonElement value)
        {
            return value.ValueKind switch
            {
                JsonValueKind.String => value.GetString(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                JsonValueKind.Number => value.GetRawText(),
                _ => null
            };
        }
    }
}
