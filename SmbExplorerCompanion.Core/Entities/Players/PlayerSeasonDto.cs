namespace SmbExplorerCompanion.Core.Entities.Players;

public abstract class PlayerSeasonDto : PlayerDetailsBaseDto
{
    public int SeasonNumber { get; set; }
    public string TeamNames { get; set; } = string.Empty;
}