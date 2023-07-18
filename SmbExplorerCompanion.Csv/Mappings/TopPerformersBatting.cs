using CsvHelper.Configuration;

namespace SmbExplorerCompanion.Csv.Mappings;

public class TopPerformersBatting : TopPerformersBase
{
    public int GamesBatting { get; set; }
    public int GamesPlayed { get; set; }
    public int AtBats { get; set; }
    public int PlateAppearances { get; set; }
    public int Runs { get; set; }
    public int Hits { get; set; }
    public int Singles { get; set; }
    public int Doubles { get; set; }
    public int Triples { get; set; }
    public int HomeRuns { get; set; }
    public int RunsBattedIn { get; set; }
    public int ExtraBaseHits { get; set; }
    public int TotalBases { get; set; }
    public int StolenBases { get; set; }
    public int CaughtStealing { get; set; }
    public int Walks { get; set; }
    public int Strikeouts { get; set; }
    public int HitByPitch { get; set; }
    public double? Obp { get; set; }
    public double? Slg { get; set; }
    public double? Ops { get; set; }
    public double? Woba { get; set; }
    public double? Iso { get; set; }
    public double? Babip { get; set; }
    public int SacrificeHits { get; set; }
    public int SacrificeFlies { get; set; }
    public double? BattingAverage { get; set; }
    public int Errors { get; set; }
    public int PassedBalls { get; set; }
    public double? PaPerGame { get; set; }
    public double? AbPerHomeRun { get; set; }
    public double? StrikeoutPercentage { get; set; }
    public double? WalkPercentage { get; set; }
    public double? ExtraBaseHitPercentage { get; set; }
    public double? OpsPlus { get; set; }

    public sealed class TopPerformersBattingCsvMapping : ClassMap<TopPerformersBatting>
    {
        public TopPerformersBattingCsvMapping()
        {
            Map(x => x.PlayerId).Name("PlayerId");
            Map(x => x.SeasonId).Name("SeasonId");
            Map(x => x.SeasonNum).Name("Season");
            Map(x => x.FirstName).Name("First Name");
            Map(x => x.LastName).Name("Last Name");
            Map(x => x.GamesBatting).Name("Games Batting");
            Map(x => x.GamesPlayed).Name("Games Played");
            Map(x => x.AtBats).Name("AB");
            Map(x => x.PlateAppearances).Name("PA");
            Map(x => x.Runs).Name("R");
            Map(x => x.Hits).Name("H");
            Map(x => x.Singles).Name("1B");
            Map(x => x.Doubles).Name("2B");
            Map(x => x.Triples).Name("3B");
            Map(x => x.HomeRuns).Name("HR");
            Map(x => x.RunsBattedIn).Name("RBI");
            Map(x => x.ExtraBaseHits).Name("XBH");
            Map(x => x.TotalBases).Name("TB");
            Map(x => x.StolenBases).Name("SB");
            Map(x => x.CaughtStealing).Name("CS");
            Map(x => x.Walks).Name("BB");
            Map(x => x.Strikeouts).Name("K");
            Map(x => x.HitByPitch).Name("HBP");
            Map(x => x.Obp).Name("OBP");
            Map(x => x.Slg).Name("SLG");
            Map(x => x.Ops).Name("OPS");
            Map(x => x.Woba).Name("wOBA");
            Map(x => x.Iso).Name("ISO");
            Map(x => x.Babip).Name("BABIP");
            Map(x => x.SacrificeHits).Name("Sac Hits");
            Map(x => x.SacrificeFlies).Name("Sac Flies");
            Map(x => x.BattingAverage).Name("BA");
            Map(x => x.Errors).Name("Errors");
            Map(x => x.PassedBalls).Name("Passed Balls");
            Map(x => x.PaPerGame).Name("PA/Game");
            Map(x => x.AbPerHomeRun).Name("AB/HR");
            Map(x => x.StrikeoutPercentage).Name("K%");
            Map(x => x.WalkPercentage).Name("BB%");
            Map(x => x.ExtraBaseHitPercentage).Name("XBH%");
            Map(x => x.OpsPlus).Name("OPS+");
        }
    }
}