using System;
using System.Windows;
using MediatR;
using SmbExplorerCompanion.Core.Commands.Queries.Teams;
using SmbExplorerCompanion.WPF.Models.Teams;
using SmbExplorerCompanion.WPF.Services;

namespace SmbExplorerCompanion.WPF.ViewModels;

public class TeamSeasonDetailViewModel : ViewModelBase
{
    public const string TeamIdProp = "TeamId";
    public const string SeasonIdProp = "SeasonId";
    private readonly INavigationService _navigationService;
    private readonly IMediator _mediator;

    public TeamSeasonDetailViewModel(INavigationService navigationService, IMediator mediator)
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

        ok = _navigationService.TryGetParameter<int>(SeasonIdProp, out var seasonId);
        if (!ok)
        {
            const string message = "Could not get season id from navigation parameters.";
            MessageBox.Show(message);
            throw new Exception(message);
        }

        SeasonId = seasonId;
        _navigationService.ClearParameters();
        
        var teamSeasonDetailResponse = _mediator.Send(
            new GetTeamSeasonDetailRequest(SeasonId, TeamId)).Result;

        if (teamSeasonDetailResponse.TryPickT1(out var exception, out var teamSeasonDetail))
        {
            MessageBox.Show(exception.Message);
            TeamSeasonDetail = new TeamSeasonDetail();
        }
    }

    private int SeasonId { get; }
    private int TeamId { get; }
    public TeamSeasonDetail TeamSeasonDetail { get; set; }
}