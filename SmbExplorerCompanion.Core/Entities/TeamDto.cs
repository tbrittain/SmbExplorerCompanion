namespace SmbExplorerCompanion.Core.Entities;

public class TeamDto
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public List<SeasonTeamHistoryDto> SeasonTeamHistory { get; set; } = new();
}