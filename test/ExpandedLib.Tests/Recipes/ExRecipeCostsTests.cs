using System.Collections.Generic;
using ExpandedLib.Registries.Recipes;
using ExpandedLib.Testing;
using Newtonsoft.Json.Linq;
using NSubstitute;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Xunit;

namespace ExpandedLib.Tests;

/// <summary>
/// The generic recipe-cost adjuster: scaling an alternate level from "normal", extracting "normal"
/// from a live recipe, and applying a level to grid recipes (direct set) and RCC blocks (distribute a
/// total across the stages, keeping the exact sum).
/// </summary>
public class ExRecipeCostsTests
{
  private static CraftingRecipeIngredient Ing(string code, int qty) =>
    new() { Code = new AssetLocation(code), Quantity = qty };

  private static GridRecipe GridRecipe(
    string output,
    params CraftingRecipeIngredient[] ings
  ) =>
    new()
    {
      Output = new CraftingRecipeIngredient
      {
        Code = new AssetLocation(output),
      },
      ResolvedIngredients = ings,
    };

  private static Block RccBlock(string code, JObject props)
  {
    var block = TestBlocks.Configure(new Block(), code, 70);
    block.BlockEntityBehaviors =
    [
      new BlockEntityBehaviorType
      {
        Name = "RightClickConstructable",
        properties = new JsonObject(props),
      },
    ];
    return block;
  }

  private static JObject Stage(string ingName, int qty) =>
    new()
    {
      ["requireStacks"] = new JArray
      {
        new JObject { ["name"] = ingName, ["quantity"] = qty },
      },
    };

  private static JArray Stages(Block block) =>
    (JArray)
      ((JObject)block.BlockEntityBehaviors[0].properties!.Token!)["stages"]!;

  #region Scale + extract

  [Fact]
  public void A_scaled_level_reduces_normal_and_floors_each_at_one()
  {
    var cat = new Dictionary<string, RecipeCostEntry>
    {
      ["x"] = new()
      {
        Kind = "grid",
        Match = "m:x",
        Levels = new()
        {
          ["normal"] = new() { ["a"] = 10, ["b"] = 1 },
        },
      },
    };

    Assert.True(ExRecipeCosts.EnsureScaledLevel(cat, "cheap", 0.4));

    Assert.Equal(4, cat["x"].Levels["cheap"]["a"]); // 10 * 0.4
    Assert.Equal(1, cat["x"].Levels["cheap"]["b"]); // 0.4 -> floored to 1
  }

  [Fact]
  public void A_scaled_level_never_overwrites_a_preset_level()
  {
    var cat = new Dictionary<string, RecipeCostEntry>
    {
      ["x"] = new()
      {
        Kind = "rcc",
        Match = "m:x",
        Levels = new()
        {
          ["normal"] = new() { ["a"] = 10 },
          ["cheap"] = new() { ["a"] = 2 }, // pinned (e.g. the Watt example)
        },
      },
    };

    ExRecipeCosts.EnsureScaledLevel(cat, "cheap", 0.4);

    Assert.Equal(2, cat["x"].Levels["cheap"]["a"]);
  }

  [Fact]
  public void Normal_is_extracted_from_the_live_grid_recipe()
  {
    var world = new TestWorld();
    world.World.GridRecipes.Returns(
      new List<GridRecipe>
      {
        GridRecipe("ppex:enginewatt-north", Ing("game:rod-iron", 8)),
      }
    );

    var cat = new Dictionary<string, RecipeCostEntry>
    {
      ["watt-grid"] = new() { Kind = "grid", Match = "ppex:enginewatt-*" },
    };

    Assert.True(ExRecipeCosts.EnsureNormalExtracted(world.Api, cat));
    Assert.Equal(8, cat["watt-grid"].Levels["normal"]["game:rod-iron"]);
  }

  #endregion

  #region Reconcile (robustness against bad edits)

  private static Dictionary<string, RecipeCostEntry> DefaultCat() =>
    new()
    {
      ["watt-rcc"] = new()
      {
        Kind = "rcc",
        Match = "ppex:enginewatt-*",
        Levels = new() { ["cheap"] = new() { ["plate"] = 2 } },
      },
      ["watt-grid"] = new() { Kind = "grid", Match = "ppex:enginewatt-*" },
    };

  [Fact]
  public void Reconcile_restores_a_deleted_entry()
  {
    var live = new Dictionary<string, RecipeCostEntry>(); // player wiped everything

    Assert.True(ExRecipeCosts.Reconcile(live, DefaultCat()));

    Assert.True(live.ContainsKey("watt-rcc"));
    Assert.Equal("ppex:enginewatt-*", live["watt-rcc"].Match);
    Assert.Equal(2, live["watt-rcc"].Levels["cheap"]["plate"]);
  }

