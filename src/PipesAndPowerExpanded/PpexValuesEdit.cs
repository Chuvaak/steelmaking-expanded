using System;

namespace PipesAndPowerExpanded;

/// <summary>
/// Hand-written companions to the source-generated config accessors, adding the write paths runtime
/// admin commands need (the generator emits read-only getters). <see cref="PpexValues.Edit"/> mutates
/// and persists the main config; <see cref="PpexRecipeValues.Save"/> persists the recipe catalogue
/// after its levels are auto-filled at load.
/// </summary>
public static partial class PpexValues
{
  /// <summary>Mutates the live config through <paramref name="mutate"/> and writes it back to
  /// <c>ppex_values.json</c>. Server-side only in practice (config changes are host-authoritative).</summary>
  public static void Edit(Action<PpexConfig> mutate)
  {
    mutate(_store.Config);
    _store.Save();
  }
}

public static partial class PpexRecipeValues
{
  /// <summary>Persists the recipe catalogue to <c>ppex_recipes.json</c> (after the framework fills in
  /// the auto-derived levels).</summary>
  public static void Save() => _store.Save();
}
