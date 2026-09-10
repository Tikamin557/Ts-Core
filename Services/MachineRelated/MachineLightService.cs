using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.GameData.Machines;
using System.Globalization;
using Ts_Core.Models.MachineRelated;

namespace Ts_Core.Services.MachineRelated
{
    /// <summary>
    /// Machine Interactionの
    /// 通常Lightを管理するサービスです。
    /// </summary>
    public static class MachineLightService
    {
        //----------------------------------------
        // Active Lights
        //----------------------------------------

        /// <summary>
        /// TsCoreが現在表示している
        /// Machine Lightを管理します。
        /// </summary>
        private static readonly Dictionary<
            string,
            GameLocation> ActiveLights =
                new();

        //----------------------------------------
        // Update Ticked
        //----------------------------------------

        /// <summary>
        /// Machine Lightの状態を更新します。
        /// </summary>
        public static void OnUpdateTicked(
            object? sender,
            UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            if (!e.IsMultipleOf(10))
                return;

            GameLocation? location =
                Game1.currentLocation;

            if (location == null)
                return;

            //----------------------------------------
            // Action Light Cleanup
            //----------------------------------------

            MachineActionEffectService
                .CleanupActionLights(
                    location);

            //----------------------------------------
            // Machine Light Update
            //----------------------------------------

            UpdateLights(
                location);
        }

        //----------------------------------------
        // Update Lights
        //----------------------------------------

        /// <summary>
        /// 現在LocationのMachine Lightを更新します。
        /// </summary>
        private static void UpdateLights(
            GameLocation location)
        {
            Dictionary<string, MachineInteractionData>
                interactionData =
                    Game1.content.Load<
                        Dictionary<
                            string,
                            MachineInteractionData>>(
                                MachineInteractionDataService.AssetName);

            HashSet<string> activeLightIds =
                new();

            //----------------------------------------
            // Machine確認
            //----------------------------------------

            foreach (StardewValley.Object machine
                in location.Objects.Values)
            {
                MachineData? machineData =
                    machine.GetMachineData();

                if (machineData?.CustomFields == null)
                    continue;

                //----------------------------------------
                // Interaction ID取得
                //----------------------------------------

                if (!machineData.CustomFields.TryGetValue(
                    MachineInteractionDataService.InteractionField,
                    out string? interactionId))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(
                    interactionId))
                {
                    continue;
                }

                //----------------------------------------
                // Interaction Data取得
                //----------------------------------------

                if (!interactionData.TryGetValue(
                    interactionId,
                    out MachineInteractionData? interaction))
                {
                    continue;
                }

                //----------------------------------------
                // 通常Light確認
                //----------------------------------------

                if (!interaction.Light)
                    continue;

                //----------------------------------------
                // Action Light優先
                //----------------------------------------

                if (MachineActionEffectService
                    .IsActionLightActive(
                        location,
                        machine))
                {
                    continue;
                }

                //----------------------------------------
                // vanilla LightWhileWorking優先
                //----------------------------------------

                if (machineData.LightWhileWorking != null
                    && machine.minutesUntilReady.Value > 0
                    && !machine.readyForHarvest.Value)
                {
                    continue;
                }

                //----------------------------------------
                // 通常Light
                //----------------------------------------

                string lightId =
                    GetLightId(
                        location,
                        machine);

                if (!UpdateLight(
                    machine,
                    location,
                    interaction,
                    lightId))
                {
                    continue;
                }

                activeLightIds.Add(
                    lightId);
            }

            //----------------------------------------
            // 不要になったLight削除
            //----------------------------------------

            List<string> removeIds =
                new();

            foreach (KeyValuePair<
                string,
                GameLocation> entry
                in ActiveLights)
            {
                if (entry.Value == location
                    && activeLightIds.Contains(
                        entry.Key))
                {
                    continue;
                }

                entry.Value.removeLightSource(
                    entry.Key);

                removeIds.Add(
                    entry.Key);
            }

            foreach (string lightId
                in removeIds)
            {
                ActiveLights.Remove(
                    lightId);
            }
        }

        //----------------------------------------
        // Update Light
        //----------------------------------------

        /// <summary>
        /// 指定Machineの通常Lightを
        /// 追加または更新します。
        /// </summary>
        /// <returns>
        /// Lightを正常に追加または更新した場合はtrue、
        /// それ以外はfalseを返します。
        /// </returns>
        private static bool UpdateLight(
            StardewValley.Object machine,
            GameLocation location,
            MachineInteractionData interaction,
            string lightId)
        {
            //----------------------------------------
            // Light Offset
            //----------------------------------------

            if (!TryGetLightOffset(
                interaction.LightOffset,
                out Vector2 offset))
            {
                return false;
            }

            //----------------------------------------
            // Light座標
            //----------------------------------------

            Vector2 position =
                new(
                    (machine.TileLocation.X
                        + offset.X)
                        * 64f
                        + 32f,

                    (machine.TileLocation.Y
                        + offset.Y)
                        * 64f
                        + 32f);

            //----------------------------------------
            // 既存Light更新
            //----------------------------------------

            if (location.hasLightSource(
                lightId))
            {
                location.repositionLightSource(
                    lightId,
                    position);

                ActiveLights[lightId] =
                    location;

                return true;
            }

            //----------------------------------------
            // Light生成
            //----------------------------------------

            Color color =
                Utility.StringToColor(
                    interaction.LightColor)
                ?? Color.White;

            LightSource source =
                new(
                    lightId,
                    LightSource.sconceLight,
                    position,
                    interaction.LightRadius,
                    color,
                    LightSource.LightContext.None,
                    0L,
                    location.NameOrUniqueName);

            location.sharedLights.AddLight(
                source);

            ActiveLights[lightId] =
                location;

            return true;
        }

        //----------------------------------------
        // Light Offset
        //----------------------------------------

        /// <summary>
        /// Light Offsetを取得します。
        /// "X, Y"形式のタイル座標で指定します。
        /// </summary>
        private static bool TryGetLightOffset(
            string? value,
            out Vector2 offset)
        {
            offset =
                Vector2.Zero;

            if (string.IsNullOrWhiteSpace(
                value))
            {
                return true;
            }

            string[] parts =
                value.Split(
                    ',');

            if (parts.Length != 2)
                return false;

            if (!float.TryParse(
                parts[0].Trim(),
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out float x))
            {
                return false;
            }

            if (!float.TryParse(
                parts[1].Trim(),
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out float y))
            {
                return false;
            }

            offset =
                new Vector2(
                    x,
                    y);

            return true;
        }

        //----------------------------------------
        // Light ID
        //----------------------------------------

        /// <summary>
        /// 通常Light用のLight Source IDを生成します。
        /// </summary>
        private static string GetLightId(
            GameLocation location,
            StardewValley.Object machine)
        {
            return
                $"TsCore.MachineLight."
                + $"{location.NameOrUniqueName}."
                + $"{(int)machine.TileLocation.X}."
                + $"{(int)machine.TileLocation.Y}";
        }

        //----------------------------------------
        // Clear
        //----------------------------------------

        /// <summary>
        /// Machine Lightと管理情報を
        /// 全て削除します。
        /// </summary>
        public static void Clear()
        {
            foreach (KeyValuePair<
                string,
                GameLocation> entry
                in ActiveLights)
            {
                if (entry.Value.hasLightSource(
                    entry.Key))
                {
                    entry.Value.removeLightSource(
                        entry.Key);
                }
            }

            ActiveLights.Clear();
        }
    }
}