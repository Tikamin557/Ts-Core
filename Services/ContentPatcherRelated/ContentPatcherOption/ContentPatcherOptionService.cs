using StardewModdingAPI;
using System.Collections;

namespace Ts_Core.Services.ContentPatcherRelated.ContentPatcherOption
{
    /// <summary>
    /// Content Patcher Content PackのConfigSchemaにある
    /// TsCore.Optionを管理します。
    /// </summary>
    internal static class ContentPatcherOptionService
    {
        //----------------------------------------
        // ConfigSchemaキー
        //----------------------------------------

        private const string OptionKey =
            "TsCore.Option";

        //----------------------------------------
        // Option状態
        //----------------------------------------

        /// <summary>
        /// Content PackごとのOption状態を保持します。
        /// </summary>
        private static readonly Dictionary<
            string,
            Dictionary<string, bool>>
            OptionsByContentPack =
                new(
                    StringComparer.OrdinalIgnoreCase);

        //----------------------------------------
        // Option状態クリア
        //----------------------------------------

        /// <summary>
        /// 現在保持しているすべての
        /// Content Patcher Option状態を削除します。
        /// </summary>
        internal static void Clear()
        {
            OptionsByContentPack.Clear();
        }

        //----------------------------------------
        // Content Pack単位更新
        //----------------------------------------

        /// <summary>
        /// 指定したContent PackのTsCore.Optionを
        /// 現在のConfig値から再構築します。
        /// </summary>
        internal static void RefreshContentPack(
            object currentConfig,
            object rawContentPack,
            string contentPackId,
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
            // このContent Packの旧状態削除
            //----------------------------------------

            OptionsByContentPack.Remove(
                contentPackId);

            //----------------------------------------
            // ConfigSchema確認
            //----------------------------------------

            if (content?.ConfigSchema == null)
                return;

            //----------------------------------------
            // Option状態
            //----------------------------------------

            Dictionary<string, bool> options =
                new(
                    StringComparer.OrdinalIgnoreCase);

            //----------------------------------------
            // ConfigSchema列挙
            //----------------------------------------

            foreach (
                KeyValuePair<
                    string,
                    Dictionary<string, object>>
                pair
                in content.ConfigSchema)
            {
                string fieldName =
                    pair.Key;

                Dictionary<string, object> fieldSchema =
                    pair.Value;

                //----------------------------------------
                // TsCore.Option取得
                //----------------------------------------

                if (!fieldSchema.TryGetValue(
                        OptionKey,
                        out object? optionValue))
                {
                    continue;
                }

                string? optionName =
                    optionValue?.ToString();

                if (string.IsNullOrWhiteSpace(
                        optionName))
                {
                    continue;
                }

                optionName =
                    optionName.Trim();

                //----------------------------------------
                // 対応Option確認
                //----------------------------------------

                if (!IsSupportedOption(
                        optionName))
                {
                    monitor.Log(
                        $"Unknown TsCore.Option '{optionName}' in '{contentPackId}' field '{fieldName}'.",
                        LogLevel.Warn);

                    continue;
                }

                //----------------------------------------
                // ConfigField取得
                //----------------------------------------

                object? configField =
                    GetDictionaryValue(
                        currentConfig,
                        fieldName);

                if (configField == null)
                    continue;

                //----------------------------------------
                // Boolean設定確認
                //----------------------------------------

                if (!IsBooleanConfigField(
                        configField))
                {
                    monitor.Log(
                        $"Ignoring TsCore.Option '{optionName}' in '{contentPackId}' field '{fieldName}' because the config field is not boolean.",
                        LogLevel.Warn);

                    continue;
                }

                //----------------------------------------
                // 現在値取得
                //----------------------------------------

                if (!TryGetBooleanValue(
                        configField,
                        out bool enabled))
                {
                    monitor.Log(
                        $"Could not read boolean value for TsCore.Option '{optionName}' in '{contentPackId}' field '{fieldName}'.",
                        LogLevel.Warn);

                    continue;
                }

                //----------------------------------------
                // 同一OptionはOR
                //----------------------------------------

                if (enabled)
                {
                    options[optionName] =
                        true;
                }
                else if (!options.ContainsKey(
                             optionName))
                {
                    options[optionName] =
                        false;
                }
            }

            //----------------------------------------
            // 状態保存
            //----------------------------------------

            if (options.Count > 0)
            {
                OptionsByContentPack[
                    contentPackId] =
                    options;
            }
        }

