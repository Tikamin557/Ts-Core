# Mod Conflict Warnings（Mod競合警告）

T's Core v1.9.3から、Content PatcherのData Assetを使用して、複数のModが同時に導入されている場合に警告を表示できます。**警告のみ**で、Modを無効化したりゲームの起動を妨げたりする機能ではありません。

← [ModderGuide](ModderGuide.md) | [README](../../README.md)

## Data Asset

Content Patcherの`EditData`から`TsCore/ModConflictWarnings`へ登録します。`Entries`のキーは競合定義の識別子です。ほかのContent Packと重複しないよう、`{{ModId}}`などを付けることをおすすめします。

```json
{
    "Action": "EditData",
    "Target": "TsCore/ModConflictWarnings",
    "Entries": {
        "{{ModId}}_CP_PIF": {
            "ModIds": [
                "Tikamin557.CP.FarmhouseAnnex",
                "Tikamin557.CP.FarmhouseAnnex_PIF"
            ]
        }
    }
}
```

この例では、T's Farmhouse AnnexのCP版とPIF版が両方読み込まれていると警告します。定義は片方のContent Packだけに登録すれば十分です。

## プロパティ

| プロパティ | 必須 | 説明 |
|---|---|---|
| `ModIds` | 必須 | ModのUniqueIDの配列。指定した**異なるIDのうち2件以上**が読み込まれていると警告します。大文字・小文字は区別しません。 |
| `Message` | 任意 | タイトル画面に表示する独自の警告文。省略または空の場合は、検出したMod名を列挙するT's Core標準の翻訳文を使用します。 |

`ModIds`には3件以上も指定できます。その場合は**全件が必要なのではなく、指定したModのうち2件以上**が読み込まれると警告します。

## Messageとi18n

```json
{
    "Action": "EditData",
    "Target": "TsCore/ModConflictWarnings",
    "Entries": {
        "{{ModId}}_CP_PIF": {
            "ModIds": [
                "Tikamin557.CP.FarmhouseAnnex",
                "Tikamin557.CP.FarmhouseAnnex_PIF"
            ],
            "Message": "{{i18n:modConflict.warning}}"
        }
    }
}
```

Content Pack自身の`i18n/default.json`や`i18n/ja.json`に`modConflict.warning`を定義してください。Content Patcherがi18nトークンを展開した文字列を、T's Coreが表示します。JSON文字列内の`\n`で改行、`\n\n`で空行を指定できます。

例えば`i18n/ja.json`には次のように記述します。

```json
{
    "modConflict.warning": "次のModが同時にインストールされています。\nいずれか一方のみをインストールしてください。"
}
```

`Message`で変更されるのは**タイトル画面の警告本文**です。SMAPIコンソールのログ形式には影響しません。

## 警告の表示タイミングと動作

- ゲーム起動後、初回のタイトル画面が表示され、言語設定などが安定するまで少し待ってから1回だけ競合を確認します。
- 検出した競合は、定義キー・Mod名・UniqueIDを含む`Warn`ログとしてSMAPIコンソールへ出力します。
- タイトル画面では確認式の警告ウィンドウを1つ表示します。複数の競合がある場合は、本文を空行で区切ってまとめて表示します。
- 長文はマウスホイールや右側のスクロールバーのドラッグでスクロールでき、**OK**で閉じられます。
- 起動時の言語で表示されます。閉じた後にタイトル画面で言語を変更しても、その起動中は警告を再表示しません。

## 注意事項

- `ModIds`にはModの表示名ではなく**UniqueID**を指定してください。
- 判定対象は**読み込まれたMod**です。Content Patcherの個々のパッチが適用されたかどうかは判定しません。
- 同じ競合グループを複数のContent Packから重複登録すると、それぞれが警告として表示されます。
- 起動時に1回だけ確認するため、起動中に定義を編集しても警告は自動的には再表示されません。
- この機能は競合を通知するもので、競合を自動修正するものではありません。
