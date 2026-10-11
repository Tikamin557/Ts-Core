namespace Ts_Core.Services.TerrainFeaturesRelated
{
    /// <summary>
    /// 地形要素管理メニューの最後の選択状態です。
    /// Mod共通の設定として保存し、ゲーム再起動後も復元します。
    /// </summary>
    internal sealed class TerrainFeaturesMenuSettings
    {
        /// <summary>全屋外マップと現在地を対象にするかどうか。</summary>
        public bool AllLocations { get; set; }

        /// <summary>チェックされている項目の識別名です。</summary>
        public List<string> SelectedTargets { get; set; } = new();
    }
}
