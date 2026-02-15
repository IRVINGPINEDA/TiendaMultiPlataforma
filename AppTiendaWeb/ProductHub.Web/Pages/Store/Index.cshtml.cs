using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProductHub.Web.Models;
using ProductHub.Web.Services;

namespace ProductHub.Web.Pages.Store;

public class IndexModel : PageModel
{
    private readonly IProductsApiClient _apiClient;
    private readonly CartSessionStore _cartStore;

    public IndexModel(IProductsApiClient apiClient, CartSessionStore cartStore)
    {
        _apiClient = apiClient;
        _cartStore = cartStore;
    }

    public IReadOnlyList<ProductDto> Products { get; private set; } = [];

    public List<CartItemModel> Cart { get; private set; } = [];

    public int CartItemsCount => Cart.Sum(i => i.Quantity);

    public decimal CartTotal => Cart.Sum(i => i.LineTotal);

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty]
    public CheckoutInputModel Checkout { get; set; } = new();

    [TempData]
    public string? SuccessMessage { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadPageDataAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostAddAsync(Guid productId, int quantity = 1, string? search = null, CancellationToken cancellationToken = default)
    {
        Search = search;
        var products = await _apiClient.GetProductsAsync(includeInactive: false, cancellationToken);
        var product = products.FirstOrDefault(p => p.Id == productId && p.IsActive);

        if (product is null)
        {
            ErrorMessage = "El producto seleccionado ya no esta disponible.";
            return RedirectToPage(new { search });
        }

        if (product.CantidadStock <= 0)
        {
            ErrorMessage = $"No hay stock disponible para '{product.Nombre}'.";
            return RedirectToPage(new { search });
        }

        var safeQuantity = Math.Max(1, quantity);
        var cart = _cartStore.Get(HttpContext.Session).ToList();
        var existing = cart.FirstOrDefault(i => i.ProductId == productId);

        if (existing is null)
        {
            cart.Add(new CartItemModel
            {
                ProductId = product.Id,
                Nombre = product.Nombre,
                Precio = product.Precio,
                ImageUrl = product.ImageUrl,
                Quantity = Math.Min(safeQuantity, product.CantidadStock),
                StockDisponible = product.CantidadStock
            });
        }
        else
        {
            existing.Nombre = product.Nombre;
            existing.Precio = product.Precio;
            existing.ImageUrl = product.ImageUrl;
            existing.StockDisponible = product.CantidadStock;
            existing.Quantity = Math.Min(existing.Quantity + safeQuantity, product.CantidadStock);
        }

        _cartStore.Save(HttpContext.Session, cart);
        SuccessMessage = "Producto agregado al carrito.";
        return RedirectToPage(new { search });
    }

    public IActionResult OnPostUpdateAsync(Guid productId, int quantity, string? search = null)
    {
        var cart = _cartStore.Get(HttpContext.Session).ToList();
        var item = cart.FirstOrDefault(i => i.ProductId == productId);

        if (item is null)
        {
            return RedirectToPage(new { search });
        }

        if (quantity <= 0)
        {
            cart.Remove(item);
        }
        else
        {
            item.Quantity = Math.Min(quantity, Math.Max(1, item.StockDisponible));
        }

        _cartStore.Save(HttpContext.Session, cart);
        return RedirectToPage(new { search });
    }

    public IActionResult OnPostRemoveAsync(Guid productId, string? search = null)
    {
        var cart = _cartStore.Get(HttpContext.Session).ToList();
        var item = cart.FirstOrDefault(i => i.ProductId == productId);

        if (item is not null)
        {
            cart.Remove(item);
            _cartStore.Save(HttpContext.Session, cart);
        }

        return RedirectToPage(new { search });
    }

    public IActionResult OnPostClearCartAsync(string? search = null)
    {
        _cartStore.Clear(HttpContext.Session);
        return RedirectToPage(new { search });
    }

    public async Task<IActionResult> OnPostCheckoutAsync(string? search = null, CancellationToken cancellationToken = default)
    {
        Search = search;
        Cart = _cartStore.Get(HttpContext.Session).ToList();

        if (Cart.Count == 0)
        {
            ErrorMessage = "Tu carrito esta vacio.";
            return RedirectToPage(new { search });
        }

        if (!ModelState.IsValid)
        {
            await LoadProductsOnlyAsync(cancellationToken);
            return Page();
        }

        var request = new CreateOrderRequest
        {
            ClienteNombre = Checkout.ClienteNombre.Trim(),
            ClienteTelefono = string.IsNullOrWhiteSpace(Checkout.ClienteTelefono) ? null : Checkout.ClienteTelefono.Trim(),
            DireccionEntrega = Checkout.DireccionEntrega.Trim(),
            Notas = string.IsNullOrWhiteSpace(Checkout.Notas) ? null : Checkout.Notas.Trim(),
            Canal = "Web",
            Items = Cart
                .Select(item => new CreateOrderItemRequest
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                })
                .ToList()
        };

        try
        {
            var order = await _apiClient.CreateOrderAsync(request, cancellationToken);

            if (order is null)
            {
                ErrorMessage = "No se pudo crear la compra.";
                return RedirectToPage(new { search });
            }

            _cartStore.Clear(HttpContext.Session);
            SuccessMessage = $"Compra creada: #{order.Id}. Total: {order.Total:C}.";
            return RedirectToPage(new { search });
        }
        catch (InvalidOperationException ex)
        {
            ErrorMessage = ex.Message;
            return RedirectToPage(new { search });
        }
    }

    private async Task LoadPageDataAsync(CancellationToken cancellationToken)
    {
        var allProducts = await _apiClient.GetProductsAsync(includeInactive: false, cancellationToken);
        Products = FilterProducts(allProducts, Search);

        var cart = _cartStore.Get(HttpContext.Session).ToList();
        Cart = SyncCartWithCurrentProducts(cart, allProducts);
        _cartStore.Save(HttpContext.Session, Cart);
    }

    private async Task LoadProductsOnlyAsync(CancellationToken cancellationToken)
    {
        var allProducts = await _apiClient.GetProductsAsync(includeInactive: false, cancellationToken);
        Products = FilterProducts(allProducts, Search);
    }

    private static IReadOnlyList<ProductDto> FilterProducts(IEnumerable<ProductDto> products, string? search)
    {
        var query = products;

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(p => p.Nombre.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                                     p.Marca.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        return query
            .OrderBy(p => p.Nombre)
            .ToList();
    }

    private static List<CartItemModel> SyncCartWithCurrentProducts(IEnumerable<CartItemModel> cartItems, IEnumerable<ProductDto> products)
    {
        var productsMap = products.ToDictionary(p => p.Id);
        var synced = new List<CartItemModel>();

        foreach (var item in cartItems)
        {
            if (!productsMap.TryGetValue(item.ProductId, out var currentProduct))
            {
                continue;
            }

            if (currentProduct.CantidadStock <= 0)
            {
                continue;
            }

            synced.Add(new CartItemModel
            {
                ProductId = currentProduct.Id,
                Nombre = currentProduct.Nombre,
                Precio = currentProduct.Precio,
                ImageUrl = currentProduct.ImageUrl,
                StockDisponible = currentProduct.CantidadStock,
                Quantity = Math.Min(item.Quantity, currentProduct.CantidadStock)
            });
        }

        return synced;
    }
}
