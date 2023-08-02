using System;
using System.Windows;
using MediatR;
using SmbExplorerCompanion.Core.Commands.Queries.Teams;
using SmbExplorerCompanion.WPF.Models.Teams;
using SmbExplorerCompanion.WPF.Services;

namespace SmbExplorerCompanion.WPF.ViewModels;

public class TeamSeasonDetailViewModel : ViewModelBase
{
    public const string SeasonTeamIdProp = "SeasonTeamId";
    private readonly INavigationService _navigationService;
    private readonly IMediator _mediator;

    public TeamSeasonDetailViewModel(INavigationService navigationService, IMediator mediator)
    {
        _navigationService = navigationService;
        _mediator = mediator;

        var ok = _navigationService.TryGetParameter<int>(SeasonTeamIdProp, out var teamSeasonId);
        if (!ok)
        {
            const string message = "Could not get team season id from navigation parameters.";
            MessageBox.Show(message);
            throw new Exception(message);
        }

        TeamSeasonId = teamSeasonId;

        _navigationService.ClearParameters();
        
        var teamSeasonDetailResponse = _mediator.Send(
            new GetTeamSeasonDetailRequest(TeamSeasonId)).Result;

        if (teamSeasonDetailResponse.TryPickT1(out var exception, out var teamSeasonDetail))
        {
            MessageBox.Show(exception.Message);
            TeamSeasonDetail = new TeamSeasonDetail();
        }
    }

    private int TeamSeasonId { get; }
    public TeamSeasonDetail TeamSeasonDetail { get; set; }
}