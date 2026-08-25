using StardewModdingAPI;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;

namespace Ts_Core.Services.ContentPatcherRelated
{
    /// <summary>
    /// Content PatcherのConfigSchema再読み込みと
    /// GMCM再登録処理を管理します。
    /// </summary>
    internal static class ContentPatcherConfigReloadService
    {
        //----------------------------------------
        // ConfigSchema再読み込み
        //----------------------------------------

        /// <summary>
        /// 最新のConfigSchemaからConfigを再構築し、
        /// Config TokenとGMCMを更新します。
        /// </summary>
        internal static int Refresh(
            object contentPatcherMod,
            object screenManagerContainer,
            object contentPack,
            object rawContentPack,
            object content,
            object currentConfig,
            HashSet<string> oldConfigKeys,
            IModHelper helper,
            IMonitor monitor)
        {
            //----------------------------------------
            // ConfigSchema取得
            //----------------------------------------

            object? configSchema =
                GetPropertyValue(
                    content,
                    "ConfigSchema");

            object? format =
                GetPropertyValue(
                    content,
                    "Format");

            if (format == null)
            {
                throw new InvalidOperationException(
                    "Could not access Content Patcher Format.");
            }

            //----------------------------------------
            // ConfigFileHandler取得
            //----------------------------------------

            object? configFileHandler =
                GetPropertyValue(
                    contentPack,
                    "ConfigFileHandler");

            if (configFileHandler == null)
            {
                throw new InvalidOperationException(
                    "Could not access Content Patcher ConfigFileHandler.");
            }

            //----------------------------------------
            // 新ConfigSchemaからConfig再構築
            //----------------------------------------

            MethodInfo? readMethod =
                configFileHandler
                    .GetType()
                    .GetMethods(
                        BindingFlags.Instance
                        | BindingFlags.Public
                        | BindingFlags.NonPublic)
                    .FirstOrDefault(method =>
                        method.Name == "Read"
                        && method.GetParameters().Length == 3);

            if (readMethod == null)
            {
                throw new InvalidOperationException(
                    "Could not access Content Patcher ConfigFileHandler.Read().");
            }

            object? newConfig =
                readMethod.Invoke(
                    configFileHandler,
                    new[]
                    {
                        rawContentPack,
                        configSchema,
                        format
                    });

            if (newConfig == null)
            {
                throw new InvalidOperationException(
                    "Failed to rebuild Content Patcher ConfigSchema.");
            }

            //----------------------------------------
            // Config内容置換
            //----------------------------------------

            ReplaceDictionaryContents(
                currentConfig,
                newConfig);

            //----------------------------------------
            // 新Configキー取得
            //----------------------------------------

            HashSet<string> newConfigKeys =
                GetDictionaryKeys(
                    currentConfig);

            //----------------------------------------
            // config.json保存
            //----------------------------------------

            SaveConfig(
                configFileHandler,
                rawContentPack,
                currentConfig,
                helper);

            //----------------------------------------
            // Config Token再構築
            //----------------------------------------

            RefreshConfigTokens(
                screenManagerContainer,
                contentPack,
                rawContentPack,
                currentConfig,
                oldConfigKeys,
                monitor);

            //----------------------------------------
            // GMCM再登録
            //----------------------------------------

            RefreshConfigMenu(
                contentPatcherMod,
                contentPack,
                rawContentPack,
                currentConfig,
                configFileHandler,
                helper,
                monitor);

            //----------------------------------------
            // 結果
            //----------------------------------------

            return newConfigKeys.Count;
        }

        //----------------------------------------
        // Config Token更新
        //----------------------------------------

