namespace SmbExplorerCompanion.Core.Entities.Players;

public abstract class PlayerSeasonDto : PlayerDetailBaseDto
{
    public int SeasonNumber { get; set; }
    public string TeamNames { get; set; } = string.Empty;
    public int Age { get; set; }
}