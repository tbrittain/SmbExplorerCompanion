using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MediatR;
using SmbExplorerCompanion.Core.Commands.Queries.Seasons;
using SmbExplorerCompanion.Core.Commands.Queries.Teams;
using SmbExplorerCompanion.WPF.Extensions;
using SmbExplorerCompanion.WPF.Models.Seasons;
using SmbExplorerCompanion.WPF.Models.Teams;
using SmbExplorerCompanion.WPF.Services;

namespace SmbExplorerCompanion.WPF.ViewModels;

public class HistoricalTeamsViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly INavigationService _navigationService;
    private HistoricalTeam? _selectedHistoricalTeam;
    private Season? _selectedSeason;

    public HistoricalTeamsViewModel(ISender mediator, INavigationService navigationService)
    {
        Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = Cursors.Wait);
        _navigationService = navigationService;
        _mediator = mediator;

        var seasonsResponse = _mediator.Send(new GetSeasonsRequest()).Result;

        if (seasonsResponse.TryPickT1(out var exception, out var seasons))
        {
            MessageBox.Show(exception.Message);
            Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);
            return;
        }

        var allSeasons = new Season
        {
            Id = default
        };
        Seasons.AddRange(seasons.Select(s => s.FromCore()));
        Seasons.Add(allSeasons);
        SelectedSeason = allSeasons;

        GetHistoricalTeams().Wait();

        PropertyChanged += OnPropertyChanged;
    }

    public ObservableCollection<Season> Seasons { get; } = new();

    public Season? SelectedSeason
    {
        get => _selectedSeason;
        set => SetField(ref _selectedSeason, value);
    }

    private async Task GetHistoricalTeams()
    {
        if (SelectedSeason is null)
        {
            MessageBox.Show("Please select a season.");
            Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);
            return;
        }

        HistoricalTeams.Clear();

        Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = Cursors.Wait);
        var historicalTeamsResponse =
            await _mediator.Send(new GetHistoricalTeamsRequest(SelectedSeason!.Id == default ? null : SelectedSeason!.Id));
        if (historicalTeamsResponse.TryPickT1(out var exception, out var historicalTeams))
        {
            MessageBox.Show(exception.Message);
            Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);
            return;
        }

        HistoricalTeams.AddRange(historicalTeams
            .Select(x => x.FromCore())
            .OrderByDescending(x => x.NumRegularSeasonWins));

        Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);
    }

    public ObservableCollection<HistoricalTeam> HistoricalTeams { get; } = new();

    public HistoricalTeam? SelectedHistoricalTeam
    {
        get => _selectedHistoricalTeam;
        set => SetField(ref _selectedHistoricalTeam, value);
    }

    private async void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(SelectedHistoricalTeam):
            {
                if (SelectedHistoricalTeam is not null)
                {
                    if (SelectedSeason is not null && SelectedHistoricalTeam.SeasonTeamId is not null)
                        NavigateToSeasonTeamDetail(SelectedHistoricalTeam);
                    else
                        NavigateToTeamOverview(SelectedHistoricalTeam);
                }

                break;
            }
            case nameof(SelectedSeason):
            {
                if (SelectedSeason is not null)
                    await GetHistoricalTeams();
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

    private void NavigateToSeasonTeamDetail(HistoricalTeam team)
    {
        if (team.SeasonTeamId is null)
            throw new ArgumentException("SeasonTeamId cannot be null.");
        var parameters = new Tuple<string, object>[]
        {
            new(TeamSeasonDetailViewModel.SeasonTeamIdProp, team.SeasonTeamId)
        };
        _navigationService.NavigateTo<TeamSeasonDetailViewModel>(parameters);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) PropertyChanged -= OnPropertyChanged;

        base.Dispose(disposing);
    }
}