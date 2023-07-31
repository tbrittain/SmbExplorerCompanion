using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MediatR;
using SmbExplorerCompanion.Core.Commands.Queries.Players;
using SmbExplorerCompanion.Core.Commands.Queries.Seasons;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.Core.Interfaces;
using SmbExplorerCompanion.WPF.Extensions;
using SmbExplorerCompanion.WPF.Mappings.Players;
using SmbExplorerCompanion.WPF.Mappings.Seasons;
using SmbExplorerCompanion.WPF.Models.Players;
using SmbExplorerCompanion.WPF.Models.Seasons;

namespace SmbExplorerCompanion.WPF.ViewModels;

public class TopBattingSeasonsViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private readonly IApplicationContext _applicationContext;
    private Season? _selectedSeason;
    private int _pageNumber = 1;

    public TopBattingSeasonsViewModel(IMediator mediator, IApplicationContext applicationContext)
    {
        _mediator = mediator;
        _applicationContext = applicationContext;

        var seasonsResponse = _mediator.Send(new GetSeasonsByFranchiseRequest(
            _applicationContext.SelectedFranchiseId!.Value)).Result;

        if (seasonsResponse.TryPickT1(out var exception, out var seasons))
        {
            MessageBox.Show(exception.Message);
            return;
        }

        var seasonMapper = new SeasonMapping();
        Seasons.AddRange(seasons.Select(s => seasonMapper.FromDto(s)));

        SelectedSeason = Seasons.OrderByDescending(x => x.Number).First();
    }

    public ObservableCollection<Season> Seasons { get; } = new();

    public Season? SelectedSeason
    {
        get => _selectedSeason;
        set => SetField(ref _selectedSeason, value);
    }

    public int PageNumber
    {
        get => _pageNumber;
        set => SetField(ref _pageNumber, value);
    }

    public string SortColumn { get; set; } = nameof(PlayerBattingSeasonDto.WeightedOpsPlusOrEraMinus);

    public ObservableCollection<PlayerSeasonBatting> TopSeasonBatters { get; } = new();

    public Task GetTopBattingSeason()
    {
        var topBattersResult = _mediator.Send(new GetTopBattingSeasonRequest(
            SelectedSeason!.Id,
            false,
            PageNumber,
            SortColumn,
            true)).Result;

        if (topBattersResult.TryPickT1(out var exception, out var topBatters))
        {
            Application.Current.Dispatcher.Invoke(() => MessageBox.Show(exception.Message));
            return Task.CompletedTask;
        }

        TopSeasonBatters.Clear();

        var mapper = new PlayerSeasonMapping();
        TopSeasonBatters.AddRange(topBatters.Select(b => mapper.FromBattingDto(b)));

        return Task.CompletedTask;
    }
}