using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using Ts_Core.Actions;
using Ts_Core.Debug;
using Ts_Core.Initializers;
using Ts_Core.Interfaces;
using Ts_Core.Patches;
using Ts_Core.Providers;
using Ts_Core.Services.Location;
using Ts_Core.Services.Notification;
using Ts_Core.Services.Relationship;
using Ts_Core.Services.WarpRelated;
using Ts_Core.Tokens;

namespace Ts_Core
{
    /// <summary>
    /// Ts_Coreのエントリーポイントです。
    /// 各サービス・トークン・アクション・デバッグ機能を初期化します。
    /// </summary>
    public class ModEntry : Mod
    {
        //----------------------------------------
        // API対応優先順位
        //----------------------------------------

        /// <summary>
        /// API対応している結婚Modの優先順位。
        /// 上から順に優先して使用される。
        /// </summary>
        private static readonly string[] SupportedMarriageApis =
        {
            "ApryllForever.PolyamorySweetLove",
            "aedenthorn.FreeLove"
        };

        //----------------------------------------
        // サービス
        //----------------------------------------

        private PartnerService service = null!;
        private IPartnerProvider provider = null!;
        private Harmony? harmony;

        //----------------------------------------
        // エントリーポイント
        //----------------------------------------

        public override void Entry(IModHelper helper)
        {
            harmony = new Harmony(ModManifest.UniqueID);

            // Stardew Valley標準のWarp警告を抑制
            WarpWarningPatch.Apply(harmony);

            InitializeServices(helper);
            RegisterSystems(helper);
            RegisterEvents(helper);

            NotificationThemeManager.Initialize(
                Helper,
                Monitor);

            NotificationService.Initialize(helper);
        }

        //----------------------------------------
        // サービス初期化
        //----------------------------------------

        private void InitializeServices(IModHelper helper)
        {
            LocationTracker.Initialize(helper);

            // 使用可能な結婚APIを検索
            string? modId = SupportedMarriageApis
                .FirstOrDefault(id => helper.ModRegistry.IsLoaded(id));

            provider = modId != null
                ? new ApiMarriageProvider(helper, modId)
                : new VanillaProvider();

            service = new PartnerService(provider);
        }

        //----------------------------------------
        // システム登録
        //----------------------------------------

        private void RegisterSystems(IModHelper helper)
        {
            // デバッグコマンド登録
            DebugCommandRegistry.Register(
                helper,
                Monitor,
                service,
                provider);

            // アクション登録
            ActionRegistry.Register();

            // Warpサービス初期化
            WarpService.Initialize(Monitor);

            // Warp定義読み込み
            WarpLoader.Load(
                helper,
                Monitor);
        }

        //----------------------------------------
        // イベント登録
        //----------------------------------------

        private void RegisterEvents(IModHelper helper)
        {
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        }

        //----------------------------------------
        // 配偶者をルーム順で取得
        //----------------------------------------

        private IEnumerable<string> GetOrderedPartners()
        {
            return service.GetRoomOrderedPartners()
                ?? Enumerable.Empty<string>();
        }

        //----------------------------------------
        // GameLaunched
        //----------------------------------------

        private void OnGameLaunched(
            object? sender,
            GameLaunchedEventArgs e)
        {
            MarriageApiInitializer.Initialize(
                Helper.ModRegistry,
                Monitor,
                provider);

            RegisterContentPatcherTokens();
        }

        //----------------------------------------
        // Content Patcher Token登録
        //----------------------------------------

        private void RegisterContentPatcherTokens()
        {
            var api =
                Helper.ModRegistry.GetApi<IContentPatcherAPI>(
                    "Pathoschild.ContentPatcher");

            if (api == null)
            {
                Monitor.Log(
                    "Content Patcher not found.",
                    LogLevel.Error);

                return;
            }

            TokenRegistrar.Register(
                api,
                ModManifest,
                service,
                GetOrderedPartners);
        }
    }
}