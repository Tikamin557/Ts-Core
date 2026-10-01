using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using Ts_Core.Models.PolyamorySweetRooms;
using Ts_Core.Services.Relationship;

namespace Ts_Core.Services.PolyamorySweetRoomsRelated
{
    /// <summary>
    /// <c>TsCore/PsrRoomPresets</c>に登録されたPresetを使って、
    /// 各配偶者部屋へ割り当てるNPCをゲーム内で設定するメニューです。
    /// </summary>
    /// <remarks>
    /// この画面上の変更はSaveボタンを確定するまでファイルへ書き込みません。
    /// 保存時は<see cref="PsrRoomContentService"/>を通して対象PSR Content Packの
    /// content.jsonを更新し、実際の部屋反映はゲーム再起動後のPSRに任せます。
    /// </remarks>
    internal sealed class PsrRoomPresetMenu : IClickableMenu
    {
        //----------------------------------------
        // 依存サービス / Preset状態
        //----------------------------------------

        private readonly ITranslationHelper translation;
        private readonly PartnerService partnerService;
        private readonly IModRegistry modRegistry;
        private readonly List<KeyValuePair<string, PsrRoomPresetModel>> presets;
        private int presetIndex;
        private List<string?> assignments = new();
        private bool showAllCandidates;
        private int roomScroll;
        private int? candidateSlot;
        private int candidateScroll;
        private bool confirmSave;
        private bool saveComplete;
        private string? errorMessage;

        //----------------------------------------
        // レイアウト定数 / Hover状態
        //----------------------------------------

        private const int MenuWidth = 920;
        private const int MenuHeight = 820;
        private const int Padding = 36;
        private const int RowHeight = 68;
        private const int MaxVisibleRows = 5;
        private const int MinVisibleRows = 1;
        private int visibleRows = MaxVisibleRows;
        private string? hoverText;

        //----------------------------------------
        // クリック判定領域
        //----------------------------------------

        private Rectangle presetBounds;
        private Rectangle typeBounds;
        private Rectangle saveBounds;
        private Rectangle resetBounds;
        private Rectangle cancelBounds;
        private Rectangle roomScrollBarBounds;
        private readonly List<Rectangle> rowBounds = new();

        //----------------------------------------
        // 初期化
        //----------------------------------------

        /// <summary>現在有効なPresetを読み込み、画面サイズに合わせてメニューを構築します。</summary>
        internal PsrRoomPresetMenu(ITranslationHelper translation, PartnerService partnerService, IModRegistry modRegistry)
        {
            this.translation = translation;
            this.partnerService = partnerService;
            this.modRegistry = modRegistry;
            presets = PsrRoomContentService.GetPresets()
                .Where(p => p.Value != null)
                .OrderBy(p => p.Key, StringComparer.OrdinalIgnoreCase)
                .ToList();

            int w = Math.Min(MenuWidth, Game1.uiViewport.Width - 48);
            int h = Math.Min(MenuHeight, Game1.uiViewport.Height - 48);
            initialize(Game1.uiViewport.Width / 2 - w / 2, Game1.uiViewport.Height / 2 - h / 2, w, h);
            LoadPreset();
            RebuildBounds();
        }

        /// <summary>現在選択中のPresetです。Presetが無い場合はnullです。</summary>
        private PsrRoomPresetModel? CurrentPreset => presets.Count == 0 ? null : presets[presetIndex].Value;

        /// <summary>選択中Presetの既存割り当てをcontent.jsonから読み込み直します。</summary>
        private void LoadPreset()
        {
            roomScroll = 0;
            candidateSlot = null;
            errorMessage = null;
            PsrRoomPresetModel? preset = CurrentPreset;
            assignments = preset == null ? new() : PsrRoomContentService.LoadAssignments(preset);
        }

