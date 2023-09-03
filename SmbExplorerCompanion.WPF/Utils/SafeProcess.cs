using System.ComponentModel;
using System.Diagnostics;
using System.Windows;

namespace SmbExplorerCompanion.WPF.Utils;

public static class SafeProcess
{
    public static void Start(string fileName)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            UseShellExecute = true,
            CreateNoWindow = true
        };

        try
        {
            Process.Start(startInfo);
        }
        catch (Win32Exception e)
        {
            MessageBox.Show($"Failed to start process {fileName}.\n\n{e.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}