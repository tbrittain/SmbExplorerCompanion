using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using SmbExplorerCompanion.Core.Commands.Queries.Players;
using SmbExplorerCompanion.Core.Commands.Queries.Seasons;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.Core.Interfaces;
using SmbExplorerCompanion.WPF.Extensions;
using SmbExplorerCompanion.WPF.Mappings.Players;
using SmbExplorerCompanion.WPF.Mappings.Seasons;
using SmbExplorerCompanion.WPF.Models.Players;
using SmbExplorerCompanion.WPF.Models.Seasons;
using SmbExplorerCompanion.WPF.Services;

namespace SmbExplorerCompanion.WPF.ViewModels;

public partial class TopBattingSeasonsViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private readonly INavigationService _navigationService;
    private Season? _selectedSeason;
    private int _pageNumber = 1;
    private PlayerBase? _selectedPlayer;
    private bool _isPlayoffs;

    public TopBattingSeasonsViewModel(IMediator mediator, IApplicationContext applicationContext, INavigationService navigationService)
    {
        _mediator = mediator;
        _navigationService = navigationService;

        var seasonsResponse = _mediator.Send(new GetSeasonsByFranchiseRequest(
            applicationContext.SelectedFranchiseId!.Value)).Result;

        if (seasonsResponse.TryPickT1(out var exception, out var seasons))
        {
            MessageBox.Show(exception.Message);
            return;
        }

        var seasonMapper = new SeasonMapping();
        Seasons.Add(new Season
        {
            Id = default,
        });
        Seasons.AddRange(seasons.Select(s => seasonMapper.FromDto(s)));
        SelectedSeason = Seasons.OrderByDescending(x => x.Number).First();

        GetTopBattingSeason();

        PropertyChanged += OnPropertyChanged;
    }

    public PlayerBase? SelectedPlayer
    {
        get => _selectedPlayer;
        set => SetField(ref _selectedPlayer, value);
    }

    public bool ShortCircuitPageNumberRefresh { get; set; }

    private bool CanSelectPreviousPage => PageNumber > 1;
    private bool CanSelectNextPage => TopSeasonBatters.Count == 20;

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
            case nameof(IsPlayoffs):
            case nameof(SelectedSeason):
            {
                ShortCircuitPageNumberRefresh = true;
                PageNumber = 1;
                ShortCircuitPageNumberRefresh = false;
                await GetTopBattingSeason();
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
                if (!ShortCircuitPageNumberRefresh)
                {
                    await GetTopBattingSeason();
                }

                break;
            }
        }
    }

    public ObservableCollection<Season> Seasons { get; } = new();

    public Season? SelectedSeason
    {
        get => _selectedSeason;
        set => SetField(ref _selectedSeason, value);
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

    public bool IsPlayoffs
    {
        get => _isPlayoffs;
        set => SetField(ref _isPlayoffs, value);
    }

    public Task GetTopBattingSeason()
    {
        var topBattersResult = _mediator.Send(new GetTopBattingSeasonRequest(
            SelectedSeason!.Id,
            IsPlayoffs,
            PageNumber,
            SortColumn,
            true)).Result;

        if (topBattersResult.TryPickT1(out var exception, out var topBatters))
        {
            Application.Current.Dispatcher.Invoke(() => MessageBox.Show(exception.Message));
            return Task.CompletedTask;
        }

        TopSeasonBatters.Clear();

        var mapper = new PlayerSeasonMapping();
        TopSeasonBatters.AddRange(topBatters.Select(b => mapper.FromBattingDto(b)));

        return Task.CompletedTask;
    }

    private void NavigateToPlayerOverview(PlayerBase player)
    {
        var parameters = new Tuple<string, object>[]
        {
            new(PlayerOverviewViewModel.PlayerIdProp, player.PlayerId)
        };
        _navigationService.NavigateTo<PlayerOverviewViewModel>(parameters);
    }

    protected override void Dispose(bool disposing)
    {
        PropertyChanged -= OnPropertyChanged;
        base.Dispose(disposing);
    }
}