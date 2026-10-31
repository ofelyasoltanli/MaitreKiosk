using MaitreKiosk.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace MaitreKiosk.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Restaurant> Restaurants => Set<Restaurant>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Restaurant>(e =>
        {
            e.Property(x => x.Name).IsRequired().HasMaxLength(100);
        });

        b.Entity<Product>(e =>
        {
            e.Property(x => x.Name).IsRequired().HasMaxLength(100);
            e.Property(x => x.Category).IsRequired().HasMaxLength(50);
            e.Property(x => x.Price).HasPrecision(18, 2);
            e.HasOne(x => x.Restaurant)
             .WithMany()
             .HasForeignKey(x => x.RestaurantId);
            e.HasIndex(x => x.RestaurantId);
        });

        b.Entity<Order>(e =>
        {
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.TotalAmount).HasPrecision(18, 2);
            e.HasOne(x => x.Restaurant)
             .WithMany()
             .HasForeignKey(x => x.RestaurantId);
            e.HasIndex(x => new { x.RestaurantId, x.Status });
        });

        b.Entity<OrderItem>(e =>
        {
            e.Property(x => x.ProductName).IsRequired().HasMaxLength(100);
            e.Property(x => x.UnitPrice).HasPrecision(18, 2);
            e.Property(x => x.LineTotal).HasPrecision(18, 2);
            e.HasOne(x => x.Order)
             .WithMany(o => o.Items)
             .HasForeignKey(x => x.OrderId);
            e.HasOne(x => x.Product)
             .WithMany()
             .HasForeignKey(x => x.ProductId);
            e.HasIndex(x => x.ProductId);
        });

        // Seed data
        b.Entity<Restaurant>().HasData(
            new Restaurant { Id = 1, Name = "Dolma Evi" },
            new Restaurant { Id = 2, Name = "Pizza Nova" },
            new Restaurant { Id = 3, Name = "Sushi Bar" });

        b.Entity<Product>().HasData(
            new Product { Id = 1, RestaurantId = 1, Name = "Dolma", Category = "Əsas yemək", Price = 8.50m, IsAvailable = true },
            new Product { Id = 2, RestaurantId = 1, Name = "Plov", Category = "Əsas yemək", Price = 12.00m, IsAvailable = true },
            new Product { Id = 3, RestaurantId = 1, Name = "Ayran", Category = "İçki", Price = 2.00m, IsAvailable = true },
            new Product { Id = 4, RestaurantId = 2, Name = "Margherita", Category = "Pizza", Price = 11.00m, IsAvailable = true },
            new Product { Id = 5, RestaurantId = 2, Name = "Pepperoni", Category = "Pizza", Price = 13.50m, IsAvailable = true },
            new Product { Id = 6, RestaurantId = 2, Name = "Cola", Category = "İçki", Price = 2.50m, IsAvailable = true },
            new Product { Id = 7, RestaurantId = 3, Name = "Sake Roll", Category = "Roll", Price = 9.00m, IsAvailable = true },
            new Product { Id = 8, RestaurantId = 3, Name = "Miso Soup", Category = "Şorba", Price = 4.50m, IsAvailable = true });
    }
}