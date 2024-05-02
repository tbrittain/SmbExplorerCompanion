using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;
using MediatR;
using SmbExplorerCompanion.Core.Commands.Actions.Franchises;
using SmbExplorerCompanion.Core.Commands.Queries.Franchises;
using SmbExplorerCompanion.Core.Commands.Queries.Summary;
using SmbExplorerCompanion.Core.Interfaces;
using SmbExplorerCompanion.WPF.Extensions;
using SmbExplorerCompanion.WPF.Models;
using SmbExplorerCompanion.WPF.Services;

namespace SmbExplorerCompanion.WPF.ViewModels;

public partial class FranchiseSelectViewModel : ViewModelBase
{
    private const string LandingViewModelDialogIdentifier = nameof(FranchiseSelectViewModel);
    private readonly IApplicationContext _applicationContext;
    private readonly IMediator _mediator;
    private readonly INavigationService _navigationService;
    private Franchise? _selectedFranchise;

    public FranchiseSelectViewModel(IMediator mediator, IApplicationContext applicationContext, INavigationService navigationService)
    {
        Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = Cursors.Wait);
        _mediator = mediator;
        _applicationContext = applicationContext;
        _navigationService = navigationService;

        var franchises = _mediator.Send(new GetAllFranchisesRequest()).Result;
        Franchises = franchises
            .Select(x => x.FromCore())
            .ToObservableCollection();
        Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);
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

    [RelayCommand(CanExecute = nameof(IsFranchiseSelected))]
    private async Task LoadFranchise()
    {
        if (SelectedFranchise is null) return;
        _applicationContext.SelectedFranchiseId = SelectedFranchise.Id;
        _applicationContext.HasFranchiseData = await _mediator.Send(new GetHasFranchiseDataRequest());

        _navigationService.NavigateTo<HomeViewModel>();
    }

    [RelayCommand]
    private async Task AddFranchise()
    {
        var addFranchiseViewModel = new DialogViewModel();
        var result =
            await DialogHost.Show(addFranchiseViewModel, LandingViewModelDialogIdentifier);

        if (result is not true) return;

        var franchiseName = addFranchiseViewModel.Text;
        if (string.IsNullOrWhiteSpace(franchiseName))
        {
            MessageBox.Show("Franchise name cannot be empty.");
            return;
        }

        var franchise = await _mediator.Send(new AddFranchiseRequest(franchiseName));
        Franchises.Add(franchise.FromCore());
        SelectedFranchise = Franchises.First(x => x.Id == franchise.Id);
    }
}