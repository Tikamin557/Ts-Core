using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Objects;
using Ts_Core.Models.ContentPatcherRelated;

namespace Ts_Core.Services.ContentPatcherRelated
{
    /// <summary>カスタム床材アイテム専用の画像アセットを提供し、Data/Objectsの参照先を変更します。</summary>
    internal static class CustomFlooringObjectTextureService
    {
        private const string Prefix = "Mods/Tikamin557.TsCore/CustomFlooringObject/";
        private static IModHelper helper = null!;
        private static IMonitor monitor = null!;
        private static bool refreshPending;
        private static string? lastSeason;
        private static int textureRevision;
        private static readonly Dictionary<string, (string Texture, int SpriteIndex, CustomFlooringObjectTexture Patch)> sources = new(StringComparer.OrdinalIgnoreCase);

        internal static void Initialize(IModHelper modHelper, IMonitor modMonitor)
        {
            helper = modHelper;
            monitor = modMonitor;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            helper.Events.Content.AssetRequested += OnAssetRequested;
            helper.Events.Content.AssetsInvalidated += OnAssetsInvalidated;
        }

        /// <summary>CPのコンテンツパック再読み込み後、次の更新フレームで画像の参照を再構築します。</summary>
        internal static void RefreshAfterContentPackReload()
        {
            refreshPending = true;
        }

        /// <summary>CPのリロード処理が完了してから、依存するアセットを順番に更新します。</summary>
        private static void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady)
            {
                lastSeason = null;
                return;
            }

            // 季節変更時にもCPの最新のData/Objectsとアイテム画像を読み直します。
            // 初回ロード時は既存の読み込み処理に任せ、季節が変わった場合だけ更新します。
            string season = Game1.currentSeason;
            if (lastSeason == null)
                lastSeason = season;
            else if (!string.Equals(lastSeason, season, StringComparison.OrdinalIgnoreCase))
            {
                lastSeason = season;
                refreshPending = true;
                // 季節変更を検出したフレームではCPの再適用完了を待ちます。
                return;
            }

            if (!refreshPending)
                return;

