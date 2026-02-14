using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ProductHub.Web.Models;

namespace ProductHub.Web.Services;

public class ProductsApiClient : IProductsApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;

    public ProductsApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<ProductDto>> GetProductsAsync(bool includeInactive, CancellationToken cancellationToken = default)
    {
        var endpoint = $"api/products?includeInactive={includeInactive.ToString().ToLowerInvariant()}";
        return await _httpClient.GetFromJsonAsync<List<ProductDto>>(endpoint, JsonOptions, cancellationToken) ?? [];
    }

    public async Task<ProductDto?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/products/{id}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<ProductDto>(JsonOptions, cancellationToken);
    }

    public async Task<ProductDto?> CreateProductAsync(ProductUpsertRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/products", request, JsonOptions, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<ProductDto>(JsonOptions, cancellationToken);
    }

    public async Task<ProductDto?> UpdateProductAsync(Guid id, ProductUpsertRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/products/{id}", request, JsonOptions, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<ProductDto>(JsonOptions, cancellationToken);
    }

    public async Task<bool> DeleteProductAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.DeleteAsync($"api/products/{id}", cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<ProductDto?> UploadImageAsync(Guid id, IFormFile file, CancellationToken cancellationToken = default)
    {
        await using var stream = file.OpenReadStream();
        using var content = new MultipartFormDataContent();
        using var fileContent = new StreamContent(stream);

        if (!string.IsNullOrWhiteSpace(file.ContentType))
        {
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
        }

        content.Add(fileContent, "file", file.FileName);

        var response = await _httpClient.PostAsync($"api/products/{id}/image", content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var message = await ReadErrorAsync(response, cancellationToken);
            throw new InvalidOperationException(message);
        }

        return await response.Content.ReadFromJsonAsync<ProductDto>(JsonOptions, cancellationToken);
    }

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            var payload = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(JsonOptions, cancellationToken);
            if (!string.IsNullOrWhiteSpace(payload?.Message))
            {
                return payload.Message;
            }
        }
        catch
        {
            // Ignore parsing errors and fallback to generic message.
        }

        return $"La API devolvió el estado {(int)response.StatusCode}.";
    }

    private sealed class ApiErrorResponse
    {
        public string? Message { get; set; }
    }
}
