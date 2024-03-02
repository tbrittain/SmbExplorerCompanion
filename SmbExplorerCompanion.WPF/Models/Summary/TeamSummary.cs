using System.Text;
using System.Windows;
using SmbExplorerCompanion.Core.Entities.Summary;

namespace SmbExplorerCompanion.WPF.Models.Summary;

public class TeamSummary
{
    public int Id { get; set; }
    public int SeasonTeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int? PlayoffSeed { get; set; }
    public int? PlayoffWins { get; set; }
    public int? PlayoffLosses { get; set; }
    public bool IsDivisionChampion { get; set; }
    public bool IsConferenceChampion { get; set; }
    public bool IsChampion { get; set; }

    private string TeamRecord => $"{Wins}-{Losses}";

    public string RegularSeasonRecordText
    {
        get
        {
            var sb = new StringBuilder($"Regular Season {TeamRecord}");

            if (IsDivisionChampion)
            {
                sb.Append(": Division Champion");
            }

            return sb.ToString();
        }
    }

    private string PlayoffRecord => PlayoffWins.HasValue && PlayoffLosses.HasValue
        ? $"{PlayoffWins.Value}-{PlayoffLosses.Value}"
        : string.Empty;

    private string PlayoffSeedText => PlayoffSeed.HasValue ? $"#{PlayoffSeed.Value} seed" : string.Empty;

    public bool HasPlayoffResults => PlayoffWins.HasValue && PlayoffLosses.HasValue;

    public string PlayoffResultsText
    {
        get
        {
            if (!PlayoffWins.HasValue || !PlayoffLosses.HasValue)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            sb.Append($"Playoffs {PlayoffRecord}");
            sb.Append($" {PlayoffSeedText}");

            if (IsChampion)
            {
                sb.Append(": League Champion");
            }
            else if (IsConferenceChampion)
            {
                sb.Append(": Conference Champion");
            }

            return sb.ToString();
        }
    }
}

public static class TeamSummaryExtensions
{
    public static TeamSummary FromCore(this TeamSummaryDto x)
    {
        return new TeamSummary
        {
            Id = x.Id,
            SeasonTeamId = x.SeasonTeamId,
            TeamName = x.TeamName,
            Wins = x.Wins,
            Losses = x.Losses,
            PlayoffSeed = x.PlayoffSeed,
            PlayoffWins = x.PlayoffWins,
            PlayoffLosses = x.PlayoffLosses,
            IsDivisionChampion = x.IsDivisionChampion,
            IsConferenceChampion = x.IsConferenceChampion,
            IsChampion = x.IsChampion,
            
        };
    }
}