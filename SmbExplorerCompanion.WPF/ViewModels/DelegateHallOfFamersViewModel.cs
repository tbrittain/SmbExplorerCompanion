using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using SmbExplorerCompanion.Core.Commands.Actions.Awards;
using SmbExplorerCompanion.Core.Commands.Queries.Players;
using SmbExplorerCompanion.Core.Commands.Queries.Seasons;
using SmbExplorerCompanion.Core.ValueObjects.Awards;
using SmbExplorerCompanion.WPF.Extensions;
using SmbExplorerCompanion.WPF.Models.Players;
using SmbExplorerCompanion.WPF.Models.Seasons;
using SmbExplorerCompanion.WPF.Services;

namespace SmbExplorerCompanion.WPF.ViewModels;

public partial class DelegateHallOfFamersViewModel : ViewModelBase
{
    private readonly MappingService _mappingService;
    private readonly IMediator _mediator;
    private Season? _selectedSeason;

    public DelegateHallOfFamersViewModel(IMediator mediator, MappingService mappingService)
    {
        Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = Cursors.Wait);
        _mediator = mediator;
        _mappingService = mappingService;

        var seasons = _mediator.Send(new GetSeasonsRequest()).Result;
        Seasons.AddRange(seasons.Select(s => s.FromCore()));
        SelectedSeason = Seasons.OrderByDescending(x => x.Number).First();

        GetHallOfFamers().Wait();

        PropertyChanged += OnPropertyChanged;

        Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);
    }

    public ObservableCollection<Season> Seasons { get; } = new();

    public Season? SelectedSeason
    {
        get => _selectedSeason;
        set => SetField(ref _selectedSeason, value);
    }

    private bool CanSubmitHallOfFamers => TopBattingCareers.Any() || TopPitchingCareers.Any();

    public ObservableCollection<PlayerBattingCareer> TopBattingCareers { get; } = new();
    public ObservableCollection<PlayerPitchingCareer> TopPitchingCareers { get; } = new();

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(SelectedSeason):
                GetHallOfFamers().Wait();
                break;
        }
    }

    private async Task GetHallOfFamers()
    {
        Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = Cursors.Wait);
        var retiredPlayerCareerStatsDto = await _mediator.Send(new GetHallOfFameCandidatesRequest(SelectedSeason!.Id));

        TopBattingCareers.Clear();
        TopPitchingCareers.Clear();

        var mappedBattingCareers = retiredPlayerCareerStatsDto.BattingCareers
            .Select(async x => await _mappingService.FromCore(x))
            .Select(x => x.Result);
        TopBattingCareers.AddRange(mappedBattingCareers);

        var mappedPitchingCareers = retiredPlayerCareerStatsDto.PitchingCareers
            .Select(async x => await _mappingService.FromCore(x))
            .Select(x => x.Result);
        TopPitchingCareers.AddRange(mappedPitchingCareers);

        SubmitHallOfFamersCommand.NotifyCanExecuteChanged();
        Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);
    }

    [RelayCommand(CanExecute = nameof(CanSubmitHallOfFamers))]
    private async Task SubmitHallOfFamers()
    {
        if (SelectedSeason is null)
        {
            MessageBox.Show("No season selected.");
            return;
        }

        Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = Cursors.Wait);

        List<PlayerHallOfFameRequestDto> hallOfFamers = new();

        foreach (var battingCareer in TopBattingCareers)
        {
            hallOfFamers.Add(new PlayerHallOfFameRequestDto
            {
                PlayerId = battingCareer.PlayerId,
                IsHallOfFamer = battingCareer.IsHallOfFamer
            });
        }

        foreach (var pitchingCareer in TopPitchingCareers)
        {
            hallOfFamers.Add(new PlayerHallOfFameRequestDto
            {
                PlayerId = pitchingCareer.PlayerId,
                IsHallOfFamer = pitchingCareer.IsHallOfFamer
            });
        }

        await _mediator.Send(new AddHallOfFamersRequest(hallOfFamers));
        MessageBox.Show($"Eligible Hall of Famers for Season {SelectedSeason.Number} added successfully!");
    }

    protected override void Dispose(bool disposing)
    {
        PropertyChanged -= OnPropertyChanged;
        base.Dispose(disposing);
    }
}