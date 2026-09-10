using StardewValley.GameData.Machines;

namespace Ts_Core.Models.MachineRelated
{
    /// <summary>
    /// Machine Interactionの設定データです。
    /// </summary>
    public sealed class MachineInteractionData
    {
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
        /// Machineの設置タイルを基準に、
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
        /// MachineをWobbleさせるかどうかです。
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
        /// Idle中にMachineを
        /// Wobbleさせるかどうかです。
        /// </summary>
        public bool IdleWobble
        {
            get;
            set;
        }

        //----------------------------------------
        // Idle Texture
        //----------------------------------------

        /// <summary>
        /// Idle中に表示するTextureです。
        /// </summary>
        public string? IdleTexture
        {
            get;
            set;
        }

        /// <summary>
        /// Idle Textureの基準座標です。
        /// "X, Y"形式のPixel座標で指定します。
        /// </summary>
        public string IdleTexturePosition
        {
            get;
            set;
        } = "0, 0";

        /// <summary>
        /// Idle Animationで使用する
        /// Frame番号です。
        /// </summary>
        public List<int>? IdleFrames
        {
            get;
            set;
        }

        /// <summary>
        /// Idle Animationの
        /// Frame表示時間です。
        /// 単一値またはFrameごとの値を
        /// ミリ秒単位で指定します。
        /// </summary>
        public object? IdleFrameDurationMs
        {
            get;
            set;
        }

        //----------------------------------------
        // Light
        //----------------------------------------

        /// <summary>
        /// Machineに常時Lightを
        /// 表示するかどうかです。
        /// </summary>
        public bool Light
        {
            get;
            set;
        }

        /// <summary>
        /// Lightのオフセットです。
        /// Machineの設置タイルを基準に、
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
    }
}