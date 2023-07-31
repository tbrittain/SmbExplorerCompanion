using System.Collections.ObjectModel;
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

    public TopPitchingCareersViewModel(IMediator mediator, INavigationService navigationService)
    {
        _mediator = mediator;
        _navigationService = navigationService;
    }

    public int PageNumber
    {
        get => _pageNumber;
        set => SetField(ref _pageNumber, value);
    }

    public string SortColumn { get; set; } = nameof(PlayerCareerDto.WeightedOpsPlusOrEraMinus);

    public ObservableCollection<PlayerPitchingCareer> TopPitchingCareers { get; } = new();

    public Task GetTopPitchingCareers()
    {
        var topPitchersResult = _mediator.Send(new GetTopPitchingCareersRequest(
            PageNumber,
            SortColumn
        )).Result;
        if (topPitchersResult.TryPickT1(out var exception, out var topPlayers))
        {
            Application.Current.Dispatcher.Invoke(() => MessageBox.Show(exception.Message));
            return Task.CompletedTask;
        }

        TopPitchingCareers.Clear();

        var mapper = new PlayerCareerMapping();
        foreach (var player in topPlayers)
        {
            TopPitchingCareers.Add(mapper.FromPitchingDto(player));
        }

        return Task.CompletedTask;
    }
}