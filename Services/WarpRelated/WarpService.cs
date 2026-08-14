using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Extensions;
using Ts_Core.Models;

namespace Ts_Core.Services.WarpRelated
{
    /// <summary>
    /// 登録済みWarp Providerのデバッグ情報です。
    /// </summary>
    internal sealed class RegisteredWarpProviderInfo
    {
        /// <summary>
        /// Provider IDです。
        /// </summary>
        public string Id { get; init; } = "";

        /// <summary>
        /// Providerを登録したModまたはContent Packです。
        /// </summary>
        public string Owner { get; init; } = "";

        /// <summary>
        /// Provider定義ファイルのパスです。
        /// </summary>
        public string SourceFile { get; init; } = "";

        /// <summary>
        /// Providerの種類です。
        /// </summary>
        public string Type { get; init; } = "";

        /// <summary>
        /// Warp検索元のLocation名です。
        /// </summary>
        public string? SourceLocation { get; init; }

        /// <summary>
        /// Warpの移動先Location名です。
        /// </summary>
        public string? TargetLocation { get; init; }

        /// <summary>
        /// 検索対象の建物タイプです。
        /// </summary>
        public string? BuildingType { get; init; }

        /// <summary>
        /// 建物座標から加算するX座標です。
        /// </summary>
        public int OffsetX { get; init; }

        /// <summary>
        /// 建物座標から加算するY座標です。
        /// </summary>
        public int OffsetY { get; init; }

        /// <summary>
        /// 建物が見つからなかった場合に使用するProviderです。
        /// </summary>
        public string? Fallback { get; init; }
    }

    /// <summary>
    /// Warp時に使用する演出の種類です。
    /// </summary>
    public enum WarpEffectMode
    {
        /// <summary>
        /// 通常Warp。
        /// </summary>
        None,

        /// <summary>
        /// 通常のMagic Warp演出。
        /// </summary>
        Magic,

        /// <summary>
        /// 簡易Magic Warp演出。
        /// </summary>
        MagicSimple
    }

    /// <summary>
    /// Warp Providerや座標を使用したワープ処理を実行するサービスです。
    /// </summary>
    public static class WarpService
    {
        //----------------------------------------
        // Monitor
        //----------------------------------------

        private static IMonitor? Monitor;

        //----------------------------------------
        // 初期化
        //----------------------------------------

        public static void Initialize(IMonitor monitor)
        {
            Monitor = monitor;
        }

        //----------------------------------------
        // 登録済みProvider情報
        //----------------------------------------

        private static readonly Dictionary<string, RegisteredWarpProviderInfo>
            RegisteredProviders =
                new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 現在登録されているWarp Provider情報を取得します。
        /// </summary>
        internal static IReadOnlyList<RegisteredWarpProviderInfo>
            GetRegisteredProviders()
        {
            return RegisteredProviders.Values
                .OrderBy(provider => provider.Owner)
                .ThenBy(provider => provider.Id)
                .ToList();
        }

        //----------------------------------------
        // Provider再読み込み
        //----------------------------------------

        /// <summary>
        /// 登録済みのWarp Providerをすべて削除します。
        /// 再読み込み前に使用します。
        /// </summary>
        internal static void ClearProviders()
        {
            WarpProviders.Clear();
            RegisteredProviders.Clear();
        }

        //----------------------------------------
        // Warp取得
        //----------------------------------------

        private static Warp GetWarp(
            GameLocation location,
            Func<Warp, bool> predicate)
        {
            return location.warps.FirstOrDefault(predicate)
                ?? throw new InvalidOperationException(
                    $"No matching warp was found in '{location.Name}'.");
        }

        private static (string Location, Point Point) GetWarpDestination(
                string sourceLocation,
                string targetLocation)
        {
            Warp warp = GetWarp(
                Game1.getLocationFromName(sourceLocation),
                w => w.TargetName == targetLocation);

            return (
                warp.TargetName,
                new Point(warp.TargetX, warp.TargetY)
            );
        }

        //----------------------------------------
        // 登録済みProvider
        //----------------------------------------

        private static readonly Dictionary<string, Func<(string Location, Point Point)>> WarpProviders
            = new();

        //----------------------------------------
        // Provider登録
        //----------------------------------------

        private static void AddProvider(
            string key,
            Func<(string Location, Point Point)> provider)
        {
            WarpProviders[key] = provider;
        }

