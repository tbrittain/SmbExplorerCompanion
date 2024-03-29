﻿namespace SmbExplorerCompanion.Core.Entities.Players;

public class PlayerPitchingSeasonDto : PlayerSeasonDto
{
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int EarnedRuns { get; set; }
    public int TotalPitches { get; set; }
    public double EarnedRunAverage { get; set; }
    public double Fip { get; set; }
    public int GamesStarted { get; set; }
    public int Saves { get; set; }
    public double InningsPitched { get; set; }
    public int Strikeouts { get; set; }
    public int Walks { get; set; }
    public int Hits { get; set; }
    public int HomeRuns { get; set; }
    public int HitByPitch { get; set; }
    public double Whip { get; set; }
    public double EraMinus { get; set; }
    public double FipMinus { get; set; }
    public int CompleteGames { get; set; }
    public int Shutouts { get; set; }
    public List<int> AwardIds { get; set; } = new();
    public bool IsChampion { get; set; }
    public double WinPercentage => Wins + Losses > 0 ? (double) Wins / (Wins + Losses) : 0;
    public double Era { get; set; }
    public int GamesFinished { get; set; }
    public int BattersFaced { get; set; }
    public double HitsPerNine { get; set; }
    public double HomeRunsPerNine { get; set; }
    public double WalksPerNine { get; set; }
    public double StrikeoutsPerNine { get; set; }
    public double StrikeoutToWalkRatio { get; set; }
    public List<int> TraitIds { get; set; } = new();
    public List<int> PitchTypeIds { get; set; } = new();
}