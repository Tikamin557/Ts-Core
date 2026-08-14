using HarmonyLib;
using StardewValley;
using System.Reflection;
using System.Reflection.Emit;

namespace Ts_Core.Patches
{
    /// <summary>
    /// T's Coreが登録するカスタムWarp Actionに対して、
    /// Stardew Valley本体が表示する「unknown warp property」警告を抑制します。
    /// </summary>
    internal static class WarpWarningPatch
    {
        /// <summary>
        /// Stardew Valley本体のWarp警告処理にHarmonyパッチを適用します。
        /// </summary>
        public static void Apply(Harmony harmony)
        {
            PatchMethod(
                harmony,
                nameof(GameLocation.updateDoors));

            PatchMethod(
                harmony,
                nameof(GameLocation.getWarpFromDoor));

            PatchMethod(
                harmony,
                nameof(GameLocation.getWarpPointTarget));
        }

        /// <summary>
        /// 指定したメソッド内のWarp警告処理を差し替えます。
        /// </summary>
        private static void PatchMethod(
            Harmony harmony,
            string methodName)
        {
            MethodInfo? method = AccessTools.Method(
                typeof(GameLocation),
                methodName);

            if (method == null)
            {
                return;
            }

            harmony.Patch(
                method,
                transpiler: new HarmonyMethod(
                    typeof(WarpWarningPatch),
                    nameof(Transpiler)));
        }

        /// <summary>
        /// Stardew Valley本体のログ警告処理を
        /// T's Coreのフィルター処理に差し替えます。
        /// </summary>
        private static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo? filterMethod = AccessTools.Method(
                typeof(WarpWarningPatch),
                nameof(LogWarpWarning));

            if (filterMethod == null)
            {
                foreach (CodeInstruction instruction in instructions)
                {
                    yield return instruction;
                }

                yield break;
            }

            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.operand is MethodInfo method
                    && method.Name == "Warn"
                    && method.ReturnType == typeof(void)
                    && method.GetParameters().Length == 1
                    && method.GetParameters()[0].ParameterType == typeof(string))
                {
                    yield return new CodeInstruction(
                        OpCodes.Call,
                        filterMethod);
                }
                else
                {
                    yield return instruction;
                }
            }
        }

        /// <summary>
        /// Stardew Valley本体のログ警告を確認し、
        /// T's CoreのWarp Actionに関する既知の警告だけを除外します。
        /// </summary>
        private static void LogWarpWarning(
            object logger,
            string message)
        {
            if (IsTsCoreWarpWarning(message))
            {
                return;
            }

            MethodInfo? warnMethod = AccessTools.Method(
                logger.GetType(),
                "Warn",
                new[]
                {
                    typeof(string)
                });

            warnMethod?.Invoke(
                logger,
                new object[]
                {
                    message
                });
        }

        /// <summary>
        /// T's Coreが登録するWarp Action名です。
        /// </summary>
        private static readonly string[] TsCoreWarpActions =
        {
            "TsCoreWarp",
            "TsCoreMagicWarp",
            "TsCoreMagicWarp_Simple"
        };

        /// <summary>
        /// T's CoreのWarp Actionに関する警告か判定します。
        /// </summary>
        private static bool IsTsCoreWarpWarning(
            string message)
        {
            return TsCoreWarpActions.Any(
                action =>
                    message.Contains(
                        $"unknown warp property '{action} ",
                        StringComparison.Ordinal));
        }
    }
}