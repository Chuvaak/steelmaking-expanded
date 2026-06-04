using System.Collections.Generic;
using BlockMigrationLib;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace SteelmakingExpanded.Migrations;

/// <summary>
/// Migrates pre-brick-variant gas blocks. The passthrough, outlet and heated-intake
/// blocks originally used brickless codes (e.g. <c>smex:gaspipe-passthrough-ns</c>);
/// adding the brick variantgroup changed those codes, so old placements load as
/// missing-block placeholders. Each is rewritten to the refractory-tier3 variant of the
/// same type and orientation (refractory brick was the original recipe ingredient and
/// shape texture).
/// </summary>
public class BrickVariantMigration : IBlockCodeMigration
{
  // type -> the orientations that existed before the brick variantgroup was added.
  private static readonly Dictionary<string, string[]> BricklessOrientations =
    new()
    {
      { "passthrough", ["ns", "we", "ud"] },
      { "outlet", ["s", "n", "w", "e"] },
      { "heated", ["ns", "sn", "we", "ew"] },
    };

  public string Name => "Gas block brick variants";

  public IEnumerable<(AssetLocation oldCode, AssetLocation newCode)> GetRemaps(
    ICoreServerAPI api
  )
  {
    foreach (var (type, orientations) in BricklessOrientations)
    foreach (string orient in orientations)
      yield return (
        new AssetLocation("smex", $"gaspipe-{type}-{orient}"),
        new AssetLocation("smex", $"gaspipe-{type}-refractorytier3-{orient}")
      );
  }
}
