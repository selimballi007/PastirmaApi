using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PastirmaApi.Application.DTOs.DashboardDTOs;
using PastirmaApi.Application.DTOs.OrderDTOs;
using PastirmaApi.Application.Interfaces.Services;
using PastirmaApi.Core.Exceptions;

namespace PastirmaApi.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// Siparişleri filtreler ve sayfalama ile getirir
        /// GET /api/orders?page=1&pageSize=10&status=pending
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

                var userId = User.FindFirst("UserId")?.Value;
                if (userId == null)
                    throw new BusinessException("Kullanıcı bulunamadı");

                var orders = await _orderService.GetOrdersAsync(userId, page, pageSize, status);
                return Ok(orders);
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
                var userId = User.FindFirst("UserId")?.Value;
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
                var userId = User.FindFirst("UserId")?.Value;
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
