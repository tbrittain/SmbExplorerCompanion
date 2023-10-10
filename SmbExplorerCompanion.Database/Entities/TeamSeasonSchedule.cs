namespace SmbExplorerCompanion.Database.Entities;

public class TeamSeasonSchedule
{
    public int Id { get; set; }
    public int HomeTeamHistoryId { get; set; }
    public virtual SeasonTeamHistory HomeTeamHistory { get; set; } = default!;

    public int AwayTeamHistoryId { get; set; }
    public virtual SeasonTeamHistory AwayTeamHistory { get; set; } = default!;

    public int? HomePitcherSeasonId { get; set; }
    public virtual PlayerSeason? HomePitcherSeason { get; set; }

    public int? AwayPitcherSeasonId { get; set; }
    public virtual PlayerSeason? AwayPitcherSeason { get; set; }

    public int Day { get; set; }
    public int GlobalGameNumber { get; set; }

    public int? HomeScore { get; set; }
    public int? AwayScore { get; set; }
}