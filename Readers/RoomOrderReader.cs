using System.Reflection;

namespace Ts_Core.Readers
{
    /// <summary>
    /// 他Modの配偶者部屋設定を読み取るクラス
    /// </summary>
    public static class RoomOrderReader
    {
        //----------------------------------------
        // 対応している部屋Mod
        //----------------------------------------

        private static readonly string[] TargetAssemblies =
        {
            "PolyamorySweetRooms",
            "CustomSpouseRooms"
        };

        private static readonly Dictionary<string, string> SupportedMods = new()
        {
            ["PolyamorySweetRooms"] = "Polyamory Sweet Rooms",
            ["CustomSpouseRooms"] = "Custom Spouse Rooms"
        };

        //----------------------------------------
        // キャッシュ
        //----------------------------------------

        private static Assembly? assembly;
        private static FieldInfo? configField;
        private static PropertyInfo? roomOrderProperty;

        //----------------------------------------
        // 部屋順読み込み
        //----------------------------------------

        /// <summary>
        /// 配偶者部屋の並び順を取得します。
        /// 対応Modが存在しない場合は null を返します。
        /// </summary>
        public static List<string>? Load()
        {
            try
            {
                //----------------------------------------
                // Assembly取得（初回のみ）
                //----------------------------------------

                if (assembly == null)
                {
                    CurrentRoomMod = "Vanilla";

                    foreach (string target in TargetAssemblies)
                    {
                        assembly = AppDomain.CurrentDomain
                            .GetAssemblies()
                            .FirstOrDefault(a => a.GetName().Name == target);

                        if (assembly != null)
                        {
                            CurrentRoomMod =
                                SupportedMods.TryGetValue(target, out string? name)
                                    ? name
                                    : target;

                            break;
                        }
                    }

                    if (assembly == null)
                        return null;
                }

                //----------------------------------------
                // Config取得（リフレクション）
                //----------------------------------------

                if (configField == null)
                {
                    Type? modEntryType = assembly
                        .GetTypes()
                        .FirstOrDefault(t => t.Name == "ModEntry");

                    if (modEntryType == null)
                        return null;

                    configField = modEntryType.GetField(
                        "Config",
                        BindingFlags.Public |
                        BindingFlags.NonPublic |
                        BindingFlags.Static);

                    if (configField == null)
                        return null;
                }

                object? config = configField.GetValue(null);

                if (config == null)
                    return null;

                //----------------------------------------
                // SpouseRoomOrder取得
                //----------------------------------------

                if (roomOrderProperty == null)
                {
                    roomOrderProperty = config.GetType().GetProperty("SpouseRoomOrder");

                    if (roomOrderProperty == null)
                        return null;
                }

                //----------------------------------------
                // 並び順取得
                //----------------------------------------

                string? order = roomOrderProperty.GetValue(config) as string;

                if (string.IsNullOrWhiteSpace(order))
                    return null;

                return order
                    .Split(',')
                    .Select(s => s.Trim())
                    .Where(s => s.Length > 0)
                    .ToList();
            }
            catch (Exception ex)
            {
            #if DEBUG
                System.Diagnostics.Debug.WriteLine(ex);
            #endif

                return null;
            }
        }

        //----------------------------------------
        // キャッシュリセット
        //----------------------------------------

        /// <summary>
        /// リフレクション情報を初期化します。
        /// </summary>
        public static void ResetCache()
        {
            assembly = null;
            configField = null;
            roomOrderProperty = null;
            CurrentRoomMod = "Vanilla";
        }

        //----------------------------------------
        // 現在使用中の部屋Mod
        //----------------------------------------

        /// <summary>
        /// 現在読み込まれている部屋Mod名です。
        /// 見つからない場合は "Vanilla" を返します。
        /// </summary>
        public static string CurrentRoomMod { get; private set; } = "Vanilla";
    }
}