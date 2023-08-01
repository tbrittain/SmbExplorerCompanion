namespace SmbExplorerCompanion.Core.Entities.Seasons;

public class SeasonDto
{
    public int Id { get; set; }
    public int FranchiseId { get; set; }
    public int Number { get; set; }
    public int NumGamesRegularSeason { get; set; }
    public int? ChampionshipWinnerId { get; set; }
}