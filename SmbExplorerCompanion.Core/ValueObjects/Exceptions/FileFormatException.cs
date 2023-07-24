namespace SmbExplorerCompanion.Core.ValueObjects.Exceptions;

public class FileFormatException : Exception
{
    public FileFormatException(string message)
    {
        Message = message;
    }

    override public string Message { get; }
}