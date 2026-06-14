using System.Linq;
using ExpandedLib.Registries.Commands;
using ExpandedLib.Registries.Preferences;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace ExpandedLib.Commands;

/// <summary>
/// Client-side <c>/exmod</c> command, shared by every Expanded mod. Exposes one sub-command per
/// registered <see cref="IExPreference"/> (e.g. <c>/exmod measure [metric|imperial]</c>): the
/// settings are per-player client preferences (the HUD/handbook render on the client), persisted
/// through <see cref="ExPreferences"/>. With no argument a sub-command reports the current value.
/// <para>
/// The sub-commands are built from whatever preferences are registered, so a new preference needs
/// no change here - just an <see cref="IExPreference"/> class and its lang keys
/// (<c>command-{key}-desc</c>, <c>pref-{key}-label</c>, <c>pref-{key}-{value}</c>).
/// </para>
/// </summary>
[CommandRegister(Side = EnumAppSide.Client)]
public sealed class ExmodCommand : IExCommand
{
  public void Register(ICoreAPI api, Mod mod)
  {
    var capi = (ICoreClientAPI)api;
    var cmd = capi
      .ChatCommands.Create("exmod")
      .WithDescription(Lang.Get("exlib:command-exmod-desc"));

    foreach (var pref in ExPreferences.All)
    {
      cmd.BeginSubCommand(pref.Key)
        .WithDescription(Lang.Get("exlib:command-" + pref.Key + "-desc"))
        .WithArgs(capi.ChatCommands.Parsers.OptionalWord("value"))
        .HandleWith(args => OnPreferenceCommand(capi, pref, args))
        .EndSubCommand();
    }
  }

  private static TextCommandResult OnPreferenceCommand(
    ICoreClientAPI api,
    IExPreference pref,
    TextCommandCallingArgs args
  )
  {
    string uid = api.World.Player.PlayerUID;
    string? word = (args[0] as string)?.ToLowerInvariant();
    string label = Lang.Get("exlib:pref-" + pref.Key + "-label");

    // No argument: report the current setting.
    if (string.IsNullOrEmpty(word))
      return TextCommandResult.Success(
        Lang.Get(
          "exlib:command-pref-current",
          label,
          ValueLabel(pref, ExPreferences.GetForPlayer(uid, pref.Key))
        )
      );

    if (!pref.Options.Contains(word))
      return TextCommandResult.Error(
        Lang.Get(
          "exlib:command-pref-invalid",
          word,
          label,
          string.Join(", ", pref.Options)
        )
      );

    ExPreferences.SetForPlayer(uid, pref.Key, word);
    return TextCommandResult.Success(
      Lang.Get("exlib:command-pref-set", label, ValueLabel(pref, word))
    );
  }

  /// <summary>The human-readable display name for a preference value (lang
  /// <c>pref-{key}-{value}</c>, e.g. <c>pref-measure-metric</c>).</summary>
  private static string ValueLabel(IExPreference pref, string value) =>
    Lang.Get("exlib:pref-" + pref.Key + "-" + value);
}
