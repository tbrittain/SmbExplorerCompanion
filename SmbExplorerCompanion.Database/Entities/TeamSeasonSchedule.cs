namespace SmbExplorerCompanion.Database.Entities;

public class TeamSeasonSchedule
{
    // TODO: Ensure keys are set properly for this entity
    public int Id { get; set; }
    public int HomeTeamHistoryId { get; set; }
    public virtual SeasonTeamHistory HomeTeamHistory { get; set; } = default!;
    
    public int AwayTeamHistoryId { get; set; }
    public virtual SeasonTeamHistory AwayTeamHistory { get; set; } = default!;
    
    public int HomePitcherSeasonId { get; set; }
    public virtual PlayerSeason HomePlayerSeason { get; set; } = default!;
    
    public int AwayPitcherSeasonId { get; set; }
    public virtual PlayerSeason AwayPlayerSeason { get; set; } = default!;
    
    public int HomeScore { get; set; }
    public int AwayScore { get; set; }
}