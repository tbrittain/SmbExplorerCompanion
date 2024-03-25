using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using SmbExplorerCompanion.Core.Commands.Queries.Players;
using SmbExplorerCompanion.Core.Commands.Queries.Seasons;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.Core.Interfaces;
using SmbExplorerCompanion.Core.ValueObjects.Seasons;
using SmbExplorerCompanion.WPF.Extensions;
using SmbExplorerCompanion.WPF.Models.Lookups;
using SmbExplorerCompanion.WPF.Models.Players;
using SmbExplorerCompanion.WPF.Models.Seasons;
using SmbExplorerCompanion.WPF.Services;

namespace SmbExplorerCompanion.WPF.ViewModels;

public partial class TopPitchingCareersViewModel : ViewModelBase
{
    private const int ResultsPerPage = 20;
    private readonly MappingService _mappingService;
    private readonly IMediator _mediator;
    private readonly INavigationService _navigationService;
    private Season? _endSeason;
    private bool _isPlayoffs;
    private bool _onlyHallOfFamers;
    private int _pageNumber = 1;
    private ObservableCollection<Season> _selectableEndSeasons;
    private Chemistry? _selectedChemistry;
    private PitcherRole? _selectedPitcherRole;
    private PlayerPitchingCareer? _selectedPlayer;
    private ThrowHandedness? _selectedThrowHandedness;
    private Season? _startSeason;
    private bool _onlyQualifiers;

    public TopPitchingCareersViewModel(IMediator mediator,
        INavigationService navigationService,
        MappingService mappingService,
        LookupCache lookupCache)
    {
        Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = Cursors.Wait);
        _mediator = mediator;
        _navigationService = navigationService;
        _mappingService = mappingService;

        var seasons = _mediator.Send(new GetSeasonsRequest()).Result;
        Seasons.AddRange(seasons.Select(s => s.FromCore()));

        var pitcherRoles = lookupCache.GetPitcherRoles().Result;
        var allPitcherRole = new PitcherRole
        {
            Id = default,
            Name = "All"
        };
        PitcherRoles.Add(allPitcherRole);
        PitcherRoles.AddRange(pitcherRoles);
        SelectedPitcherRole = allPitcherRole;

        var chemistryTypes = lookupCache.GetChemistryTypes().Result;
        var allChemistry = new Chemistry
        {
            Id = default,
            Name = "All"
        };
        ChemistryTypes = chemistryTypes
            .Prepend(allChemistry)
            .ToObservableCollection();
        SelectedChemistry = allChemistry;

        var throwHandednessTypes = lookupCache.GetThrowHandednessTypes().Result;
        var allThrowHandedness = new ThrowHandedness
        {
            Id = default,
            Name = "All"
        };
        ThrowHandednessTypes = throwHandednessTypes
            .Prepend(allThrowHandedness)
            .ToObservableCollection();
        SelectedThrowHandedness = allThrowHandedness;

