using System.Collections.Generic;
using System.Linq;
using System.Windows;
using MediatR;
using SmbExplorerCompanion.Core.Commands.Queries.Franchises;
using SmbExplorerCompanion.WPF.Mappings;
using SmbExplorerCompanion.WPF.Models;

namespace SmbExplorerCompanion.WPF.ViewModels;

public class LandingViewModel : ViewModelBase
{
    private readonly IMediator _mediator;

    public LandingViewModel(IMediator mediator)
    {
        _mediator = mediator;

        var franchiseResponse = _mediator.Send(new GetAllFranchisesRequest()).Result;
        if (franchiseResponse.TryPickT1(out var exception, out var franchises))
        {
            MessageBox.Show(exception.Message);
            Franchises = new List<Franchise>();
        }
        else
        {
            var mapper = new FranchiseMapping();
            Franchises = franchises
                .Select(x => mapper
                    .FranchiseDtoToFranchise(x)).ToList();
        }
    }
    
    public List<Franchise> Franchises { get; set; }
}