        /// <summary>
        /// 現在のViewportとメニュー高さから各UI領域を再計算します。
        /// 下部ボタンを先に確保し、残りの高さから表示可能な部屋行数を決定します。
        /// </summary>
        private void RebuildBounds()
        {
            int x = xPositionOnScreen + Padding;
            int contentWidth = width - Padding * 2;
            presetBounds = new Rectangle(x, yPositionOnScreen + 88, contentWidth, 60);
            typeBounds = new Rectangle(x, yPositionOnScreen + 164, contentWidth, 60);

            rowBounds.Clear();
            int rowY = yPositionOnScreen + 330;
            int buttonY = yPositionOnScreen + height - 84;
            const int roomButtonHeight = 58;
            const int roomButtonBottomGap = 24;
            int availableRoomHeight = Math.Max(roomButtonHeight, buttonY - roomButtonBottomGap - rowY);
            visibleRows = Math.Clamp(
                ((availableRoomHeight - roomButtonHeight) / RowHeight) + 1,
                MinVisibleRows,
                MaxVisibleRows);

            int scrollBarWidth = 20;
            int scrollBarGap = 12;
            int rowWidth = contentWidth - scrollBarWidth - scrollBarGap;
            for (int i = 0; i < visibleRows; i++)
            {
                rowBounds.Add(new Rectangle(x, rowY + i * RowHeight, rowWidth, roomButtonHeight));
            }

            roomScrollBarBounds = new Rectangle(
                x + rowWidth + scrollBarGap,
                rowY,
                scrollBarWidth,
                (visibleRows - 1) * RowHeight + roomButtonHeight);

            int bw = (contentWidth - 24) / 3;
            saveBounds = new Rectangle(x, buttonY, bw, 60);
            resetBounds = new Rectangle(x + bw + 12, buttonY, bw, 60);
            cancelBounds = new Rectangle(x + (bw + 12) * 2, buttonY, bw, 60);
        }

        //----------------------------------------
        // 入力処理
        //----------------------------------------

        /// <summary>Preset切替、候補選択、Save/Reset/Cancel等の左クリックを処理します。</summary>
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (saveComplete)
            {
                if (GetModalOkBounds().Contains(x, y))
                {
                    saveComplete = false;
                    Game1.exitActiveMenu();
                }
                return;
            }

            if (confirmSave)
            {
                Rectangle yes = GetModalYesBounds();
                Rectangle no = GetModalNoBounds();
                if (yes.Contains(x, y))
                {
                    confirmSave = false;
                    Save();
                }
                else if (no.Contains(x, y))
                {
                    confirmSave = false;
                    Game1.playSound("bigDeSelect");
                }
                return;
            }

            if (candidateSlot.HasValue)
            {
                HandleCandidateClick(x, y);
                return;
            }

            if (presets.Count > 1 && presetBounds.Contains(x, y))
            {
                presetIndex = (presetIndex + 1) % presets.Count;
                LoadPreset();
                Game1.playSound("shwip");
                return;
            }

            if (typeBounds.Contains(x, y))
            {
                showAllCandidates = !showAllCandidates;
                Game1.playSound("shwip");
                return;
            }

            PsrRoomPresetModel? preset = CurrentPreset;
            if (preset != null)
            {
                if (preset.Slots.Count > visibleRows && roomScrollBarBounds.Contains(x, y))
                {
                    int maxRoom = preset.Slots.Count - visibleRows;
                    Rectangle thumb = GetRoomScrollThumbBounds(preset.Slots.Count);
                    if (y < thumb.Top)
                        roomScroll = Math.Max(0, roomScroll - visibleRows);
                    else if (y > thumb.Bottom)
                        roomScroll = Math.Min(maxRoom, roomScroll + visibleRows);
                    Game1.playSound("shwip");
                    return;
                }

                int visible = Math.Min(visibleRows, preset.Slots.Count - roomScroll);
                for (int i = 0; i < visible; i++)
                {
                    Rectangle choice = new(
                        rowBounds[i].X + rowBounds[i].Width / 2,
                        rowBounds[i].Y,
                        rowBounds[i].Width / 2,
                        rowBounds[i].Height);
                    if (choice.Contains(x, y))
                    {
                        candidateSlot = roomScroll + i;
                        candidateScroll = 0;
                        Game1.playSound("smallSelect");
                        return;
                    }
                }
            }

            if (saveBounds.Contains(x, y) && CurrentPreset != null)
            {
                if (!PsrRoomContentService.IsContentPackAvailable(CurrentPreset))
                {
                    errorMessage = translation.Get("psrPreset.error.contentPackMissing", new { ModId = CurrentPreset.PsrContentPackId });
                    Game1.playSound("cancel");
                }
                else
                {
                    confirmSave = true;
                    Game1.playSound("smallSelect");
                }
                return;
            }

            if (resetBounds.Contains(x, y) && CurrentPreset != null)
            {
                assignments = CurrentPreset.Slots.Select(_ => (string?)null).ToList();
                Game1.playSound("trashcan");
                return;
            }

