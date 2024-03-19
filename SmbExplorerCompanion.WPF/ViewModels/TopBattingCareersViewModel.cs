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

        PropertyChanged += OnPropertyChanged;

        var positions = lookupCache.GetPositions().Result;
        var allPosition = new Position
        {
            Id = 0,
            Name = "All"
        };
        Positions.Add(allPosition);
        Positions.AddRange(positions.Where(x => x.IsPrimaryPosition));
        SelectedPosition = allPosition;
        
        var seasons = _mediator.Send(new GetSeasonsRequest()).Result;
        Seasons.AddRange(seasons.Select(s => s.FromCore()));

        GetTopBattingCareers().Wait();
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
        Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = Cursors.Wait);
        SeasonRange? seasonRange = (StartSeason, EndSeason) switch
        {
            (not null, not null) => new SeasonRange(StartSeason.Id, EndSeason.Id),
            _ => null
        };

        var topPlayers = await _mediator.Send(new GetTopBattingCareersRequest(
            pageNumber: PageNumber,
            limit: ResultsPerPage,
            orderBy: SortColumn,
            onlyHallOfFamers: OnlyHallOfFamers,
            primaryPositionId: SelectedPosition?.Id == 0 ? null : SelectedPosition?.Id,
            seasonRange: seasonRange
        ));

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