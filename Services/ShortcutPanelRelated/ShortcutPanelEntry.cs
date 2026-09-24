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

        /// <summary>
        /// 現在このショートカットを登録可能か判定します。
        /// </summary>
        private readonly Func<bool>? isAvailable;

        /// <summary>
        /// 登録できない状態で選択された時の処理です。
        /// </summary>
        private readonly Action? unavailableAction;

        /// <summary>
        /// 使用不可時にホバー表示する説明文。
        /// </summary>
        private readonly Func<string?>? unavailableHoverTextProvider;

        /// <summary>
        /// 使用不可の項目をクリックした時にキャンセル音を鳴らすか。
        /// </summary>
        internal bool PlayUnavailableSound { get; }

        //----------------------------------------
        // Constructor
        //----------------------------------------

        /// <summary>
        /// ショートカットパネルへ登録するMod機能1件分の情報と実行処理を初期化します。
        /// </summary>
        internal ShortcutPanelEntry(
            string id,
            string displayName,
            Func<Texture2D?>? iconTextureProvider,
            Rectangle? iconSourceRect,
            Action action,
            Func<bool>? isAvailable = null,
            Action? unavailableAction = null,
            Func<string?>? unavailableHoverTextProvider = null,
            bool playUnavailableSound = true)
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

            this.isAvailable =
                isAvailable;

            this.unavailableAction =
                unavailableAction;

            this.unavailableHoverTextProvider =
                unavailableHoverTextProvider;

            PlayUnavailableSound =
                playUnavailableSound;
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
        // Availability
        //----------------------------------------

        /// <summary>
        /// このMod機能が現在の環境で利用可能かを判定します。
        /// </summary>
        internal bool IsAvailable()
        {
            return isAvailable?.Invoke() ?? true;
        }

        /// <summary>
        /// このMod機能が利用できない状態で選択された時の代替処理を実行します。
        /// </summary>
        internal void ExecuteUnavailableAction()
        {
            unavailableAction?.Invoke();
        }

        /// <summary>
        /// このMod機能が利用できない理由として表示するホバーテキストを取得します。
        /// </summary>
        internal string? GetUnavailableHoverText()
        {
            return unavailableHoverTextProvider?.Invoke();
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
