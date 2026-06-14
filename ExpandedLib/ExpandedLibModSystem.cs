using ExpandedLib.Blocks.Structures;
using ExpandedLib.Registries.Commands;
using ExpandedLib.Registries.Entities;
using ExpandedLib.Registries.Preferences;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace ExpandedLib;

/// <summary>
/// Entry point for the shared Expanded Lib mod (<c>exlib</c>). Registers the library's own
/// blocks / block entities / behaviours (the invisible structure filler and the multiblock
/// structure behaviour) and points the <see cref="StructureFillers"/> helper at this mod's
/// filler block, so every dependent mod's mega-blocks reuse a single shared filler.
/// <para>
/// On the client it also owns the shared per-player display preferences
/// (<see cref="Registries.Preferences.ExPreferences"/>, e.g. the metric/imperial unit system): it loads the
/// per-player <c>exmod.json</c>, discovers the <c>[PreferenceRegister]</c> definitions, registers
/// the <c>/exmod</c> command and patches the handbook to convert measurement units.
/// </para>
/// <para>
/// The block-network graph manager and the world block-code migrator are separate
/// <c>ModSystem</c>s in this assembly (<see cref="Blocks.Networks.BlockNetworkModSystem"/>,
/// <see cref="Blocks.Migrations.BlockMigrationModSystem"/>); the game auto-loads them too.
/// </para>
/// </summary>
public class ExpandedLibModSystem : ModSystem
{
  private Harmony? _harmony;

  public override void Start(ICoreAPI api)
  {
        // Auto-register the library's [EntityRegister] classes (filler block + entity, the
        // MultiblockStructure behaviour) under the exlib domain.
        EntityRegistry.RegisterAll(api, Mod, GetType().Assembly);

    // The shared filler block this lib ships; dependent mods' mega-blocks reserve their
    // footprint cells with it (see StructureFillers).
    StructureFillers.FillerCode = new AssetLocation(
      Mod.Info.ModID,
      "structurefiller"
    );
  }

  public override void StartClientSide(ICoreClientAPI api)
  {
    // Per-player, client-side display preferences (the HUD/handbook render here). Load the saved
    // choices, then discover the [PreferenceRegister] definitions (e.g. the metric/imperial unit
    // system) so the /exmod command and ApplyForPlayer below can see them.
    ExPreferences.LoadConfig(api);
    PreferenceRegistry.RegisterAll(api, Mod, GetType().Assembly);

    // Apply the local player's saved choices once the world (and player) are ready.
    api.Event.LevelFinalize += () =>
      ExPreferences.ApplyForPlayer(api.World.Player.PlayerUID);

    // Auto-register the library's [CommandRegister] client commands (the /exmod preferences
    // command builds a sub-command per registered preference) under the exlib domain.
    CommandRegistry.RegisterAll(api, Mod, GetType().Assembly);

    // Patch the handbook so metric unit mentions in page text follow the chosen system. A failure
    // here (e.g. the survival handbook page shape changed) must not break the rest of the lib.
    try
    {
      if (!Harmony.HasAnyPatches(Mod.Info.ModID))
      {
        _harmony = new Harmony(Mod.Info.ModID);
        _harmony.PatchAll(GetType().Assembly);
      }
    }
    catch (System.Exception e)
    {
      api.Logger.Warning(
        "[exlib] Could not patch the handbook for unit display; it will stay metric. {0}",
        e
      );
    }
  }

  public override void Dispose()
  {
    _harmony?.UnpatchAll(Mod.Info.ModID);
    _harmony = null;
    base.Dispose();
  }
}
