using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TokenizableStrings;
using Ts_Core.Models.BigCraftableRelated;

namespace Ts_Core.Services.BigCraftableRelated
{
    /// <summary>
    /// BigCraftable Extensionの
    /// 各種機能を管理するサービスです。
    /// </summary>
    public static class BigCraftableExtensionService
    {
        //----------------------------------------
        // Interact
        //----------------------------------------

        /// <summary>
        /// BigCraftableに設定された
        /// BigCraftable ExtensionのTileActionを実行します。
        /// </summary>
        /// <param name="machine">
        /// 操作されたBigCraftableです。
        /// </param>
        /// <param name="location">
        /// BigCraftableが存在する場所です。
        /// </param>
        /// <param name="player">
        /// BigCraftableを操作したプレイヤーです。
        /// </param>
        /// <returns>
        /// BigCraftableの操作を処理した場合はtrue、
        /// それ以外はfalseを返します。
        /// </returns>
        public static bool Interact(
            StardewValley.Object machine,
            GameLocation location,
            Farmer player,
            bool justCheckingForActivity = false)
        {
            //----------------------------------------
            // Extension Data取得
            //----------------------------------------

            if (!BigCraftableExtensionDataService.TryGetExtensionData(
                machine,
                out string extensionId,
                out BigCraftableExtensionData extension))
            {
                return false;
            }

            //----------------------------------------
            // TileAction取得
            //----------------------------------------

            string? action =
                extension.TileAction;

            if (string.IsNullOrWhiteSpace(
                action))
            {
                return false;
            }

            //----------------------------------------
            // Activity Check
            //----------------------------------------

            if (justCheckingForActivity)
            {
                return true;
            }

            //----------------------------------------
            // Condition
            //----------------------------------------

            if (!string.IsNullOrWhiteSpace(
                extension.Condition))
            {
                if (!GameStateQuery.CheckConditions(
                    extension.Condition,
                    location,
                    player,
                    null,
                    player.ActiveObject))
                {
                    ShowMessage(
                        extension.InvalidConditionMessage,
                        player);

                    player.ignoreItemConsumptionThisFrame =
                        true;

                    return true;
                }
            }

            //----------------------------------------
            // Required Item
            //----------------------------------------

            string? requiredItem =
                null;

            int requiredCount =
                1;

            bool consumeRequiredItem =
                extension.ConsumeRequiredItem;

            StardewValley.Object? heldItem =
                null;

            if (!string.IsNullOrWhiteSpace(
                extension.RequiredItem))
            {
                requiredItem =
                    ItemRegistry.QualifyItemId(
                        extension.RequiredItem);

                //----------------------------------------
                // Invalid Item ID
                //----------------------------------------

                if (requiredItem == null)
                {
                    ShowMessage(
                        extension.InvalidItemMessage,
                        player);

                    player.ignoreItemConsumptionThisFrame =
                        true;

                    return true;
                }

                //----------------------------------------
                // Required Count
                //----------------------------------------

                if (extension.RequiredItemCount > 0)
                {
                    requiredCount =
                        extension.RequiredItemCount;
                }

                heldItem =
                    player.ActiveObject;

                //----------------------------------------
                // Item Not Found
                //----------------------------------------

                if (heldItem == null
                    || heldItem.QualifiedItemId != requiredItem)
                {
                    ShowMessage(
                        extension.InvalidItemMessage,
                        player);

                    player.ignoreItemConsumptionThisFrame =
                        true;

                    return true;
                }

                //----------------------------------------
                // Insufficient Count
                //----------------------------------------

                if (heldItem.Stack < requiredCount)
                {
                    ShowCountMessage(
                        extension.InvalidCountMessage,
                        requiredCount,
                        player);

                    player.ignoreItemConsumptionThisFrame =
                        true;

                    return true;
                }
            }

            //----------------------------------------
            // TileAction実行
            //----------------------------------------

            Vector2 tile =
                machine.TileLocation;

            xTile.Dimensions.Location tileLocation =
                new(
                    (int)tile.X,
                    (int)tile.Y);

            bool actionSucceeded =
                location.performAction(
                    action,
                    player,
                    tileLocation);

            if (!actionSucceeded)
                return false;

            //----------------------------------------
            // Action Effects
            //----------------------------------------

            MachineActionEffectService.Play(
                machine,
                location,
                extension);

            //----------------------------------------
            // Action Texture / Animation
            //----------------------------------------

            MachineAnimationTextureService.StartAction(
                machine,
                extensionId,
                extension);

            //----------------------------------------
            // Consume Required Item
            //----------------------------------------

            if (heldItem != null
                && consumeRequiredItem)
            {
                heldItem.Stack -=
                    requiredCount;

                if (heldItem.Stack <= 0)
                {
                    player.removeItemFromInventory(
                        heldItem);

                    player.showNotCarrying();
                }
            }

            return true;
        }

        //----------------------------------------
        // Message
        //----------------------------------------

        /// <summary>
        /// Extension Dataに設定された
        /// メッセージを表示します。
        /// </summary>
        /// <param name="message">
        /// 表示するメッセージです。
        /// </param>
        /// <param name="player">
        /// Machineを操作したプレイヤーです。
        /// </param>
        private static void ShowMessage(
            string? message,
            Farmer player)
        {
            if (string.IsNullOrWhiteSpace(
                message))
            {
                return;
            }

            string parsedMessage =
                TokenParser.ParseText(
                    message,
                    null,
                    null,
                    player);

            Game1.showRedMessage(
                parsedMessage);
        }

        //----------------------------------------
        // Count Message
        //----------------------------------------

        /// <summary>
        /// Extension Dataに設定された
        /// 個数メッセージを表示します。
        /// </summary>
        /// <param name="message">
        /// 表示するメッセージです。
        /// </param>
        /// <param name="requiredCount">
        /// 必要なアイテム数です。
        /// </param>
        /// <param name="player">
        /// Machineを操作したプレイヤーです。
        /// </param>
        private static void ShowCountMessage(
            string? message,
            int requiredCount,
            Farmer player)
        {
            if (string.IsNullOrWhiteSpace(
                message))
            {
                return;
            }

            bool ParseItemCount(
                string[] query,
                out string replacement,
                Random random,
                Farmer tokenPlayer)
            {
                if (query.Length > 0
                    && query[0] == "ItemCount")
                {
                    replacement =
                        requiredCount.ToString();

                    return true;
                }

                replacement =
                    null!;

                return false;
            }

            string parsedMessage =
                TokenParser.ParseText(
                    message,
                    null,
                    ParseItemCount,
                    player);

            Game1.showRedMessage(
                parsedMessage);
        }
    }
}