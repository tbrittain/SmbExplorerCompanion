using SmbExplorerCompanion.Shared.Enums;
using SmbExplorerCompanion.Shared.Utils;

namespace SmbExplorerCompanion.Core.Entities.Players;

public class PlayerFieldingRankingDto : PlayerBaseDto
{
    public string TeamNames { get; set; } = string.Empty;
    public string PrimaryPosition { get; set; } = string.Empty;
    private Position Position => EnumUtils.ParseEnumByDescription<Position>(PrimaryPosition);
    public int SeasonId { get; set; }
    public int Speed { get; set; }
    public int Fielding { get; set; }
    public int? Arm { get; set; }
    public int? PlateAppearances { get; set; }
    public double? InningsPitched { get; set; }

    public int Errors { get; set; }
    public int PassedBalls { get; set; }

    /// <summary>
    ///     Cannon Arm, Dive Wizard, Utility, Magic Hands
    /// </summary>
    public List<string> PositiveFieldingTraits { get; init; } = new();

    /// <summary>
    ///     Noodle Arm, Wild Thrower, Butter Fingers
    /// </summary>
    public List<string> NegativeFieldingTraits { get; init; } = new();

    /// <summary>
    ///     This will be a weighted ranking of the player's fielding ability,
    ///     as a combination of their fielding, speed, and arm ratings,
    ///     their fielding traits, and the number of errors and passed balls they have,
    ///     which also depends on the number of innings/plate appearances they have
    /// </summary>
    public double WeightedFieldingRanking { get; set; }
}