        /// <summary>
        /// Warp Providerを登録します。
        /// </summary>
        public static void RegisterProvider(
            WarpProviderModel model,
            string owner,
            string sourceFile,
            IMonitor monitor)
        {
            //----------------------------------------
            // 重複チェック
            //----------------------------------------

            if (RegisteredProviders.TryGetValue(
                    model.Id,
                    out RegisteredWarpProviderInfo? existingProvider))
            {
                monitor.Log(
                    $"Duplicate Warp Provider '{model.Id}' ignored.\n" +
                    $"Already registered by: {existingProvider.Owner}\n" +
                    $"Existing file: {existingProvider.SourceFile}\n" +
                    $"Ignored provider owner: {owner}\n" +
                    $"Ignored file: {sourceFile}",
                    LogLevel.Warn);

                return;
            }

            //----------------------------------------
            // Provider登録
            //----------------------------------------

            switch (model.Type)
            {
                case "Warp":

                    AddProvider(
                        model.Id,
                        () => GetWarpDestination(
                            model.Source!,
                            model.Target!));

                    break;

                case "Building":

                    RegisterBuildingWarp(
                        model.Id,
                        model.BuildingType!,
                        model.OffsetX,
                        model.OffsetY,
                        model.Fallback ?? "FarmHouseFront");

                    break;

                default:

                    monitor.Log(
                        $"Unknown Warp Provider type '{model.Type}' " +
                        $"in '{Path.GetFileName(sourceFile)}'.",
                        LogLevel.Warn);

                    return;
            }

            //----------------------------------------
            // Provider情報を保存
            //----------------------------------------

            RegisteredProviders[model.Id] =
                new RegisteredWarpProviderInfo
                {
                    Id = model.Id,
                    Owner = owner,
                    SourceFile = sourceFile,
                    Type = model.Type,

                    SourceLocation = model.Source,
                    TargetLocation = model.Target,

                    BuildingType = model.BuildingType,
                    OffsetX = model.OffsetX,
                    OffsetY = model.OffsetY,

                    Fallback = model.Fallback
                };

            monitor.Log(
                $"Registered Warp Provider '{model.Id}' " +
                $"from '{owner}'.",
                LogLevel.Trace);
        }

        //----------------------------------------
        // Warp実行
        //----------------------------------------

        /// <summary>
        /// Warp Providerを使用してWarpします。
        /// </summary>
        public static bool Warp(
            string key,
            WarpEffectMode effectMode,
            int? facingDirection = null,
            string? audioCue = null)
        {
            if (!WarpProviders.TryGetValue(
                    key,
                    out Func<(string Location, Point Point)>? provider))
            {
                return false;
            }

            var destination = provider();

            Warp(
                destination.Location,
                destination.Point,
                effectMode,
                facingDirection,
                audioCue);

            return true;
        }

        /// <summary>
        /// 指定座標へWarpします。
        /// </summary>
        public static void Warp(
            string location,
            Point point,
            WarpEffectMode effectMode,
            int? facingDirection = null,
            string? audioCue = null)
        {
            WarpInternal(
                location,
                point,
                effectMode,
                facingDirection,
                audioCue);
        }

        //----------------------------------------
        // 互換用 overload
        //----------------------------------------

        /// <summary>
        /// 従来のbool指定によるWarpです。
        /// </summary>
        public static bool Warp(
            string key,
            bool magic,
            int? facingDirection = null)
        {
            return Warp(
                key,
                magic
                    ? WarpEffectMode.Magic
                    : WarpEffectMode.None,
                facingDirection);
        }

        /// <summary>
        /// 従来のbool指定によるWarpです。
        /// </summary>
        public static void Warp(
            string location,
            Point point,
            bool magic,
            int? facingDirection = null)
        {
            Warp(
                location,
                point,
                magic
                    ? WarpEffectMode.Magic
                    : WarpEffectMode.None,
                facingDirection);
        }

        //----------------------------------------
        // Building Warp
        //----------------------------------------

        private static void RegisterBuildingWarp(
            string key,
            string buildingType,
            int xoffset,
            int yoffset,
            string fallback = "FarmHouseFront")
        {
            AddProvider(
                key,
                () =>
                {
                    Farm farm = Game1.getFarm();

                    Building? building = farm.buildings
                        .FirstOrDefault(b => b.buildingType.Value == buildingType);

                    if (building != null)
                    {
                        return (
                            "Farm",
                            new Point(
                                building.tileX.Value + xoffset,
                                building.tileY.Value + yoffset
                            )
                        );
                    }

                    if (WarpProviders.TryGetValue(fallback, out var fallbackProvider))
                        return fallbackProvider();

                    throw new InvalidOperationException(
                        $"Fallback warp '{fallback}' was not found.");
                }
            );
        }

        //----------------------------------------
        // Map Warp
        //----------------------------------------

        public static bool WarpToMap(
            string locationName,
            WarpEffectMode effectMode,
            int? facingDirection = null,
            string? audioCue = null)
        {
            GameLocation? location =
                Game1.getLocationFromName(locationName);

            if (location == null)
                return false;

            int x = 0;
            int y = 0;

            Utility.getDefaultWarpLocation(
                locationName,
                ref x,
                ref y);

            if (x == 0 && y == 0)
            {
                Monitor?.Log(
                    $"WarpToMap failed: '{locationName}' has no default warp location.",
                    LogLevel.Warn);

                return false;
            }

            Warp(
                location.Name,
                new Point(x, y),
                effectMode,
                facingDirection,
                audioCue);

            return true;
        }

