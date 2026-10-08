using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Machines;
using Ts_Core.Models.BigCraftableRelated;

namespace Ts_Core.Services.BigCraftableRelated
{
    /// <summary>
    /// BigCraftable Extension用の
    /// Data Assetを管理するサービスです。
    /// </summary>
    public static class BigCraftableExtensionDataService
    {
        //----------------------------------------
        // Constants
        //----------------------------------------

        /// <summary>
        /// BigCraftable Extension用Data Assetの名前です。
        /// </summary>
        public const string AssetName =
            "TsCore/BigCraftableExtension";

        /// <summary>
        /// Data/Machines側で
        /// BigCraftable Extension IDを指定するCustomField名です。
        /// </summary>
        public const string ExtensionField =
            "TsCore/BigCraftableExtension";

        /// <summary>
        /// BigCraftable Extensionで使用する
        /// InteractMethodです。
        /// </summary>
        private const string InteractMethod =
            "Ts_Core.Services.BigCraftableRelated.BigCraftableExtensionService, Ts_Core: Interact";

        //----------------------------------------
        // Collision Size
        //----------------------------------------

        /// <summary>
        /// 現在のBigCraftable Extension Dataで使用される
        /// 最大Collision Widthです。
        /// </summary>
        public static int MaxCollisionWidth
        {
            get;
            private set;
        } = 1;

        /// <summary>
        /// 現在のBigCraftable Extension Dataで使用される
        /// 最大Collision Heightです。
        /// </summary>
        public static int MaxCollisionHeight
        {
            get;
            private set;
        } = 1;

        //----------------------------------------
        // Texture Size
        //----------------------------------------

        /// <summary>
        /// 現在のBigCraftable Extension Dataで使用される
        /// 最大Texture Widthです。
        /// </summary>
        public static int MaxTextureWidth
        {
            get;
            private set;
        } = 16;

        //----------------------------------------
        // Tile Property Range
        //----------------------------------------

        /// <summary>
        /// 現在のBigCraftable Extension Dataで使用される
        /// Tile Propertyの最小X Offsetです。
        /// </summary>
        public static int MinTilePropertyOffsetX
        {
            get;
            private set;
        } = 0;

        /// <summary>
        /// 現在のBigCraftable Extension Dataで使用される
        /// Tile Propertyの最大X Offsetです。
        /// </summary>
        public static int MaxTilePropertyOffsetX
        {
            get;
            private set;
        } = 0;

        /// <summary>
        /// 現在のBigCraftable Extension Dataで使用される
        /// Tile Propertyの最小Y Offsetです。
        /// </summary>
        public static int MinTilePropertyOffsetY
        {
            get;
            private set;
        } = 0;

        /// <summary>
        /// 現在のBigCraftable Extension Dataで使用される
        /// Tile Propertyの最大Y Offsetです。
        /// </summary>
        public static int MaxTilePropertyOffsetY
        {
            get;
            private set;
        } = 0;

        //----------------------------------------
        // Asset Requested
        //----------------------------------------

