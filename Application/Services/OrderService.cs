using Microsoft.EntityFrameworkCore;
using PastirmaApi.Application.DTOs.DashboardDTOs;
using PastirmaApi.Application.DTOs.OrderDTOs;
using PastirmaApi.Application.Interfaces.Services;
using PastirmaApi.Core.Entities;
using PastirmaApi.Infrastructure.Data;

namespace PastirmaApi.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;

        public OrderService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedResponse<OrderDTO>> GetOrdersAsync(
            string userId,
            int page,
            int pageSize,
            string? status)
        {
            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .AsQueryable();

            // Status filtresi
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(o => o.Status.ToString() == status);
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var orders = await query
                .OrderByDescending(o => o.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var orderDtos = orders.Select(o => new OrderDTO
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                UserId = o.User.Id,
                UserName = o.User.Username,
                UserEmail = o.User.Email,
                Quantity = o.OrderItems.Sum(oi => oi.Quantity),
                Amount = o.Amount,
                Status = o.Status,
                CreatedAt = o.CreatedDate,
                UpdatedAt = o.UpdatedDate ?? o.CreatedDate
            }).ToList();

            return new PaginatedResponse<OrderDTO>
            {
                Items = orderDtos,
                TotalCount = totalCount,
                PageSize = pageSize,
                CurrentPage = page,
                TotalPages = totalPages
            };
        }

        public async Task<OrderDTO?> GetOrderDetailsAsync(string userId, string orderId)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id.ToString() == orderId);

            if (order == null)
                return null;

            return new OrderDTO
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                UserId = order.User.Id,
                UserName = order.User.Username,
                UserEmail = order.User.Email,
                Quantity = order.OrderItems.Sum(oi => oi.Quantity),
                Amount = order.Amount,
                Status = order.Status,
                CreatedAt = order.CreatedDate,
                UpdatedAt = order.UpdatedDate ?? order.CreatedDate
            };
        }

        public async Task<bool> UpdateOrderStatusAsync(string userId, string orderId, OrderStatus status)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id.ToString() == orderId);

            if (order == null)
                return false;

            order.Status = OrderStatus.Completed; // Güncellenen duruma göre ayarlayın
            order.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
