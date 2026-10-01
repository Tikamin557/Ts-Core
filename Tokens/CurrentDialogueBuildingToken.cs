using StardewValley.Buildings;
using Ts_Core.Interfaces;
using Ts_Core.Services.DialogueRelated;

namespace Ts_Core.Tokens
{
    /// <summary>
    /// Building.doActionからTsCoreDialogueが呼ばれている間だけ、
    /// 操作元Buildingの永続IDをContent Patcherへ公開する診断用Tokenです。
    /// </summary>
    internal static class CurrentDialogueBuildingToken
    {
        private const string BuildingIdKey =
            "Tikamin557.TsCore/DialogueBuildingId";

        internal static void Register(
            IContentPatcherAPI api,
            StardewModdingAPI.IManifest manifest)
        {
            api.RegisterToken(
                manifest,
                "CurrentDialogueBuildingId",
                GetValue);
        }

        private static IEnumerable<string>? GetValue()
        {
            Building? building =
                DialogueBuildingContext.ActiveBuilding;

            if (building == null)
                return new[]
                {
                    "NoBuilding"
                };

            if (!building.modData.TryGetValue(
                    BuildingIdKey,
                    out string? buildingId)
                || string.IsNullOrWhiteSpace(buildingId))
            {
                buildingId = Guid.NewGuid()
                    .ToString("N");

                building.modData[BuildingIdKey] =
                    buildingId;
            }

            return new[]
            {
                buildingId
            };
        }
    }
}
