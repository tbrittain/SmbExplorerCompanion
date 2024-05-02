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
using SmbExplorerCompanion.Core.ValueObjects.Seasons;
using SmbExplorerCompanion.WPF.Extensions;
using SmbExplorerCompanion.WPF.Models.Seasons;
using SmbExplorerCompanion.WPF.Models.Teams;
using SmbExplorerCompanion.WPF.Services;

namespace SmbExplorerCompanion.WPF.ViewModels;

public class HistoricalTeamsViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly INavigationService _navigationService;
    private Season? _endSeason;
    private ObservableCollection<Season> _selectableEndSeasons;
    private HistoricalTeam? _selectedHistoricalTeam;
    private Season? _startSeason;

    public HistoricalTeamsViewModel(ISender mediator, INavigationService navigationService)
    {
        Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = Cursors.Wait);
        _navigationService = navigationService;
        _mediator = mediator;

        var seasons = _mediator.Send(new GetSeasonsRequest()).Result;

        Seasons.AddRange(seasons.Select(s => s.FromCore()));
        var minSeason = Seasons.OrderBy(x => x.Id).First();
        var maxSeason = Seasons.OrderByDescending(x => x.Id).First();
        StartSeason = minSeason;
        EndSeason = maxSeason;
        MinSeasonId = minSeason.Id;

        GetHistoricalTeams().Wait();

        PropertyChanged += OnPropertyChanged;
    }

    public ObservableCollection<Season> Seasons { get; } = new();

    public ObservableCollection<Season> SelectableEndSeasons
    {
        get => _selectableEndSeasons;
        private set => SetField(ref _selectableEndSeasons, value);
    }

    private int MinSeasonId { get; }

    public Season? StartSeason
    {
        get => _startSeason;
        set
        {
            SetField(ref _startSeason, value);

            if (value is not null)
            {
                var endSeasons = Seasons.Where(x => x.Id >= value.Id).ToList();
                SelectableEndSeasons = new ObservableCollection<Season>(endSeasons);
                EndSeason = endSeasons.LastOrDefault();
            }
            else
            {
                EndSeason = null;
            }
        }
    }

    public Season? EndSeason
    {
        get => _endSeason;
        set => SetField(ref _endSeason, value);
    }

    public ObservableCollection<HistoricalTeam> HistoricalTeams { get; } = new();

    public HistoricalTeam? SelectedHistoricalTeam
    {
        get => _selectedHistoricalTeam;
        set => SetField(ref _selectedHistoricalTeam, value);
    }

    private async Task GetHistoricalTeams()
    {
        if (StartSeason is not null && EndSeason is not null && StartSeason.Id > EndSeason.Id)
            return;

        Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = Cursors.Wait);
        HistoricalTeams.Clear();

        var seasonRange = (StartSeason, EndSeason) switch
        {
            (null, null) => new SeasonRange(MinSeasonId),
            (not null, null) => new SeasonRange(StartSeason.Id),
            (null, not null) => new SeasonRange(MinSeasonId, EndSeason.Id),
            (not null, not null) => new SeasonRange(StartSeason.Id, EndSeason.Id)
        };
        var historicalTeams =
            await _mediator.Send(new GetHistoricalTeamsRequest(seasonRange: seasonRange));

        HistoricalTeams.AddRange(historicalTeams
            .Select(x => x.FromCore())
            .OrderByDescending(x => x.NumRegularSeasonWins));

        Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);
    }

    private async void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(SelectedHistoricalTeam):
            {
                if (SelectedHistoricalTeam is not null)
                {
                    if (StartSeason is not null && SelectedHistoricalTeam.SeasonTeamId is not null)
                        NavigateToSeasonTeamDetail(SelectedHistoricalTeam);
                    else
                        NavigateToTeamOverview(SelectedHistoricalTeam);
                }

                break;
            }
            case nameof(StartSeason):
            case nameof(EndSeason):
            {
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