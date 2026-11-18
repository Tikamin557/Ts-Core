# ショートカットパネル

T's Coreには、プレイヤー向けのショートカットパネルと、C# Modから機能を登録するための公開APIがあります。

## 表示位置と操作（v1.9.2）

T's CoreのGMCMで **ショートカットパネルの表示位置** を **自由（Free）** に設定すると、開閉タブの上にある移動ハンドルをドラッグしてパネルを移動できます。初期設定の **固定（Fixed）** では、従来どおり所持金UIを基準に配置されます。自由配置した位置は保存され、ゲーム再起動後も維持されます。画面端で見切れないよう位置は補正されます。

開閉タブとパネル本体の表示倍率は個別に設定でき、自由配置にも反映されます。

## CJB Cheats Menuのワープショートカット（v1.9.2）

**CJB Cheats Menu** が導入されている場合、ショートカットパネルの空きスロットから **Mod機能** → **CJB Cheats Menu** を選び、分類されたワープ先一覧から目的地を選択できます。登録したスロットからそのワープ先を利用できます。候補はCJB Cheats Menu側が提供する一覧に従い、CJBの設定やContent Patcherによる編集も反映されます。

この連携はCJBの公開APIではなく内部実装に依存するため、CJBの更新により互換性が変わる可能性があります。ゲーム内で言語を変更した直後にワープ先の名前が正しく表示されない場合は、ゲームの完全再起動で改善することがあります（CJB本体のワープ画面でも発生する場合があります）。


## 地形要素管理のショートカット（v1.9.5）

ショートカットパネルの **Mod機能** からT's Coreの**地形要素管理**メニューを開けます。GMCMで設定できるキー（初期値 `L`）からも同じメニューを開けます。詳しくは[Terrain Featuresガイド](ModderGuide_TerrainFeatures.md)を参照してください。

## C# Mod作者向け

公開 `ITsCoreApi` のショートカット登録APIを使うと、独自機能をパネルへ登録できます。登録仕様はAPIインターフェイスと既存のModder Guideを参照してください。

## v2.0.4：初期設定

ショートカットパネルは**新規インストール時、デフォルトで無効**です。必要な場合はGMCMから有効にできます。既存の `config.json` に保存された有効・無効の設定は変更されません。

## アイコンのカスタマイズ（v2.0.6）

T's Coreの標準ショートカットアイコン22種類を個別のゲームアセットとして公開しています。UIリカラーModなどのContent Patcherコンテンツパックから `Load` または `EditImage` を使って変更できます。パッチがない場合はT's Coreの標準PNGが使用されます。

### 公開アセット一覧

