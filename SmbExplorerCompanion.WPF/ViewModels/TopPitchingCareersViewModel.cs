using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using MediatR;
using SmbExplorerCompanion.Core.Commands.Queries.Players;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.WPF.Mappings.Players;
using SmbExplorerCompanion.WPF.Models.Players;
using SmbExplorerCompanion.WPF.Services;

namespace SmbExplorerCompanion.WPF.ViewModels;

public class TopPitchingCareersViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private readonly INavigationService _navigationService;
    private int _pageNumber = 1;
    private PlayerPitchingCareer? _selectedPlayer;

    public TopPitchingCareersViewModel(IMediator mediator, INavigationService navigationService)
    {
        _mediator = mediator;
        _navigationService = navigationService;

        PropertyChanged += OnPropertyChanged;

        GetTopPitchingCareers().Wait();
    }

    public int PageNumber
    {
        get => _pageNumber;
        set => SetField(ref _pageNumber, value);
    }

    public PlayerPitchingCareer? SelectedPlayer
    {
        get => _selectedPlayer;
        set => SetField(ref _selectedPlayer, value);
    }

    public string SortColumn { get; set; } = nameof(PlayerCareerDto.WeightedOpsPlusOrEraMinus);

    public ObservableCollection<PlayerPitchingCareer> TopPitchingCareers { get; } = new();

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(SelectedPlayer):
            {
                if (SelectedPlayer is not null)
                    NavigateToPlayerOverview(SelectedPlayer);
                break;
            }
        }
    }

    private void NavigateToPlayerOverview(PlayerCareerBase player)
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
            PageNumber,
            SortColumn
        ));
        if (topPitchersResult.TryPickT1(out var exception, out var topPlayers))
        {
            Application.Current.Dispatcher.Invoke(() => MessageBox.Show(exception.Message));
            return;
        }

        TopPitchingCareers.Clear();

        var mapper = new PlayerCareerMapping();
        foreach (var player in topPlayers)
        {
            TopPitchingCareers.Add(mapper.FromPitchingDto(player));
        }
    }

    protected override void Dispose(bool disposing)
    {
        PropertyChanged -= OnPropertyChanged;
        base.Dispose(disposing);
    }
}