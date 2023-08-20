using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using SmbExplorerCompanion.Core.Commands.Queries.Lookups;
using SmbExplorerCompanion.Core.Commands.Queries.Players;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.WPF.Extensions;
using SmbExplorerCompanion.WPF.Mappings.Lookups;
using SmbExplorerCompanion.WPF.Mappings.Players;
using SmbExplorerCompanion.WPF.Models.Lookups;
using SmbExplorerCompanion.WPF.Models.Players;
using SmbExplorerCompanion.WPF.Services;

namespace SmbExplorerCompanion.WPF.ViewModels;

public partial class TopPitchingCareersViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private readonly INavigationService _navigationService;
    private bool _onlyHallOfFamers;
    private int _pageNumber = 1;
    private PlayerPitchingCareer? _selectedPlayer;
    private PitcherRole? _selectedPitcherRole;

    public TopPitchingCareersViewModel(IMediator mediator, INavigationService navigationService)
    {
        _mediator = mediator;
        _navigationService = navigationService;

        PropertyChanged += OnPropertyChanged;
        
        var pitcherRolesResponse = _mediator.Send(new GetAllPitcherRolesRequest()).Result;
        if (pitcherRolesResponse.TryPickT1(out var exception, out var pitcherRoles))
        {
            MessageBox.Show(exception.Message);
            return;
        }

        var allPitcherRole = new PitcherRole
        {
            Id = 0,
            Name = "All"
        };
        PitcherRoles.Add(allPitcherRole);
        var pitcherRoleMapper = new PitcherRoleMapping();
        PitcherRoles.AddRange(pitcherRoles.Select(p => pitcherRoleMapper.FromDto(p)));
        
        SelectedPitcherRole = allPitcherRole;

        GetTopPitchingCareers().Wait();
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
            case nameof(SelectedPitcherRole):
            case nameof(OnlyHallOfFamers):
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
    
    private bool ShortCircuitPageNumberRefresh { get; set; }
    
    private const int ResultsPerPage = 20;
    private bool CanSelectPreviousPage => PageNumber > 1;

    private bool CanSelectNextPage => TopPitchingCareers.Count == ResultsPerPage;

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
        var topPitchersResult = await _mediator.Send(new GetTopPitchingCareersRequest(
            pageNumber: PageNumber,
            limit: ResultsPerPage,
            orderBy: SortColumn,
            onlyHallOfFamers: OnlyHallOfFamers,
            pitcherRoleId: SelectedPitcherRole?.Id == 0 ? null : SelectedPitcherRole?.Id
        ));

        if (topPitchersResult.TryPickT1(out var exception, out var topPlayers))
        {
            Application.Current.Dispatcher.Invoke(() => MessageBox.Show(exception.Message));
            return;
        }

        TopPitchingCareers.Clear();

        var mapper = new PlayerCareerMapping();
        TopPitchingCareers.AddRange(topPlayers.Select(b => mapper.FromPitchingDto(b)));
        
        IncrementPageCommand.NotifyCanExecuteChanged();
        DecrementPageCommand.NotifyCanExecuteChanged();
    }

    protected override void Dispose(bool disposing)
    {
        PropertyChanged -= OnPropertyChanged;
        base.Dispose(disposing);
    }
}