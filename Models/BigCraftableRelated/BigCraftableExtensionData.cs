using Microsoft.Xna.Framework;
using StardewValley.GameData.Machines;
using System.Globalization;

namespace Ts_Core.Models.BigCraftableRelated
{
    /// <summary>
    /// BigCraftable Extensionの設定データです。
    /// </summary>
    public sealed class BigCraftableExtensionData
    {
        //----------------------------------------
        // Collision
        //----------------------------------------

        private int collisionWidth =
            1;

        private int collisionHeight =
            1;

        /// <summary>
        /// BigCraftableの衝突判定Sizeです。
        /// "Width, Height"形式で
        /// タイル単位で指定します。
        /// </summary>
        public string CollisionSize
        {
            get
            {
                return
                    $"{collisionWidth}, {collisionHeight}";
            }

            set
            {
                if (TryParseIntPair(
                    value,
                    out int width,
                    out int height))
                {
                    collisionWidth =
                        Math.Max(
                            1,
                            width);

                    collisionHeight =
                        Math.Max(
                            1,
                            height);
                }
                else
                {
                    collisionWidth =
                        1;

                    collisionHeight =
                        1;
                }
            }
        }

        /// <summary>
        /// BigCraftableの衝突判定の横幅です。
        /// </summary>
        public int CollisionWidth =>
            collisionWidth;

        /// <summary>
        /// BigCraftableの衝突判定の高さです。
        /// </summary>
        public int CollisionHeight =>
            collisionHeight;

        private List<string>? playerPassableTiles;

        private readonly HashSet<Point> playerPassableTileOffsets =
            new();

        /// <summary>
        /// Farmerが通行できるCollision Tileです。
        /// BigCraftableの設置タイルを基準に、
        /// "X, Y"形式の相対座標で指定します。
        /// </summary>
        public List<string>? PlayerPassableTiles
        {
            get
            {
                return playerPassableTiles;
            }

            set
            {
                playerPassableTiles =
                    value;

                playerPassableTileOffsets.Clear();

                if (value == null)
                    return;

                foreach (string tile in value)
                {
                    if (!TryParseIntPair(
                        tile,
                        out int x,
                        out int y))
                    {
                        continue;
                    }

                    playerPassableTileOffsets.Add(
                        new Point(
                            x,
                            y));
                }
            }
        }

        /// <summary>
        /// Farmerが通行できるCollision Tileの
        /// 相対座標です。
        /// </summary>
        public IReadOnlySet<Point> PlayerPassableTileOffsets =>
            playerPassableTileOffsets;

        //----------------------------------------
        // Tile Properties
        //----------------------------------------

        /// <summary>
        /// BigCraftableの相対Tileへ追加する
        /// Tile Propertyです。
        /// </summary>
        public List<BigCraftableTilePropertyData>? TileProperties
        {
            get;
            set;
        }

        //----------------------------------------
        // Texture
        //----------------------------------------

        /// <summary>
        /// 通常時に表示するTextureです。
        /// 未指定の場合はData/BigCraftablesの
        /// Textureを使用します。
        /// </summary>
        public string? Texture
        {
            get;
            set;
        }

        /// <summary>
        /// 通常Textureの基準座標です。
        /// "X, Y"形式のPixel座標で指定します。
        /// 未指定の場合はData/BigCraftablesの
        /// Texture位置を使用します。
        /// </summary>
        public string? TexturePosition
        {
            get;
            set;
        }

        private int textureWidth =
            16;

        private int textureHeight =
            32;

        /// <summary>
        /// BigCraftable TextureのSizeです。
        /// "Width, Height"形式で
        /// Pixel単位で指定します。
        /// </summary>
        public string TextureSize
        {
            get
            {
                return
                    $"{textureWidth}, {textureHeight}";
            }

            set
            {
                if (TryParseIntPair(
                    value,
                    out int width,
                    out int height))
                {
                    textureWidth =
                        Math.Max(
                            1,
                            width);

                    textureHeight =
                        Math.Max(
                            1,
                            height);
                }
                else
                {
                    textureWidth =
                        16;

                    textureHeight =
                        32;
                }
            }
        }

