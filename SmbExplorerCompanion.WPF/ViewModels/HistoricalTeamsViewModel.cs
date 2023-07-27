using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MediatR;
using SmbExplorerCompanion.Core.Commands.Queries.Teams;
using SmbExplorerCompanion.WPF.Extensions;
using SmbExplorerCompanion.WPF.Mappings.Teams;
using SmbExplorerCompanion.WPF.Models.Teams;
using SmbExplorerCompanion.WPF.Services;

namespace SmbExplorerCompanion.WPF.ViewModels;

public class HistoricalTeamsViewModel : ViewModelBase
{
    private HistoricalTeam? _selectedHistoricalTeam;
    private readonly INavigationService _navigationService;

    public HistoricalTeamsViewModel(ISender mediator, INavigationService navigationService)
    {
        _navigationService = navigationService;
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
                .Select(x => mapper.FromDto(x))
                .ToObservableCollection();
        }
        
        this.PropertyChanged += OnPropertyChanged;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(SelectedHistoricalTeam):
            {
                if (SelectedHistoricalTeam is not null)
                    NavigateToTeamOverview(SelectedHistoricalTeam);
                break;
            }
        }
    }

    public ObservableCollection<HistoricalTeam> HistoricalTeams { get; }

    public HistoricalTeam? SelectedHistoricalTeam
    {
        get => _selectedHistoricalTeam;
        set => SetField(ref _selectedHistoricalTeam, value);
    }

    private void NavigateToTeamOverview(HistoricalTeam team)
    {
        var parameters = new Tuple<string, object>[]
        {
            new(TeamOverviewViewModel.TeamIdProp, team.Id)
        };
        _navigationService.NavigateTo<TeamOverviewViewModel>(parameters);
    }
}