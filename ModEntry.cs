using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using Ts_Core.Actions;
using Ts_Core.Debug;
using Ts_Core.Initializers;
using Ts_Core.Interfaces;
using Ts_Core.Patches;
using Ts_Core.Providers;
using Ts_Core.Services.BuildingRelated;
using Ts_Core.Services.ContentPatcherRelated;
using Ts_Core.Services.FarmhouseFixes;
using Ts_Core.Services.Location;
using Ts_Core.Services.LocationFixes;
using Ts_Core.Services.Migration;
using Ts_Core.Services.Notification;
using Ts_Core.Services.Relationship;
using Ts_Core.Services.WarpRelated;
using Ts_Core.Tokens;
using Ts_Core.Models;
using Ts_Core.Services.GenericModConfigMenuRelated;

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
        // Config
        //----------------------------------------

        internal static ModConfig Config
        {
            get;
            private set;
        } = new();

        //----------------------------------------
        // サービス
        //----------------------------------------

        private PartnerService service = null!;
        private IPartnerProvider provider = null!;
        private Harmony? harmony;

        //----------------------------------------
        // Content Patcher
        //----------------------------------------

        /// <summary>
        /// GameLaunched後にGMCM表示条件を
        /// 1回だけ反映するためのフラグ。
        /// </summary>
        private bool refreshContentPatcherConfigMenus;

        //----------------------------------------
        // エントリーポイント
        //----------------------------------------

        public override void Entry(
            IModHelper helper)
        {
            //----------------------------------------
            // Config読み込み
            //----------------------------------------

            Config =
                helper.ReadConfig<ModConfig>();

            harmony =
                new Harmony(
                    ModManifest.UniqueID);

            //----------------------------------------
            // Harmony Patch
            //----------------------------------------

            // Stardew Valley標準のWarp警告を抑制
            WarpWarningPatch.Apply(
                harmony);

            // Warp暗転オーバーレイを最終描画
            WarpBlackoutOverlayPatch.Apply(
                harmony);

            // Stardew Valley標準のレイントーテムの不具合を修正
            RainTotemPatch.Apply(
                harmony,
                Monitor);

            // FarmHouse配偶者部屋の不要タイルを修正
            SpouseRoomTileFixPatch.Apply(
                harmony);

            // FarmHouseへのWarp時に指定座標が入口へ
            // 変更される問題を修正
            FarmHouseWarpFixPatch.Apply(
                harmony);

            // 建物移動・撤去時にBuilding Lightを更新
            BuildingLightPatch.Apply(
                harmony);

            // Buildingに条件付きDrawLayerを追加
            BuildingDrawLayerPatch.Apply(
                harmony);

            // Buildingの建築Location制限を適用
            BuildingLocationRestrictionPatch.Apply(
                harmony);

            //----------------------------------------
            // 初期化
            //----------------------------------------

            InitializeServices(
                helper);

            RegisterSystems(
                helper);

            RegisterEvents(
                helper);

            NotificationThemeManager.Initialize(
                Helper,
                Monitor);

            NotificationService.Initialize(
                helper);
        }

        //----------------------------------------
        // サービス初期化
        //----------------------------------------

        private void InitializeServices(
            IModHelper helper)
        {
            LocationTracker.Initialize(
                helper);

            //----------------------------------------
            // 使用可能な結婚APIを検索
            //----------------------------------------

            string? modId =
                SupportedMarriageApis
                    .FirstOrDefault(
                        id =>
                            helper.ModRegistry
                                .IsLoaded(id));

            provider =
                modId != null
                    ? new ApiMarriageProvider(
                        helper,
                        modId)
                    : new VanillaProvider();

            service =
                new PartnerService(
                    provider);
        }

        //----------------------------------------
        // システム登録
        //----------------------------------------

        private void RegisterSystems(
            IModHelper helper)
        {
            //----------------------------------------
            // デバッグコマンド登録
            //----------------------------------------

            DebugCommandRegistry.Register(
                helper,
                Monitor,
                service,
                provider);

            //----------------------------------------
            // アクション登録
            //----------------------------------------

            ActionRegistry.Register();

            //----------------------------------------
            // Warpサービス初期化
            //----------------------------------------

            WarpService.Initialize(
                Monitor);

            //----------------------------------------
            // Previous Home記録サービス初期化
            //----------------------------------------

            PreviousHomeService.Initialize(
                helper);

            //----------------------------------------
            // Warp暗転オーバーレイ初期化
            //----------------------------------------

            WarpBlackoutOverlayService.Initialize(
                helper);

            //----------------------------------------
            // FarmHouseセラー入口修正
            //----------------------------------------

            CellarEntranceFixService.Initialize(
                helper,
                Monitor);

            //----------------------------------------
            // Farmhand Map更新補完
            //----------------------------------------

            FarmhandMapRefreshFixService.Initialize(
                helper,
                Monitor);

            //----------------------------------------
            // Warp定義読み込み
            //----------------------------------------

            WarpLoader.Load(
                helper,
                Monitor);

            //----------------------------------------
            // Building Light定義読み込み
            //----------------------------------------

            BuildingProviderLoader.Load(
                helper,
                Monitor);

            //----------------------------------------
            // Migration定義読み込み
            //----------------------------------------

            MigrationService.Load(
                helper,
                Monitor);
        }

        //----------------------------------------
        // イベント登録
        //----------------------------------------

        private void RegisterEvents(
            IModHelper helper)
        {
            //----------------------------------------
            // GameLoop
            //----------------------------------------

            helper.Events.GameLoop.GameLaunched
                += OnGameLaunched;

            // GMCM初期更新用
            helper.Events.GameLoop.UpdateTicked
                += OnUpdateTicked;

            // Building Light / Migration
            helper.Events.GameLoop.SaveLoaded
                += OnSaveLoaded;

            helper.Events.GameLoop.TimeChanged
                += OnTimeChanged;

            //----------------------------------------
            // Player
            //----------------------------------------

            helper.Events.Player.Warped
                += OnWarped;

            //----------------------------------------
            // Content
            //----------------------------------------

            helper.Events.Content.AssetReady
                += OnAssetReady;
        }

        //----------------------------------------
        // 配偶者をルーム順で取得
        //----------------------------------------

        private IEnumerable<string> GetOrderedPartners()
        {
            return service
                .GetRoomOrderedPartners()
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

            //----------------------------------------
            // T's Core GMCM登録
            //----------------------------------------

            GenericModConfigMenuService.Register(
                Helper,
                ModManifest,
                getConfig: () => Config,
                setConfig: config => Config = config);

            //----------------------------------------
            // Content Patcher自身のGMCM初期登録後に
            // T's Core独自のGMCM表示条件を反映
            //----------------------------------------

            refreshContentPatcherConfigMenus =
                true;
        }

        //----------------------------------------
        // UpdateTicked
        // Content Patcher GMCM初期更新用
        //----------------------------------------

        private void OnUpdateTicked(
            object? sender,
            UpdateTickedEventArgs e)
        {
            //----------------------------------------
            // 起動時GMCM更新待ち
            //----------------------------------------

            if (!refreshContentPatcherConfigMenus)
                return;

            refreshContentPatcherConfigMenus =
                false;

            //----------------------------------------
            // T's Core GMCM表示条件反映
            //----------------------------------------

            ContentPatcherReloadService
                .RefreshConfigMenus(
                    Helper,
                    Monitor);

            //----------------------------------------
            // 起動時の1回だけ使用
            //----------------------------------------

            Helper.Events.GameLoop.UpdateTicked
                -= OnUpdateTicked;
        }

        //----------------------------------------
        // SaveLoaded
        //----------------------------------------

        private void OnSaveLoaded(
            object? sender,
            SaveLoadedEventArgs e)
        {
            //----------------------------------------
            // Building Migration
            //----------------------------------------

            BuildingMigrationService
                .ApplyMigrations(
                    Monitor);

            //----------------------------------------
            // Building Light
            //----------------------------------------

            BuildingLightService
                .UpdateLights();
        }

        //----------------------------------------
        // TimeChanged
        //----------------------------------------

        private void OnTimeChanged(
            object? sender,
            TimeChangedEventArgs e)
        {
            BuildingLightService
                .UpdateLights();
        }

        //----------------------------------------
        // Warped
        //----------------------------------------

        private void OnWarped(
            object? sender,
            WarpedEventArgs e)
        {
            if (!e.IsLocalPlayer)
                return;

            //----------------------------------------
            // Building Light
            //----------------------------------------

            BuildingLightService
                .UpdateLights();
        }

        //----------------------------------------
        // AssetReady
        //----------------------------------------

        private void OnAssetReady(
            object? sender,
            AssetReadyEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            if (!e.NameWithoutLocale
                .IsEquivalentTo(
                    "Data/Buildings"))
            {
                return;
            }

            BuildingLightService
                .UpdateLights();
        }

        //----------------------------------------
        // Content Patcher Token登録
        //----------------------------------------

        private void RegisterContentPatcherTokens()
        {
            var api =
                Helper.ModRegistry
                    .GetApi<IContentPatcherAPI>(
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