using StardewModdingAPI;
using System.Collections;
using System.Reflection;

namespace Ts_Core.Services.ContentPatcherRelated
{
    /// <summary>
    /// Content PatcherのDynamicTokens再読み込み処理を管理します。
    /// </summary>
    internal static class ContentPatcherDynamicTokenReloadService
    {
        //----------------------------------------
        // DynamicTokens再読み込み
        //----------------------------------------

        /// <summary>
        /// 最新のcontent.jsonからDynamicTokensを再構築します。
        /// </summary>
        internal static void Refresh(
            object contentPatcherMod,
            object screenManagerContainer,
            object contentPack,
            object rawContentPack,
            object content,
            IMonitor monitor)
        {
            //----------------------------------------
            // 最新DynamicTokens取得
            //----------------------------------------

            object? dynamicTokens =
                GetPropertyValue(
                    content,
                    "DynamicTokens");

            if (dynamicTokens is not IEnumerable dynamicTokenEnumerable)
            {
                throw new InvalidOperationException(
                    "Could not access Content Patcher DynamicTokens.");
            }

            //----------------------------------------
            // Installed Mods取得
            //----------------------------------------

            MethodInfo? getInstalledModsMethod =
                contentPatcherMod
                    .GetType()
                    .GetMethod(
                        "GetInstalledMods",
                        BindingFlags.Instance
                        | BindingFlags.Public
                        | BindingFlags.NonPublic,
                        binder: null,
                        types: Type.EmptyTypes,
                        modifiers: null);

            if (getInstalledModsMethod == null)
            {
                throw new InvalidOperationException(
                    "Could not access Content Patcher GetInstalledMods().");
            }

            object? installedMods =
                getInstalledModsMethod.Invoke(
                    contentPatcherMod,
                    null);

            if (installedMods == null)
            {
                throw new InvalidOperationException(
                    "Could not access Content Patcher installed mod context.");
            }

            //----------------------------------------
            // Content Pack情報取得
            //----------------------------------------

            object? manifest =
                GetPropertyValue(
                    contentPack,
                    "Manifest");

            object? migrator =
                GetPropertyValue(
                    contentPack,
                    "Migrator");

            object? logPath =
                GetPropertyValue(
                    contentPack,
                    "LogPath");

            if (manifest == null
                || migrator == null
                || logPath == null)
            {
                throw new InvalidOperationException(
                    "Could not access Content Patcher content pack metadata.");
            }

            //----------------------------------------
            // Active Screen一覧取得
            //----------------------------------------

            MethodInfo? getActiveValuesMethod =
                screenManagerContainer
                    .GetType()
                    .GetMethod(
                        "GetActiveValues",
                        BindingFlags.Instance
                        | BindingFlags.Public
                        | BindingFlags.NonPublic);

            if (getActiveValuesMethod == null)
            {
                throw new InvalidOperationException(
                    "Could not access Content Patcher active ScreenManagers.");
            }

            object? activeValues =
                getActiveValuesMethod.Invoke(
                    screenManagerContainer,
                    null);

            if (activeValues is not IEnumerable screenEnumerable)
            {
                throw new InvalidOperationException(
                    "Could not enumerate Content Patcher active ScreenManagers.");
            }

            //----------------------------------------
            // 各Screen
            //----------------------------------------

            foreach (object? entry in screenEnumerable)
            {
                if (entry == null)
                    continue;

                object? screenManager =
                    GetPropertyValue(
                        entry,
                        "Value");

                if (screenManager == null)
                    continue;

                RefreshScreen(
                    screenManager,
                    contentPack,
                    rawContentPack,
                    dynamicTokenEnumerable,
                    manifest,
                    migrator,
                    installedMods,
                    logPath,
                    monitor);
            }

            monitor.Log(
                "Content Patcher dynamic tokens updated.",
                LogLevel.Trace);
        }

        //----------------------------------------
        // Screen単位の再構築
        //----------------------------------------

