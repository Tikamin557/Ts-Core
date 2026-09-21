using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Ts_Core.Services.ShortcutPanelRelated
{
    /// <summary>
    /// Shortcut Panelに登録できる
    /// 1つのショートカットを表します。
    /// </summary>
    internal sealed class ShortcutPanelEntry
    {
        //----------------------------------------
        // ID
        //----------------------------------------

        /// <summary>
        /// ショートカットを識別するID。
        /// </summary>
        internal string Id { get; }

        //----------------------------------------
        // Display Name
        //----------------------------------------

        /// <summary>
        /// 画面に表示する名前。
        /// </summary>
        internal string DisplayName { get; }

        //----------------------------------------
        // Icon
        //----------------------------------------

        /// <summary>
        /// アイコンに使用するテクスチャを
        /// 必要な時点で取得します。
        ///
        /// nullの場合はアイコンなし。
        /// </summary>
        private readonly Func<Texture2D?>?
            iconTextureProvider;

        /// <summary>
        /// IconTexture内で
        /// アイコンとして使用する範囲。
        ///
        /// nullの場合は
        /// テクスチャ全体を使用します。
        /// </summary>
        internal Rectangle? IconSourceRect { get; }

        //----------------------------------------
        // Action
        //----------------------------------------

        /// <summary>
        /// ショートカットを実行する処理。
        /// </summary>
        private readonly Action action;

        //----------------------------------------
        // Constructor
        //----------------------------------------

        internal ShortcutPanelEntry(
            string id,
            string displayName,
            Func<Texture2D?>? iconTextureProvider,
            Rectangle? iconSourceRect,
            Action action)
        {
            Id =
                id;

            DisplayName =
                displayName;

            this.iconTextureProvider =
                iconTextureProvider;

            IconSourceRect =
                iconSourceRect;

            this.action =
                action;
        }

        //----------------------------------------
        // Get Icon Texture
        //----------------------------------------

        /// <summary>
        /// 現在使用可能な
        /// アイコンテクスチャを取得します。
        /// </summary>
        internal Texture2D? GetIconTexture()
        {
            return iconTextureProvider?.Invoke();
        }

        //----------------------------------------
        // Execute
        //----------------------------------------

        /// <summary>
        /// ショートカットを実行します。
        /// </summary>
        internal void Execute()
        {
            action();
        }
    }
}