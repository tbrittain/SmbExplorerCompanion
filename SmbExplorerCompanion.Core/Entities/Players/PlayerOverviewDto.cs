namespace SmbExplorerCompanion.Core.Entities.Players;

public class PlayerOverviewDto : PlayerCareerBaseDto
{
    public string CurrentTeam { get; set; } = string.Empty;
    public int? CurrentTeamId { get; set; }

    public PlayerCareerBattingDto CareerBatting { get; set; } = new();
    public PlayerCareerPitchingDto CareerPitching { get; set; } = new();

    public List<PlayerBattingOverviewDto> PlayerSeasonBatting { get; set; } = new();
    public List<PlayerBattingOverviewDto> PlayerPlayoffBatting { get; set; } = new();
    public List<PlayerPitchingOverviewDto> PlayerSeasonPitching { get; set; } = new();
    public List<PlayerPitchingOverviewDto> PlayerPlayoffPitching { get; set; } = new();
    public List<PlayerGameStatOverviewDto> GameStats { get; set; } = new();
}