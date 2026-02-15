using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductHub.Api.Dtos;
using ProductHub.Infrastructure.Data;
using ProductHub.Infrastructure.Entities;

namespace ProductHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private static readonly HashSet<string> AllowedChannels = ["Web", "Movil"];

    private readonly AppDbContext _dbContext;

    public OrdersController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderResponse>>> GetAll([FromQuery] string? estado = null, CancellationToken cancellationToken = default)
    {
        var normalizedStatus = OrderStatus.Normalize(estado);
        if (!string.IsNullOrWhiteSpace(estado) && normalizedStatus is null)
        {
            return BadRequest(new { message = "El estado solicitado no es valido." });
        }

        var query = _dbContext.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(normalizedStatus))
        {
            query = query.Where(o => o.Estado == normalizedStatus);
        }

        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

        return Ok(orders.Select(MapToResponse));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderResponse>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _dbContext.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (order is null)
        {
            return NotFound();
        }

        return Ok(MapToResponse(order));
    }

    [HttpPost]
    public async Task<ActionResult<OrderResponse>> Create([FromBody] CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Items.Count == 0)
        {
            return BadRequest(new { message = "Debes incluir al menos un producto para realizar la compra." });
        }

        if (request.Items.Any(i => i.Quantity <= 0))
        {
            return BadRequest(new { message = "Todas las cantidades deben ser mayores a cero." });
        }

        var groupedItems = request.Items
            .GroupBy(i => i.ProductId)
            .Select(group => new
            {
                ProductId = group.Key,
                Quantity = group.Sum(i => i.Quantity)
            })
            .ToList();

        var productIds = groupedItems.Select(i => i.ProductId).Distinct().ToList();

        var products = await _dbContext.Products
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, cancellationToken);

        var missingProducts = productIds.Where(id => !products.ContainsKey(id)).ToList();
        if (missingProducts.Count != 0)
        {
            return BadRequest(new
            {
                message = "Hay productos que no existen.",
                productIds = missingProducts
            });
        }

        foreach (var item in groupedItems)
        {
            var product = products[item.ProductId];

            if (!product.IsActive)
            {
                return BadRequest(new { message = $"El producto '{product.Nombre}' no esta disponible." });
            }

            if (product.CantidadStock < item.Quantity)
            {
                return BadRequest(new
                {
                    message = $"Stock insuficiente para '{product.Nombre}'. Disponible: {product.CantidadStock}."
                });
            }
        }

        var order = new Order
        {
            Id = Guid.NewGuid(),
            ClienteNombre = request.ClienteNombre.Trim(),
            ClienteTelefono = request.ClienteTelefono?.Trim(),
            DireccionEntrega = request.DireccionEntrega.Trim(),
            Notas = request.Notas?.Trim(),
            Canal = NormalizeChannel(request.Canal),
            Estado = OrderStatus.Pendiente,
            CreatedAt = DateTime.UtcNow
        };

        decimal total = 0;
        foreach (var item in groupedItems)
        {
            var product = products[item.ProductId];
            var lineTotal = product.Precio * item.Quantity;

            product.CantidadStock -= item.Quantity;
            total += lineTotal;

            order.Items.Add(new OrderItem
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                ProductName = product.Nombre,
                UnitPrice = product.Precio,
                Quantity = item.Quantity,
                LineTotal = lineTotal
            });
        }

        order.Total = total;

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = order.Id }, MapToResponse(order));
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<OrderResponse>> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedStatus = OrderStatus.Normalize(request.Estado);
        if (normalizedStatus is null)
        {
            return BadRequest(new { message = "El estado solicitado no es valido." });
        }

        var order = await _dbContext.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (order is null)
        {
            return NotFound();
        }

        if (order.Estado == normalizedStatus)
        {
            return Ok(MapToResponse(order));
        }

        if (order.Estado != OrderStatus.Cancelada && normalizedStatus == OrderStatus.Cancelada)
        {
            await RestockProductsAsync(order, cancellationToken);
        }

        if (order.Estado == OrderStatus.Cancelada && normalizedStatus != OrderStatus.Cancelada)
        {
            var stockCheck = await TryDeductProductsFromCancelledOrderAsync(order, cancellationToken);
            if (!stockCheck.Success)
            {
                return BadRequest(new { message = stockCheck.ErrorMessage });
            }
        }

        order.Estado = normalizedStatus;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(MapToResponse(order));
    }

    private async Task RestockProductsAsync(Order order, CancellationToken cancellationToken)
    {
        var productIds = order.Items.Select(i => i.ProductId).Distinct().ToList();

        var products = await _dbContext.Products
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, cancellationToken);

        foreach (var item in order.Items)
        {
            if (products.TryGetValue(item.ProductId, out var product))
            {
                product.CantidadStock += item.Quantity;
            }
        }
    }

    private async Task<(bool Success, string? ErrorMessage)> TryDeductProductsFromCancelledOrderAsync(Order order, CancellationToken cancellationToken)
    {
        var productIds = order.Items.Select(i => i.ProductId).Distinct().ToList();

        var products = await _dbContext.Products
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, cancellationToken);

        foreach (var item in order.Items)
        {
            if (!products.TryGetValue(item.ProductId, out var product))
            {
                return (false, $"No se encontro el producto con id {item.ProductId} para reactivar la orden.");
            }

            if (product.CantidadStock < item.Quantity)
            {
                return (false, $"No hay stock suficiente para reactivar '{item.ProductName}'.");
            }
        }

        foreach (var item in order.Items)
        {
            products[item.ProductId].CantidadStock -= item.Quantity;
        }

        return (true, null);
    }

    private static string NormalizeChannel(string? channel)
    {
        if (string.IsNullOrWhiteSpace(channel))
        {
            return "Web";
        }

        var normalized = channel.Trim();
        var match = AllowedChannels.FirstOrDefault(ch => ch.Equals(normalized, StringComparison.OrdinalIgnoreCase));

        return match ?? "Web";
    }

    private static OrderResponse MapToResponse(Order order)
    {
        return new OrderResponse
        {
            Id = order.Id,
            ClienteNombre = order.ClienteNombre,
            ClienteTelefono = order.ClienteTelefono,
            DireccionEntrega = order.DireccionEntrega,
            Notas = order.Notas,
            Estado = order.Estado,
            Canal = order.Canal,
            Total = order.Total,
            CreatedAt = order.CreatedAt,
            Items = order.Items
                .OrderBy(i => i.ProductName)
                .Select(i => new OrderItemResponse
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    UnitPrice = i.UnitPrice,
                    Quantity = i.Quantity,
                    LineTotal = i.LineTotal
                })
                .ToList()
        };
    }
}
