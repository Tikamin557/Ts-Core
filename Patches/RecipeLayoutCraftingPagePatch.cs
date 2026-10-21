using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Ts_Core.Services;

namespace Ts_Core.Patches
{
    /// <summary>
    /// PC版CraftingPage内で通常レシピとTsCore/CustomRecipePagesのレシピを切り替えます。
    /// インベントリ・作成操作はバニラのCraftingPageをそのまま使用します。
    /// </summary>
    public static class RecipeLayoutCraftingPagePatch
    {
        private sealed class PageState
        {
            public bool DedicatedView;
            public bool HasGroups;
            public bool InitialLayoutPrepared;
            public HashSet<string> Starts { get; } = new(StringComparer.Ordinal);
            public HashSet<string> Ends { get; } = new(StringComparer.Ordinal);
            public bool HasPreviousRecipe;
            public bool PreviousEndedGroup;
        }

        private static readonly ConditionalWeakTable<CraftingPage, PageState> States = new();
        private static IMonitor? monitor;
        private static ITranslationHelper? translations;
        private static MethodInfo? reposition;

        public static void Apply(Harmony harmony, IMonitor logger, ITranslationHelper translation)
        {
            monitor = logger;
            translations = translation;
            reposition = AccessTools.Method(typeof(CraftingPage), "RepositionElements");
            if (reposition == null)
                return; // Android版は対象外
            MethodInfo? getRecipes = AccessTools.Method(typeof(CraftingPage), "GetRecipesToDisplay");
            MethodInfo? layout = AccessTools.Method(typeof(CraftingPage), "layoutRecipes");
            MethodInfo? click = AccessTools.Method(typeof(CraftingPage), "receiveLeftClick", new[] { typeof(int), typeof(int), typeof(bool) });
            MethodInfo? draw = AccessTools.Method(typeof(CraftingPage), "draw", new[] { typeof(SpriteBatch) });
            if (getRecipes != null)
                harmony.Patch(getRecipes, postfix: new HarmonyMethod(typeof(RecipeLayoutCraftingPagePatch), nameof(GetRecipesPostfix)));
            if (layout != null)
                harmony.Patch(layout, transpiler: new HarmonyMethod(typeof(RecipeLayoutCraftingPagePatch), nameof(LayoutTranspiler)));
            if (click != null)
                harmony.Patch(click, prefix: new HarmonyMethod(typeof(RecipeLayoutCraftingPagePatch), nameof(ClickPrefix)));
            if (draw != null)
                harmony.Patch(draw,
                    prefix: new HarmonyMethod(typeof(RecipeLayoutCraftingPagePatch), nameof(DrawPrefix)),
                    postfix: new HarmonyMethod(typeof(RecipeLayoutCraftingPagePatch), nameof(DrawPostfix)));
        }

        private static Rectangle ButtonBounds(CraftingPage page)
        {
            // レシピグリッドの左上側。メニューの移動・UI Scaleに追従します。
            return new Rectangle(page.xPositionOnScreen - 72, page.yPositionOnScreen + 80, 64, 64);
        }

        private static bool ClickPrefix(CraftingPage __instance, int x, int y)
        {
            PageState state = States.GetValue(__instance, _ => new PageState());
            if (!state.HasGroups || !ButtonBounds(__instance).Contains(x, y))
                return true;
            state.DedicatedView = !state.DedicatedView;
            __instance.currentCraftingPage = 0;
            try
            {
                reposition?.Invoke(__instance, null);
                Game1.playSound("smallSelect");
            }
            catch (Exception ex)
            {
                state.DedicatedView = !state.DedicatedView;
                monitor?.Log($"CustomRecipePages: failed to switch view: {ex}", LogLevel.Error);
            }
            return false;
        }

