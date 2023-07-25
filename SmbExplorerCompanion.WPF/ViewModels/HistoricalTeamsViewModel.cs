using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using MediatR;
using SmbExplorerCompanion.Core.Commands.Queries.Teams;
using SmbExplorerCompanion.WPF.Extensions;
using SmbExplorerCompanion.WPF.Mappings.Teams;
using SmbExplorerCompanion.WPF.Models.Teams;

namespace SmbExplorerCompanion.WPF.ViewModels;

public class HistoricalTeamsViewModel : ViewModelBase
{
    public HistoricalTeamsViewModel(IMediator mediator)
    {
        var historicalTeamsResponse = mediator.Send(new GetHistoricalTeamsRequest()).Result;
        if (historicalTeamsResponse.TryPickT1(out var exception, out var historicalTeams))
        {
            MessageBox.Show(exception.Message);
            HistoricalTeams = new ObservableCollection<HistoricalTeam>();
        }
        else
        {
            var mapper = new HistoricalTeamMapping();
            HistoricalTeams = historicalTeams
                .Select(x => mapper.HistoricalTeamFromHistoricalTeamDto(x))
                .ToObservableCollection();
        }
    }

    public ObservableCollection<HistoricalTeam> HistoricalTeams { get; }
}