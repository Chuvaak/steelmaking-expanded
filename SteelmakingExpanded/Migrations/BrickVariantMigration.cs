using System.Collections.Generic;
using BlockMigrationLib;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace SteelmakingExpanded.Migrations;

/// <summary>
/// Migrates blocks that gained a brick/refractory-tier variantgroup. The gas passthrough,
/// outlet and heated-intake, plus the cowper-stove intake, originally used codes without
/// that group (e.g. <c>smex:gaspipe-passthrough-ns</c>, <c>smex:cowperstove-intake-s</c>);
/// adding the group changed those codes, so old placements load as missing-block
/// placeholders. Each is rewritten to the tier3-refractory variant of the same base and
/// orientation (refractory brick was the original recipe ingredient and shape texture).
/// </summary>
public class BrickVariantMigration : IBlockCodeMigration
{
  /// <summary>
  /// One block that gained a variant: its code without the new group, the variant value
  /// inserted before the orientation, and the orientations that existed beforehand.
  /// </summary>
  private readonly record struct Entry(
    string CodeBase,
    string InsertedVariant,
    string[] Orientations
  );

  private static readonly Entry[] Entries =
  [
    new("gaspipe-passthrough", "refractorytier3", ["ns", "we", "ud"]),
    new("gaspipe-outlet", "refractorytier3", ["s", "n", "w", "e"]),
    new("gaspipe-heated", "refractorytier3", ["ns", "sn", "we", "ew"]),
    new("cowperstove-intake", "tier3", ["n", "s", "w", "e"]),
    new("smokestack-intake", "tier3", ["n", "s", "w", "e"]),
  ];

  public string Name => "Brick and refractory-tier variants";

  public IEnumerable<(AssetLocation oldCode, AssetLocation newCode)> GetRemaps(
    ICoreServerAPI api
  )
  {
    foreach (var (codeBase, inserted, orientations) in Entries)
    foreach (string orient in orientations)
      yield return (
        new AssetLocation("smex", $"{codeBase}-{orient}"),
        new AssetLocation("smex", $"{codeBase}-{inserted}-{orient}")
      );
  }
}
