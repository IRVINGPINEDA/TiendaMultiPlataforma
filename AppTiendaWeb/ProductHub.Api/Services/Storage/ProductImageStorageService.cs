using Microsoft.Extensions.Options;
using ProductHub.Api.Options;

namespace ProductHub.Api.Services.Storage;

public class ProductImageStorageService : IProductImageStorageService
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    };

    private static readonly HashSet<string> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp"
    };

    private readonly IWebHostEnvironment _environment;
    private readonly StorageOptions _options;

    public ProductImageStorageService(IWebHostEnvironment environment, IOptions<StorageOptions> options)
    {
        _environment = environment;
        _options = options.Value;
    }

    public async Task<string> SaveProductImageAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
        {
            throw new FileStorageException("Debe seleccionar una imagen válida.");
        }

        var maxBytes = _options.MaxSizeMB * 1024 * 1024;
        if (file.Length > maxBytes)
        {
            throw new FileStorageException($"La imagen excede el tamaño máximo de {_options.MaxSizeMB} MB.");
        }

        var extension = Path.GetExtension(file.FileName);
        if (!AllowedExtensions.Contains(extension) || !AllowedMimeTypes.Contains(file.ContentType))
        {
            throw new FileStorageException("Formato de imagen inválido. Solo se permiten JPG, PNG o WEBP.");
        }

        var webRootPath = _environment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRootPath))
        {
            webRootPath = Path.Combine(_environment.ContentRootPath, "wwwroot");
        }

        var normalizedUploadPath = _options.UploadPath.Trim('/').Replace('/', Path.DirectorySeparatorChar);
        var targetFolder = Path.Combine(webRootPath, normalizedUploadPath);
        Directory.CreateDirectory(targetFolder);

        var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var fullPath = Path.Combine(targetFolder, fileName);

        await using var stream = File.Create(fullPath);
        await file.CopyToAsync(stream, cancellationToken);

        var publicPath = $"/{_options.UploadPath.Trim('/').Replace('\\', '/')}/{fileName}";
        return publicPath;
    }
}
