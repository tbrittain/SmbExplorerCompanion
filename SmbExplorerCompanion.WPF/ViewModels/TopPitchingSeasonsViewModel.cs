﻿using System;
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

public partial class TopPitchingSeasonsViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private readonly INavigationService _navigationService;
    private bool _isPlayoffs;
    private int _pageNumber = 1;
    private PlayerBase? _selectedPlayer;
    private Season? _selectedSeason;
    private bool _onlyRookies;

    public TopPitchingSeasonsViewModel(IMediator mediator, INavigationService navigationService, IApplicationContext applicationContext)
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

        Seasons.AddRange(seasons.Select(s => seasonMapper.FromDto(s)));
        SelectedSeason = Seasons.OrderByDescending(x => x.Number).First();
        MinSeasonId = Seasons.OrderBy(x => x.Number).First().Id;

        Seasons.Add(new Season
        {
            Id = default
        });

        GetTopPitchingSeason().Wait();

        PropertyChanged += OnPropertyChanged;
    }

    public ObservableCollection<Season> Seasons { get; } = new();

    private int MinSeasonId { get; }


    public Season? SelectedSeason
    {
        get => _selectedSeason;
        set
        {
            if (value is not null && (value?.Id == default(int) || value?.Id == MinSeasonId))
            {
                ShortCircuitOnlyRookiesRefresh = true;
                OnlyRookies = false;
                ShortCircuitOnlyRookiesRefresh = false;
            }

            SetField(ref _selectedSeason, value);

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
            if (SelectedSeason is null) return false;
            if (SelectedSeason.Id == default) return false;
            if (SelectedSeason.Id == MinSeasonId) return false;

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

    public PlayerBase? SelectedPlayer
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
            case nameof(SelectedSeason):
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
        var topPitchersResult = await _mediator.Send(new GetTopPitchingSeasonRequest(
            seasonId: SelectedSeason!.Id == default ? null : SelectedSeason!.Id,
            isPlayoffs: IsPlayoffs,
            pageNumber: PageNumber,
            orderBy: SortColumn,
            limit: ResultsPerPage,
            onlyRookies: OnlyRookies,
            descending: true));

        if (topPitchersResult.TryPickT1(out var exception, out var topPitchers))
        {
            Application.Current.Dispatcher.Invoke(() => MessageBox.Show(exception.Message));
            return;
        }

        TopSeasonPitchers.Clear();

        var mapper = new PlayerSeasonMapping();
        TopSeasonPitchers.AddRange(topPitchers.Select(p => mapper.FromPitchingDto(p)));
        
        IncrementPageCommand.NotifyCanExecuteChanged();
        DecrementPageCommand.NotifyCanExecuteChanged();
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