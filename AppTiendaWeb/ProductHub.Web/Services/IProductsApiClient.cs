using ProductHub.Web.Models;

namespace ProductHub.Web.Services;

public interface IProductsApiClient
{
    Task<IReadOnlyList<ProductDto>> GetProductsAsync(bool includeInactive, CancellationToken cancellationToken = default);

    Task<ProductDto?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ProductDto?> CreateProductAsync(ProductUpsertRequest request, CancellationToken cancellationToken = default);

    Task<ProductDto?> UpdateProductAsync(Guid id, ProductUpsertRequest request, CancellationToken cancellationToken = default);

    Task<bool> DeleteProductAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ProductDto?> UploadImageAsync(Guid id, IFormFile file, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OrderDto>> GetOrdersAsync(string? estado, CancellationToken cancellationToken = default);

    Task<OrderDto?> GetOrderByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<OrderDto?> UpdateOrderStatusAsync(Guid id, UpdateOrderStatusRequest request, CancellationToken cancellationToken = default);
}
