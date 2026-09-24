using StardewModdingAPI;
using System.Collections;
using System.Reflection;
using Ts_Core.Interfaces;

namespace Ts_Core.Services.ShortcutPanelRelated
{
    /// <summary>
    /// Shortcut Panelから利用するGMCM関連機能です。
    /// 一覧取得だけReflectionを使用し、設定画面を開く処理は公式APIを使用します。
    /// </summary>
    internal static class ShortcutPanelGmcmService
    {
        private const string GenericModConfigMenuId =
            "spacechase0.GenericModConfigMenu";

        private const string ProxyTargetFieldName = "__Target";
        private const string ConfigManagerFieldName = "ConfigManager";
        private const string GetAllMethodName = "GetAll";
        private const string AnyEditableInGamePropertyName = "AnyEditableInGame";
        private const string ModManifestPropertyName = "ModManifest";
        private const string OpenListMenuNewMethodName = "OpenListMenuNew";

        /// <summary>
        /// Generic Mod Config Menuが現在インストールされているかを判定します。
        /// </summary>
        internal static bool IsInstalled(IModHelper helper)
        {
            return helper.ModRegistry.IsLoaded(GenericModConfigMenuId);
        }

        /// <summary>
        /// GMCM内部情報をReflectionで参照し、ゲーム内で設定変更可能なMod一覧を取得します。
        /// </summary>
        internal static IReadOnlyList<IManifest> GetEditableMods(
            IModHelper helper,
            IMonitor monitor)
        {
            try
            {
                IGenericModConfigMenuApi? api =
                    helper.ModRegistry.GetApi<IGenericModConfigMenuApi>(
                        GenericModConfigMenuId);

                if (api == null)
                    return Array.Empty<IManifest>();

                object gmcmApi = GetActualApiObject(api);

                FieldInfo? configManagerField =
                    gmcmApi.GetType().GetField(
                        ConfigManagerFieldName,
                        BindingFlags.Instance |
                        BindingFlags.Public |
                        BindingFlags.NonPublic);

                if (configManagerField == null)
                {
                    LogReflectionFailure(
                        monitor,
                        $"Field '{ConfigManagerFieldName}' was not found.");
                    return Array.Empty<IManifest>();
                }

                object? configManager =
                    configManagerField.GetValue(gmcmApi);

                if (configManager == null)
                {
                    LogReflectionFailure(monitor, "ConfigManager was null.");
                    return Array.Empty<IManifest>();
                }

                MethodInfo? getAllMethod =
                    configManager.GetType().GetMethod(
                        GetAllMethodName,
                        BindingFlags.Instance |
                        BindingFlags.Public |
                        BindingFlags.NonPublic);

                if (getAllMethod == null)
                {
                    LogReflectionFailure(
                        monitor,
                        $"Method '{GetAllMethodName}' was not found.");
                    return Array.Empty<IManifest>();
                }

                object? rawConfigs =
                    getAllMethod.Invoke(configManager, null);

                if (rawConfigs is not IEnumerable configs)
                {
                    LogReflectionFailure(
                        monitor,
                        "GetAll did not return an enumerable value.");
                    return Array.Empty<IManifest>();
                }

                List<IManifest> manifests = new();

                foreach (object? config in configs)
                {
                    if (config == null)
                        continue;

                    Type configType = config.GetType();

                    PropertyInfo? editableProperty =
                        configType.GetProperty(
                            AnyEditableInGamePropertyName,
                            BindingFlags.Instance |
                            BindingFlags.Public |
                            BindingFlags.NonPublic);

                    PropertyInfo? manifestProperty =
                        configType.GetProperty(
                            ModManifestPropertyName,
                            BindingFlags.Instance |
                            BindingFlags.Public |
                            BindingFlags.NonPublic);

                    if (editableProperty == null || manifestProperty == null)
                    {
                        LogReflectionFailure(
                            monitor,
                            "Required ModConfig properties were not found.");
                        return Array.Empty<IManifest>();
                    }

                    if (editableProperty.GetValue(config) is not bool editable
                        || !editable)
                    {
                        continue;
                    }

                    if (manifestProperty.GetValue(config) is IManifest manifest)
                        manifests.Add(manifest);
                }

                return manifests
                    .OrderBy(
                        manifest => manifest.Name,
                        StringComparer.CurrentCultureIgnoreCase)
                    .ToArray();
            }
            catch (Exception ex)
            {
                monitor.Log(
                    "Failed to retrieve GMCM mods for the Shortcut Panel. " +
                    "The GMCM internal structure may have changed.\n" + ex,
                    LogLevel.Warn);

                return Array.Empty<IManifest>();
            }
        }

        /// <summary>
        /// 保存済みUniqueIDから現在のManifestを取得します。
        /// </summary>
        internal static IManifest? GetManifest(
            IModHelper helper,
            string? modId)
        {
            if (string.IsNullOrWhiteSpace(modId))
                return null;

            return helper.ModRegistry.Get(modId)?.Manifest;
        }

