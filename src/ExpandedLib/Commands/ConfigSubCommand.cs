using System.Linq;
using ExpandedLib.Registries.Commands;
using ExpandedLib.Registries.Config;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace ExpandedLib.Commands;

/// <summary>
/// Adds <c>/exmod config [&lt;mod&gt; [&lt;value&gt; [&lt;new&gt;]]]</c>: the generic, mod-agnostic switch for
/// the gameplay tunables any mod exposes through <see cref="ExConfigProfiles"/> (a config marked
/// <c>[ExConfigRegister(..., Manageable = true)]</c>). With no argument it lists the registered configs;
/// with a mod code it lists that mod's values and their current settings; with a value name it prints
/// that value; with a new value it parses, validates, sets and persists it - applied immediately,
/// since everything reads the value through the live accessor (no world reload). Server-side, since the
/// config is host-authoritative (the <c>/exmod</c> root requires <c>controlserver</c>).
/// </summary>
[SubCommandRegister(Side = EnumAppSide.Server)]
public sealed class ConfigSubCommand : IExSubCommand
{
  public string ParentName => "exmod";

  public void Register(ICoreAPI api, Mod mod, IChatCommand parent)
  {
    var parsers = api.ChatCommands.Parsers;

    parent
      .BeginSubCommand("config")
      .WithDescription(Lang.Get("exlib:command-config-desc"))
      .WithArgs(
        parsers.OptionalWord("mod"),
        parsers.OptionalWord("value"),
        parsers.OptionalWord("newvalue")
      )
      .HandleWith(OnCommand)
      .EndSubCommand();
  }

  private static TextCommandResult OnCommand(TextCommandCallingArgs args)
  {
    string? code = args[0] as string;
    string? name = args[1] as string;
    string? raw = args[2] as string;

    if (code == null)
      return TextCommandResult.Success(ListConfigs());

    if (!ExConfigProfiles.TryGet(code, out var config))
      return TextCommandResult.Error(
        Lang.Get("exlib:command-config-unknown", code, KnownCodes())
      );

    if (name == null)
      return TextCommandResult.Success(ListValues(config));

    // Read: print the current value.
    if (raw == null)
    {
      if (!config.TryGet(name, out var canonical, out var value))
        return TextCommandResult.Error(
          Lang.Get("exlib:command-config-novalue", name, config.ModId)
        );
      return TextCommandResult.Success(
        Lang.Get("exlib:command-config-current", canonical, value)
      );
    }

    // Write: parse, validate, set and persist (applied immediately).
    var result = config.Set(name, raw);
    return result.Status switch
    {
      ExConfigEditStatus.Ok => TextCommandResult.Success(
        Lang.Get(
          "exlib:command-config-set",
          result.Name,
          result.OldValue,
          result.NewValue
        )
      ),
      ExConfigEditStatus.UnknownValue => TextCommandResult.Error(
        Lang.Get("exlib:command-config-novalue", name, config.ModId)
      ),
      ExConfigEditStatus.ParseFailed => TextCommandResult.Error(
        Lang.Get(
          "exlib:command-config-parsefail",
          raw,
          result.Name,
          result.Expected
        )
      ),
      ExConfigEditStatus.OutOfRange => TextCommandResult.Error(
        Lang.Get("exlib:command-config-range", result.Name)
      ),
      _ => TextCommandResult.Error(Lang.Get("exlib:command-config-novalue", name, config.ModId)),
    };
  }

  private static string ListConfigs()
  {
    if (ExConfigProfiles.Codes.Count == 0)
      return Lang.Get("exlib:command-config-none");

    var lines = ExConfigProfiles
      .Codes.OrderBy(c => c)
      .Select(c =>
        ExConfigProfiles.TryGet(c, out var cfg)
          ? $"  {cfg.ModId} ({cfg.FileName})"
          : c
      );
    return Lang.Get("exlib:command-config-list")
      + "\n"
      + string.Join("\n", lines);
  }

  private static string ListValues(IExConfigAccess config)
  {
    var lines = config.ValueNames.Select(n =>
      config.TryGet(n, out var canonical, out var value)
        ? $"  {canonical} = {value}"
        : n
    );
    return Lang.Get("exlib:command-config-values", config.ModId)
      + "\n"
      + string.Join("\n", lines);
  }

  private static string KnownCodes() =>
    string.Join(", ", ExConfigProfiles.Codes.OrderBy(c => c));
}
