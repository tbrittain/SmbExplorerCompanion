using System;
using System.Linq;
using System.Windows.Controls;
using SmbExplorerCompanion.WPF.EventHandlers;
using SmbExplorerCompanion.WPF.Models.Players;
using SmbExplorerCompanion.WPF.ViewModels;

namespace SmbExplorerCompanion.WPF.Views;

public partial class DelegateHallOfFamersView : IDisposable
{
    public DelegateHallOfFamersView()
    {
        InitializeComponent();

        TopPitchingCareersDataGrid.Sorting += DataGridDefaultSortBehavior.DataGridOnSorting;
        TopBattingCareersDataGrid.Sorting += DataGridDefaultSortBehavior.DataGridOnSorting;
    }

    public void Dispose()
    {
        TopPitchingCareersDataGrid.Sorting -= DataGridDefaultSortBehavior.DataGridOnSorting;
        TopBattingCareersDataGrid.Sorting -= DataGridDefaultSortBehavior.DataGridOnSorting;

        GC.SuppressFinalize(this);
    }
}