        /// <summary>
        /// 初回描画前にレシピ配置を確定し、初回の配置変更が画面に映らないようにします。
        /// </summary>
        private static void DrawPrefix(CraftingPage __instance)
        {
            PageState state = States.GetValue(__instance, _ => new PageState());
            if (!state.InitialLayoutPrepared)
            {
                state.InitialLayoutPrepared = true;
                try
                {
                    reposition?.Invoke(__instance, null);
                }
                catch (Exception ex)
                {
                    monitor?.Log($"CustomRecipePages: failed to prepare initial layout: {ex}", LogLevel.Warn);
                }
            }
            DrawToggleButton(__instance, Game1.spriteBatch, state);
        }

        /// <summary>
        /// ボタンはCraftingPage本体より前に描画し、バニラのカーソルが上に重なるようにします。
        /// </summary>
        private static void DrawToggleButton(CraftingPage page, SpriteBatch b, PageState state)
        {
            if (!state.HasGroups)
                return;
            Rectangle bounds = ButtonBounds(page);
            IClickableMenu.drawTextureBox(
                b, Game1.menuTexture, new Rectangle(0, 256, 60, 60),
                bounds.X, bounds.Y, bounds.Width, bounds.Height,
                Color.White, 1f, drawShadow: false);
            string label = state.DedicatedView ? "T" : "V";
            Vector2 size = Game1.smallFont.MeasureString(label);
            b.DrawString(Game1.smallFont, label,
                new Vector2(bounds.Center.X - size.X / 2, bounds.Center.Y - size.Y / 2), Game1.textColor);
        }

        /// <summary>
        /// ホバー説明は通常のメニュー描画後に表示します。
        /// 表示するのは現在のモードではなく、クリック後に切り替わるモードです。
        /// </summary>
        private static void DrawPostfix(CraftingPage __instance, SpriteBatch b)
        {
            if (!States.TryGetValue(__instance, out PageState? state) || !state.HasGroups ||
                !ButtonBounds(__instance).Contains(Game1.getMouseX(), Game1.getMouseY()))
                return;
            string hoverText = state.DedicatedView
                ? translations?.Get("customRecipePages.showVanilla") ?? "Show Vanilla Menu"
                : translations?.Get("customRecipePages.showTsCore") ?? "Show T's Core Menu";
            IClickableMenu.drawHoverText(b, hoverText, Game1.smallFont);
        }

        private static void GetRecipesPostfix(CraftingPage __instance, ref List<string> __result)
        {
            PageState state = States.GetValue(__instance, _ => new PageState());
            state.Starts.Clear();
            state.Ends.Clear();
            state.HasPreviousRecipe = false;
            state.PreviousEndedGroup = false;

            var groups = CustomRecipePageDataService.LoadGroups();
            // バニラのDataアセットには未習得レシピも含まれます。
            Dictionary<string, string> all;
            try
            {
                all = Game1.content.Load<Dictionary<string, string>>(
                    __instance.cooking ? "Data/CookingRecipes" : "Data/CraftingRecipes");
            }
            catch
            {
                state.HasGroups = false;
                return;
            }
            var entries = CustomRecipePageDataService.GetEntries(groups, all.Keys, __instance.cooking, monitor);
            state.HasGroups = entries.Count > 0;
            if (!state.HasGroups)
            {
                state.DedicatedView = false;
                return;
            }

            var assigned = new HashSet<string>(StringComparer.Ordinal);
            var dedicated = new List<string>();
            var available = new HashSet<string>(__result, StringComparer.Ordinal);
            foreach (var entry in entries)
            {
                assigned.Add(entry.Id);
                if (!available.Contains(entry.Id))
                    continue;
                dedicated.Add(entry.Id);
                if (entry.DedicatedStart)
                    state.Starts.Add(entry.Id);
                if (entry.DedicatedEnd)
                    state.Ends.Add(entry.Id);
            }
            if (state.DedicatedView)
                __result = dedicated;
            else
            {
                __result = __result.FindAll(id => !assigned.Contains(id));
                state.Starts.Clear();
                state.Ends.Clear();
            }
        }

