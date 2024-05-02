namespace SmbExplorerCompanion.Core.Entities.Players;

public abstract class PlayerCareerBaseDto : PlayerDetailBaseDto
{
    public int StartSeasonNumber { get; set; }
    public int EndSeasonNumber { get; set; }
    public bool IsRetired { get; set; }
    public int Age { get; set; }
    public int? RetiredCurrentAge { get; set; }
    public int NumSeasons { get; set; }
    public List<int> AwardIds { get; set; } = new();
    public int NumChampionships { get; set; }
    public bool IsHallOfFamer { get; set; }
}