using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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

public partial class TopPitchingSeasonsViewModel : ViewModelBase
{
    private const int ResultsPerPage = 20;
    private readonly MappingService _mappingService;
    private readonly IMediator _mediator;
    private readonly INavigationService _navigationService;
    private Season? _endSeason;
    private bool _isPlayoffs;
    private bool _onlyRookies;
    private int _pageNumber = 1;
    private ObservableCollection<Season> _selectableEndSeasons;
    private Chemistry? _selectedChemistry;
    private PitcherRole? _selectedPitcherRole;
    private PlayerSeasonBase? _selectedPlayer;
    private ThrowHandedness? _selectedThrowHandedness;
    private ObservableCollection<Trait> _selectedTraits;
    private Season? _startSeason;
    private bool _onlyQualifiers;

    public TopPitchingSeasonsViewModel(IMediator mediator,
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
        StartSeason = Seasons.OrderByDescending(x => x.Number).First();
        MinSeasonId = Seasons.OrderBy(x => x.Number).First().Id;

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

        var traits = lookupCache.GetTraits().Result;
        Traits = traits
            .Where(x => !x.IsSmb3)
            .ToObservableCollection();
        SelectedTraits = new ObservableCollection<Trait>();

        GetTopPitchingSeason().Wait();
        PropertyChanged += OnPropertyChanged;
        SelectedTraits.CollectionChanged += SelectedTraitsOnCollectionChanged;
        Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);
    }

    public bool OnlyQualifiers
    {
        get => _onlyQualifiers;
        set => SetField(ref _onlyQualifiers, value);
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

    public ObservableCollection<Trait> Traits { get; }

    public ObservableCollection<Trait> SelectedTraits
    {
        get => _selectedTraits;
        set => SetField(ref _selectedTraits, value);
    }

    public ObservableCollection<Season> Seasons { get; } = new();

    public ObservableCollection<PitcherRole> PitcherRoles { get; } = new();

    public PitcherRole? SelectedPitcherRole
    {
        get => _selectedPitcherRole;
        set => SetField(ref _selectedPitcherRole, value);
    }

    private int MinSeasonId { get; }

    public Season? StartSeason
    {
        get => _startSeason;
        set
        {
            if (value is not null && (value.Id == default || value.Id == MinSeasonId))
            {
                ShortCircuitOnlyRookiesRefresh = true;
                OnlyRookies = false;
                ShortCircuitOnlyRookiesRefresh = false;
            }

            SetField(ref _startSeason, value);
            OnPropertyChanged(nameof(CanSelectOnlyRookies));

            if (value is not null)
            {
                var endSeasons = Seasons.Where(x => x.Id >= value.Id).ToList();
                SelectableEndSeasons = new ObservableCollection<Season>(endSeasons);
                if (EndSeason is null || EndSeason.Id < value.Id)
                    EndSeason = endSeasons.LastOrDefault();
            }
            else
            {
                EndSeason = null;
            }
        }
    }

    public ObservableCollection<Season> SelectableEndSeasons
    {
        get => _selectableEndSeasons;
        private set => SetField(ref _selectableEndSeasons, value);
    }

    public Season? EndSeason
    {
        get => _endSeason;
        set
        {
            if (value is not null && value.Id != StartSeason?.Id)
            {
                ShortCircuitOnlyRookiesRefresh = true;
                OnlyRookies = false;
                ShortCircuitOnlyRookiesRefresh = false;
            }

            SetField(ref _endSeason, value);
            OnPropertyChanged(nameof(CanSelectOnlyRookies));
        }
    }

    public int PageNumber
    {
        get => _pageNumber;
        set
        {
            if (value < 1) return;
            SetField(ref _pageNumber, value);

            IncrementPageCommand.NotifyCanExecuteChanged();
            DecrementPageCommand.NotifyCanExecuteChanged();
        }
    }

    public bool IsPlayoffs
    {
        get => _isPlayoffs;
        set => SetField(ref _isPlayoffs, value);
    }

    public bool CanSelectOnlyRookies
    {
        get
        {
            if (StartSeason is null) return false;
            if (StartSeason.Id == default) return false;
            if (StartSeason.Id == MinSeasonId) return false;

            return true;
        }
    }

    public bool OnlyRookies
    {
        get => _onlyRookies;
        set => SetField(ref _onlyRookies, value);
    }

    public string SortColumn { get; set; } = nameof(PlayerPitchingSeasonDto.WeightedOpsPlusOrEraMinus);

    public ObservableCollection<PlayerSeasonPitching> TopSeasonPitchers { get; } = new();

    public PlayerSeasonBase? SelectedPlayer
    {
        get => _selectedPlayer;
        set => SetField(ref _selectedPlayer, value);
    }

    public bool ShortCircuitPageNumberRefresh { get; set; }
    private bool ShortCircuitOnlyRookiesRefresh { get; set; }

    private bool CanSelectPreviousPage => PageNumber > 1;
    private bool CanSelectNextPage => TopSeasonPitchers.Count == ResultsPerPage;

    private void SelectedTraitsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(this, new PropertyChangedEventArgs(nameof(SelectedTraits)));
    }

    [RelayCommand]
    private void ClearSeasons()
    {
        StartSeason = Seasons.OrderBy(x => x.Id).Last();
        EndSeason = Seasons.OrderBy(x => x.Id).Last();
    }

    private async void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(OnlyRookies):
            case nameof(IsPlayoffs):
            case nameof(StartSeason):
            case nameof(EndSeason):
            case nameof(SelectedPitcherRole):
            case nameof(SelectedChemistry):
            case nameof(SelectedThrowHandedness):
            case nameof(SelectedTraits):
            case nameof(OnlyQualifiers):
            {
                ShortCircuitPageNumberRefresh = true;
                PageNumber = 1;
                ShortCircuitPageNumberRefresh = false;

                if (!ShortCircuitOnlyRookiesRefresh) await GetTopPitchingSeason();
                break;
            }
            case nameof(SelectedPlayer):
            {
                if (SelectedPlayer is not null)
                    NavigateToPlayerOverview(SelectedPlayer);
                break;
            }
            case nameof(PageNumber):
            {
                if (!ShortCircuitPageNumberRefresh) await GetTopPitchingSeason();

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

    public async Task GetTopPitchingSeason()
    {
        if (StartSeason is not null && EndSeason is not null && StartSeason.Id > EndSeason.Id)
            return;

        Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = Cursors.Wait);
        var seasonRange = (StartSeason, EndSeason) switch
        {
            (null, null) => new SeasonRange(MinSeasonId),
            (not null, null) => new SeasonRange(StartSeason.Id),
            (null, not null) => new SeasonRange(MinSeasonId, EndSeason.Id),
            (not null, not null) => new SeasonRange(StartSeason.Id, EndSeason.Id)
        };

        var topPitchers = await _mediator.Send(new GetTopPitchingSeasonRequest(
            new GetPitchingSeasonsFilters
            {
                Seasons = seasonRange,
                IsPlayoffs = IsPlayoffs,
                PageNumber = PageNumber,
                OrderBy = SortColumn,
                Limit = ResultsPerPage,
                OnlyRookies = OnlyRookies,
                PitcherRoleId = SelectedPitcherRole?.Id == 0 ? null : SelectedPitcherRole?.Id,
                Descending = true,
                ChemistryId = SelectedChemistry?.Id == 0 ? null : SelectedChemistry?.Id,
                ThrowHandednessId = SelectedThrowHandedness?.Id == 0 ? null : SelectedThrowHandedness?.Id,
                TraitIds = SelectedTraits.Select(x => x.Id).ToList(),
                OnlyQualifiedPlayers = OnlyQualifiers
            }));

        TopSeasonPitchers.Clear();
        var mappedTopSeasonPitchers = topPitchers
            .Select(async x => await _mappingService.FromCore(x))
            .Select(x => x.Result);
        TopSeasonPitchers.AddRange(mappedTopSeasonPitchers);

        IncrementPageCommand.NotifyCanExecuteChanged();
        DecrementPageCommand.NotifyCanExecuteChanged();
        Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);
    }

    private void NavigateToPlayerOverview(PlayerSeasonBase player)
    {
        var parameters = new Tuple<string, object>[]
        {
            new(PlayerOverviewViewModel.PlayerIdProp, player.PlayerId),
            new(PlayerOverviewViewModel.SeasonIdProp, player.SeasonId)
        };
        _navigationService.NavigateTo<PlayerOverviewViewModel>(parameters);
    }

    protected override void Dispose(bool disposing)
    {
        PropertyChanged -= OnPropertyChanged;
        base.Dispose(disposing);
    }
}