using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Objects;
using Ts_Core.Models.BigCraftableRelated;

namespace Ts_Core.Services.BigCraftableRelated
{
    /// <summary>
    /// BigCraftable Extensionの
    /// 通常Texture情報を取得するServiceです。
    /// </summary>
    public static class BigCraftableExtensionTextureService
    {
        //----------------------------------------
        // Monitor
        //----------------------------------------

        /// <summary>
        /// Log出力に使用するMonitorです。
        /// </summary>
        private static IMonitor? Monitor;

        //----------------------------------------
        // Logged Warnings
        //----------------------------------------

        /// <summary>
        /// すでに表示した警告を管理します。
        /// </summary>
        private static readonly HashSet<string>
            LoggedWarnings =
                new();

        //----------------------------------------
        // Initialize
        //----------------------------------------

        /// <summary>
        /// Serviceを初期化します。
        /// </summary>
        public static void Initialize(
            IMonitor monitor)
        {
            Monitor =
                monitor;
        }

        //----------------------------------------
        // GetTextureSize
        //----------------------------------------

        /// <summary>
        /// BigCraftable Extensionで使用する
        /// 通常Texture Sizeを取得します。
        /// 不正な値はVanilla SizeへFallbackします。
        /// </summary>
        public static void GetTextureSize(
            StardewValley.Object obj,
            BigCraftableExtensionData extension,
            out int width,
            out int height)
        {
            width =
                extension.TextureWidth;

            height =
                extension.TextureHeight;

            //----------------------------------------
            // Texture Width
            //----------------------------------------

            if (width <= 0)
            {
                LogWarningOnce(
                    $"{obj.ItemId}:InvalidTextureWidth",
                    $"BigCraftable Extension for '{obj.ItemId}': "
                    + $"TextureWidth {width} is invalid. "
                    + "TextureWidth must be greater than 0. "
                    + "The vanilla width 16 will be used instead.");

                width =
                    16;
            }

            //----------------------------------------
            // Texture Height
            //----------------------------------------

            if (height <= 0)
            {
                LogWarningOnce(
                    $"{obj.ItemId}:InvalidTextureHeight",
                    $"BigCraftable Extension for '{obj.ItemId}': "
                    + $"TextureHeight {height} is invalid. "
                    + "TextureHeight must be greater than 0. "
                    + "The vanilla height 32 will be used instead.");

                height =
                    32;
            }
        }

        //----------------------------------------
        // GetTexture
        //----------------------------------------

        /// <summary>
        /// BigCraftable Extensionで使用する
        /// 通常Textureを取得します。
        /// </summary>
        public static Texture2D GetTexture(
            StardewValley.Object obj,
            BigCraftableExtensionData extension)
        {
            //----------------------------------------
            // Extension Texture
            //----------------------------------------

            if (!string.IsNullOrWhiteSpace(
                extension.Texture))
            {
                try
                {
                    return Game1.content.Load<Texture2D>(
                        extension.Texture);
                }
                catch (Exception ex)
                {
                    LogWarningOnce(
                        $"{obj.ItemId}:TextureLoad",
                        $"BigCraftable Extension for '{obj.ItemId}': "
                        + $"failed to load Texture "
                        + $"'{extension.Texture}'. "
                        + "The vanilla texture will be used instead. "
                        + $"({ex.Message})");
                }
            }

            //----------------------------------------
            // Vanilla Texture
            //----------------------------------------

            ParsedItemData itemData =
                ItemRegistry.GetDataOrErrorItem(
                    obj.QualifiedItemId);

            return itemData.GetTexture();
        }

        //----------------------------------------
        // GetSourceRect
        //----------------------------------------

