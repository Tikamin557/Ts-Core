using StardewModdingAPI;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Ts_Core.Services.ContentPatcherRelated
{
    /// <summary>
    /// Content Patcher Content PackのConfigSchemaにある
    /// T's Core独自のGMCM表示条件を処理します。
    /// </summary>
    internal static class ContentPatcherConfigMenuFilterService
    {
        //----------------------------------------
        // 独自ConfigSchemaキー
        //----------------------------------------

        private const string ShowIfModKey =
            "TsCore.ShowIfMod";

        private const string ShowIfAllModsKey =
            "TsCore.ShowIfAllMods";

        //----------------------------------------
        // GMCM用Config生成試行
        //----------------------------------------

        /// <summary>
        /// T's Core独自のGMCM表示条件が存在する場合のみ、
        /// 表示する項目だけを含むConfigを生成します。
        /// </summary>
        internal static bool TryCreateFilteredConfig(
            object currentConfig,
            object rawContentPack,
            IModHelper helper,
            IMonitor monitor,
            [NotNullWhen(true)] out object? filteredConfig)
        {
            filteredConfig = null;

            //----------------------------------------
            // Content Pack取得
            //----------------------------------------

            if (rawContentPack is not IContentPack contentPack)
            {
                throw new InvalidOperationException(
                    "Could not access Content Patcher Content Pack.");
            }

            //----------------------------------------
            // content.json読み込み
            //----------------------------------------

            ContentFileModel? content =
                contentPack.ReadJsonFile<ContentFileModel>(
                    "content.json");

            //----------------------------------------
            // T's Core独自条件確認
            //----------------------------------------

            if (!HasCustomDisplayConditions(
                    content))
            {
                return false;
            }

            //----------------------------------------
            // GMCM用Config生成
            //----------------------------------------

            filteredConfig =
                CreateFilteredConfigCore(
                    currentConfig,
                    content,
                    helper,
                    monitor);

            return true;
        }

        //----------------------------------------
        // GMCM用Config生成
        //----------------------------------------

        /// <summary>
        /// T's Core独自のGMCM表示条件を評価し、
        /// 表示する項目だけを含むConfigを生成します。
        /// </summary>
        internal static object CreateFilteredConfig(
            object currentConfig,
            object rawContentPack,
            IModHelper helper,
            IMonitor monitor)
        {
            //----------------------------------------
            // Content Pack取得
            //----------------------------------------

            if (rawContentPack is not IContentPack contentPack)
            {
                throw new InvalidOperationException(
                    "Could not access Content Patcher Content Pack.");
            }

            //----------------------------------------
            // content.json読み込み
            //----------------------------------------

            ContentFileModel? content =
                contentPack.ReadJsonFile<ContentFileModel>(
                    "content.json");

            //----------------------------------------
            // GMCM用Config生成
            //----------------------------------------

            return CreateFilteredConfigCore(
                currentConfig,
                content,
                helper,
                monitor);
        }

        //----------------------------------------
        // GMCM用Config生成本体
        //----------------------------------------

        /// <summary>
        /// 読み込み済みのConfigSchemaを使用して、
        /// GMCM表示用Configを生成します。
        /// </summary>
        private static object CreateFilteredConfigCore(
            object currentConfig,
            ContentFileModel? content,
            IModHelper helper,
            IMonitor monitor)
        {
            //----------------------------------------
            // currentConfigと同じ型のDictionaryを生成
            //----------------------------------------

            Type configType =
                currentConfig.GetType();

            object? filteredConfig =
                Activator.CreateInstance(
                    configType);

            if (filteredConfig == null)
            {
                throw new InvalidOperationException(
                    "Could not create filtered Content Patcher Config.");
            }

            //----------------------------------------
            // Indexer取得
            //----------------------------------------

            PropertyInfo? indexer =
                configType.GetProperty(
                    "Item",
                    BindingFlags.Instance
                    | BindingFlags.Public
                    | BindingFlags.NonPublic);

            if (indexer == null)
            {
                throw new InvalidOperationException(
                    "Could not access Content Patcher Config indexer.");
            }

            //----------------------------------------
            // Config列挙
            //----------------------------------------

            if (currentConfig is not IEnumerable enumerable)
            {
                throw new InvalidOperationException(
                    "Could not enumerate Content Patcher Config.");
            }

            foreach (object? item in enumerable)
            {
                if (item == null)
                    continue;

                object? key =
                    GetPropertyValue(
                        item,
                        "Key");

                object? value =
                    GetPropertyValue(
                        item,
                        "Value");

                if (key == null
                    || value == null)
                {
                    continue;
                }

                string fieldName =
                    key.ToString()
                    ?? "";

                //----------------------------------------
                // TsCore.ShowIfMod
                //----------------------------------------

                string? showIfMod =
                    GetConfigSchemaValue(
                        content,
                        fieldName,
                        ShowIfModKey);

                if (!string.IsNullOrWhiteSpace(
                        showIfMod))
                {
                    string[] modIds =
                        ParseModIds(
                            showIfMod);

                    bool anyModLoaded =
                        modIds.Any(
                            modId =>
                                helper.ModRegistry.IsLoaded(
                                    modId));

                    if (!anyModLoaded)
                    {
                        monitor.Log(
                            $"Hiding GMCM config field '{fieldName}' because none of the required mods are loaded: " +
                            $"{string.Join(", ", modIds)}.",
                            LogLevel.Trace);

                        continue;
                    }
                }

                //----------------------------------------
                // TsCore.ShowIfAllMods
                //----------------------------------------

                string? showIfAllMods =
                    GetConfigSchemaValue(
                        content,
                        fieldName,
                        ShowIfAllModsKey);

                if (!string.IsNullOrWhiteSpace(
                        showIfAllMods))
                {
                    string[] modIds =
                        ParseModIds(
                            showIfAllMods);

                    bool allModsLoaded =
                        modIds.All(
                            modId =>
                                helper.ModRegistry.IsLoaded(
                                    modId));

                    if (!allModsLoaded)
                    {
                        monitor.Log(
                            $"Hiding GMCM config field '{fieldName}' because not all required mods are loaded: " +
                            $"{string.Join(", ", modIds)}.",
                            LogLevel.Trace);

                        continue;
                    }
                }

                //----------------------------------------
                // GMCM用Configへ追加
                //----------------------------------------

                indexer.SetValue(
                    filteredConfig,
                    value,
                    new[]
                    {
                        key
                    });
            }

            return filteredConfig;
        }

        //----------------------------------------
        // 独自表示条件確認
        //----------------------------------------

        /// <summary>
        /// ConfigSchema内にT's Core独自の
        /// GMCM表示条件が1件でも存在するか確認します。
        /// </summary>
        private static bool HasCustomDisplayConditions(
            ContentFileModel? content)
        {
            if (content?.ConfigSchema == null)
                return false;

            foreach (
                Dictionary<string, object> field
                in content.ConfigSchema.Values)
            {
                if (field.ContainsKey(
                        ShowIfModKey)
                    || field.ContainsKey(
                        ShowIfAllModsKey))
                {
                    return true;
                }
            }

            return false;
        }

        //----------------------------------------
        // ConfigSchema独自値取得
        //----------------------------------------

        /// <summary>
        /// 指定したConfigSchema項目から
        /// T's Core独自キーの値を取得します。
        /// </summary>
        private static string? GetConfigSchemaValue(
            ContentFileModel? content,
            string fieldName,
            string propertyName)
        {
            //----------------------------------------
            // ConfigSchema確認
            //----------------------------------------

            if (content?.ConfigSchema == null)
                return null;

            //----------------------------------------
            // Config項目取得
            //----------------------------------------

            if (!content.ConfigSchema.TryGetValue(
                    fieldName,
                    out Dictionary<string, object>? field))
            {
                return null;
            }

            //----------------------------------------
            // 独自キー取得
            //----------------------------------------

            if (!field.TryGetValue(
                    propertyName,
                    out object? value))
            {
                return null;
            }

            return value?.ToString();
        }

        //----------------------------------------
        // Mod ID解析
        //----------------------------------------

        /// <summary>
        /// カンマ区切りのMod ID一覧を解析します。
        /// 改行や前後の空白は除去されます。
        /// </summary>
        private static string[] ParseModIds(
            string value)
        {
            return value.Split(
                ',',
                StringSplitOptions.RemoveEmptyEntries
                | StringSplitOptions.TrimEntries);
        }

        //----------------------------------------
        // Property取得
        //----------------------------------------

        private static object? GetPropertyValue(
            object? instance,
            string propertyName)
        {
            return ContentPatcherReloadService
                .GetPropertyValue(
                    instance,
                    propertyName);
        }

        //----------------------------------------
        // content.json最小モデル
        //----------------------------------------

        /// <summary>
        /// T's Core独自のConfigSchema拡張を取得するための
        /// content.json最小モデルです。
        /// </summary>
        private sealed class ContentFileModel
        {
            public Dictionary<
                string,
                Dictionary<string, object>>?
                ConfigSchema
            { get; set; }
        }
    }
}