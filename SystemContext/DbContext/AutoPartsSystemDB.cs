using Microsoft.EntityFrameworkCore;
using Model.Entities;
using SystemContext;
using SystemModel.Entities;

namespace SystemContext
{
    public class AutoPartsSystemDB : DbContext
    {
        public AutoPartsSystemDB(DbContextOptions<AutoPartsSystemDB> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            #region Category Information
            modelBuilder.Entity<Category>()
                .HasOne(c => c.User)
                .WithMany(u => u.Categories)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Category>()
                .HasMany(c => c.ProductTypes)
                .WithOne(pt => pt.Category)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Category>()
                .Property(c => c.Name)
                .HasMaxLength(100)
                .IsRequired();
            #endregion

            #region ProductType Information
            modelBuilder.Entity<ProductType>()
                .HasOne(pt => pt.User)
                .WithMany(u => u.ProductTypes)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProductType>()
                .HasOne(pt => pt.Category)
                .WithMany(c => c.ProductTypes)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProductType>()
                .HasMany(pt => pt.Products)
                .WithOne(p => p.ProductType)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProductType>()
                .Property(pt => pt.Name)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<ProductType>()
                .Property(pt => pt.CategoryID)
                .IsRequired();
            #endregion

            #region Product Information
            modelBuilder.Entity<Product>()
                .HasOne(p => p.User)
                .WithMany(u => u.Products)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Product>()
                .HasMany(p => p.Orders)
                .WithOne(o => o.Product)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Product>()
                .HasMany(p => p.ProductCars)
                .WithOne(pc => pc.Product)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.ProductType)
                .WithMany(pt => pt.Products)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Product>()
                .Property(p => p.Name)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<Product>()
                .Property(p => p.ProductTypeID)
                .IsRequired();
            #endregion

            #region Order Information
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Product)
                .WithMany(p => p.Orders)
                .OnDelete(DeleteBehavior.Restrict);
            #endregion

            #region Car Information
            modelBuilder.Entity<Car>()
                .HasOne(c => c.User)
                .WithMany(u => u.Cars)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Car>()
                .HasMany(c => c.ProductCars)
                .WithOne(pc => pc.Car)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Car>()
                .Property(c => c.Model)
                .HasMaxLength(100)
                .IsRequired();
            #endregion

            #region ProductCar Information
            modelBuilder.Entity<ProductCar>()
                .HasKey(pc => new { pc.ProductID, pc.CarID }); // Composite Key

            modelBuilder.Entity<ProductCar>()
                .HasOne(pc => pc.Product)
                .WithMany(p => p.ProductCars)
                .HasForeignKey(pc => pc.ProductID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProductCar>()
                .HasOne(pc => pc.Car)
                .WithMany(c => c.ProductCars)
                .HasForeignKey(pc => pc.CarID)
                .OnDelete(DeleteBehavior.Restrict);
            #endregion

            #region Expense Information
            modelBuilder.Entity<Expens>()
                .HasOne(e => e.User)
                .WithMany(u => u.Expenses)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Expens>()
                .Property(e => e.Name)
                .HasMaxLength(100);
            #endregion
        }

        #region DbSet Information
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<ProductType> ProductTypes { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<ProductCar> ProductCars { get; set; }
        public virtual DbSet<Car> Cars { get; set; }
        public virtual DbSet<Expens> Expenses { get; set; }
        public virtual DbSet<User> Users { get; set; }
        #endregion
    }
}

