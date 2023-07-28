using System;
using System.Linq;
using System.Windows;
using MediatR;
using SmbExplorerCompanion.Core.Commands.Queries.Players;
using SmbExplorerCompanion.WPF.Mappings.Players;
using SmbExplorerCompanion.WPF.Models.Players;
using SmbExplorerCompanion.WPF.Services;

namespace SmbExplorerCompanion.WPF.ViewModels;

public class PlayerOverviewViewModel : ViewModelBase
{
    public const string PlayerIdProp = "PlayerId";
    private readonly INavigationService _navigationService;
    private readonly IMediator _mediator;

    public PlayerOverviewViewModel(INavigationService navigationService, IMediator mediator)
    {
        _navigationService = navigationService;
        _mediator = mediator;

        var ok = _navigationService.TryGetParameter<int>(PlayerIdProp, out var playerId);
        if (!ok)
        {
            const string message = "Could not get player id from navigation parameters.";
            MessageBox.Show(message);
            throw new Exception(message);
        }

        PlayerId = playerId;
        _navigationService.ClearParameters();

        var playerOverviewResponse = _mediator.Send(new GetPlayerOverviewRequest(PlayerId)).Result;
        if (playerOverviewResponse.TryPickT1(out var exception, out var playerOverview))
        {
            MessageBox.Show(exception.Message);
            PlayerOverview = new PlayerOverview();
        }
        else
        {
            var mapper = new PlayerOverviewMapping();
            var overview = mapper.FromDto(playerOverview);
            overview.PopulateCareerStats();
            PlayerOverview = overview;
        }
    }

    public Visibility CareerPitchingVisibility => PlayerOverview.IsPitcher ? Visibility.Visible : Visibility.Collapsed;
    public Visibility CareerBattingVisibility => PlayerOverview.IsPitcher ? Visibility.Collapsed : Visibility.Visible;

    public Visibility OverallBattingVisibility => PlayerOverview.PlayerSeasonBatting.Any() ||
                                                  PlayerOverview.PlayerPlayoffBatting.Any()
        ? Visibility.Visible
        : Visibility.Collapsed;
    public Visibility SeasonBattingVisibility => PlayerOverview.PlayerSeasonBatting.Any() ? Visibility.Visible : Visibility.Collapsed;
    public Visibility PlayoffBattingVisibility => PlayerOverview.PlayerPlayoffBatting.Any() ? Visibility.Visible : Visibility.Collapsed;
    public Visibility OverallPitchingVisibility => PlayerOverview.PlayerSeasonPitching.Any() ||
                                                   PlayerOverview.PlayerPlayoffPitching.Any()
        ? Visibility.Visible
        : Visibility.Collapsed;
    public Visibility SeasonPitchingVisibility => PlayerOverview.PlayerSeasonPitching.Any() ? Visibility.Visible : Visibility.Collapsed;
    public Visibility PlayoffPitchingVisibility => PlayerOverview.PlayerPlayoffPitching.Any() ? Visibility.Visible : Visibility.Collapsed;

    public int PitcherGridRow => PlayerOverview.IsPitcher ? 0 : 1;
    public int BatterGridRow => PlayerOverview.IsPitcher ? 1 : 0;

    public PlayerOverview PlayerOverview { get; set; }

    private int PlayerId { get; set; }
}