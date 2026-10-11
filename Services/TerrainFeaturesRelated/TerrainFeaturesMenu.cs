using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace Ts_Core.Services.TerrainFeaturesRelated
{
    /// <summary>
    /// Terrain Features の対象・範囲・操作を選択する専用メニューです。
    /// 全マップ操作と削除操作は、実行前に確認画面を表示します。
    /// </summary>
    internal sealed class TerrainFeaturesMenu : IClickableMenu
    {
        private readonly IMonitor monitor;
        private readonly ITranslationHelper translations;
        private readonly IModHelper helper;
        private const string SettingsPath = "TerrainFeaturesConfig.json";
        private readonly HashSet<TerrainFeaturesSelectionFactory.Target> selected = new();
        private readonly TerrainFeaturesSelectionFactory.Target[] targets = Enum.GetValues<TerrainFeaturesSelectionFactory.Target>();
        // 一括指定と個別指定を独立した行に配置します。null はセクション見出しです。
        private readonly List<TerrainFeaturesSelectionFactory.Target?[]> displayRows = new();
        private readonly List<Rectangle> targetBounds = new();
        private Rectangle scopeBounds;
        private Rectangle resetBounds;
        private Rectangle clearBounds;
        private Rectangle generateBounds;
        private Rectangle confirmBounds;
        private Rectangle cancelBounds;
        private int scroll;
        private int visibleRows;
        private Rectangle listBounds;
        private Rectangle scrollbarTrack;
        private Rectangle scrollbarThumb;
        private Rectangle messageBounds;
        private Rectangle statusBounds;
        private bool draggingScrollbar;
        private int dragOffset;
        private const int Columns = 2;
        // 「すべて」は個別項目の一括選択としても機能します。
        // 個別項目を外した場合は、広範囲に作用する「すべて」の選択も解除します。
        private static readonly Dictionary<TerrainFeaturesSelectionFactory.Target, TerrainFeaturesSelectionFactory.Target[]> Groups = new()
        {
            [TerrainFeaturesSelectionFactory.Target.ResourceClump] = new[]
            {
                TerrainFeaturesSelectionFactory.Target.Stump,
                TerrainFeaturesSelectionFactory.Target.Log,
                TerrainFeaturesSelectionFactory.Target.Boulder
            },
            [TerrainFeaturesSelectionFactory.Target.Object] = new[]
            {
                TerrainFeaturesSelectionFactory.Target.Weeds,
                TerrainFeaturesSelectionFactory.Target.Twig,
                TerrainFeaturesSelectionFactory.Target.Rock,
                TerrainFeaturesSelectionFactory.Target.Forage,
                TerrainFeaturesSelectionFactory.Target.Fence
            },
            [TerrainFeaturesSelectionFactory.Target.TFeature] = new[]
            {
                TerrainFeaturesSelectionFactory.Target.Bush,
                TerrainFeaturesSelectionFactory.Target.Tree,
                TerrainFeaturesSelectionFactory.Target.Grass,
                TerrainFeaturesSelectionFactory.Target.Path,
                TerrainFeaturesSelectionFactory.Target.Crop,
                TerrainFeaturesSelectionFactory.Target.TilledSoil
            }
        };
        private const int RowHeight = 48;
        private int lastScreenWidth;
        private int lastScreenHeight;
        private bool allLocations;
        private bool confirming;
        private Operation pending;
        private string status = string.Empty;
        private enum Operation { Reset, Clear, Generate }

        internal TerrainFeaturesMenu(IMonitor monitor, ITranslationHelper translations, IModHelper helper)
        {
            this.monitor = monitor;
            this.translations = translations;
            this.helper = helper;
            LoadSettings();
            BuildDisplayRows();
            RebuildLayout();
        }

        /// <summary>保存された項目のうち、現在のバージョンで有効なものだけを復元します。</summary>
        private void LoadSettings()
        {
            try
            {
                TerrainFeaturesMenuSettings? settings = helper.Data.ReadJsonFile<TerrainFeaturesMenuSettings>(SettingsPath);
                if (settings == null) return;
                allLocations = settings.AllLocations;
                foreach (string name in settings.SelectedTargets ?? new List<string>())
                {
                    if (Enum.TryParse(name, true, out TerrainFeaturesSelectionFactory.Target target)
                        && Enum.IsDefined(typeof(TerrainFeaturesSelectionFactory.Target), target))
                        selected.Add(target);
                }
            }
            catch (Exception ex)
            {
                monitor.Log($"Could not load Terrain Features menu settings: {ex}", LogLevel.Warn);
            }
        }

        /// <summary>操作のたびに保存し、メニューを閉じたりゲームを終了しても選択状態を保持します。</summary>
        private void SaveSettings()
        {
            try
            {
                helper.Data.WriteJsonFile(SettingsPath, new TerrainFeaturesMenuSettings
                {
                    AllLocations = allLocations,
                    SelectedTargets = selected.Select(target => target.ToString()).ToList()
                });
            }
            catch (Exception ex)
            {
                monitor.Log($"Could not save Terrain Features menu settings: {ex}", LogLevel.Warn);
            }
        }

        private string T(string key) => translations.Get("terrainFeatures." + key).ToString();

        /// <summary>一括指定を先に並べ、個別指定は従来の並び（ブッシュを左上）に保ちます。</summary>
        private void BuildDisplayRows()
        {
            displayRows.Clear();
            displayRows.Add(new TerrainFeaturesSelectionFactory.Target?[] { null }); // 一括指定
            var bulk = new[] { TerrainFeaturesSelectionFactory.Target.ResourceClump, TerrainFeaturesSelectionFactory.Target.Object, TerrainFeaturesSelectionFactory.Target.TFeature };
            // 最初の null は「すべて」専用の選択欄です（見出しとは行番号で区別）。
            displayRows.Add(new TerrainFeaturesSelectionFactory.Target?[] { null, bulk[0] });
            displayRows.Add(new TerrainFeaturesSelectionFactory.Target?[] { bulk[1], bulk[2] });
            displayRows.Add(new TerrainFeaturesSelectionFactory.Target?[] { null }); // 個別指定
            var individual = targets.Where(t => !bulk.Contains(t)).ToArray();
            for (int i = 0; i < individual.Length; i += Columns)
                displayRows.Add(individual.Skip(i).Take(Columns).Select(t => (TerrainFeaturesSelectionFactory.Target?)t).ToArray());
        }

        private bool IsHeaderRow(int row) => row == 0 || row == 3;
        private string HeaderText(int row) => row == 0 ? T("bulkSection") : T("individualSection");

        private void RebuildLayout()
        {
            float scale = Math.Max(0.01f, Game1.options.uiScale);
            int sw = Game1.game1.uiScreen?.Width ?? (int)(Game1.graphics.GraphicsDevice.Viewport.Width / scale);
            int sh = Game1.game1.uiScreen?.Height ?? (int)(Game1.graphics.GraphicsDevice.Viewport.Height / scale);
            lastScreenWidth = sw;
            lastScreenHeight = sh;

            // 一覧に必要な高さを優先しつつ、画面の上下には余白を残します。
            // 確認文・ボタン・結果表示の高さは固定し、一覧だけをスクロールさせます。
            const int listTopOffset = 158;
            const int footerHeight = 178;
            const int maxMenuHeight = 820;
            int totalRows = displayRows.Count;
            int marginY = Math.Max(28, Math.Min(72, sh / 14));
            int availableHeight = Math.Max(260, sh - marginY * 2);
            int desiredHeight = listTopOffset + totalRows * RowHeight + footerHeight;
            int w = Math.Min(760, Math.Max(240, sw - 32));
            int h = Math.Min(Math.Min(maxMenuHeight, availableHeight), desiredHeight);

            // 余った高さを一覧と確認文の間に残さないよう、行数単位でメニューを縮めます。
            visibleRows = Math.Max(1, Math.Min(totalRows, (h - listTopOffset - footerHeight) / RowHeight));
            h = Math.Min(h, listTopOffset + visibleRows * RowHeight + footerHeight);
            initialize((sw - w) / 2, (sh - h) / 2, w, h);
            // Stardew Valley 標準の閉じるボタンを配置します。
            initializeUpperRightCloseButton();
            scopeBounds = new Rectangle(xPositionOnScreen + 24, yPositionOnScreen + 76, width - 48, 64);

            int listTop = yPositionOnScreen + listTopOffset;
            listBounds = new Rectangle(xPositionOnScreen + 24, listTop, width - 48, visibleRows * RowHeight);
            int footerTop = listBounds.Bottom + 12;
            messageBounds = new Rectangle(xPositionOnScreen + 24, footerTop, width - 48, 30);
            int buttonY = messageBounds.Bottom + 6;
            const int buttonHeight = 58;
            // ゲーム標準ボタンの影が結果表示にかからないよう16px空けます。
            statusBounds = new Rectangle(xPositionOnScreen + 24, buttonY + buttonHeight + 18, width - 48, 28);
            const int buttonMargin = 28;
            const int buttonGap = 12;
            int bw = (width - buttonMargin * 2 - buttonGap * 2) / 3;
            int buttonStartX = xPositionOnScreen + (width - (bw * 3 + buttonGap * 2)) / 2;
            resetBounds = new Rectangle(buttonStartX, buttonY, bw, buttonHeight);
            clearBounds = new Rectangle(resetBounds.Right + buttonGap, buttonY, bw, buttonHeight);
            generateBounds = new Rectangle(clearBounds.Right + buttonGap, buttonY, bw, buttonHeight);
            confirmBounds = new Rectangle(xPositionOnScreen + 24, buttonY, (width - 56) / 2, buttonHeight);
            cancelBounds = new Rectangle(confirmBounds.Right + 8, buttonY, (width - 56) / 2, buttonHeight);

            scroll = Math.Clamp(scroll, 0, Math.Max(0, totalRows - visibleRows));
            targetBounds.Clear();
            bool needScroll = totalRows > visibleRows;
            int reserved = needScroll ? 28 : 0;
            int columnWidth = (listBounds.Width - reserved - 8) / Columns;
            for (int row = 0; row < visibleRows; row++)
            {
                for (int column = 0; column < Columns; column++)
                {
                    targetBounds.Add(new Rectangle(
                        listBounds.X + column * (columnWidth + 8),
                        listBounds.Y + row * RowHeight,
                        columnWidth, RowHeight - 4));
                }
            }
            scrollbarTrack = new Rectangle(listBounds.Right - 19, listBounds.Y, 16, listBounds.Height);
            int thumbHeight = Math.Max(28, scrollbarTrack.Height * visibleRows / totalRows);
            int maxScroll = Math.Max(1, totalRows - visibleRows);
            int thumbY = scrollbarTrack.Y + (scrollbarTrack.Height - thumbHeight) * scroll / maxScroll;
            scrollbarThumb = new Rectangle(scrollbarTrack.X, thumbY, scrollbarTrack.Width, thumbHeight);


        }

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            base.gameWindowSizeChanged(oldBounds, newBounds);
            RebuildLayout();
        }

        /// <summary>メニューを閉じます。選択状態は操作時に保存済みです。</summary>
        private void CloseMenu()
        {
            Game1.playSound("bigDeSelect");
            exitThisMenuNoSound();
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (upperRightCloseButton?.containsPoint(x, y) == true ||
                !new Rectangle(xPositionOnScreen, yPositionOnScreen, width, height).Contains(x, y))
            {
                CloseMenu();
                return;
            }

            if (!confirming && scrollbarTrack.Contains(x, y) && displayRows.Count > visibleRows)
            {
                draggingScrollbar = true;
                dragOffset = scrollbarThumb.Contains(x, y) ? y - scrollbarThumb.Y : scrollbarThumb.Height / 2;
                UpdateScrollbar(y);
                return;
            }
            if (confirming)
            {
                if (confirmBounds.Contains(x, y)) Execute();
                else if (cancelBounds.Contains(x, y)) { Game1.playSound("bigDeSelect"); confirming = false; }
                return;
            }
            if (scopeBounds.Contains(x, y)) { allLocations = !allLocations; SaveSettings(); Game1.playSound("smallSelect"); return; }
            for (int i = 0; i < targetBounds.Count; i++)
            {
                int row = scroll + i / Columns;
                int column = i % Columns;
                if (row >= displayRows.Count || IsHeaderRow(row) || column >= displayRows[row].Length || !targetBounds[i].Contains(x, y)) continue;
                if (row == 1 && column == 0)
                {
                    // 「すべて」は全17項目の選択をまとめて切り替えます。
                    bool allSelected = targets.All(selected.Contains);
                    if (allSelected) selected.Clear();
                    else selected.UnionWith(targets);
                    SaveSettings();
                    Game1.playSound(allSelected ? "shwip" : "smallSelect");
                    return;
                }
                var target = displayRows[row][column]!.Value;
                bool wasSelected = selected.Contains(target);
                ToggleTarget(target);
                SaveSettings();
                Game1.playSound(wasSelected ? "shwip" : "smallSelect");
                return;
            }
            if (selected.Count == 0) return;
            if (resetBounds.Contains(x, y)) { pending = Operation.Reset; confirming = true; Game1.playSound("smallSelect"); }
            else if (clearBounds.Contains(x, y)) { pending = Operation.Clear; confirming = true; Game1.playSound("smallSelect"); }
            else if (generateBounds.Contains(x, y)) { pending = Operation.Generate; confirming = true; Game1.playSound("smallSelect"); }
        }

        /// <summary>メニュー外の右クリックで閉じます。メニュー内の右クリックは無操作です。</summary>
        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            if (!new Rectangle(xPositionOnScreen, yPositionOnScreen, width, height).Contains(x, y))
                CloseMenu();
        }

        /// <summary>「すべて」と個別項目の選択状態を連動させます。</summary>
        private void ToggleTarget(TerrainFeaturesSelectionFactory.Target target)
        {
            bool select = !selected.Contains(target);
            if (select) selected.Add(target);
            else selected.Remove(target);

            if (Groups.TryGetValue(target, out var members))
            {
                foreach (var member in members)
                {
                    if (select) selected.Add(member);
                    else selected.Remove(member);
                }
            }
            else if (!select)
            {
                // グループが残ると、個別項目を外しても全体操作が実行されてしまうため解除します。
                foreach (var group in Groups)
                {
                    if (group.Value.Contains(target))
                        selected.Remove(group.Key);
                }
            }
        }

        private void UpdateScrollbar(int y)
        {
            int totalRows = displayRows.Count;
            int range = scrollbarTrack.Height - scrollbarThumb.Height;
            if (range <= 0) return;
            int next = Math.Clamp((int)Math.Round((double)(y - dragOffset - scrollbarTrack.Y) * (totalRows - visibleRows) / range), 0, totalRows - visibleRows);
            if (next != scroll) { scroll = next; RebuildLayout(); }
        }

        public override void leftClickHeld(int x, int y)
        {
            base.leftClickHeld(x, y);
            if (draggingScrollbar) UpdateScrollbar(y);
        }

        public override void releaseLeftClick(int x, int y)
        {
            base.releaseLeftClick(x, y);
            draggingScrollbar = false;
        }

        public override void receiveScrollWheelAction(int direction)
        {
            if (!confirming)
            {
                int next = Math.Clamp(scroll - Math.Sign(direction), 0, Math.Max(0, displayRows.Count - visibleRows));
                if (next != scroll) { scroll = next; RebuildLayout(); }
            }
        }

        private void Execute()
        {
            confirming = false;
            if (!Context.IsWorldReady || !Context.IsMainPlayer || Game1.currentLocation == null)
            {
                status = T("hostOnly");
                return;
            }
            if (selected.Count == 0)
                return;

            Game1.playSound("Ship");
            var selection = TerrainFeaturesSelectionFactory.Create(selected);
            IEnumerable<GameLocation> locations = allLocations
                ? Game1.locations.Where(p => p.IsOutdoors).Concat(new[] { Game1.currentLocation })
                : new[] { Game1.currentLocation };
            int success = 0;
            int failed = 0;
            // 実行中の列挙変更を避け、同じロケーションを重複処理しません。
            foreach (GameLocation location in locations.Distinct().ToList())
            {
                try
                {
                    bool ok;
                    switch (pending)
                    {
                        case Operation.Clear:
                            TerrainFeaturesClearService.Clear(location, selection);
                            ok = true;
                            break;
                        case Operation.Generate:
                            ok = TerrainFeaturesGenerateService.TryGenerate(location, selection);
                            break;
                        default:
                            ok = TerrainFeaturesResetService.TryReset(location, selection, monitor);
                            break;
                    }
                    if (ok) success++; else failed++;
                }
                catch (Exception ex)
                {
                    failed++;
                    monitor.Log($"Terrain Features {pending} failed in {location.NameOrUniqueName}: {ex}", LogLevel.Error);
                }
            }
            status = T("completed").Replace("{{success}}", success.ToString()).Replace("{{failed}}", failed.ToString());
        }

        /// <summary>ゲーム標準のフォントに控えめな影を付けて描画します。文字の位置・倍率は変更しません。</summary>
        private static void DrawShadowedText(SpriteBatch b, SpriteFont font, string text, Vector2 position, Color color, float scale)
        {
            b.DrawString(font, text, position + new Vector2(2f, 2f), Color.Black * 0.35f,
                0f, Vector2.Zero, scale, SpriteEffects.None, 1f);
            b.DrawString(font, text, position, color,
                0f, Vector2.Zero, scale, SpriteEffects.None, 1f);
        }

        /// <summary>ゲーム本体の会話フォントを使用してボタンの文字を描画します。</summary>
        private static void DrawButton(SpriteBatch b, Rectangle bounds, string label, int verticalPadding = 8, bool enabled = true)
        {
            // 選択項目がない場合は淡いグレーで無効状態を示し、文字の視認性を維持します。クリックは既存処理で無効化済みです。
            Color tint = enabled ? Color.White : new Color(215, 215, 215);
            // 標準の大きな影は無効化し、4pxだけずらした控えめな影を描画します。
            IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), bounds.X + 4, bounds.Y + 4, bounds.Width, bounds.Height, Color.Black * 0.35f, 1f, false);
            IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), bounds.X, bounds.Y, bounds.Width, bounds.Height, tint, 1f, false);
            Vector2 size = Game1.dialogueFont.MeasureString(label);
            float factor = Math.Min(1f, Math.Min((bounds.Width - 12f) / Math.Max(1f, size.X), (bounds.Height - verticalPadding) / Math.Max(1f, size.Y)));
            DrawShadowedText(b, Game1.dialogueFont, label,
                new Vector2(bounds.Center.X - size.X * factor / 2f, bounds.Center.Y - size.Y * factor / 2f),
                enabled ? Game1.textColor : new Color(85, 80, 80), factor);
        }

        /// <summary>
        /// Stardew Valley 本体のチェックボックス画像と会話フォントを使用します。
        /// 選択範囲・行間・列数は従来のまま維持します。
        /// </summary>
        private static void DrawCheckbox(SpriteBatch b, Rectangle bounds, bool isChecked, string label)
        {
            const int checkboxSize = 36;
            Vector2 checkboxPosition = new(bounds.X + 4, bounds.Center.Y - checkboxSize / 2f);
            // mouseCursors のチェックボックス (9x9) をゲーム標準の4倍で描画します。
            b.Draw(Game1.mouseCursors, checkboxPosition,
                new Rectangle(isChecked ? 236 : 227, 425, 9, 9), Color.White,
                0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);

            Vector2 textSize = Game1.dialogueFont.MeasureString(label);
            float textScale = Math.Min(1f,
                Math.Min((bounds.Width - checkboxSize - 18f) / Math.Max(1f, textSize.X),
                         (bounds.Height - 2f) / Math.Max(1f, textSize.Y)));
            DrawShadowedText(b, Game1.dialogueFont, label,
                new Vector2(bounds.X + 4 + checkboxSize + 9, bounds.Center.Y - textSize.Y * textScale / 2f),
                Game1.textColor, textScale);
        }

        public override void draw(SpriteBatch b)
        {
            float scale = Math.Max(0.01f, Game1.options.uiScale);
            int sw = Game1.game1.uiScreen?.Width ?? (int)(Game1.graphics.GraphicsDevice.Viewport.Width / scale);
            int sh = Game1.game1.uiScreen?.Height ?? (int)(Game1.graphics.GraphicsDevice.Viewport.Height / scale);
            if (sw != lastScreenWidth || sh != lastScreenHeight)
                RebuildLayout();
            b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, sw, sh), Color.Black * 0.65f);
            IClickableMenu.drawTextureBox(b, xPositionOnScreen, yPositionOnScreen, width, height, Color.White);
            string title = T("title");
            Vector2 titleSize = Game1.dialogueFont.MeasureString(title);
            float titleScale = Math.Min(1.4f, (width - 48f) / Math.Max(1f, titleSize.X));
            DrawShadowedText(b, Game1.dialogueFont, title, new Vector2(xPositionOnScreen + 24, yPositionOnScreen + 20), Game1.textColor, titleScale);
            DrawButton(b, scopeBounds, allLocations ? T("allLocations") : T("currentLocation"), verticalPadding: 24);
            for (int i = 0; i < targetBounds.Count; i++)
            {
                int row = scroll + i / Columns;
                int column = i % Columns;
                if (row >= displayRows.Count) continue;
                if (IsHeaderRow(row))
                {
                    if (column == 0)
                    {
                        string heading = HeaderText(row);
                        Vector2 headingSize = Game1.dialogueFont.MeasureString(heading);
                        float headingScale = Math.Min(1f, (listBounds.Width - 12f) / Math.Max(1f, headingSize.X));
                        DrawShadowedText(b, Game1.dialogueFont, heading,
                            new Vector2(listBounds.X + 4, targetBounds[i].Center.Y - headingSize.Y * headingScale / 2f),
                            Game1.textColor, headingScale);
                    }
                    continue;
                }
                if (column >= displayRows[row].Length) continue;
                if (row == 1 && column == 0)
                {
                    DrawCheckbox(b, targetBounds[i], targets.All(selected.Contains), T("selectAll"));
                    continue;
                }
                var target = displayRows[row][column]!.Value;
                DrawCheckbox(b, targetBounds[i], selected.Contains(target), T("target." + target));
            }
            if (displayRows.Count > visibleRows)
            {
                IClickableMenu.drawTextureBox(b, scrollbarTrack.X - 3, scrollbarTrack.Y - 3, scrollbarTrack.Width + 6, scrollbarTrack.Height + 6, Color.White);
                b.Draw(Game1.staminaRect, scrollbarThumb, new Color(117, 80, 68));
            }
            if (confirming)
            {
                string message = T("confirm").Replace("{{operation}}", T("operation." + pending));
                Vector2 messageSize = Game1.smallFont.MeasureString(message);
                float messageScale = Math.Min(1f, (width - 48f) / Math.Max(1f, messageSize.X));
                DrawShadowedText(b, Game1.smallFont, message, new Vector2(messageBounds.X, messageBounds.Y), Game1.textColor, messageScale);
                DrawButton(b, confirmBounds, T("confirmButton"));
                DrawButton(b, cancelBounds, T("cancel"));
            }
            else
            {
                DrawButton(b, resetBounds, T("operation.Reset"), enabled: selected.Count > 0);
                DrawButton(b, clearBounds, T("operation.Clear"), enabled: selected.Count > 0);
                DrawButton(b, generateBounds, T("operation.Generate"), enabled: selected.Count > 0);
            }
            if (!string.IsNullOrEmpty(status))
            {
                Vector2 statusSize = Game1.smallFont.MeasureString(status);
                float statusScale = Math.Min(1f, (width - 48f) / Math.Max(1f, statusSize.X));
                DrawShadowedText(b, Game1.smallFont, status, new Vector2(statusBounds.X, statusBounds.Y), Game1.textColor, statusScale);
            }
            upperRightCloseButton?.draw(b);
            drawMouse(b);
        }
    }
}
