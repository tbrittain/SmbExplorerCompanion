using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using MediatR;
using SmbExplorerCompanion.Core.Commands.Queries.Teams;
using SmbExplorerCompanion.WPF.Extensions;
using SmbExplorerCompanion.WPF.Mappings.Teams;
using SmbExplorerCompanion.WPF.Models.Teams;
using SmbExplorerCompanion.WPF.Services;

namespace SmbExplorerCompanion.WPF.ViewModels;

public class HistoricalTeamsViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private HistoricalTeam? _selectedHistoricalTeam;

    public HistoricalTeamsViewModel(ISender mediator, INavigationService navigationService)
    {
        Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = Cursors.Wait);
        _navigationService = navigationService;
        var historicalTeamsResponse = mediator.Send(new GetHistoricalTeamsRequest()).Result;
        if (historicalTeamsResponse.TryPickT1(out var exception, out var historicalTeams))
        {
            MessageBox.Show(exception.Message);
            Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);
            HistoricalTeams = new ObservableCollection<HistoricalTeam>();
        }
        else
        {
            var mapper = new HistoricalTeamMapping();
            HistoricalTeams = historicalTeams
                .Select(x => mapper.FromDto(x))
                .ToObservableCollection();
        }

        PropertyChanged += OnPropertyChanged;
        
        Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);
    }

    public ObservableCollection<HistoricalTeam> HistoricalTeams { get; }

    public HistoricalTeam? SelectedHistoricalTeam
    {
        get => _selectedHistoricalTeam;
        set => SetField(ref _selectedHistoricalTeam, value);
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

    private void NavigateToTeamOverview(HistoricalTeam team)
    {
        var parameters = new Tuple<string, object>[]
        {
            new(TeamOverviewViewModel.TeamIdProp, team.TeamId)
        };
        _navigationService.NavigateTo<TeamOverviewViewModel>(parameters);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) PropertyChanged -= OnPropertyChanged;

        base.Dispose(disposing);
    }
}