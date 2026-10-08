using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using System.Reflection;

namespace Ts_Core.Services.ShortcutPanelRelated
{
    /// <summary>
    /// CJB Cheats Menu が実際に読み込んでいるワープ一覧を取得し、
    /// Shortcut Panel から選択・実行できる形へ変換します。
    /// CJB側に公開APIがないため、一覧取得のみ必要最小限のReflectionを使用します。
    /// </summary>
    internal static class CjbCheatsMenuWarpService
    {
        private const string CjbModId = "CJBok.CheatsMenu";
        internal const string ShortcutIdPrefix = "TsCore/CjbWarp/";

        /// <summary>CJB Cheats Menu が導入されているか確認します。</summary>
        internal static bool IsInstalled(IModHelper helper)
        {
            return helper.ModRegistry.IsLoaded(CjbModId);
        }

        /// <summary>
        /// CJB Cheats Menu の現在のワープ一覧を取得します。
        /// HideWarps / AddWarps / Content Patcher編集を含む、CJB自身が生成した一覧を使用します。
        /// </summary>
        internal static IReadOnlyList<CjbWarpEntry> GetWarps(
            IModHelper helper,
            IMonitor monitor,
            bool visibleOnly = true)
        {
            if (!IsInstalled(helper))
                return Array.Empty<CjbWarpEntry>();

            try
            {
                IModInfo? modInfo = helper.ModRegistry.Get(CjbModId);
                if (modInfo == null)
                    return Array.Empty<CjbWarpEntry>();

                object? cjbMod = GetMemberValue(modInfo, "Mod");
                if (cjbMod == null)
                {
                    monitor.Log(
                        "Could not access the CJB Cheats Menu mod instance. CJB warp shortcuts will be unavailable.",
                        LogLevel.Warn);
                    return Array.Empty<CjbWarpEntry>();
                }

                object? loader = GetMemberValue(cjbMod, "WarpContentLoader");
                if (loader == null)
                {
                    monitor.Log(
                        "Could not access CJB Cheats Menu's WarpContentLoader. Its internal implementation may have changed.",
                        LogLevel.Warn);
                    return Array.Empty<CjbWarpEntry>();
                }

                object? rawSections = InvokeNoArgumentMethod(loader, "LoadWarpSections");
                object? rawWarps = InvokeNoArgumentMethod(loader, "LoadWarps");

                Dictionary<string, string> sectionNames =
                    ReadSectionNames(rawSections);

                List<CjbWarpEntry> result = new();

                if (rawWarps is not System.Collections.IEnumerable warps)
                    return result;

                foreach (object? rawWarp in warps)
                {
                    if (rawWarp == null)
                        continue;

                    string id = GetStringMember(rawWarp, "Id");
                    string sectionId = GetStringMember(rawWarp, "SectionId");
                    string displayName = GetStringMember(rawWarp, "DisplayName");
                    string location = GetStringMember(rawWarp, "Location");
                    string? condition = GetNullableStringMember(rawWarp, "Condition");
                    Vector2 tile = GetVector2Member(rawWarp, "Tile");

                    if (string.IsNullOrWhiteSpace(id)
                        || string.IsNullOrWhiteSpace(displayName)
                        || string.IsNullOrWhiteSpace(location))
                    {
                        continue;
                    }

                    // 選択一覧ではCJBのWarpタブと同じく、
                    // 現在条件を満たさない行き先を表示しません。
                    if (visibleOnly
                        && !GameStateQuery.CheckConditions(condition))
                    {
                        continue;
                    }

                    sectionNames.TryGetValue(
                        sectionId,
                        out string? sectionName);

                    result.Add(
                        new CjbWarpEntry(
                            id,
                            sectionId,
                            sectionName ?? sectionId,
                            displayName,
                            location,
                            tile,
                            condition));
                }

                return result;
            }
            catch (Exception ex)
            {
                monitor.Log(
                    $"Failed to read CJB Cheats Menu warp data. CJB warp shortcuts will be unavailable.\n{ex}",
                    LogLevel.Warn);
                return Array.Empty<CjbWarpEntry>();
            }
        }

        /// <summary>
        /// 現在のCJBワープをShortcut Registryへ登録します。
        /// 保存済みスロットはワープIDを含むShortcut IDから復元できます。
        /// </summary>
        internal static void RegisterWarpShortcuts(
            IModHelper helper,
            IMonitor monitor)
        {
            foreach (CjbWarpEntry warp in GetWarps(
                         helper,
                         monitor,
                         visibleOnly: false))
            {
                string shortcutId = GetShortcutId(warp.Id);
                string warpId = warp.Id;

                ShortcutPanelRegistry.Register(
                    new ShortcutPanelEntry(
                        shortcutId,
                        warp.DisplayName,
                        () => helper.ModContent.Load<Microsoft.Xna.Framework.Graphics.Texture2D>(
                            "assets/ShortcutPanel/CjbCheatsMenu.png"),
                        iconSourceRect: null,
                        action: () => ExecuteWarp(helper, monitor, warpId),
                        isAvailable: () =>
                            IsInstalled(helper)
                            && FindWarp(helper, monitor, warpId) != null,
                        unavailableAction: null,
                        unavailableHoverTextProvider: () =>
                            helper.Translation.Get(
                                "shortcutPanel.CjbWarp.unavailable")));
            }
        }

