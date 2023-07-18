using CsvHelper.Configuration;

namespace SmbExplorerCompanion.Csv.Mappings;

public class SeasonSchedule
{
    public Guid HomeTeamId { get; set; }
    public string HomeTeamName { get; set; } = default!;
    public Guid AwayTeamId { get; set; }
    public string AwayTeamName { get; set; } = default!;
    public int SeasonId { get; set; }
    public int SeasonNum { get; set; }
    public int GlobalGameNumber { get; set; }
    public int Day { get; set; }
    public int? HomeScore { get; set; }
    public int? AwayScore { get; set; }
    public Guid? HomePitcherId { get; set; }
    public string? HomePitcherName { get; set; }
    public Guid? AwayPitcherId { get; set; }
    public string? AwayPitcherName { get; set; }

    public sealed class SeasonScheduleCsvMapping : ClassMap<SeasonSchedule>
    {
        public SeasonScheduleCsvMapping()
        {
            Map(x => x.HomeTeamId).Name("HomeTeamId");
            Map(x => x.HomeTeamName).Name("Home Team");
            Map(x => x.AwayTeamId).Name("AwayTeamId");
            Map(x => x.AwayTeamName).Name("Away Team");
            Map(x => x.SeasonId).Name("SeasonId");
            Map(x => x.SeasonNum).Name("Season");
            Map(x => x.GlobalGameNumber).Name("Game Number");
            Map(x => x.Day).Name("Day");
            Map(x => x.HomeScore).Name("Home Score");
            Map(x => x.AwayScore).Name("Away Score");
            Map(x => x.HomePitcherId).Name("HomePitcherId");
            Map(x => x.HomePitcherName).Name("Home Pitcher");
            Map(x => x.AwayPitcherId).Name("AwayPitcherId");
            Map(x => x.AwayPitcherName).Name("Away Pitcher");
        }
    }
}