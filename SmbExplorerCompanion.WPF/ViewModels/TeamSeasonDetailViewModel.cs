using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using MediatR;
using ScottPlot;
using SmbExplorerCompanion.Core.Commands.Queries.Teams;
using SmbExplorerCompanion.Core.Entities.Teams;
using SmbExplorerCompanion.WPF.Models.Players;
using SmbExplorerCompanion.WPF.Models.Teams;
using SmbExplorerCompanion.WPF.Services;

namespace SmbExplorerCompanion.WPF.ViewModels;

public class TeamSeasonDetailViewModel : ViewModelBase
{
    public const string SeasonTeamIdProp = "SeasonTeamId";
    private readonly INavigationService _navigationService;
    private PlayerBase? _selectedPlayer;
    private bool _includeDivisionTeamsInPlot = true;
    private WpfPlot? _teamSchedulePlot;
    private bool _includeMarginOfVictoryInPlot;

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
        
        if (teamScheduleBreakdownResponse.TryPickT1(out exception, out var divisionScheduleBreakdown))
        {
            MessageBox.Show(exception.Message);
        }
        else
        {
            TeamScheduleBreakdowns = SetBreakdowns(divisionScheduleBreakdown.TeamScheduleBreakdowns);
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
            case nameof(IncludeMarginOfVictoryInPlot):
            case nameof(IncludeDivisionTeamsInPlot):
            {
                if (_teamSchedulePlot is not null)
                    DrawTeamSchedulePlot(_teamSchedulePlot);
                break;
            }
        }
    }

    public bool IncludeDivisionTeamsInPlot
    {
        get => _includeDivisionTeamsInPlot;
        set => SetField(ref _includeDivisionTeamsInPlot, value);
    }

    public bool IncludeMarginOfVictoryInPlot
    {
        get => _includeMarginOfVictoryInPlot;
        set => SetField(ref _includeMarginOfVictoryInPlot, value);
    }

    public List<HashSet<TeamScheduleBreakdown>> TeamScheduleBreakdowns { get; set; } = new();

    private static List<HashSet<TeamScheduleBreakdown>> SetBreakdowns(List<HashSet<TeamScheduleBreakdownDto>> divisionSchedules)
    {
        var divisionScheduleBreakdowns = new List<HashSet<TeamScheduleBreakdown>>();
        foreach (var teamSchedule in divisionSchedules)
        {
            var teamBreakdown = new HashSet<TeamScheduleBreakdown>();
            var wins = 0;
            foreach (var matchup in teamSchedule)
            {
                if (matchup.TeamScore > matchup.OpponentTeamScore)
                    wins++;
                else if (matchup.TeamScore < matchup.OpponentTeamScore)
                    wins--;
                var x = new TeamScheduleBreakdown(matchup.TeamHistoryId, matchup.TeamName, matchup.OpponentTeamHistoryId,
                    matchup.OpponentTeamName, matchup.Day, matchup.GlobalGameNumber, matchup.TeamScore, matchup.OpponentTeamScore,
                    wins);
                teamBreakdown.Add(x);
            }

            divisionScheduleBreakdowns.Add(teamBreakdown);
        }

        return divisionScheduleBreakdowns;
    }

    public void DrawTeamSchedulePlot(WpfPlot plot)
    {
        _teamSchedulePlot = plot;
        plot.Plot.Clear();
        plot.Plot.Palette = Palette.Microcharts;

        var min = 0D;
        var max = 0D;

        var breakdowns = TeamScheduleBreakdowns;
        if (!IncludeDivisionTeamsInPlot)
        {
            breakdowns = breakdowns
                .Where(b => b
                    .All(x => x.TeamHistoryId == TeamSeasonId))
                .ToList();
        }

        foreach (var breakdown in breakdowns)
        {
            var teamScheduleBreakdowns = breakdown
                .OrderBy(b => b.Day)
                .ToHashSet();
        
            var days = teamScheduleBreakdowns
                .Select(b => (double)b.Day)
                .ToArray();
            min = Math.Min(min, days.Min());
            max = Math.Max(max, days.Max());
            
            var steps = teamScheduleBreakdowns
                .Select(b => (double)b.WinsDelta)
                .ToArray();
        
            var scatterPlot = plot.Plot.AddScatterStep(xs: days, ys: steps, label: breakdown.First().TeamName, lineWidth: 2);
            scatterPlot.MarkerShape = MarkerShape.filledCircle;
            scatterPlot.MarkerSize = 2;

            if (IncludeMarginOfVictoryInPlot)
            {
                var yErrPos = teamScheduleBreakdowns
                    .Select(b => b.TeamScore > b.OpponentTeamScore ? b.TeamScore - b.OpponentTeamScore : 0D)
                    .ToArray();
                var yErrNeg = teamScheduleBreakdowns
                    .Select(b => b.TeamScore < b.OpponentTeamScore ? b.OpponentTeamScore - b.TeamScore : 0D)
                    .ToArray();

                var xErrors = teamScheduleBreakdowns
                    .Select(_ => 0D)
                    .ToArray();
                
                plot.Plot.AddErrorBars(xs: days, ys: steps,xErrorsPositive: xErrors, xErrorsNegative: xErrors, yErrorsNegative: yErrNeg, yErrorsPositive: yErrPos);
            }
        }

        plot.Plot.AddHorizontalLine(0, color: System.Drawing.Color.Black, style: LineStyle.Dash);
        plot.Height = 300;
        plot.Width = 1000;
        plot.Plot.SetAxisLimits(xMin: min, xMax: max);
        
        plot.Plot.XAxis.TickLabelStyle(rotation: 45);
        plot.Plot.XAxis.Label("Day");
        plot.Plot.YAxis.Label("Games >.500");
        plot.Plot.Title("Team Schedule");
        plot.Plot.Legend(location: Alignment.LowerLeft);
        plot.Render();
    }

    private void NavigateToPlayerOverview(PlayerBase player)
    {
        var parameters = new Tuple<string, object>[]
        {
            new(PlayerOverviewViewModel.PlayerIdProp, player.PlayerId),
            new(PlayerOverviewViewModel.TeamSeasonIdProp, TeamSeasonId)
        };
        _navigationService.NavigateTo<PlayerOverviewViewModel>(parameters);
    }

    public PlayerBase? SelectedPlayer
    {
        get => _selectedPlayer;
        set => SetField(ref _selectedPlayer, value);
    }

    public int TeamSeasonId { get; }
    public TeamSeasonDetail TeamSeasonDetail { get; set; }
}