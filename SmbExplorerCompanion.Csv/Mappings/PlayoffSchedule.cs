using CsvHelper.Configuration;

namespace SmbExplorerCompanion.Csv.Mappings;

public class PlayoffSchedule
{
    public Guid Team1Id { get; set; }
    public string Team1Name { get; set; } = default!;
    public int Team1Seed { get; set; }
    public Guid Team2Id { get; set; }
    public string Team2Name { get; set; } = default!;
    public int Team2Seed { get; set; }
    public int SeasonId { get; set; }
    public int SeasonNum { get; set; }
    public int SeriesNum { get; set; }
    public int GlobalGameNumber { get; set; }
    public Guid HomeTeamId { get; set; }
    public string HomeTeamName { get; set; } = default!;
    public Guid AwayTeamId { get; set; }
    public string AwayTeamName { get; set; } = default!;
    public int? HomeScore { get; set; }
    public int? AwayScore { get; set; }
    public Guid? HomePitcherId { get; set; }
    public string? HomePitcherName { get; set; }
    public Guid? AwayPitcherId { get; set; }
    public string? AwayPitcherName { get; set; }

    public sealed class PlayoffScheduleCsvMapping : ClassMap<PlayoffSchedule>
    {
        public PlayoffScheduleCsvMapping()
        {
            Map(x => x.Team1Id).Name("Team1Id");
            Map(x => x.Team1Name).Name("Team 1");
            Map(x => x.Team1Seed).Name("Team 1 Seed");
            Map(x => x.Team2Id).Name("Team2Id");
            Map(x => x.Team2Name).Name("Team 2");
            Map(x => x.Team2Seed).Name("Team 2 Seed");
            Map(x => x.SeasonId).Name("SeasonId");
            Map(x => x.SeasonNum).Name("Season");
            Map(x => x.SeriesNum).Name("Series");
            Map(x => x.GlobalGameNumber).Name("Game Number");
            Map(x => x.HomeTeamId).Name("HomeTeamId");
            Map(x => x.HomeTeamName).Name("Home Team");
            Map(x => x.AwayTeamId).Name("AwayTeamId");
            Map(x => x.AwayTeamName).Name("Away Team");
            Map(x => x.HomeScore).Name("Home Score");
            Map(x => x.AwayScore).Name("Away Score");
            Map(x => x.HomePitcherId).Name("HomePitcherId");
            Map(x => x.HomePitcherName).Name("Home Pitcher");
            Map(x => x.AwayPitcherId).Name("AwayPitcherId");
            Map(x => x.AwayPitcherName).Name("Away Pitcher");
        }
    }
}