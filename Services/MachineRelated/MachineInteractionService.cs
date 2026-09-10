using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.GameData.Machines;
using StardewValley.TokenizableStrings;
using Ts_Core.Models.MachineRelated;

namespace Ts_Core.Services.MachineRelated
{
    /// <summary>
    /// Machineの操作時に設定された
    /// Machine Interactionを実行するサービスです。
    /// </summary>
    public static class MachineInteractionService
    {
        //----------------------------------------
        // Interact
        //----------------------------------------

        /// <summary>
        /// Machineに設定されたInteractionを実行します。
        /// </summary>
        /// <param name="machine">
        /// 操作されたMachineです。
        /// </param>
        /// <param name="location">
        /// Machineが存在する場所です。
        /// </param>
        /// <param name="player">
        /// Machineを操作したプレイヤーです。
        /// </param>
        /// <returns>
        /// Machineの操作を処理した場合はtrue、
        /// それ以外はfalseを返します。
        /// </returns>
        public static bool Interact(
            StardewValley.Object machine,
            GameLocation location,
            Farmer player)
        {
            MachineData? machineData =
                machine.GetMachineData();

            if (machineData?.CustomFields == null)
                return false;

            //----------------------------------------
            // Interaction ID取得
            //----------------------------------------

            if (!machineData.CustomFields.TryGetValue(
                MachineInteractionDataService.InteractionField,
                out string? interactionId))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(
                interactionId))
            {
                return false;
            }

            //----------------------------------------
            // Interaction Data取得
            //----------------------------------------

            Dictionary<string, MachineInteractionData>
                interactionData =
                    Game1.content.Load<
                        Dictionary<
                            string,
                            MachineInteractionData>>(
                                MachineInteractionDataService.AssetName);

            if (!interactionData.TryGetValue(
                interactionId,
                out MachineInteractionData? interaction))
            {
                return false;
            }

            //----------------------------------------
            // TileAction取得
            //----------------------------------------

            string? action =
                interaction.TileAction;

            if (string.IsNullOrWhiteSpace(
                action))
            {
                return false;
            }

            //----------------------------------------
            // Condition
            //----------------------------------------

            if (!string.IsNullOrWhiteSpace(
                interaction.Condition))
            {
                if (!GameStateQuery.CheckConditions(
                    interaction.Condition,
                    location,
                    player,
                    null,
                    player.ActiveObject))
                {
                    ShowMessage(
                        interaction.InvalidConditionMessage,
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
                interaction.ConsumeRequiredItem;

            StardewValley.Object? heldItem =
                null;

            if (!string.IsNullOrWhiteSpace(
                interaction.RequiredItem))
            {
                requiredItem =
                    ItemRegistry.QualifyItemId(
                        interaction.RequiredItem);

                //----------------------------------------
                // Invalid Item ID
                //----------------------------------------

                if (requiredItem == null)
                {
                    ShowMessage(
                        interaction.InvalidItemMessage,
                        player);

                    player.ignoreItemConsumptionThisFrame =
                        true;

                    return true;
                }

                //----------------------------------------
                // Required Count
                //----------------------------------------

                if (interaction.RequiredItemCount > 0)
                {
                    requiredCount =
                        interaction.RequiredItemCount;
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
                        interaction.InvalidItemMessage,
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
                        interaction.InvalidCountMessage,
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
                interaction);

            //----------------------------------------
            // Action Texture / Animation
            //----------------------------------------

            MachineAnimationTextureService.StartAction(
                machine,
                interactionId,
                interaction);

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
        /// Interaction Dataに設定された
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
        /// Interaction Dataに設定された
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