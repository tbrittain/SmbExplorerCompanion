namespace SmbExplorerCompanion.Core.Entities.Players;

public class RetiredPlayerCareerStatsDto
{
    public List<PlayerCareerBattingDto> BattingCareers { get; set; } = new();
    public List<PlayerCareerPitchingDto> PitchingCareers { get; set; } = new();
}