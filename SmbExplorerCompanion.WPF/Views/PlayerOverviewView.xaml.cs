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
        
        PlayerGameStatsRadialPlot.Configuration.Pan = false;
        PlayerGameStatsRadialPlot.Configuration.Zoom = false;

        viewModel.DrawPlayerGameStatsPercentilePlot(PlayerGameStatsPercentilePlot);

        PlayerGameStatsPercentilePlot.Configuration.Pan = false;
        PlayerGameStatsPercentilePlot.Configuration.Zoom = false;

        viewModel.DrawPlayerKpisPercentilePlot(PlayerKpisPercentilePlot);

        PlayerKpisPercentilePlot.Configuration.Pan = false;
        PlayerKpisPercentilePlot.Configuration.Zoom = false;
    }
}