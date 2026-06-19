using ExpandedLib.Testing;
using NSubstitute;
using SteelmakingExpanded.BlockStructures.BlastFurnace.BlockEntities;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Xunit;

namespace SteelmakingExpanded.Tests;

/// <summary>
/// The reinforced hopper block entity (the blast-mix magazine's loader): its typed 8-slot layout (the
/// iron/coke/flux columns the bell hopper crafts from) and the block-info readout that surfaces the
/// bell's magazine/dropping state below it. The slot accept rules are covered by
/// <see cref="HopperSlotTests"/>; this pins the slot <em>layout</em> and the info text.
/// </summary>
public class HopperReinforcedBeTests
{
  private static BlockEntityHopperReinforced Hopper(TestWorld world, BlockPos pos)
  {
    var be = new BlockEntityHopperReinforced
    {
      Pos = pos,
      Block = TestBlocks.Configure(new Block(), "smex:hopperreinforced", 91),
    };
    world.Place(pos, be.Block, be);
    world.Attach(be);
    return be;
  }

  private static BlockEntityHopperBell BellBelow(TestWorld world, BlockEntityHopperReinforced hopper)
  {
    var pos = hopper.Pos.DownCopy();
    var bell = new BlockEntityHopperBell
    {
      Pos = pos,
      Block = TestBlocks.Configure(new Block(), "smex:hopperbell", 90),
    };
    world.Place(pos, bell.Block, bell);
    world.Attach(bell);
    return bell;
  }

  /// <summary>A player holding Ctrl (for the bell-dropping toggle interaction).</summary>
  private static IPlayer CtrlPlayer()
  {
    var player = Substitute.For<IPlayer>();
    var entity = Substitute.For<EntityPlayer>();
    entity.Controls.CtrlKey = true; // Controls is a real field on the proxy
    player.Entity.Returns(entity);
    return player;
  }

  #region Slot layout

  [Theory]
  [InlineData(0, "iron")]
  [InlineData(1, "iron")]
  [InlineData(2, "coke")]
  [InlineData(3, "lime")]
  [InlineData(4, "iron")]
  [InlineData(5, "iron")]
  [InlineData(6, "coke")]
  [InlineData(7, "lime")]
  public void The_eight_slots_are_typed_iron_coke_and_flux(int slot, string expectedType)
  {
    var hopper = Hopper(new TestWorld(), new BlockPos(0, 16, 0));

    Assert.Equal(8, hopper.Inventory.Count);
    var typed = Assert.IsType<ItemSlotBlastFurnace>(hopper.Inventory[slot]);
    Assert.Equal(expectedType, typed.AllowedType);
  }

  #endregion

  #region Bell-dropping toggle

  [Fact]
  public void Ctrl_interacting_toggles_the_bell_hoppers_dropping()
  {
    var world = new TestWorld();
    var hopper = Hopper(world, new BlockPos(0, 16, 0));
    var bell = BellBelow(world, hopper);
    Assert.False(bell.IsDropping);

    hopper.OnInteract(CtrlPlayer()); // ctrl + right-click starts the drop
    Assert.True(bell.IsDropping);

    hopper.OnInteract(CtrlPlayer()); // and again stops it
    Assert.False(bell.IsDropping);
  }

  #endregion
}
