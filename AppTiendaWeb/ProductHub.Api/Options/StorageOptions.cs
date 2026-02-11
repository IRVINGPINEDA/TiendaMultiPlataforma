namespace ProductHub.Api.Options;

public class StorageOptions
{
    public string UploadPath { get; set; } = "uploads/products";

    public int MaxSizeMB { get; set; } = 5;
}
