using System;

namespace SteelmakingExpanded;

/// <summary>
/// Hand-written companion to the source-generated <see cref="SmexValues"/> accessor. The generator
/// emits only read-only getters; this adds the single write path needed by runtime admin commands
/// (e.g. <c>/exmod molds</c>) to mutate a tunable and persist it. Most code should keep reading the
/// generated getters - this is for the rare server-side toggle.
/// </summary>
public static partial class SmexValues
{
  /// <summary>Mutates the live config through <paramref name="mutate"/> and writes it back to
  /// <c>smex.json</c>. Server-side only in practice (config changes are host-authoritative).</summary>
  public static void Edit(Action<SmexConfig> mutate)
  {
    mutate(_store.Config);
    _store.Save();
  }
}
