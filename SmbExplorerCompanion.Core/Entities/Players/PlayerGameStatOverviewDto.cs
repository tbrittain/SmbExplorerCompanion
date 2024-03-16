namespace SmbExplorerCompanion.Core.Entities.Players;

public class PlayerGameStatOverviewDto : GameStatDto
{
    public int SeasonNumber { get; set; }
    public int SeasonId { get; set; }
    public int Age { get; set; }
    public string TeamNames { get; set; } = string.Empty;
    public int Salary { get; set; }
    public string? SecondaryPosition { get; set; } = string.Empty;
    public List<int> TraitIds { get; set; } = new();
    public List<int> PitchTypeIds { get; set; } = new();
}