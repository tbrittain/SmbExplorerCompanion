using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using ScottPlot;
using ScottPlot.Drawing;
using SmbExplorerCompanion.Core.Commands.Queries.Players;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.WPF.Extensions;
using SmbExplorerCompanion.WPF.Mappings.Players;
using SmbExplorerCompanion.WPF.Models.Players;
using SmbExplorerCompanion.WPF.Services;

namespace SmbExplorerCompanion.WPF.ViewModels;

public partial class PlayerOverviewViewModel : ViewModelBase
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
        MostRecentSeasonStats = overview
            .GameStats
            .OrderByDescending(x => x.SeasonNumber)
            .First();
        
        var leagueAverageResponse = mediator.Send(new GetLeagueAverageGameStatsCommand(MostRecentSeasonStats.SeasonId, PlayerOverview.IsPitcher)).Result;
        if (leagueAverageResponse.TryPickT1(out exception, out var leagueAverage))
        {
            MessageBox.Show(exception.Message);
            Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);
            return;
        }
        LeagueAverage = leagueAverage;

        var similarPlayersResponse = mediator.Send(new GetSimilarPlayersRequest(PlayerId, !overview.IsPitcher)).Result;
        if (similarPlayersResponse.TryPickT1(out exception, out var similarPlayers))
        {
            MessageBox.Show(exception.Message);
            Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);
            return;
        }

        var similarPlayerMapper = new SimilarPlayerMapping();
        SimilarPlayers.AddRange(similarPlayers.Select(similarPlayerMapper.FromDto));

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
    public ObservableCollection<SimilarPlayer> SimilarPlayers { get; } = new();

    [RelayCommand]
    private void NavigateToPlayerOverviewPage(int playerId)
    {
        var playerParams = new Tuple<string, object>[]
        {
            new(PlayerIdProp, playerId)
        };
        _navigationService.NavigateTo<PlayerOverviewViewModel>(playerParams);
    }

    private PlayerGameStatOverview MostRecentSeasonStats { get; set; } = null!;
    private GameStatDto LeagueAverage { get; set; } = null!;

    public void DrawRadialPlot(WpfPlot plot)
    {
        plot.Plot.Clear();
        
        var power = MostRecentSeasonStats.Power;
        var contact = MostRecentSeasonStats.Contact;
        var speed = MostRecentSeasonStats.Speed;
        var fielding = MostRecentSeasonStats.Fielding;
        var arm = MostRecentSeasonStats.Arm ?? 0;
        var velocity = MostRecentSeasonStats.Velocity ?? 0;
        var junk = MostRecentSeasonStats.Junk ?? 0;
        var accuracy = MostRecentSeasonStats.Accuracy ?? 0;
        
        var averagePower = LeagueAverage.Power;
        var averageContact = LeagueAverage.Contact;
        var averageSpeed = LeagueAverage.Speed;
        var averageFielding = LeagueAverage.Fielding;
        var averageArm = LeagueAverage.Arm ?? 0;
        var averageVelocity = LeagueAverage.Velocity ?? 0;
        var averageJunk = LeagueAverage.Junk ?? 0;
        var averageAccuracy = LeagueAverage.Accuracy ?? 0;
        double[,] values;
        double[] maxValues;
        string[] categoryLabels;

        if (PlayerOverview.IsPitcher)
        {
            values = new double[,]
            {
                {velocity, junk, accuracy, fielding, power, contact, speed},
                {averageVelocity, averageJunk, averageAccuracy, averageFielding, averagePower, averageContact, averageSpeed}
            };
            maxValues = new[] {99D, 99D, 99D, 99D, 99D, 99D, 99D};
            categoryLabels = new[]
            {
                "Velocity", "Junk", "Accuracy", "Fielding", "Power", "Contact", "Speed"
            };
        }
        else
        {
            values = new double[,]
            {
                {power, contact, speed, fielding, arm},
                {averagePower, averageContact, averageSpeed, averageFielding, averageArm}
            };
            maxValues = new[] {99D, 99D, 99D, 99D, 99D};
            categoryLabels = new[] {"Power", "Contact", "Speed", "Fielding", "Arm"};
        }

        plot.Plot.Palette = Palette.DarkPastel;
        var radarPlot = plot.Plot.AddRadar(values: values, maxValues: maxValues, independentAxes: false);
        radarPlot.HatchOptions = new HatchOptions[]
        {
            new() {Pattern = HatchStyle.StripedUpwardDiagonal},
            new() {Pattern = HatchStyle.StripedDownwardDiagonal},
        };
        
        radarPlot.AxisType = RadarAxis.Polygon;
        
        var isPitcher = PlayerOverview.IsPitcher;
        var leagueAverageText = isPitcher ? "League Average Pitcher" : "League Average Batter";
        radarPlot.GroupLabels = new[] {PlayerOverview.PlayerName, leagueAverageText};
        radarPlot.CategoryLabels = categoryLabels;
        radarPlot.ShowAxisValues = true;
  
        plot.Height = 300;
        plot.Width = 300;
        plot.Plot.Grid(lineStyle: LineStyle.Dot);
        plot.Plot.Title($"Player Attributes (Season {MostRecentSeasonStats.SeasonNumber})");
        plot.Plot.Legend();
        plot.Plot.AxisAuto();
        plot.Plot.AxisZoom(1.5, 1.5);
        plot.Render();
    }

    private int PlayerId { get; }
}