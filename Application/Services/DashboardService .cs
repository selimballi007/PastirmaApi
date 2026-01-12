using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using PastirmaApi.Application.DTOs.AddressDTOs;
using PastirmaApi.Application.DTOs.DashboardDTOs;
using PastirmaApi.Application.DTOs.OrderDTOs;
using PastirmaApi.Application.Interfaces.Services;
using PastirmaApi.Core.Entities;
using PastirmaApi.Core.Exceptions;
using PastirmaApi.Infrastructure.Data;
using PastirmaApi.Infrastructure.Identity;
using System.Security.Claims;

namespace PastirmaApi.Application.Services
{
    [Authorize]
    public class DashboardService:IDashboardService
    {
        private readonly ApplicationDbContext _context;
        private readonly IJwtService _jwtService;

        public DashboardService(ApplicationDbContext context, IJwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        public async Task<DashboardResponse> GetDashboardDataAsync()
        {            
            var stats = await GetStatsAsync();
            var recentOrders = await GetRecentOrdersAsync(10);
            var salesData = await GetSalesDataAsync("month");
            var quickStats = await GetQuickStatsAsync();

            return new DashboardResponse
            {
                Stats = stats,
                RecentOrders = recentOrders,
                SalesData = salesData,
                QuickStats = quickStats
            };
        }

        public async Task<DashboardStats> GetStatsAsync()
        {
            var now = DateTime.UtcNow;
            var thirtyDaysAgo = now.AddDays(-30);
            var sixtyDaysAgo = now.AddDays(-60);

            // Bu ay ve ge�en ay�n verilerini al
            var currentPeriodOrders = await _context.Orders
                .Where(o => o.CreatedDate >= thirtyDaysAgo && o.CreatedDate <= now)
                .ToListAsync();

            var previousPeriodOrders = await _context.Orders
                .Where(o => o.CreatedDate >= sixtyDaysAgo && o.CreatedDate < thirtyDaysAgo)
                .ToListAsync();

            // �statistikleri hesapla
            var totalSales = currentPeriodOrders
                .Where(o => o.Status == OrderStatus.Delivered)
                .Sum(o => o.TotalAmount);

            var previousSales = previousPeriodOrders
                .Where(o => o.Status == OrderStatus.Delivered)
                .Sum(o => o.TotalAmount);

            var totalOrders = currentPeriodOrders.Count;
            var previousOrders = previousPeriodOrders.Count;

            // M��teri say�s�
            var totalCustomers = await _context.Users
                .Where(u => u.Role == UserRole.Customer)
                .CountAsync();

            var previousCustomers = await _context.Users
                .Where(u => u.Role == UserRole.Customer && u.CreatedDate < thirtyDaysAgo)
                .CountAsync();

            // �r�n say�s�
            var totalProducts = await _context.Products.CountAsync();
            var previousProducts = await _context.Products
                .Where(p => p.CreatedDate < thirtyDaysAgo)
                .CountAsync();

            return new DashboardStats
            {
                TotalSales = totalSales,
                TotalOrders = totalOrders,
                TotalCustomers = totalCustomers,
                TotalProducts = totalProducts,
                SalesChange = CalculatePercentageChange(previousSales, totalSales),
                OrdersChange = CalculatePercentageChange(previousOrders, totalOrders),
                CustomersChange = CalculatePercentageChange(previousCustomers, totalCustomers),
                ProductsChange = CalculatePercentageChange(previousProducts, totalProducts)
            };
        }

        public async Task<List<OrderDTO>> GetRecentOrdersAsync(int limit)
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.ShippingAddress)
                .OrderByDescending(o => o.CreatedDate)
                .Take(limit)
                .ToListAsync();