        /// <summary>
        /// BigCraftable Extension関連Assetが
        /// 要求された時の処理です。
        /// </summary>
        public static void OnAssetRequested(
            object? sender,
            AssetRequestedEventArgs e)
        {
            //----------------------------------------
            // BigCraftable Extension Data Asset
            //----------------------------------------

            if (e.NameWithoutLocale.IsEquivalentTo(
                AssetName))
            {
                e.LoadFrom(
                    () =>
                        new Dictionary<
                            string,
                            BigCraftableExtensionData>(),
                    AssetLoadPriority.Exclusive);

                return;
            }

            //----------------------------------------
            // Data/Machines
            //----------------------------------------

            if (!e.NameWithoutLocale.IsEquivalentTo(
                "Data/Machines"))
            {
                return;
            }

            e.Edit(
                asset =>
                {
                    IDictionary<string, MachineData> machines =
                        asset.AsDictionary<
                            string,
                            MachineData>().Data;

                    //----------------------------------------
                    // Machine確認
                    //----------------------------------------

                    foreach (KeyValuePair<
                        string,
                        MachineData> entry
                        in machines)
                    {
                        MachineData machineData =
                            entry.Value;

                        if (machineData.CustomFields == null)
                            continue;

                        //----------------------------------------
                        // BigCraftable Extension確認
                        //----------------------------------------

                        if (!machineData.CustomFields.ContainsKey(
                            ExtensionField))
                        {
                            continue;
                        }

                        //----------------------------------------
                        // 既存のInteractMethodを優先
                        //----------------------------------------

                        if (!string.IsNullOrWhiteSpace(
                            machineData.InteractMethod))
                        {
                            continue;
                        }

                        //----------------------------------------
                        // TsCore InteractMethodを自動設定
                        //----------------------------------------

                        machineData.InteractMethod =
                            InteractMethod;
                    }
                },
                AssetEditPriority.Late);
        }

        //----------------------------------------
        // Asset Ready
        //----------------------------------------

        /// <summary>
        /// BigCraftable Extension Data Assetの
        /// 読み込み完了時に最大Collision Size、
        /// 最大Texture Width、
        /// Tile Property範囲を更新します。
        /// </summary>
        public static void OnAssetReady(
            object? sender,
            AssetReadyEventArgs e)
        {
            //----------------------------------------
            // Asset確認
            //----------------------------------------

            if (!e.NameWithoutLocale.IsEquivalentTo(
                AssetName))
            {
                return;
            }

            //----------------------------------------
            // Extension Data取得
            //----------------------------------------

            Dictionary<string, BigCraftableExtensionData>
                extensions =
                    Game1.content.Load<
                        Dictionary<
                            string,
                            BigCraftableExtensionData>>(
                                AssetName);

            //----------------------------------------
            // 初期値
            //----------------------------------------

            int maxWidth =
                1;

            int maxHeight =
                1;

            int maxTextureWidth =
                16;

            int minTilePropertyOffsetX =
                0;

            int maxTilePropertyOffsetX =
                0;

            int minTilePropertyOffsetY =
                0;

            int maxTilePropertyOffsetY =
                0;

            //----------------------------------------
            // 最大Collision Size・Texture Width・
            // Tile Property範囲取得
            //----------------------------------------

            foreach (BigCraftableExtensionData extension
                in extensions.Values)
            {
                //----------------------------------------
                // Collision Width
                //----------------------------------------

                maxWidth =
                    Math.Max(
                        maxWidth,
                        Math.Max(
                            1,
                            extension.CollisionWidth));

                //----------------------------------------
                // Collision Height
                //----------------------------------------

                maxHeight =
                    Math.Max(
                        maxHeight,
                        Math.Max(
                            1,
                            extension.CollisionHeight));

                //----------------------------------------
                // Texture Width
                //----------------------------------------

                int textureWidth =
                    extension.TextureWidth > 0
                        ? extension.TextureWidth
                        : 16;

                maxTextureWidth =
                    Math.Max(
                        maxTextureWidth,
                        textureWidth);

                //----------------------------------------
                // Tile Property Range
                //----------------------------------------

                if (extension.TileProperties != null)
                {
                    foreach (BigCraftableTilePropertyData property
                        in extension.TileProperties)
                    {
                        foreach (Point offset
                            in property.TileOffsets)
                        {
                            minTilePropertyOffsetX =
                                Math.Min(
                                    minTilePropertyOffsetX,
                                    offset.X);

                            maxTilePropertyOffsetX =
                                Math.Max(
                                    maxTilePropertyOffsetX,
                                    offset.X);

                            minTilePropertyOffsetY =
                                Math.Min(
                                    minTilePropertyOffsetY,
                                    offset.Y);

                            maxTilePropertyOffsetY =
                                Math.Max(
                                    maxTilePropertyOffsetY,
                                    offset.Y);
                        }
                    }
                }
            }

            //----------------------------------------
            // 更新
            //----------------------------------------

            MaxCollisionWidth =
                maxWidth;

            MaxCollisionHeight =
                maxHeight;

            MaxTextureWidth =
                maxTextureWidth;

            MinTilePropertyOffsetX =
                minTilePropertyOffsetX;

            MaxTilePropertyOffsetX =
                maxTilePropertyOffsetX;

            MinTilePropertyOffsetY =
                minTilePropertyOffsetY;

            MaxTilePropertyOffsetY =
                maxTilePropertyOffsetY;
        }

