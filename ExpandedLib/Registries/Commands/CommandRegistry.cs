using System;
using System.Reflection;
using Vintagestory.API.Common;

namespace ExpandedLib.Registries.Commands;

/// <summary>
/// Reflection-driven chat-command registration for mods built on ExpandedLib, the command-side
/// counterpart to <see cref="Entities.EntityRegistry"/>. Scans an assembly for
/// <see cref="IExCommand"/> classes carrying <see cref="CommandRegisterAttribute"/> and builds
/// each one, so a mod system never hand-wires <c>api.ChatCommands.Create(...)</c> calls.
/// </summary>
public static class CommandRegistry
{
  /// <summary>
  /// Registers every <see cref="CommandRegisterAttribute"/>-decorated <see cref="IExCommand"/>
  /// in <paramref name="asm"/> (default: the calling mod's own assembly) whose declared side
  /// matches <paramref name="api"/>. Call from <c>ModSystem.Start</c> for universal/server
  /// commands and/or <c>StartClientSide</c> for client commands - each command's
  /// <see cref="CommandRegisterAttribute.Side"/> ensures it only registers once.
  /// </summary>
  public static void RegisterAll(ICoreAPI api, Mod mod, Assembly? asm = null)
  {
    asm ??= Assembly.GetCallingAssembly();
    string modId = mod.Info.ModID;

    foreach (Type type in ReflectionScan.GetCandidateTypes(asm))
    {
      var attr = type.GetCustomAttribute<CommandRegisterAttribute>();
      if (attr == null)
        continue;

      // Universal commands register on whichever side runs; sided ones only on their own side.
      if (attr.Side != EnumAppSide.Universal && attr.Side != api.Side)
        continue;

      if (!typeof(IExCommand).IsAssignableFrom(type))
      {
        api.Logger.Warning(
          "[{0}] CommandRegistry: {1} has [CommandRegister] but does not implement IExCommand; skipped.",
          modId,
          type.FullName
        );
        continue;
      }

      var command = (IExCommand)Activator.CreateInstance(type)!;
      command.Register(api, mod);
    }
  }
}
