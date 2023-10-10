namespace SmbExplorerCompanion.Shared.Constants;

public static class FileExports
{
    public static readonly string BaseSmbExplorerExportsDirectory =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SMB3Explorer");
    
    public static readonly string BaseApplicationDirectory =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SmbExplorerCompanion");

    public static readonly string LogDirectory = Path.Combine(BaseApplicationDirectory, "Logs");

    public static readonly string TempDirectory = Path.GetTempPath();
}