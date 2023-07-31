namespace SmbExplorerCompanion.WPF.Models.Seasons;

public class Season
{
    public int Id { get; set; }
    public int FranchiseId { get; set; }
    public int Number { get; set; }
    public int NumGamesRegularSeason { get; set; }
    public int? ChampionshipWinnerId { get; set; }

    public bool HasPlayoffsCompleted => ChampionshipWinnerId is not null;
}