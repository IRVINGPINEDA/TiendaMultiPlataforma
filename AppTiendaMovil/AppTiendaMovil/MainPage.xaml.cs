using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Diagnostics;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;

namespace AppTiendaMovil
{
    public partial class MainPage : ContentPage
    {
        public ObservableCollection<Product> Products { get; } = new ObservableCollection<Product>();
        // full list fetched from the API (unfiltered)
        private List<Product> _allProducts = new List<Product>();

        // simple in-memory cart (no server interaction)
        private List<Product> _cart = new List<Product>();

        // TODO: replace with your real API URL
        private const string ApiUrl = "https://localhost:7219/api/Products";

        public MainPage()
        {
            InitializeComponent();

            BindingContext = this;

            // load products from API
            _ = LoadProductsAsync();
        }

        private void OnSearchClicked(object? sender, EventArgs e)
        {
            // apply filter using the current search text and the include-inactive switch
            FilterProducts(SearchEntry?.Text, IncludeInactiveCheck?.IsChecked == true);
        }

        private void OnNewProductClicked(object? sender, EventArgs e)
        {
            DisplayAlert("Nuevo producto", "Navegar a la pantalla de creación (no implementado)", "OK");
        }

        private void OnEditClicked(object? sender, EventArgs e)
        {
            DisplayAlert("Editar", "Editar producto (no implementado)", "OK");
        }

        private void OnImageClicked(object? sender, EventArgs e)
        {
            DisplayAlert("Imagen", "Administrar imagen (no implementado)", "OK");
        }

        private async void OnDeleteClicked(object? sender, EventArgs e)
        {
            var ok = await DisplayAlert("Eliminar", "¿Desea eliminar este producto?", "Sí", "No");
            if (ok)
            {
                // find product from sender context
                if (sender is VisualElement ve && ve.BindingContext is Product p)
                {
                    Products.Remove(p);
                }
            }
        }

        private async void OnBuyVisibleClicked(object? sender, EventArgs e)
        {
            var visible = Products.ToList();
            if (!visible.Any())
            {
                await DisplayAlert("Carrito", "No hay productos visibles para comprar.", "OK");
                return;
            }

            // add visible products to the in-memory cart (no server call)
            _cart.AddRange(visible);

            var total = visible.Sum(p => p.Price);
            await DisplayAlert("Compra", $"Se han añadido {visible.Count} productos al carrito. Total: {total:C}", "OK");
        }

        private async void OnViewCartClicked(object? sender, EventArgs e)
        {
            if (!_cart.Any())
            {
                await DisplayAlert("Carrito", "El carrito está vacío.", "OK");
                return;
            }

            var total = _cart.Sum(p => p.Price);
            await DisplayAlert("Carrito", $"Productos en carrito: {_cart.Count}\nTotal: {total:C}", "OK");
        }

        private void FilterProducts(string? query, bool includeInactive)
        {
            var q = (query ?? string.Empty).Trim();
            var filtered = _allProducts.Where(p =>
                (includeInactive || p.IsActive) &&
                (string.IsNullOrEmpty(q) || p.Nombre.Contains(q, StringComparison.OrdinalIgnoreCase) || p.Marca.Contains(q, StringComparison.OrdinalIgnoreCase))
            ).ToList();

            Products.Clear();
            foreach (var p in filtered)
                Products.Add(p);
        }

        private async System.Threading.Tasks.Task LoadProductsAsync()
        {
            try
            {
                // prepare API URL for emulator/device
                var api = ApiUrl;
                try
                {
                    if (api.Contains("localhost", StringComparison.OrdinalIgnoreCase) && DeviceInfo.Platform == DevicePlatform.Android)
                    {
                        // Android emulator cannot reach host's localhost; use 10.0.2.2 for the default emulator
                        api = api.Replace("localhost", "10.0.2.2", StringComparison.OrdinalIgnoreCase);
                        Debug.WriteLine($"Adjusted API URL for Android emulator: {api}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error checking DeviceInfo: {ex}");
                }

                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                // create handler that can accept dev certificates for localhost (ONLY for development)
                HttpClientHandler handler = new HttpClientHandler();
                if (api.StartsWith("https://localhost", StringComparison.OrdinalIgnoreCase) || api.Contains("https://10.0.2.2"))
                {
                    handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                }

                using var client = new HttpClient(handler);

                // perform GET so we can inspect raw response when there are errors
                var resp = await client.GetAsync(api);
                var raw = await resp.Content.ReadAsStringAsync();
                Debug.WriteLine($"HTTP {resp.StatusCode} from {api}: {raw}");

                if (!resp.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"API returned {(int)resp.StatusCode} {resp.ReasonPhrase}: {raw}");
                }

                // deserialize into our local Product type
                var list = JsonSerializer.Deserialize<List<Product>>(raw, opts);
                if (list == null || list.Count == 0)
                {
                    // fallback sample when API returns nothing
                    list = new List<Product>
                    {
                        new Product {
                            Image = "dotnet_bot.png",
                            Name = "GForce RTX5090 Asus Gaming",
                            Brand = "Asus",
                            Price = 79000.00m,
                            Stock = 8,
                            IsActive = true
                        }
                    };
                }

                _allProducts = list;
                FilterProducts(SearchEntry?.Text, IncludeInactiveCheck?.IsChecked == true);
            }
            catch (Exception ex)
            {
                // if load fails, show a minimal fallback and let the user know
                Products.Clear();
                Products.Add(new Product {
                    Image = "dotnet_bot.png",
                    Name = "Error cargando productos",
                    Brand = ex.Message,
                    Price = 0m,
                    Stock = 0,
                    IsActive = false
                });

                await DisplayAlert("Error", $"No se pudo cargar la lista de productos: {ex.Message}", "OK");
            }
        }

    public class Product
    {
        // API model (Spanish) - these map to the server entity you shared
        public Guid Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public int CantidadStock { get; set; }
        public string? Descripcion { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }

        // compatibility properties used by the existing UI (English names)
        public string Image { get => ImageUrl ?? string.Empty; set => ImageUrl = value; }
        public string Name { get => Nombre; set => Nombre = value; }
        public string Brand { get => Marca; set => Marca = value; }
        public decimal Price { get => Precio; set => Precio = value; }
        public int Stock { get => CantidadStock; set => CantidadStock = value; }

        public string StatusText => IsActive ? "Activo" : "Inactivo";
        public Color StatusColor => IsActive ? Colors.Green : Colors.Gray;
    }
          
       
    }
}
