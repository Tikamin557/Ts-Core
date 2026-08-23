using StardewModdingAPI;
using System.Collections;
using System.Reflection;

namespace Ts_Core.Services.ContentPatcherRelated
{
    /// <summary>
    /// Content Patcher Content Packの
    /// 再読み込み処理全体を管理します。
    /// </summary>
    internal static class ContentPatcherReloadService
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
                    "Content Patcher reload is unavailable.",
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
                    "Content Patcher reload service initialized.",
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
                    "Failed to initialize Content Patcher reload service.\n" +
                    ex,
                    LogLevel.Warn);

                return false;
            }
        }

        //----------------------------------------
        // Content Pack再読み込み
        //----------------------------------------

        /// <summary>
        /// 指定したContent Patcher Content Packを
        /// 再読み込みします。
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
                    $"Reloading Content Patcher content pack '{contentPackId}'...",
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
                    ContentPatcherConfigReloadService
                        .GetDictionaryKeys(
                            currentConfig);

                //----------------------------------------
                // content.json再読み込み
                //----------------------------------------

                if (!TryReloadContent(
                        contentPack,
                        contentPackId,
                        monitor))
                {
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

                //----------------------------------------
                // 共通オブジェクト取得
                //----------------------------------------

                object? rawContentPack =
                    GetPropertyValue(
                        contentPack,
                        "ContentPack");

                if (rawContentPack == null)
                {
                    monitor.Log(
                        $"Could not access ContentPack for '{contentPackId}'.",
                        LogLevel.Warn);

                    return false;
                }

                //----------------------------------------
                // ConfigSchema再読み込み
                //----------------------------------------

                int newConfigCount =
                    ContentPatcherConfigReloadService.Refresh(
                        contentPatcherMod!,
                        screenManagerContainer!,
                        contentPack,
                        rawContentPack,
                        content,
                        currentConfig,
                        oldConfigKeys,
                        helper,
                        monitor);

                //----------------------------------------
                // DynamicTokens再読み込み
                //----------------------------------------

                ContentPatcherDynamicTokenReloadService.Refresh(
                    contentPatcherMod!,
                    screenManagerContainer!,
                    contentPack,
                    rawContentPack,
                    content,
                    monitor);

                //----------------------------------------
                // 古い永久無効Patch記録を削除
                //----------------------------------------

                ClearDisabledPatches(
                    contentPackId,
                    monitor);

                //----------------------------------------
                // Content Patcher自身のPatch Reload
                //----------------------------------------

                ReloadPatches(
                    contentPackId,
                    monitor);

                //----------------------------------------
                // 完了
                //----------------------------------------

                monitor.Log(
                    $"Content Patcher content pack reloaded for '{contentPackId}'. " +
                    $"Config fields: {oldConfigKeys.Count} -> {newConfigCount}",
                    LogLevel.Info);

                return true;
            }
            catch (TargetInvocationException ex)
            {
                Exception actualException =
                    ex.InnerException
                    ?? ex;

                monitor.Log(
                    $"Content Patcher reload failed for '{contentPackId}'.\n" +
                    actualException,
                    LogLevel.Error);

                return false;
            }
            catch (Exception ex)
            {
                monitor.Log(
                    $"Content Patcher reload failed for '{contentPackId}'.\n" +
                    ex,
                    LogLevel.Error);

                return false;
            }
        }

        //----------------------------------------
        // content.json再読み込み
        //----------------------------------------

        /// <summary>
        /// LoadedContentPackのcontent.jsonを再読み込みします。
        /// </summary>
        private static bool TryReloadContent(
            object contentPack,
            string contentPackId,
            IMonitor monitor)
        {
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

            return true;
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
        // Disabled Patch履歴削除
        //----------------------------------------

        /// <summary>
        /// 指定したContent Packに属する
        /// 古い永久無効Patch記録を削除します。
        /// </summary>
        private static void ClearDisabledPatches(
            string contentPackId,
            IMonitor monitor)
        {
            if (screenManagerContainer == null)
            {
                throw new InvalidOperationException(
                    "Content Patcher ScreenManager is unavailable.");
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
                // PatchManager取得
                //----------------------------------------

                object? patchManager =
                    GetMemberValue(
                        screenManager,
                        "PatchManager");

                if (patchManager == null)
                {
                    throw new InvalidOperationException(
                        "Could not access Content Patcher PatchManager.");
                }

                //----------------------------------------
                // PermanentlyDisabledPatches取得
                //----------------------------------------

                object? disabledPatches =
                    GetMemberValue(
                        patchManager,
                        "PermanentlyDisabledPatches");

                if (disabledPatches is not IList list)
                {
                    throw new InvalidOperationException(
                        "Could not access Content Patcher PermanentlyDisabledPatches.");
                }

                //----------------------------------------
                // 対象Content Pack分だけ削除
                //----------------------------------------

                for (int i = list.Count - 1;
                     i >= 0;
                     i--)
                {
                    object? disabledPatch =
                        list[i];

                    if (disabledPatch == null)
                        continue;

                    object? rawContentPack =
                        GetPropertyValue(
                            disabledPatch,
                            "ContentPack");

                    object? manifest =
                        GetPropertyValue(
                            rawContentPack,
                            "Manifest");

                    string? uniqueId =
                        GetPropertyValue(
                            manifest,
                            "UniqueID")
                            ?.ToString();

                    if (!string.Equals(
                            uniqueId,
                            contentPackId,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    list.RemoveAt(i);
                }
            }

            monitor.Log(
                $"Cleared old disabled patch records for '{contentPackId}'.",
                LogLevel.Trace);
        }

        //----------------------------------------
        // Property取得
        //----------------------------------------

        /// <summary>
        /// Reflectionでプロパティ値を取得します。
        /// </summary>
        internal static object? GetPropertyValue(
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
    }
}