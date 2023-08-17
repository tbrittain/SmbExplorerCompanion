using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MediatR;
using SmbExplorerCompanion.Core.Commands.Queries.Players;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.WPF.Extensions;
using SmbExplorerCompanion.WPF.Mappings.Players;
using SmbExplorerCompanion.WPF.Models.Players;
using SmbExplorerCompanion.WPF.Services;

namespace SmbExplorerCompanion.WPF.ViewModels;

public class TopBattingCareersViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private readonly INavigationService _navigationService;
    private int _pageNumber = 1;
    private PlayerBattingCareer? _selectedPlayer;

    public TopBattingCareersViewModel(INavigationService navigationService, IMediator mediator)
    {
        _navigationService = navigationService;
        _mediator = mediator;

        PropertyChanged += OnPropertyChanged;

        GetTopBattingCareers().Wait();
    }

    public int PageNumber
    {
        get => _pageNumber;
        set => SetField(ref _pageNumber, value);
    }

    public string SortColumn { get; set; } = nameof(PlayerCareerBattingDto.WeightedOpsPlusOrEraMinus);

    public PlayerBattingCareer? SelectedPlayer
    {
        get => _selectedPlayer;
        set => SetField(ref _selectedPlayer, value);
    }

    public ObservableCollection<PlayerBattingCareer> TopBattingCareers { get; } = new();

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

    public async Task GetTopBattingCareers()
    {
        var topBattersResult = await _mediator.Send(new GetTopBattingCareersRequest(
            PageNumber,
            SortColumn
        ));
        if (topBattersResult.TryPickT1(out var exception, out var topPlayers))
        {
            Application.Current.Dispatcher.Invoke(() => MessageBox.Show(exception.Message));
            return;
        }

        TopBattingCareers.Clear();

        var mapper = new PlayerCareerMapping();
        TopBattingCareers.AddRange(topPlayers.Select(b => mapper.FromBattingDto(b)));
    }

    protected override void Dispose(bool disposing)
    {
        PropertyChanged -= OnPropertyChanged;
        base.Dispose(disposing);
    }
}