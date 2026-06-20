using System.IO;
using System.Linq;
using ExpandedLib.Registries.Config;
using NSubstitute;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Xunit;

namespace ExpandedLib.Tests;

/// <summary>A config POCO spanning every editable value type plus a non-editable complex one, so the
/// store's value-listing and parse/format paths are all exercised.</summary>
internal sealed class EditableConfig : IExVersionedConfig
{
  public string? ConfigVersion { get; set; }
  public int Count { get; set; } = 10;
  public long Big { get; set; } = 1000;
  public float Rate { get; set; } = 1.5f;
  public double Precise { get; set; } = 2.25d;
  public bool Enabled { get; set; } = true;
  public string Label { get; set; } = "normal";

  /// <summary>Complex type: must be excluded from the editable value set.</summary>
  public int[] NotEditable { get; set; } = [1, 2, 3];

  /// <summary>Read-only: must be excluded too.</summary>
  public int Derived => Count * 2;
}

/// <summary>
/// The runtime config-editing path the generic <c>/exmod config</c> command drives through
/// <see cref="IExConfigAccess"/>: which values are exposed, reading and formatting them, and parsing /
/// validating / setting new ones - plus the legacy-file rename that carries a player's tuning over a
/// config rename.
/// </summary>
public class ConfigEditTests
{
  private static ExConfigRegister<EditableConfig> Store() =>
    new("editable.json", "fakemod");

  #region Value listing
  [Fact]
  public void ValueNames_lists_only_simple_read_write_values()
  {
    IExConfigAccess store = Store();

    Assert.Equal(
      ["Count", "Big", "Rate", "Precise", "Enabled", "Label"],
      store.ValueNames.ToArray()
    );
  }
  #endregion

  #region Reading values
  [Fact]
  public void TryGet_is_case_insensitive_and_returns_canonical_name()
  {
    IExConfigAccess store = Store();

    bool found = store.TryGet("rate", out string name, out string value);

    Assert.True(found);
    Assert.Equal("Rate", name); // canonical casing from the config
    Assert.Equal("1.5", value); // invariant formatting
  }

  [Fact]
  public void TryGet_formats_bool_as_lowercase_word()
  {
    IExConfigAccess store = Store();

    store.TryGet("Enabled", out _, out string value);

    Assert.Equal("true", value);
  }

  [Fact]
  public void TryGet_returns_false_for_unknown_or_non_editable_value()
  {
    IExConfigAccess store = Store();

    Assert.False(store.TryGet("nope", out _, out _));
    Assert.False(store.TryGet("NotEditable", out _, out _)); // complex type
    Assert.False(store.TryGet("Derived", out _, out _)); // read-only
    Assert.False(store.TryGet("ConfigVersion", out _, out _)); // version stamp
  }
  #endregion

  #region Setting values
  [Fact]
  public void Set_parses_and_applies_each_value_type()
  {
    var store = Store();

    Assert.Equal(ExConfigEditStatus.Ok, store.Set("count", "42").Status);
    Assert.Equal(ExConfigEditStatus.Ok, store.Set("rate", "3.75").Status);
    Assert.Equal(ExConfigEditStatus.Ok, store.Set("enabled", "off").Status);
    Assert.Equal(ExConfigEditStatus.Ok, store.Set("label", "cheap").Status);

    Assert.Equal(42, store.Config.Count);
    Assert.Equal(3.75f, store.Config.Rate, 3);
    Assert.False(store.Config.Enabled);
    Assert.Equal("cheap", store.Config.Label);
  }

  [Fact]
  public void Set_reports_old_and_new_value_and_canonical_name()
  {
    var store = Store();

    var result = store.Set("RATE", "9");

    Assert.Equal(ExConfigEditStatus.Ok, result.Status);
    Assert.Equal("Rate", result.Name);
    Assert.Equal("1.5", result.OldValue);
    Assert.Equal("9", result.NewValue);
  }

  [Theory]
  [InlineData("on", true)]
  [InlineData("1", true)]
  [InlineData("yes", true)]
  [InlineData("false", false)]
  [InlineData("0", false)]
  [InlineData("no", false)]
  public void Set_accepts_lenient_boolean_words(string raw, bool expected)
  {
    var store = Store();

    Assert.Equal(ExConfigEditStatus.Ok, store.Set("enabled", raw).Status);
    Assert.Equal(expected, store.Config.Enabled);
  }

