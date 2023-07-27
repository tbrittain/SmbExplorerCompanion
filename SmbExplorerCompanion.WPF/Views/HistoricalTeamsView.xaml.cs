﻿using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace SmbExplorerCompanion.WPF.Views;

public partial class HistoricalTeamsView : IDisposable
{
    public HistoricalTeamsView()
    {
        InitializeComponent();
        ScrollViewer.PreviewMouseWheel += ListViewScrollViewer_PreviewMouseWheel;
    }
    
    private void ListViewScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
    {
        var scv = (ScrollViewer)sender;

        if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)) 
        {
            // FIXME TODO
            if (e.Delta < 0)
            {
                scv.LineRight();
            }
            else if (e.Delta > 0)
            {
                scv.LineLeft();
            }
        }
        else
        {
            if (e.Delta < 0)
            {
                scv.LineDown();
            }
            else if (e.Delta > 0)
            {
                scv.LineUp();
            }
        }
    }

    public void Dispose()
    {
        ScrollViewer.PreviewMouseWheel -= ListViewScrollViewer_PreviewMouseWheel;
        GC.SuppressFinalize(this);
    }
}