using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductHub.Api.Dtos;
using ProductHub.Api.Services.Storage;
using ProductHub.Infrastructure.Data;
using ProductHub.Infrastructure.Entities;

namespace ProductHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly IProductImageStorageService _imageStorageService;

    public ProductsController(AppDbContext dbContext, IProductImageStorageService imageStorageService)
    {
        _dbContext = dbContext;
        _imageStorageService = imageStorageService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductResponse>>> GetAll([FromQuery] bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Products.AsNoTracking();

        if (!includeInactive)
        {
            query = query.Where(p => p.IsActive);
        }

        var products = await query
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

        return Ok(products.Select(MapToResponse));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductResponse>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _dbContext.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (product is null)
        {
            return NotFound();
        }

        return Ok(MapToResponse(product));
    }

    [HttpPost]
    public async Task<ActionResult<ProductResponse>> Create([FromBody] CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Nombre = request.Nombre.Trim(),
            Marca = request.Marca.Trim(),
            Precio = request.Precio,
            CantidadStock = request.CantidadStock,
            Descripcion = request.Descripcion?.Trim(),
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = product.Id }, MapToResponse(product));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProductResponse>> Update(Guid id, [FromBody] UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (product is null)
        {
            return NotFound();
        }

        product.Nombre = request.Nombre.Trim();
        product.Marca = request.Marca.Trim();
        product.Precio = request.Precio;
        product.CantidadStock = request.CantidadStock;
        product.Descripcion = request.Descripcion?.Trim();

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(MapToResponse(product));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (product is null)
        {
            return NotFound();
        }

        product.IsActive = false;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpPost("{id:guid}/image")]
    public async Task<ActionResult<ProductResponse>> UploadImage(Guid id, [FromForm] IFormFile file, CancellationToken cancellationToken = default)
    {
        var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (product is null)
        {
            return NotFound();
        }

        try
        {
            var imageUrl = await _imageStorageService.SaveProductImageAsync(file, cancellationToken);
            product.ImageUrl = imageUrl;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok(MapToResponse(product));
        }
        catch (FileStorageException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private ProductResponse MapToResponse(Product product)
    {
        return new ProductResponse
        {
            Id = product.Id,
            Nombre = product.Nombre,
            Marca = product.Marca,
            Precio = product.Precio,
            CantidadStock = product.CantidadStock,
            Descripcion = product.Descripcion,
            ImageUrl = BuildImageUrl(product.ImageUrl),
            CreatedAt = product.CreatedAt,
            IsActive = product.IsActive
        };
    }

    private string? BuildImageUrl(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return imageUrl;
        }

        if (Uri.TryCreate(imageUrl, UriKind.Absolute, out _))
        {
            return imageUrl;
        }

        return $"{Request.Scheme}://{Request.Host}{imageUrl}";
    }
}
