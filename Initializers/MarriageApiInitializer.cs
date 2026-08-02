using StardewModdingAPI;
using Ts_Core.Interfaces;
using Ts_Core.Providers;

namespace Ts_Core.Initializers
{
    /// <summary>
    /// 結婚Mod APIの初期化を行います。
    /// </summary>
    public static class MarriageApiInitializer
    {
        //----------------------------------------
        // API初期化
        //----------------------------------------

        /// <summary>
        /// 使用中の結婚ModからAPIを取得してProviderへ設定します。
        /// </summary>
        public static void Initialize(
            IModRegistry modRegistry,
            IMonitor monitor,
            IPartnerProvider provider)
        {
            //----------------------------------------
            // API対応Provider以外は対象外
            //----------------------------------------

            if (provider is not ApiMarriageProvider apiProvider)
                return;

            //----------------------------------------
            // API取得
            //----------------------------------------

            IMarriageApi? marriageApi =
                modRegistry.GetApi<IMarriageApi>(apiProvider.ModId);

            if (marriageApi == null)
            {
                monitor.Log(
                   $"Failed to load marriage API: {apiProvider.ModId}",
                    LogLevel.Warn);

                return;
            }

            //----------------------------------------
            // Providerへ設定
            //----------------------------------------

            apiProvider.SetApi(marriageApi);
        }
    }
}