        //----------------------------------------
        // Extension Data
        //----------------------------------------

        /// <summary>
        /// 指定されたBigCraftableに設定されている
        /// BigCraftable Extensionデータを取得します。
        /// </summary>
        public static bool TryGetExtensionData(
            StardewValley.Object obj,
            out string extensionId,
            out BigCraftableExtensionData extensionData)
        {
            extensionId =
                string.Empty;

            extensionData =
                null!;

            //----------------------------------------
            // BigCraftable確認
            //----------------------------------------

            if (!obj.bigCraftable.Value)
            {
                return false;
            }

            //----------------------------------------
            // Item IDから取得
            //----------------------------------------

            return TryGetExtensionData(
                obj.ItemId,
                out extensionId,
                out extensionData);
        }

        /// <summary>
        /// 指定されたBigCraftable Item IDに設定されている
        /// BigCraftable Extensionデータを取得します。
        /// </summary>
        public static bool TryGetExtensionData(
            string bigCraftableId,
            out string extensionId,
            out BigCraftableExtensionData extensionData)
        {
            extensionId =
                string.Empty;

            extensionData =
                null!;

            //----------------------------------------
            // Item ID確認
            //----------------------------------------

            if (string.IsNullOrWhiteSpace(
                bigCraftableId))
            {
                return false;
            }

            //----------------------------------------
            // Qualified ID対応
            //----------------------------------------

            if (bigCraftableId.StartsWith(
                "(BC)",
                StringComparison.Ordinal))
            {
                bigCraftableId =
                    bigCraftableId.Substring(
                        4);
            }

            //----------------------------------------
            // BigCraftable Data取得
            //----------------------------------------

            if (!Game1.bigCraftableData.TryGetValue(
                bigCraftableId,
                out var bigCraftableData))
            {
                return false;
            }

            if (bigCraftableData.CustomFields == null)
            {
                return false;
            }

            //----------------------------------------
            // Extension ID取得
            //----------------------------------------

            if (!bigCraftableData.CustomFields.TryGetValue(
                ExtensionField,
                out string? foundExtensionId))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(
                foundExtensionId))
            {
                return false;
            }

            //----------------------------------------
            // Extension Data Asset取得
            //----------------------------------------

            Dictionary<string, BigCraftableExtensionData>
                extensions =
                    Game1.content.Load<
                        Dictionary<
                            string,
                            BigCraftableExtensionData>>(
                                AssetName);

            //----------------------------------------
            // Extension Data取得
            //----------------------------------------

            if (!extensions.TryGetValue(
                foundExtensionId,
                out BigCraftableExtensionData? foundExtension)
                || foundExtension == null)
            {
                return false;
            }

            extensionId =
                foundExtensionId;

            extensionData =
                foundExtension;

            return true;
        }

        //----------------------------------------
        // Clear
        //----------------------------------------

        /// <summary>
        /// BigCraftable Extensionの
        /// Size・Tile Property範囲情報を初期化します。
        /// </summary>
        public static void Clear()
        {
            MaxCollisionWidth =
                1;

            MaxCollisionHeight =
                1;

            MaxTextureWidth =
                16;

            MinTilePropertyOffsetX =
                0;

            MaxTilePropertyOffsetX =
                0;

            MinTilePropertyOffsetY =
                0;

            MaxTilePropertyOffsetY =
                0;
        }
    }
}