        GetTopPitchingCareers().Wait();
        PropertyChanged += OnPropertyChanged;

        Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);
    }

    public bool OnlyQualifiers
    {
        get => _onlyQualifiers;
        set => SetField(ref _onlyQualifiers, value);
    }

    public bool IsPlayoffs
    {
        get => _isPlayoffs;
        set => SetField(ref _isPlayoffs, value);
    }

    public ObservableCollection<Chemistry> ChemistryTypes { get; }

    public Chemistry? SelectedChemistry
    {
        get => _selectedChemistry;
        set => SetField(ref _selectedChemistry, value);
    }

    public ObservableCollection<ThrowHandedness> ThrowHandednessTypes { get; }

    public ThrowHandedness? SelectedThrowHandedness
    {
        get => _selectedThrowHandedness;
        set => SetField(ref _selectedThrowHandedness, value);
    }

    public ObservableCollection<Season> Seasons { get; } = new();

    public ObservableCollection<Season> SelectableEndSeasons
    {
        get => _selectableEndSeasons;
        private set => SetField(ref _selectableEndSeasons, value);
    }

    public Season? StartSeason
    {
        get => _startSeason;
        set
        {
            SetField(ref _startSeason, value);

            if (value is not null)
            {
                var endSeasons = Seasons.Where(x => x.Id >= value.Id).ToList();
                SelectableEndSeasons = new ObservableCollection<Season>(endSeasons);
                EndSeason = endSeasons.LastOrDefault();
            }
            else
            {
                EndSeason = null;
            }
        }
    }

    public Season? EndSeason
    {
        get => _endSeason;
        set => SetField(ref _endSeason, value);
    }

    public int PageNumber
    {
        get => _pageNumber;
        private set
        {
            if (value < 1) return;
            SetField(ref _pageNumber, value);

            IncrementPageCommand.NotifyCanExecuteChanged();
            DecrementPageCommand.NotifyCanExecuteChanged();
        }
    }

    public bool OnlyHallOfFamers
    {
        get => _onlyHallOfFamers;
        set => SetField(ref _onlyHallOfFamers, value);
    }

    public PlayerPitchingCareer? SelectedPlayer
    {
        get => _selectedPlayer;
        set => SetField(ref _selectedPlayer, value);
    }

    public string SortColumn { get; set; } = nameof(PlayerCareerPitchingDto.WeightedOpsPlusOrEraMinus);

    public ObservableCollection<PlayerPitchingCareer> TopPitchingCareers { get; } = new();
    public ObservableCollection<PitcherRole> PitcherRoles { get; } = new();

    public PitcherRole? SelectedPitcherRole
    {
        get => _selectedPitcherRole;
        set => SetField(ref _selectedPitcherRole, value);
    }

    private bool ShortCircuitPageNumberRefresh { get; set; }
    private bool CanSelectPreviousPage => PageNumber > 1;

    private bool CanSelectNextPage => TopPitchingCareers.Count == ResultsPerPage;

    [RelayCommand]
    private void ClearSeasons()
    {
        StartSeason = null;
        EndSeason = null;
    }

    private async void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(SelectedPlayer):
            {
                if (SelectedPlayer is not null)
                    NavigateToPlayerOverview(SelectedPlayer);
                break;
            }
            case nameof(StartSeason):
            case nameof(EndSeason):
            case nameof(SelectedPitcherRole):
            case nameof(OnlyHallOfFamers):
            case nameof(SelectedChemistry):
            case nameof(SelectedThrowHandedness):
            case nameof(IsPlayoffs):
            case nameof(OnlyQualifiers):
            {
                ShortCircuitPageNumberRefresh = true;
                PageNumber = 1;
                ShortCircuitPageNumberRefresh = false;
                await GetTopPitchingCareers();
                break;
            }
            case nameof(PageNumber):
            {
                if (!ShortCircuitPageNumberRefresh) await GetTopPitchingCareers();
                break;
            }
        }
    }

    [RelayCommand(CanExecute = nameof(CanSelectNextPage))]
    private void IncrementPage()
    {
        PageNumber++;
    }

    [RelayCommand(CanExecute = nameof(CanSelectPreviousPage))]
    private void DecrementPage()
    {
        PageNumber--;
    }

    private void NavigateToPlayerOverview(PlayerBase player)
    {
        var parameters = new Tuple<string, object>[]
        {
            new(PlayerOverviewViewModel.PlayerIdProp, player.PlayerId)
        };
        _navigationService.NavigateTo<PlayerOverviewViewModel>(parameters);
    }

    public async Task GetTopPitchingCareers()
    {
        if (StartSeason is not null && EndSeason is not null && StartSeason.Id > EndSeason.Id)
            return;

        Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = Cursors.Wait);
        SeasonRange? seasonRange = (StartSeason, EndSeason) switch
        {
            (not null, not null) => new SeasonRange(StartSeason.Id, EndSeason.Id),
            _ => null
        };

        var topPitchers = await _mediator.Send(new GetTopPitchingCareersRequest(
            new GetPitchingCareersFilters
            {
                PageNumber = PageNumber,
                Limit = ResultsPerPage,
                OrderBy = SortColumn,
                OnlyHallOfFamers = OnlyHallOfFamers,
                PitcherRoleId = SelectedPitcherRole?.Id == 0 ? null : SelectedPitcherRole?.Id,
                Seasons = seasonRange,
                ThrowHandednessId = SelectedThrowHandedness?.Id == 0 ? null : SelectedThrowHandedness?.Id,
                ChemistryId = SelectedChemistry?.Id == 0 ? null : SelectedChemistry?.Id,
                IsPlayoffs = IsPlayoffs,
                OnlyQualifiedPlayers = OnlyQualifiers
            }));

        TopPitchingCareers.Clear();
        var mappedPitchingCareers = topPitchers
            .Select(async x => await _mappingService.FromCore(x))
            .Select(x => x.Result);
        TopPitchingCareers.AddRange(mappedPitchingCareers);

        IncrementPageCommand.NotifyCanExecuteChanged();
        DecrementPageCommand.NotifyCanExecuteChanged();
        Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);
    }

    protected override void Dispose(bool disposing)
    {
        PropertyChanged -= OnPropertyChanged;
        base.Dispose(disposing);
    }
}