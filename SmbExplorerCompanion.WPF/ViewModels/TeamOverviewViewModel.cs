using System;
using System.ComponentModel;
using System.Windows;
using MediatR;
using SmbExplorerCompanion.Core.Commands.Queries.Teams;
using SmbExplorerCompanion.WPF.Mappings.Teams;
using SmbExplorerCompanion.WPF.Models.Teams;
using SmbExplorerCompanion.WPF.Services;

namespace SmbExplorerCompanion.WPF.ViewModels;

public class TeamOverviewViewModel : ViewModelBase
{
    public const string TeamIdProp = "TeamId";

    private readonly INavigationService _navigationService;
    private readonly IMediator _mediator;
    private TeamTopPlayerHistory? _selectedPlayer;

    public TeamOverviewViewModel(INavigationService navigationService, IMediator mediator)
    {
        _navigationService = navigationService;
        _mediator = mediator;

        var ok = _navigationService.TryGetParameter<int>(TeamIdProp, out var teamId);
        if (!ok)
        {
            const string message = "Could not get team id from navigation parameters.";
            MessageBox.Show(message);
            throw new Exception(message);
        }

        TeamId = teamId;
        _navigationService.ClearParameters();

        var teamOverviewResponse = _mediator.Send(new GetTeamOverviewRequest(TeamId)).Result;
        if (teamOverviewResponse.TryPickT1(out var exception, out var teamOverview))
        {
            MessageBox.Show(exception.Message);
            TeamOverview = new TeamOverview();
        }
        else
        {
            var mapper = new TeamOverviewMapping();
            TeamOverview = mapper.FromDto(teamOverview);
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

    private int TeamId { get; set; }

    public string TeamRecord => $"{TeamOverview.NumWins}-{TeamOverview.NumLosses}, {TeamOverview.WinPercentage:F3} W-L%";

    public TeamOverview TeamOverview { get; set; }

    public TeamTopPlayerHistory? SelectedPlayer
    {
        get => _selectedPlayer;
        set => SetField(ref _selectedPlayer, value);
    }

    private void NavigateToPlayerOverview(TeamTopPlayerHistory player)
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