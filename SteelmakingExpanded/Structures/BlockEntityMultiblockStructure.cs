using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;

namespace SteelmakingExpanded.Structures;

/// <summary>
/// Base block entity for the mod's multiblock machines (blast furnace, cowper stove,
/// bessemer control). Runs a slow monitor tick that detects when the structure is
/// completed or broken, and a production tick that fires only while complete.
/// Subclasses supply the orientation logic, production behavior, and status messages.
/// </summary>
public abstract class BlockEntityMultiblockStructure : BlockEntity
{
  protected MultiblockStructure? _structure;
  protected MultiblockStructure? _highlightedStructure;
  protected int _currentAngle = -1;
  private long _completionTickId;
  private long _productionTickId;

  /// <summary>Whether every block of the multiblock structure is currently in place.</summary>
  public bool StructureComplete { get; protected set; }

  /// <summary>Interval (ms) of the structure-completion monitor tick.</summary>
  protected virtual int CompletionTickMs => 3000;

  /// <summary>Interval (ms) of the production tick (runs only while complete).</summary>
  protected virtual int ProductionTickMs => 1000;

  public override void Initialize(ICoreAPI api)
  {
    base.Initialize(api);
    if (api.Side == EnumAppSide.Server)
    {
      // The monitor tick runs unconditionally so the structure is detected both
      // when it is completed (gain) and when it is broken (loss). The production
      // tick only runs while complete — and each OnProductionTick also guards on
      // StructureComplete, so it is harmless if it briefly runs otherwise.
      StartMonitorTick();
      if (StructureComplete)
        StartProductionTick();
    }
  }

  /// <summary>Starts both the completion monitor and the production tick.</summary>
  protected void StartStructureTick()
  {
    StartMonitorTick();
    StartProductionTick();
  }

  protected void StartMonitorTick()
  {
    if (_completionTickId == 0 && Api.Side == EnumAppSide.Server)
      _completionTickId = RegisterGameTickListener(
        OnMonitorStructureTick,
        CompletionTickMs
      );
  }

  protected void StartProductionTick()
  {
    if (_productionTickId == 0 && Api.Side == EnumAppSide.Server)
      _productionTickId = RegisterGameTickListener(
        OnProductionTick,
        ProductionTickMs
      );
  }

  protected void StopProductionTick()
  {
    if (_productionTickId != 0)
    {
      UnregisterGameTickListener(_productionTickId);
      _productionTickId = 0;
    }
  }

  /// <summary>Stops both ticks (used on block removal).</summary>
  protected void StopStructureTick()
  {
    StopProductionTick();
    if (_completionTickId != 0)
    {
      UnregisterGameTickListener(_completionTickId);
      _completionTickId = 0;
    }
  }

  private void OnMonitorStructureTick(float dt)
  {
    UpdateStructureRotation();
    if (_structure == null)
      return;

    bool nowComplete = _structure.InCompleteBlockCount(Api.World, Pos) == 0;
    if (nowComplete == StructureComplete)
      return;

    StructureComplete = nowComplete;
    if (nowComplete)
    {
      OnStructureCompleted();
      StartProductionTick();
    }
    else
    {
      OnStructureLost();
      StopProductionTick();
    }
    MarkDirty(true);
  }

  /// <summary>Called when a previously complete structure becomes incomplete. Default: no-op.</summary>
  protected virtual void OnStructureLost() { }

  /// <summary>Per-tick production logic; runs server-side only while the structure is complete.</summary>
  protected abstract void OnProductionTick(float dt);

  /// <summary>Recomputes the structure's rotation/angle from the block orientation.</summary>
  protected abstract void UpdateStructureRotation();

  /// <summary>Converts a structure-local offset into a world position for the current rotation.</summary>
  protected virtual BlockPos GetGlobalPos(int localX, int localY, int localZ)
  {
    var (dx, dz) = _currentAngle switch
    {
      90 => (localZ, -localX),
      180 => (-localX, -localZ),
      270 => (-localZ, localX),
      _ => (localX, localZ), // Default case covers 0 degrees
    };

    return Pos.AddCopy(dx, localY, dz);
  }

