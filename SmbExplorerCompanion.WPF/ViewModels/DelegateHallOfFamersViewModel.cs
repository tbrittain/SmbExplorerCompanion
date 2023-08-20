using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MediatR;
using SmbExplorerCompanion.Core.Commands.Queries.Players;
using SmbExplorerCompanion.Core.Commands.Queries.Seasons;
using SmbExplorerCompanion.Core.Interfaces;
using SmbExplorerCompanion.WPF.Extensions;
using SmbExplorerCompanion.WPF.Mappings.Players;
using SmbExplorerCompanion.WPF.Mappings.Seasons;
using SmbExplorerCompanion.WPF.Models.Players;
using SmbExplorerCompanion.WPF.Models.Seasons;

namespace SmbExplorerCompanion.WPF.ViewModels;

public class DelegateHallOfFamersViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private Season? _selectedSeason;

    public DelegateHallOfFamersViewModel(IMediator mediator, IApplicationContext applicationContext)
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

        Seasons.AddRange(seasons.Select(s => seasonMapper.FromDto(s)));
        SelectedSeason = Seasons.OrderByDescending(x => x.Number).First();

        GetHallOfFamers().Wait();

        PropertyChanged += OnPropertyChanged;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(SelectedSeason):
                GetHallOfFamers().Wait();
                break;
        }
    }

    public ObservableCollection<Season> Seasons { get; } = new();

    public Season? SelectedSeason
    {
        get => _selectedSeason;
        set => SetField(ref _selectedSeason, value);
    }

    private async Task GetHallOfFamers()
    {
        var response = await _mediator.Send(new GetHallOfFameCandidatesRequest(SelectedSeason!.Id));

        if (response.TryPickT2(out var exception, out var rest))
        {
            MessageBox.Show(exception.Message);
            return;
        }

        TopBattingCareers.Clear();
        TopPitchingCareers.Clear();

        if (rest.TryPickT1(out _, out var retiredPlayers)) return;

        var battingMapper = new PlayerCareerMapping();
        TopBattingCareers.AddRange(retiredPlayers.BattingCareers
            .Select(b => battingMapper.FromBattingDto(b)));

        var pitchingMapper = new PlayerCareerMapping();
        TopPitchingCareers.AddRange(retiredPlayers.PitchingCareers
            .Select(p => pitchingMapper.FromPitchingDto(p)));
    }

    public ObservableCollection<PlayerBattingCareer> TopBattingCareers { get; } = new();
    public ObservableCollection<PlayerPitchingCareer> TopPitchingCareers { get; } = new();

    protected override void Dispose(bool disposing)
    {
        PropertyChanged -= OnPropertyChanged;
        base.Dispose(disposing);
    }
}