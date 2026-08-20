using StardewModdingAPI;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;

namespace Ts_Core.Services.ContentPatcherRelated
{
    /// <summary>
    /// Content PatcherのConfigSchema再読み込み処理を管理します。
    /// </summary>
    internal static class ContentPatcherConfigReloadService
    {
        //----------------------------------------
        // Mod ID
        //----------------------------------------

        private const string ContentPatcherModId =
            "Pathoschild.ContentPatcher";

        //----------------------------------------
        // Content Patcher内部情報
        //----------------------------------------

        private static object? contentPatcherMod;
        private static object? contentPacks;
        private static object? screenManagerContainer;

        private static object? commandHandler;
        private static MethodInfo? commandHandleMethod;

        private static bool initialized;

        //----------------------------------------
        // 初期化
        //----------------------------------------

        /// <summary>
        /// Content Patcher内部の必要なオブジェクトを取得します。
        /// </summary>
        internal static bool Initialize(
            IModHelper helper,
            IMonitor monitor)
        {
            //----------------------------------------
            // 初期化済み
            //----------------------------------------

            if (initialized)
            {
                return contentPatcherMod != null
                    && contentPacks != null
                    && screenManagerContainer != null
                    && commandHandler != null
                    && commandHandleMethod != null;
            }

            //----------------------------------------
            // Content Patcher確認
            //----------------------------------------

            IModInfo? modInfo =
                helper.ModRegistry.Get(
                    ContentPatcherModId);

            if (modInfo == null)
            {
                monitor.Log(
                    "Content Patcher was not found. " +
                    "Content Patcher config reload is unavailable.",
                    LogLevel.Trace);

                return false;
            }

            try
            {
                //----------------------------------------
                // Content Patcher ModEntry取得
                //----------------------------------------

                PropertyInfo? modProperty =
                    modInfo
                        .GetType()
                        .GetProperty(
                            "Mod",
                            BindingFlags.Instance
                            | BindingFlags.Public
                            | BindingFlags.NonPublic);

                if (modProperty == null)
                {
                    monitor.Log(
                        "Could not access the Content Patcher mod instance.",
                        LogLevel.Warn);

                    return false;
                }

                contentPatcherMod =
                    modProperty.GetValue(
                        modInfo);

                if (contentPatcherMod == null)
                {
                    monitor.Log(
                        "Content Patcher mod instance was null.",
                        LogLevel.Warn);

                    return false;
                }

                //----------------------------------------
                // ContentPacks取得
                //----------------------------------------

                FieldInfo? contentPacksField =
                    contentPatcherMod
                        .GetType()
                        .GetField(
                            "ContentPacks",
                            BindingFlags.Instance
                            | BindingFlags.Public
                            | BindingFlags.NonPublic);

                if (contentPacksField == null)
                {
                    monitor.Log(
                        "Could not access Content Patcher ContentPacks.",
                        LogLevel.Warn);

                    return false;
                }

                contentPacks =
                    contentPacksField.GetValue(
                        contentPatcherMod);

                if (contentPacks == null)
                {
                    monitor.Log(
                        "Content Patcher ContentPacks was null.",
                        LogLevel.Warn);

                    return false;
                }

                //----------------------------------------
                // ScreenManager取得
                //----------------------------------------

                FieldInfo? screenManagerField =
                    contentPatcherMod
                        .GetType()
                        .GetField(
                            "ScreenManager",
                            BindingFlags.Instance
                            | BindingFlags.Public
                            | BindingFlags.NonPublic);

                if (screenManagerField == null)
                {
                    monitor.Log(
                        "Could not access Content Patcher ScreenManager.",
                        LogLevel.Warn);

                    return false;
                }

                screenManagerContainer =
                    screenManagerField.GetValue(
                        contentPatcherMod);

                if (screenManagerContainer == null)
                {
                    monitor.Log(
                        "Content Patcher ScreenManager was null.",
                        LogLevel.Warn);

                    return false;
                }

                //----------------------------------------
                // CommandHandler取得
                //----------------------------------------

                FieldInfo? commandHandlerField =
                    contentPatcherMod
                        .GetType()
                        .GetField(
                            "CommandHandler",
                            BindingFlags.Instance
                            | BindingFlags.Public
                            | BindingFlags.NonPublic);

                if (commandHandlerField == null)
                {
                    monitor.Log(
                        "Could not access Content Patcher CommandHandler.",
                        LogLevel.Warn);

                    return false;
                }

                object? handler =
                    commandHandlerField.GetValue(
                        contentPatcherMod);

                if (handler == null)
                {
                    monitor.Log(
                        "Content Patcher CommandHandler was null.",
                        LogLevel.Warn);

                    return false;
                }

                //----------------------------------------
                // CommandHandler.Handle取得
                //----------------------------------------

                MethodInfo? handleMethod =
                    handler
                        .GetType()
                        .GetMethod(
                            "Handle",
                            BindingFlags.Instance
                            | BindingFlags.Public
                            | BindingFlags.NonPublic,
                            binder: null,
                            types:
                            new[]
                            {
                                typeof(string[])
                            },
                            modifiers: null);

                if (handleMethod == null)
                {
                    monitor.Log(
                        "Could not access Content Patcher CommandHandler.Handle().",
                        LogLevel.Warn);

                    return false;
                }

                //----------------------------------------
                // 取得結果を保存
                //----------------------------------------

                commandHandler =
                    handler;

                commandHandleMethod =
                    handleMethod;

                //----------------------------------------
                // 成功
                //----------------------------------------

                initialized = true;

                monitor.Log(
                    "Content Patcher config reload service initialized.",
                    LogLevel.Trace);

                return true;
            }
            catch (Exception ex)
            {
                initialized = false;

                contentPatcherMod = null;
                contentPacks = null;
                screenManagerContainer = null;
                commandHandler = null;
                commandHandleMethod = null;

                monitor.Log(
                    "Failed to initialize Content Patcher config reload service.\n" +
                    ex,
                    LogLevel.Warn);

                return false;
            }


        }