        /// <summary>
        /// 指定ModのGMCM設定画面を公式APIで開きます。
        /// </summary>
        internal static bool TryOpenModMenu(
            IModHelper helper,
            IMonitor monitor,
            string? modId)
        {
            try
            {
                IManifest? manifest = GetManifest(helper, modId);
                if (manifest == null)
                    return false;

                IGenericModConfigMenuApi? api =
                    helper.ModRegistry.GetApi<IGenericModConfigMenuApi>(
                        GenericModConfigMenuId);

                if (api == null)
                    return false;

                api.OpenModMenuAsChildMenu(
                    manifest);
                return true;
            }
            catch (Exception ex)
            {
                monitor.Log(
                    $"Failed to open GMCM for '{modId}'.\n{ex}",
                    LogLevel.Warn);
                return false;
            }
        }


        /// <summary>
        /// GMCMのMod一覧画面を開きます。
        /// 通常の「Mod Options」ボタンと同じOpenListMenuNewを呼び出します。
        /// </summary>
        internal static bool TryOpenListMenu(
            IModHelper helper,
            IMonitor monitor)
        {
            try
            {
                IGenericModConfigMenuApi? api =
                    helper.ModRegistry.GetApi<IGenericModConfigMenuApi>(
                        GenericModConfigMenuId);

                if (api == null)
                    return false;

                // The object returned by GetApi is GMCM's API object (or SMAPI's
                // proxy for it), not the GenericModConfigMenu.Mod instance.
                // OpenListMenuNew belongs to GenericModConfigMenu.Mod, so obtain
                // that singleton from the same assembly before invoking it.
                object gmcmApi = GetActualApiObject(api);
                Assembly gmcmAssembly = gmcmApi.GetType().Assembly;

                Type? modType =
                    gmcmAssembly.GetType("GenericModConfigMenu.Mod");

                if (modType == null)
                {
                    LogReflectionFailure(
                        monitor,
                        "Type 'GenericModConfigMenu.Mod' was not found.");
                    return false;
                }

                object? gmcmMod = GetStaticModInstance(modType);
                if (gmcmMod == null)
                {
                    LogReflectionFailure(
                        monitor,
                        "The GenericModConfigMenu.Mod instance was not found.");
                    return false;
                }

                MethodInfo? openListMenuMethod =
                    modType.GetMethod(
                        OpenListMenuNewMethodName,
                        BindingFlags.Instance |
                        BindingFlags.Public |
                        BindingFlags.NonPublic,
                        binder: null,
                        types: new[] { typeof(int?) },
                        modifiers: null);

                if (openListMenuMethod == null)
                {
                    LogReflectionFailure(
                        monitor,
                        $"Method '{OpenListMenuNewMethodName}' was not found.");
                    return false;
                }

                openListMenuMethod.Invoke(
                    gmcmMod,
                    new object?[] { null });

                return true;
            }
            catch (Exception ex)
            {
                monitor.Log(
                    "Failed to open the GMCM mod list for the Shortcut Panel.\n" + ex,
                    LogLevel.Warn);
                return false;
            }
        }

        /// <summary>
        /// GMCM本体のstaticフィールド/プロパティからModインスタンスを取得します。
        /// フィールド名には依存せず、GenericModConfigMenu.Mod型そのものを探します。
        /// </summary>
        private static object? GetStaticModInstance(Type modType)
        {
            const BindingFlags flags =
                BindingFlags.Static |
                BindingFlags.Public |
                BindingFlags.NonPublic;

            foreach (FieldInfo field in modType.GetFields(flags))
            {
                if (!modType.IsAssignableFrom(field.FieldType))
                    continue;

                object? value = field.GetValue(null);
                if (value != null)
                    return value;
            }

            foreach (PropertyInfo property in modType.GetProperties(flags))
            {
                if (!property.CanRead
                    || !modType.IsAssignableFrom(property.PropertyType)
                    || property.GetIndexParameters().Length != 0)
                {
                    continue;
                }

                object? value = property.GetValue(null);
                if (value != null)
                    return value;
            }

            return null;
        }

        /// <summary>
        /// SMAPIから取得したGMCM APIのラッパー内部から、実際のGMCMオブジェクトを取得します。
        /// </summary>
        private static object GetActualApiObject(
            IGenericModConfigMenuApi api)
        {
            Type apiType = api.GetType();

            FieldInfo? directConfigManager =
                apiType.GetField(
                    ConfigManagerFieldName,
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic);

            if (directConfigManager != null)
                return api;

            FieldInfo? targetField =
                apiType.GetField(
                    ProxyTargetFieldName,
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic);

            if (targetField == null)
            {
                throw new InvalidOperationException(
                    $"Neither '{ConfigManagerFieldName}' nor " +
                    $"'{ProxyTargetFieldName}' was found on the GMCM API object.");
            }

            object? target = targetField.GetValue(api);
            if (target == null)
                throw new InvalidOperationException("The GMCM API proxy target was null.");

            return target;
        }

        /// <summary>
        /// GMCM内部構造のReflection取得に失敗した場合、原因確認用のログを出力します。
        /// </summary>
        private static void LogReflectionFailure(
            IMonitor monitor,
            string reason)
        {
            monitor.Log(
                "Failed to retrieve GMCM mods for the Shortcut Panel. " +
                "The GMCM internal structure may have changed. " + reason,
                LogLevel.Warn);
        }
    }
}
