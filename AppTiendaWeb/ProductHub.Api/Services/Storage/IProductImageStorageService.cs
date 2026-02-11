namespace ProductHub.Api.Services.Storage;

public interface IProductImageStorageService
{
    Task<string> SaveProductImageAsync(IFormFile file, CancellationToken cancellationToken = default);
}