        /// <summary>
        /// Content PatcherのConfig Tokenを
        /// 現在のConfigSchemaに合わせて再構築します。
        /// </summary>
        private static void RefreshConfigTokens(
            object screenManagerContainer,
            object contentPack,
            object rawContentPack,
            object currentConfig,
            HashSet<string> oldConfigKeys,
            IMonitor monitor)
        {
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

            if (activeValues is not IEnumerable enumerable)
            {
                throw new InvalidOperationException(
                    "Could not enumerate Content Patcher active ScreenManagers.");
            }

            //----------------------------------------
            // 各Screen
            //----------------------------------------

            foreach (object? entry in enumerable)
            {
                if (entry == null)
                    continue;

                object? screenManager =
                    GetPropertyValue(
                        entry,
                        "Value");

                if (screenManager == null)
                    continue;

                //----------------------------------------
                // TokenManager取得
                //----------------------------------------

                object? tokenManager =
                    GetPropertyValue(
                        screenManager,
                        "TokenManager");

                if (tokenManager == null)
                {
                    throw new InvalidOperationException(
                        "Could not access Content Patcher TokenManager.");
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
                // RemoveLocalToken取得
                //----------------------------------------

                MethodInfo? removeLocalTokenMethod =
                    modContext
                        .GetType()
                        .GetMethod(
                            "RemoveLocalToken",
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

                if (removeLocalTokenMethod == null)
                {
                    throw new InvalidOperationException(
                        "Could not access Content Patcher RemoveLocalToken().");
                }

                //----------------------------------------
                // 旧Config Tokenをすべて削除
                //----------------------------------------

                foreach (string key in oldConfigKeys)
                {
                    removeLocalTokenMethod.Invoke(
                        modContext,
                        new object[]
                        {
                            key
                        });
                }

                //----------------------------------------
                // ScreenManager.AddConfigToken取得
                //----------------------------------------

                MethodInfo? addConfigTokenMethod =
                    screenManager
                        .GetType()
                        .GetMethods(
                            BindingFlags.Instance
                            | BindingFlags.Public
                            | BindingFlags.NonPublic)
                        .FirstOrDefault(method =>
                            method.Name == "AddConfigToken"
                            && method.GetParameters().Length == 4);

                if (addConfigTokenMethod == null)
                {
                    throw new InvalidOperationException(
                        "Could not access Content Patcher AddConfigToken().");
                }

                //----------------------------------------
                // 新Config Tokenを追加
                //----------------------------------------

                if (currentConfig is not IEnumerable configEnumerable)
                {
                    throw new InvalidOperationException(
                        "Could not enumerate Content Patcher Config.");
                }

                foreach (object? item in configEnumerable)
                {
                    if (item == null)
                        continue;

                    string? key =
                        GetPropertyValue(
                            item,
                            "Key")
                            ?.ToString();

                    object? value =
                        GetPropertyValue(
                            item,
                            "Value");

                    if (string.IsNullOrWhiteSpace(
                            key)
                        || value == null)
                    {
                        continue;
                    }

                    addConfigTokenMethod.Invoke(
                        screenManager,
                        new object[]
                        {
                            key,
                            value,
                            modContext,
                            contentPack
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

                object?[] updateArguments =
                {
                    null
                };

                updateContextMethod.Invoke(
                    tokenManager,
                    updateArguments);
            }

            monitor.Log(
                "Content Patcher config tokens updated.",
                LogLevel.Trace);
        }

        //----------------------------------------
        // Config保存
        //----------------------------------------

        /// <summary>
        /// 再構築したConfigをconfig.jsonへ保存します。
        /// </summary>
        private static void SaveConfig(
            object configFileHandler,
            object rawContentPack,
            object config,
            IModHelper helper)
        {
            MethodInfo? saveMethod =
                configFileHandler
                    .GetType()
                    .GetMethods(
                        BindingFlags.Instance
                        | BindingFlags.Public
                        | BindingFlags.NonPublic)
                    .FirstOrDefault(method =>
                        method.Name == "Save"
                        && method.GetParameters().Length == 3);

            if (saveMethod == null)
            {
                throw new InvalidOperationException(
                    "Could not access Content Patcher ConfigFileHandler.Save().");
            }

            saveMethod.Invoke(
                configFileHandler,
                new object[]
                {
                    rawContentPack,
                    config,
                    helper
                });
        }

        //----------------------------------------
        // フィルタ済みConfigでGMCM再登録
        //----------------------------------------

        /// <summary>
        /// あらかじめ生成されたGMCM表示用Configを使用して、
        /// Content PackのGMCM項目だけを再登録します。
        /// </summary>
        internal static void RefreshConfigMenuOnly(
            object contentPatcherMod,
            object contentPack,
            object rawContentPack,
            object currentConfig,
            object gmcmConfig,
            IModHelper helper,
            IMonitor monitor)
        {
            //----------------------------------------
            // ConfigFileHandler取得
            //----------------------------------------

            object? configFileHandler =
                GetPropertyValue(
                    contentPack,
                    "ConfigFileHandler");

            if (configFileHandler == null)
            {
                throw new InvalidOperationException(
                    "Could not access Content Patcher ConfigFileHandler.");
            }

            //----------------------------------------
            // GMCM再登録
            //----------------------------------------

            RefreshConfigMenu(
                contentPatcherMod,
                contentPack,
                rawContentPack,
                currentConfig,
                gmcmConfig,
                configFileHandler,
                helper,
                monitor);
        }

        //----------------------------------------
        // GMCM再登録
        //----------------------------------------

        /// <summary>
        /// 現在のConfigを使用して、
        /// T's Core独自の表示条件を適用したGMCM項目を再登録します。
        /// </summary>
        private static void RefreshConfigMenu(
            object contentPatcherMod,
            object contentPack,
            object rawContentPack,
            object currentConfig,
            object configFileHandler,
            IModHelper helper,
            IMonitor monitor)
        {
            //----------------------------------------
            // GMCM表示用Config生成
            //----------------------------------------

            object gmcmConfig =
                ContentPatcherConfigMenuFilterService
                    .CreateFilteredConfig(
                        currentConfig,
                        rawContentPack,
                        helper,
                        monitor);

            //----------------------------------------
            // GMCM再登録
            //----------------------------------------

            RefreshConfigMenu(
                contentPatcherMod,
                contentPack,
                rawContentPack,
                currentConfig,
                gmcmConfig,
                configFileHandler,
                helper,
                monitor);
        }

        /// <summary>
        /// 指定されたGMCM表示用Configを使用して、
        /// Content PackのGMCM項目を再登録します。
        /// </summary>
        private static void RefreshConfigMenu(
            object contentPatcherMod,
            object contentPack,
            object rawContentPack,
            object currentConfig,
            object gmcmConfig,
            object configFileHandler,
            IModHelper helper,
            IMonitor monitor)
        {
            //----------------------------------------
            // Content Patcher Assembly
            //----------------------------------------

            Assembly assembly =
                contentPatcherMod
                    .GetType()
                    .Assembly;

            //----------------------------------------
            // Manifest取得
            //----------------------------------------

            object? manifest =
                GetPropertyValue(
                    contentPack,
                    "Manifest");

            if (manifest == null)
            {
                throw new InvalidOperationException(
                    "Could not access Content Patcher Content Pack manifest.");
            }

            //----------------------------------------
            // Config型
            //----------------------------------------

            Type configType =
                currentConfig.GetType();

            //----------------------------------------
            // GenericModConfigMenuIntegration<T>
            //----------------------------------------

            Type? genericMenuTypeDefinition =
                assembly.GetType(
                    "Pathoschild.Stardew.Common.Integrations.GenericModConfigMenu.GenericModConfigMenuIntegration`1");

            if (genericMenuTypeDefinition == null)
            {
                throw new InvalidOperationException(
                    "Could not find Content Patcher GenericModConfigMenuIntegration.");
            }

            Type genericMenuType =
                genericMenuTypeDefinition
                    .MakeGenericType(
                        configType);

            //----------------------------------------
            // Func<TConfig>作成
            //----------------------------------------

            Type getConfigDelegateType =
                typeof(Func<>)
                    .MakeGenericType(
                        configType);

            Delegate getConfigDelegate =
                Expression
                    .Lambda(
                        getConfigDelegateType,
                        Expression.Constant(
                            currentConfig,
                            configType))
                    .Compile();

            //----------------------------------------
            // Reset Action
            //----------------------------------------

            Action resetAction =
                () =>
                {
                    ResetConfig(
                        currentConfig);
                };

            //----------------------------------------
            // SaveAndApply Action
            //----------------------------------------

            MethodInfo? onConfigChangedMethod =
                contentPatcherMod
                    .GetType()
                    .GetMethod(
                        "OnContentPackConfigChanged",
                        BindingFlags.Instance
                        | BindingFlags.Public
                        | BindingFlags.NonPublic);

            if (onConfigChangedMethod == null)
            {
                throw new InvalidOperationException(
                    "Could not access Content Patcher OnContentPackConfigChanged().");
            }

            Action saveAndApplyAction =
                () =>
                {
                    SaveConfig(
                        configFileHandler,
                        rawContentPack,
                        currentConfig,
                        helper);

                    onConfigChangedMethod.Invoke(
                        contentPatcherMod,
                        new[]
                        {
                            contentPack
                        });
                };

            //----------------------------------------
            // Generic Integration生成
            //----------------------------------------

            ConstructorInfo? genericMenuConstructor =
                genericMenuType
                    .GetConstructors(
                        BindingFlags.Instance
                        | BindingFlags.Public
                        | BindingFlags.NonPublic)
                    .FirstOrDefault(constructor =>
                        constructor.GetParameters().Length == 6);

            if (genericMenuConstructor == null)
            {
                throw new InvalidOperationException(
                    "Could not find Content Patcher GenericModConfigMenuIntegration constructor.");
            }

            object genericMenu =
                genericMenuConstructor.Invoke(
                    new object[]
                    {
                        helper.ModRegistry,
                        monitor,
                        manifest,
                        getConfigDelegate,
                        resetAction,
                        saveAndApplyAction
                    });

            //----------------------------------------
            // GenericModConfigMenuIntegrationForContentPack
            //----------------------------------------

            Type? contentPackMenuType =
                assembly.GetType(
                    "ContentPatcher.Framework.GenericModConfigMenuIntegrationForContentPack");

            if (contentPackMenuType == null)
            {
                throw new InvalidOperationException(
                    "Could not find Content Patcher GenericModConfigMenuIntegrationForContentPack.");
            }

            //----------------------------------------
            // Constructor取得
            //----------------------------------------

            ConstructorInfo? contentPackMenuConstructor =
                contentPackMenuType
                    .GetConstructors(
                        BindingFlags.Instance
                        | BindingFlags.Public
                        | BindingFlags.NonPublic)
                    .FirstOrDefault(constructor =>
                        constructor.GetParameters().Length == 3);

            if (contentPackMenuConstructor == null)
            {
                throw new InvalidOperationException(
                    "Could not find Content Patcher Content Pack GMCM constructor.");
            }

            //----------------------------------------
            // ParseCommaDelimitedField Delegate
            //----------------------------------------

            MethodInfo? parseMethod =
                contentPatcherMod
                    .GetType()
                    .GetMethod(
                        "ParseCommaDelimitedField",
                        BindingFlags.Instance
                        | BindingFlags.Public
                        | BindingFlags.NonPublic);

            if (parseMethod == null)
            {
                throw new InvalidOperationException(
                    "Could not access Content Patcher ParseCommaDelimitedField().");
            }

            Type parseDelegateType =
                contentPackMenuConstructor
                    .GetParameters()[1]
                    .ParameterType;

            Delegate parseDelegate =
                parseMethod.CreateDelegate(
                    parseDelegateType,
                    contentPatcherMod);

            //----------------------------------------
            // Content Pack GMCM Integration生成
            //----------------------------------------

            object contentPackMenu =
                contentPackMenuConstructor.Invoke(
                    new object[]
                    {
                        rawContentPack,
                        parseDelegate,
                        gmcmConfig
                    });

            //----------------------------------------
            // Register取得
            //----------------------------------------

            MethodInfo? registerMethod =
                contentPackMenuType
                    .GetMethods(
                        BindingFlags.Instance
                        | BindingFlags.Public
                        | BindingFlags.NonPublic)
                    .FirstOrDefault(method =>
                        method.Name == "Register"
                        && method.GetParameters().Length == 2);

            if (registerMethod == null)
            {
                throw new InvalidOperationException(
                    "Could not access Content Patcher GMCM Register().");
            }

            //----------------------------------------
            // GMCM再登録
            //----------------------------------------

            registerMethod.Invoke(
                contentPackMenu,
                new object[]
                {
                    genericMenu,
                    monitor
                });

            monitor.Log(
                "Content Patcher GMCM configuration re-registered.",
                LogLevel.Trace);
        }

        //----------------------------------------
        // Config Reset
        //----------------------------------------

        /// <summary>
        /// Configの全項目をDefault値へ戻します。
        /// </summary>
        private static void ResetConfig(
            object config)
        {
            if (config is not IEnumerable enumerable)
                return;

            foreach (object? item in enumerable)
            {
                if (item == null)
                    continue;

                object? field =
                    GetPropertyValue(
                        item,
                        "Value");

                if (field == null)
                    continue;

                //----------------------------------------
                // DefaultValues
                //----------------------------------------

                object? defaultValues =
                    GetPropertyValue(
                        field,
                        "DefaultValues");

                if (defaultValues == null)
                    continue;

                //----------------------------------------
                // SetValue
                //----------------------------------------

                MethodInfo? setValueMethod =
                    field
                        .GetType()
                        .GetMethods(
                            BindingFlags.Instance
                            | BindingFlags.Public
                            | BindingFlags.NonPublic)
                        .FirstOrDefault(method =>
                            method.Name == "SetValue"
                            && method.GetParameters().Length == 1);

                if (setValueMethod == null)
                    continue;

                setValueMethod.Invoke(
                    field,
                    new[]
                    {
                        defaultValues
                    });
            }
        }

        //----------------------------------------
        // Dictionary置換
        //----------------------------------------

        /// <summary>
        /// Dictionaryオブジェクト自体を維持したまま、
        /// 内容だけ新しいDictionaryへ置き換えます。
        /// </summary>
        private static void ReplaceDictionaryContents(
            object target,
            object source)
        {
            MethodInfo? clearMethod =
                target
                    .GetType()
                    .GetMethod(
                        "Clear",
                        BindingFlags.Instance
                        | BindingFlags.Public
                        | BindingFlags.NonPublic);

            PropertyInfo? indexer =
                target
                    .GetType()
                    .GetProperty(
                        "Item",
                        BindingFlags.Instance
                        | BindingFlags.Public
                        | BindingFlags.NonPublic);

            if (clearMethod == null
                || indexer == null
                || source is not IEnumerable enumerable)
            {
                throw new InvalidOperationException(
                    "Could not update Content Patcher Config dictionary.");
            }

            clearMethod.Invoke(
                target,
                null);

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

                if (key == null)
                    continue;

                indexer.SetValue(
                    target,
                    value,
                    new[]
                    {
                        key
                    });
            }
        }

        //----------------------------------------
        // Dictionaryキー取得
        //----------------------------------------

        /// <summary>
        /// Config Dictionaryのキー一覧を取得します。
        /// </summary>
        internal static HashSet<string> GetDictionaryKeys(
            object dictionary)
        {
            HashSet<string> keys =
                new(
                    StringComparer.OrdinalIgnoreCase);

            if (dictionary is not IEnumerable enumerable)
                return keys;

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
    }
}