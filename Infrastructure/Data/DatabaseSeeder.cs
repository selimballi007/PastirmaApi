using Microsoft.EntityFrameworkCore;
using PastirmaApi.Core.Entities;
using BCrypt.Net;

namespace PastirmaApi.Infrastructure.Data
{
    public class DatabaseSeeder
    {
        private readonly ApplicationDbContext _context;

        public DatabaseSeeder(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            // Check if data already exists
            if (await _context.Users.AnyAsync())
            {
                Console.WriteLine("Database already has data. Skipping seed.");
                return;
            }

            Console.WriteLine("Starting database seed...");

            // 1. Seed Users
            var adminUser = new User
            {
                Email = "admin@pastirma.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                Username = "admin",
                FullName = "Admin User",
                IsGuest = false,
                IsVerified = true,
                Role = UserRole.Admin,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            var customerUser = new User
            {
                Email = "customer@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Customer123!"),
                Username = "customer",
                FullName = "Test Customer",
                IsGuest = false,
                IsVerified = true,
                Role = UserRole.Customer,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            await _context.Users.AddRangeAsync(adminUser, customerUser);
            await _context.SaveChangesAsync();

            Console.WriteLine("✓ Users seeded");

            // 2. Seed Categories
            var categories = new[]
            {
                new Category { Name = "Pastırma", Icon = "🥩", DisplayOrder = 1, IsActive = true, CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow },
                new Category { Name = "Sucuk", Icon = "🌭", DisplayOrder = 2, IsActive = true, CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow },
                new Category { Name = "Kavurma", Icon = "🍖", DisplayOrder = 3, IsActive = true, CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow },
                new Category { Name = "Köfte", Icon = "🥘", DisplayOrder = 4, IsActive = true, CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow },
                new Category { Name = "Şarküteri", Icon = "🧀", DisplayOrder = 5, IsActive = true, CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow }
            };

            await _context.Categories.AddRangeAsync(categories);
            await _context.SaveChangesAsync();

            Console.WriteLine("✓ Categories seeded");

            // 3. Seed Products
            var products = new[]
            {
                new Product
                {
                    Name = "Kayseri Pastırması Premium",
                    Description = "Geleneksel yöntemlerle hazırlanmış, özenle seçilmiş dana etinden yapılmış premium Kayseri pastırması. Lezzet ve kalite garantisi.",
                    Price = 899.90m,
                    OldPrice = 1099.90m,
                    Stock = 50,
                    CategoryId = categories[0].Id,
                    ImageUrl = "https://placehold.co/600x400/ff6b6b/white?text=Kayseri+Pastirması",
                    IsBestseller = true,
                    BestsellerOrder = 1,
                    IsCampaign = true,
                    CampaignOrder = 1,
                    IsNew = true,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                },
                new Product
                {
                    Name = "Dana Sucuk 500gr",
                    Description = "100% dana etinden üretilmiş, katkısız ve doğal baharatlarla hazırlanmış geleneksel dana sucuk.",
                    Price = 299.90m,
                    Stock = 100,
                    CategoryId = categories[1].Id,
                    ImageUrl = "https://placehold.co/600x400/e74c3c/white?text=Dana+Sucuk",
                    IsBestseller = true,
                    BestsellerOrder = 2,
                    IsCampaign = false,
                    IsNew = false,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                },
                new Product
                {
                    Name = "El Yapımı Kavurma",
                    Description = "Geleneksel yöntemlerle hazırlanmış, kendi yağında pişirilmiş enfes kavurma. Kahvaltı sofralarınızın vazgeçilmezi.",
                    Price = 449.90m,
                    OldPrice = 549.90m,
                    Stock = 30,
                    CategoryId = categories[2].Id,
                    ImageUrl = "https://placehold.co/600x400/f39c12/white?text=Kavurma",
                    IsBestseller = false,
                    IsCampaign = true,
                    CampaignOrder = 2,
                    IsNew = true,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                },
                new Product
                {
                    Name = "İnegöl Köfte (1kg)",
                    Description = "Özel baharatlarla hazırlanmış, dana ve kuzu karışımı İnegöl köfte. Mangalda veya tavada muhteşem bir lezzet.",
                    Price = 349.90m,
                    Stock = 75,
                    CategoryId = categories[3].Id,
                    ImageUrl = "https://placehold.co/600x400/8b4513/white?text=İnegöl+Köfte",
                    IsBestseller = true,
                    BestsellerOrder = 3,
                    IsNew = false,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                },
                new Product
                {
                    Name = "Dilim Pastırma 200gr",
                    Description = "Pratik kullanım için özel olarak dilimlenmiş premium pastırma. Kahvaltı ve mezeler için ideal.",
                    Price = 189.90m,
                    Stock = 120,
                    CategoryId = categories[0].Id,
                    ImageUrl = "https://placehold.co/600x400/c0392b/white?text=Dilim+Pastırma",
                    IsCampaign = false,
                    IsNew = true,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                },
                new Product
                {
                    Name = "Fermente Sucuk 750gr",
                    Description = "Özel fermantasyon süreciyle olgunlaştırılmış, yoğun aromalı fermente sucuk. Gurmelerin tercihi.",
                    Price = 429.90m,
                    OldPrice = 499.90m,
                    Stock = 45,
                    CategoryId = categories[1].Id,
                    ImageUrl = "https://placehold.co/600x400/d35400/white?text=Fermente+Sucuk",
                    IsCampaign = true,
                    CampaignOrder = 3,
                    IsNew = false,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                },
                new Product
                {
                    Name = "Antep Kavurması 500gr",
                    Description = "Antep usulü hazırlanmış, baharatlı ve lezzetli kavurma. Kahvaltı masalarınızın yıldızı.",
                    Price = 379.90m,
                    Stock = 60,
                    CategoryId = categories[2].Id,
                    ImageUrl = "https://placehold.co/600x400/e67e22/white?text=Antep+Kavurması",
                    IsBestseller = false,
                    IsNew = true,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                },
                new Product
                {
                    Name = "Izgara Köfte Karışımı",
                    Description = "Mangal için özel harmanlanmış köfte harcı. Sadece şekil verin ve pişirin!",
                    Price = 279.90m,
                    Stock = 90,
                    CategoryId = categories[3].Id,
                    ImageUrl = "https://placehold.co/600x400/a0522d/white?text=Köfte+Karışımı",
                    IsBestseller = false,
                    IsNew = false,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                },
                new Product
                {
                    Name = "Kurutulmuş Dana Bonfile",
                    Description = "Premium dana bonfile etinden hazırlanmış, geleneksel yöntemlerle kurutulmuş özel ürün.",
                    Price = 1299.90m,
                    Stock = 25,
                    CategoryId = categories[4].Id,
                    ImageUrl = "https://placehold.co/600x400/922b21/white?text=Dana+Bonfile",
                    IsBestseller = false,
                    IsCampaign = false,
                    IsNew = true,
                    IsSpecialOffer = true,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                },
                new Product
                {
                    Name = "Mangal Sucuk Paketi 1.5kg",
                    Description = "Mangal keyfi için ideal, 3x500gr paket halinde hazırlanmış özel sucuk paketi.",
                    Price = 799.90m,
                    OldPrice = 899.90m,
                    Stock = 40,
                    CategoryId = categories[1].Id,
                    ImageUrl = "https://placehold.co/600x400/c0392b/white?text=Mangal+Sucuk",
                    IsBestseller = true,
                    BestsellerOrder = 4,
                    IsCampaign = true,
                    CampaignOrder = 4,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                }
            };

            await _context.Products.AddRangeAsync(products);
            await _context.SaveChangesAsync();

            // Reload products to ensure IDs are populated
            var savedProducts = await _context.Products.OrderBy(p => p.Id).ToListAsync();

            Console.WriteLine("✓ Products seeded");

            // 4. Seed Blog Categories
            var blogCategories = new[]
            {
                new BlogCategory { Name = "Tarifler", Icon = "👨‍🍳", DisplayOrder = 1, IsActive = true, CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow },
                new BlogCategory { Name = "Bilgi", Icon = "📚", DisplayOrder = 2, IsActive = true, CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow },
                new BlogCategory { Name = "Hikayeler", Icon = "📖", DisplayOrder = 3, IsActive = true, CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow }
            };

            await _context.BlogCategories.AddRangeAsync(blogCategories);
            await _context.SaveChangesAsync();

            Console.WriteLine("✓ Blog Categories seeded");

            // 5. Seed Blog Posts
            var blogPosts = new[]
            {
                new BlogPost
                {
                    Title = "Evde Pastırma Nasıl Saklanır?",
                    Content = "<p>Pastırmanın tazeliğini ve lezzetini korumak için doğru saklama koşulları çok önemlidir.</p><h2>Buzdolabında Saklama</h2><p>Pastırmanızı buzdolabının en soğuk bölümünde, tercihen 0-4°C arasında muhafaza edin. Orijinal ambalajında veya hava almayan bir kapta saklayın.</p><h2>Dondurucuda Saklama</h2><p>Uzun süreli saklama için dondurucuyu tercih edebilirsiniz. Porsiyonlara ayırıp vakumlu paketlerde dondurun.</p>",
                    Excerpt = "Pastırmanın tazeliğini ve lezzetini uzun süre korumak için doğru saklama yöntemlerini öğrenin.",
                    ImageUrl = "https://placehold.co/800x400/3498db/white?text=Blog+Saklama",
                    CategoryId = blogCategories[0].Id, // Tarifler
                    AuthorId = adminUser.Id,
                    PublishedDate = DateTime.UtcNow.AddDays(-10),
                    IsFeatured = true,
                    ViewCount = 145,
                    ReadTime = "3 min",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow.AddDays(-10),
                    UpdatedDate = DateTime.UtcNow.AddDays(-10)
                },
                new BlogPost
                {
                    Title = "Pastırma ve Sucuk Arasındaki Farklar",
                    Content = "<p>Türk mutfağının iki vazgeçilmez ürünü pastırma ve sucuk, üretim yöntemleri ve lezzet profilleri açısından farklılık gösterir.</p><h2>Üretim Süreci</h2><p>Pastırma, dana etinin çemen ile kaplanıp kurutulmasıyla; sucuk ise kıyma haline getirilip baharatlarla karıştırılıp bağırsağa doldurulmasıyla üretilir.</p><h2>Kullanım Alanları</h2><p>Pastırma genellikle dilim dilim servis edilirken, sucuk pişirilerek tüketilir.</p>",
                    Excerpt = "Türk mutfağının iki önemli ürünü pastırma ve sucuğun üretim süreçleri ve kullanım alanlarındaki farkları keşfedin.",
                    ImageUrl = "https://placehold.co/800x400/2ecc71/white?text=Blog+Farklar",
                    CategoryId = blogCategories[1].Id, // Bilgi
                    AuthorId = adminUser.Id,
                    PublishedDate = DateTime.UtcNow.AddDays(-7),
                    IsFeatured = true,
                    ViewCount = 203,
                    ReadTime = "4 min",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow.AddDays(-7),
                    UpdatedDate = DateTime.UtcNow.AddDays(-7)
                },
                new BlogPost
                {
                    Title = "Kayseri Pastırmasının Tarihi",
                    Content = "<p>Kayseri pastırması, yüzyıllardır Anadolu mutfağının en değerli ürünlerinden biri olarak varlığını sürdürmektedir.</p><h2>Tarihçe</h2><p>Pastırma yapımı, göçebe Türklerin et saklama yöntemlerinden gelişerek günümüze kadar gelmiştir. Özellikle Kayseri bölgesi, iklim koşulları sayesinde pastırma üretiminde öne çıkmıştır.</p><h2>Geleneksel Üretim</h2><p>Geleneksel üretim yöntemleri, kuşaktan kuşağa aktarılarak günümüze kadar korunmuştur.</p>",
                    Excerpt = "Asırlık bir geleneğe sahip Kayseri pastırmasının tarihini ve kültürel önemini keşfedin.",
                    ImageUrl = "https://placehold.co/800x400/e67e22/white?text=Blog+Tarih",
                    CategoryId = blogCategories[2].Id, // Hikayeler
                    AuthorId = adminUser.Id,
                    PublishedDate = DateTime.UtcNow.AddDays(-5),
                    IsFeatured = true,
                    ViewCount = 178,
                    ReadTime = "5 min",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow.AddDays(-5),
                    UpdatedDate = DateTime.UtcNow.AddDays(-5)
                }
            };

            await _context.BlogPosts.AddRangeAsync(blogPosts);
            await _context.SaveChangesAsync();

            Console.WriteLine("✓ Blog Posts seeded");

            // 6. Seed Hero Slides
            var heroSlides = new[]
            {
                new HeroSlide
                {
                    Title = "Premium Kayseri Pastırması",
                    Subtitle = "Geleneksel lezzet, modern kalite",
                    Description = "Özenle seçilmiş dana etinden, geleneksel yöntemlerle üretilmiş premium pastırma",
                    ImageUrl = "https://placehold.co/1920x600/ff6b6b/white?text=Kayseri+Pastırması",
                    ButtonText = "Hemen İncele",
                    ButtonLink = "/products?filter=best-sellers",
                    DisplayOrder = 1,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                },
                new HeroSlide
                {
                    Title = "Taze Sucuk Çeşitleri",
                    Subtitle = "%100 Dana Eti",
                    Description = "Katkısız, doğal ve taze sucuk çeşitlerimizle sofralarınıza lezzet katın",
                    ImageUrl = "https://placehold.co/1920x600/e74c3c/white?text=Taze+Sucuk",
                    ButtonText = "Ürünleri Gör",
                    ButtonLink = "/products?category=2",
                    DisplayOrder = 2,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                },
                new HeroSlide
                {
                    Title = "Kampanyalı Ürünler",
                    Subtitle = "Fırsatları Kaçırmayın",
                    Description = "Seçili ürünlerde %20'ye varan indirimler",
                    ImageUrl = "https://placehold.co/1920x600/f39c12/white?text=Kampanya",
                    ButtonText = "Kampanyaları Gör",
                    ButtonLink = "/products?filter=campaign",
                    DisplayOrder = 3,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                }
            };

            await _context.HeroSlides.AddRangeAsync(heroSlides);
            await _context.SaveChangesAsync();

            Console.WriteLine("✓ Hero Slides seeded");

            // 7. Seed Reviews
            var reviews = new[]
            {
                new Review
                {
                    UserId = customerUser.Id,
                    ProductId = savedProducts[0].Id, // Kayseri Pastırması Premium
                    Rating = 5,
                    Comment = "Harika bir ürün! Gerçekten çok kaliteli ve lezzetli. Kesinlikle tavsiye ederim.",
                    Status = ReviewStatus.Approved,
                    ApprovedDate = DateTime.UtcNow.AddDays(-8),
                    CreatedDate = DateTime.UtcNow.AddDays(-8),
                    UpdatedDate = DateTime.UtcNow.AddDays(-8)
                },
                new Review
                {
                    UserId = customerUser.Id,
                    ProductId = savedProducts[1].Id, // Dana Sucuk 500gr
                    Rating = 4,
                    Comment = "Çok güzel bir sucuk. Kahvaltıda harika oluyor. Fiyat performans olarak iyi.",
                    Status = ReviewStatus.Approved,
                    ApprovedDate = DateTime.UtcNow.AddDays(-6),
                    CreatedDate = DateTime.UtcNow.AddDays(-6),
                    UpdatedDate = DateTime.UtcNow.AddDays(-6)
                },
                new Review
                {
                    UserId = customerUser.Id,
                    ProductId = savedProducts[3].Id, // İnegöl Köfte (1kg)
                    Rating = 5,
                    Comment = "İnegöl köfte denilince akla ilk gelen bu ürün olmalı. Muhteşem lezzet!",
                    Status = ReviewStatus.Approved,
                    ApprovedDate = DateTime.UtcNow.AddDays(-3),
                    CreatedDate = DateTime.UtcNow.AddDays(-3),
                    UpdatedDate = DateTime.UtcNow.AddDays(-3)
                }
            };

            await _context.Reviews.AddRangeAsync(reviews);
            await _context.SaveChangesAsync();

            Console.WriteLine("✓ Reviews seeded");

            Console.WriteLine("✅ Database seed completed successfully!");
            Console.WriteLine("\n📧 Admin credentials:");
            Console.WriteLine("   Email: admin@pastirma.com");
            Console.WriteLine("   Password: Admin123!");
            Console.WriteLine("\n📧 Customer credentials:");
            Console.WriteLine("   Email: customer@test.com");
            Console.WriteLine("   Password: Customer123!");
        }
    }
}
