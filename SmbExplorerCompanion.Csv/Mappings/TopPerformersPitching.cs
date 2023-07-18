using CsvHelper.Configuration;

namespace SmbExplorerCompanion.Csv.Mappings;

public class TopPerformersPitching : TopPerformersBase
{
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int CompleteGames { get; set; }
    public int Shutouts { get; set; }
    public int Hits { get; set; }
    public int EarnedRuns { get; set; }
    public int HomeRuns { get; set; }
    public int Walks { get; set; }
    public int Strikeouts { get; set; }
    public double? InningsPitched { get; set; }
    public double? Era { get; set; }
    public int TotalPitches { get; set; }
    public int Saves { get; set; }
    public int HitByPitch { get; set; }
    public int BattersFaced { get; set; }
    public int GamesPlayed { get; set; }
    public int GamesStarted { get; set; }
    public int GamesFinished { get; set; }
    public int RunsAllowed { get; set; }
    public int WildPitches { get; set; }
    public double? BattingAverageAgainst { get; set; }
    public double? Fip { get; set; }
    public double? Whip { get; set; }
    public double? WinPercentage { get; set; }
    public double? OpponentObp { get; set; }
    public double? StrikeoutsPerWalk { get; set; }
    public double? StrikeoutsPerNine { get; set; }
    public double? WalksPerNine { get; set; }
    public double? HitsPerNine { get; set; }
    public double? HomeRunsPerNine { get; set; }
    public double? PitchesPerInning { get; set; }
    public double? PitchesPerGame { get; set; }
    public double? EraMinus { get; set; }
    public double? FipMinus { get; set; }

    public sealed class TopPerformersPitchingCsvMapping : ClassMap<TopPerformersPitching>
    {
        public TopPerformersPitchingCsvMapping()
        {
            Map(x => x.PlayerId).Name("PlayerId");
            Map(x => x.SeasonId).Name("SeasonId");
            Map(x => x.SeasonNum).Name("Season");
            Map(x => x.FirstName).Name("First Name");
            Map(x => x.LastName).Name("Last Name");
            Map(x => x.Wins).Name("W");
            Map(x => x.Losses).Name("L");
            Map(x => x.CompleteGames).Name("CG");
            Map(x => x.Shutouts).Name("CGSO");
            Map(x => x.Hits).Name("H");
            Map(x => x.EarnedRuns).Name("ER");
            Map(x => x.HomeRuns).Name("HR");
            Map(x => x.Walks).Name("BB");
            Map(x => x.Strikeouts).Name("K");
            Map(x => x.InningsPitched).Name("IP");
            Map(x => x.Era).Name("ERA");
            Map(x => x.TotalPitches).Name("TP");
            Map(x => x.Saves).Name("SV");
            Map(x => x.HitByPitch).Name("HBP");
            Map(x => x.BattersFaced).Name("Batters Faced");
            Map(x => x.GamesPlayed).Name("Games Played");
            Map(x => x.GamesStarted).Name("Games Started");
            Map(x => x.GamesFinished).Name("Games Finished");
            Map(x => x.RunsAllowed).Name("Runs Allowed");
            Map(x => x.WildPitches).Name("WP");
            Map(x => x.BattingAverageAgainst).Name("BAA");
            Map(x => x.Fip).Name("FIP");
            Map(x => x.Whip).Name("WHIP");
            Map(x => x.WinPercentage).Name("WPCT");
            Map(x => x.OpponentObp).Name("Opp OBP");
            Map(x => x.StrikeoutsPerWalk).Name("K/BB");
            Map(x => x.StrikeoutsPerNine).Name("K/9");
            Map(x => x.WalksPerNine).Name("BB/9");
            Map(x => x.HitsPerNine).Name("H/9");
            Map(x => x.HomeRunsPerNine).Name("HR/9");
            Map(x => x.PitchesPerInning).Name("Pitches Per Inning");
            Map(x => x.PitchesPerGame).Name("Pitches Per Game");
            Map(x => x.EraMinus).Name("ERA-");
            Map(x => x.FipMinus).Name("FIP-");
        }
    }
}