using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.Collections;
using MediatR;
using SmbExplorerCompanion.Core.Commands.Queries.Lookups;
using SmbExplorerCompanion.Core.Commands.Queries.Players;
using SmbExplorerCompanion.Core.Commands.Queries.Seasons;
using SmbExplorerCompanion.Core.Commands.Queries.Teams;
using SmbExplorerCompanion.Core.Interfaces;
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
        SeasonTeams.AddRange(seasonTeams.Select(s => seasonTeamMapper.FromTeamDto(s)));

        var topSeasonBattersResponse = await _mediator.Send(
            new GetTopBattingSeasonRequest(
                seasonId: SelectedSeason.Id,
                limit: 5));
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
                limit: 5));

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
                onlyRookies: true));

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
                onlyRookies: true));

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
                    limit: 5));

            if (topBattersPerTeamResponse.TryPickT1(out exception, out var topBattersPerTeam))
            {
                MessageBox.Show(exception.Message);
                return;
            }

            var topBattersPerTeamObservable = topBattersPerTeam
                .Select(s => seasonPlayerMapper.FromBattingDto(s));

            TopBattersPerTeam.Add(new ObservableGroup<int, PlayerSeasonBatting>(team.TeamId, topBattersPerTeamObservable));
            
            var topPitchersPerTeamResponse = await _mediator.Send(
                new GetTopPitchingSeasonRequest(
                    seasonId: SelectedSeason.Id,
                    teamId: team.TeamId,
                    limit: 5));

            if (topPitchersPerTeamResponse.TryPickT1(out exception, out var topPitchersPerTeam))
            {
                MessageBox.Show(exception.Message);
                return;
            }

            var topPitchersPerTeamObservable = topPitchersPerTeam
                .Select(s => seasonPlayerMapper.FromPitchingDto(s));
            
            TopPitchersPerTeam.Add(new ObservableGroup<int, PlayerSeasonPitching>(team.TeamId, topPitchersPerTeamObservable));
        }

        // TODO: Fielding
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
    public ObservableGroupedCollection<int, PlayerSeasonBatting> TopBattersPerTeam { get; } = new();
    public ObservableGroupedCollection<int, PlayerSeasonPitching> TopPitchersPerTeam { get; } = new();

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