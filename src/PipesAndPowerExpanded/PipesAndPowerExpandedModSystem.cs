using ExpandedLib.Blocks.Networks;
using ExpandedLib.Helpers;
using ExpandedLib.Registries.Commands;
using ExpandedLib.Registries.Entities;
using ExpandedLib.Registries.Preferences;
using ExpandedLib.Registries.Recipes;
using HarmonyLib;
using PipesAndPowerExpanded.BlockNetworkPipe;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace PipesAndPowerExpanded;

/// <summary>
/// Main mod system for Pipes and Power Expanded. Loads the gameplay tunables, patches the vanilla
/// chimney look-at info, auto-registers every <c>[BlockRegister]</c>/<c>[ItemRegister]</c>/etc.
/// decorated class, adds the creative tab, and registers the unified "pipe" network type (gases + liquids).
/// </summary>
public class PipesAndPowerExpandedModSystem : ModSystem
{
  private Harmony? _harmony;

  public override void Start(ICoreAPI api)
  {
    // Load gameplay tunables from ModConfig/ppex_values.json (writes defaults on first run).
    PpexValues.Load(api);
    // The steam-machine recipe cost catalogue (ppex_recipes.json).
    PpexRecipeValues.Load(api);

    // Patch the vanilla chimney's look-at info so a chimney venting one of our pipes
    // reports it (the gas draw itself runs in PipeNetwork's tick).
    if (!Harmony.HasAnyPatches(Mod.Info.ModID))
    {
      _harmony = new Harmony(Mod.Info.ModID);
      _harmony.PatchAll(GetType().Assembly);
    }

    // The shared structure-filler block and network/structure framework live in the exlib
    // mod (a hard dependency); exlib points StructureFillers at exlib:structurefiller and
    // registers its own classes. Here we only register ppex's own content.
    EntityRegistry.RegisterAll(api, Mod, GetType().Assembly);

    // The unified pipe network (gas + liquid pools).
    var netManager = api.ModLoader.GetModSystem<BlockNetworkModSystem>();
    netManager.RegisterNetworkType("pipe", () => new PipeNetwork(netManager));
  }

  public override void Dispose()
  {
    _harmony?.UnpatchAll(Mod.Info.ModID);
    _harmony = null;
    base.Dispose();
  }

  public override void StartServerSide(ICoreServerAPI api)
  {
    // Server-side sub-commands (e.g. /exmod steam) + apply the active recipe cost level.
    CommandRegistry.RegisterAll(api, Mod, GetType().Assembly);
    ApplyRecipeLevel(api);
  }

  #region Creative category
  public override void StartClientSide(ICoreClientAPI api)
  {
    ExCreativeTabs.EnsureTab(Mod.Info.ModID);

    // Register ppex's display preferences (the metric/imperial unit system) into the library's
    // shared store, then build their .exmod sub-commands. exlib loads/persists/applies the values.
    PreferenceRegistry.RegisterAll(api, Mod, GetType().Assembly);
    CommandRegistry.RegisterAll(api, Mod, GetType().Assembly);
    // Mirror the recipe cost level on the client so the handbook/crafting grid agree with the server.
    ApplyRecipeLevel(api);
  }
  #endregion

  // Fills any auto-derived catalogue levels (persisting them), then applies the active level to the
  // live grid/RCC recipes. Recipes have resolved by StartServerSide/StartClientSide.
  private static void ApplyRecipeLevel(ICoreAPI api)
  {
    var catalogue = PpexRecipeValues.Recipes;
    // Repair any wrong edit / deletion against the code defaults before using the catalogue.
    bool changed = ExRecipeCosts.Reconcile(
      catalogue,
      PpexRecipeConfig.DefaultCatalogue()
    );
    changed |= ExRecipeCosts.EnsureNormalExtracted(api, catalogue);
    changed |= ExRecipeCosts.EnsureScaledLevel(catalogue, "cheap", 0.4);
    if (changed)
      PpexRecipeValues.Save();

    ExRecipeCosts.Apply(api, catalogue, PpexValues.RecipeLevel);
  }
}