        /// <summary>指定ワープを現在のCJBデータから再取得して実行します。</summary>
        internal static void ExecuteWarp(
            IModHelper helper,
            IMonitor monitor,
            string warpId)
        {
            CjbWarpEntry? warp =
                FindWarp(helper, monitor, warpId);

            if (warp == null)
                return;

            Game1.exitActiveMenu();
            Game1.player.swimming.Value = false;
            Game1.player.changeOutOfSwimSuit();

            // CJBと同じく、Farmの(0,0)は各プレイヤーの農場入口を使用します。
            if (warp.Location == "Farm"
                && warp.Tile == Vector2.Zero)
            {
                WarpToFarm();
                return;
            }

            Game1.warpFarmer(
                warp.Location,
                (int)warp.Tile.X,
                (int)warp.Tile.Y,
                false);
        }

        /// <summary>ワープIDから保存用Shortcut IDを作成します。</summary>
        internal static string GetShortcutId(string warpId)
        {
            return ShortcutIdPrefix + warpId;
        }

        private static CjbWarpEntry? FindWarp(
            IModHelper helper,
            IMonitor monitor,
            string warpId)
        {
            return GetWarps(helper, monitor)
                .FirstOrDefault(
                    warp => string.Equals(
                        warp.Id,
                        warpId,
                        StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>CJBのFarmワープと同じ農場入口処理です。</summary>
        private static void WarpToFarm()
        {
            string cabinName =
                Game1.player.homeLocation.Value;

            if (!Context.IsMainPlayer
                && cabinName != null)
            {
                foreach (GameLocation location
                         in Game1.locations)
                {
                    foreach (Building building
                             in location.buildings)
                    {
                        if (building.indoors.Value?.uniqueName.Value
                            != cabinName)
                        {
                            continue;
                        }

                        int tileX =
                            building.tileX.Value
                            + building.humanDoor.X;

                        int tileY =
                            building.tileY.Value
                            + building.humanDoor.Y
                            + 1;

                        Game1.warpFarmer(
                            location.Name,
                            tileX,
                            tileY,
                            false);
                        return;
                    }
                }
            }

            Point farmhousePos =
                Game1.getFarm().GetMainFarmHouseEntry();

            Game1.warpFarmer(
                "Farm",
                farmhousePos.X,
                farmhousePos.Y,
                false);
        }

        private static Dictionary<string, string> ReadSectionNames(
            object? rawSections)
        {
            Dictionary<string, string> result =
                new(StringComparer.OrdinalIgnoreCase);

            if (rawSections is not System.Collections.IEnumerable sections)
                return result;

            foreach (object? rawSection in sections)
            {
                if (rawSection == null)
                    continue;

                string id = GetStringMember(rawSection, "Id");
                string displayName = GetStringMember(rawSection, "DisplayName");

                if (!string.IsNullOrWhiteSpace(id))
                    result[id] = displayName;
            }

            return result;
        }

        private static object? InvokeNoArgumentMethod(
            object instance,
            string methodName)
        {
            MethodInfo? method =
                instance.GetType().GetMethod(
                    methodName,
                    BindingFlags.Instance
                    | BindingFlags.Public
                    | BindingFlags.NonPublic,
                    binder: null,
                    types: Type.EmptyTypes,
                    modifiers: null);

            return method?.Invoke(
                instance,
                null);
        }

        private static object? GetMemberValue(
            object instance,
            string memberName)
        {
            Type type = instance.GetType();

            PropertyInfo? property =
                type.GetProperty(
                    memberName,
                    BindingFlags.Instance
                    | BindingFlags.Public
                    | BindingFlags.NonPublic);

            if (property != null)
                return property.GetValue(instance);

            FieldInfo? field =
                type.GetField(
                    memberName,
                    BindingFlags.Instance
                    | BindingFlags.Public
                    | BindingFlags.NonPublic);

            return field?.GetValue(instance);
        }

        private static string GetStringMember(
            object instance,
            string memberName)
        {
            return GetMemberValue(instance, memberName)
                as string
                ?? string.Empty;
        }

        private static string? GetNullableStringMember(
            object instance,
            string memberName)
        {
            return GetMemberValue(instance, memberName)
                as string;
        }

        private static Vector2 GetVector2Member(
            object instance,
            string memberName)
        {
            return GetMemberValue(instance, memberName)
                is Vector2 value
                    ? value
                    : Vector2.Zero;
        }
    }

    /// <summary>CJB Cheats Menuのワープ1件分を保持します。</summary>
    internal sealed record CjbWarpEntry(
        string Id,
        string SectionId,
        string SectionName,
        string DisplayName,
        string Location,
        Vector2 Tile,
        string? Condition);
}
