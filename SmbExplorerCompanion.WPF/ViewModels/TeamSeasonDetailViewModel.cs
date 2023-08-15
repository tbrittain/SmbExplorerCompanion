using System;
using System.ComponentModel;
using System.Windows;
using MediatR;
using SmbExplorerCompanion.Core.Commands.Queries.Teams;
using SmbExplorerCompanion.WPF.Mappings.Teams;
using SmbExplorerCompanion.WPF.Models.Players;
using SmbExplorerCompanion.WPF.Models.Teams;
using SmbExplorerCompanion.WPF.Services;

namespace SmbExplorerCompanion.WPF.ViewModels;

public class TeamSeasonDetailViewModel : ViewModelBase
{
    public const string SeasonTeamIdProp = "SeasonTeamId";
    private readonly INavigationService _navigationService;
    private PlayerBase? _selectedPlayer;

    public TeamSeasonDetailViewModel(INavigationService navigationService, ISender mediator)
    {
        _navigationService = navigationService;

        var ok = _navigationService.TryGetParameter<int>(SeasonTeamIdProp, out var teamSeasonId);
        if (!ok)
        {
            const string message = "Could not get team season id from navigation parameters.";
            MessageBox.Show(message);
            throw new Exception(message);
        }

        TeamSeasonId = teamSeasonId;

        _navigationService.ClearParameters();

        var teamSeasonDetailResponse = mediator.Send(
            new GetTeamSeasonDetailRequest(TeamSeasonId)).Result;

        if (teamSeasonDetailResponse.TryPickT1(out var exception, out var teamSeasonDetail))
        {
            MessageBox.Show(exception.Message);
            TeamSeasonDetail = new TeamSeasonDetail();
        }
        else
        {
            var mapper = new TeamSeasonDetailMapping();
            TeamSeasonDetail = mapper.FromTeamSeasonDetailDto(teamSeasonDetail);
        }

        PropertyChanged += OnPropertyChanged;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
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

    public PlayerBase? SelectedPlayer
    {
        get => _selectedPlayer;
        set => SetField(ref _selectedPlayer, value);
    }

    private int TeamSeasonId { get; }
    public TeamSeasonDetail TeamSeasonDetail { get; set; }
}