| アセット名 | 用途 | 標準画像サイズ（px） |
| --- | --- | --- |
| `Mods/Tikamin557.TsCore/ShortcutPanel/CjbCheatsMenu` | CJB Cheats Menu ワープ | 47x47 |
| `Mods/Tikamin557.TsCore/ShortcutPanel/GameMenu` | ゲームメニュー選択 | 48x48 |
| `Mods/Tikamin557.TsCore/ShortcutPanel/GameMenuAnimals` | 動物タブ | 48x48 |
| `Mods/Tikamin557.TsCore/ShortcutPanel/GameMenuCollections` | コレクションタブ | 48x48 |
| `Mods/Tikamin557.TsCore/ShortcutPanel/GameMenuCrafting` | クラフトタブ | 48x48 |
| `Mods/Tikamin557.TsCore/ShortcutPanel/GameMenuExit` | 終了タブ | 48x48 |
| `Mods/Tikamin557.TsCore/ShortcutPanel/GameMenuInventory` | インベントリタブ | 48x48 |
| `Mods/Tikamin557.TsCore/ShortcutPanel/GameMenuMap` | マップタブ | 48x48 |
| `Mods/Tikamin557.TsCore/ShortcutPanel/GameMenuOptions` | オプションタブ | 48x48 |
| `Mods/Tikamin557.TsCore/ShortcutPanel/GameMenuSkills` | スキルタブ | 48x48 |
| `Mods/Tikamin557.TsCore/ShortcutPanel/GameMenuSocial` | ソーシャルタブ | 48x48 |
| `Mods/Tikamin557.TsCore/ShortcutPanel/GameMenuSpecialItems` | 特別なアイテム・能力タブ | 48x48 |
| `Mods/Tikamin557.TsCore/ShortcutPanel/GmcmSelectMod` | GMCMのMod選択 | 48x48 |
| `Mods/Tikamin557.TsCore/ShortcutPanel/GmcmTopMenu` | GMCMメインメニュー | 48x48 |
| `Mods/Tikamin557.TsCore/ShortcutPanel/MissingShortcut` | 未登録・不明なショートカット | 16x16 |
| `Mods/Tikamin557.TsCore/ShortcutPanel/PsrRoomPresets` | 配偶者部屋のプリセット | 48x48 |
| `Mods/Tikamin557.TsCore/ShortcutPanel/ScreenshotCurrentScreen` | スクリーンショット（現在の画面） | 16x16 |
| `Mods/Tikamin557.TsCore/ShortcutPanel/ScreenshotFullMap` | スクリーンショット（マップ全体） | 16x16 |
| `Mods/Tikamin557.TsCore/ShortcutPanel/SmapiConsole` | SMAPIコンソール | 48x48 |
| `Mods/Tikamin557.TsCore/ShortcutPanel/TerrainFeatures` | 地形要素管理 | 48x48 |
| `Mods/Tikamin557.TsCore/ShortcutPanel/TimeSkip` | 時間スキップ | 32x32 |
| `Mods/Tikamin557.TsCore/ShortcutPanel/TimeSkipDuration` | 時間指定スキップ | 32x32 |

標準PNGはT's Coreの `assets/ShortcutPanel/` にあります。差し替え用画像は標準画像と同じサイズの透過PNGを推奨します。アセット名には `.png` や言語コードを付けません。ゲームメニュー本体のアイコンとは別のアセットです。

### Content Patcherでの使用例

スキルタブのショートカットアイコン全体を `Load` で差し替える例です。以下のパッチを `content.json` の `Changes` に追加します。

```json
{
    "Action": "Load",
    "Target": "Mods/Tikamin557.TsCore/ShortcutPanel/GameMenuSkills",
    "FromFile": "assets/GameMenuSkills.png"
}
```

`EditImage` を使用する場合は以下のように指定します。`PatchMode: Replace` は透明部分も含めて置き換えます。

```json
{
    "Action": "EditImage",
    "Target": "Mods/Tikamin557.TsCore/ShortcutPanel/GameMenuSkills",
    "FromFile": "assets/GameMenuSkills.png",
    "PatchMode": "Replace"
}
```

上記は同じアイコンを変更するための別々の例です。どちらか一方を使用してください。`FromFile` はコンテンツパック内の画像を指します。対応にはT's Core v2.0.6以降が必要です。T's Coreを任意の依存Modとする場合は、パッチに `When: { "HasMod": "Tikamin557.TsCore" }` を追加できます（v2.0.6以降が導入されていることが前提です）。

## 記号の描画色（v2.0.6）

GMCMのショートカットパネル設定に **記号の描画色（Symbol Color）** が追加されました。

- **デフォルト（Default）**：従来のゲームの文字色（`Game1.textColor`）を使用します。
- **白（White）**：開閉ボタンの矢印、移動ハンドル、選択画面のスクロール矢印などの記号を白で描画します。暗いUIテーマで記号が見づらい場合に使用してください。

通常のテキストやPNGアイコンの色はこの設定で変更されません。UIリカラーModによる通常テキストの色変更は維持されます。

設定は `config.json` の `ShortcutPanelForegroundColor` に保存されます（初期値 `Default`）。
