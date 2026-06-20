// exlib-owned right-click construction behavior, referenced by the mega-blocks (engines,
// boilers, bessemer converter) under the JSON behavior name "ExRightClickConstructable".
//
// On 1.22 it is a thin subclass of the vanilla BEBehaviorRightClickConstructable, so the
// primary build reuses vanilla's well-tested logic unchanged and only adds a clean drops
// accessor (replacing the old reflection into the protected rcc field). On 1.20/1.21, where
// the vanilla behavior does not exist, it is a full reimplementation backed by
// ExRightClickConstruction. Owning the JSON name on all versions lets the mod C# reference a
// single type and keeps vanilla blocks (e.g. the waterwheel) on vanilla's own behavior.
using ExpandedLib.Registries.Entities;
using Vintagestory.API.Common;

namespace ExpandedLib.Blocks.Construction;

#if GAME_GE_1_22
using System;
using Vintagestory.GameContent;

[BlockEntityBehaviorRegister("ExRightClickConstructable", PrefixModId = false)]
public class ExRightClickConstructable(BlockEntity blockentity)
  : BEBehaviorRightClickConstructable(blockentity)
{
  /// <summary>The materials this block would scatter at <paramref name="ratio"/> (0..1) of the
  /// consumed stacks. Reaches the protected base <c>rcc</c> directly (no reflection needed).</summary>
  public ItemStack[] GetConstructionDrops(float ratio, Random rand) =>
    rcc.GetDrops(ratio, rand);
}
#else
using System;
using Vintagestory.API.Client;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

[BlockEntityBehaviorRegister("ExRightClickConstructable", PrefixModId = false)]
public class ExRightClickConstructable : BlockEntityBehavior, IInteractable
{
  private readonly ExRightClickConstruction rcc = new();
  private float brokenDropsRatio = 1f;

  public CompositeShape shape { get; protected set; }
  public bool IsComplete => rcc.CurrentCompletedStage == rcc.Stages.Length - 1;
  public event Action<CompositeShape>? OnShapeChanged;

  public ExRightClickConstructable(BlockEntity blockentity)
    : base(blockentity)
  {
    shape = blockentity.Block.Shape;
  }

  public override void Initialize(ICoreAPI api, JsonObject properties)
  {
    base.Initialize(api, properties);
    brokenDropsRatio = properties["brokenDropsRatio"].AsFloat(1f);
    var stages = properties["stages"].AsObject<ExConstructionStage[]>(null);
    rcc.LateInit(stages, api, "Block " + Block.Code);
    UpdateShape();
  }

  public bool OnBlockInteractStart(
    IWorldAccessor world,
    IPlayer byPlayer,
    BlockSelection blockSel,
    ref EnumHandling handling
  )
  {
    handling = EnumHandling.PreventDefault;
    if (rcc.OnInteract(byPlayer.Entity, byPlayer.Entity.RightHandItemSlot))
    {
      UpdateShape();
      Blockentity.MarkDirty(true);
    }
    return true;
  }

  public override void FromTreeAttributes(
    ITreeAttribute tree,
    IWorldAccessor world
  )
  {
    base.FromTreeAttributes(tree, world);
    rcc.FromTreeAttributes(tree);
  }

  public override void ToTreeAttributes(ITreeAttribute tree)
  {
    rcc.ToTreeAttributes(tree);
    base.ToTreeAttributes(tree);
    UpdateShape();
  }

  public override bool OnTesselation(
    ITerrainMeshPool mesher,
    ITesselatorAPI tessThreadTesselator
  ) => true;

  public override void GetBlockInfo(
    IPlayer forPlayer,
    System.Text.StringBuilder dsc
  )
  {
    base.GetBlockInfo(forPlayer, dsc);
    if (Api.World.EntityDebugMode)
      dsc.AppendLine(
        $"<font color='#ccc'>construction stage= {rcc.CurrentCompletedStage} of {rcc.Stages.Length}</font>"
      );
  }

  public override void OnBlockBroken(IPlayer? byPlayer = null)
  {
    if (byPlayer?.WorldData.CurrentGameMode != EnumGameMode.Creative)
      foreach (var drop in rcc.GetDrops(brokenDropsRatio, Api.World.Rand))
        Api.World.SpawnItemEntity(drop, Pos.ToVec3d());
  }

  /// <summary>The materials this block would scatter at <paramref name="ratio"/> (0..1) of the
  /// consumed stacks.</summary>
  public ItemStack[] GetConstructionDrops(float ratio, Random rand) =>
    rcc.GetDrops(ratio, rand);

  /// <summary>The next-stage build-material hover help. On 1.22 vanilla supplies this via
  /// IInteractableWithHelp; on legacy the host block surfaces it through
  /// <see cref="AppendConstructionHelp"/> from its GetPlacedBlockInteractionHelp override.</summary>
  public WorldInteraction[]? GetConstructionInteractionHelp() =>
    rcc.GetInteractionHelp();

  /// <summary>Prepends the construction help of the block-entity at the selection (if it has this
  /// behavior) to <paramref name="baseHelp"/>. For legacy block GetPlacedBlockInteractionHelp overrides.</summary>
  public static WorldInteraction[] AppendConstructionHelp(
    IWorldAccessor world,
    BlockSelection selection,
    WorldInteraction[] baseHelp
  )
  {
    var help = world
      .BlockAccessor.GetBlockEntity(selection.Position)
      ?.GetBehavior<ExRightClickConstructable>()
      ?.GetConstructionInteractionHelp();
    if (help == null || help.Length == 0)
      return baseHelp;
    if (baseHelp == null || baseHelp.Length == 0)
      return help;
    var combined = new WorldInteraction[help.Length + baseHelp.Length];
    help.CopyTo(combined, 0);
    baseHelp.CopyTo(combined, help.Length);
    return combined;
  }

  private void UpdateShape()
  {
    shape = new CompositeShape
    {
      Base = Block.Shape.Base,
      rotateY = Block.Shape.rotateY,
      SelectiveElements = rcc.getShapeElements(),
    };
    OnShapeChanged?.Invoke(shape);
  }
}
#endif
