using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using SmbExplorerCompanion.Core.Commands.Actions.Franchises;
using SmbExplorerCompanion.Core.Commands.Queries.Franchises;
using SmbExplorerCompanion.WPF.Extensions;
using SmbExplorerCompanion.WPF.Mappings;
using SmbExplorerCompanion.WPF.Models;
using SmbExplorerCompanion.WPF.Services;

namespace SmbExplorerCompanion.WPF.ViewModels;

public partial class LandingViewModel : ViewModelBase
{
    private const string LandingViewModelDialogIdentifier = nameof(LandingViewModel);
    private readonly IMediator _mediator;
    private readonly IApplicationContext _applicationContext;
    private readonly INavigationService _navigationService;
    private Franchise? _selectedFranchise;

    public LandingViewModel(IMediator mediator, IApplicationContext applicationContext, INavigationService navigationService)
    {
        _mediator = mediator;
        _applicationContext = applicationContext;
        _navigationService = navigationService;

        var franchiseResponse = _mediator.Send(new GetAllFranchisesRequest()).Result;
        if (franchiseResponse.TryPickT1(out var exception, out var franchises))
        {
            MessageBox.Show(exception.Message);
            Franchises = new ObservableCollection<Franchise>();
        }
        else
        {
            var mapper = new FranchiseMapping();
            Franchises = franchises
                .Select(x => mapper
                    .FranchiseDtoToFranchise(x))
                .ToObservableCollection();
        }
    }

    [RelayCommand(CanExecute = nameof(IsFranchiseSelected))]
    private void LoadFranchise()
    {
        if (SelectedFranchise is null) return;
        _applicationContext.SelectedFranchiseId = SelectedFranchise.Id;
        
        _navigationService.NavigateTo<HomeViewModel>();
    }

    public ObservableCollection<Franchise> Franchises { get; }

    public Franchise? SelectedFranchise
    {
        get => _selectedFranchise;
        set
        {
            SetField(ref _selectedFranchise, value);
            LoadFranchiseCommand.NotifyCanExecuteChanged();
        }
    }

    public bool IsFranchiseSelected => SelectedFranchise is not null;

    [RelayCommand]
    private async Task AddFranchise()
    {
        var addFranchiseViewModel = new DialogViewModel();
        var result =
            await MaterialDesignThemes.Wpf.DialogHost.Show(addFranchiseViewModel, LandingViewModelDialogIdentifier);

        if (result is not true) return;

        var franchiseName = addFranchiseViewModel.Text;
        if (string.IsNullOrWhiteSpace(franchiseName))
        {
            MessageBox.Show("Franchise name cannot be empty.");
            return;
        }

        var franchiseResponse = await _mediator.Send(new AddFranchiseRequest(franchiseName));
        if (franchiseResponse.TryPickT1(out var exception, out var franchise))
        {
            MessageBox.Show(exception.Message);
            return;
        }

        var mapper = new FranchiseMapping();
        Franchises.Add(mapper.FranchiseDtoToFranchise(franchise));
    }
}