  [Fact]
  public void Reconcile_repairs_structural_fields_and_restores_a_deleted_pinned_level()
  {
    var live = new Dictionary<string, RecipeCostEntry>
    {
      // Player blanked the match and deleted the pinned cheap level.
      ["watt-rcc"] = new()
      {
        Kind = "grid",
        Match = "",
        Levels = new(),
      },
    };

    Assert.True(ExRecipeCosts.Reconcile(live, DefaultCat()));

    Assert.Equal("rcc", live["watt-rcc"].Kind); // restored
    Assert.Equal("ppex:enginewatt-*", live["watt-rcc"].Match); // restored
    Assert.Equal(2, live["watt-rcc"].Levels["cheap"]["plate"]); // restored pin
  }

  [Fact]
  public void Reconcile_clamps_nonsense_quantities_to_at_least_one()
  {
    var live = new Dictionary<string, RecipeCostEntry>
    {
      ["watt-grid"] = new()
      {
        Kind = "grid",
        Match = "ppex:enginewatt-*",
        Levels = new()
        {
          ["cheap"] = new() { ["a"] = 0, ["b"] = -5 },
        },
      },
    };

    ExRecipeCosts.Reconcile(live, DefaultCat());

    Assert.Equal(1, live["watt-grid"].Levels["cheap"]["a"]);
    Assert.Equal(1, live["watt-grid"].Levels["cheap"]["b"]);
  }

  [Fact]
  public void Reconcile_keeps_valid_player_numbers_and_extra_entries()
  {
    var live = new Dictionary<string, RecipeCostEntry>
    {
      ["watt-grid"] = new()
      {
        Kind = "grid",
        Match = "ppex:enginewatt-*",
        Levels = new() { ["cheap"] = new() { ["a"] = 3 } }, // intentional edit
      },
      ["my-custom"] = new()
      {
        Kind = "grid",
        Match = "mod:thing",
        Levels = new() { ["cheap"] = new() { ["x"] = 7 } },
      },
    };

    ExRecipeCosts.Reconcile(live, DefaultCat());

    Assert.Equal(3, live["watt-grid"].Levels["cheap"]["a"]); // kept
    Assert.True(live.ContainsKey("my-custom")); // extra entry kept
    Assert.Equal(7, live["my-custom"].Levels["cheap"]["x"]);
  }

  #endregion

  #region Apply

  [Fact]
  public void Applying_a_grid_level_sets_the_ingredient_quantities()
  {
    var world = new TestWorld();
    var plate = Ing("game:metalplate-iron", 4);
    var rod = Ing("game:rod-iron", 8);
    world.World.GridRecipes.Returns(
      new List<GridRecipe> { GridRecipe("ppex:enginewatt-north", plate, rod) }
    );

    var cat = new Dictionary<string, RecipeCostEntry>
    {
      ["watt-grid"] = new()
      {
        Kind = "grid",
        Match = "ppex:enginewatt-*",
        Levels = new()
        {
          ["cheap"] = new()
          {
            ["game:metalplate-iron"] = 2,
            ["game:rod-iron"] = 4,
          },
        },
      },
    };

    ExRecipeCosts.Apply(world.Api, cat, "cheap");

    Assert.Equal(2, plate.Quantity);
    Assert.Equal(4, rod.Quantity);
  }

  [Fact]
  public void Applying_an_rcc_level_distributes_the_total_and_keeps_the_exact_sum()
  {
    var world = new TestWorld();
    // Same ingredient across two stages: 4 + 2 = 6 normally.
    var props = new JObject
    {
      ["stages"] = new JArray { Stage("plate", 4), Stage("plate", 2) },
    };
    var block = RccBlock("ppex:enginewatt-north", props);
    world.World.Blocks.Returns(new List<Block> { block });

    var cat = new Dictionary<string, RecipeCostEntry>
    {
      ["watt-rcc"] = new()
      {
        Kind = "rcc",
        Match = "ppex:enginewatt-*",
        Levels = new() { ["cheap"] = new() { ["plate"] = 3 } },
      },
    };

    ExRecipeCosts.Apply(world.Api, cat, "cheap");

    var stages = Stages(block);
    int q0 = (int)stages[0]["requireStacks"]![0]!["quantity"]!;
    int q1 = (int)stages[1]["requireStacks"]![0]!["quantity"]!;
    Assert.Equal(3, q0 + q1); // total hit exactly
    Assert.True(q0 >= q1); // proportional to the original 4:2 split
  }

  [Fact]
  public void Applying_an_unknown_level_leaves_recipes_untouched()
  {
    var world = new TestWorld();
    var rod = Ing("game:rod-iron", 8);
    world.World.GridRecipes.Returns(
      new List<GridRecipe> { GridRecipe("ppex:enginewatt-north", rod) }
    );

    var cat = new Dictionary<string, RecipeCostEntry>
    {
      ["watt-grid"] = new()
      {
        Kind = "grid",
        Match = "ppex:enginewatt-*",
        Levels = new() { ["cheap"] = new() { ["game:rod-iron"] = 4 } },
      },
    };

    ExRecipeCosts.Apply(world.Api, cat, "normal"); // not present -> no-op

    Assert.Equal(8, rod.Quantity);
  }

  #endregion
}
