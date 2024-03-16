using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using SmbExplorerCompanion.Core.Commands.Queries.Lookups;
using SmbExplorerCompanion.Core.Commands.Queries.Players;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.WPF.Extensions;
using SmbExplorerCompanion.WPF.Models.Lookups;
using SmbExplorerCompanion.WPF.Models.Players;
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

    public TopBattingCareersViewModel(INavigationService navigationService, IMediator mediator, MappingService mappingService)
    {
        Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = Cursors.Wait);
        _navigationService = navigationService;
        _mediator = mediator;
        _mappingService = mappingService;

        PropertyChanged += OnPropertyChanged;

        var positionsResponse = _mediator.Send(new GetPositionsRequest()).Result;
        if (positionsResponse.TryPickT1(out var exception, out var positions))
        {
            MessageBox.Show(exception.Message);
            return;
        }

        var allPosition = new Position
        {
            Id = 0,
            Name = "All"
        };
        Positions.Add(allPosition);
        Positions.AddRange(positions
            .Where(x => x.IsPrimaryPosition)
            .Select(p => p.FromCore()));

        SelectedPosition = allPosition;

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
        var topBattersResult = await _mediator.Send(new GetTopBattingCareersRequest(
            pageNumber: PageNumber,
            limit: ResultsPerPage,
            orderBy: SortColumn,
            onlyHallOfFamers: OnlyHallOfFamers,
            primaryPositionId: SelectedPosition?.Id == 0 ? null : SelectedPosition?.Id
        ));

        if (topBattersResult.TryPickT1(out var exception, out var topPlayers))
        {
            Application.Current.Dispatcher.Invoke(() => MessageBox.Show(exception.Message));
            return;
        }

        TopBattingCareers.Clear();
        var topBattingCareers = topPlayers
                .Select(async x => await _mappingService.FromCore(x))
                .Select(x => x.Result);
        TopBattingCareers.AddRange(topBattingCareers);

        IncrementPageCommand.NotifyCanExecuteChanged();
        DecrementPageCommand.NotifyCanExecuteChanged();
        Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);
    }

    protected override void Dispose(bool disposing)
    {
        PropertyChanged -= OnPropertyChanged;
        base.Dispose(disposing);
    }
}