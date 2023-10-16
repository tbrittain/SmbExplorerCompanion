using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using ScottPlot.Plottable;
using SmbExplorerCompanion.WPF.EventHandlers;
using SmbExplorerCompanion.WPF.Models.Teams;
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
        TeamSchedulePlot.MouseMove += TeamSchedulePlotOnMouseMove;
    }
    
    private Tooltip? _tooltip;

    private void TeamSchedulePlotOnMouseMove(object sender, MouseEventArgs e)
    {
        if (DataContext is not TeamSeasonDetailViewModel viewModel) return;

        var position = e.GetPosition(TeamSchedulePlot);
        var (mouseX, mouseY) = TeamSchedulePlot.Plot.GetCoordinate((float)position.X, (float)position.Y);

        // Define a threshold for how close the cursor must be to a data point to show the tooltip
        const double thresholdX = 1; // Example value, adjust as needed
        const double thresholdY = 0.5; // Example value, adjust as needed

        // Iterate over all data points and find the one nearest to the mouse
        // Note: This is a simple linear search. For large datasets, a more efficient search strategy might be needed.
        TeamScheduleBreakdown? nearestPoint = null;
        var nearestDistance = double.MaxValue;
        
        var breakdowns = viewModel.TeamScheduleBreakdowns;
        if (!viewModel.IncludeDivisionTeamsInPlot)
        {
            breakdowns = breakdowns
                .Where(b => b
                    .All(x => x.TeamHistoryId == viewModel.TeamSeasonId))
                .ToList();
        }
        
        foreach (var breakdown in breakdowns.SelectMany(x => x))
        {
            var distanceX = Math.Abs(breakdown.Day - mouseX);
            var distanceY = Math.Abs(breakdown.WinsDelta - mouseY);
            var euclideanDistance = Math.Sqrt(distanceX * distanceX + distanceY * distanceY);

            if (distanceX < thresholdX && distanceY < thresholdY && euclideanDistance < nearestDistance)
            {
                nearestDistance = euclideanDistance;
                nearestPoint = breakdown;
            }
        }

        if (nearestPoint != null)
        {
            if (_tooltip is not null)
            {
                TeamSchedulePlot.Plot.Remove(_tooltip);
                _tooltip = null;
            }

            var wasWin = nearestPoint.TeamScore > nearestPoint.OpponentTeamScore ? "win" : "lose";
            var annotationText = $"Game {nearestPoint.Day}: {nearestPoint.TeamName} {wasWin} against {nearestPoint.OpponentTeamName} {nearestPoint.TeamScore} - {nearestPoint.OpponentTeamScore}";
            _tooltip = TeamSchedulePlot.Plot.AddTooltip(annotationText, nearestPoint.Day, nearestPoint.WinsDelta);

            // Redraw the plot to show the new annotation
            TeamSchedulePlot.Plot.Render();
        }
        else
        {
            if (_tooltip is not null)
            {
                TeamSchedulePlot.Plot.Remove(_tooltip);
                _tooltip = null;
            }
            TeamSchedulePlot.Plot.Render();
        }
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