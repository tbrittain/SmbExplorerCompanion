using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using SmbExplorerCompanion.Core.Commands.Actions.Awards;
using SmbExplorerCompanion.Core.Commands.Queries.Players;
using SmbExplorerCompanion.Core.Commands.Queries.Seasons;
using SmbExplorerCompanion.Core.Interfaces;
using SmbExplorerCompanion.Core.ValueObjects.Awards;
using SmbExplorerCompanion.WPF.Extensions;
using SmbExplorerCompanion.WPF.Mappings.Players;
using SmbExplorerCompanion.WPF.Mappings.Seasons;
using SmbExplorerCompanion.WPF.Models.Players;
using SmbExplorerCompanion.WPF.Models.Seasons;
using SmbExplorerCompanion.WPF.Services;

namespace SmbExplorerCompanion.WPF.ViewModels;

public partial class DelegateHallOfFamersViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private readonly INavigationService _navigationService;
    private PlayerBase? _selectedPlayer;
    private Season? _selectedSeason;

    public DelegateHallOfFamersViewModel(IMediator mediator,
        IApplicationContext applicationContext,
        INavigationService navigationService)
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

        GetHallOfFamers().Wait();

        PropertyChanged += OnPropertyChanged;
    }

    public ObservableCollection<Season> Seasons { get; } = new();

    public Season? SelectedSeason
    {
        get => _selectedSeason;
        set => SetField(ref _selectedSeason, value);
    }

    private bool CanSubmitHallOfFamers => TopBattingCareers.Any() || TopPitchingCareers.Any();

    public ObservableCollection<PlayerBattingCareer> TopBattingCareers { get; } = new();
    public ObservableCollection<PlayerPitchingCareer> TopPitchingCareers { get; } = new();

    public PlayerBase? SelectedPlayer
    {
        get => _selectedPlayer;
        set => SetField(ref _selectedPlayer, value);
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(SelectedSeason):
                GetHallOfFamers().Wait();
                break;
            case nameof(SelectedPlayer):
            {
                if (SelectedPlayer is not null)
                    NavigateToPlayerOverview(SelectedPlayer);
                break;
            }
        }
    }

    private void NavigateToPlayerOverview(PlayerBase player)
    {
        var parameters = new Tuple<string, object>[]
        {
            new(PlayerOverviewViewModel.PlayerIdProp, player.PlayerId)
        };
        _navigationService.NavigateTo<PlayerOverviewViewModel>(parameters);
    }

    private async Task GetHallOfFamers()
    {
        var response = await _mediator.Send(new GetHallOfFameCandidatesRequest(SelectedSeason!.Id));

        if (response.TryPickT2(out var exception, out var rest))
        {
            MessageBox.Show(exception.Message);
            return;
        }

        TopBattingCareers.Clear();
        TopPitchingCareers.Clear();

        if (rest.TryPickT1(out _, out var retiredPlayers))
        {
            MessageBox.Show("No retired players found. Please try another season.");
            SubmitHallOfFamersCommand.NotifyCanExecuteChanged();
            return;
        }

        var battingMapper = new PlayerCareerMapping();
        TopBattingCareers.AddRange(retiredPlayers.BattingCareers
            .Select(b => battingMapper.FromBattingDto(b)));

        var pitchingMapper = new PlayerCareerMapping();
        TopPitchingCareers.AddRange(retiredPlayers.PitchingCareers
            .Select(p => pitchingMapper.FromPitchingDto(p)));

        SubmitHallOfFamersCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanSubmitHallOfFamers))]
    private async Task SubmitHallOfFamers()
    {
        if (SelectedSeason is null)
        {
            MessageBox.Show("No season selected.");
            return;
        }

        List<PlayerHallOfFameRequestDto> hallOfFamers = new();

        foreach (var battingCareer in TopBattingCareers)
        {
            hallOfFamers.Add(new PlayerHallOfFameRequestDto
            {
                PlayerId = battingCareer.PlayerId,
                IsHallOfFamer = battingCareer.IsHallOfFamer
            });
        }

        foreach (var pitchingCareer in TopPitchingCareers)
        {
            hallOfFamers.Add(new PlayerHallOfFameRequestDto
            {
                PlayerId = pitchingCareer.PlayerId,
                IsHallOfFamer = pitchingCareer.IsHallOfFamer
            });
        }

        var response = await _mediator.Send(new AddHallOfFamersRequest(hallOfFamers));
        if (response.TryPickT1(out var exception, out _))
        {
            MessageBox.Show("Unable to add player awards. Please try again. " + exception.Message);
            return;
        }

        MessageBox.Show($"Eligible Hall of Famers for Season {SelectedSeason.Number} added successfully!");
    }

    protected override void Dispose(bool disposing)
    {
        PropertyChanged -= OnPropertyChanged;
        base.Dispose(disposing);
    }
}