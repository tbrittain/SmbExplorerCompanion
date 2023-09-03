namespace SmbExplorerCompanion.WPF.Models.Seasons;

public class Season
{
    public int Id { get; set; }
    public int FranchiseId { get; set; }
    public int Number { get; set; }
    public int NumGamesRegularSeason { get; set; }
    public int? ChampionshipWinnerId { get; set; }
    public bool IsNewSeason { get; set; }

    // ReSharper disable once UnusedMember.Global
    public string DisplaySeasonNumber
    {
        get
        {
            if (Number == default) return "All Seasons";
            if (IsNewSeason) return $"Season {Number} (New)";

            return $"Season {Number}";
        }
    }

    public bool CanSelectPlayoffs => Id == default || ChampionshipWinnerId is not null;
}