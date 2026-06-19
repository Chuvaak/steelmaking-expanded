using System.Collections.Generic;

namespace ExpandedLib.Registries.Recipes;

/// <summary>
/// One managed recipe in a mod's cost catalogue: its kind (how the framework finds and edits it) and
/// the ingredient totals it should cost at each named level. The "normal" level is auto-filled from
/// the live recipe on first run (see <see cref="ExRecipeCosts.EnsureNormalExtracted"/>); a mod ships
/// the alternate level(s) - e.g. "cheap" - as defaults, and players can edit any number afterwards.
/// </summary>
public class RecipeCostEntry
{
  /// <summary>How to locate and edit the recipe: <c>"grid"</c> (a crafting recipe, matched by output
  /// code) or <c>"rcc"</c> (a right-click-construction block, matched by block code).</summary>
  public string Kind { get; set; } = "grid";

  /// <summary>Wildcard code matched against the grid output / RCC block code (e.g.
  /// <c>"ppex:enginewatt-*"</c>). Kept separate from the catalogue key so the same block can have
  /// both a grid entry and an rcc entry under distinct keys.</summary>
  public string Match { get; set; } = "";

  /// <summary>level name → (ingredient key → total quantity). Ingredient keys are the ingredient's
  /// code for grid recipes, and the construction ingredient's <c>name</c> (falling back to its code)
  /// for RCC. An RCC total is distributed back across the stages that use that ingredient.</summary>
  public Dictionary<string, Dictionary<string, int>> Levels { get; set; } =
    new();
}
