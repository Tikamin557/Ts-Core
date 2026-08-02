using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Extensions;
using Ts_Core.Models;

namespace Ts_Core.Services.WarpRelated
{
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
        // 登録元一覧
        //----------------------------------------

        private static readonly Dictionary<string, string> ProviderSources
            = new();

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

        static WarpService()
        {
        }

        public static void RegisterProvider(
            WarpProviderModel model,
            string source,
            IMonitor monitor)
        {
            // 重複チェック
            if (ProviderSources.ContainsKey(model.Id))
            {
                monitor.Log(
                    $"Duplicate Warp Provider '{model.Id}' ignored.\n" +
                    $"Already registered by: {ProviderSources[model.Id]}\n" +
                    $"Ignored: {source}",
                    LogLevel.Warn);

                return;
            }

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
                        $"Unknown Warp Provider type '{model.Type}' in '{Path.GetFileName(source)}'.",
                        LogLevel.Warn);

                    return;
            }

            // 登録元を保存
            ProviderSources[model.Id] = source;

            monitor.Log(
                $"Registered Warp Provider '{model.Id}'",
                LogLevel.Trace);
        }

        //----------------------------------------
        // Warp実行
        //----------------------------------------

        public static bool Warp(
            string key,
            bool magic,
            int? facingDirection = null)
        {
            if (!WarpProviders.TryGetValue(key, out var provider))
            {
                return false;
            }

            var destination = provider();

            Warp(
                destination.Location,
                destination.Point,
                magic,
                facingDirection);

            return true;
        }

        public static void Warp(
            string location,
            Point point,
            bool magic,
            int? facingDirection = null)
        {
            WarpInternal(
                location,
                point,
                magic,
                facingDirection
            );
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
        bool magic,
        int? facingDirection = null)
        {
            GameLocation? location = Game1.getLocationFromName(locationName);

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
                magic,
                facingDirection);

            return true;
        }

        //----------------------------------------
        // 内部Warp
        //----------------------------------------

        private static void WarpInternal(
            string location,
            Point point,
            bool magic,
            int? facingDirection)
        {
            if (magic)
            {
                MagicWarp(location, point, facingDirection);
                return;
            }

            int direction = facingDirection ?? Game1.player.FacingDirection;

            Game1.warpFarmer(
                location,
                point.X,
                point.Y,
                direction);
        }

        //----------------------------------------
        // Magic Warp
        //----------------------------------------

        private const int MagicWarpDelay = 1000;

        private static void MagicWarp(
            string location,
            Point point,
            int? facingDirection)
        {
            GameLocation? currentLocation = Game1.currentLocation;
            Farmer player = Game1.player;
            int direction = facingDirection ?? player.FacingDirection;

            if (currentLocation == null)
                return;

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
                        Game1.random.Next((int)player.Position.X - 256,
                                          (int)player.Position.X + 192),
                        Game1.random.Next((int)player.Position.Y - 256,
                                          (int)player.Position.Y + 192)
                    ),
                    flicker: false,
                    Game1.random.NextBool()
                ));
            }

            // 横方向の光エフェクト
            int j2 = 0;
            Point playerTile = player.TilePoint;

            for (int x = playerTile.X + 8; x >= playerTile.X - 8; x--)
            {
                currentLocation.TemporarySprites.Add(
                    new TemporaryAnimatedSprite(
                        6,
                        new Vector2(x, playerTile.Y) * 64f,
                        Color.White,
                        8,
                        flipped: false,
                        50f)
                    {
                        layerDepth = 1f,
                        delayBeforeAnimationStart = j2 * 25,
                        motion = new Vector2(-0.25f, 0f)
                    });

                j2++;
            }

            // 魔法効果音を再生
            currentLocation.playSound("wand");

            // プレイヤー操作を一時停止
            Game1.freezeControls = true;
            Game1.displayFarmer = false;
            player.CanMove = false;

            // 画面を白くフラッシュ
            Game1.flashAlpha = 1f;

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

            }, MagicWarpDelay);
        }
    }
}