  [Fact]
  public void Set_rejects_unparseable_input_without_changing_the_value()
  {
    var store = Store();

    var result = store.Set("count", "lots");

    Assert.Equal(ExConfigEditStatus.ParseFailed, result.Status);
    Assert.Equal(10, store.Config.Count); // unchanged
  }

  [Fact]
  public void Set_rejects_negative_number_as_out_of_range()
  {
    var store = Store();

    var result = store.Set("rate", "-1");

    Assert.Equal(ExConfigEditStatus.OutOfRange, result.Status);
    Assert.Equal(1.5f, store.Config.Rate, 3); // unchanged
  }

  [Fact]
  public void Set_returns_unknown_for_a_missing_value()
  {
    var store = Store();

    Assert.Equal(
      ExConfigEditStatus.UnknownValue,
      store.Set("nope", "1").Status
    );
  }
  #endregion

  #region Legacy file rename
  [Fact]
  public void Load_renames_a_present_legacy_file_to_the_current_name()
  {
    using var dir = new TempModConfig();
    File.WriteAllText(dir.Path("old.json"), "{}");

    var store = new ExConfigRegister<EditableConfig>("new.json", "fakemod")
    {
      LegacyFileNames = ["old.json"],
    };
    store.Load(FakeApi());

    Assert.True(File.Exists(dir.Path("new.json")));
    Assert.False(File.Exists(dir.Path("old.json")));
  }

  [Fact]
  public void Load_leaves_legacy_file_alone_when_current_file_exists()
  {
    using var dir = new TempModConfig();
    File.WriteAllText(dir.Path("old.json"), "{}");
    File.WriteAllText(dir.Path("new.json"), "{}");

    var store = new ExConfigRegister<EditableConfig>("new.json", "fakemod")
    {
      LegacyFileNames = ["old.json"],
    };
    store.Load(FakeApi());

    Assert.True(File.Exists(dir.Path("old.json"))); // untouched
  }

  [Fact]
  public void Load_with_no_legacy_names_is_a_noop()
  {
    using var dir = new TempModConfig();

    var store = new ExConfigRegister<EditableConfig>("new.json", "fakemod");
    store.Load(FakeApi()); // must not throw or create anything

    Assert.False(File.Exists(dir.Path("new.json")));
  }
  #endregion

  /// <summary>A fake API whose <c>LoadModConfig</c> returns null (so Load falls back to defaults) and
  /// whose mod version resolves; the legacy rename runs against the real filesystem via GamePaths.</summary>
  private static ICoreAPI FakeApi()
  {
    var api = Substitute.For<ICoreAPI>();
    api.Logger.Returns(Substitute.For<ILogger>());
    api.LoadModConfig<EditableConfig>(Arg.Any<string>())
      .Returns((EditableConfig?)null);

    var mod = Substitute.For<Mod>();
    typeof(Mod)
      .GetProperty("Info")!
      .SetValue(mod, new ModInfo { Version = "1.0.0" });
    var modLoader = Substitute.For<IModLoader>();
    modLoader.GetMod("fakemod").Returns(mod);
    api.ModLoader.Returns(modLoader);

    return api;
  }

  /// <summary>Points <see cref="GamePaths.ModConfig"/> at a throwaway temp folder for a test and
  /// removes it afterwards, so the rename runs against a real - but disposable - directory.</summary>
  private sealed class TempModConfig : System.IDisposable
  {
    private readonly string _root = System.IO.Path.Combine(
      System.IO.Path.GetTempPath(),
      "exlib_cfgtest_" + System.Guid.NewGuid().ToString("N")
    );

    public TempModConfig()
    {
      GamePaths.DataPath = _root;
      Directory.CreateDirectory(GamePaths.ModConfig);
    }

    public string Path(string file) =>
      System.IO.Path.Combine(GamePaths.ModConfig, file);

    public void Dispose()
    {
      try
      {
        Directory.Delete(_root, recursive: true);
      }
      catch
      { /* best-effort cleanup */
      }
    }
  }
}