  /// <summary>
  /// Player interaction entry point: re-checks completeness, fires the completed/lost
  /// callbacks, and (client-side) shows the build outline or a status message.
  /// </summary>
  public virtual void Interact(IPlayer byPlayer)
  {
    UpdateStructureRotation();
    if (_structure == null)
      return;

    if (byPlayer.WorldData.EntityControls.ShiftKey)
    {
      if (Api is ICoreClientAPI capi)
      {
        _highlightedStructure?.ClearHighlights(Api.World, byPlayer);
        _highlightedStructure = null;
      }
      return;
    }

    // Tally which blocks are missing (keyed by the wanted code) while counting,
    // so we can both draw the projection and print an exact shopping list.
    var missingByCode = new Dictionary<AssetLocation, int>();
    int missingCount = _structure.InCompleteBlockCount(
      Api.World,
      Pos,
      (haveBlock, wantBlockCode) =>
      {
        // Slots that air satisfies (the open shaft, an optional coal pile) are
        // meant to be left empty — they are not materials to gather.
        if (AcceptsAir(wantBlockCode))
          return;
        missingByCode.TryGetValue(wantBlockCode, out int count);
        missingByCode[wantBlockCode] = count + 1;
      }
    );
    bool wasComplete = StructureComplete;
    StructureComplete = missingCount == 0;

    if (Api.Side == EnumAppSide.Server)
    {
      if (StructureComplete && !wasComplete)
      {
        OnStructureCompleted();
        StartStructureTick();
        MarkDirty(true);
      }
      else if (!StructureComplete && wasComplete)
      {
        OnStructureLost();
        StopProductionTick();
        MarkDirty(true);
      }

      if (!StructureComplete && byPlayer is IServerPlayer serverPlayer)
        SendMissingBlocksReport(serverPlayer, missingByCode);
    }

    if (Api is ICoreClientAPI clientApi)
    {
      if (missingCount > 0)
      {
        _highlightedStructure = _structure;
        clientApi.TriggerIngameError(
          this,
          "incomplete",
          GetIncompleteMessage(missingCount)
        );
        HighlightIncompleteSafe(_highlightedStructure, byPlayer);
      }
      else
      {
        clientApi.TriggerIngameError(this, "complete", GetCompleteMessage());
        _highlightedStructure?.ClearHighlights(Api.World, byPlayer);
        _highlightedStructure = null;
      }
    }
  }

  /// <summary>
  /// Crash-safe replacement for vanilla <see cref="MultiblockStructure.HighlightIncompleteParts"/>.
  /// The vanilla method tints each empty slot with <c>SearchBlocks(wantedCode)[0]</c>;
  /// when a wanted (possibly wildcard) code resolves to no registered block, that
  /// indexes an empty array and throws <see cref="System.IndexOutOfRangeException"/>.
  /// Because this runs from a render-stage interaction, the exception is a critical
  /// mod error that aborts the whole interaction — so the build outline silently
  /// never appears. This mirrors the vanilla logic but falls back to a neutral tint
  /// for any slot whose wanted block cannot be resolved.
  /// </summary>
  private void HighlightIncompleteSafe(
    MultiblockStructure structure,
    IPlayer player
  )
  {
    var offsets = structure.TransformedOffsets;
    if (offsets == null)
      return;

    // Vanilla keeps its number -> code map private, so rebuild it from the public
    // BlockNumbers dictionary to learn what block each offset slot wants.
    var codeByNumber = new Dictionary<int, AssetLocation>();
    foreach (var kv in structure.BlockNumbers)
      codeByNumber[kv.Value] = kv.Key;

    var positions = new List<BlockPos>();
    var colors = new List<int>();

    foreach (var offset in offsets)
    {
      if (!codeByNumber.TryGetValue(offset.W, out AssetLocation? wanted))
        continue;

      Block actual = Api.World.BlockAccessor.GetBlockRaw(
        Pos.X + offset.X,
        Pos.InternalY + offset.Y,
        Pos.Z + offset.Z
      );
      if (WildcardUtil.Match(wanted, actual.Code))
        continue;

      positions.Add(new BlockPos(offset.X, offset.Y, offset.Z).Add(Pos));

      if (actual.Id != 0)
      {
        // A wrong solid block occupies the slot — vanilla tints these red.
        colors.Add(ColorUtil.ColorFromRgba(215, 94, 94, 0x60));
        continue;
      }

      // Empty slot: tint with the wanted block's color when it resolves, otherwise
      // fall back to a neutral blue instead of crashing on an empty SearchBlocks.
      Block[] matches = Api.World.SearchBlocks(wanted);
      if (matches.Length == 0)
      {
        colors.Add(ColorUtil.ColorFromRgba(94, 94, 215, 0x60));
        continue;
      }

      int color = matches[0].GetColor(Api as ICoreClientAPI, Pos) & 0xFFFFFF;
      color |= 0x60 << 24;
      colors.Add(color);
    }

    Api.World.HighlightBlocks(
      player,
      MultiblockStructure.HighlightSlotId,
      positions,
      colors
    );
  }

