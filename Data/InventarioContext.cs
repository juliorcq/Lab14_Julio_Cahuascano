
using System;
using System.Collections.Generic;
using Lab06_Julio_Cahuascano.Models;
using Lab06_Julio_Cahuascano.Models.Scaffolded;
using Microsoft.EntityFrameworkCore;

namespace Lab06_Julio_Cahuascano.Data;

public partial class InventarioContext : DbContext
{
    public InventarioContext(DbContextOptions<InventarioContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<StockMovement> StockMovements { get; set; }

    public virtual DbSet<Supplier> Suppliers { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.IdCategory)
                .HasName("categories_pkey");

            entity.ToTable("categories");

            entity.Property(e => e.IdCategory)
                .HasColumnName("id_category");

            entity.Property(e => e.Description)
                .HasMaxLength(250)
                .HasColumnName("description");

            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.IdProduct)
                .HasName("products_pkey");

            entity.ToTable("products");

            entity.Property(e => e.IdProduct)
                .HasColumnName("id_product");

            entity.Property(e => e.Description)
                .HasMaxLength(250)
                .HasColumnName("description");

            entity.Property(e => e.IdCategory)
                .HasColumnName("id_category");

            entity.Property(e => e.IdSupplier)
                .HasColumnName("id_supplier");

            entity.Property(e => e.Name)
                .HasMaxLength(150)
                .HasColumnName("name");

            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .HasColumnName("price");

            entity.Property(e => e.Stock)
                .HasColumnName("stock");

            entity.HasOne(d => d.IdCategoryNavigation)
                .WithMany(p => p.Products)
                .HasForeignKey(d => d.IdCategory)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_products_categories");

            entity.HasOne(d => d.IdSupplierNavigation)
                .WithMany(p => p.Products)
                .HasForeignKey(d => d.IdSupplier)
                .HasConstraintName("fk_products_suppliers");
        });

        modelBuilder.Entity<StockMovement>(entity =>
        {
            entity.HasKey(e => e.IdMovement)
                .HasName("stock_movements_pkey");

            entity.ToTable("stock_movements");

            entity.Property(e => e.IdMovement)
                .HasColumnName("id_movement");

            entity.Property(e => e.Description)
                .HasMaxLength(250)
                .HasColumnName("description");

            entity.Property(e => e.IdProduct)
                .HasColumnName("id_product");

            entity.Property(e => e.IdUser)
                .HasColumnName("id_user");

            entity.Property(e => e.MovementDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("movement_date");

            entity.Property(e => e.MovementType)
                .HasMaxLength(20)
                .HasColumnName("movement_type");

            entity.Property(e => e.Quantity)
                .HasColumnName("quantity");

            entity.HasOne(d => d.IdProductNavigation)
                .WithMany(p => p.StockMovements)
                .HasForeignKey(d => d.IdProduct)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_movements_products");
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(e => e.IdSupplier)
                .HasName("suppliers_pkey");

            entity.ToTable("suppliers");

            entity.Property(e => e.IdSupplier)
                .HasColumnName("id_supplier");

            entity.Property(e => e.Address)
                .HasMaxLength(200)
                .HasColumnName("address");

            entity.Property(e => e.Email)
                .HasMaxLength(120)
                .HasColumnName("email");

            entity.Property(e => e.Name)
                .HasMaxLength(150)
                .HasColumnName("name");

            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
        });

        // Configuración de la tabla users
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.IdUser)
                .HasName("users_pkey");

            entity.ToTable("users");

            entity.Property(e => e.IdUser)
                .HasColumnName("id_user");

            entity.Property(e => e.Username)
                .HasMaxLength(100)
                .HasColumnName("username");

            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");

            entity.Property(e => e.Role)
                .HasMaxLength(30)
                .HasColumnName("role");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
