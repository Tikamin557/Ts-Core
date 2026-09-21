using Microsoft.Xna.Framework;
using StardewValley;

namespace Ts_Core.Services.DialogueRelated
{
    /// <summary>
    /// TsCore Dialogueを
    /// TouchAction / TileActionから開始します。
    /// </summary>
    public static class DialogueAction
    {
        //----------------------------------------
        // TouchAction
        //----------------------------------------

        /// <summary>
        /// TouchActionからDialogueを開始します。
        /// </summary>
        public static void HandleTouchAction(
            GameLocation location,
            string[] action,
            Farmer who,
            Vector2 playerStandingPosition)
        {
            StartDialogue(
                location,
                action,
                who);
        }

        //----------------------------------------
        // TileAction
        //----------------------------------------

        /// <summary>
        /// TileActionからDialogueを開始します。
        /// </summary>
        public static bool HandleTileAction(
            GameLocation location,
            string[] action,
            Farmer who,
            Point tile)
        {
            return StartDialogue(
                location,
                action,
                who);
        }

        //----------------------------------------
        // Dialogue開始
        //----------------------------------------

        /// <summary>
        /// Action引数からDialogue IDを取得して
        /// Dialogueを開始します。
        /// </summary>
        private static bool StartDialogue(
            GameLocation location,
            string[] action,
            Farmer who)
        {
            //----------------------------------------
            // 引数確認
            //----------------------------------------

            // TsCoreDialogue <DialogueId>
            if (action.Length < 2)
                return false;

            string dialogueId =
                action[1];

            if (string.IsNullOrWhiteSpace(
                    dialogueId))
            {
                return false;
            }

            //----------------------------------------
            // Dialogue開始
            //----------------------------------------

            return DialogueService.Show(
                dialogueId,
                location,
                who);
        }
    }
}