using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Ts_Core.Services;

namespace Ts_Core.Patches
{
    /// <summary>
    /// Better Craftingのレシピ配置用コピーだけを絞り込みます。
    /// 元のカテゴリ・検索対象・レシピデータには変更を加えません。
    /// </summary>
    internal static class CustomRecipePageBetterCraftingPatch
    {
        private sealed class PageState
        {
            public bool Dedicated;
            public bool HasRecipes;
        }

        private static readonly ConditionalWeakTable<object, PageState> States = new();
        private static IMonitor? monitor;
        private static ITranslationHelper? translations;
        private static MethodInfo? layout;
        private static PropertyInfo? editing;
        private static FieldInfo? filter;

        public static void Apply(Harmony harmony, IMonitor logger, ITranslationHelper translation, IModHelper helper)
        {
            Type? type = AccessTools.TypeByName("Leclair.Stardew.BetterCrafting.Menus.BetterCraftingPage");
            if (type == null)
                return;
            MethodInfo? layoutMethod = AccessTools.Method(type, "LayoutRecipes");
            MethodInfo? draw = AccessTools.Method(type, "draw", new[] { typeof(SpriteBatch) });
            MethodInfo? click = AccessTools.Method(type, "receiveLeftClick", new[] { typeof(int), typeof(int), typeof(bool) });
            if (layoutMethod == null || draw == null || click == null)
                return;
            monitor = logger;
            translations = translation;
            layout = layoutMethod;
            editing = AccessTools.Property(type, "Editing");
            filter = AccessTools.Field(type, "Filter");
            try
            {
                harmony.Patch(layoutMethod, transpiler: new HarmonyMethod(typeof(CustomRecipePageBetterCraftingPatch), nameof(LayoutTranspiler)));
                // Better Craftingのdraw()内ではなく、SMAPIのメニュー描画完了後に描画します。
                // メニューのカーソル描画はボタン描画後に行います。
                helper.Events.Display.RenderedActiveMenu += OnRenderedActiveMenu;
                harmony.Patch(click, prefix: new HarmonyMethod(typeof(CustomRecipePageBetterCraftingPatch), nameof(ClickPrefix)));
            }
            catch (Exception ex)
            {
                monitor?.Log($"CustomRecipePages: Better Crafting compatibility patch failed: {ex}", LogLevel.Warn);
            }
        }

        private static bool IsEditing(object page) => editing?.GetValue(page) is true;
        private static bool IsSearching(object page) => filter?.GetValue(page) is string text && !string.IsNullOrWhiteSpace(text);

        private static HashSet<string> GetIds(bool cooking)
        {
            Dictionary<string, string> known = Game1.content.Load<Dictionary<string, string>>(
                cooking ? "Data/CookingRecipes" : "Data/CraftingRecipes");
            return CustomRecipePageDataService.GetEntries(CustomRecipePageDataService.LoadGroups(), known.Keys, cooking, monitor)
                .Select(e => e.Id).ToHashSet(StringComparer.Ordinal);
        }

        /// <summary>
        /// LayoutRecipesのローカル変数13（ToListで生成された配置用一覧）だけを加工します。
        /// 検索中・カテゴリ編集時はBetter Crafting本来の一覧を維持します。
        /// </summary>
        private static List<T> FilterRecipes<T>(List<T> recipes, object page)
        {
            if (IsEditing(page) || IsSearching(page))
                return recipes;
            PageState state = States.GetValue(page, _ => new PageState());
            try
            {
                bool cooking = AccessTools.Field(page.GetType(), "cooking")?.GetValue(page) is true;
                var ids = GetIds(cooking);
                state.HasRecipes = ids.Count > 0;
                if (!state.HasRecipes)
                    return recipes;
                PropertyInfo? name = AccessTools.Property(typeof(T), "Name");
                if (name == null)
                    return recipes;
                recipes.RemoveAll(recipe => ids.Contains(name.GetValue(recipe) as string ?? "") != state.Dedicated);
            }
            catch (Exception ex)
            {
                monitor?.Log($"CustomRecipePages: Better Crafting recipe filtering failed: {ex}", LogLevel.Warn);
            }
            return recipes;
        }

