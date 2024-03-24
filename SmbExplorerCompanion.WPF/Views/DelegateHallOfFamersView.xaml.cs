using System;
using SmbExplorerCompanion.WPF.EventHandlers;

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