using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using SmbExplorerCompanion.Core.Commands.Queries.Players;
using SmbExplorerCompanion.Core.Commands.Queries.Seasons;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.Core.ValueObjects.Seasons;
using SmbExplorerCompanion.WPF.Extensions;
using SmbExplorerCompanion.WPF.Models.Lookups;
using SmbExplorerCompanion.WPF.Models.Players;
using SmbExplorerCompanion.WPF.Models.Seasons;
using SmbExplorerCompanion.WPF.Services;

namespace SmbExplorerCompanion.WPF.ViewModels;

public partial class TopPitchingSeasonsViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private readonly INavigationService _navigationService;
    private bool _isPlayoffs;
    private int _pageNumber = 1;
    private PlayerSeasonBase? _selectedPlayer;
    private Season? _startSeason;
    private bool _onlyRookies;
    private PitcherRole? _selectedPitcherRole;
    private readonly MappingService _mappingService;
    private ObservableCollection<Season> _selectableEndSeasons;
    private Season? _endSeason;

    public TopPitchingSeasonsViewModel(IMediator mediator,
        INavigationService navigationService,
        MappingService mappingService,
        LookupCache lookupCache)
    {
        Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = Cursors.Wait);
        _mediator = mediator;
        _navigationService = navigationService;
        _mappingService = mappingService;

        var seasons = _mediator.Send(new GetSeasonsRequest()).Result;
        Seasons.AddRange(seasons.Select(s => s.FromCore()));
        StartSeason = Seasons.OrderByDescending(x => x.Number).First();
        MinSeasonId = Seasons.OrderBy(x => x.Number).First().Id;

        var pitcherRoles = lookupCache.GetPitcherRoles().Result;

        var allPitcherRole = new PitcherRole
        {
            Id = 0,
            Name = "All"
        };
        PitcherRoles.Add(allPitcherRole);
        PitcherRoles.AddRange(pitcherRoles);
        SelectedPitcherRole = allPitcherRole;

        GetTopPitchingSeason().Wait();
        PropertyChanged += OnPropertyChanged;
        Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);
    }

    public ObservableCollection<Season> Seasons { get; } = new();

    public ObservableCollection<PitcherRole> PitcherRoles { get; } = new();

    public PitcherRole? SelectedPitcherRole
    {
        get => _selectedPitcherRole;
        set => SetField(ref _selectedPitcherRole, value);
    }

    private int MinSeasonId { get; }

    public Season? StartSeason
    {
        get => _startSeason;
        set
        {
            if (value is not null && (value.Id == default || value.Id == MinSeasonId))
            {
                ShortCircuitOnlyRookiesRefresh = true;
                OnlyRookies = false;
                ShortCircuitOnlyRookiesRefresh = false;
            }

            SetField(ref _startSeason, value);
            OnPropertyChanged(nameof(CanSelectOnlyRookies));
    
            if (value is not null)
            {
                var endSeasons = Seasons.Where(x => x.Id >= value.Id).ToList();
                SelectableEndSeasons = new ObservableCollection<Season>(endSeasons);
                if (EndSeason is null || EndSeason.Id < value.Id)
                    EndSeason = endSeasons.LastOrDefault();
            }
            else
            {
                EndSeason = null;
            }
        }
    }

    public ObservableCollection<Season> SelectableEndSeasons
    {
        get => _selectableEndSeasons;
        private set => SetField(ref _selectableEndSeasons, value);
    }

    [RelayCommand]
    private void ClearSeasons()
    {
        StartSeason = Seasons.OrderBy(x => x.Id).Last();
        EndSeason = Seasons.OrderBy(x => x.Id).Last();
    }

    public Season? EndSeason
    {
        get => _endSeason;
        set
        {
            if (value is not null && value.Id != StartSeason?.Id)
            {
                ShortCircuitOnlyRookiesRefresh = true;
                OnlyRookies = false;
                ShortCircuitOnlyRookiesRefresh = false;
            }

            SetField(ref _endSeason, value);
            OnPropertyChanged(nameof(CanSelectOnlyRookies));
        }
    }

    public int PageNumber
    {
        get => _pageNumber;
        set
        {
            if (value < 1) return;
            SetField(ref _pageNumber, value);

            IncrementPageCommand.NotifyCanExecuteChanged();
            DecrementPageCommand.NotifyCanExecuteChanged();
        }
    }

    public bool IsPlayoffs
    {
        get => _isPlayoffs;
        set => SetField(ref _isPlayoffs, value);
    }

    public bool CanSelectOnlyRookies
    {
        get
        {
            if (StartSeason is null) return false;
            if (StartSeason.Id == default) return false;
            if (StartSeason.Id == MinSeasonId) return false;

            return true;
        }
    }

    public bool OnlyRookies
    {
        get => _onlyRookies;
        set => SetField(ref _onlyRookies, value);
    }

    public string SortColumn { get; set; } = nameof(PlayerPitchingSeasonDto.WeightedOpsPlusOrEraMinus);

    public ObservableCollection<PlayerSeasonPitching> TopSeasonPitchers { get; } = new();

    public PlayerSeasonBase? SelectedPlayer
    {
        get => _selectedPlayer;
        set => SetField(ref _selectedPlayer, value);
    }

    public bool ShortCircuitPageNumberRefresh { get; set; }
    private bool ShortCircuitOnlyRookiesRefresh { get; set; }

    private const int ResultsPerPage = 20;

    private bool CanSelectPreviousPage => PageNumber > 1;
    private bool CanSelectNextPage => TopSeasonPitchers.Count == ResultsPerPage;

    private async void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(OnlyRookies):
            case nameof(IsPlayoffs):
            case nameof(StartSeason):
            case nameof(EndSeason):
            case nameof(SelectedPitcherRole):
            {
                ShortCircuitPageNumberRefresh = true;
                PageNumber = 1;
                ShortCircuitPageNumberRefresh = false;

                if (!ShortCircuitOnlyRookiesRefresh) await GetTopPitchingSeason();
                break;
            }
            case nameof(SelectedPlayer):
            {
                if (SelectedPlayer is not null)
                    NavigateToPlayerOverview(SelectedPlayer);
                break;
            }
            case nameof(PageNumber):
            {
                if (!ShortCircuitPageNumberRefresh) await GetTopPitchingSeason();

                break;
            }
        }
    }

    [RelayCommand(CanExecute = nameof(CanSelectNextPage))]
    private void IncrementPage()
    {
        PageNumber++;
    }

    [RelayCommand(CanExecute = nameof(CanSelectPreviousPage))]
    private void DecrementPage()
    {
        PageNumber--;
    }

    public async Task GetTopPitchingSeason()
    {
        Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = Cursors.Wait);
        var seasonRange = (StartSeason, EndSeason) switch
        {
            (null, null) => new SeasonRange(MinSeasonId),
            (not null, null) => new SeasonRange(StartSeason.Id),
            (null, not null) => new SeasonRange(MinSeasonId, EndSeason.Id),
            (not null, not null) => new SeasonRange(StartSeason.Id, EndSeason.Id),
        };

        var topPitchers = await _mediator.Send(new GetTopPitchingSeasonRequest(
            seasons: seasonRange,
            isPlayoffs: IsPlayoffs,
            pageNumber: PageNumber,
            orderBy: SortColumn,
            limit: ResultsPerPage,
            onlyRookies: OnlyRookies,
            pitcherRoleId: SelectedPitcherRole?.Id == 0 ? null : SelectedPitcherRole?.Id,
            descending: true));

        TopSeasonPitchers.Clear();
        var mappedTopSeasonPitchers = topPitchers
            .Select(async x => await _mappingService.FromCore(x))
            .Select(x => x.Result);
        TopSeasonPitchers.AddRange(mappedTopSeasonPitchers);

        IncrementPageCommand.NotifyCanExecuteChanged();
        DecrementPageCommand.NotifyCanExecuteChanged();
        Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);
    }

    private void NavigateToPlayerOverview(PlayerSeasonBase player)
    {
        var parameters = new Tuple<string, object>[]
        {
            new(PlayerOverviewViewModel.PlayerIdProp, player.PlayerId),
            new(PlayerOverviewViewModel.SeasonIdProp, player.SeasonId)
        };
        _navigationService.NavigateTo<PlayerOverviewViewModel>(parameters);
    }

    protected override void Dispose(bool disposing)
    {
        PropertyChanged -= OnPropertyChanged;
        base.Dispose(disposing);
    }
}