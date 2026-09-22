using System;
using Microsoft.Xna.Framework.Graphics;

namespace Ts_Core.Interfaces
{
    /// <summary>
    /// T's Coreが外部Modへ公開するAPIです。
    /// </summary>
    public interface ITsCoreApi
    {
        /// <summary>
        /// Shortcut Panelへ
        /// Mod機能を登録します。
        /// </summary>
        /// <param name="id">
        /// Shortcutを識別する一意のID。
        /// </param>
        /// <param name="displayName">
        /// Shortcut Panelに表示する名前。
        /// </param>
        /// <param name="iconTextureProvider">
        /// ShortcutのアイコンTextureを返す処理。
        /// nullの場合はアイコンなし。
        /// </param>
        /// <param name="action">
        /// Shortcut実行時に呼び出す処理。
        /// </param>
        void RegisterShortcut(
            string id,
            string displayName,
            Func<Texture2D?>? iconTextureProvider,
            Action action);
    }
}