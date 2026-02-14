using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProductHub.Web.Models;
using ProductHub.Web.Services;

namespace ProductHub.Web.Pages.Products;

public class CreateModel : PageModel
{
    private readonly IProductsApiClient _apiClient;

    public CreateModel(IProductsApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [BindProperty]
    public ProductInputModel Input { get; set; } = new();

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var request = new ProductUpsertRequest
        {
            Nombre = Input.Nombre,
            Marca = Input.Marca,
            Precio = Input.Precio,
            CantidadStock = Input.CantidadStock,
            Descripcion = Input.Descripcion
        };

        var created = await _apiClient.CreateProductAsync(request, cancellationToken);

        if (created is null)
        {
            ModelState.AddModelError(string.Empty, "No fue posible crear el producto.");
            return Page();
        }

        return RedirectToPage("Index");
    }
}