        /// <summary>
        /// 一つのScreenManagerについてDynamicTokensを再構築します。
        /// </summary>
        private static void RefreshScreen(
            object screenManager,
            object contentPack,
            object rawContentPack,
            IEnumerable dynamicTokens,
            object manifest,
            object migrator,
            object installedMods,
            object logPath,
            IMonitor monitor)
        {
            //----------------------------------------
            // TokenManager取得
            //----------------------------------------

            object? tokenManager =
                GetMemberValue(
                    screenManager,
                    "TokenManager");

            if (tokenManager == null)
            {
                throw new InvalidOperationException(
                    "Could not access Content Patcher TokenManager.");
            }

            //----------------------------------------
            // PatchLoader取得
            //----------------------------------------

            object? patchLoader =
                GetMemberValue(
                    screenManager,
                    "PatchLoader");

            if (patchLoader == null)
            {
                throw new InvalidOperationException(
                    "Could not access Content Patcher PatchLoader.");
            }

            //----------------------------------------
            // ModTokenContext取得
            //----------------------------------------

            MethodInfo? trackLocalTokensMethod =
                tokenManager
                    .GetType()
                    .GetMethods(
                        BindingFlags.Instance
                        | BindingFlags.Public
                        | BindingFlags.NonPublic)
                    .FirstOrDefault(method =>
                        method.Name == "TrackLocalTokens"
                        && method.GetParameters().Length == 1);

            if (trackLocalTokensMethod == null)
            {
                throw new InvalidOperationException(
                    "Could not access Content Patcher TrackLocalTokens().");
            }

            object? modContext =
                trackLocalTokensMethod.Invoke(
                    tokenManager,
                    new[]
                    {
                        rawContentPack
                    });

            if (modContext == null)
            {
                throw new InvalidOperationException(
                    "Could not access Content Patcher ModTokenContext.");
            }

            //----------------------------------------
            // 旧DynamicTokensを完全削除
            //----------------------------------------

            ClearDynamicTokens(
                modContext);

            //----------------------------------------
            // TokenParser生成
            //----------------------------------------

            object tokenParser =
                CreateTokenParser(
                    modContext,
                    manifest,
                    migrator,
                    installedMods);

            //----------------------------------------
            // AddDynamicToken取得
            //----------------------------------------

            MethodInfo? addDynamicTokenMethod =
                modContext
                    .GetType()
                    .GetMethods(
                        BindingFlags.Instance
                        | BindingFlags.Public
                        | BindingFlags.NonPublic)
                    .FirstOrDefault(method =>
                        method.Name == "AddDynamicToken"
                        && method.GetParameters().Length == 3);

            if (addDynamicTokenMethod == null)
            {
                throw new InvalidOperationException(
                    "Could not access Content Patcher AddDynamicToken().");
            }

            //----------------------------------------
            // DynamicTokens登録
            //----------------------------------------

            foreach (object? dynamicToken in dynamicTokens)
            {
                if (dynamicToken == null)
                    continue;

                string? name =
                    GetPropertyValue(
                        dynamicToken,
                        "Name")
                        ?.ToString();

                string? value =
                    GetPropertyValue(
                        dynamicToken,
                        "Value")
                        ?.ToString();

                object? when =
                    GetPropertyValue(
                        dynamicToken,
                        "When");

                //----------------------------------------
                // Nameチェック
                //----------------------------------------

                if (string.IsNullOrWhiteSpace(name))
                {
                    monitor.Log(
                        "Ignored Content Patcher dynamic token: " +
                        "the token name can't be empty.",
                        LogLevel.Warn);

                    continue;
                }

                if (name.Contains(':'))
                {
                    monitor.Log(
                        $"Ignored Content Patcher dynamic token '{name}': " +
                        "the token name can't have positional input arguments (: character).",
                        LogLevel.Warn);

                    continue;
                }

                //----------------------------------------
                // When解析
                //----------------------------------------

                if (!TryParseConditions(
                        patchLoader,
                        when,
                        tokenParser,
                        logPath,
                        out object? conditions,
                        out object? immutableRequiredModIds,
                        out string conditionError))
                {
                    monitor.Log(
                        $"Ignored dynamic token '{name}': " +
                        $"its When field is invalid: {conditionError}.",
                        LogLevel.Warn);

                    continue;
                }

                if (conditions == null
                    || immutableRequiredModIds == null)
                {
                    continue;
                }

                //----------------------------------------
                // Value解析
                //----------------------------------------

                object? managedValue;

                if (!string.IsNullOrWhiteSpace(value))
                {
                    if (!TryParseString(
                            tokenParser,
                            value,
                            immutableRequiredModIds,
                            logPath,
                            out managedValue,
                            out string valueError))
                    {
                        monitor.Log(
                            $"Ignored dynamic token '{name}': " +
                            $"the token value is invalid: {valueError}",
                            LogLevel.Warn);

                        continue;
                    }
                }
                else
                {
                    managedValue =
                        CreateLiteralString(
                            contentPack,
                            logPath);
                }

                if (managedValue == null)
                {
                    throw new InvalidOperationException(
                        $"Could not create the value for dynamic token '{name}'.");
                }

                //----------------------------------------
                // Dynamic Token登録
                //----------------------------------------

                addDynamicTokenMethod.Invoke(
                    modContext,
                    new[]
                    {
                        name,
                        managedValue,
                        conditions
                    });
            }

            //----------------------------------------
            // Token Context更新
            //----------------------------------------

            MethodInfo? updateContextMethod =
                tokenManager
                    .GetType()
                    .GetMethods(
                        BindingFlags.Instance
                        | BindingFlags.Public
                        | BindingFlags.NonPublic)
                    .FirstOrDefault(method =>
                        method.Name == "UpdateContext"
                        && method.GetParameters().Length == 1);

            if (updateContextMethod == null)
            {
                throw new InvalidOperationException(
                    "Could not access Content Patcher TokenManager.UpdateContext().");
            }

            updateContextMethod.Invoke(
                tokenManager,
                new object?[]
                {
                    null
                });
        }

