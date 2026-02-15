using Microsoft.EntityFrameworkCore;
using ProductHub.Infrastructure.Entities;

namespace ProductHub.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var product = modelBuilder.Entity<Product>();

        product.ToTable("Products");
        product.HasKey(p => p.Id);

        product.Property(p => p.Nombre)
            .IsRequired()
            .HasMaxLength(120);

        product.Property(p => p.Marca)
            .IsRequired()
            .HasMaxLength(80);

        product.Property(p => p.Precio)
            .HasPrecision(18, 2)
            .IsRequired();

        product.Property(p => p.CantidadStock)
            .IsRequired();

        product.Property(p => p.Descripcion)
            .HasMaxLength(1000);

        product.Property(p => p.ImageUrl)
            .HasMaxLength(300);

        product.Property(p => p.CreatedAt)
            .IsRequired();

        product.Property(p => p.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        product.HasIndex(p => p.IsActive);

        var order = modelBuilder.Entity<Order>();

        order.ToTable("Orders");
        order.HasKey(o => o.Id);

        order.Property(o => o.ClienteNombre)
            .IsRequired()
            .HasMaxLength(120);

        order.Property(o => o.ClienteTelefono)
            .HasMaxLength(40);

        order.Property(o => o.DireccionEntrega)
            .IsRequired()
            .HasMaxLength(250);

        order.Property(o => o.Notas)
            .HasMaxLength(1000);

        order.Property(o => o.Estado)
            .IsRequired()
            .HasMaxLength(30)
            .HasDefaultValue(OrderStatus.Pendiente);

        order.Property(o => o.Canal)
            .IsRequired()
            .HasMaxLength(30)
            .HasDefaultValue("Web");

        order.Property(o => o.Total)
            .HasPrecision(18, 2)
            .IsRequired();

        order.Property(o => o.CreatedAt)
            .IsRequired();

        order.HasIndex(o => o.Estado);
        order.HasIndex(o => o.CreatedAt);

        var orderItem = modelBuilder.Entity<OrderItem>();

        orderItem.ToTable("OrderItems");
        orderItem.HasKey(i => i.Id);

        orderItem.Property(i => i.ProductName)
            .IsRequired()
            .HasMaxLength(120);

        orderItem.Property(i => i.UnitPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        orderItem.Property(i => i.Quantity)
            .IsRequired();

        orderItem.Property(i => i.LineTotal)
            .HasPrecision(18, 2)
            .IsRequired();

        orderItem.HasOne(i => i.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        orderItem.HasOne(i => i.Product)
            .WithMany(p => p.OrderItems)
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        orderItem.HasIndex(i => i.OrderId);
        orderItem.HasIndex(i => i.ProductId);
    }
}
