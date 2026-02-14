using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProductHub.Web.Models;
using ProductHub.Web.Services;

namespace ProductHub.Web.Pages.Products;

public class ImageModel : PageModel
{
    private readonly IProductsApiClient _apiClient;

    public ImageModel(IProductsApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Guid Id { get; private set; }

    public ProductDto? Product { get; private set; }

    [BindProperty]
    public IFormFile? UploadFile { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        Id = id;
        Product = await _apiClient.GetProductByIdAsync(id, cancellationToken);

        if (Product is null)
        {
            return NotFound();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id, CancellationToken cancellationToken)
    {
        Id = id;
        Product = await _apiClient.GetProductByIdAsync(id, cancellationToken);

        if (Product is null)
        {
            return NotFound();
        }

        if (UploadFile is null)
        {
            ModelState.AddModelError(string.Empty, "Debes seleccionar un archivo.");
            return Page();
        }

        try
        {
            await _apiClient.UploadImageAsync(id, UploadFile, cancellationToken);
            return RedirectToPage("Edit", new { id });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            Product = await _apiClient.GetProductByIdAsync(id, cancellationToken);
            return Page();
        }
    }
}