        //----------------------------------------
        // 旧DynamicTokens削除
        //----------------------------------------

        /// <summary>
        /// ModTokenContextに登録されている
        /// Dynamic Token関連状態を完全に削除します。
        /// </summary>
        private static void ClearDynamicTokens(
            object modContext)
        {
            //----------------------------------------
            // DynamicTokens取得
            //----------------------------------------

            object? dynamicTokens =
                GetMemberValue(
                    modContext,
                    "DynamicTokens");

            object? dynamicContext =
                GetMemberValue(
                    modContext,
                    "DynamicContext");

            if (dynamicTokens == null
                || dynamicContext == null)
            {
                throw new InvalidOperationException(
                    "Could not access Content Patcher Dynamic Token context.");
            }

            //----------------------------------------
            // 現在登録されているToken名取得
            //----------------------------------------

            List<string> tokenNames =
                GetDictionaryKeys(
                    dynamicTokens);

            //----------------------------------------
            // DynamicContextからToken削除
            //----------------------------------------

            MethodInfo? removeMethod =
                dynamicContext
                    .GetType()
                    .GetMethod(
                        "Remove",
                        BindingFlags.Instance
                        | BindingFlags.Public
                        | BindingFlags.NonPublic,
                        binder: null,
                        types:
                        new[]
                        {
                            typeof(string)
                        },
                        modifiers: null);

            if (removeMethod == null)
            {
                throw new InvalidOperationException(
                    "Could not access Content Patcher DynamicContext.Remove().");
            }

            foreach (string tokenName in tokenNames)
            {
                removeMethod.Invoke(
                    dynamicContext,
                    new object[]
                    {
                        tokenName
                    });
            }

            //----------------------------------------
            // DynamicTokens
            //----------------------------------------

            ClearCollection(
                dynamicTokens);

            //----------------------------------------
            // DynamicTokenValues
            //----------------------------------------

            object? dynamicTokenValues =
                GetMemberValue(
                    modContext,
                    "DynamicTokenValues");

            if (dynamicTokenValues == null)
            {
                throw new InvalidOperationException(
                    "Could not access Content Patcher DynamicTokenValues.");
            }

            ClearCollection(
                dynamicTokenValues);

            //----------------------------------------
            // DynamicTokenDependencies
            //----------------------------------------

            object? dynamicTokenDependencies =
                GetMemberValue(
                    modContext,
                    "DynamicTokenDependencies");

            if (dynamicTokenDependencies == null)
            {
                throw new InvalidOperationException(
                    "Could not access Content Patcher DynamicTokenDependencies.");
            }

            ClearCollection(
                dynamicTokenDependencies);

            //----------------------------------------
            // DynamicTokenDependents
            //----------------------------------------

            object? dynamicTokenDependents =
                GetMemberValue(
                    modContext,
                    "DynamicTokenDependents");

            if (dynamicTokenDependents == null)
            {
                throw new InvalidOperationException(
                    "Could not access Content Patcher DynamicTokenDependents.");
            }

            ClearCollection(
                dynamicTokenDependents);

            //----------------------------------------
            // InterdependentTokens
            //----------------------------------------

            object? interdependentTokens =
                GetMemberValue(
                    modContext,
                    "InterdependentTokens");

            if (interdependentTokens == null)
            {
                throw new InvalidOperationException(
                    "Could not access Content Patcher InterdependentTokens.");
            }

            ClearCollection(
                interdependentTokens);

            //----------------------------------------
            // HasNewTokens
            //----------------------------------------

