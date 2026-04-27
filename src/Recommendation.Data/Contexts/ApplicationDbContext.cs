using Microsoft.EntityFrameworkCore;
using Recommendation.Shared.Models.Entities;

namespace Recommendation.Data.Contexts;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<BestSellerByCategory> BestsellersByCategory => Set<BestSellerByCategory>();
    public DbSet<BestSellerGeneral> BestsellersGeneral => Set<BestSellerGeneral>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("product");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Name).HasColumnName("name");
            entity.Property(x => x.Category).HasColumnName("category");
            entity.Property(x => x.Price).HasColumnName("price");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("order");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.UserId).HasColumnName("user_id");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("order_item");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.OrderId).HasColumnName("order_id");
            entity.Property(x => x.ProductId).HasColumnName("product_id");
            entity.Property(x => x.Quantity).HasColumnName("quantity");

            entity
                .HasOne(x => x.Order)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.OrderId);

            entity
                .HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .HasPrincipalKey(x => x.Id);
        });

        modelBuilder.Entity<BestSellerByCategory>(entity =>
        {
            entity.ToTable("bestsellers_by_category");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Category).HasColumnName("category");
            entity.Property(x => x.ProductId).HasColumnName("product_id");
            entity.Property(x => x.BuyerCount).HasColumnName("buyer_count");
            entity.Property(x => x.Rank).HasColumnName("rank");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<BestSellerGeneral>(entity =>
        {
            entity.ToTable("bestsellers_general");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.ProductId).HasColumnName("product_id");
            entity.Property(x => x.BuyerCount).HasColumnName("buyer_count");
            entity.Property(x => x.Rank).HasColumnName("rank");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        });
    }
}
