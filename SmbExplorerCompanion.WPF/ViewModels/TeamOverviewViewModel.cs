using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using MediatR;
using SmbExplorerCompanion.Core.Commands.Queries.Teams;
using SmbExplorerCompanion.WPF.Models.Teams;
using SmbExplorerCompanion.WPF.Services;

namespace SmbExplorerCompanion.WPF.ViewModels;

public class TeamOverviewViewModel : ViewModelBase
{
    public const string TeamIdProp = "TeamId";
    private readonly INavigationService _navigationService;
    private TeamTopPlayerHistory? _selectedPlayer;
    private TeamOverviewSeasonHistory? _selectedTeamSeason;

    public TeamOverviewViewModel(INavigationService navigationService, ISender mediator)
    {
        Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = Cursors.Wait);
        _navigationService = navigationService;

        var ok = _navigationService.TryGetParameter<int>(TeamIdProp, out var teamId);
        if (!ok)
        {
            const string message = "Could not get team id from navigation parameters.";
            MessageBox.Show(message);
            throw new Exception(message);
        }

        TeamId = teamId;
        _navigationService.ClearParameters();

        var teamOverview = mediator.Send(new GetTeamOverviewRequest(TeamId)).Result;
        TeamOverview = teamOverview.FromCore();

        PropertyChanged += OnPropertyChanged;

        Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);
    }

    private int TeamId { get; }

    public TeamOverview TeamOverview { get; set; }

    public TeamTopPlayerHistory? SelectedPlayer
    {
        get => _selectedPlayer;
        set => SetField(ref _selectedPlayer, value);
    }

    public TeamOverviewSeasonHistory? SelectedTeamSeason
    {
        get => _selectedTeamSeason;
        set => SetField(ref _selectedTeamSeason, value);
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(SelectedPlayer):
            {
                if (SelectedPlayer is not null)
                    NavigateToPlayerOverview(SelectedPlayer);
                break;
            }
            case nameof(SelectedTeamSeason):
            {
                if (SelectedTeamSeason is not null)
                    NavigateToTeamSeasonDetail(SelectedTeamSeason);
                break;
            }
        }
    }

    private void NavigateToPlayerOverview(TeamTopPlayerHistory player)
    {
        var parameters = new Tuple<string, object>[]
        {
            new(PlayerOverviewViewModel.PlayerIdProp, player.PlayerId)
        };
        _navigationService.NavigateTo<PlayerOverviewViewModel>(parameters);
    }

    private void NavigateToTeamSeasonDetail(TeamOverviewSeasonHistory teamSeason)
    {
        var parameters = new Tuple<string, object>[]
        {
            new(TeamSeasonDetailViewModel.SeasonTeamIdProp, teamSeason.SeasonTeamId)
        };
        _navigationService.NavigateTo<TeamSeasonDetailViewModel>(parameters);
    }

    protected override void Dispose(bool disposing)
    {
        PropertyChanged -= OnPropertyChanged;
        base.Dispose(disposing);
    }
}