            SetMemberValue(
                modContext,
                "HasNewTokens",
                true);
        }

        //----------------------------------------
        // TokenParser生成
        //----------------------------------------

        /// <summary>
        /// Content Patcher内部のTokenParserを生成します。
        /// </summary>
        private static object CreateTokenParser(
            object modContext,
            object manifest,
            object migrator,
            object installedMods)
        {
            Assembly assembly =
                modContext
                    .GetType()
                    .Assembly;

            Type? tokenParserType =
                assembly.GetType(
                    "ContentPatcher.Framework.TokenParser");

            if (tokenParserType == null)
            {
                throw new InvalidOperationException(
                    "Could not find Content Patcher TokenParser.");
            }

            ConstructorInfo? constructor =
                tokenParserType
                    .GetConstructors(
                        BindingFlags.Instance
                        | BindingFlags.Public
                        | BindingFlags.NonPublic)
                    .FirstOrDefault(candidate =>
                        candidate.GetParameters().Length == 4);

            if (constructor == null)
            {
                throw new InvalidOperationException(
                    "Could not find Content Patcher TokenParser constructor.");
            }

            return constructor.Invoke(
                new[]
                {
                    modContext,
                    manifest,
                    migrator,
                    installedMods
                });
        }

        //----------------------------------------
        // When解析
        //----------------------------------------

        /// <summary>
        /// Content Patcher本来の処理を使用して
        /// Dynamic TokenのWhen条件を解析します。
        /// </summary>
        private static bool TryParseConditions(
            object patchLoader,
            object? when,
            object tokenParser,
            object logPath,
            out object? conditions,
            out object? immutableRequiredModIds,
            out string error)
        {
            conditions = null;
            immutableRequiredModIds = null;
            error = "";

            MethodInfo? method =
                patchLoader
                    .GetType()
                    .GetMethods(
                        BindingFlags.Instance
                        | BindingFlags.Public
                        | BindingFlags.NonPublic)
                    .FirstOrDefault(candidate =>
                        candidate.Name == "TryParseConditions"
                        && candidate.GetParameters().Length == 6);

            if (method == null)
            {
                throw new InvalidOperationException(
                    "Could not access Content Patcher PatchLoader.TryParseConditions().");
            }

            object?[] arguments =
            {
                when,
                tokenParser,
                logPath,
                null,
                null,
                null
            };

            bool success =
                method.Invoke(
                    patchLoader,
                    arguments)
                is true;

            conditions =
                arguments[3];

            immutableRequiredModIds =
                arguments[4];

            error =
                arguments[5]?.ToString()
                ?? "";

            return success;
        }

        //----------------------------------------
        // Value解析
        //----------------------------------------

        /// <summary>
        /// Content Patcher本来の処理を使用して
        /// Dynamic TokenのValueを解析します。
        /// </summary>
        private static bool TryParseString(
            object tokenParser,
            string value,
            object immutableRequiredModIds,
            object logPath,
            out object? managedValue,
            out string error)
        {
            managedValue = null;
            error = "";

            //----------------------------------------
            // TryParseString取得
            //----------------------------------------

            MethodInfo? method =
                tokenParser
                    .GetType()
                    .GetMethods(
                        BindingFlags.Instance
                        | BindingFlags.Public
                        | BindingFlags.NonPublic)
                    .FirstOrDefault(candidate =>
                        candidate.Name == "TryParseString"
                        && candidate.GetParameters().Length == 6);

            if (method == null)
            {
                throw new InvalidOperationException(
                    "Could not access Content Patcher TokenParser.TryParseString().");
            }

            //----------------------------------------
            // 引数
            //----------------------------------------

            object?[] arguments =
            {
                value,
                immutableRequiredModIds,
                logPath,

                // out string? error
                null,

                // out IManagedTokenString? parsed
                null,

                // Func<LexTokenToken, string?>? preValidate
                null
            };

            //----------------------------------------
            // 実行
            //----------------------------------------

            bool success =
                method.Invoke(
                    tokenParser,
                    arguments)
                is true;

            //----------------------------------------
            // out値取得
            //----------------------------------------

            error =
                arguments[3]?.ToString()
                ?? "";

            managedValue =
                arguments[4];

            return success;
        }

        //----------------------------------------
        // LiteralString生成
        //----------------------------------------

        /// <summary>
        /// 空のDynamic Token Value用LiteralStringを生成します。
        /// </summary>
        private static object CreateLiteralString(
            object contentPack,
            object logPath)
        {
            Assembly assembly =
                contentPack
                    .GetType()
                    .Assembly;

