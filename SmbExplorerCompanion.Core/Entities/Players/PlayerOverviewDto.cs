namespace SmbExplorerCompanion.Core.Entities.Players;

public class PlayerOverviewDto : PlayerCareerBaseDto
{
    public string CurrentTeam { get; set; } = string.Empty;
    public int? CurrentTeamId { get; set; }

    public PlayerCareerBattingDto CareerBatting { get; set; } = new();
    public PlayerCareerPitchingDto CareerPitching { get; set; } = new();

    public List<PlayerBattingSeasonDto> PlayerSeasonBatting { get; set; } = new();
    public List<PlayerBattingSeasonDto> PlayerPlayoffBatting { get; set; } = new();
    public List<PlayerPitchingSeasonDto> PlayerSeasonPitching { get; set; } = new();
    public List<PlayerPitchingSeasonDto> PlayerPlayoffPitching { get; set; } = new();
    public List<PlayerGameStatOverviewDto> GameStats { get; set; } = new();
}