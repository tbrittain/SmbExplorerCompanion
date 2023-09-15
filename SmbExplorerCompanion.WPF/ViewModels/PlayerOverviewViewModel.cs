﻿using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
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

    public PlayerOverviewViewModel(INavigationService navigationService, ISender mediator)
    {
        Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = Cursors.Wait);
        _navigationService = navigationService;

        var ok = _navigationService.TryGetParameter<int>(PlayerIdProp, out var playerId);
        if (!ok)
        {
            const string message = "Could not get player id from navigation parameters.";
            MessageBox.Show(message);
            throw new Exception(message);
        }

        PlayerId = playerId;
        _navigationService.ClearParameters();

        var playerOverviewResponse = mediator.Send(new GetPlayerOverviewRequest(PlayerId)).Result;
        if (playerOverviewResponse.TryPickT1(out var exception, out var playerOverview))
        {
            MessageBox.Show(exception.Message);
            PlayerOverview = new PlayerOverview();
            Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);
            return;
        }

        var mapper = new PlayerOverviewMapping();
        var overview = mapper.FromDto(playerOverview);
        PlayerOverview = overview;
        
        Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);
    }

    public bool HasAnyBatting => HasSeasonBatting || HasPlayoffBatting;
    public bool HasSeasonBatting => PlayerOverview.PlayerSeasonBatting.Any();
    public bool HasPlayoffBatting => PlayerOverview.PlayerPlayoffBatting.Any();
    
    public bool HasAnyPitching => HasSeasonPitching || HasPlayoffPitching;
    public bool HasSeasonPitching => PlayerOverview.PlayerSeasonPitching.Any();
    public bool HasPlayoffPitching => PlayerOverview.PlayerPlayoffPitching.Any();

    public int PitcherGridRow => PlayerOverview.IsPitcher ? 0 : 1;
    public int BatterGridRow => PlayerOverview.IsPitcher ? 1 : 0;

    public PlayerOverview PlayerOverview { get; }

    private int PlayerId { get; }
}