        //----------------------------------------
        // Option有効確認
        //----------------------------------------

        /// <summary>
        /// 指定したOptionがいずれかのContent Packで
        /// 有効になっているか確認します。
        /// </summary>
        internal static bool IsEnabled(
            string optionName)
        {
            foreach (
                Dictionary<string, bool> options
                in OptionsByContentPack.Values)
            {
                if (options.TryGetValue(
                        optionName,
                        out bool enabled)
                    && enabled)
                {
                    return true;
                }
            }

            return false;
        }

        //----------------------------------------
        // 対応Option確認
        //----------------------------------------

        private static bool IsSupportedOption(
            string optionName)
        {
            return string.Equals(
                       optionName,
                       ContentPatcherOptionIds.HideEmilyParrot,
                       StringComparison.OrdinalIgnoreCase)
                || string.Equals(
                       optionName,
                       ContentPatcherOptionIds.HideSebastianFrog,
                       StringComparison.OrdinalIgnoreCase);
        }

        //----------------------------------------
        // Boolean Config確認
        //----------------------------------------

        /// <summary>
        /// Content Patcher ConfigField.IsBoolean()を使用して
        /// Boolean設定か確認します。
        /// </summary>
        private static bool IsBooleanConfigField(
            object configField)
        {
            System.Reflection.MethodInfo? method =
                configField
                    .GetType()
                    .GetMethod(
                        "IsBoolean",
                        System.Reflection.BindingFlags.Instance
                        | System.Reflection.BindingFlags.Public
                        | System.Reflection.BindingFlags.NonPublic);

            if (method == null)
                return false;

            return method.Invoke(
                configField,
                null)
                is true;
        }

        //----------------------------------------
        // Boolean値取得
        //----------------------------------------

        /// <summary>
        /// ConfigField.Valueから現在のBoolean値を取得します。
        /// </summary>
        private static bool TryGetBooleanValue(
            object configField,
            out bool value)
        {
            value = false;

            //----------------------------------------
            // Value取得
            //----------------------------------------

            object? configValue =
                GetPropertyValue(
                    configField,
                    "Value");

            if (configValue is not IEnumerable enumerable)
                return false;

            //----------------------------------------
            // 値取得
            //----------------------------------------

            string? rawValue =
                null;

            int count =
                0;

            foreach (object? item in enumerable)
            {
                if (item == null)
                    continue;

                count++;

                if (count > 1)
                    return false;

                rawValue =
                    item.ToString();
            }

            //----------------------------------------
            // Boolean解析
            //----------------------------------------

            if (count != 1
                || string.IsNullOrWhiteSpace(
                    rawValue))
            {
                return false;
            }

            return bool.TryParse(
                rawValue,
                out value);
        }

        //----------------------------------------
        // Dictionary値取得
        //----------------------------------------

        private static object? GetDictionaryValue(
            object dictionary,
            string key)
        {
            if (dictionary is not IEnumerable enumerable)
                return null;

            foreach (object? item in enumerable)
            {
                if (item == null)
                    continue;

                string? itemKey =
                    GetPropertyValue(
                        item,
                        "Key")
                        ?.ToString();

                if (!string.Equals(
                        itemKey,
                        key,
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                return GetPropertyValue(
                    item,
                    "Value");
            }

            return null;
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
        /// TsCore.Optionを取得するための
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