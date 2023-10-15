using System;
using System.Windows;
using SmbExplorerCompanion.WPF.EventHandlers;
using SmbExplorerCompanion.WPF.ViewModels;

namespace SmbExplorerCompanion.WPF.Views;

public partial class TeamSeasonDetailView : IDisposable
{
    public TeamSeasonDetailView()
    {
        InitializeComponent();
        
        SeasonBattersDataGrid.Sorting += DataGridDefaultSortBehavior.DataGridOnSorting;
        SeasonPitchersDataGrid.Sorting += DataGridDefaultSortBehavior.DataGridOnSorting;
        PlayoffBattersDataGrid.Sorting += DataGridDefaultSortBehavior.DataGridOnSorting;
        PlayoffPitchersDataGrid.Sorting += DataGridDefaultSortBehavior.DataGridOnSorting;
        
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is not TeamSeasonDetailViewModel viewModel) return;
        viewModel.DrawTeamSchedulePlot(TeamSchedulePlot);
    }

    public void Dispose()
    {
        SeasonBattersDataGrid.Sorting -= DataGridDefaultSortBehavior.DataGridOnSorting;
        SeasonPitchersDataGrid.Sorting -= DataGridDefaultSortBehavior.DataGridOnSorting;
        PlayoffBattersDataGrid.Sorting -= DataGridDefaultSortBehavior.DataGridOnSorting;
        PlayoffPitchersDataGrid.Sorting -= DataGridDefaultSortBehavior.DataGridOnSorting;
        GC.SuppressFinalize(this);
    }
}