        //----------------------------------------
        // Content Pack再読み込み
        //----------------------------------------

        /// <summary>
        /// 指定したContent Patcher Content Packの
        /// ConfigSchemaを再読み込みします。
        /// </summary>
        internal static bool ReloadContentPack(
            string contentPackId,
            IModHelper helper,
            IMonitor monitor)
        {
            //----------------------------------------
            // ID確認
            //----------------------------------------

            if (string.IsNullOrWhiteSpace(
                    contentPackId))
            {
                monitor.Log(
                    "Content Pack ID is required.",
                    LogLevel.Warn);

                return false;
            }

            //----------------------------------------
            // 初期化
            //----------------------------------------

            if (!Initialize(
                    helper,
                    monitor))
            {
                return false;
            }

            try
            {
                //----------------------------------------
                // 対象Content Pack取得
                //----------------------------------------

                object? contentPack =
                    FindContentPack(
                        contentPackId);

                if (contentPack == null)
                {
                    monitor.Log(
                        $"No Content Patcher content pack with the unique ID '{contentPackId}'.",
                        LogLevel.Warn);

                    return false;
                }

                monitor.Log(
                    $"Reloading Content Patcher config schema for '{contentPackId}'...",
                    LogLevel.Info);

                //----------------------------------------
                // 現在のConfig取得
                //----------------------------------------

                object? currentConfig =
                    GetPropertyValue(
                        contentPack,
                        "Config");

                if (currentConfig == null)
                {
                    monitor.Log(
                        $"Could not access Config for '{contentPackId}'.",
                        LogLevel.Warn);

                    return false;
                }

                //----------------------------------------
                // 旧Configキー保存
                //----------------------------------------

                HashSet<string> oldConfigKeys =
                    GetDictionaryKeys(
                        currentConfig);

                //----------------------------------------
                // content.json再読み込み
                //----------------------------------------

                MethodInfo? tryReloadContentMethod =
                    contentPack
                        .GetType()
                        .GetMethod(
                            "TryReloadContent",
                            BindingFlags.Instance
                            | BindingFlags.Public
                            | BindingFlags.NonPublic);

                if (tryReloadContentMethod == null)
                {
                    monitor.Log(
                        $"Could not access TryReloadContent() for '{contentPackId}'.",
                        LogLevel.Warn);

                    return false;
                }

                object?[] reloadArguments =
                {
                    null
                };

                bool reloadSuccess =
                    tryReloadContentMethod.Invoke(
                        contentPack,
                        reloadArguments)
                    is true;

                if (!reloadSuccess)
                {
                    string error =
                        reloadArguments[0]?.ToString()
                        ?? "Unknown error.";

                    monitor.Log(
                        $"Failed to reload content pack '{contentPackId}': {error}",
                        LogLevel.Error);

                    return false;
                }

                //----------------------------------------
                // 最新Content取得
                //----------------------------------------

                object? content =
                    GetPropertyValue(
                        contentPack,
                        "Content");

                if (content == null)
                {
                    monitor.Log(
                        $"Could not access Content for '{contentPackId}'.",
                        LogLevel.Warn);

                    return false;
                }

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
                    monitor.Log(
                        $"Could not access Format for '{contentPackId}'.",
                        LogLevel.Warn);

                    return false;
                }

                //----------------------------------------
                // ConfigFileHandler取得
                //----------------------------------------

                object? configFileHandler =
                    GetPropertyValue(
                        contentPack,
                        "ConfigFileHandler");

                object? rawContentPack =
                    GetPropertyValue(
                        contentPack,
                        "ContentPack");

                if (configFileHandler == null
                    || rawContentPack == null)
                {
                    monitor.Log(
                        $"Could not access configuration objects for '{contentPackId}'.",
                        LogLevel.Warn);

                    return false;
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
                    monitor.Log(
                        $"Could not access ConfigFileHandler.Read() for '{contentPackId}'.",
                        LogLevel.Warn);

                    return false;
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
                    monitor.Log(
                        $"Failed to rebuild ConfigSchema for '{contentPackId}'.",
                        LogLevel.Warn);

                    return false;
                }

                //----------------------------------------
                // Config内容を置換
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
                    contentPack,
                    rawContentPack,
                    currentConfig,
                    oldConfigKeys,
                    monitor);

