using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ExpandedLib.Testing;

/// <summary>
/// Resolves the Vintage Story game assemblies (VintagestoryAPI, VSSurvivalMod, the bundled
/// libraries, …) at runtime from the matching game install. The test/harness projects reference
/// those DLLs with <c>Private=false</c> (they are never shipped), so without this probe a headless
/// <c>dotnet test</c> run cannot load them. Registered automatically via a module initializer in
/// every assembly that includes this type's <see cref="Register"/> call.
///
/// The install is chosen by the game version this assembly was compiled for, so legacy test runs
/// load the right version's DLLs. Rather than a per-TFM <c>#if</c> ladder, the build stamps the
/// matching env-var name (<c>VINTAGE_STORY</c> / <c>VINTAGE_STORY_121</c> / …) into the assembly as
/// <c>[AssemblyMetadata("GameInstallEnv")]</c> from the version manifest in
/// <c>src/Directory.Build.props</c>, so adding a game version needs no change here. The path comes
/// from that environment variable, or - mirroring the props - the gitignored repo-root <c>.env</c>
/// file when the variable isn't set in the environment (the legacy vars usually live only there).
/// </summary>
public static class VsAssemblyResolver
{
  private static readonly object Gate = new();
  private static bool _registered;

  /// <summary>Name of the environment variable pointing at the matching install, stamped by the
  /// build from the version manifest. Falls back to the primary install var if absent.</summary>
  private static readonly string InstallKey =
    typeof(VsAssemblyResolver)
      .Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
      .FirstOrDefault(a => a.Key == "GameInstallEnv")
      ?.Value
    ?? "VINTAGE_STORY";

  /// <summary>Idempotently hooks <see cref="AppDomain.AssemblyResolve"/> to probe the game folders.</summary>
  public static void Register()
  {
    lock (Gate)
    {
      if (_registered)
        return;
      _registered = true;
    }

    string? vs = ResolveInstallPath();
    if (string.IsNullOrEmpty(vs))
      return;

    string[] dirs = [vs, Path.Combine(vs, "Lib"), Path.Combine(vs, "Mods")];

    AppDomain.CurrentDomain.AssemblyResolve += (_, args) =>
    {
      string? name = new AssemblyName(args.Name).Name;
      if (name == null)
        return null;
      foreach (string dir in dirs)
      {
        string path = Path.Combine(dir, name + ".dll");
        if (File.Exists(path))
          return Assembly.LoadFrom(path);
      }
      return null;
    };
  }

  /// <summary>The install path for this TFM: the <see cref="InstallKey"/> environment variable if set,
  /// otherwise its value from the repo-root <c>.env</c> file (found by walking up from the test
  /// output directory). Null when neither yields a value.</summary>
  private static string? ResolveInstallPath()
  {
    string? fromEnv = Environment.GetEnvironmentVariable(InstallKey);
    if (!string.IsNullOrEmpty(fromEnv))
      return fromEnv;
    return ReadFromDotEnv(InstallKey);
  }

  private static string? ReadFromDotEnv(string key)
  {
    for (
      DirectoryInfo? dir = new(AppContext.BaseDirectory);
      dir != null;
      dir = dir.Parent
    )
    {
      string envPath = Path.Combine(dir.FullName, ".env");
      if (!File.Exists(envPath))
        continue;
      foreach (string line in File.ReadAllLines(envPath))
      {
        int eq = line.IndexOf('=');
        if (eq <= 0)
          continue;
        if (line[..eq].Trim() == key)
          return line[(eq + 1)..].Trim();
      }
      return null; // .env found but no such key
    }
    return null;
  }
}

internal static class HarnessModuleInitializer
{
  // Deliberately used in a class library: the resolver must be live before any harness type that
  // references the game assemblies is touched. Safe here - it only hooks AssemblyResolve.
#pragma warning disable CA2255
  [ModuleInitializer]
#pragma warning restore CA2255
  internal static void Init() => VsAssemblyResolver.Register();
}
