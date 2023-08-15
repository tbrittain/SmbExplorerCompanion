using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.Collections;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using SmbExplorerCompanion.Core.Commands.Actions.Awards;
using SmbExplorerCompanion.Core.Commands.Queries.Lookups;
using SmbExplorerCompanion.Core.Commands.Queries.Players;
using SmbExplorerCompanion.Core.Commands.Queries.Seasons;
using SmbExplorerCompanion.Core.Commands.Queries.Teams;
using SmbExplorerCompanion.Core.Interfaces;
using SmbExplorerCompanion.Core.ValueObjects.Awards;
using SmbExplorerCompanion.WPF.Extensions;
using SmbExplorerCompanion.WPF.Mappings.Lookups;
using SmbExplorerCompanion.WPF.Mappings.Players;
using SmbExplorerCompanion.WPF.Mappings.Seasons;
using SmbExplorerCompanion.WPF.Mappings.Teams;
using SmbExplorerCompanion.WPF.Models.Lookups;
using SmbExplorerCompanion.WPF.Models.Players;
using SmbExplorerCompanion.WPF.Models.Seasons;
using SmbExplorerCompanion.WPF.Models.Teams;

// ReSharper disable InconsistentNaming
namespace SmbExplorerCompanion.WPF.ViewModels;

public partial class DelegateAwardsViewModel : ViewModelBase
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
        BattingAwards.AddRange(AllAwards.Where(a => a.IsBattingAward));
        PitchingAwards.AddRange(AllAwards.Where(a => a.IsPitchingAward));
        FieldingAwards.AddRange(AllAwards.Where(a => a.IsFieldingAward));

        GetAwardNomineesForSeason().Wait();

        PropertyChanged += OnPropertyChanged;
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

        var seasonTeamMapper = new SeasonTeamMapping();
        SeasonTeams.Clear();
        SeasonTeams.AddRange(seasonTeams
            .Select(s => seasonTeamMapper.FromTeamDto(s))
            .OrderBy(x => x.TeamName));

        var topSeasonBattersResponse = await _mediator.Send(
            new GetTopBattingSeasonRequest(
                seasonId: SelectedSeason.Id,
                limit: 5,
                includeChampionAwards: false));
        if (topSeasonBattersResponse.TryPickT1(out exception, out var topSeasonBatters))
        {
            MessageBox.Show(exception.Message);
            return;
        }

        var seasonPlayerMapper = new PlayerSeasonMapping();
        TopSeasonBatters.AddRange(topSeasonBatters
            .Select(s => seasonPlayerMapper.FromBattingDto(s)));

        var topSeasonPitchersResponse = await _mediator.Send(
            new GetTopPitchingSeasonRequest(
                seasonId: SelectedSeason.Id,
                limit: 5,
                includeChampionAwards: false));

        if (topSeasonPitchersResponse.TryPickT1(out exception, out var topSeasonPitchers))
        {
            MessageBox.Show(exception.Message);
            return;
        }

        TopSeasonPitchers.AddRange(topSeasonPitchers
            .Select(s => seasonPlayerMapper.FromPitchingDto(s)));

        var topSeasonBattingRookiesResponse = await _mediator.Send(
            new GetTopBattingSeasonRequest(
                seasonId: SelectedSeason.Id,
                limit: 5,
                onlyRookies: true,
                includeChampionAwards: false));

        if (topSeasonBattingRookiesResponse.TryPickT1(out exception, out var topSeasonBattingRookies))
        {
            MessageBox.Show(exception.Message);
            return;
        }

        TopSeasonBattingRookies.AddRange(topSeasonBattingRookies
            .Select(s => seasonPlayerMapper.FromBattingDto(s)));

        var topSeasonPitchingRookiesResponse = await _mediator.Send(
            new GetTopPitchingSeasonRequest(
                seasonId: SelectedSeason.Id,
                limit: 5,
                onlyRookies: true,
                includeChampionAwards: false));

        if (topSeasonPitchingRookiesResponse.TryPickT1(out exception, out var topSeasonPitchingRookies))
        {
            MessageBox.Show(exception.Message);
            return;
        }

        TopSeasonPitchingRookies.AddRange(topSeasonPitchingRookies
            .Select(s => seasonPlayerMapper.FromPitchingDto(s)));

        foreach (var team in SeasonTeams)
        {
            var topBattersPerTeamResponse = await _mediator.Send(
                new GetTopBattingSeasonRequest(
                    seasonId: SelectedSeason.Id,
                    teamId: team.TeamId,
                    limit: 5,
                    includeChampionAwards: false));

            if (topBattersPerTeamResponse.TryPickT1(out exception, out var topBattersPerTeam))
            {
                MessageBox.Show(exception.Message);
                return;
            }

            var topBattersPerTeamObservable = topBattersPerTeam
                .Select(s => seasonPlayerMapper.FromBattingDto(s));

            TopBattersPerTeam.Add(new ObservableGroup<string, PlayerSeasonBatting>(team.TeamName, topBattersPerTeamObservable));

            var topPitchersPerTeamResponse = await _mediator.Send(
                new GetTopPitchingSeasonRequest(
                    seasonId: SelectedSeason.Id,
                    teamId: team.TeamId,
                    limit: 5,
                    includeChampionAwards: false));

            if (topPitchersPerTeamResponse.TryPickT1(out exception, out var topPitchersPerTeam))
            {
                MessageBox.Show(exception.Message);
                return;
            }

            var topPitchersPerTeamObservable = topPitchersPerTeam
                .Select(s => seasonPlayerMapper.FromPitchingDto(s));

            TopPitchersPerTeam.Add(new ObservableGroup<string, PlayerSeasonPitching>(team.TeamName, topPitchersPerTeamObservable));
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

        var request = new AddPlayerAwardsRequest(playerAwardRequestDtos, SelectedSeason.Id);
        var response = await _mediator.Send(request);
        
        if (response.TryPickT1(out var exception, out _))
        {
            MessageBox.Show("Unable to add player awards. Please try again." + exception.Message);
            return;
        }
        
        MessageBox.Show($"Player awards for season {SelectedSeason.Number} added successfully!");
    }

    public ObservableCollection<SimpleTeam> SeasonTeams { get; } = new();
    private ObservableCollection<PlayerAward> AllAwards { get; } = new();
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

    public ObservableCollection<PlayerFieldingRanking> TopFielding1B { get; } = new();
    public ObservableCollection<PlayerFieldingRanking> TopFielding2B { get; } = new();
    public ObservableCollection<PlayerFieldingRanking> TopFielding3B { get; } = new();
    public ObservableCollection<PlayerFieldingRanking> TopFieldingSS { get; } = new();
    public ObservableCollection<PlayerFieldingRanking> TopFieldingLF { get; } = new();
    public ObservableCollection<PlayerFieldingRanking> TopFieldingCF { get; } = new();
    public ObservableCollection<PlayerFieldingRanking> TopFieldingRF { get; } = new();
    public ObservableCollection<PlayerFieldingRanking> TopFieldingC { get; } = new();
    public ObservableCollection<PlayerFieldingRanking> TopFieldingP { get; } = new();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            PropertyChanged -= OnPropertyChanged;
        }

        base.Dispose(disposing);
    }
}