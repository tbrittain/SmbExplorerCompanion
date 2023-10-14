namespace SmbExplorerCompanion.Core.Entities.Players;

public class PlayerGameStatOverviewDto : GameStatDto
{
    public int SeasonNumber { get; set; }
    public int SeasonId { get; set; }
    public int Age { get; set; }
    public string TeamNames { get; set; } = string.Empty;
    public int Salary { get; set; }
    public string? SecondaryPosition { get; set; } = string.Empty;
    public string Traits { get; set; } = string.Empty;
    public string PitchTypes { get; set; } = string.Empty;
}