using HarmonyLib;
using StardewModdingAPI;
using System.Reflection;
using Ts_Core.Services.Relationship;

namespace Ts_Core.Patches
{
    /// <summary>
    /// Polyamory Sweet Roomsが、現在のData/Charactersに存在しない配偶者の
    /// 部屋を生成しようとした際の例外を防止します。
    /// </summary>
    internal static class PolyamorySweetRoomsCompatibilityPatch
    {
        //----------------------------------------
        // PSR識別情報 / Reflectionキャッシュ
        //----------------------------------------

        private const string PsrModId = "ApryllForever.PolyamorySweetRooms";
        private const string PsrModEntryTypeName = "PolyamorySweetRooms.ModEntry";

        private static IMonitor? Monitor;
        private static FieldInfo? roomNameField;
        private static PropertyInfo? roomNameProperty;

        /// <summary>
        /// Polyamory Sweet Roomsが導入されている場合のみ互換パッチを適用します。
        /// </summary>
        internal static void Apply(
            Harmony harmony,
            IModHelper helper,
            IMonitor monitor)
        {
            //----------------------------------------
            // PSR未導入時は何もしない
            //----------------------------------------

            if (!helper.ModRegistry.IsLoaded(PsrModId))
                return;

            Monitor = monitor;

            try
            {
                Type? modEntryType =
                    AccessTools.TypeByName(PsrModEntryTypeName);

                MethodInfo? method =
                    modEntryType?
                        .GetMethods(
                            BindingFlags.Static
                            | BindingFlags.Instance
                            | BindingFlags.Public
                            | BindingFlags.NonPublic)
                        .FirstOrDefault(method =>
                            method.Name == "MakeSpouseRoom"
                            && method.GetParameters().Length == 4
                            && method.GetParameters()[2].ParameterType.Name == "SpouseRoomData");

                if (method == null)
                {
                    Monitor.Log(
                        "Polyamory Sweet Rooms is installed, but MakeSpouseRoom could not be found. " +
                        "The compatibility patch for missing spouse NPCs was not applied. " +
                        "Polyamory Sweet Rooms may have changed in an update.",
                        LogLevel.Warn);

                    return;
                }

                harmony.Patch(
                    method,
                    prefix: new HarmonyMethod(
                        typeof(PolyamorySweetRoomsCompatibilityPatch),
                        nameof(MakeSpouseRoomPrefix)));
            }
            catch (Exception ex)
            {
                Monitor.Log(
                    $"Failed to apply the Polyamory Sweet Rooms compatibility patch. " +
                    $"The rest of T's Core will continue normally.\n{ex}",
                    LogLevel.Warn);
            }
        }

        /// <summary>
        /// PSRのSpouseRoomData.nameが現在のData/Charactersに存在しない場合、
        /// その部屋の生成だけをスキップします。
        /// </summary>
        private static bool MakeSpouseRoomPrefix(
            object __2)
        {
            try
            {
                string? npcName = GetRoomName(__2);

                if (string.IsNullOrWhiteSpace(npcName))
                    return true;

                //----------------------------------------
                // 現在存在するNPCはPSR本来の処理へ
                //----------------------------------------

                if (RomanceCandidateService.Exists(npcName))
                    return true;

                //----------------------------------------
                // 削除済みNPC等はPSR処理をスキップ
                //----------------------------------------

                Monitor?.Log(
                    $"Skipped a Polyamory Sweet Rooms spouse room for '{npcName}' because that character " +
                    "does not exist in the current Data/Characters.",
                    LogLevel.Debug);

                return false;
            }
            catch (Exception ex)
            {
                //----------------------------------------
                // 互換処理側の問題でPSR本来の処理を
                // 妨げない
                //----------------------------------------

                Monitor?.Log(
                    $"Failed to check a Polyamory Sweet Rooms spouse room before it was created. " +
                    $"The original PSR method will run normally.\n{ex}",
                    LogLevel.Warn);

                return true;
            }
        }

        //----------------------------------------
        // PSR SpouseRoomDataからNPC名を取得
        //----------------------------------------

        /// <summary>
        /// PSRのSpouseRoomDataからname/NameをReflectionで取得します。
        /// PSR更新によるField/Property差異を許容するため両方を確認します。
        /// </summary>
        private static string? GetRoomName(
            object roomData)
        {
            Type type = roomData.GetType();

            roomNameField ??=
                AccessTools.Field(
                    type,
                    "name")
                ?? AccessTools.Field(
                    type,
                    "Name");

            if (roomNameField?.GetValue(roomData) is string fieldValue)
                return fieldValue;

            roomNameProperty ??=
                AccessTools.Property(
                    type,
                    "name")
                ?? AccessTools.Property(
                    type,
                    "Name");

            return roomNameProperty?.GetValue(roomData) as string;
        }
    }
}