        /// <summary>
        /// BigCraftable Textureの横幅です。
        /// </summary>
        public int TextureWidth =>
            textureWidth;

        /// <summary>
        /// BigCraftable Textureの高さです。
        /// </summary>
        public int TextureHeight =>
            textureHeight;

        /// <summary>
        /// 通常Animationで使用する
        /// Frame番号です。
        /// 未指定の場合はAnimationしません。
        /// </summary>
        public List<int>? Frames
        {
            get;
            set;
        }

        /// <summary>
        /// 通常Animationの
        /// Frame表示時間です。
        /// 単一値またはFrameごとの値を
        /// ミリ秒単位で指定します。
        /// </summary>
        public object? FrameDurationMs
        {
            get;
            set;
        }

        //----------------------------------------
        // Draw Layers
        //----------------------------------------

        /// <summary>
        /// BigCraftableに追加して描画する
        /// DrawLayerの一覧です。
        /// </summary>
        public List<BigCraftableDrawLayerData>? DrawLayers
        {
            get;
            set;
        }

        //----------------------------------------
        // Placement Condition
        //----------------------------------------

        /// <summary>
        /// BigCraftableを設置できる条件です。
        /// Game State Query形式で指定します。
        /// </summary>
        public string? PlacementCondition
        {
            get;
            set;
        }

        //----------------------------------------
        // Tile Action
        //----------------------------------------

        /// <summary>
        /// 実行するTileActionです。
        /// </summary>
        public string? TileAction
        {
            get;
            set;
        }

        //----------------------------------------
        // Condition
        //----------------------------------------

        /// <summary>
        /// 使用条件です。
        /// </summary>
        public string? Condition
        {
            get;
            set;
        }

        /// <summary>
        /// 使用条件を満たしていない場合に
        /// 表示するメッセージです。
        /// </summary>
        public string? InvalidConditionMessage
        {
            get;
            set;
        }

        //----------------------------------------
        // Required Item
        //----------------------------------------

        /// <summary>
        /// 使用に必要なアイテムです。
        /// </summary>
        public string? RequiredItem
        {
            get;
            set;
        }

        /// <summary>
        /// 使用に必要なアイテム数です。
        /// </summary>
        public int RequiredItemCount
        {
            get;
            set;
        } = 1;

        /// <summary>
        /// 使用時に必要アイテムを
        /// 消費するかどうかです。
        /// </summary>
        public bool ConsumeRequiredItem
        {
            get;
            set;
        } = true;

        /// <summary>
        /// 必要なアイテムを持っていない場合に
        /// 表示するメッセージです。
        /// </summary>
        public string? InvalidItemMessage
        {
            get;
            set;
        }

        /// <summary>
        /// 必要なアイテム数が不足している場合に
        /// 表示するメッセージです。
        /// </summary>
        public string? InvalidCountMessage
        {
            get;
            set;
        }

        //----------------------------------------
        // Action Effects
        //----------------------------------------

        /// <summary>
        /// TileAction成功時に再生する
        /// Machine Effectです。
        /// </summary>
        public List<MachineEffects>? ActionEffects
        {
            get;
            set;
        }

        /// <summary>
        /// ActionEffectsを実行する確率です。
        /// 0～1の範囲で指定します。
        /// </summary>
        public float ActionEffectChance
        {
            get;
            set;
        } = 1f;

        //----------------------------------------
        // Action Light
        //----------------------------------------

        /// <summary>
        /// TileAction成功時に
        /// Lightを表示するかどうかです。
        /// </summary>
        public bool ActionLight
        {
            get;
            set;
        }

        /// <summary>
        /// Action Lightのオフセットです。
        /// BigCraftableの設置タイルを基準に、
        /// タイル単位で指定します。
        /// </summary>
        public string ActionLightOffset
        {
            get;
            set;
        } = "0, 0";

        /// <summary>
        /// Action Lightの範囲です。
        /// </summary>
        public float ActionLightRadius
        {
            get;
            set;
        } = 2f;

        /// <summary>
        /// Action Lightの色です。
        /// MonoGameのColor名などで指定します。
        /// </summary>
        public string ActionLightColor
        {
            get;
            set;
        } = "White";

