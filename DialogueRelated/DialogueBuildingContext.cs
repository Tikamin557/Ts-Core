using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;

namespace Ts_Core.Services.DialogueRelated
{
    /// <summary>
    /// Building.doActionから開始されたTsCore Dialogueへ
    /// 操作元Buildingを渡すための一時コンテキストです。
    /// </summary>
    internal static class DialogueBuildingContext
    {
        private static Building? activeBuilding;

        /// <summary>
        /// 現在Building.doAction中のBuildingです。
        /// </summary>
        internal static Building? ActiveBuilding =>
            activeBuilding;

        /// <summary>
        /// Building.doActionの追跡Patchを登録します。
        /// </summary>
        internal static void Apply(
            Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(
                    typeof(Building),
                    nameof(Building.doAction),
                    new[]
                    {
                        typeof(Vector2),
                        typeof(Farmer)
                    }
                ),
                prefix: new HarmonyMethod(
                    typeof(DialogueBuildingContext),
                    nameof(BuildingDoActionPrefix)
                ),
                postfix: new HarmonyMethod(
                    typeof(DialogueBuildingContext),
                    nameof(BuildingDoActionPostfix)
                )
            );
        }

        private static void BuildingDoActionPrefix(
            Building __instance,
            ref Building? __state)
        {
            __state = activeBuilding;
            activeBuilding = __instance;
        }

        private static void BuildingDoActionPostfix(
            Building? __state)
        {
            activeBuilding = __state;
        }
    }
}
