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
using SmbExplorerCompanion.Core.Interfaces;
using SmbExplorerCompanion.WPF.Extensions;
using SmbExplorerCompanion.WPF.Mappings;
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

        var franchiseResponse = _mediator.Send(new GetAllFranchisesRequest()).Result;
        if (franchiseResponse.TryPickT1(out var exception, out var franchises))
        {
            MessageBox.Show(exception.Message);
            Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);
            Franchises = new ObservableCollection<Franchise>();
        }
        else
        {
            var mapper = new FranchiseMapping();
            Franchises = franchises
                .Select(x => mapper
                    .FromDto(x))
                .ToObservableCollection();
            Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);
        }
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
    private void LoadFranchise()
    {
        if (SelectedFranchise is null) return;
        _applicationContext.SelectedFranchiseId = SelectedFranchise.Id;

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

        var franchiseResponse = await _mediator.Send(new AddFranchiseRequest(franchiseName));
        if (franchiseResponse.TryPickT1(out var exception, out var franchise))
        {
            MessageBox.Show(exception.Message);
            return;
        }

        var mapper = new FranchiseMapping();
        Franchises.Add(mapper.FromDto(franchise));
        SelectedFranchise = Franchises.First(x => x.Id == franchise.Id);
    }
}