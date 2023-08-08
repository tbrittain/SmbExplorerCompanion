﻿using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using MediatR;
using SmbExplorerCompanion.Core.Commands.Queries.Lookups;
using SmbExplorerCompanion.Core.Commands.Queries.Seasons;
using SmbExplorerCompanion.Core.Interfaces;
using SmbExplorerCompanion.WPF.Extensions;
using SmbExplorerCompanion.WPF.Mappings.Lookups;
using SmbExplorerCompanion.WPF.Mappings.Seasons;
using SmbExplorerCompanion.WPF.Models.Lookups;
using SmbExplorerCompanion.WPF.Models.Players;
using SmbExplorerCompanion.WPF.Models.Seasons;

namespace SmbExplorerCompanion.WPF.ViewModels;

public class DelegateAwardsViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private Season? _selectedSeason;

    public DelegateAwardsViewModel(IMediator mediator, IApplicationContext applicationContext)
    {
        _mediator = mediator;
        
        var seasonsResponse = _mediator.Send(new GetSeasonsByFranchiseRequest(
            applicationContext.SelectedFranchiseId!.Value)).Result;

        if (seasonsResponse.TryPickT1(out var exception, out var seasons))
        {
            MessageBox.Show(exception.Message);
            return;
        }

        var seasonMapper = new SeasonMapping();
        Seasons.Add(new Season
        {
            Id = default
        });
        Seasons.AddRange(seasons.Select(s => seasonMapper.FromDto(s)));
        SelectedSeason = Seasons.OrderByDescending(x => x.Number).First();
        
        var regularSeasonAwards = _mediator.Send(GetPlayerAwardsRequest.Default).Result;
        if (regularSeasonAwards.TryPickT1(out exception, out var awards))
        {
            MessageBox.Show(exception.Message);
            return;
        }

        var awardsMapper = new PlayerAwardMapping();
        AllAwards.AddRange(awards.Select(a => awardsMapper.FromDto(a)));
    }
    
    // TODO: create sub-collections for each award type for easier binding
    public ObservableCollection<PlayerAward> AllAwards { get; } = new();
    
    public ObservableCollection<Season> Seasons { get; } = new();

    public Season? SelectedSeason
    {
        get => _selectedSeason;
        set => SetField(ref _selectedSeason, value);
    }
    
    public ObservableCollection<PlayerSeasonBatting> TopSeasonBatters { get; } = new();
    public ObservableCollection<PlayerSeasonPitching> TopSeasonPitchers { get; } = new();
    
    // TODO: Will need one of these for each position
    public ObservableCollection<PlayerFieldingRanking> TopFielding1B { get; } = new();
}