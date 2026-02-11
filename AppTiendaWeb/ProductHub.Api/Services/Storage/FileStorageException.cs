namespace ProductHub.Api.Services.Storage;

public class FileStorageException : Exception
{
    public FileStorageException(string message)
        : base(message)
    {
    }
}
