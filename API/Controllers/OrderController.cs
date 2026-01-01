using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PastirmaApi.Application.DTOs.DashboardDTOs;
using PastirmaApi.Application.DTOs.OrderDTOs;
using PastirmaApi.Application.Interfaces.Services;
using PastirmaApi.Core.Exceptions;
using System.Security.Claims;

namespace PastirmaApi.API.Controllers
{
    [Route("api/order")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// Checkout - Yeni sipariş oluşturur (misafir veya kayıtlı kullanıcı)
        /// POST /api/order/checkout
        /// </summary>
        [HttpPost("checkout")]
        public async Task<ActionResult<OrderDTO>> Checkout([FromBody] CreateOrderDTO createOrderDto)
        {
            try
            {
                // Get userId from token if authenticated
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                int? userId = userIdClaim != null ? int.Parse(userIdClaim) : null;

                var order = await _orderService.CreateOrderAsync(createOrderDto, userId);

                return Ok(order);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Sipariş oluşturulurken bir hata oluştu.", error = ex.Message });
            }
        }

        /// <summary>
        /// Sipariş takibi - Sipariş numarası ve email ile sipariş sorgulaması
        /// GET /api/order/track?orderNumber=PST-20231231-0001&email=customer@example.com
        /// </summary>
        [HttpGet("track")]
        public async Task<ActionResult<OrderDTO>> TrackOrder(
            [FromQuery] string orderNumber,
            [FromQuery] string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(orderNumber) || string.IsNullOrWhiteSpace(email))
                {
                    return BadRequest(new { message = "Sipariş numarası ve email gereklidir." });
                }

                var order = await _orderService.TrackOrderAsync(orderNumber, email);

                if (order == null)
                {
                    return NotFound(new { message = "Sipariş bulunamadı." });
                }

                return Ok(order);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Sipariş sorgulanırken bir hata oluştu.", error = ex.Message });
            }
        }

        /// <summary>
        /// Siparişleri filtreler ve sayfalama ile getirir
        /// Admin: Tüm siparişler, User: Sadece kendi siparişleri
        /// GET /api/order?page=1&pageSize=10&status=pending
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<OrderDTO>>> GetOrders(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? status = null)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                // Check if user is admin (use ClaimTypes.Role for mapped claim)
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var isAdmin = userRole == "Admin";

                if (isAdmin)
                {
                    // Admin: Get all orders
                    var allOrders = await _orderService.GetAllOrdersAsync(page, pageSize, status);
                    return Ok(allOrders);
                }
                else
                {
                    // Regular user: Get only their orders
                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (userId == null)
                        throw new BusinessException("Kullanıcı bulunamadı");

                    var orders = await _orderService.GetOrdersAsync(userId, page, pageSize, status);
                    return Ok(orders);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Siparişler alınırken bir hata oluştu.", error = ex.Message });
            }
        }

        /// <summary>
        /// Belirli bir sipariş detayını getirir
        /// GET /api/orders/{orderId}
        /// </summary>
        [HttpGet("{orderId}")]
        public async Task<ActionResult<OrderDTO>> GetOrderDetails(string orderId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                    throw new BusinessException("Kullanıcı bulunamadı");


                var order = await _orderService.GetOrderDetailsAsync(userId, orderId);

                if (order == null)
                {
                    return NotFound(new { message = "Sipariş bulunamadı." });
                }

                return Ok(order);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Sipariş detayları alınırken bir hata oluştu.", error = ex.Message });
            }
        }

        /// <summary>
        /// Sipariş durumunu günceller
        /// PATCH /api/orders/{orderId}/status
        /// </summary>
        [HttpPatch("{orderId}/status")]
        public async Task<ActionResult> UpdateOrderStatus(
            string orderId,
            [FromBody] UpdateOrderStatusRequest request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                    throw new BusinessException("Kullanıcı bulunamadı");

                var success = await _orderService.UpdateOrderStatusAsync(userId, orderId, request.Status);

                if (!success)
                {
                    return NotFound(new { message = "Sipariş bulunamadı veya güncellenemedi." });
                }

                return Ok(new { message = "Sipariş durumu güncellendi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Sipariş durumu güncellenirken bir hata oluştu.", error = ex.Message });
            }
        }
    }
}
