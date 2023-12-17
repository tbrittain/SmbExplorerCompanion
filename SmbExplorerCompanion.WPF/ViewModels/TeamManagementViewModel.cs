using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using MediatR;
using SmbExplorerCompanion.Core.Commands.Queries.Seasons;
using SmbExplorerCompanion.WPF.Mappings.Seasons;
using SmbExplorerCompanion.WPF.Models.Seasons;
using SmbExplorerCompanion.WPF.Services;

namespace SmbExplorerCompanion.WPF.ViewModels;

public class TeamManagementViewModel : ViewModelBase
{
    public const string TeamIdProp = "TeamId";
    private readonly IMediator _mediator;

    public TeamManagementViewModel(INavigationService navigationService, IMediator mediator)
    {
        Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = Cursors.Wait);
        _mediator = mediator;

        var ok = navigationService.TryGetParameter<int>(TeamIdProp, out var teamId);
        if (!ok)
        {
            const string message = "Could not get team id from navigation parameters.";
            MessageBox.Show(message);
            throw new Exception(message);
        }

        navigationService.ClearParameters();
        TeamId = teamId;
        
        var seasonsResponse = _mediator.Send(new GetSeasonsRequest()).Result;

        if (seasonsResponse.TryPickT1(out var exception, out var seasons))
        {
            MessageBox.Show(exception.Message);
            Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);
            return;
        }

        var seasonMapper = new SeasonMapping();

        Seasons.AddRange(seasons.Select(s => seasonMapper.FromDto(s)));
    }

    private int TeamId { get; }

    private List<Season> Seasons { get; } = new();
}