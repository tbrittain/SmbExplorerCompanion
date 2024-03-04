using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Collections;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using SmbExplorerCompanion.Core.Commands.Actions.Awards;
using SmbExplorerCompanion.Core.Commands.Queries.Lookups;
using SmbExplorerCompanion.Core.Commands.Queries.Players;
using SmbExplorerCompanion.Core.Commands.Queries.Seasons;
using SmbExplorerCompanion.Core.Commands.Queries.Teams;
using SmbExplorerCompanion.Core.ValueObjects.Awards;
using SmbExplorerCompanion.WPF.Extensions;
using SmbExplorerCompanion.WPF.Models.Lookups;
using SmbExplorerCompanion.WPF.Models.Players;
using SmbExplorerCompanion.WPF.Models.Seasons;
using SmbExplorerCompanion.WPF.Models.Teams;
using SmbExplorerCompanion.WPF.Services;

namespace SmbExplorerCompanion.WPF.ViewModels;

public partial class DelegateAwardsViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private readonly MappingService _mappingService;
    private Season? _selectedSeason;

    public DelegateAwardsViewModel(IMediator mediator, MappingService mappingService)
    {
        Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = Cursors.Wait);
        _mediator = mediator;
        _mappingService = mappingService;

        var seasonsResponse = _mediator.Send(new GetSeasonsRequest()).Result;
        if (seasonsResponse.TryPickT1(out var exception, out var seasons))
        {
            MessageBox.Show(exception.Message);
            Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);
            return;
        }

        Seasons.Add(new Season
        {
            Id = default
        });
        Seasons.AddRange(seasons.Select(s => s.FromCore()));
        SelectedSeason = Seasons.OrderByDescending(x => x.Number).First();

        var regularSeasonAwards = _mediator.Send(GetPlayerAwardsRequest.Default).Result;
        if (regularSeasonAwards.TryPickT1(out exception, out var awards))
        {
            MessageBox.Show(exception.Message);
            Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);
            return;
        }

        AllAwards.AddRange(awards.Select(a => a.FromCore()));
        BattingAwards.AddRange(AllAwards.Where(a => a.IsBattingAward));
        PitchingAwards.AddRange(AllAwards.Where(a => a.IsPitchingAward));
        FieldingAwards.AddRange(AllAwards.Where(a => a.IsFieldingAward));
        
        var positionsResponse = _mediator.Send(new GetPositionsRequest()).Result;
        if (positionsResponse.TryPickT1(out exception, out var positions))
        {
            MessageBox.Show(exception.Message);
            Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);
            return;
        }

        Positions.AddRange(positions
            .Where(x => x.IsPrimaryPosition)
            .Select(p => p.FromCore()));

        GetAwardNomineesForSeason().Wait();

        PropertyChanged += OnPropertyChanged;
        
        Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);
    }

    private async void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(SelectedSeason):
            {
                if (SelectedSeason is not null)
                    await GetAwardNomineesForSeason();
                break;
            }
        }
    }

    private async Task GetAwardNomineesForSeason()
    {
        if (SelectedSeason is null)
        {
            MessageBox.Show("No season selected.");
            return;
        }

        var seasonTeamsResponse = await _mediator.Send(
            new GetSeasonTeamsRequest(SelectedSeason.Id));

        if (seasonTeamsResponse.TryPickT1(out var exception, out var seasonTeams))
        {
            MessageBox.Show(exception.Message);
            return;
        }

        SeasonTeams.Clear();
        SeasonTeams.AddRange(seasonTeams
            .Select(s => s.FromCore())
            .OrderBy(x => x.TeamName));

        var atLeastOneUserAwardAdded = false;
        var topSeasonBattersResponse = await _mediator.Send(
            new GetTopBattingSeasonRequest(
                seasonId: SelectedSeason.Id,
                limit: 10,
                includeChampionAwards: false,
                onlyUserAssignableAwards: true));
        if (topSeasonBattersResponse.TryPickT1(out exception, out var topSeasonBatters))
        {
            MessageBox.Show(exception.Message);
            return;
        }

        TopSeasonBatters.Clear();
        var mappedTopBatters = topSeasonBatters
            .Select(async x => await _mappingService.FromCore(x))
            .Select(x => x.Result);
        TopSeasonBatters.AddRange(mappedTopBatters);
        
        if (TopSeasonBatters.Any(x => x.Awards.Any()))
            atLeastOneUserAwardAdded = true;

        var topSeasonPitchersResponse = await _mediator.Send(
            new GetTopPitchingSeasonRequest(
                seasonId: SelectedSeason.Id,
                limit: 10,
                includeChampionAwards: false,
                onlyUserAssignableAwards: true));

        if (topSeasonPitchersResponse.TryPickT1(out exception, out var topSeasonPitchers))
        {
            MessageBox.Show(exception.Message);
            return;
        }

        TopSeasonPitchers.Clear();
        var mappedTopPitchers = topSeasonPitchers
            .Select(async x => await _mappingService.FromCore(x))
            .Select(x => x.Result);
        TopSeasonPitchers.AddRange(mappedTopPitchers);
        if (TopSeasonPitchers.Any(x => x.Awards.Any()))
            atLeastOneUserAwardAdded = true;

        var topSeasonBattingRookiesResponse = await _mediator.Send(
            new GetTopBattingSeasonRequest(
                seasonId: SelectedSeason.Id,
                limit: 10,
                onlyRookies: true,
                includeChampionAwards: false,
                onlyUserAssignableAwards: true));

        if (topSeasonBattingRookiesResponse.TryPickT1(out exception, out var topSeasonBattingRookies))
        {
            MessageBox.Show(exception.Message);
            return;
        }

        TopSeasonBattingRookies.Clear();
        var mappedRookieBatters = topSeasonBattingRookies
            .Select(async x => await _mappingService.FromCore(x))
            .Select(x => x.Result);
        TopSeasonBattingRookies.AddRange(mappedRookieBatters);

        if (TopSeasonBattingRookies.Any(x => x.Awards.Any()))
            atLeastOneUserAwardAdded = true;

        var topSeasonPitchingRookiesResponse = await _mediator.Send(
            new GetTopPitchingSeasonRequest(
                seasonId: SelectedSeason.Id,
                limit: 10,
                onlyRookies: true,
                includeChampionAwards: false,
                onlyUserAssignableAwards: true));

        if (topSeasonPitchingRookiesResponse.TryPickT1(out exception, out var topSeasonPitchingRookies))
        {
            MessageBox.Show(exception.Message);
            return;
        }

        TopSeasonPitchingRookies.Clear();
        var mappedRookiePitchers = topSeasonPitchingRookies
            .Select(async x => await _mappingService.FromCore(x))
            .Select(x => x.Result);
        TopSeasonPitchingRookies.AddRange(mappedRookiePitchers);
        if (TopSeasonPitchingRookies.Any(x => x.Awards.Any()))
            atLeastOneUserAwardAdded = true;

        TopBattersPerTeam.Clear();
        TopPitchersPerTeam.Clear();
        foreach (var team in SeasonTeams)
        {
            var topBattersPerTeamResponse = await _mediator.Send(
                new GetTopBattingSeasonRequest(
                    seasonId: SelectedSeason.Id,
                    teamId: team.TeamId,
                    limit: 5,
                    includeChampionAwards: false,
                    onlyUserAssignableAwards: true));

            if (topBattersPerTeamResponse.TryPickT1(out exception, out var topBattersPerTeam))
            {
                MessageBox.Show(exception.Message);
                return;
            }

            var topBattersPerTeamObservable = topBattersPerTeam
                .Select(async x => await _mappingService.FromCore(x))
                .Select(x => x.Result)
                .ToList();
            
            // Automate the process of suggesting all-stars. We will use a proxy of if ANY awards have been added to the
            // overall players, then we will not add all-stars. Otherwise, we do it, as a way to save time.

            var allStarAward = AllAwards.First(x => x.OriginalName == "All-Star");
            if (!atLeastOneUserAwardAdded)
            {
                // Only assign to the top two by default
                foreach (var player in topBattersPerTeamObservable.Take(2))
                {
                    player.Awards.Add(allStarAward);
                }
            }

            TopBattersPerTeam.Add(new ObservableGroup<string, PlayerSeasonBatting>(team.TeamName, topBattersPerTeamObservable));

            var topPitchersPerTeamResponse = await _mediator.Send(
                new GetTopPitchingSeasonRequest(
                    seasonId: SelectedSeason.Id,
                    teamId: team.TeamId,
                    limit: 5,
                    includeChampionAwards: false,
                    onlyUserAssignableAwards: true));

            if (topPitchersPerTeamResponse.TryPickT1(out exception, out var topPitchersPerTeam))
            {
                MessageBox.Show(exception.Message);
                return;
            }

            var topPitchersPerTeamObservable = topPitchersPerTeam
                .Select(async x => await _mappingService.FromCore(x))
                .Select(x => x.Result)
                .ToList();

            if (!atLeastOneUserAwardAdded)
            {
                foreach (var player in topPitchersPerTeamObservable.Take(2))
                {
                    player.Awards.Add(allStarAward);
                }
            }

            TopPitchersPerTeam.Add(new ObservableGroup<string, PlayerSeasonPitching>(team.TeamName, topPitchersPerTeamObservable));
        }

        TopBattersByPosition.Clear();
        foreach (var position in Positions)
        {
            var topBattersByPositionResponse = await _mediator.Send(
                new GetTopBattingSeasonRequest(
                    seasonId: SelectedSeason.Id,
                    primaryPositionId: position.Id,
                    limit: 5,
                    includeChampionAwards: false,
                    onlyUserAssignableAwards: true));

            if (topBattersByPositionResponse.TryPickT1(out exception, out var topBattersByPosition))
            {
                MessageBox.Show(exception.Message);
                return;
            }

            var topBattersByPositionObservable = topBattersByPosition
                .Select(async x => await _mappingService.FromCore(x))
                .Select(x => x.Result)
                .ToList();

            if (!atLeastOneUserAwardAdded)
            {
                foreach (var player in topBattersByPositionObservable.Take(1))
                {
                    player.Awards.Add(AllAwards.First(x => x.OriginalName == "Silver Slugger"));
                }
            }

            TopBattersByPosition.Add(new ObservableGroup<string, PlayerSeasonBatting>(position.Name, topBattersByPositionObservable));
        }

        // TODO: Fielding
    }

    [RelayCommand]
    private async Task SubmitAwardees()
    {
        if (SelectedSeason is null)
        {
            MessageBox.Show("No season selected.");
            return;
        }
        
        Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = Cursors.Wait);

        List<PlayerAwardRequestDto> playerAwardRequestDtos = new();

        // Reduce all of the players in every collection who have assigned awards into a single list
        foreach (var topSeasonBatter in TopSeasonBatters)
        {
            if (topSeasonBatter.Awards.Any())
            {
                playerAwardRequestDtos.AddRange(topSeasonBatter.Awards
                    .Select(award => new PlayerAwardRequestDto
                    {
                        PlayerId = topSeasonBatter.PlayerId,
                        AwardId = award.Id
                    }));
            }
        }

        foreach (var topSeasonPitcher in TopSeasonPitchers)
        {
            if (topSeasonPitcher.Awards.Any())
            {
                playerAwardRequestDtos.AddRange(topSeasonPitcher.Awards
                    .Select(award => new PlayerAwardRequestDto
                    {
                        PlayerId = topSeasonPitcher.PlayerId,
                        AwardId = award.Id
                    }));
            }
        }

        foreach (var topSeasonBattingRookie in TopSeasonBattingRookies)
        {
            if (topSeasonBattingRookie.Awards.Any())
            {
                playerAwardRequestDtos.AddRange(topSeasonBattingRookie.Awards
                    .Select(award => new PlayerAwardRequestDto
                    {
                        PlayerId = topSeasonBattingRookie.PlayerId,
                        AwardId = award.Id
                    }));
            }
        }

        foreach (var topSeasonPitchingRookie in TopSeasonPitchingRookies)
        {
            if (topSeasonPitchingRookie.Awards.Any())
            {
                playerAwardRequestDtos.AddRange(topSeasonPitchingRookie.Awards
                    .Select(award => new PlayerAwardRequestDto
                    {
                        PlayerId = topSeasonPitchingRookie.PlayerId,
                        AwardId = award.Id
                    }));
            }
        }

        foreach (var topBattersPerTeam in TopBattersPerTeam)
        {
            foreach (var topBatter in topBattersPerTeam)
            {
                if (topBatter.Awards.Any())
                {
                    playerAwardRequestDtos.AddRange(topBatter.Awards
                        .Select(award => new PlayerAwardRequestDto
                        {
                            PlayerId = topBatter.PlayerId,
                            AwardId = award.Id
                        }));
                }
            }
        }

        foreach (var topPitchersPerTeam in TopPitchersPerTeam)
        {
            foreach (var topPitcher in topPitchersPerTeam)
            {
                if (topPitcher.Awards.Any())
                {
                    playerAwardRequestDtos.AddRange(topPitcher.Awards
                        .Select(award => new PlayerAwardRequestDto
                        {
                            PlayerId = topPitcher.PlayerId,
                            AwardId = award.Id
                        }));
                }
            }
        }

        foreach (var topBattersPerPosition in TopBattersByPosition)
        {
            foreach (var topBatter in topBattersPerPosition)
            {
                if (topBatter.Awards.Any())
                {
                    playerAwardRequestDtos.AddRange(topBatter.Awards
                        .Select(award => new PlayerAwardRequestDto
                        {
                            PlayerId = topBatter.PlayerId,
                            AwardId = award.Id
                        }));
                }
            }
        }

        var request = new AddPlayerAwardsRequest(playerAwardRequestDtos.Distinct().ToList(), SelectedSeason.Id);
        var response = await _mediator.Send(request);
        
        Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);

        if (response.TryPickT1(out var exception, out _))
        {
            MessageBox.Show("Unable to add player awards. Please try again. " + exception.Message);
            return;
        }

        MessageBox.Show($"Player awards for Season {SelectedSeason.Number} added successfully!");
    }

    private ObservableCollection<SimpleTeam> SeasonTeams { get; } = new();
    private ObservableCollection<PlayerAward> AllAwards { get; } = new();
    private List<Position> Positions { get; } = new();
    public ObservableCollection<PlayerAward> BattingAwards { get; } = new();
    public ObservableCollection<PlayerAward> PitchingAwards { get; } = new();
    public ObservableCollection<PlayerAward> FieldingAwards { get; } = new();

    public ObservableCollection<Season> Seasons { get; } = new();

    public Season? SelectedSeason
    {
        get => _selectedSeason;
        set => SetField(ref _selectedSeason, value);
    }

    public ObservableCollection<PlayerSeasonBatting> TopSeasonBatters { get; } = new();
    public ObservableCollection<PlayerSeasonPitching> TopSeasonPitchers { get; } = new();
    public ObservableCollection<PlayerSeasonBatting> TopSeasonBattingRookies { get; } = new();
    public ObservableCollection<PlayerSeasonPitching> TopSeasonPitchingRookies { get; } = new();
    public ObservableGroupedCollection<string, PlayerSeasonBatting> TopBattersPerTeam { get; } = new();
    public ObservableGroupedCollection<string, PlayerSeasonPitching> TopPitchersPerTeam { get; } = new();
    public ObservableGroupedCollection<string, PlayerSeasonBatting> TopBattersByPosition { get; } = new();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            PropertyChanged -= OnPropertyChanged;
        }

        base.Dispose(disposing);
    }
}