        /// <summary>
        /// 専用行の前後で改行します。大型クラフトの占有が残る行は飛ばします。
        /// </summary>
        private static void BeforeRecipe(CraftingPage page, CraftingRecipe recipe, object grid, ref int column, ref int row)
        {
            if (!States.TryGetValue(page, out PageState? state) ||
                !state.DedicatedView || (state.Starts.Count == 0 && state.Ends.Count == 0))
                return;
            bool newRow = state.HasPreviousRecipe &&
                (state.PreviousEndedGroup || state.Starts.Contains(recipe.name));
            if (newRow && column != 0)
            {
                column = 0;
                row++;
            }
            if (state.Starts.Contains(recipe.name) && column == 0)
            {
                while (row < 4 && RowHasOccupiedCells(grid, row))
                    row++;
            }
            state.PreviousEndedGroup = state.Ends.Contains(recipe.name);
            state.HasPreviousRecipe = true;
        }

        private static bool RowHasOccupiedCells(object grid, int row)
        {
            if (grid is not Array cells || cells.Rank != 2 || row >= cells.GetLength(1))
                return false;
            for (int column = 0; column < cells.GetLength(0); column++)
                if (cells.GetValue(column, row) != null)
                    return true;
            return false;
        }

        private static bool IsLocal(CodeInstruction instruction, int index, bool load)
        {
            OpCode opcode = instruction.opcode;
            if (load && index == 4 && opcode == OpCodes.Ldloc_S ||
                !load && index == 9 && opcode == OpCodes.Stloc_S)
                return instruction.operand is LocalBuilder local ? local.LocalIndex == index :
                    instruction.operand is byte b ? b == index :
                    instruction.operand is int i && i == index;
            return false;
        }

        /// <summary>
        /// 既存のRowOccupancyFixで確認済みのIL挿入位置を維持します。
        /// </summary>
        private static IEnumerable<CodeInstruction> LayoutTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var codes = new List<CodeInstruction>(instructions);
            MethodInfo? callback = AccessTools.Method(typeof(RecipeLayoutCraftingPagePatch), nameof(BeforeRecipe));
            if (callback == null)
                return codes;
            int storeIndex = -1;
            for (int i = 1; i + 1 < codes.Count; i++)
            {
                if (IsLocal(codes[i], 9, false) &&
                    (codes[i + 1].opcode == OpCodes.Br || codes[i + 1].opcode == OpCodes.Br_S) &&
                    (codes[i - 1].opcode == OpCodes.Newobj || codes[i - 1].opcode == OpCodes.Call || codes[i - 1].opcode == OpCodes.Callvirt))
                {
                    storeIndex = i;
                    break;
                }
            }
            if (storeIndex < 0)
            {
                monitor?.Log("CustomRecipePages: layoutRecipes patch skipped (recipe local/branch pattern not found).", LogLevel.Warn);
                return codes;
            }
            int branchIndex = storeIndex + 1;
            int rowCheckIndex = -1;
            for (int i = branchIndex + 1; i + 2 < codes.Count; i++)
            {
                if (IsLocal(codes[i], 4, true) && codes[i + 1].opcode == OpCodes.Ldc_I4_4 &&
                    (codes[i + 2].opcode == OpCodes.Blt || codes[i + 2].opcode == OpCodes.Blt_S))
                {
                    rowCheckIndex = i;
                    break;
                }
            }
            if (rowCheckIndex < 0)
            {
                monitor?.Log("CustomRecipePages: layoutRecipes patch skipped (row boundary check not found).", LogLevel.Warn);
                return codes;
            }
            var rowCheckLabel = generator.DefineLabel();
            codes[rowCheckIndex].labels.Add(rowCheckLabel);
            codes[branchIndex].opcode = OpCodes.Br;
            codes[branchIndex].operand = rowCheckLabel;
            codes.InsertRange(branchIndex, new[]
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldloc_S, (byte)9),
                new CodeInstruction(OpCodes.Ldloc_S, (byte)6),
                new CodeInstruction(OpCodes.Ldloca_S, (byte)3),
                new CodeInstruction(OpCodes.Ldloca_S, (byte)4),
                new CodeInstruction(OpCodes.Call, callback)
            });
            return codes;
        }
    }
}