        /// <summary>
        /// BigCraftable Extensionで使用する
        /// 通常TextureのSource Rectangleを取得します。
        /// </summary>
        public static Rectangle GetSourceRect(
            StardewValley.Object obj,
            BigCraftableExtensionData extension)
        {
            //----------------------------------------
            // Texture Size
            //----------------------------------------

            GetTextureSize(
                obj,
                extension,
                out int textureWidth,
                out int textureHeight);

            //----------------------------------------
            // Vanilla Data
            //----------------------------------------

            ParsedItemData itemData =
                ItemRegistry.GetDataOrErrorItem(
                    obj.QualifiedItemId);

            //----------------------------------------
            // Vanilla Source Rectangle
            //----------------------------------------

            int offset =
                0;

            if (obj is Mannequin)
            {
                offset =
                    2;
            }

            Rectangle vanillaSourceRect =
                itemData.GetSourceRect(
                    offset,
                    obj.ParentSheetIndex);

            //----------------------------------------
            // Texture Position未指定
            //----------------------------------------

            if (string.IsNullOrWhiteSpace(
                extension.TexturePosition))
            {
                vanillaSourceRect.Width =
                    textureWidth;

                vanillaSourceRect.Height =
                    textureHeight;

                return vanillaSourceRect;
            }

            //----------------------------------------
            // Texture Position解析
            //----------------------------------------

            if (!TryParseTexturePosition(
                extension.TexturePosition,
                out Point texturePosition))
            {
                LogWarningOnce(
                    $"{obj.ItemId}:InvalidTexturePosition",
                    $"BigCraftable Extension for '{obj.ItemId}': "
                    + $"TexturePosition "
                    + $"'{extension.TexturePosition}' "
                    + "has an invalid format. "
                    + "Expected format: \"X, Y\". "
                    + "The vanilla texture position will be used instead.");

                vanillaSourceRect.Width =
                    textureWidth;

                vanillaSourceRect.Height =
                    textureHeight;

                return vanillaSourceRect;
            }

            //----------------------------------------
            // Custom Source Rectangle
            //----------------------------------------

            Rectangle customSourceRect =
                new Rectangle(
                    texturePosition.X,
                    texturePosition.Y,
                    textureWidth,
                    textureHeight);

            //----------------------------------------
            // Texture取得
            //----------------------------------------

            Texture2D texture =
                GetTexture(
                    obj,
                    extension);

            //----------------------------------------
            // Texture範囲確認
            //----------------------------------------

            if (customSourceRect.X < 0
                || customSourceRect.Y < 0
                || customSourceRect.Right > texture.Width
                || customSourceRect.Bottom > texture.Height)
            {
                LogWarningOnce(
                    $"{obj.ItemId}:TextureSourceOutOfBounds",
                    $"BigCraftable Extension for '{obj.ItemId}': "
                    + $"TexturePosition "
                    + $"'{extension.TexturePosition}' "
                    + $"with TextureWidth {textureWidth} "
                    + $"and TextureHeight {textureHeight} "
                    + $"is outside the texture bounds "
                    + $"({texture.Width}x{texture.Height}). "
                    + "The vanilla texture position will be used instead.");

                vanillaSourceRect.Width =
                    textureWidth;

                vanillaSourceRect.Height =
                    textureHeight;

                return vanillaSourceRect;
            }

            //----------------------------------------
            // Custom Source Rectangle
            //----------------------------------------

            return customSourceRect;
        }

        //----------------------------------------
        // TryParseTexturePosition
        //----------------------------------------

        /// <summary>
        /// "X, Y"形式のTexture座標を解析します。
        /// </summary>
        private static bool TryParseTexturePosition(
            string value,
            out Point position)
        {
            position =
                Point.Zero;

            string[] parts =
                value.Split(',');

            if (parts.Length != 2)
            {
                return false;
            }

            if (!int.TryParse(
                parts[0].Trim(),
                out int x))
            {
                return false;
            }

            if (!int.TryParse(
                parts[1].Trim(),
                out int y))
            {
                return false;
            }

            position =
                new Point(
                    x,
                    y);

            return true;
        }

        //----------------------------------------
        // Warning
        //----------------------------------------

        /// <summary>
        /// 同じ警告を1度だけ表示します。
        /// </summary>
        private static void LogWarningOnce(
            string key,
            string message)
        {
            if (Monitor == null)
                return;

            if (!LoggedWarnings.Add(
                key))
            {
                return;
            }

            Monitor.Log(
                message,
                LogLevel.Warn);
        }

        //----------------------------------------
        // Clear
        //----------------------------------------

        /// <summary>
        /// Serviceの管理情報を削除します。
        /// </summary>
        public static void Clear()
        {
            LoggedWarnings.Clear();
        }
    }
}