        /// <summary>
        /// Action Lightを表示する時間です。
        /// ミリ秒単位で指定します。
        /// </summary>
        public int ActionLightDurationMs
        {
            get;
            set;
        } = 1000;

        //----------------------------------------
        // Action Wobble
        //----------------------------------------

        /// <summary>
        /// TileAction成功時に
        /// BigCraftableをWobbleさせるかどうかです。
        /// </summary>
        public bool ActionWobble
        {
            get;
            set;
        }

        /// <summary>
        /// Action Wobbleを継続する時間です。
        /// ミリ秒単位で指定します。
        /// </summary>
        public int ActionWobbleDurationMs
        {
            get;
            set;
        } = 1000;

        //----------------------------------------
        // Action Texture
        //----------------------------------------

        /// <summary>
        /// Action時に表示するTextureです。
        /// </summary>
        public string? ActionTexture
        {
            get;
            set;
        }

        /// <summary>
        /// Action Textureの基準座標です。
        /// "X, Y"形式のPixel座標で指定します。
        /// </summary>
        public string ActionTexturePosition
        {
            get;
            set;
        } = "0, 0";

        /// <summary>
        /// Action Animationで使用する
        /// Frame番号です。
        /// </summary>
        public List<int>? ActionFrames
        {
            get;
            set;
        }

        /// <summary>
        /// Action Animationの
        /// Frame表示時間です。
        /// 単一値またはFrameごとの値を
        /// ミリ秒単位で指定します。
        /// </summary>
        public object? ActionFrameDurationMs
        {
            get;
            set;
        }

        /// <summary>
        /// ActionFrames未指定時に
        /// Action Textureを表示する時間です。
        /// ミリ秒単位で指定します。
        /// </summary>
        public int ActionDurationMs
        {
            get;
            set;
        } = 1000;

        //----------------------------------------
        // Idle Effects
        //----------------------------------------

        /// <summary>
        /// Idle中に再生する
        /// Machine Effectです。
        /// </summary>
        public List<MachineEffects>? IdleEffects
        {
            get;
            set;
        }

        /// <summary>
        /// IdleEffectsを実行する確率です。
        /// 0～1の範囲で指定します。
        /// </summary>
        public float IdleEffectChance
        {
            get;
            set;
        } = 1f;

        //----------------------------------------
        // Idle Wobble
        //----------------------------------------

        /// <summary>
        /// Idle中にBigCraftableを
        /// Wobbleさせるかどうかです。
        /// </summary>
        public bool IdleWobble
        {
            get;
            set;
        }

        //----------------------------------------
        // Light
        //----------------------------------------

        /// <summary>
        /// BigCraftableに常時Lightを
        /// 表示するかどうかです。
        /// </summary>
        public bool Light
        {
            get;
            set;
        }

        /// <summary>
        /// Lightのオフセットです。
        /// BigCraftableの設置タイルを基準に、
        /// タイル単位で指定します。
        /// </summary>
        public string LightOffset
        {
            get;
            set;
        } = "0, 0";

        /// <summary>
        /// Lightの範囲です。
        /// </summary>
        public float LightRadius
        {
            get;
            set;
        } = 2f;

        /// <summary>
        /// Lightの色です。
        /// MonoGameのColor名などで指定します。
        /// </summary>
        public string LightColor
        {
            get;
            set;
        } = "White";

        //----------------------------------------
        // Int Pair Parser
        //----------------------------------------

        /// <summary>
        /// "Value1, Value2"形式の整数値を解析します。
        /// </summary>
        private static bool TryParseIntPair(
            string? value,
            out int first,
            out int second)
        {
            first =
                0;

            second =
                0;

            if (string.IsNullOrWhiteSpace(
                value))
            {
                return false;
            }

            string[] parts =
                value.Split(
                    ',');

            if (parts.Length != 2)
                return false;

            if (!int.TryParse(
                parts[0].Trim(),
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out first))
            {
                return false;
            }

            if (!int.TryParse(
                parts[1].Trim(),
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out second))
            {
                return false;
            }

            return true;
        }
    }
}