                //----------------------------------------
                // Content Patcher自身のPatch Reload
                //----------------------------------------

                ReloadPatches(
                    contentPackId,
                    monitor);

                //----------------------------------------
                // GMCM再登録
                //----------------------------------------

                RefreshConfigMenu(
                    contentPack,
                    rawContentPack,
                    currentConfig,
                    configFileHandler,
                    helper,
                    monitor);

                //----------------------------------------
                // 完了
                //----------------------------------------

                monitor.Log(
                    $"Content Patcher config schema reloaded for '{contentPackId}'. " +
                    $"Config fields: {oldConfigKeys.Count} -> {newConfigKeys.Count}",
                    LogLevel.Info);

                return true;
            }
            catch (TargetInvocationException ex)
            {
                Exception actualException =
                    ex.InnerException
                    ?? ex;

                monitor.Log(
                    $"Content Patcher config reload failed for '{contentPackId}'.\n" +
                    actualException,
                    LogLevel.Error);

                return false;
            }
            catch (Exception ex)
            {
                monitor.Log(
                    $"Content Patcher config reload failed for '{contentPackId}'.\n" +
                    ex,
                    LogLevel.Error);

                return false;
            }
        }

        //----------------------------------------
        // Content Pack検索
        //----------------------------------------

        /// <summary>
        /// UniqueIDからLoadedContentPackを検索します。
        /// </summary>
        private static object? FindContentPack(
            string contentPackId)
        {
            if (contentPacks is not IEnumerable enumerable)
                return null;

            foreach (object? pack in enumerable)
            {
                if (pack == null)
                    continue;

                object? manifest =
                    GetPropertyValue(
                        pack,
                        "Manifest");

                string? uniqueId =
                    GetPropertyValue(
                        manifest,
                        "UniqueID")
                        ?.ToString();

                if (string.Equals(
                        uniqueId,
                        contentPackId,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return pack;
                }
            }

            return null;
        }

        //----------------------------------------
        // Config Token更新
        //----------------------------------------

        /// <summary>
        /// Content PatcherのConfig Tokenを
        /// 現在のConfigSchemaに合わせて再構築します。
        /// </summary>
        private static void RefreshConfigTokens(
            object contentPack,
            object rawContentPack,
            object currentConfig,
            HashSet<string> oldConfigKeys,
            IMonitor monitor)
        {
            if (screenManagerContainer == null)
                return;

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

                if (currentConfig is IEnumerable configEnumerable)
                {
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
                return;

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
        // Patch Reload
        //----------------------------------------

        /// <summary>
        /// Content Patcher自身のreloadコマンドを使用して
        /// Content PackのPatchを再読み込みします。
        /// </summary>
        private static void ReloadPatches(
            string contentPackId,
            IMonitor monitor)
        {
            if (commandHandler == null
                || commandHandleMethod == null)
            {
                throw new InvalidOperationException(
                    "Content Patcher CommandHandler is unavailable.");
            }

            monitor.Log(
                $"Reloading Content Patcher patches for '{contentPackId}'...",
                LogLevel.Trace);

            commandHandleMethod.Invoke(
                commandHandler,
                new object[]
                {
            new[]
            {
                "reload",
                contentPackId
            }
                });
        }

        //----------------------------------------
        // GMCM再登録
        //----------------------------------------

        /// <summary>
        /// 現在のConfigを使用して
        /// Content PackのGMCM項目を再登録します。
        /// </summary>
        private static void RefreshConfigMenu(
            object contentPack,
            object rawContentPack,
            object currentConfig,
            object configFileHandler,
            IModHelper helper,
            IMonitor monitor)
        {
            if (contentPatcherMod == null)
            {
                throw new InvalidOperationException(
                    "Content Patcher mod instance is unavailable.");
            }

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
                currentConfig
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
        private static HashSet<string> GetDictionaryKeys(
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

        /// <summary>
        /// Reflectionでプロパティ値を取得します。
        /// </summary>
        private static object? GetPropertyValue(
            object? instance,
            string propertyName)
        {
            if (instance == null)
                return null;

            PropertyInfo? property =
                instance
                    .GetType()
                    .GetProperty(
                        propertyName,
                        BindingFlags.Instance
                        | BindingFlags.Public
                        | BindingFlags.NonPublic);

            return property?.GetValue(
                instance);
        }
    }
}