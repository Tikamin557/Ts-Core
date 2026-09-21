using HarmonyLib;
using StardewValley;
using Ts_Core.Services.BigCraftableRelated;

namespace Ts_Core.Patches.BigCraftableRelated
{
    /// <summary>
    /// BigCraftable Extensionの
    /// 睡眠処理を補完します。
    /// </summary>
    public static class BigCraftableSleepActionPatch
    {
        //----------------------------------------
        // Apply
        //----------------------------------------

        public static void Apply(
            Harmony harmony)
        {
            harmony.Patch(
                original:
                    AccessTools.Method(
                        typeof(GameLocation),
                        nameof(GameLocation.answerDialogueAction),
                        new[]
                        {
                            typeof(string),
                            typeof(string[])
                        }),
                postfix:
                    new HarmonyMethod(
                        typeof(BigCraftableSleepActionPatch),
                        nameof(AnswerDialogueActionPostfix)));
        }

        //----------------------------------------
        // Postfix
        //----------------------------------------

        private static void AnswerDialogueActionPostfix(
            string questionAndAnswer)
        {
            //----------------------------------------
            // TsCore Sleep確認中ではない
            //----------------------------------------

            if (!BigCraftableSleepActionService
                .IsSleepQuestionActive)
            {
                return;
            }

            //----------------------------------------
            // Yes
            //----------------------------------------

            if (questionAndAnswer
                == "SleepTent_Yes")
            {
                // SleepTent_Yesで非表示になった
                // Farmerを再表示
                Game1.displayFarmer =
                    true;

                // 睡眠を確定
                BigCraftableSleepActionService
                    .ConfirmSleep();

                return;
            }

            //----------------------------------------
            // No
            //----------------------------------------

            if (questionAndAnswer
                == "SleepTent_No")
            {
                BigCraftableSleepActionService
                    .ClearSleepQuestion();
            }
        }
    }
}