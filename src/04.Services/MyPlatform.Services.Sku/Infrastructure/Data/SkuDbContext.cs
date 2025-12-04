using Microsoft.EntityFrameworkCore;
using MyPlatform.Services.Sku.Domain.Entities;

namespace MyPlatform.Services.Sku.Infrastructure.Data;

/// <summary>
/// SKU服务数据库上下文
/// </summary>
public class SkuDbContext : DbContext
{
    /// <summary>
    /// 商品集合
    /// </summary>
    public DbSet<Product> Products => Set<Product>();

    /// <summary>
    /// SKU集合
    /// </summary>
    public DbSet<ProductSku> ProductSkus => Set<ProductSku>();

    /// <summary>
    /// 库存集合
    /// </summary>
    public DbSet<SkuStock> SkuStocks => Set<SkuStock>();

    /// <summary>
    /// 价格历史集合
    /// </summary>
    public DbSet<SkuPriceHistory> SkuPriceHistories => Set<SkuPriceHistory>();

    /// <summary>
    /// 创建数据库上下文
    /// </summary>
    /// <param name="options">数据库上下文选项</param>
    public SkuDbContext(DbContextOptions<SkuDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// 配置实体映射
    /// </summary>
    /// <param name="modelBuilder">模型构建器</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 配置Product实体
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("products");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.TenantId).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.Brand).HasMaxLength(100);
            entity.Property(e => e.RowVersion).IsRowVersion();
            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => e.CategoryId);
        });

        // 配置ProductSku实体
        modelBuilder.Entity<ProductSku>(entity =>
        {
            entity.ToTable("product_skus");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.TenantId).HasMaxLength(50);
            entity.Property(e => e.SkuCode).HasMaxLength(100).IsRequired();
            entity.Property(e => e.SkuName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Price).HasPrecision(18, 4);
            entity.Property(e => e.CostPrice).HasPrecision(18, 4);
            entity.Property(e => e.AttributesJson).HasMaxLength(2000);
            entity.Property(e => e.RowVersion).IsRowVersion();
            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => e.ProductId);
            entity.HasIndex(e => e.SkuCode);
        });

        // 配置SkuStock实体
        modelBuilder.Entity<SkuStock>(entity =>
        {
            entity.ToTable("sku_stocks");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.TenantId).HasMaxLength(50);
            entity.Property(e => e.RowVersion).IsRowVersion();
            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => e.SkuId);
        });

        // 配置SkuPriceHistory实体
        modelBuilder.Entity<SkuPriceHistory>(entity =>
        {
            entity.ToTable("sku_price_histories");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.TenantId).HasMaxLength(50);
            entity.Property(e => e.OldPrice).HasPrecision(18, 4);
            entity.Property(e => e.NewPrice).HasPrecision(18, 4);
            entity.Property(e => e.Reason).HasMaxLength(500);
            entity.Property(e => e.OperatorId).HasMaxLength(50);
            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => e.SkuId);
            entity.HasIndex(e => e.CreatedAt);
        });
    }
}
