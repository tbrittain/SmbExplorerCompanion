using System;
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

        _navigationService.ClearParameters();

        var teamOverviewResponse = _mediator.Send(new GetTeamOverviewRequest(teamId)).Result;
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

        TeamId = teamId;
    }

    public int TeamId { get; set; }

    public TeamOverview TeamOverview { get; set; }
}