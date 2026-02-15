using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Maui.Devices;

namespace AppTiendaMovil;

public partial class MainPage : ContentPage
{
    private const string DefaultApiBaseUrl = "https://localhost:7219/";

    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private List<ProductDto> _allProducts = [];

    public ObservableCollection<ProductDto> Products { get; } = [];

    public ObservableCollection<CartItemViewModel> CartItems { get; } = [];

    public MainPage()
    {
        InitializeComponent();
        BindingContext = this;

        _httpClient = CreateHttpClient();
        ApiStatusLabel.Text = $"API: {_httpClient.BaseAddress}";

        UpdateCartSummary();
        _ = LoadProductsAsync();
    }

    private async Task LoadProductsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/products?includeInactive=false");
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(await ReadErrorAsync(response));
            }

            var products = await response.Content.ReadFromJsonAsync<List<ProductDto>>(_jsonOptions) ?? [];

            _allProducts = products
                .Where(p => p.IsActive)
                .OrderBy(p => p.Nombre)
                .ToList();

            ApplySearch(SearchEntry.Text);
            SyncCartWithProducts();
            UpdateCartSummary();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            await DisplayAlertAsync("Error", $"No se pudieron cargar los productos: {ex.Message}", "OK");
        }
    }

    private static HttpClient CreateHttpClient()
    {
        var apiBase = ResolveApiBaseUrl(DefaultApiBaseUrl);

        var handler = new HttpClientHandler();
        if (apiBase.Contains("localhost", StringComparison.OrdinalIgnoreCase) ||
            apiBase.Contains("10.0.2.2", StringComparison.OrdinalIgnoreCase))
        {
            handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
        }

        return new HttpClient(handler)
        {
            BaseAddress = new Uri(apiBase, UriKind.Absolute),
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    private static string ResolveApiBaseUrl(string configuredBaseUrl)
    {
        var value = configuredBaseUrl;

        if (!value.EndsWith('/'))
        {
            value += "/";
        }

        if (DeviceInfo.Platform == DevicePlatform.Android &&
            value.Contains("localhost", StringComparison.OrdinalIgnoreCase))
        {
            value = value.Replace("localhost", "10.0.2.2", StringComparison.OrdinalIgnoreCase);
        }

        return value;
    }

    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        ApplySearch(e.NewTextValue);
    }

    private async void OnRefreshClicked(object? sender, EventArgs e)
    {
        await LoadProductsAsync();
    }

    private void ApplySearch(string? query)
    {
        var term = (query ?? string.Empty).Trim();

        var filtered = _allProducts
            .Where(p => string.IsNullOrWhiteSpace(term) ||
                        p.Nombre.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                        p.Marca.Contains(term, StringComparison.OrdinalIgnoreCase))
            .ToList();

        Products.Clear();
        foreach (var product in filtered)
        {
            Products.Add(product);
        }
    }

    private async void OnAddToCartClicked(object? sender, EventArgs e)
    {
        if (sender is not Button button || button.BindingContext is not ProductDto product)
        {
            return;
        }

        if (product.CantidadStock <= 0)
        {
            await DisplayAlertAsync("Sin stock", "Este producto no tiene stock disponible.", "OK");
            return;
        }

        var existingIndex = FindCartItemIndex(product.Id);
        if (existingIndex < 0)
        {
            CartItems.Add(new CartItemViewModel
            {
                ProductId = product.Id,
                ProductName = product.Nombre,
                UnitPrice = product.Precio,
                Quantity = 1,
                StockDisponible = product.CantidadStock
            });
        }
        else
        {
            var item = CartItems[existingIndex];
            if (item.Quantity >= item.StockDisponible)
            {
                await DisplayAlertAsync("Stock maximo", "No puedes agregar mas unidades de este producto.", "OK");
                return;
            }

            CartItems[existingIndex] = item with { Quantity = item.Quantity + 1, StockDisponible = product.CantidadStock };
        }

        UpdateCartSummary();
    }

    private async void OnIncreaseCartItemClicked(object? sender, EventArgs e)
    {
        if (sender is not Button button || button.BindingContext is not CartItemViewModel item)
        {
            return;
        }

        if (item.Quantity >= item.StockDisponible)
        {
            await DisplayAlertAsync("Stock maximo", "No hay mas stock disponible para este producto.", "OK");
            return;
        }

        ReplaceCartItem(item.ProductId, item with { Quantity = item.Quantity + 1 });
        UpdateCartSummary();
    }

    private void OnDecreaseCartItemClicked(object? sender, EventArgs e)
    {
        if (sender is not Button button || button.BindingContext is not CartItemViewModel item)
        {
            return;
        }

        if (item.Quantity <= 1)
        {
            RemoveCartItem(item.ProductId);
        }
        else
        {
            ReplaceCartItem(item.ProductId, item with { Quantity = item.Quantity - 1 });
        }

        UpdateCartSummary();
    }

    private void OnRemoveCartItemClicked(object? sender, EventArgs e)
    {
        if (sender is not Button button || button.BindingContext is not CartItemViewModel item)
        {
            return;
        }

        RemoveCartItem(item.ProductId);
        UpdateCartSummary();
    }

    private async void OnCheckoutClicked(object? sender, EventArgs e)
    {
        if (CartItems.Count == 0)
        {
            await DisplayAlertAsync("Carrito", "Agrega productos antes de comprar.", "OK");
            return;
        }

        var clienteNombre = ClienteNombreEntry.Text?.Trim();
        var direccion = DireccionEntregaEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(clienteNombre) || string.IsNullOrWhiteSpace(direccion))
        {
            await DisplayAlertAsync("Datos incompletos", "Nombre y direccion son obligatorios.", "OK");
            return;
        }

        var request = new CreateOrderRequest
        {
            ClienteNombre = clienteNombre,
            ClienteTelefono = string.IsNullOrWhiteSpace(ClienteTelefonoEntry.Text) ? null : ClienteTelefonoEntry.Text.Trim(),
            DireccionEntrega = direccion,
            Notas = string.IsNullOrWhiteSpace(NotasEditor.Text) ? null : NotasEditor.Text.Trim(),
            Canal = "Movil",
            Items = CartItems
                .Select(i => new CreateOrderItemRequest
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity
                })
                .ToList()
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/orders", request, _jsonOptions);
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(await ReadErrorAsync(response));
            }

            var order = await response.Content.ReadFromJsonAsync<OrderDto>(_jsonOptions);
            CartItems.Clear();
            UpdateCartSummary();

            ClienteNombreEntry.Text = string.Empty;
            ClienteTelefonoEntry.Text = string.Empty;
            DireccionEntregaEntry.Text = string.Empty;
            NotasEditor.Text = string.Empty;

            await DisplayAlertAsync("Compra registrada", $"Orden #{order?.Id} creada. Total: {order?.Total:C}", "OK");
            await LoadProductsAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", ex.Message, "OK");
        }
    }

    private void SyncCartWithProducts()
    {
        for (var i = CartItems.Count - 1; i >= 0; i--)
        {
            var item = CartItems[i];
            var product = _allProducts.FirstOrDefault(p => p.Id == item.ProductId);

            if (product is null || product.CantidadStock <= 0)
            {
                CartItems.RemoveAt(i);
                continue;
            }

            var safeQuantity = Math.Min(item.Quantity, product.CantidadStock);
            CartItems[i] = item with
            {
                ProductName = product.Nombre,
                UnitPrice = product.Precio,
                StockDisponible = product.CantidadStock,
                Quantity = safeQuantity
            };
        }
    }

    private void UpdateCartSummary()
    {
        var totalItems = CartItems.Sum(i => i.Quantity);
        var totalPrice = CartItems.Sum(i => i.LineTotal);

        CartSummaryLabel.Text = CartItems.Count == 0
            ? "Carrito vacio"
            : $"Productos: {totalItems} | Total: {totalPrice:C}";
    }

    private int FindCartItemIndex(Guid productId)
    {
        for (var i = 0; i < CartItems.Count; i++)
        {
            if (CartItems[i].ProductId == productId)
            {
                return i;
            }
        }

        return -1;
    }

    private void ReplaceCartItem(Guid productId, CartItemViewModel updated)
    {
        var index = FindCartItemIndex(productId);
        if (index >= 0)
        {
            CartItems[index] = updated;
        }
    }

    private void RemoveCartItem(Guid productId)
    {
        var index = FindCartItemIndex(productId);
        if (index >= 0)
        {
            CartItems.RemoveAt(index);
        }
    }

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response)
    {
        try
        {
            var payload = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
            if (!string.IsNullOrWhiteSpace(payload?.Message))
            {
                return payload.Message;
            }
        }
        catch
        {
            // Ignore parse errors and return fallback.
        }

        return $"La API devolvio {(int)response.StatusCode} ({response.ReasonPhrase}).";
    }

    public sealed class ProductDto
    {
        public Guid Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Marca { get; set; } = string.Empty;

        public decimal Precio { get; set; }

        public int CantidadStock { get; set; }

        public string? ImageUrl { get; set; }

        public bool IsActive { get; set; }

        public string ImageSource => string.IsNullOrWhiteSpace(ImageUrl) ? "dotnet_bot.png" : ImageUrl;

        public string PrecioText => Precio.ToString("C");

        public string StockText => $"Stock: {CantidadStock}";
    }

    public sealed record CartItemViewModel
    {
        public Guid ProductId { get; init; }

        public string ProductName { get; init; } = string.Empty;

        public decimal UnitPrice { get; init; }

        public int Quantity { get; init; }

        public int StockDisponible { get; init; }

        public decimal LineTotal => UnitPrice * Quantity;

        public string QuantityText => $"Cantidad: {Quantity} (stock {StockDisponible})";

        public string TotalText => $"Subtotal: {LineTotal:C}";
    }

    public sealed class CreateOrderRequest
    {
        public string ClienteNombre { get; set; } = string.Empty;

        public string? ClienteTelefono { get; set; }

        public string DireccionEntrega { get; set; } = string.Empty;

        public string? Notas { get; set; }

        public string? Canal { get; set; }

        public IReadOnlyList<CreateOrderItemRequest> Items { get; set; } = [];
    }

    public sealed class CreateOrderItemRequest
    {
        public Guid ProductId { get; set; }

        public int Quantity { get; set; }
    }

    public sealed class OrderDto
    {
        public Guid Id { get; set; }

        public decimal Total { get; set; }
    }

    private sealed class ApiErrorResponse
    {
        public string? Message { get; set; }
    }
}
