namespace SmbExplorerCompanion.Core.Entities.Players;

public class PlayerCareerPitchingDto : PlayerCareerBaseDto
{
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int EarnedRuns { get; set; }
    public int TotalPitches { get; set; }
    public double EarnedRunAverage { get; set; }
    public double Fip { get; set; }
    public int GamesPlayed { get; set; }
    public int GamesStarted { get; set; }
    public int Saves { get; set; }
    public double InningsPitched { get; set; }
    public int Strikeouts { get; set; }
    public double Whip { get; set; }
    public double EraMinus { get; set; }
    public double FipMinus { get; set; }
    public int CompleteGames { get; set; }
    public int Shutouts { get; set; }
    public int Walks { get; set; }
    public int Hits { get; set; }
    public int HomeRuns { get; set; }
    public int HitByPitch { get; set; }
}