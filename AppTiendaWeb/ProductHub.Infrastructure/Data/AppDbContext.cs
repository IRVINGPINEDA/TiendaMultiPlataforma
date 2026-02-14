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
    }
}