            return orders.Select(o => new OrderDTO
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                UserId = o.UserId,
                UserName = o.User?.FullName ?? o.BillingAddress?.FullName ?? o.ShippingAddress?.FullName ?? o.GuestName,
                UserEmail = o.User?.Email ?? o.BillingAddress?.FullName ?? o.ShippingAddress?.FullName ?? o.GuestEmail,
                GuestName = o.GuestName,
                GuestEmail = o.GuestEmail,
                GuestPhone = o.GuestPhone,
                OrderItems = new List<OrderItemDTO>(),
                SubTotal = o.SubTotal,
                ShippingCost = o.ShippingCost,
                TotalAmount = o.TotalAmount,
                PaymentMethod = o.PaymentMethod,
                PaymentStatus = o.PaymentStatus,
                OrderStatus = o.Status,
                Notes = o.Notes,
                AdminNotes = o.AdminNotes,
                CreatedDate = o.CreatedDate,
                UpdatedDate = o.UpdatedDate
            }).ToList();
        }

        public async Task<List<SalesDataPoint>> GetSalesDataAsync(string period)
        {
            var now = DateTime.UtcNow;
            DateTime startDate;
            int dataPoints;

            // Period'a g�re tarih aral��� belirle
            switch (period.ToLower())
            {
                case "week":
                    startDate = now.AddDays(-7);
                    dataPoints = 7;
                    break;
                case "year":
                    startDate = now.AddMonths(-12);
                    dataPoints = 12;
                    break;
                case "month":
                default:
                    startDate = now.AddMonths(-6);
                    dataPoints = 6;
                    break;
            }

            var orders = await _context.Orders
                .Where(o => o.CreatedDate >= startDate && o.Status == OrderStatus.Delivered)
                .ToListAsync();

            var salesData = new List<SalesDataPoint>();

            if (period.ToLower() == "week")
            {
                // G�nl�k veri
                for (int i = 0; i < dataPoints; i++)
                {
                    var date = startDate.AddDays(i);
                    var dayOrders = orders.Where(o => o.CreatedDate.Date == date.Date);

                    salesData.Add(new SalesDataPoint
                    {
                        Date = date.ToString("yyyy-MM-dd"),
                        Sales = dayOrders.Sum(o => o.TotalAmount),
                        Orders = dayOrders.Count()
                    });
                }
            }
            else
            {
                // Ayl�k veri
                for (int i = 0; i < dataPoints; i++)
                {
                    var date = startDate.AddMonths(i);
                    var monthOrders = orders.Where(o =>
                        o.CreatedDate.Year == date.Year &&
                        o.CreatedDate.Month == date.Month);

                    salesData.Add(new SalesDataPoint
                    {
                        Date = date.ToString("yyyy-MM-01"),
                        Sales = monthOrders.Sum(o => o.TotalAmount),
                        Orders = monthOrders.Count()
                    });
                }
            }

            return salesData;
        }

        public async Task<QuickStats> GetQuickStatsAsync()
        {
            var now = DateTime.UtcNow;
            var thirtyDaysAgo = now.AddDays(-30);

            // Son 30 g�n�n verileri
            var recentOrders = await _context.Orders
                .Where(o => o.CreatedDate >= thirtyDaysAgo)
                .ToListAsync();

            // Site ziyaret�i say�s� (varsay�msal, ger�ek uygulamada analytics'ten gelecek)
            var visitors = 10000; // Placeholder

            // D�n���m oran�: (Tamamlanan sipari� / Toplam ziyaret�i) * 100
            var completedOrders = recentOrders.Count(o => o.Status == OrderStatus.Delivered);
            var conversionRate = visitors > 0 ? (double)completedOrders / visitors * 100 : 0;

            // Ortalama sipari� de�eri
            var averageOrderValue = completedOrders > 0
                ? recentOrders.Where(o => o.Status == OrderStatus.Delivered).Average(o => o.TotalAmount)
                : 0;

            // Aktif kullan�c�lar (son 30 g�nde sipari� veren)
            var activeUsers = await _context.Orders
                .Where(o => o.CreatedDate >= thirtyDaysAgo)
                .Select(o => o.UserId)
                .Distinct()
                .CountAsync();

            // D���k stoklu �r�nler (stok < 10)
            var lowStockProducts = await _context.Products
                .Where(p => p.Stock < 10)
                .CountAsync();

            return new QuickStats
            {
                ConversionRate = Math.Round(conversionRate, 2),
                AverageOrderValue = Math.Round(averageOrderValue, 2),
                ActiveUsers = activeUsers,
                LowStockProducts = lowStockProducts
            };
        }

        public async Task<int> GetNotificationCountAsync()
        {
            // Okunmam�� bildirim say�s�
            // Bu k�s�m notification sisteminize g�re de�i�ecek
            return await _context.Notifications
                .Where(n => !n.IsRead)
                .CountAsync();
        }

        public async Task<object> GetLowStockAlertsAsync()
        {
            // D���k stoklu �r�nleri d�nd�r
            var lowStockProducts = await _context.Products
                .Where(p => p.Stock < 10)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Stock,
                    p.Price
                })
                .ToListAsync();

            return lowStockProducts;
        }

        // Y�zde de�i�im hesaplama
        private double CalculatePercentageChange(decimal oldValue, decimal newValue)
        {
            if (oldValue == 0)
                return newValue > 0 ? 100 : 0;

            var change = ((newValue - oldValue) / oldValue) * 100;
            return Math.Round((double)change, 1);
        }

        private double CalculatePercentageChange(int oldValue, int newValue)
        {
            if (oldValue == 0)
                return newValue > 0 ? 100 : 0;

            var change = ((double)(newValue - oldValue) / oldValue) * 100;
            return Math.Round(change, 1);
        }
    }
}