            refreshPending = false;
            // 既存のTexture2Dを参照するゲーム側キャッシュを避けるため、再読み込みごとに専用アセット名を更新します。
            textureRevision++;
            try
            {
                // 古い定義とData/Objectsを破棄し、最新のCPパッチで再構築します。
                helper.GameContent.InvalidateCache(CustomFlooringDataService.AssetName);
                helper.GameContent.InvalidateCache("Data/Objects");
                Game1.content.Load<Dictionary<string, ObjectData>>("Data/Objects");

                // ロケール付きで読み込まれた画像も対象にして破棄します。
                // 生成画像はゲーム側から .ja-JP などの名前で要求される場合があります。
                string[] assets = sources.Keys.ToArray();
                foreach (string asset in assets)
                {
                    InvalidateGeneratedTexture(asset);
                    // 無効化だけで終わらせず、新しい定義で画像を再生成します。
                    helper.GameContent.Load<Texture2D>(asset);
                }

                // ItemRegistryが保持するParsedItemDataの画像参照も破棄します。
                // Data/Objectsと専用画像の再読み込み後に実行し、既存アイテムの描画にも反映させます。
                ItemRegistry.ResetCache();


                monitor.Log($"Custom flooring object textures refreshed ({assets.Length} item(s)); item registry cache reset.", LogLevel.Trace);
            }
            catch (Exception ex)
            {
                monitor.Log($"Failed to refresh custom flooring object textures after CP reload: {ex}", LogLevel.Warn);
            }
        }

        private static void OnAssetsInvalidated(object? sender, AssetsInvalidatedEventArgs e)
        {
            bool definitionsChanged = e.NamesWithoutLocale.Any(n => n.IsEquivalentTo(CustomFlooringDataService.AssetName) || n.IsEquivalentTo("Data/FloorsAndPaths"));
            bool objectsChanged = e.NamesWithoutLocale.Any(n => n.IsEquivalentTo("Data/Objects"));
            if (definitionsChanged || objectsChanged)
            {
                foreach (string asset in sources.Keys.ToArray())
                    InvalidateGeneratedTexture(asset);
                // SMAPI may request older revision assets again when returning to the title screen.
                // Keep their source definitions so those reload requests can still be served.
                if (definitionsChanged && !objectsChanged)
                    helper.GameContent.InvalidateCache("Data/Objects");
            }
            else
            {
                foreach (var source in sources)
                    if (e.NamesWithoutLocale.Any(n => n.IsEquivalentTo(source.Value.Texture) || n.IsEquivalentTo(source.Value.Patch.Texture)))
                        InvalidateGeneratedTexture(source.Key);
            }
        }

        /// <summary>言語指定の有無にかかわらず、生成した床材アイテム画像のキャッシュを破棄します。</summary>
        private static void InvalidateGeneratedTexture(string assetName)
        {
            helper.GameContent.InvalidateCache(info => info.Name.IsEquivalentTo(assetName));
        }

        private static void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Objects"))
            {
                e.Edit(asset =>
                {
                    var objects = asset.AsDictionary<string, ObjectData>().Data;
                    var definitions = Game1.content.Load<Dictionary<string, CustomFlooringModel>>(CustomFlooringDataService.AssetName);
                    var floors = Game1.content.Load<Dictionary<string, StardewValley.GameData.FloorsAndPaths.FloorPathData>>("Data/FloorsAndPaths");
                    foreach (var floor in floors)
                    {
                        if (!definitions.TryGetValue(floor.Key, out var definition) || definition?.ObjectTexture is not { } patch || string.IsNullOrWhiteSpace(patch.Texture) || string.IsNullOrWhiteSpace(floor.Value?.ItemId))
                            continue;
                        string itemId = floor.Value.ItemId;
                        if (!objects.TryGetValue(itemId, out var data))
                            continue;
                        string assetName = Prefix + itemId + "/reload-" + textureRevision;
                        sources[assetName] = (data.Texture ?? "Maps/springobjects", data.SpriteIndex, patch);

                        data.Texture = assetName;
                        // 元の画像シート上の位置を維持し、ゲーム側のソース矩形計算と一致させます。
                        // 専用アセットには元のシートと同じ寸法の画像を使用します。
                    }
                }, AssetEditPriority.Late);
                return;
            }

            string requested = e.NameWithoutLocale.Name;
            if (!sources.TryGetValue(requested, out var source))
                return;
            // 元のシート全体を独立したアセットとして複製し、対象の16x16領域だけを変更します。
            // 独自のRGBA計算を避け、透明ピクセルとOverlayの挙動をEditImageと揃えます。
            e.LoadFrom(() => CreateBaseTexture(source), AssetLoadPriority.Medium);
            e.Edit(asset =>
            {
                Texture2D overlay = Game1.content.Load<Texture2D>(source.Patch.Texture);
                var mode = string.Equals(source.Patch.PatchMode, "Overlay", StringComparison.OrdinalIgnoreCase)
                    ? PatchMode.Overlay : PatchMode.Replace;
                Texture2D destination = asset.AsImage().Data;
                int columns = destination.Width / 16;
                Rectangle target = new(source.SpriteIndex % columns * 16, source.SpriteIndex / columns * 16, 16, 16);


                asset.AsImage().PatchImage(overlay,
                    new Rectangle(source.Patch.X, source.Patch.Y, 16, 16), target, mode);

            }, AssetEditPriority.Late);
        }

        /// <summary>元のアイテム画像シートを独立したテクスチャとしてコピーします。</summary>
        private static Texture2D CreateBaseTexture((string Texture, int SpriteIndex, CustomFlooringObjectTexture Patch) source)
        {
            Texture2D original = Game1.content.Load<Texture2D>(source.Texture);
            int columns = original.Width / 16;
            if (columns == 0 || source.SpriteIndex < 0 || (source.SpriteIndex / columns + 1) * 16 > original.Height)
                throw new ArgumentOutOfRangeException(nameof(source), "Custom flooring ObjectTexture base region is outside the image.");

            Color[] pixels = new Color[original.Width * original.Height];
            original.GetData(pixels);
            Texture2D texture = new(Game1.graphics.GraphicsDevice, original.Width, original.Height);
            texture.SetData(pixels);
            return texture;
        }
    }
}
