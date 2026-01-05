using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PastirmaApi.API.Hubs;
using PastirmaApi.Application.DTOs.AddressDTOs;
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
        private readonly IHubContext<OrderHub> _hubContext;

        public OrderService(ApplicationDbContext context, IHubContext<OrderHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<PaginatedResponse<OrderDTO>> GetAllOrdersAsync(
            int page,
            int pageSize,
            string? status)
        {
            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.ShippingAddress)
                .Include(o => o.BillingAddress)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.Images)
                .AsQueryable();

            // Status filter
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
                UserId = o.UserId,
                UserName = o.User?.Username,
                UserEmail = o.User?.Email ?? o.GuestEmail,
                GuestName = o.GuestName,
                GuestEmail = o.GuestEmail,
                GuestPhone = o.GuestPhone,
                ShippingAddress = o.ShippingAddress != null ? new AddressDTO
                {
                    Id = o.ShippingAddress.Id,
                    FullName = o.ShippingAddress.FullName,
                    Phone = o.ShippingAddress.Phone,
                    AddressLine1 = o.ShippingAddress.AddressLine1,
                    AddressLine2 = o.ShippingAddress.AddressLine2,
                    City = o.ShippingAddress.City,
                    District = o.ShippingAddress.District,
                    PostalCode = o.ShippingAddress.PostalCode,
                    Notes = o.ShippingAddress.Notes,
                    IsDefault = o.ShippingAddress.IsDefault
                } : null,
                BillingAddress = o.BillingAddress != null ? new AddressDTO
                {
                    Id = o.BillingAddress.Id,
                    FullName = o.BillingAddress.FullName,
                    Phone = o.BillingAddress.Phone,
                    AddressLine1 = o.BillingAddress.AddressLine1,
                    AddressLine2 = o.BillingAddress.AddressLine2,
                    City = o.BillingAddress.City,
                    District = o.BillingAddress.District,
                    PostalCode = o.BillingAddress.PostalCode,
                    Notes = o.BillingAddress.Notes,
                    IsDefault = o.BillingAddress.IsDefault
                } : null,
                OrderItems = o.OrderItems.Select(oi => new OrderItemDTO
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId,
                    ProductName = oi.Product.Name,
                    ProductImageUrl = oi.Product.ImageUrl,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.TotalPrice
                }).ToList(),
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

            return new PaginatedResponse<OrderDTO>
            {
                Items = orderDtos,
                TotalCount = totalCount,
                PageSize = pageSize,
                CurrentPage = page,
                TotalPages = totalPages
            };
        }

        public async Task<PaginatedResponse<OrderDTO>> GetOrdersAsync(
            string userId,
            int page,
            int pageSize,
            string? status)
        {
            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.ShippingAddress)
                .Include(o => o.BillingAddress)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId.ToString() == userId)
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
                UserId = o.UserId,
                UserName = o.User?.Username,
                UserEmail = o.User?.Email ?? o.GuestEmail,
                GuestName = o.GuestName,
                GuestEmail = o.GuestEmail,
                GuestPhone = o.GuestPhone,
                ShippingAddress = o.ShippingAddress != null ? new AddressDTO
                {
                    Id = o.ShippingAddress.Id,
                    FullName = o.ShippingAddress.FullName,
                    Phone = o.ShippingAddress.Phone,
                    AddressLine1 = o.ShippingAddress.AddressLine1,
                    AddressLine2 = o.ShippingAddress.AddressLine2,
                    City = o.ShippingAddress.City,
                    District = o.ShippingAddress.District,
                    PostalCode = o.ShippingAddress.PostalCode,
                    Notes = o.ShippingAddress.Notes,
                    IsDefault = o.ShippingAddress.IsDefault
                } : null,
                BillingAddress = o.BillingAddress != null ? new AddressDTO
                {
                    Id = o.BillingAddress.Id,
                    FullName = o.BillingAddress.FullName,
                    Phone = o.BillingAddress.Phone,
                    AddressLine1 = o.BillingAddress.AddressLine1,
                    AddressLine2 = o.BillingAddress.AddressLine2,
                    City = o.BillingAddress.City,
                    District = o.BillingAddress.District,
                    PostalCode = o.BillingAddress.PostalCode,
                    Notes = o.BillingAddress.Notes,
                    IsDefault = o.BillingAddress.IsDefault
                } : null,
                OrderItems = o.OrderItems.Select(oi => new OrderItemDTO
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId,
                    ProductName = oi.Product.Name,
                    ProductImageUrl = oi.Product.ImageUrl,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.TotalPrice
                }).ToList(),
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
                .Include(o => o.ShippingAddress)
                .Include(o => o.BillingAddress)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id.ToString() == orderId && o.UserId.ToString() == userId);

            if (order == null)
                return null;

            return new OrderDTO
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                UserId = order.UserId,
                UserName = order.User?.Username,
                UserEmail = order.User?.Email ?? order.GuestEmail,
                GuestName = order.GuestName,
                GuestEmail = order.GuestEmail,
                GuestPhone = order.GuestPhone,
                ShippingAddress = order.ShippingAddress != null ? new AddressDTO
                {
                    Id = order.ShippingAddress.Id,
                    FullName = order.ShippingAddress.FullName,
                    Phone = order.ShippingAddress.Phone,
                    AddressLine1 = order.ShippingAddress.AddressLine1,
                    AddressLine2 = order.ShippingAddress.AddressLine2,
                    City = order.ShippingAddress.City,
                    District = order.ShippingAddress.District,
                    PostalCode = order.ShippingAddress.PostalCode,
                    Notes = order.ShippingAddress.Notes,
                    IsDefault = order.ShippingAddress.IsDefault
                } : null,
                BillingAddress = order.BillingAddress != null ? new AddressDTO
                {
                    Id = order.BillingAddress.Id,
                    FullName = order.BillingAddress.FullName,
                    Phone = order.BillingAddress.Phone,
                    AddressLine1 = order.BillingAddress.AddressLine1,
                    AddressLine2 = order.BillingAddress.AddressLine2,
                    City = order.BillingAddress.City,
                    District = order.BillingAddress.District,
                    PostalCode = order.BillingAddress.PostalCode,
                    Notes = order.BillingAddress.Notes,
                    IsDefault = order.BillingAddress.IsDefault
                } : null,
                OrderItems = order.OrderItems.Select(oi => new OrderItemDTO
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId,
                    ProductName = oi.Product.Name,
                    ProductImageUrl = oi.Product.ImageUrl,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.TotalPrice
                }).ToList(),
                SubTotal = order.SubTotal,
                ShippingCost = order.ShippingCost,
                TotalAmount = order.TotalAmount,
                PaymentMethod = order.PaymentMethod,
                PaymentStatus = order.PaymentStatus,
                OrderStatus = order.Status,
                Notes = order.Notes,
                AdminNotes = order.AdminNotes,
                CreatedDate = order.CreatedDate,
                UpdatedDate = order.UpdatedDate
            };
        }

        public async Task<bool> UpdateOrderStatusAsync(string userId, string orderId, OrderStatus status)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id.ToString() == orderId);

            if (order == null)
                return false;

            order.Status = status;
            order.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<OrderDTO> CreateOrderAsync(CreateOrderDTO createOrderDto, int? userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Validate and prepare order items
                var orderItems = new List<OrderItem>();
                decimal subTotal = 0;

                foreach (var item in createOrderDto.OrderItems)
                {
                    var product = await _context.Products
                        .FirstOrDefaultAsync(p => p.Id == item.ProductId);

                    if (product == null)
                        throw new InvalidOperationException($"Ürün bulunamadı: {item.ProductId}");

                    if (product.Stock < item.Quantity)
                        throw new InvalidOperationException($"Yetersiz stok: {product.Name}");

                    var orderItem = new OrderItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price,
                        TotalPrice = product.Price * item.Quantity
                    };

                    orderItems.Add(orderItem);
                    subTotal += orderItem.TotalPrice;

                    // Update stock
                    product.Stock -= item.Quantity;
                }

                // 2. Calculate shipping cost
                decimal shippingCost = subTotal >= 500 ? 0 : 50;
                decimal totalAmount = subTotal + shippingCost;

                // 3. Create shipping address
                var shippingAddress = new Address
                {
                    FullName = createOrderDto.ShippingAddress.FullName,
                    Phone = createOrderDto.ShippingAddress.Phone,
                    AddressLine1 = createOrderDto.ShippingAddress.AddressLine1,
                    AddressLine2 = createOrderDto.ShippingAddress.AddressLine2,
                    City = createOrderDto.ShippingAddress.City,
                    District = createOrderDto.ShippingAddress.District,
                    PostalCode = createOrderDto.ShippingAddress.PostalCode,
                    Notes = createOrderDto.ShippingAddress.Notes,
                    UserId = userId
                };
                _context.Addresses.Add(shippingAddress);
                await _context.SaveChangesAsync(); // Save to get ID

                // 4. Create billing address if different
                Address? billingAddress = null;
                if (createOrderDto.BillingAddress != null)
                {
                    billingAddress = new Address
                    {
                        FullName = createOrderDto.BillingAddress.FullName,
                        Phone = createOrderDto.BillingAddress.Phone,
                        AddressLine1 = createOrderDto.BillingAddress.AddressLine1,
                        AddressLine2 = createOrderDto.BillingAddress.AddressLine2,
                        City = createOrderDto.BillingAddress.City,
                        District = createOrderDto.BillingAddress.District,
                        PostalCode = createOrderDto.BillingAddress.PostalCode,
                        Notes = createOrderDto.BillingAddress.Notes,
                        UserId = userId
                    };
                    _context.Addresses.Add(billingAddress);
                    await _context.SaveChangesAsync(); // Save to get ID
                }

                // 5. Generate order number (PST-YYYYMMDD-0001)
                var today = DateTime.UtcNow.Date;
                var orderPrefix = $"PST-{today:yyyyMMdd}-";
                var todayOrders = await _context.Orders
                    .Where(o => o.OrderNumber.StartsWith(orderPrefix))
                    .CountAsync();
                var orderNumber = $"{orderPrefix}{(todayOrders + 1):D4}";

                // 6. Create order
                var order = new Order
                {
                    OrderNumber = orderNumber,
                    UserId = userId,
                    GuestName = createOrderDto.GuestName,
                    GuestEmail = createOrderDto.GuestEmail,
                    GuestPhone = createOrderDto.GuestPhone,
                    ShippingAddressId = shippingAddress.Id,
                    BillingAddressId = billingAddress?.Id,
                    OrderItems = orderItems,
                    SubTotal = subTotal,
                    ShippingCost = shippingCost,
                    TotalAmount = totalAmount,
                    PaymentMethod = createOrderDto.PaymentMethod,
                    PaymentStatus = createOrderDto.PaymentMethod == PaymentMethod.CreditCard
                        ? "Paid"
                        : "Pending",
                    Status = OrderStatus.Pending,
                    Notes = createOrderDto.Notes,
                    CreatedDate = DateTime.UtcNow
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                // 6. Send SignalR notification to Admin users
                await _hubContext.Clients.Group("Admin").SendAsync("NewOrder");

                // 7. Return OrderDTO
                return await GetOrderByIdAsync(order.Id)
                    ?? throw new InvalidOperationException("Sipariş oluşturuldu ancak yüklenemedi");
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<OrderDTO?> GetOrderByIdAsync(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.ShippingAddress)
                .Include(o => o.BillingAddress)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.Images)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return null;

            return MapToOrderDTO(order);
        }

        public async Task<OrderDTO?> TrackOrderAsync(string orderNumber, string email)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.ShippingAddress)
                .Include(o => o.BillingAddress)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.Images)
                .FirstOrDefaultAsync(o =>
                    o.OrderNumber == orderNumber &&
                    (o.User!.Email == email || o.GuestEmail == email));

            if (order == null)
                return null;

            return MapToOrderDTO(order);
        }

        public async Task<bool> UpdateOrderStatusByIdAsync(int orderId, OrderStatus status)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return false;

            order.Status = status;
            order.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        private static OrderDTO MapToOrderDTO(Order order)
        {
            return new OrderDTO
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                UserId = order.UserId,
                UserName = order.User?.Username,
                UserEmail = order.User?.Email ?? order.GuestEmail,
                GuestName = order.GuestName,
                GuestEmail = order.GuestEmail,
                GuestPhone = order.GuestPhone,
                ShippingAddress = order.ShippingAddress != null ? new AddressDTO
                {
                    Id = order.ShippingAddress.Id,
                    FullName = order.ShippingAddress.FullName,
                    Phone = order.ShippingAddress.Phone,
                    AddressLine1 = order.ShippingAddress.AddressLine1,
                    AddressLine2 = order.ShippingAddress.AddressLine2,
                    City = order.ShippingAddress.City,
                    District = order.ShippingAddress.District,
                    PostalCode = order.ShippingAddress.PostalCode,
                    Notes = order.ShippingAddress.Notes,
                    IsDefault = order.ShippingAddress.IsDefault
                } : null,
                BillingAddress = order.BillingAddress != null ? new AddressDTO
                {
                    Id = order.BillingAddress.Id,
                    FullName = order.BillingAddress.FullName,
                    Phone = order.BillingAddress.Phone,
                    AddressLine1 = order.BillingAddress.AddressLine1,
                    AddressLine2 = order.BillingAddress.AddressLine2,
                    City = order.BillingAddress.City,
                    District = order.BillingAddress.District,
                    PostalCode = order.BillingAddress.PostalCode,
                    Notes = order.BillingAddress.Notes,
                    IsDefault = order.BillingAddress.IsDefault
                } : null,
                OrderItems = order.OrderItems.Select(oi => new OrderItemDTO
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId,
                    ProductName = oi.Product.Name,
                    ProductImageUrl = oi.Product.ImageUrl,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.TotalPrice
                }).ToList(),
                SubTotal = order.SubTotal,
                ShippingCost = order.ShippingCost,
                TotalAmount = order.TotalAmount,
                PaymentMethod = order.PaymentMethod,
                PaymentStatus = order.PaymentStatus,
                OrderStatus = order.Status,
                Notes = order.Notes,
                AdminNotes = order.AdminNotes,
                CreatedDate = order.CreatedDate,
                UpdatedDate = order.UpdatedDate
            };
        }
    }
}
