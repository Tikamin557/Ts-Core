namespace Ts_Core.Models.ContentPatcherRelated
{
    /// <summary>設置前に選択できる床材画像の定義です。座標はピクセル単位です。</summary>
    internal sealed class CustomFlooringModel
    {
        public List<CustomFlooringVariant> Variants { get; set; } = new();

        /// <summary>床材アイテムの表示画像を差し替え・合成する任意の設定です。</summary>
        public CustomFlooringObjectTexture? ObjectTexture { get; set; }

        /// <summary>撤去時の破片を表示するかどうか。省略時はバニラと同じ動作です。</summary>
        public bool ShowRemovalDebris { get; set; } = true;

        /// <summary>この床材の上に別の床材を設置して張り替えられるようにします。省略時はfalseです。</summary>
        public bool AllowFlooringReplacement { get; set; } = false;

        /// <summary>設置プレビューの強調表示方法です。Default / None / Outline を指定します。</summary>
        public string PlacementHighlight { get; set; } = "Default";

        /// <summary>設置前にVariantを切り替えた時の効果音です。省略またはnullなら無音です。</summary>
        public string? VariantChangeSound { get; set; }

        /// <summary>設置前にVariantの前後を確認できるHUDを表示します。</summary>
        public bool VariantPreviewEnabled { get; set; } = false;

        /// <summary>選択中のVariant名をプレビュー下の別ウィンドウに表示します。省略時はfalseです。</summary>
        public bool VariantPreviewShowName { get; set; } = false;

        /// <summary>Carouselは横並びのプレビュー、Builderは全Variantを一覧で選択する表示です。</summary>
        public string VariantPreviewMode { get; set; } = "Carousel";

        /// <summary>Builderモードの1行あたりの列数です。1～10に制限します。</summary>
        public int VariantPreviewColumns { get; set; } = 5;

        /// <summary>Builder一覧で空欄にする位置番号です。1始まりで、カンマ区切りで複数指定できます。</summary>
        public string SelectablePreviewEmptyOrders { get; set; } = "";

        /// <summary>HUDに表示するVariant数です。3、5、7、9に対応します。</summary>
        public int VariantPreviewCount { get; set; } = 5;

        /// <summary>HUDの拡大補正です。0～5の範囲で指定します。</summary>
        public int VariantPreviewScale { get; set; } = 0;

        /// <summary>HUDの横位置を10 UIピクセル単位で調整します。</summary>
        public int VariantPreviewOffsetX { get; set; } = 0;

        /// <summary>HUDの縦位置を10 UIピクセル単位で調整します。</summary>
        public int VariantPreviewOffsetY { get; set; } = 0;

        /// <summary>識別表示時に使う任意の16x16画像です。未指定なら標準の識別マーカーを描画します。</summary>
        public CustomFlooringIdentificationMarker? IdentificationMarker { get; set; }

        /// <summary>IdentificationMarkerの日本語キーにも対応します。</summary>
        [System.Text.Json.Serialization.JsonPropertyName("識別マーカー")]
        public CustomFlooringIdentificationMarker? JapaneseIdentificationMarker
        {
            get => IdentificationMarker;
            set => IdentificationMarker = value;
        }
    }

    /// <summary>アイテム画像に適用する16x16画像と編集モードです。</summary>
    internal sealed class CustomFlooringObjectTexture
    {
        public string Texture { get; set; } = "";
        public int X { get; set; }
        public int Y { get; set; }
        public string PatchMode { get; set; } = "Replace";
    }

    /// <summary>識別マーカーとして使用するタイルシート上の16x16ピクセル画像です。</summary>
    internal sealed class CustomFlooringIdentificationMarker
    {
        public string Texture { get; set; } = "";
        public int X { get; set; }
        public int Y { get; set; }
    }

    /// <summary>タイルシート上の16x16ピクセルの画像を指定します。</summary>
    internal sealed class CustomFlooringVariant
    {
        /// <summary>保存データからVariantを特定するための固定IDです。配列の並び替えでも変更しないでください。</summary>
        public string Id { get; set; } = "";

        /// <summary>Variantプレビューに表示する任意の名前です。未指定なら(none)を表示します。</summary>
        public string? DisplayName { get; set; }

        /// <summary>Builderモードの一覧でのみ使用する表示順です。未指定は指定済みの後に並びます。</summary>
        public int? SelectablePreviewOrder { get; set; }

        public string Texture { get; set; } = "";
        public int X { get; set; }
        public int Y { get; set; }

        /// <summary>ランダムテクスチャの変更周期（AMPM / DayNight / Daily / Weekly / Seasonal）。</summary>
        public string RandomTextureChange { get; set; } = "Daily";

        /// <summary>trueなら同一床材IDとVariant IDの全設置物で抽選結果を共有します。</summary>
        public bool RandomTextureShared { get; set; } = false;

        /// <summary>ランダムに使用する16x16のテクスチャ候補です。</summary>
        public List<CustomFlooringRandomTexture>? RandomTextures { get; set; }

        /// <summary>省略時は全Variantで同期します。指定するとID別の位相で再生します。</summary>
        public string? AnimationId { get; set; }

        /// <summary>省略時は同期します。falseの場合は設置時の再生位相を床材ごとに保存します。</summary>
        public bool AnimationSync { get; set; } = true;

        /// <summary>アニメーションフレーム。未指定なら従来のX/Yで静止画を表示します。</summary>
        public List<CustomFlooringAnimationFrame>? Animation { get; set; }

        /// <summary>設置音。省略時は床材の基本設定、nullは無音です。</summary>
        private string? placementSound;
        public string? PlacementSound { get => placementSound; set { placementSound = value; HasPlacementSound = true; } }
        /// <summary>撤去音。省略時は床材の基本設定、nullは無音です。</summary>
        private string? removalSound;
        public string? RemovalSound { get => removalSound; set { removalSound = value; HasRemovalSound = true; } }
        /// <summary>足音。省略時は床材の基本設定、nullは無音です。</summary>
        private string? footstepSound;
        public string? FootstepSound { get => footstepSound; set { footstepSound = value; HasFootstepSound = true; } }

        // JSON上での未指定と明示的なnullを区別します。
        [System.Text.Json.Serialization.JsonIgnore]
        public bool HasPlacementSound { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public bool HasRemovalSound { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public bool HasFootstepSound { get; set; }
    }
    /// <summary>ランダム表示に使用する16x16ピクセルの画像です。</summary>
    internal sealed class CustomFlooringRandomTexture
    {
        /// <summary>抽選の相対的な重みです。省略時は1、0なら抽選対象から除外します。</summary>
        public int Weight { get; set; } = 1;

        public string Texture { get; set; } = "";
        public int X { get; set; }
        public int Y { get; set; }
        /// <summary>この候補の足音です。省略時はVariantの設定を引き継ぎ、nullは無音です。</summary>
        private string? footstepSound;
        public string? FootstepSound { get => footstepSound; set { footstepSound = value; HasFootstepSound = true; } }
        // JSON上での未指定と明示的なnullを区別します。
        [System.Text.Json.Serialization.JsonIgnore]
        public bool HasFootstepSound { get; set; }
        /// <summary>この候補を抽選したときに再生するアニメーションです。未指定なら静止画像です。</summary>
        public List<CustomFlooringAnimationFrame>? Animation { get; set; }
    }

    /// <summary>16x16ピクセルのアニメーションフレームと表示時間（ミリ秒）です。</summary>
    internal sealed class CustomFlooringAnimationFrame
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Duration { get; set; } = 200;
    }

}
