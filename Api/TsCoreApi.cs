using Microsoft.Xna.Framework.Graphics;
using Ts_Core.Interfaces;
using Ts_Core.Services.ShortcutPanelRelated;

namespace Ts_Core.Api
{
    /// <summary>
    /// T's Coreが外部Modへ公開するAPIです。
    /// </summary>
    public sealed class TsCoreApi
        : ITsCoreApi
    {
        /// <summary>
        /// Shortcut Panelへ
        /// Mod機能を登録します。
        /// </summary>
        public void RegisterShortcut(
            string id,
            string displayName,
            Func<Texture2D?>? iconTextureProvider,
            Action action)
        {
            //----------------------------------------
            // ID
            //----------------------------------------

            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException(
                    "Shortcut ID must not be empty.",
                    nameof(id));
            }

            //----------------------------------------
            // 表示名
            //----------------------------------------

            if (string.IsNullOrWhiteSpace(
                displayName))
            {
                throw new ArgumentException(
                    "Shortcut display name must not be empty.",
                    nameof(displayName));
            }

            //----------------------------------------
            // 実行処理
            //----------------------------------------

            if (action == null)
            {
                throw new ArgumentNullException(
                    nameof(action));
            }

            //----------------------------------------
            // 登録
            //----------------------------------------

            ShortcutPanelRegistry.Register(
                new ShortcutPanelEntry(
                    id,
                    displayName,
                    iconTextureProvider,
                    null,
                    action));
        }
    }
}
