using System;
using System.Collections.Generic;

namespace Ts_Core.Services.ShortcutPanelRelated
{
    /// <summary>
    /// Shortcut Panelで使用できる
    /// ショートカットを管理します。
    /// </summary>
    internal static class ShortcutPanelRegistry
    {
        //----------------------------------------
        // 登録データ
        //----------------------------------------

        private static readonly Dictionary<string, ShortcutPanelEntry>
            entries =
                new(
                    StringComparer.OrdinalIgnoreCase);

        //----------------------------------------
        // Register
        //----------------------------------------

        /// <summary>
        /// ショートカットを登録します。
        /// </summary>
        internal static void Register(
            ShortcutPanelEntry entry)
        {
            if (string.IsNullOrWhiteSpace(
                entry.Id))
            {
                throw new ArgumentException(
                    "Shortcut ID must not be empty.",
                    nameof(entry));
            }

            entries[entry.Id] =
                entry;
        }

        //----------------------------------------
        // Get
        //----------------------------------------

        /// <summary>
        /// 指定したIDのショートカットを取得します。
        /// </summary>
        internal static ShortcutPanelEntry? Get(
            string id)
        {
            if (string.IsNullOrWhiteSpace(
                id))
            {
                return null;
            }

            entries.TryGetValue(
                id,
                out ShortcutPanelEntry? entry);

            return entry;
        }

        //----------------------------------------
        // GetAll
        //----------------------------------------

        /// <summary>
        /// 登録されているすべての
        /// ショートカットを取得します。
        /// </summary>
        internal static IReadOnlyCollection<ShortcutPanelEntry>
            GetAll()
        {
            return entries.Values;
        }

        //----------------------------------------
        // Contains
        //----------------------------------------

        /// <summary>
        /// 指定したIDのショートカットが
        /// 登録されているか確認します。
        /// </summary>
        internal static bool Contains(
            string id)
        {
            if (string.IsNullOrWhiteSpace(
                id))
            {
                return false;
            }

            return entries.ContainsKey(
                id);
        }

        //----------------------------------------
        // Clear
        //----------------------------------------

        /// <summary>
        /// 登録されているショートカットを
        /// すべて解除します。
        /// </summary>
        internal static void Clear()
        {
            entries.Clear();
        }
    }
}