using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.GameData.Machines;
using System.Globalization;
using Ts_Core.Models.MachineRelated;

namespace Ts_Core.Services.MachineRelated
{
    /// <summary>
    /// Machine Interaction成功時の
    /// Action Effectsを実行・管理するサービスです。
    /// </summary>
    public static class MachineActionEffectService
    {
        //----------------------------------------
        // Action Light Generations
        //----------------------------------------

        /// <summary>
        /// Action Lightごとの世代番号です。
        /// 再発動時に古い遅延削除処理が
        /// 新しいLightを削除しないように使用します。
        /// </summary>
        private static readonly Dictionary<
            string,
            int> ActionLightGenerations =
                new();

        //----------------------------------------
        // Play
        //----------------------------------------

        /// <summary>
        /// Interaction Dataに設定された
        /// Action Effectsを実行します。
        /// </summary>
        /// <param name="machine">
        /// Effectを実行するMachineです。
        /// </param>
        /// <param name="location">
        /// Machineが存在する場所です。
        /// </param>
        /// <param name="interaction">
        /// Machine Interactionの設定データです。
        /// </param>
        public static void Play(
            StardewValley.Object machine,
            GameLocation location,
            MachineInteractionData interaction)
        {
            PlayMachineEffects(
                machine,
                interaction);

            PlayActionLight(
                machine,
                location,
                interaction);

            MachineActionWobbleService.Start(
                machine,
                interaction);
        }

        //----------------------------------------
        // Machine Effects
        //----------------------------------------

        /// <summary>
        /// ActionEffectsに設定された
        /// Machine Effectsを実行します。
        /// </summary>
        private static void PlayMachineEffects(
            StardewValley.Object machine,
            MachineInteractionData interaction)
        {
            if (interaction.ActionEffects == null
                || interaction.ActionEffects.Count == 0)
            {
                return;
            }

            //----------------------------------------
            // Chance
            //----------------------------------------

            float chance =
                Math.Clamp(
                    interaction.ActionEffectChance,
                    0f,
                    1f);

            if (Game1.random.NextDouble()
                >= chance)
            {
                return;
            }

            //----------------------------------------
            // Machine Effects
            //----------------------------------------

            foreach (MachineEffects effect
                in interaction.ActionEffects)
            {
                if (machine.PlayMachineEffect(
                    effect))
                {
                    break;
                }
            }
        }

        //----------------------------------------
        // Action Light
        //----------------------------------------

        /// <summary>
        /// Action Lightを表示します。
        /// </summary>
        private static void PlayActionLight(
            StardewValley.Object machine,
            GameLocation location,
            MachineInteractionData interaction)
        {
            if (!interaction.ActionLight)
                return;

            if (interaction.ActionLightDurationMs <= 0)
                return;

            //----------------------------------------
            // Light Offset
            //----------------------------------------

            if (!TryGetLightOffset(
                interaction.ActionLightOffset,
                out Vector2 offset))
            {
                return;
            }

            //----------------------------------------
            // Light ID
            //----------------------------------------

            string lightId =
                GetActionLightId(
                    location,
                    machine);

            //----------------------------------------
            // Generation更新
            //----------------------------------------

            int generation =
                1;

            if (ActionLightGenerations.TryGetValue(
                lightId,
                out int currentGeneration))
            {
                generation =
                    currentGeneration + 1;
            }

            ActionLightGenerations[lightId] =
                generation;

            //----------------------------------------
            // 既存Light削除
            //----------------------------------------

            if (location.hasLightSource(
                lightId))
            {
                location.removeLightSource(
                    lightId);
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
            // Light生成
            //----------------------------------------

            Color color =
                Utility.StringToColor(
                    interaction.ActionLightColor)
                ?? Color.White;

            LightSource source =
                new(
                    lightId,
                    LightSource.sconceLight,
                    position,
                    interaction.ActionLightRadius,
                    color,
                    LightSource.LightContext.None,
                    0L,
                    location.NameOrUniqueName);

            location.sharedLights.AddLight(
                source);

            //----------------------------------------
            // 指定時間後に削除
            //----------------------------------------

            DelayedAction.functionAfterDelay(
                () =>
                {
                    //----------------------------------------
                    // 再発動済みなら
                    // 古い削除処理は無視
                    //----------------------------------------

                    if (!ActionLightGenerations.TryGetValue(
                        lightId,
                        out int activeGeneration))
                    {
                        return;
                    }

                    if (activeGeneration != generation)
                        return;

                    //----------------------------------------
                    // Light削除
                    //----------------------------------------

                    location.removeLightSource(
                        lightId);

                    ActionLightGenerations.Remove(
                        lightId);
                },
                interaction.ActionLightDurationMs);
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
        // Action Light Active Check
        //----------------------------------------

        /// <summary>
        /// 指定MachineのAction Lightが
        /// 現在有効か取得します。
        /// </summary>
        public static bool IsActionLightActive(
            GameLocation location,
            StardewValley.Object machine)
        {
            string lightId =
                GetActionLightId(
                    location,
                    machine);

            return location.hasLightSource(
                lightId);
        }

        //----------------------------------------
        // Action Light ID
        //----------------------------------------

        /// <summary>
        /// Action Light用のLight Source IDを生成します。
        /// </summary>
        private static string GetActionLightId(
            GameLocation location,
            StardewValley.Object machine)
        {
            return
                $"TsCore.MachineActionLight."
                + $"{location.NameOrUniqueName}."
                + $"{(int)machine.TileLocation.X}."
                + $"{(int)machine.TileLocation.Y}";
        }

        //----------------------------------------
        // Action Light Cleanup
        //----------------------------------------

        /// <summary>
        /// すでに存在しないMachineの
        /// Action Lightを削除します。
        /// </summary>
        public static void CleanupActionLights(
            GameLocation location)
        {
            List<string> removeIds =
                new();

            string prefix =
                $"TsCore.MachineActionLight."
                + $"{location.NameOrUniqueName}.";

            //----------------------------------------
            // Action Light確認
            //----------------------------------------

            foreach (KeyValuePair<string, int> entry
                in ActionLightGenerations)
            {
                string lightId =
                    entry.Key;

                //----------------------------------------
                // このLocationのLightのみ確認
                //----------------------------------------

                if (!lightId.StartsWith(
                    prefix,
                    StringComparison.Ordinal))
                {
                    continue;
                }

                //----------------------------------------
                // 対応するMachineが存在するか確認
                //----------------------------------------

                bool machineExists =
                    false;

                foreach (StardewValley.Object machine
                    in location.Objects.Values)
                {
                    if (GetActionLightId(
                        location,
                        machine) == lightId)
                    {
                        machineExists =
                            true;

                        break;
                    }
                }

                if (machineExists)
                    continue;

                //----------------------------------------
                // Machineが存在しないのでLight削除
                //----------------------------------------

                if (location.hasLightSource(
                    lightId))
                {
                    location.removeLightSource(
                        lightId);
                }

                removeIds.Add(
                    lightId);
            }

            //----------------------------------------
            // 管理情報削除
            //----------------------------------------

            foreach (string lightId
                in removeIds)
            {
                ActionLightGenerations.Remove(
                    lightId);
            }
        }

        //----------------------------------------
        // Clear
        //----------------------------------------

        /// <summary>
        /// Action Lightの管理情報を
        /// 全て削除します。
        /// </summary>
        public static void Clear()
        {
            ActionLightGenerations.Clear();
        }
    }
}