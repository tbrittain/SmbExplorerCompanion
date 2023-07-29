using System.Threading.Tasks;
using System.Windows;
using MediatR;
using SmbExplorerCompanion.Core.Commands.Queries.Players;
using SmbExplorerCompanion.WPF.Services;

namespace SmbExplorerCompanion.WPF.ViewModels;

public class TopBattingCareersViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private readonly INavigationService _navigationService;
    private int _pageNumber = 1;

    public TopBattingCareersViewModel(INavigationService navigationService, IMediator mediator)
    {
        _navigationService = navigationService;
        _mediator = mediator;

        GetTopBattingCareers();
    }

    public int PageNumber
    {
        get => _pageNumber;
        set => SetField(ref _pageNumber, value);
    }

    private Task GetTopBattingCareers()
    {
        var topBattersResult = _mediator.Send(new GetTopBattingCareersRequest(pageNumber: PageNumber)).Result;
        if (topBattersResult.TryPickT1(out var exception, out var topPlayers))
        {
            Application.Current.Dispatcher.Invoke(() => MessageBox.Show(exception.Message));
            return Task.CompletedTask;
        }
        
        return Task.CompletedTask;
    }
}