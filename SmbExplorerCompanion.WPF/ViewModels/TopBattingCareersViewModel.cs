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
using SmbExplorerCompanion.Core.Interfaces;
using SmbExplorerCompanion.Core.ValueObjects.Seasons;
using SmbExplorerCompanion.WPF.Extensions;
using SmbExplorerCompanion.WPF.Models.Lookups;
using SmbExplorerCompanion.WPF.Models.Players;
using SmbExplorerCompanion.WPF.Models.Seasons;
using SmbExplorerCompanion.WPF.Services;

namespace SmbExplorerCompanion.WPF.ViewModels;

public partial class TopBattingCareersViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private readonly INavigationService _navigationService;
    private bool _onlyHallOfFamers;
    private int _pageNumber = 1;
    private PlayerBattingCareer? _selectedPlayer;
    private Position? _selectedPosition;
    private readonly MappingService _mappingService;
    private Season? _startSeason;
    private ObservableCollection<Season> _selectableEndSeasons;
    private Season? _endSeason;
    private Chemistry? _selectedChemistry;
    private BatHandedness? _selectedBatHandedness;
    private bool _isPlayoffs;

    public TopBattingCareersViewModel(
        INavigationService navigationService,
        IMediator mediator,
        MappingService mappingService,
        LookupCache lookupCache)
    {
        Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = Cursors.Wait);
        _navigationService = navigationService;
        _mediator = mediator;
        _mappingService = mappingService;

        var seasons = _mediator.Send(new GetSeasonsRequest()).Result;
        Seasons.AddRange(seasons.Select(s => s.FromCore()));

        var positions = lookupCache.GetPositions().Result;
        var allPosition = new Position
        {
            Id = default,
            Name = "All"
        };
        Positions.Add(allPosition);
        Positions.AddRange(positions.Where(x => x.IsPrimaryPosition));
        SelectedPosition = allPosition;

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

        GetTopBattingCareers().Wait();

        PropertyChanged += OnPropertyChanged;
    }

    public bool IsPlayoffs
    {
        get => _isPlayoffs;
        set => SetField(ref _isPlayoffs, value);
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

    public int PageNumber
    {
        get => _pageNumber;
        private set
        {
            if (value < 1) return;
            SetField(ref _pageNumber, value);

            IncrementPageCommand.NotifyCanExecuteChanged();
            DecrementPageCommand.NotifyCanExecuteChanged();
        }
    }

    public bool OnlyHallOfFamers
    {
        get => _onlyHallOfFamers;
        set => SetField(ref _onlyHallOfFamers, value);
    }

    public string SortColumn { get; set; } = nameof(PlayerCareerBattingDto.WeightedOpsPlusOrEraMinus);

    public PlayerBattingCareer? SelectedPlayer
    {
        get => _selectedPlayer;
        set => SetField(ref _selectedPlayer, value);
    }

    public ObservableCollection<PlayerBattingCareer> TopBattingCareers { get; } = new();
    public ObservableCollection<Position> Positions { get; } = new();

    public Position? SelectedPosition
    {
        get => _selectedPosition;
        set => SetField(ref _selectedPosition, value);
    }

    private async void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(SelectedPlayer):
            {
                if (SelectedPlayer is not null)
                    NavigateToPlayerOverview(SelectedPlayer);
                break;
            }
            case nameof(StartSeason):
            case nameof(EndSeason):
            case nameof(SelectedPosition):
            case nameof(OnlyHallOfFamers):
            case nameof(SelectedBatHandedness):
            case nameof(SelectedChemistry):
            case nameof(IsPlayoffs):
            {
                ShortCircuitPageNumberRefresh = true;
                PageNumber = 1;
                ShortCircuitPageNumberRefresh = false;
                await GetTopBattingCareers();
                break;
            }
            case nameof(PageNumber):
            {
                if (!ShortCircuitPageNumberRefresh) await GetTopBattingCareers();
                break;
            }
        }
    }

    private bool ShortCircuitPageNumberRefresh { get; set; }

    private const int ResultsPerPage = 20;
    private bool CanSelectPreviousPage => PageNumber > 1;

    private bool CanSelectNextPage => TopBattingCareers.Count == ResultsPerPage;

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

    [RelayCommand]
    private void ClearSeasons()
    {
        StartSeason = null;
        EndSeason = null;
    }

    private void NavigateToPlayerOverview(PlayerBase player)
    {
        var parameters = new Tuple<string, object>[]
        {
            new(PlayerOverviewViewModel.PlayerIdProp, player.PlayerId)
        };
        _navigationService.NavigateTo<PlayerOverviewViewModel>(parameters);
    }

    public async Task GetTopBattingCareers()
    {
        if (StartSeason is not null && EndSeason is not null && StartSeason.Id > EndSeason.Id)
            return;

        Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = Cursors.Wait);
        SeasonRange? seasonRange = (StartSeason, EndSeason) switch
        {
            (not null, not null) => new SeasonRange(StartSeason.Id, EndSeason.Id),
            _ => null
        };

        var topPlayers = await _mediator.Send(new GetTopBattingCareersRequest(
            new GetBattingCareersFilters
            {
                PageNumber = PageNumber,
                Limit = ResultsPerPage,
                OrderBy = SortColumn,
                OnlyHallOfFamers = OnlyHallOfFamers,
                PrimaryPositionId = SelectedPosition?.Id == 0 ? null : SelectedPosition?.Id,
                Seasons = seasonRange,
                ChemistryId = SelectedChemistry?.Id == 0 ? null : SelectedChemistry?.Id,
                BatHandednessId = SelectedBatHandedness?.Id == 0 ? null : SelectedBatHandedness?.Id,
                IsPlayoffs = IsPlayoffs
            }));

        TopBattingCareers.Clear();
        var topBattingCareers = topPlayers
            .Select(async x => await _mappingService.FromCore(x))
            .Select(x => x.Result);
        TopBattingCareers.AddRange(topBattingCareers);

        IncrementPageCommand.NotifyCanExecuteChanged();
        DecrementPageCommand.NotifyCanExecuteChanged();
        Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);
    }

    public ObservableCollection<Season> Seasons { get; } = new();

    public ObservableCollection<Season> SelectableEndSeasons
    {
        get => _selectableEndSeasons;
        private set => SetField(ref _selectableEndSeasons, value);
    }

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

    protected override void Dispose(bool disposing)
    {
        PropertyChanged -= OnPropertyChanged;
        base.Dispose(disposing);
    }
}