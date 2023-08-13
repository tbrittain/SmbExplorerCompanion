using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MediatR;
using SmbExplorerCompanion.Core.Commands.Queries.Lookups;
using SmbExplorerCompanion.Core.Commands.Queries.Players;
using SmbExplorerCompanion.Core.Commands.Queries.Seasons;
using SmbExplorerCompanion.Core.Interfaces;
using SmbExplorerCompanion.WPF.Extensions;
using SmbExplorerCompanion.WPF.Mappings.Lookups;
using SmbExplorerCompanion.WPF.Mappings.Players;
using SmbExplorerCompanion.WPF.Mappings.Seasons;
using SmbExplorerCompanion.WPF.Models.Lookups;
using SmbExplorerCompanion.WPF.Models.Players;
using SmbExplorerCompanion.WPF.Models.Seasons;
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

        GetAwardNominees().Wait();
    }

    private async Task GetAwardNominees()
    {
        if (SelectedSeason is null)
        {
            MessageBox.Show("No season selected.");
            return;
        }
        
        var topSeasonBattersResponse = await _mediator.Send(
            new GetTopBattingSeasonRequest(
                seasonId: SelectedSeason.Id,
                limit: 5));
        if (topSeasonBattersResponse.TryPickT1(out var exception, out var topSeasonBatters))
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
    }

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

    public ObservableCollection<PlayerFieldingRanking> TopFielding1B { get; } = new();
    public ObservableCollection<PlayerFieldingRanking> TopFielding2B { get; } = new();
    public ObservableCollection<PlayerFieldingRanking> TopFielding3B { get; } = new();
    public ObservableCollection<PlayerFieldingRanking> TopFieldingSS { get; } = new();
    public ObservableCollection<PlayerFieldingRanking> TopFieldingLF { get; } = new();
    public ObservableCollection<PlayerFieldingRanking> TopFieldingCF { get; } = new();
    public ObservableCollection<PlayerFieldingRanking> TopFieldingRF { get; } = new();
    public ObservableCollection<PlayerFieldingRanking> TopFieldingC { get; } = new();
    public ObservableCollection<PlayerFieldingRanking> TopFieldingP { get; } = new();
}