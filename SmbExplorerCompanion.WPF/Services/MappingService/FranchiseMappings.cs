using System.Linq;
using System.Threading.Tasks;
using SmbExplorerCompanion.Core.Entities.Summary;
using SmbExplorerCompanion.WPF.Extensions;
using SmbExplorerCompanion.WPF.Models.Summary;

// ReSharper disable once CheckNamespace
namespace SmbExplorerCompanion.WPF.Services;

public partial class MappingService
{
    public Task<FranchiseSummary> FromCore(FranchiseSummaryDto x)
    {
        return Task.FromResult(new FranchiseSummary
        {
            NumPlayers = x.NumPlayers,
            NumSeasons = x.NumSeasons,
            MostRecentSeasonNumber = x.MostRecentSeasonNumber,
            NumHallOfFamers = x.NumHallOfFamers,
            MostRecentChampionTeamId = x.MostRecentChampionTeamId,
            MostRecentChampionTeamName = x.MostRecentChampionTeamName,
            MostRecentMvpPlayerId = x.MostRecentMvpPlayerId,
            MostRecentMvpPlayerName = x.MostRecentMvpPlayerName,
            MostRecentCyYoungPlayerId = x.MostRecentCyYoungPlayerId,
            MostRecentCyYoungPlayerName = x.MostRecentCyYoungPlayerName,
            TopHomeRuns = x.TopHomeRuns.FromCore(),
            TopHits = x.TopHits.FromCore(),
            TopRunsBattedIn = x.TopRunsBattedIn.FromCore(),
            TopWins = x.TopWins.FromCore(),
            TopSaves = x.TopSaves.FromCore(),
            TopStrikeouts = x.TopStrikeouts.FromCore(),
            CurrentGreats = x.CurrentGreats
                .Select(async y => await FromCore(y))
                .Select(y => y.Result)
                .ToObservableCollection()
        });
    }
}