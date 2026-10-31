# Decoratable Cellar（地下室の壁紙・床材変更）

T's Core 2.0.2では、既存の`Cellar`型を維持したまま、壁紙と床材の張り替えを有効にできます。

← [Modder Guideに戻る](ModderGuide.md)

## 有効化

`Cellar`で使用するマップの**Map Property** `TsCoreDecoratableCellar` を `T` に設定します。ほかのLocation型では有効になりません。

## 領域の設定

| Tile Property | レイヤー | 用途 |
|---|---|---|
| `WallID` | `Back` または `Buildings` | 壁紙の領域ID |
| `FloorID` | `Back` | 床材の領域ID |

同じ領域IDのタイルをまとめて張り替えます。壁紙は3段（`Back`の2段と`Buildings`の1段）のうち変更可能なタイルを更新します。床材の2×2模様は`FloorID`領域の左上を基準に配置されます。

Map Propertyの`WallIDs`と`FloorIDs`で、領域IDと初期素材IDを任意で指定できます。

```text
WallIDs = Wall_1 0, Wall_2 1
FloorIDs = Floor_1 0, Floor_2 1
```

カンマ区切りで「領域ID 初期素材ID」を指定します。初期素材IDは省略できます。数値は書式の例です。Tile Propertyで指定された領域は、リストに含まれていなくても認識されます。

## 使用できる素材

通常の壁紙・床材アイテムを使用できます。バニラ素材と`Data/AdditionalWallpaperFlooring`で追加された素材セットに対応します。

素材IDはバニラ素材なら`0`、`1`など、追加素材なら`セットID:番号`（導入時の例：`Shen.Walls:0`、`Shen.Floors:0`）です。追加素材のテクスチャはContent Pack側で提供する必要があります。必要に応じてT's CoreがCellarマップにタイルシートを登録します。

## 保存と復元

壁紙・床材の選択はCellarごとの`modData`に別々に保存されます。ロード後や対応するマップ再読み込み後に再適用され、保存済みの選択はマップの初期素材より優先されます。

マップ更新時には領域IDを維持してください。この機能は元の`Cellar`型を変更しません。

← [Modder Guideに戻る](ModderGuide.md)
