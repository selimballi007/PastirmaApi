using Microsoft.EntityFrameworkCore;
using PastirmaApi.Core.Entities;
using PastirmaApi.Infrastructure.Data.Extensions;

namespace PastirmaApi.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<User> Users => Set<User>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<Address> Addresses => Set<Address>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<Review> Reviews => Set<Review>();        
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Favorite> Favorites => Set<Favorite>();
        public DbSet<HeroSlide> HeroSlides => Set<HeroSlide>();
        public DbSet<ProductImage> ProductImages => Set<ProductImage>();
        public DbSet<BlogPost> BlogPosts => Set<BlogPost>();
        public DbSet<BlogCategory> BlogCategories => Set<BlogCategory>();
        public DbSet<ContactFormSubmission> ContactFormSubmissions => Set<ContactFormSubmission>();
        public override int SaveChanges()
        {
            UpdateTimeStamps();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimeStamps();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimeStamps()
        {
            var entries = ChangeTracker.Entries<BaseEntity>();

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)                
                    entry.Entity.CreatedDate = DateTime.UtcNow;
                
                else if (entry.State == EntityState.Modified)                
                    entry.Entity.UpdatedDate = DateTime.UtcNow;
            }
        }

        public void SoftDelete<T>(T entity) where T : BaseEntity
        {
            entity.IsActive = false;
            Entry(entity).State = EntityState.Modified;
            SaveChanges();
        }

        public async Task SoftDeleteAsync<T>(T Entity) where T : BaseEntity
        {
            Entity.IsActive = false;
            Entry(Entity).State = EntityState.Modified;
            await SaveChangesAsync();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Global snake_case convention
            ApplySnakeCaseNamingConvention(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.Role).IsRequired();
                entity.HasQueryFilter(e => e.IsActive);
            });

            modelBuilder.Entity<BlogPost>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Category)
                    .WithMany(c => c.BlogPosts)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Author)
                    .WithMany()
                    .HasForeignKey(e => e.AuthorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<BlogCategory>(entity =>
            {
                entity.HasKey(e => e.Id);
            });
        }

        private void ApplySnakeCaseNamingConvention(ModelBuilder modelBuilder)
        {
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                // Table names
                entity.SetTableName(entity.GetTableName()?.ToSnakeCase());

                // Column names
                foreach (var property in entity.GetProperties())
                {
                    property.SetColumnName(property.GetColumnName().ToSnakeCase());
                }

                // Keys
                foreach (var key in entity.GetKeys())
                {
                    key.SetName(key.GetName()?.ToSnakeCase());
                }

                // Indexes
                foreach (var index in entity.GetIndexes())
                {
                    index.SetDatabaseName(index.GetDatabaseName()?.ToSnakeCase());
                }
            }
        }
    }
}
