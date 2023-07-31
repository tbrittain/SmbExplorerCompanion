using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using MediatR;
using SmbExplorerCompanion.Core.Commands.Queries.Seasons;
using SmbExplorerCompanion.Core.Interfaces;
using SmbExplorerCompanion.WPF.Extensions;
using SmbExplorerCompanion.WPF.Mappings.Seasons;
using SmbExplorerCompanion.WPF.Models.Seasons;

namespace SmbExplorerCompanion.WPF.ViewModels;

public class TopBattingSeasonsViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private readonly IApplicationContext _applicationContext;
    private Season? _selectedSeason;

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
}