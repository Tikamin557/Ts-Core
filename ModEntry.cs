using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using Ts_Core.Actions;
using Ts_Core.Api;
using Ts_Core.Debug;
using Ts_Core.Initializers;
using Ts_Core.Interfaces;
using Ts_Core.Models;
using Ts_Core.Patches;
using Ts_Core.Patches.BigCraftableRelated;
using Ts_Core.Providers;
using Ts_Core.Services.BigCraftableRelated;
using Ts_Core.Services.BuildingRelated;
using Ts_Core.Services.ContentPatcherRelated;
using Ts_Core.Services.DebugSupport;
using Ts_Core.Services.DialogueRelated;
using Ts_Core.Services.FarmhouseFixes;
using Ts_Core.Services.GameStateQueryRelated;
using Ts_Core.Services.GenericModConfigMenuRelated;
using Ts_Core.Services.Location;
using Ts_Core.Services.LocationFixes;
using Ts_Core.Services.MapRelated.TimedExit;
using Ts_Core.Services.Migration;
using Ts_Core.Services.Notification;
using Ts_Core.Services.Relationship;
using Ts_Core.Services.ScreenshotRelated;
using Ts_Core.Services.ShortcutPanelRelated;
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
        // 外部API対応優先順位
        //----------------------------------------

        /// <summary>
        /// 外部API対応している結婚Modの優先順位。
        /// 上から順に優先して使用される。
        /// </summary>
        private static readonly string[] SupportedMarriageApis =
        {
            "ApryllForever.PolyamorySweetLove",
            "aedenthorn.FreeLove"
        };

        //----------------------------------------
        // 設定
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

        /// <summary>
        /// Modの起動時に呼ばれ、設定の読み込み・サービス初期化・各システムとイベントの登録を行います。
        /// </summary>
        public override void Entry(
            IModHelper helper)
        {
            //----------------------------------------
            // 設定読み込み
            //----------------------------------------

            Config =
                helper.ReadConfig<ModConfig>();

            harmony =
                new Harmony(
                    ModManifest.UniqueID);

            //----------------------------------------
            // Harmonyパッチ
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

            // FarmHouse配偶者部屋の特殊表示物を
            // T's Core Option設定に応じて調整
            SpouseRoomVisualFixPatch.Apply(
                harmony);

            // FarmHouseの通常玄関からのWarpを追跡
            FarmHouseEntranceTracker.Apply(
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

            // BigCraftable Extensionの
            // 設置範囲を適用
            BigCraftablePlacementPatch.Apply(
                harmony);

            // BigCraftable Extensionの
            // 追加Collision範囲を占有タイルとして扱う
            BigCraftableOccupancyPatch.Apply(
                harmony);

            // BigCraftable Extensionの
            // 追加Collision範囲を移動Collisionに適用
            BigCraftableMovementCollisionPatch.Apply(
                harmony);

            // BigCraftable Extensionの
            // xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            BigCraftableAndroidTapToMovePatch.Apply(
                harmony,
                Monitor);

            // BigCraftable Extensionの
            // TilePropertyを有効化
            BigCraftableTilePropertyPatch.Apply(
                harmony);

            // BigCraftable Extensionの
            // TileActionを有効化
            BigCraftableActionPatch.Apply(
                harmony);

            // BigCraftable Extensionの
            // Sleep時のFarmer表示を補完
            BigCraftableSleepActionPatch.Apply(
                harmony);

            // BigCraftable Extensionの
            // 継続寝床での起床判定を補完
            BigCraftableSleepWakeUpPatch.Apply(
                harmony);

            // BigCraftable Extensionの
            // Action / Idle Wobbleを有効化
            MachineShouldWobblePatch.Apply(
                harmony);

            // BigCraftable Extensionの
            // Texture / Animation / DrawLayerを有効化
            MachineAnimationTexturePatch.Apply(
                harmony);

            // BigCraftable Extensionの
            // Texture Size / DrawLayerをMenu描画に適用
            BigCraftableMenuDrawPatch.Apply(
                harmony);

            // BigCraftable Extensionの
            // Texture Size / DrawLayerをCrafting Recipe描画に適用
            BigCraftableCraftingRecipeDrawPatch.Apply(
                harmony);

            // BigCraftable Extensionの
            // Crafting Page描画位置を中央に補正
            BigCraftableCraftingPageDrawPatch.Apply(
                harmony);

            // BigCraftable Extensionの
            // Texture Size / DrawLayerをCrafting Page描画に適用
            BigCraftableCraftingPagePatch.Apply(
                harmony);

            // BigCraftable Extensionの
            // 手持ち状態でのTexture / DrawLayerを適用
            BigCraftableHeldDrawPatch.Apply(
                harmony);

            // BigCraftable Extensionの
            // 拡張Collision範囲へのTool Actionを適用
            BigCraftableToolActionPatch.Apply(
                harmony);

            // BigCraftable Extensionの
            // 仮想Collision範囲からの素手回収を有効化
            BigCraftableBareHandRemovalPatch.Apply(
                harmony);

            // BigCraftable Extensionの
            // 拡張Texture用描画範囲を有効化
            BigCraftableDrawCullingPatch.Apply(
                harmony,
                Monitor);

            // BigCraftable Extensionの
            // 拡張Texture用描画範囲を有効化
            // (Ultra Smooth Mod導入時のみ)
            BigCraftableUltraSmoothCompatibilityPatch.Apply(
                harmony,
                Monitor);

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

            // ショートカットパネルより先に初期化し、
            // パネル描画前のCurrent Screenを取得できるようにする。
            ScreenshotService.Initialize(
                helper,
                Monitor);

            ShortcutPanelService.Initialize(
                helper,
                Monitor);
        }

        //----------------------------------------
        // 外部API
        //----------------------------------------

        /// <summary>
        /// 外部Modへ公開する
        /// T's Core APIを返します。
        /// </summary>
        public override object GetApi()
        {
            return new TsCoreApi();
        }

        //----------------------------------------
        // サービス初期化
        //----------------------------------------

        /// <summary>
        /// T's Coreで使用する各サービスを初期化します。
        /// </summary>
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

        /// <summary>
        /// T's Coreが提供する各機能やシステムを登録します。
        /// </summary>
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
            // Time Skip初期化
            //----------------------------------------

            TimeSkipService.Initialize(
                Helper);

            //----------------------------------------
            // Machine Texture / Animation初期化
            //----------------------------------------

            MachineAnimationTextureService.Initialize(
                Monitor);

            BigCraftableExtensionTextureService.Initialize(
                Monitor);

            //----------------------------------------
            // アクション登録
            //----------------------------------------

            ActionRegistry.Register();

            //----------------------------------------
            // Game State Query登録
            //----------------------------------------

            GameStateQueryService.Register();

            //----------------------------------------
            // BigCraftable Extension
            // Sleep TileAction登録
            //----------------------------------------

            BigCraftableSleepActionService.Register();

            //----------------------------------------
            // BigCraftable Extension
            // Sleep継続寝床サービス初期化
            //----------------------------------------

            BigCraftableSleepPersistenceService.Initialize(
                helper);

            //----------------------------------------
            // Warp Providerサービス初期化
            //----------------------------------------

            WarpProviderService.Initialize(
                Monitor);

            //----------------------------------------
            // Warpサービス初期化
            //----------------------------------------

            WarpService.Initialize(
                Monitor);

            //----------------------------------------
            // Timed Exitサービス初期化
            //----------------------------------------

            TimedExitService.Initialize(
                helper,
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
            // Dialogue初期化
            //----------------------------------------

            DialogueService.Initialize(
                Monitor);

            //----------------------------------------
            // 配偶者部屋特殊表示物修正初期化
            //----------------------------------------

            SpouseRoomVisualFixService.Initialize(
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
        }

        //----------------------------------------
        // イベント登録
        //----------------------------------------

        /// <summary>
        /// ゲーム中に使用するSMAPIイベントを登録します。
        /// </summary>
        private void RegisterEvents(
            IModHelper helper)
        {
            //----------------------------------------
            // GameLoop
            //----------------------------------------

            // ゲーム起動完了処理
            helper.Events.GameLoop.GameLaunched
                += OnGameLaunched;

            // BigCraftable Extension - Sleep
            helper.Events.GameLoop.DayStarted
                += BigCraftableSleepActionService.OnDayStarted;

            // GMCM初期更新用
            helper.Events.GameLoop.UpdateTicked
                += OnUpdateTicked;

            // BigCraftable Extension - Light
            helper.Events.GameLoop.UpdateTicked
                += MachineLightService.OnUpdateTicked;

            // BigCraftable Extension - Idle Wobble
            helper.Events.GameLoop.UpdateTicked
                += MachineIdleWobbleService.OnUpdateTicked;

            // Building Light / Migration
            // BigCraftable Extension - Sleep継続寝床読込
            helper.Events.GameLoop.SaveLoaded
                += OnSaveLoaded;

            // BigCraftable Extension - Sleep継続寝床保存
            helper.Events.GameLoop.Saving
                += OnSaving;

            // 時刻変更処理
            helper.Events.GameLoop.TimeChanged
                += OnTimeChanged;

            // BigCraftable Extension - Idle Effects
            helper.Events.GameLoop.TimeChanged
                += MachineIdleEffectService.OnTimeChanged;

            // BigCraftable Extension - 状態クリア
            helper.Events.GameLoop.ReturnedToTitle
                += OnReturnedToTitle;

            //----------------------------------------
            // Player
            //----------------------------------------

            helper.Events.Player.Warped
                += OnWarped;

            //----------------------------------------
            // Content
            //----------------------------------------

            // Building Providers
            helper.Events.Content.AssetRequested
                += BuildingProviderDataService.OnAssetRequested;

            // Migration
            helper.Events.Content.AssetRequested
                += MigrationDataService.OnAssetRequested;

            // Warp Providers
            helper.Events.Content.AssetRequested
                += WarpProviderDataService.OnAssetRequested;

            // Notification Themes
            helper.Events.Content.AssetRequested
                += NotificationThemeDataService.OnAssetRequested;

            // Dialogues
            helper.Events.Content.AssetRequested
                += DialogueDataService.OnAssetRequested;

            // BigCraftable Extension
            helper.Events.Content.AssetRequested
                += BigCraftableExtensionDataService.OnAssetRequested;

            helper.Events.Content.AssetReady
                += BigCraftableExtensionDataService.OnAssetReady;

            helper.Events.Content.AssetReady
                += OnAssetReady;
        }

        //----------------------------------------
        // 配偶者をルーム順で取得
        //----------------------------------------

        /// <summary>
        /// 現在の配偶者を、部屋配置などで使用する順序に並べて返します。
        /// </summary>
        private IEnumerable<string> GetOrderedPartners()
        {
            return service
                .GetRoomOrderedPartners()
                ?? Enumerable.Empty<string>();
        }

        //----------------------------------------
        // GameLaunched
        //----------------------------------------

        /// <summary>
        /// ゲーム起動完了時の初期化処理を行います。
        /// </summary>
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
                Monitor,
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

        /// <summary>
        /// ゲームの更新tickごとに必要なT's Coreの処理を実行します。
        /// </summary>
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

        /// <summary>
        /// セーブデータ読み込み完了時に必要な初期化・再登録処理を行います。
        /// </summary>
        private void OnSaveLoaded(
            object? sender,
            SaveLoadedEventArgs e)
        {
            // SaveLoaded時点で現在の言語が確定しているため、
            // TsCore内蔵Shortcutの表示名を現在のi18nで更新します。
            ShortcutPanelService.RegisterBuiltInShortcuts();

            //----------------------------------------
            // BigCraftable Extension
            // Sleep継続寝床読込
            //----------------------------------------

            BigCraftableSleepPersistenceService.Load();

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
        // Saving
        //----------------------------------------

        /// <summary>
        /// セーブ実行時に必要な保存前処理を行います。
        /// </summary>
        private void OnSaving(
            object? sender,
            SavingEventArgs e)
        {
            //----------------------------------------
            // BigCraftable Extension
            // Sleep継続寝床保存
            //----------------------------------------

            BigCraftableSleepPersistenceService.Save();
        }

        //----------------------------------------
        // ReturnedToTitle
        //----------------------------------------

        /// <summary>
        /// タイトル画面へ戻った時に、セーブ固有の状態をリセットします。
        /// </summary>
        private void OnReturnedToTitle(
            object? sender,
            ReturnedToTitleEventArgs e)
        {
            //----------------------------------------
            // BigCraftable Extension
            //----------------------------------------

            MachineActionEffectService.Clear();

            MachineActionWobbleService.Clear();

            MachineIdleWobbleService.Clear();

            MachineAnimationTextureService.Clear();

            MachineLightService.Clear();

            BigCraftableSleepActionService.Clear();

            BigCraftableSleepPersistenceService.Clear();

            BigCraftableExtensionTextureService.Clear();

            BigCraftableExtensionDataService.Clear();
        }

        //----------------------------------------
        // TimeChanged
        //----------------------------------------

        /// <summary>
        /// ゲーム内時刻が変化した時に必要な処理を実行します。
        /// </summary>
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

        /// <summary>
        /// プレイヤーが別ロケーションへ移動した時に必要な処理を実行します。
        /// </summary>
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

        /// <summary>
        /// ゲームアセットの読み込み完了時に、対象アセットに必要な処理を行います。
        /// </summary>
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

        /// <summary>
        /// Content Patcherから利用できるT's Core独自トークンを登録します。
        /// </summary>
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
