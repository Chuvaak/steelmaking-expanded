using System.Collections.Generic;
using ExpandedLib.Registries.Config;
using ExpandedLib.Registries.Recipes;

namespace PipesAndPowerExpanded;

/// <summary>
/// The steam-machine recipe cost catalogue, written to <c>ModConfig/ppex_recipes.json</c> alongside
/// the main <c>ppex_values.json</c>. Each entry names a grid recipe (by output) or RCC construction (by
/// block) to manage and its ingredient totals per cost level. The <c>normal</c> level is auto-filled
/// from the recipes as authored on first run, and any level a recipe doesn't pin is filled by scaling
/// <c>normal</c> (so the whole "cheap" tier appears as explicit, editable numbers). Players/admins can
/// then set any specific number. The active level is chosen by <c>/exmod steam &lt;level&gt;</c>
/// (stored in <see cref="PpexConfig.RecipeLevel"/>) and applied on the next world reload.
/// </summary>
[ExConfigRegister("ppex_recipes.json", "ppex")]
public class PpexRecipeConfig : IExVersionedConfig
{
  public string? ConfigVersion { get; set; }

  private Dictionary<string, RecipeCostEntry>? _recipes;

  /// <summary>Never null: a missing or null <c>Recipes</c> in the file falls back to the code
  /// defaults (a wrong edit can't blank the catalogue). Missing/broken individual entries are
  /// repaired against <see cref="DefaultCatalogue"/> at load by <see cref="ExRecipeCosts.Reconcile"/>.</summary>
  public Dictionary<string, RecipeCostEntry> Recipes
  {
    get => _recipes ??= Defaults();
    set => _recipes = value;
  }

  /// <summary>A fresh copy of the shipped catalogue defaults, used to repair the loaded file.</summary>
  public static Dictionary<string, RecipeCostEntry> DefaultCatalogue() =>
    Defaults();

  private static RecipeCostEntry Rcc(string match) =>
    new() { Kind = "rcc", Match = match };

  private static RecipeCostEntry Grid(string match) =>
    new() { Kind = "grid", Match = match };

  private static Dictionary<string, RecipeCostEntry> Defaults() =>
    new()
    {
      // RCC constructions (the heavy multiblock build costs).
      ["boilercornish-rcc"] = Rcc("ppex:boilercornish-*"),
      ["boilerlancashire-rcc"] = Rcc("ppex:boilerlancashire-*"),
      ["enginecornish-rcc"] = Rcc("ppex:enginecornish-*"),
      // The Watt engine pins exact "cheap" totals (the worked example); everything else scales.
      ["enginewatt-rcc"] = new()
      {
        Kind = "rcc",
        Match = "ppex:enginewatt-*",
        Levels = new()
        {
          ["cheap"] = new()
          {
            ["ppex:rcc-ingredient-metalplate"] = 2,
            ["ppex:rcc-ingredient-rod"] = 8,
            ["ppex:rcc-ingredient-nailsandstrips"] = 4,
            ["game:burnedbrick-fire"] = 12,
          },
        },
      },

      // Grid "frame" recipes for the machines.
      ["boilercornish-grid"] = Grid("ppex:boilercornish-*"),
      ["boilerlancashire-grid"] = Grid("ppex:boilerlancashire-*"),
      ["enginecornish-grid"] = Grid("ppex:enginecornish-*"),
      ["enginewatt-grid"] = Grid("ppex:enginewatt-*"),
      ["enginefluidpump-grid"] = Grid("ppex:enginefluidpump-*"),
      ["enginempgenerator-grid"] = Grid("ppex:enginempgenerator-*"),
      ["manualfluidpump-grid"] = Grid("ppex:manualfluidpump-*"),
      ["steamcondenser-grid"] = Grid("ppex:steamcondenser-*"),
    };
}
