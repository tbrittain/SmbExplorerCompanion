using System;
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
            PlayerOverview = mapper.FromDto(playerOverview);
        }
    }

    public PlayerOverview PlayerOverview { get; set; }

    private int PlayerId { get; set; }
}