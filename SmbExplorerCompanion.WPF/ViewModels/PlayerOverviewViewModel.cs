﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using ScottPlot;
using ScottPlot.Drawing;
using SmbExplorerCompanion.Core.Commands.Queries.Players;
using SmbExplorerCompanion.Core.Commands.Queries.Seasons;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.Core.Interfaces;
using SmbExplorerCompanion.WPF.Extensions;
using SmbExplorerCompanion.WPF.Mappings.Players;
using SmbExplorerCompanion.WPF.Mappings.Seasons;
using SmbExplorerCompanion.WPF.Models.Players;
using SmbExplorerCompanion.WPF.Models.Seasons;
using SmbExplorerCompanion.WPF.Services;

namespace SmbExplorerCompanion.WPF.ViewModels;

public partial class PlayerOverviewViewModel : ViewModelBase
{
    public const string PlayerIdProp = "PlayerId";
    private readonly INavigationService _navigationService;
    private Season? _selectedSeason;
    private readonly ISender _mediator;

    public PlayerOverviewViewModel(INavigationService navigationService, ISender mediator, IApplicationContext applicationContext)
    {
        Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = Cursors.Wait);
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
            Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);
            return;
        }

        var mapper = new PlayerOverviewMapping();
        var overview = mapper.FromDto(playerOverview);
        PlayerOverview = overview;
        SeasonStats = overview
            .GameStats
            .OrderByDescending(x => x.SeasonNumber)
            .First();

        var similarPlayersResponse = _mediator.Send(new GetSimilarPlayersRequest(PlayerId, !PlayerOverview.IsPitcher)).Result;
        if (similarPlayersResponse.TryPickT1(out exception, out var similarPlayers))
        {
            MessageBox.Show(exception.Message);
            Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);
            return;
        }

        var similarPlayerMapper = new SimilarPlayerMapping();
        SimilarPlayers.AddRange(similarPlayers.Select(similarPlayerMapper.FromDto));
        
        var seasonsResponse = _mediator.Send(new GetSeasonsByFranchiseRequest(
            applicationContext.SelectedFranchiseId!.Value)).Result;

        if (seasonsResponse.TryPickT1(out exception, out var seasons))
        {
            MessageBox.Show(exception.Message);
            Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);
            return;
        }

        var seasonMapper = new SeasonMapping();

        var applicableSeasonIds = overview
            .GameStats
            .Select(x => x.SeasonId)
            .Distinct()
            .ToList();
        var playerSeasons = seasons
            .Where(x => applicableSeasonIds.Contains(x.Id))
            .OrderBy(x => x.Number)
            .Select(s => seasonMapper.FromDto(s))
            .ToList();
        Seasons.AddRange(playerSeasons);
        SelectedSeason = Seasons.OrderByDescending(x => x.Number).First();
        
        GetLeagueAveragesForSeason().Wait();

        PropertyChanged += OnPropertyChanged;
        Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);
    }

    private async void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(SelectedSeason):
            {
                if (SelectedSeason is null) return;
                await GetLeagueAveragesForSeason();
                break;
            }
        }
    }

    private async Task GetLeagueAveragesForSeason()
    {
        if (SelectedSeason is null)
        {
            MessageBox.Show("No season selected.");
            return;
        }
        
        var leagueAverageResponse = await _mediator.Send(
            new GetLeagueAverageGameStatsRequest(SelectedSeason.Id, PlayerOverview.IsPitcher));
        if (leagueAverageResponse.TryPickT1(out var exception, out var leagueAverage))
        {
            MessageBox.Show(exception.Message);
            Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);
            return;
        }
        LeagueAverageGameStats = leagueAverage;
        
        var playerGameStatPercentilesResponse = await _mediator.Send(new GetPlayerGameStatPercentilesRequest(PlayerId, SelectedSeason.Id, PlayerOverview.IsPitcher));
        if (playerGameStatPercentilesResponse.TryPickT1(out exception, out var playerGameStatPercentiles))
        {
            MessageBox.Show(exception.Message);
            Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);
            return;
        }
        PlayerGameStatPercentiles = playerGameStatPercentiles;

        SeasonStats = PlayerOverview
            .GameStats
            .Single(x => x.SeasonId == SelectedSeason.Id);

        if (_playerGameStatsRadialPlot is not null) 
            DrawPlayerGameStatsRadialPlot(_playerGameStatsRadialPlot);
        if (_playerGameStatsPercentilePlot is not null) 
            DrawPlayerGameStatsPercentilePlot(_playerGameStatsPercentilePlot);
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

    private WpfPlot? _playerGameStatsRadialPlot;
    private WpfPlot? _playerGameStatsPercentilePlot;

    [RelayCommand]
    private void NavigateToPlayerOverviewPage(int playerId)
    {
        var playerParams = new Tuple<string, object>[]
        {
            new(PlayerIdProp, playerId)
        };
        _navigationService.NavigateTo<PlayerOverviewViewModel>(playerParams);
    }

    private PlayerGameStatOverview SeasonStats { get; set; }
    private GameStatDto LeagueAverageGameStats { get; set; }
    private PlayerGameStatPercentileDto PlayerGameStatPercentiles { get; set; }

    public ObservableCollection<Season> Seasons { get; } = new();
    public Season? SelectedSeason
    {
        get => _selectedSeason;
        set => SetField(ref _selectedSeason, value);
    }

    public void DrawPlayerGameStatsRadialPlot(WpfPlot plot)
    {
        _playerGameStatsRadialPlot = plot;
        plot.Plot.Clear();
        
        var power = SeasonStats.Power;
        var contact = SeasonStats.Contact;
        var speed = SeasonStats.Speed;
        var fielding = SeasonStats.Fielding;
        var arm = SeasonStats.Arm ?? 0;
        var velocity = SeasonStats.Velocity ?? 0;
        var junk = SeasonStats.Junk ?? 0;
        var accuracy = SeasonStats.Accuracy ?? 0;
        
        var averagePower = LeagueAverageGameStats.Power;
        var averageContact = LeagueAverageGameStats.Contact;
        var averageSpeed = LeagueAverageGameStats.Speed;
        var averageFielding = LeagueAverageGameStats.Fielding;
        var averageArm = LeagueAverageGameStats.Arm ?? 0;
        var averageVelocity = LeagueAverageGameStats.Velocity ?? 0;
        var averageJunk = LeagueAverageGameStats.Junk ?? 0;
        var averageAccuracy = LeagueAverageGameStats.Accuracy ?? 0;
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
        plot.Plot.Title($"Player Attributes (Season {SeasonStats.SeasonNumber})");
        plot.Plot.Legend();
        plot.Plot.AxisAuto();
        plot.Plot.AxisZoom(1.5, 1.5);
        plot.Render();
    }

    public void DrawPlayerGameStatsPercentilePlot(WpfPlot plot)
    {
        _playerGameStatsPercentilePlot = plot;
        plot.Plot.Clear();
        
        var power = PlayerGameStatPercentiles.Power;
        var contact = PlayerGameStatPercentiles.Contact;
        var speed = PlayerGameStatPercentiles.Speed;
        var fielding = PlayerGameStatPercentiles.Fielding;
        var arm = PlayerGameStatPercentiles.Arm ?? 0;
        var velocity = PlayerGameStatPercentiles.Velocity ?? 0;
        var junk = PlayerGameStatPercentiles.Junk ?? 0;
        var accuracy = PlayerGameStatPercentiles.Accuracy ?? 0;
        
        plot.Plot.Palette = Palette.DarkPastel;

        double[] values;
        double[] labelPositions;
        string[] labels;
        if (PlayerOverview.IsPitcher)
        {
            values = new[] {velocity, junk, accuracy, fielding, power, contact, speed};
            labelPositions = new[] {0D, 1, 2, 3, 4, 5, 6};
            labels = new[]
            {
                "Velocity", "Junk", "Accuracy", "Fielding", "Power", "Contact", "Speed"
            };
        }
        else
        {
            values = new[] {power, contact, speed, fielding, arm};
            labelPositions = new[] {0D, 1, 2, 3, 4};
            labels = new[] {"Power", "Contact", "Speed", "Fielding", "Arm"};
        }
        
        List<ScottPlot.Plottable.Bar> bars = new();
        for (var i = 0; i < values.Length; i++)
        {
            var value = values[i];
            
            var clampValue = (int)Math.Round(Math.Clamp(value, 0, 99));
            ScottPlot.Plottable.Bar bar = new()
            {
                Value = value,
                Position = i,
                FillColor = GetValueColor(clampValue),
                Label = value.ToString(CultureInfo.InvariantCulture),
                LineWidth = 2,
            };
            bars.Add(bar);
        }

        plot.Plot.AddBarSeries(bars);
        plot.Plot.SetAxisLimits(yMin: 0, yMax: 100);
        plot.Plot.XTicks(positions: labelPositions, labels: labels);

        plot.Height = 300;
        plot.Width = 350;
        var playerType = PlayerOverview.IsPitcher ? "Pitcher" : "Batter";
        var title = $"{playerType} Attribute Percentiles (Season {SeasonStats.SeasonNumber})";
        plot.Plot.Title(title);
        plot.Render();
    }
    
    private static Color GetValueColor(int value)
    {
        float weight;
        if (value <= 50)
        {
            weight = value / 50f;

            // Interpolate between blue and white
            var red = (int)(255 * weight);
            var green = (int)(255 * weight);
            const int blue = 255;

            return Color.FromArgb(red, green, blue);
        }
        else
        {
            weight = (value - 50) / 50f;

            // Interpolate between white and red
            const int red = 255;
            var green = (int)(255 * (1 - weight));
            var blue = (int)(255 * (1 - weight));

            return Color.FromArgb(red, green, blue);
        }
    }

    private int PlayerId { get; }
}