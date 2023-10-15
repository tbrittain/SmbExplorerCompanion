using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using MediatR;
using ScottPlot;
using SmbExplorerCompanion.Core.Commands.Queries.Teams;
using SmbExplorerCompanion.Core.Entities.Teams;
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
        Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = Cursors.Wait);
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
        
        var teamScheduleBreakdownResponse = mediator.Send(
            new GetTeamScheduleBreakdownRequest(TeamSeasonId)).Result;
        
        if (teamScheduleBreakdownResponse.TryPickT1(out exception, out var teamScheduleBreakdown))
        {
            MessageBox.Show(exception.Message);
        }
        else
        {
            TeamScheduleBreakdowns = SetBreakdowns(teamScheduleBreakdown);
        }

        PropertyChanged += OnPropertyChanged;
        
        Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);
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

    public HashSet<TeamScheduleBreakdown> TeamScheduleBreakdowns { get; set; } = new();

    private static HashSet<TeamScheduleBreakdown> SetBreakdowns(HashSet<TeamScheduleBreakdownDto> breakdownDtos)
    {
        var breakdowns = new HashSet<TeamScheduleBreakdown>();
        var wins = 0;
        foreach (var dto in breakdownDtos)
        {
            if (dto.TeamScore > dto.OpponentTeamScore)
                wins++;
            else if (dto.TeamScore < dto.OpponentTeamScore)
                wins--;
            var breakdown = new TeamScheduleBreakdown(dto.TeamHistoryId, dto.TeamName, dto.OpponentTeamHistoryId,
                dto.OpponentTeamName, dto.Day, dto.GlobalGameNumber, dto.TeamScore, dto.OpponentTeamScore,
                wins);
            breakdowns.Add(breakdown);
        }

        return breakdowns;
    }

    public void DrawTeamSchedulePlot(WpfPlot plot)
    {
        plot.Plot.Clear();
        
        var teamScheduleBreakdowns = TeamScheduleBreakdowns
            .OrderBy(b => b.Day)
            .ToHashSet();
        
        var days = teamScheduleBreakdowns.Select(b => (double)b.Day).ToArray();
        var steps = teamScheduleBreakdowns.Select(b => (double)b.WinsDelta).ToArray();
        
        plot.Plot.AddScatterStep(xs: days, ys: steps, label: "Games >.500", lineWidth: 2);
        plot.Plot.AddHorizontalLine(0, color: System.Drawing.Color.Black, style: LineStyle.Dash);
        plot.Height = 300;
        plot.Width = 1000;
        plot.Plot.SetAxisLimits(xMin: days.Min(), xMax: days.Max());
        
        plot.Plot.XAxis.TickLabelStyle(rotation: 45);
        plot.Plot.XAxis.Label("Day");
        plot.Plot.YAxis.Label("Games >.500");
        plot.Plot.Title("Team Schedule");
        plot.Render();
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