  private static readonly AssetLocation AirCode = new("game:air");

  /// <summary>
  /// True when an empty (air) block satisfies this structure slot — e.g. the
  /// "game:air" interior or an "@(air|coalpile)" fuel slot. Such positions are
  /// meant to be left empty, so they are not counted as materials to gather.
  /// </summary>
  private static bool AcceptsAir(AssetLocation wantBlockCode) =>
    WildcardUtil.Match(wantBlockCode, AirCode);

  /// <summary>
  /// Sends the player a chat breakdown of every block still missing from the
  /// structure and how many of each, resolving (possibly wildcard) codes to
  /// readable block names.
  /// </summary>
  private void SendMissingBlocksReport(
    IServerPlayer player,
    Dictionary<AssetLocation, int> missingByCode
  )
  {
    if (missingByCode.Count == 0)
      return;

    var sb = new StringBuilder();
    sb.Append(Lang.Get("smex:structure-missing-header"));

    foreach (
      var entry in missingByCode
        .OrderByDescending(e => e.Value)
        .ThenBy(e => ResolveBlockName(e.Key))
    )
    {
      sb.Append('\n');
      sb.Append(
        Lang.Get(
          "smex:structure-missing-line",
          entry.Value,
          ResolveBlockName(entry.Key)
        )
      );
    }

    player.SendMessage(
      GlobalConstants.GeneralChatGroup,
      sb.ToString(),
      EnumChatType.Notification
    );
  }

  /// <summary>
  /// Resolves a structure block code — which may be a wildcard such as
  /// "smex:blastfurnacedoor*" — to a human-readable display name.
  /// </summary>
  private string ResolveBlockName(AssetLocation wantBlockCode)
  {
    Block? block = Api.World.GetBlock(wantBlockCode);
    if (block == null)
    {
      Block[] matches = Api.World.SearchBlocks(wantBlockCode);
      if (matches.Length > 0)
        block = matches[0];
    }

    return block != null
      ? new ItemStack(block).GetName()
      : wantBlockCode.ToShortString();
  }

  /// <summary>Called when the structure transitions to complete. Default: no-op.</summary>
  protected virtual void OnStructureCompleted() { }

  /// <summary>Returns the ingame-error message shown when the structure is missing <paramref name="missingCount"/> blocks.</summary>
  protected abstract string GetIncompleteMessage(int missingCount);

  /// <summary>Returns the ingame-error message shown when the structure is complete.</summary>
  protected abstract string GetCompleteMessage();

  public override void OnBlockRemoved()
  {
    base.OnBlockRemoved();
    StopStructureTick();
    if (Api is ICoreClientAPI capi)
      _highlightedStructure?.ClearHighlights(Api.World, capi.World.Player);
  }

  public override void ToTreeAttributes(ITreeAttribute tree)
  {
    base.ToTreeAttributes(tree);
    tree.SetBool("structureComplete", StructureComplete);
  }

  public override void FromTreeAttributes(
    ITreeAttribute tree,
    IWorldAccessor worldForResolving
  )
  {
    base.FromTreeAttributes(tree, worldForResolving);
    StructureComplete = tree.GetBool("structureComplete");
  }
}