        private static IEnumerable<CodeInstruction> LayoutTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            // FilteredRecipes.ToList()の直後のstloc.s V_13を対象にします。
            int index = codes.FindLastIndex(i => i.opcode == OpCodes.Stloc_S &&
                (i.operand is LocalBuilder l && l.LocalIndex == 13 || i.operand is byte b && b == 13));
            if (index < 0 || index < 2 || codes[index - 1].operand is not MethodInfo toList ||
                toList.Name != "ToList" || !toList.IsGenericMethod)
            {
                monitor?.Log("CustomRecipePages: Better Crafting layout IL anchor was not found.", LogLevel.Warn);
                return codes;
            }
            MethodInfo callback = AccessTools.Method(typeof(CustomRecipePageBetterCraftingPatch), nameof(FilterRecipes))!
                .MakeGenericMethod(toList.GetGenericArguments());
            // スタック: List<T> -> List<T>, pageを追加してから戻します。
            codes.Insert(index, new CodeInstruction(OpCodes.Ldarg_0));
            codes.Insert(index + 1, new CodeInstruction(OpCodes.Call, callback));
            return codes;
        }

        private static Rectangle Bounds(IClickableMenu page) =>
            new(page.xPositionOnScreen - 128, page.yPositionOnScreen + 80, 64, 64);

        private static bool ClickPrefix(object __instance, int x, int y)
        {
            if (__instance is not IClickableMenu page || IsEditing(__instance) || IsSearching(__instance))
                return true;
            PageState state = States.GetValue(__instance, _ => new PageState());
            if (!state.HasRecipes || !Bounds(page).Contains(x, y))
                return true;
            state.Dedicated = !state.Dedicated;
            try
            {
                layout?.Invoke(__instance, null);
                Game1.playSound("smallSelect");
            }
            catch (Exception ex)
            {
                state.Dedicated = !state.Dedicated;
                monitor?.Log($"CustomRecipePages: Better Crafting page switch failed: {ex}", LogLevel.Warn);
            }
            return false;
        }

        private static void OnRenderedActiveMenu(object? sender, RenderedActiveMenuEventArgs e)
        {
            if (Game1.activeClickableMenu is IClickableMenu page &&
                page.GetType().FullName == "Leclair.Stardew.BetterCrafting.Menus.BetterCraftingPage")
                DrawPostfix(page, e.SpriteBatch);
        }

        private static void DrawPostfix(object __instance, SpriteBatch b)
        {
            if (__instance is not IClickableMenu page || IsEditing(__instance) || IsSearching(__instance))
                return;
            PageState state = States.GetValue(__instance, _ => new PageState());
            if (!state.HasRecipes)
                return;
            Rectangle bounds = Bounds(page);
            IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60),
                bounds.X, bounds.Y, bounds.Width, bounds.Height, Color.White, 1f, drawShadow: false);
            string label = state.Dedicated ? "T" : "V";
            Vector2 size = Game1.smallFont.MeasureString(label);
            b.DrawString(Game1.smallFont, label,
                new Vector2(bounds.Center.X - size.X / 2, bounds.Center.Y - size.Y / 2), Game1.textColor);
            if (bounds.Contains(Game1.getMouseX(), Game1.getMouseY()))
            {
                string hover = state.Dedicated
                    ? translations?.Get("customRecipePages.showVanilla") ?? "Show Vanilla Menu"
                    : translations?.Get("customRecipePages.showTsCore") ?? "Show T's Core Menu";
                IClickableMenu.drawHoverText(b, hover, Game1.smallFont);
            }

            // Game1.drawMouseCursor()ではなく、メニューと同じSpriteBatchに
            // Stardew Valley標準のカーソルを描画します。
            page.drawMouse(b);
        }
    }
}