        //----------------------------------------
        // 互換用 overload
        //----------------------------------------

        public static bool WarpToMap(
            string locationName,
            bool magic,
            int? facingDirection = null)
        {
            return WarpToMap(
                locationName,
                magic
                    ? WarpEffectMode.Magic
                    : WarpEffectMode.None,
                facingDirection);
        }

        //----------------------------------------
        // 内部Warp
        //----------------------------------------

        private static void WarpInternal(
     string location,
     Point point,
     WarpEffectMode effectMode,
     int? facingDirection,
     string? audioCue)
        {
            switch (effectMode)
            {
                //----------------------------------------
                // Magic Warp
                //----------------------------------------

                case WarpEffectMode.Magic:

                    MagicWarp(
                        location,
                        point,
                        facingDirection,
                        simple: false,
                        audioCue);

                    return;

                //----------------------------------------
                // Simple Magic Warp
                //----------------------------------------

                case WarpEffectMode.MagicSimple:

                    MagicWarp(
                        location,
                        point,
                        facingDirection,
                        simple: true,
                        audioCue);

                    return;

                //----------------------------------------
                // Normal Warp
                //----------------------------------------

                default:

                    int direction =
                        facingDirection
                        ?? Game1.player.FacingDirection;

                    // Audio Cueが指定されている場合のみ再生
                    if (!string.IsNullOrWhiteSpace(audioCue))
                    {
                        Game1.currentLocation?.playSound(
                            audioCue);
                    }

                    Game1.warpFarmer(
                        location,
                        point.X,
                        point.Y,
                        direction);

                    return;
            }
        }

        //----------------------------------------
        // Magic Warp
        //----------------------------------------

        private const int MagicWarpDelay = 1000;
        private const int SimpleMagicWarpDelay = 500;

        private static void MagicWarp(
            string location,
            Point point,
            int? facingDirection,
            bool simple,
            string? audioCue)
        {
            GameLocation? currentLocation =
                Game1.currentLocation;

            Farmer player =
                Game1.player;

            int direction =
                facingDirection
                ?? player.FacingDirection;

            if (currentLocation == null)
                return;

            //----------------------------------------
            // Full Magic Warp専用エフェクト
            //----------------------------------------

            if (!simple)
            {
                // プレイヤー周囲に魔法エフェクトを表示
                for (int j = 0; j < 12; j++)
                {
                    currentLocation.TemporarySprites.Add(
                        new TemporaryAnimatedSprite(
                            354,
                            Game1.random.Next(25, 75),
                            6,
                            1,
                            new Vector2(
                                Game1.random.Next(
                                    (int)player.Position.X - 256,
                                    (int)player.Position.X + 192),
                                Game1.random.Next(
                                    (int)player.Position.Y - 256,
                                    (int)player.Position.Y + 192)
                            ),
                            flicker: false,
                            Game1.random.NextBool()
                        ));
                }

                // 横方向の光エフェクト
                int j2 = 0;
                Point playerTile =
                    player.TilePoint;

                for (int x = playerTile.X + 8;
                     x >= playerTile.X - 8;
                     x--)
                {
                    currentLocation.TemporarySprites.Add(
                        new TemporaryAnimatedSprite(
                            6,
                            new Vector2(
                                x,
                                playerTile.Y) * 64f,
                            Color.White,
                            8,
                            flipped: false,
                            50f)
                        {
                            layerDepth = 1f,
                            delayBeforeAnimationStart =
                                j2 * 25,
                            motion =
                                new Vector2(-0.25f, 0f)
                        });

                    j2++;
                }
            }

            //----------------------------------------
            // 共通Magic Warp演出
            //----------------------------------------

            // Warp効果音
            string warpSound =
                string.IsNullOrWhiteSpace(audioCue)
                    ? "wand"
                    : audioCue;

            currentLocation.playSound(
                warpSound);

            // プレイヤー操作を一時停止
            Game1.freezeControls = true;
            player.CanMove = false;

            if (simple)
            {
                // 移動を停止して立ち状態に戻す
                player.Halt();
                player.FarmerSprite.StopAnimation();
            }
            else
            {
                Game1.displayFarmer = false;
            }

            // 画面を白くフラッシュ
            Game1.flashAlpha = 1f;

            int warpDelay =
                simple
                    ? SimpleMagicWarpDelay
                    : MagicWarpDelay;

            // 一定時間後にWarp
            DelayedAction.fadeAfterDelay(() =>
            {
                Game1.warpFarmer(
                    location,
                    point.X,
                    point.Y,
                    direction);

                // プレイヤー操作を再開
                Game1.fadeToBlackAlpha = 0.99f;
                Game1.screenGlow = false;
                Game1.displayFarmer = true;
                player.CanMove = true;
                Game1.freezeControls = false;

            }, warpDelay);
        }
    }
}