using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
using SmbExplorerCompanion.Core.Interfaces;
using SmbExplorerCompanion.Core.ValueObjects.Seasons;
using SmbExplorerCompanion.WPF.Extensions;
using SmbExplorerCompanion.WPF.Models.Lookups;
using SmbExplorerCompanion.WPF.Models.Players;
using SmbExplorerCompanion.WPF.Models.Seasons;
using SmbExplorerCompanion.WPF.Services;

namespace SmbExplorerCompanion.WPF.ViewModels;

public partial class TopBattingSeasonsViewModel : ViewModelBase
{
    private const int ResultsPerPage = 20;
    private readonly MappingService _mappingService;
    private readonly IMediator _mediator;
    private readonly INavigationService _navigationService;
    private Season? _endSeason;
    private bool _isPlayoffs;
    private bool _onlyRookies;
    private int _pageNumber = 1;
    private ObservableCollection<Season> _selectableEndSeasons;
    private BatHandedness? _selectedBatHandedness;
    private Chemistry? _selectedChemistry;
    private PlayerSeasonBase? _selectedPlayer;
    private Position? _selectedPosition;
    private Position? _selectedSecondaryPosition;
    private ObservableCollection<Trait> _selectedTraits;
    private Season? _startSeason;

    public TopBattingSeasonsViewModel(IMediator mediator,
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

        var positions = lookupCache.GetPositions().Result;
        var allPosition = new Position
        {
            Id = default,
            Name = "All"
        };
        Positions.Add(allPosition);
        Positions.AddRange(positions.Where(x => x.IsPrimaryPosition));

        SecondaryPositions.Add(allPosition);
        SecondaryPositions.AddRange(positions);
        SelectedPosition = allPosition;
        SelectedSecondaryPosition = allPosition;

        var chemistryTypes = lookupCache.GetChemistryTypes().Result;
        var allChemistry = new Chemistry
        {
            Id = default,
            Name = "All"
        };
        ChemistryTypes = chemistryTypes
            .Prepend(allChemistry)
            .ToObservableCollection();
        SelectedChemistry = allChemistry;

        var batHandednessTypes = lookupCache.GetBatHandednessTypes().Result;
        var allBatHandedness = new BatHandedness
        {
            Id = default,
            Name = "All"
        };
        BatHandednessTypes = batHandednessTypes
            .Prepend(allBatHandedness)
            .ToObservableCollection();
        SelectedBatHandedness = allBatHandedness;

        var traits = lookupCache.GetTraits().Result;
        Traits = traits
            .Where(x => !x.IsSmb3)
            .ToObservableCollection();
        SelectedTraits = new ObservableCollection<Trait>();

        GetTopBattingSeason().Wait();
        PropertyChanged += OnPropertyChanged;
        SelectedTraits.CollectionChanged += SelectedTraitsOnCollectionChanged;
        Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);
    }

    public ObservableCollection<Chemistry> ChemistryTypes { get; }

    public Chemistry? SelectedChemistry
    {
        get => _selectedChemistry;
        set => SetField(ref _selectedChemistry, value);
    }

    public ObservableCollection<BatHandedness> BatHandednessTypes { get; }

    public BatHandedness? SelectedBatHandedness
    {
        get => _selectedBatHandedness;
        set => SetField(ref _selectedBatHandedness, value);
    }

    public ObservableCollection<Trait> Traits { get; }

    public ObservableCollection<Trait> SelectedTraits
    {
        get => _selectedTraits;
        set => SetField(ref _selectedTraits, value);
    }

    public ObservableCollection<Position> SecondaryPositions { get; } = new();

    public Position? SelectedSecondaryPosition
    {
        get => _selectedSecondaryPosition;
        set => SetField(ref _selectedSecondaryPosition, value);
    }

    public PlayerSeasonBase? SelectedPlayer
    {
        get => _selectedPlayer;
        set => SetField(ref _selectedPlayer, value);
    }

    public bool ShortCircuitPageNumberRefresh { get; set; }
    private bool ShortCircuitOnlyRookiesRefresh { get; set; }
    private bool CanSelectPreviousPage => PageNumber > 1;

    private bool CanSelectNextPage => TopSeasonBatters.Count == ResultsPerPage;

    public ObservableCollection<Season> Seasons { get; } = new();

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

    public string SortColumn { get; set; } = nameof(PlayerBattingSeasonDto.WeightedOpsPlusOrEraMinus);

    public ObservableCollection<PlayerSeasonBatting> TopSeasonBatters { get; } = new();

    public ObservableCollection<Position> Positions { get; } = new();

    public Position? SelectedPosition
    {
        get => _selectedPosition;
        set => SetField(ref _selectedPosition, value);
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
            if (StartSeason.Id != EndSeason?.Id) return false;

            return true;
        }
    }

    public bool OnlyRookies
    {
        get => _onlyRookies;
        set => SetField(ref _onlyRookies, value);
    }

    private void SelectedTraitsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(this, new PropertyChangedEventArgs(nameof(SelectedTraits)));
    }

    [RelayCommand]
    private void ClearSeasons()
    {
        StartSeason = Seasons.OrderBy(x => x.Id).Last();
        EndSeason = Seasons.OrderBy(x => x.Id).Last();
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

    private async void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(OnlyRookies):
            case nameof(IsPlayoffs):
            case nameof(StartSeason):
            case nameof(EndSeason):
            case nameof(SelectedPosition):
            case nameof(SelectedChemistry):
            case nameof(SelectedBatHandedness):
            case nameof(SelectedTraits):
            case nameof(SelectedSecondaryPosition):
            {
                ShortCircuitPageNumberRefresh = true;
                PageNumber = 1;
                ShortCircuitPageNumberRefresh = false;

                if (!ShortCircuitOnlyRookiesRefresh) await GetTopBattingSeason();
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
                if (!ShortCircuitPageNumberRefresh) await GetTopBattingSeason();

                break;
            }
        }
    }

    public async Task GetTopBattingSeason()
    {
        if (StartSeason is not null && EndSeason is not null && StartSeason.Id > EndSeason.Id)
            return;

        Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = Cursors.Wait);
        var seasonRange = (StartSeason, EndSeason) switch
        {
            (null, null) => new SeasonRange(MinSeasonId),
            (not null, null) => new SeasonRange(StartSeason.Id),
            (null, not null) => new SeasonRange(MinSeasonId, EndSeason.Id),
            (not null, not null) => new SeasonRange(StartSeason.Id, EndSeason.Id)
        };

        var topBatters = await _mediator.Send(new GetTopBattingSeasonRequest(
            new GetBattingSeasonsFilters
            {
                Seasons = seasonRange,
                IsPlayoffs = IsPlayoffs,
                PageNumber = PageNumber,
                OrderBy = SortColumn,
                OnlyRookies = OnlyRookies,
                Limit = ResultsPerPage,
                PrimaryPositionId = SelectedPosition?.Id == 0 ? null : SelectedPosition?.Id,
                Descending = true,
                ChemistryId = SelectedChemistry?.Id == 0 ? null : SelectedChemistry?.Id,
                BatHandednessId = SelectedBatHandedness?.Id == 0 ? null : SelectedBatHandedness?.Id,
                TraitIds = SelectedTraits.Select(x => x.Id).ToList(),
                SecondaryPositionId = SelectedSecondaryPosition?.Id == 0 ? null : SelectedSecondaryPosition?.Id
            }));

        TopSeasonBatters.Clear();
        var topSeasonBatters = topBatters
            .Select(async x => await _mappingService.FromCore(x))
            .Select(x => x.Result);
        TopSeasonBatters.AddRange(topSeasonBatters);

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