            Type? literalStringType =
                assembly.GetType(
                    "ContentPatcher.Framework.LiteralString");

            if (literalStringType == null)
            {
                throw new InvalidOperationException(
                    "Could not find Content Patcher LiteralString.");
            }

            ConstructorInfo? constructor =
                literalStringType
                    .GetConstructors(
                        BindingFlags.Instance
                        | BindingFlags.Public
                        | BindingFlags.NonPublic)
                    .FirstOrDefault(candidate =>
                        candidate.GetParameters().Length == 2);

            if (constructor == null)
            {
                throw new InvalidOperationException(
                    "Could not find Content Patcher LiteralString constructor.");
            }

            return constructor.Invoke(
                new object[]
                {
                    "",
                    logPath
                });
        }

        //----------------------------------------
        // Collection Clear
        //----------------------------------------

        /// <summary>
        /// ReflectionでCollectionをClearします。
        /// </summary>
        private static void ClearCollection(
            object collection)
        {
            MethodInfo? clearMethod =
                collection
                    .GetType()
                    .GetMethod(
                        "Clear",
                        BindingFlags.Instance
                        | BindingFlags.Public
                        | BindingFlags.NonPublic,
                        binder: null,
                        types: Type.EmptyTypes,
                        modifiers: null);

            if (clearMethod == null)
            {
                throw new InvalidOperationException(
                    $"Could not clear Content Patcher collection '{collection.GetType().Name}'.");
            }

            clearMethod.Invoke(
                collection,
                null);
        }

        //----------------------------------------
        // Dictionaryキー取得
        //----------------------------------------

        /// <summary>
        /// ReflectionでDictionaryのキー一覧を取得します。
        /// </summary>
        private static List<string> GetDictionaryKeys(
            object dictionary)
        {
            if (dictionary is not IEnumerable enumerable)
            {
                throw new InvalidOperationException(
                    "Could not enumerate Content Patcher DynamicTokens.");
            }

            List<string> keys =
                new();

            foreach (object? item in enumerable)
            {
                if (item == null)
                    continue;

                string? key =
                    GetPropertyValue(
                        item,
                        "Key")
                        ?.ToString();

                if (!string.IsNullOrWhiteSpace(
                        key))
                {
                    keys.Add(
                        key);
                }
            }

            return keys;
        }

        //----------------------------------------
        // Member取得
        //----------------------------------------

        /// <summary>
        /// ReflectionでFieldまたはPropertyの値を取得します。
        /// </summary>
        private static object? GetMemberValue(
            object instance,
            string memberName)
        {
            Type type =
                instance.GetType();

            PropertyInfo? property =
                type.GetProperty(
                    memberName,
                    BindingFlags.Instance
                    | BindingFlags.Public
                    | BindingFlags.NonPublic);

            if (property != null)
            {
                return property.GetValue(
                    instance);
            }

            FieldInfo? field =
                type.GetField(
                    memberName,
                    BindingFlags.Instance
                    | BindingFlags.Public
                    | BindingFlags.NonPublic);

            return field?.GetValue(
                instance);
        }

        //----------------------------------------
        // Member設定
        //----------------------------------------

        /// <summary>
        /// ReflectionでFieldまたはPropertyの値を設定します。
        /// </summary>
        private static void SetMemberValue(
            object instance,
            string memberName,
            object? value)
        {
            Type type =
                instance.GetType();

            PropertyInfo? property =
                type.GetProperty(
                    memberName,
                    BindingFlags.Instance
                    | BindingFlags.Public
                    | BindingFlags.NonPublic);

            if (property != null
                && property.CanWrite)
            {
                property.SetValue(
                    instance,
                    value);

                return;
            }

            FieldInfo? field =
                type.GetField(
                    memberName,
                    BindingFlags.Instance
                    | BindingFlags.Public
                    | BindingFlags.NonPublic);

            if (field == null)
            {
                throw new InvalidOperationException(
                    $"Could not access Content Patcher member '{memberName}'.");
            }

            field.SetValue(
                instance,
                value);
        }

        //----------------------------------------
        // Property取得
        //----------------------------------------

        /// <summary>
        /// Reflectionでプロパティ値を取得します。
        /// </summary>
        private static object? GetPropertyValue(
            object? instance,
            string propertyName)
        {
            return ContentPatcherReloadService
                .GetPropertyValue(
                    instance,
                    propertyName);
        }
    }
}