            if (cancelBounds.Contains(x, y))
            {
                Game1.exitActiveMenu();
                Game1.playSound("bigDeSelect");
            }
        }

        /// <summary>部屋の割り当てボタンを右クリックした時、その部屋だけ未割り当てへ戻します。</summary>
        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            if (confirmSave || saveComplete || candidateSlot.HasValue)
                return;

            PsrRoomPresetModel? preset = CurrentPreset;
            if (preset == null)
                return;

            int visible = Math.Min(visibleRows, preset.Slots.Count - roomScroll);
            for (int i = 0; i < visible; i++)
            {
                Rectangle choice = new(
                    rowBounds[i].X + rowBounds[i].Width / 2,
                    rowBounds[i].Y,
                    rowBounds[i].Width / 2,
                    rowBounds[i].Height);

                if (!choice.Contains(x, y))
                    continue;

                int index = roomScroll + i;
                if (index < assignments.Count)
                    assignments[index] = null;

                Game1.playSound("trashcan");
                return;
            }
        }

        /// <summary>部屋一覧または候補一覧のスクロールを処理します。</summary>
        public override void receiveScrollWheelAction(int direction)
        {
            if (confirmSave || saveComplete)
                return;

            if (candidateSlot.HasValue)
            {
                List<string?> candidates = GetCandidates(candidateSlot.Value);
                int max = Math.Max(0, candidates.Count - 8);
                candidateScroll = Math.Clamp(candidateScroll + (direction < 0 ? 1 : -1), 0, max);
                return;
            }

            PsrRoomPresetModel? preset = CurrentPreset;
            if (preset == null)
                return;

            int maxRoom = Math.Max(0, preset.Slots.Count - visibleRows);
            roomScroll = Math.Clamp(roomScroll + (direction < 0 ? 1 : -1), 0, maxRoom);
        }

        /// <summary>Escape等のキー入力による候補一覧・確認画面・メニューの終了を処理します。</summary>
        public override void receiveKeyPress(Microsoft.Xna.Framework.Input.Keys key)
        {
            if (key == Microsoft.Xna.Framework.Input.Keys.Escape)
            {
                if (saveComplete) { Game1.exitActiveMenu(); return; }
                if (confirmSave) { confirmSave = false; return; }
                if (candidateSlot.HasValue) { candidateSlot = null; return; }
                Game1.exitActiveMenu();
                return;
            }
            base.receiveKeyPress(key);
        }

        /// <summary>候補一覧内のNPCクリックを処理し、重複割り当てを防止します。</summary>
        private void HandleCandidateClick(int x, int y)
        {
            int slot = candidateSlot!.Value;
            Rectangle list = GetCandidateListBounds();
            if (!list.Contains(x, y))
            {
                candidateSlot = null;
                return;
            }

            List<string?> candidates = GetCandidates(slot);
            int localY = y - (list.Y + 16);
            int visibleIndex = localY / 50;
            int index = candidateScroll + visibleIndex;
            if (visibleIndex >= 0 && visibleIndex < 8
                && index >= 0 && index < candidates.Count)
            {
                string? selected = candidates[index];
                if (selected == null || !IsAssignedElsewhere(selected, slot))
                {
                    assignments[slot] = selected;
                    candidateSlot = null;
                    Game1.playSound("coin");
                }
                else
                {
                    Game1.playSound("cancel");
                }
            }
        }

        //----------------------------------------
        // NPC候補 / 割り当て判定
        //----------------------------------------

        /// <summary>現在の候補表示モードに応じたNPC一覧を作成します。先頭のnullは未割り当てです。</summary>
        private List<string?> GetCandidates(int currentSlot)
        {
            IEnumerable<string> source = showAllCandidates
                ? RomanceCandidateService.GetCandidates(GetActiveAdditionalCandidates())
                : partnerService.GetPartners() ?? Enumerable.Empty<string>();

            List<string?> result = new() { null };
            result.AddRange(source
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(RomanceCandidateService.GetDisplayName, StringComparer.CurrentCultureIgnoreCase));

            string? current = currentSlot < assignments.Count ? assignments[currentSlot] : null;
            if (!string.IsNullOrWhiteSpace(current)
                && !result.OfType<string>().Contains(current, StringComparer.OrdinalIgnoreCase))
            {
                result.Add(current);
            }
            return result;
        }

        /// <summary>提供Modが現在ロードされているAdditionalCandidatesだけを返します。</summary>
        private IEnumerable<string> GetActiveAdditionalCandidates()
        {
            if (CurrentPreset?.AdditionalCandidates == null)
                yield break;

            foreach ((string npcName, PsrRoomAdditionalCandidateModel candidate) in CurrentPreset.AdditionalCandidates)
            {
                if (string.IsNullOrWhiteSpace(npcName)
                    || candidate == null
                    || string.IsNullOrWhiteSpace(candidate.ModId)
                    || !modRegistry.IsLoaded(candidate.ModId.Trim()))
                {
                    continue;
                }

                yield return npcName.Trim();
            }
        }

        /// <summary>指定NPCが現在有効なAdditionalCandidateか確認します。</summary>
        private bool IsActiveAdditionalCandidate(string npcName)
        {
            if (CurrentPreset?.AdditionalCandidates == null)
                return false;

            KeyValuePair<string, PsrRoomAdditionalCandidateModel> entry = CurrentPreset.AdditionalCandidates
                .FirstOrDefault(pair => string.Equals(pair.Key?.Trim(), npcName.Trim(), StringComparison.OrdinalIgnoreCase));
            PsrRoomAdditionalCandidateModel? candidate = entry.Value;
            if (candidate == null || string.IsNullOrWhiteSpace(candidate.ModId))
                return false;

            return modRegistry.IsLoaded(candidate.ModId.Trim());
        }

        /// <summary>指定NPCが現在のSlot以外へ既に割り当てられているか確認します。</summary>
        private bool IsAssignedElsewhere(string name, int currentSlot)
        {
            for (int i = 0; i < assignments.Count; i++)
            {
                if (i != currentSlot && string.Equals(assignments[i], name, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        //----------------------------------------
        // 保存
        //----------------------------------------

        /// <summary>現在の画面上の割り当てを対象PSR Content Packへ保存します。</summary>
        private void Save()
        {
            if (CurrentPreset == null)
                return;

            if (PsrRoomContentService.Save(CurrentPreset, assignments, out string? error))
            {
                saveComplete = true;
                errorMessage = null;
                Game1.playSound("newArtifact");
            }
            else
            {
                errorMessage = translation.Get("psrPreset.error.save", new { Error = error ?? "Unknown error" });
                Game1.playSound("cancel");
            }
        }

        //----------------------------------------
        // Hover判定
        //----------------------------------------

        /// <summary>
        /// DisplayNameのHoverText、削除済みNPC、他室へ割り当て済みNPCの
        /// Hover説明をカーソル位置から判定します。
        /// </summary>
        public override void performHoverAction(int x, int y)
        {
            hoverText = null;

            if (!confirmSave && !saveComplete)
            {
                if (candidateSlot.HasValue)
                {
                    int slot = candidateSlot.Value;
                    Rectangle list = GetCandidateListBounds();
                    if (list.Contains(x, y))
                    {
                        List<string?> candidates = GetCandidates(slot);
                        int localY = y - (list.Y + 16);
                        int visibleIndex = localY / 50;
                        int index = candidateScroll + visibleIndex;
                        if (visibleIndex >= 0 && visibleIndex < 8
                            && index >= 0 && index < candidates.Count
                            && candidates[index] is string name
                            && IsAssignedElsewhere(name, slot))
                        {
                            hoverText = translation.Get("psrPreset.hover.assignedElsewhere");
                        }
                    }
                }
                else
                {
                    PsrRoomPresetModel? preset = CurrentPreset;
                    if (preset != null)
                    {
                        int visible = Math.Min(visibleRows, Math.Max(0, preset.Slots.Count - roomScroll));
                        for (int i = 0; i < visible; i++)
                        {
                            int index = roomScroll + i;
                            PsrRoomPresetSlotModel slot = preset.Slots[index];
                            string displayName = !string.IsNullOrWhiteSpace(slot.DisplayName)
                                ? slot.DisplayName
                                : string.IsNullOrWhiteSpace(slot.Id) ? $"Slot {index + 1}" : slot.Id;
                            Vector2 displayNameSize = Game1.smallFont.MeasureString(displayName);
                            Rectangle displayNameBounds = new(
                                rowBounds[i].X + 8,
                                rowBounds[i].Y + 12,
                                (int)MathF.Ceiling(displayNameSize.X),
                                (int)MathF.Ceiling(displayNameSize.Y));

                            if (displayNameBounds.Contains(x, y)
                                && !string.IsNullOrWhiteSpace(slot.HoverText))
                            {
                                hoverText = slot.HoverText;
                                break;
                            }

                            Rectangle choice = new(
                                rowBounds[i].X + rowBounds[i].Width / 2,
                                rowBounds[i].Y,
                                rowBounds[i].Width / 2,
                                rowBounds[i].Height);

                            string? assigned = index < assignments.Count ? assignments[index] : null;
                            if (choice.Contains(x, y)
                                && !string.IsNullOrWhiteSpace(assigned)
                                && IsMissingCharacter(assigned))
                            {
                                hoverText = translation.Get("psrPreset.hover.missingCharacter");
                                break;
                            }
                        }
                    }
                }
            }

            base.performHoverAction(x, y);
        }

        //----------------------------------------
        // メイン描画
        //----------------------------------------

        /// <summary>設定画面本体と、必要に応じて候補一覧・確認Modal・Hoverを描画します。</summary>
        public override void draw(SpriteBatch b)
        {
            b.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * 0.55f);
            IClickableMenu.drawTextureBox(b, xPositionOnScreen, yPositionOnScreen, width, height, Color.White);

            string title = translation.Get("psrPreset.title");
            Utility.drawTextWithShadow(b, title, Game1.dialogueFont,
                new Vector2(xPositionOnScreen + width / 2 - Game1.dialogueFont.MeasureString(title).X / 2, yPositionOnScreen + 22), Game1.textColor);

            if (presets.Count == 0)
            {
                DrawCentered(b, translation.Get("psrPreset.noPreset"), yPositionOnScreen + 180, Game1.smallFont, Game1.textColor);
                DrawButton(b, cancelBounds, translation.Get("psrPreset.cancel"));
                drawMouse(b);
                return;
            }

            string presetText = translation.Get("psrPreset.target", new { Id = presets[presetIndex].Key });
            DrawButton(b, presetBounds, presetText);

            string typeText = showAllCandidates
                ? translation.Get("psrPreset.type.all")
                : translation.Get("psrPreset.type.current");
            DrawButton(b, typeBounds, translation.Get("psrPreset.type", new { Type = typeText }));

            string warning = translation.Get("psrPreset.warning");
            Utility.drawTextWithShadow(b, warning, Game1.smallFont,
                new Vector2(xPositionOnScreen + Padding, yPositionOnScreen + 244), Color.DarkRed);

            DrawRooms(b);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                DrawCentered(b, errorMessage, yPositionOnScreen + height - 116, Game1.smallFont, Color.DarkRed);
            }

            DrawButton(b, saveBounds, translation.Get("psrPreset.save"));
            DrawButton(b, resetBounds, translation.Get("psrPreset.reset"));
            DrawButton(b, cancelBounds, translation.Get("psrPreset.cancel"));

            if (candidateSlot.HasValue)
                DrawCandidateList(b, candidateSlot.Value);
            if (confirmSave)
                DrawSaveConfirmation(b);
            if (saveComplete)
                DrawSaveComplete(b);

            if (!string.IsNullOrWhiteSpace(hoverText))
                drawHoverText(b, hoverText, Game1.smallFont);

            drawMouse(b);
        }

        //----------------------------------------
        // 部屋一覧描画
        //----------------------------------------

        /// <summary>現在のData/Charactersにも有効なAdditionalCandidatesにも存在しないNPCか判定します。</summary>
        private bool IsMissingCharacter(string npcName)
        {
            if (RomanceCandidateService.Exists(npcName))
                return false;

            return !IsActiveAdditionalCandidate(npcName);
        }

        /// <summary>現在のスクロール位置から表示対象の部屋行と割り当てボタンを描画します。</summary>
        private void DrawRooms(SpriteBatch b)
        {
            PsrRoomPresetModel preset = CurrentPreset!;
            int visible = Math.Min(visibleRows, Math.Max(0, preset.Slots.Count - roomScroll));
            for (int i = 0; i < visible; i++)
            {
                int index = roomScroll + i;
                Rectangle bounds = rowBounds[i];
                PsrRoomPresetSlotModel slot = preset.Slots[index];
                string displayName = !string.IsNullOrWhiteSpace(slot.DisplayName)
                    ? slot.DisplayName
                    : string.IsNullOrWhiteSpace(slot.Id) ? $"Slot {index + 1}" : slot.Id;
                Utility.drawTextWithShadow(b, displayName, Game1.smallFont, new Vector2(bounds.X + 8, bounds.Y + 12), Game1.textColor);

                Rectangle choice = new(bounds.X + bounds.Width / 2, bounds.Y, bounds.Width / 2, bounds.Height);
                string value = assignments.Count > index && !string.IsNullOrWhiteSpace(assignments[index])
                    ? RomanceCandidateService.GetDisplayName(assignments[index]!)
                    : translation.Get("psrPreset.unassigned");
                bool missingCharacter = assignments.Count > index
                    && !string.IsNullOrWhiteSpace(assignments[index])
                    && IsMissingCharacter(assignments[index]!);
                DrawButton(b, choice, missingCharacter ? value + "  *" : value);
            }

            if (preset.Slots.Count > visibleRows)
            {
                DrawScrollBarTrack(b, roomScrollBarBounds);

                Rectangle thumb = GetRoomScrollThumbBounds(preset.Slots.Count);
                DrawScrollBarThumb(b, thumb);
            }
        }

        /// <summary>部屋数と現在位置からスクロールバーのThumb領域を計算します。</summary>
        private Rectangle GetRoomScrollThumbBounds(int totalRooms)
        {
            if (totalRooms <= visibleRows)
                return roomScrollBarBounds;

            int maxRoom = totalRooms - visibleRows;
            int thumbHeight = Math.Max(48, roomScrollBarBounds.Height * visibleRows / totalRooms);
            int travel = roomScrollBarBounds.Height - thumbHeight;
            int thumbY = roomScrollBarBounds.Y + (maxRoom == 0 ? 0 : travel * roomScroll / maxRoom);
            return new Rectangle(roomScrollBarBounds.X, thumbY, roomScrollBarBounds.Width, thumbHeight);
        }

        //----------------------------------------
        // NPC候補一覧描画
        //----------------------------------------

        /// <summary>選択中Slotへ割り当て可能なNPC候補一覧をModal表示します。</summary>
        private void DrawCandidateList(SpriteBatch b, int slot)
        {
            b.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * 0.35f);
            Rectangle list = GetCandidateListBounds();
            IClickableMenu.drawTextureBox(b, list.X, list.Y, list.Width, list.Height, Color.White);
            List<string?> candidates = GetCandidates(slot);
            int visible = Math.Min(8, candidates.Count - candidateScroll);
            for (int i = 0; i < visible; i++)
            {
                int index = candidateScroll + i;
                string? name = candidates[index];
                string text = name == null ? translation.Get("psrPreset.unassigned") : RomanceCandidateService.GetDisplayName(name);
                bool disabled = name != null && IsAssignedElsewhere(name, slot);
                Color color = disabled ? Color.Gray : Game1.textColor;
                Utility.drawTextWithShadow(b, text, Game1.smallFont, new Vector2(list.X + 24, list.Y + 18 + i * 50), color);
            }
        }

        /// <summary>NPC候補一覧の表示領域を返します。</summary>
        private Rectangle GetCandidateListBounds()
        {
            int w = Math.Min(560, width - 96);
            return new Rectangle(xPositionOnScreen + width / 2 - w / 2, yPositionOnScreen + 176, w, 432);
        }

        //----------------------------------------
        // 保存確認 / 完了Modal
        //----------------------------------------

        /// <summary>content.json上書き前の確認Modalを描画します。</summary>
        private void DrawSaveConfirmation(SpriteBatch b)
        {
            DrawModalBase(b, translation.Get("psrPreset.confirm.title"), translation.Get("psrPreset.confirm.message"));
            DrawButton(b, GetModalYesBounds(), translation.Get("psrPreset.yes"));
            DrawButton(b, GetModalNoBounds(), translation.Get("psrPreset.no"));
        }

        /// <summary>保存成功後の再起動注意を含む完了Modalを描画します。</summary>
        private void DrawSaveComplete(SpriteBatch b)
        {
            DrawModalBase(b, translation.Get("psrPreset.complete.title"), translation.Get("psrPreset.complete.message"));
            DrawButton(b, GetModalOkBounds(), "OK");
        }

        /// <summary>確認・完了Modal共通の背景、タイトル、折り返し本文を描画します。</summary>
        private void DrawModalBase(SpriteBatch b, string title, string message)
        {
            b.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * 0.5f);
            Rectangle modal = GetModalBounds();
            IClickableMenu.drawTextureBox(b, modal.X, modal.Y, modal.Width, modal.Height, Color.White);
            DrawCentered(b, title, modal.Y + 32, Game1.dialogueFont, Game1.textColor);

            string wrapped = Game1.parseText(message, Game1.smallFont, modal.Width - 80);
            Vector2 size = Game1.smallFont.MeasureString(wrapped);
            Utility.drawTextWithShadow(
                b,
                wrapped,
                Game1.smallFont,
                new Vector2(modal.Center.X - size.X / 2, modal.Y + 118),
                Game1.textColor);
        }

        /// <summary>現在のViewport内に収まるModal領域を返します。</summary>
        private Rectangle GetModalBounds()
        {
            int w = Math.Min(760, Game1.uiViewport.Width - 80);
            int h = Math.Min(400, Game1.uiViewport.Height - 80);
            return new Rectangle(Game1.uiViewport.Width / 2 - w / 2, Game1.uiViewport.Height / 2 - h / 2, w, h);
        }

        private Rectangle GetModalYesBounds()
        {
            Rectangle modal = GetModalBounds();
            return new Rectangle(modal.Center.X - 210, modal.Bottom - 88, 180, 60);
        }

        private Rectangle GetModalNoBounds()
        {
            Rectangle modal = GetModalBounds();
            return new Rectangle(modal.Center.X + 30, modal.Bottom - 88, 180, 60);
        }

        private Rectangle GetModalOkBounds()
        {
            Rectangle modal = GetModalBounds();
            return new Rectangle(modal.Center.X - 90, modal.Bottom - 88, 180, 60);
        }

        //----------------------------------------
        // 共通描画Helper
        //----------------------------------------

        /// <summary>T's Core PSR画面共通のボタンを控えめな影付きで描画します。</summary>
        private static void DrawButton(SpriteBatch b, Rectangle bounds, string text)
        {
            // Vanillaの大きな影より少し控えめな影を付ける。
            const int shadowOffset = 4;
            IClickableMenu.drawTextureBox(
                b,
                Game1.menuTexture,
                new Rectangle(0, 256, 60, 60),
                bounds.X + shadowOffset,
                bounds.Y + shadowOffset,
                bounds.Width,
                bounds.Height,
                Color.Black * 0.35f,
                1f,
                drawShadow: false);

            IClickableMenu.drawTextureBox(
                b,
                Game1.menuTexture,
                new Rectangle(0, 256, 60, 60),
                bounds.X,
                bounds.Y,
                bounds.Width,
                bounds.Height,
                Color.White,
                1f,
                drawShadow: false);

            Vector2 size = Game1.smallFont.MeasureString(text);
            Utility.drawTextWithShadow(b, text, Game1.smallFont,
                new Vector2(bounds.Center.X - size.X / 2, bounds.Center.Y - size.Y / 2), Game1.textColor);
        }

        /// <summary>細幅でも潰れない単純矩形のスクロールバートラックを描画します。</summary>
        private static void DrawScrollBarTrack(SpriteBatch b, Rectangle bounds)
        {
            // 細い領域にTextureBoxを縮小すると枠と影が重なって見えるため、
            // スクロールバーだけは単純な矩形で描画する。
            b.Draw(Game1.staminaRect, bounds, Color.Black * 0.28f);

            Rectangle inner = new(bounds.X + 3, bounds.Y + 3, bounds.Width - 6, bounds.Height - 6);
            b.Draw(Game1.staminaRect, inner, Color.Wheat * 0.45f);
        }

        /// <summary>スクロールバーのThumbを描画します。</summary>
        private static void DrawScrollBarThumb(SpriteBatch b, Rectangle bounds)
        {
            b.Draw(Game1.staminaRect, bounds, Color.Black * 0.45f);

            Rectangle inner = new(bounds.X + 3, bounds.Y + 3, bounds.Width - 6, bounds.Height - 6);
            b.Draw(Game1.staminaRect, inner, Color.Wheat);
        }

        /// <summary>指定Y位置へテキストを画面中央揃えで描画します。</summary>
        private static void DrawCentered(SpriteBatch b, string text, int y, SpriteFont font, Color color)
        {
            Vector2 size = font.MeasureString(text);
            Utility.drawTextWithShadow(b, text, font, new Vector2(Game1.uiViewport.Width / 2 - size.X / 2, y), color);
        }
    }
}
