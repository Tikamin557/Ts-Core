using StardewValley;
using System.Reflection;

namespace Ts_Core.Services.TerrainFeaturesRelated
{
    /// <summary>
    /// Reset / Generate の再生成元となるロケーションを準備します。
    /// 生成失敗時に既存のマップを変更しないため、呼び出し側は成功を確認してから変更してください。
    /// </summary>
    internal static class TerrainFeaturesCloneService
    {
        /// <summary>
        /// 元 Mod の MakeClone に相当する、(mapPath, Name) コンストラクターによる複製です。
        /// 対応しないロケーションは安全のため処理対象外とします。
        /// </summary>
        internal static bool TryCreate(GameLocation location, out GameLocation? clone)
        {
            clone = null;
            if (location == null)
                return false;

            try
            {
                ConstructorInfo? constructor = location.GetType().GetConstructor(new[] { typeof(string), typeof(string) });
                if (constructor == null)
                    return false;

                clone = constructor.Invoke(new object[] { location.mapPath.Value, location.Name }) as GameLocation;
                if (clone == null)
                    return false;

                clone.DayUpdate(Game1.dayOfMonth);
                return true;
            }
            catch
            {
                clone = null;
                return false;
            }
        }
    }
}
