using System;
using System.Windows;
using SmbExplorerCompanion.WPF.EventHandlers;
using SmbExplorerCompanion.WPF.ViewModels;

namespace SmbExplorerCompanion.WPF.Views;

public partial class PlayerOverviewView : IDisposable
{
    public PlayerOverviewView()
    {
        InitializeComponent();

        PlayerSeasonBattingDataGrid.Sorting += DataGridDefaultSortBehavior.DataGridOnSorting;
        PlayerPlayoffBattingDataGrid.Sorting += DataGridDefaultSortBehavior.DataGridOnSorting;
        PlayerSeasonPitchingDataGrid.Sorting += DataGridDefaultSortBehavior.DataGridOnSorting;
        PlayerPlayoffPitchingDataGrid.Sorting += DataGridDefaultSortBehavior.DataGridOnSorting;

        DataContextChanged += OnDataContextChanged;
    }

    public void Dispose()
    {
        PlayerSeasonBattingDataGrid.Sorting -= DataGridDefaultSortBehavior.DataGridOnSorting;
        PlayerPlayoffBattingDataGrid.Sorting -= DataGridDefaultSortBehavior.DataGridOnSorting;
        PlayerSeasonPitchingDataGrid.Sorting -= DataGridDefaultSortBehavior.DataGridOnSorting;
        PlayerPlayoffPitchingDataGrid.Sorting -= DataGridDefaultSortBehavior.DataGridOnSorting;
        GC.SuppressFinalize(this);
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is not PlayerOverviewViewModel viewModel) return;
        viewModel.DrawPlayerGameStatsRadialPlot(PlayerGameStatsRadialPlot);
        viewModel.DrawPlayerGameStatsPercentilePlot(PlayerGameStatsPercentilePlot);
        viewModel.DrawPlayerKpisPercentilePlot(PlayerKpisPercentilePlot);
    }
}