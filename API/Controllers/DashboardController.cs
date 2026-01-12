using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PastirmaApi.Application.DTOs.DashboardDTOs;
using PastirmaApi.Application.DTOs.OrderDTOs;
using PastirmaApi.Application.Interfaces.Services;
using PastirmaApi.Core.Exceptions;
using System.Security.Claims;

namespace PastirmaApi.API.Controllers
{
    [Route("api/dashboard")]
    [ApiController]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        /// <summary>
        /// Tüm dashboard verilerini getirir
        /// GET /api/dashboard
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<DashboardResponse>> GetDashboardData()
        {
            try
            {
                var dashboardData = await _dashboardService.GetDashboardDataAsync();
                return Ok(dashboardData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Dashboard verileri alınırken bir hata oluştu.", error = ex.Message });
            }
        }

        /// <summary>
        /// Sadece istatistikleri getirir
        /// GET /api/dashboard/stats
        /// </summary>
        [HttpGet("stats")]
        public async Task<ActionResult<DashboardStats>> GetStats()
        {
            try
            {
                var stats = await _dashboardService.GetStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "İstatistikler alınırken bir hata oluştu.", error = ex.Message });
            }
        }

        /// <summary>
        /// Son siparişleri getirir
        /// GET /api/dashboard/orders/recent?limit=10
        /// </summary>
        [HttpGet("orders/recent")]
        public async Task<ActionResult<List<OrderDTO>>> GetRecentOrders([FromQuery] int limit = 10)
        {
            try
            {
                if (limit < 1 || limit > 100)
                {
                    return BadRequest(new { message = "Limit 1 ile 100 arasında olmalıdır." });
                }

                var orders = await _dashboardService.GetRecentOrdersAsync(limit);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Siparişler alınırken bir hata oluştu.", error = ex.Message });
            }
        }

        /// <summary>
        /// Satış verilerini getirir
        /// GET /api/dashboard/sales?period=month
        /// </summary>
        [HttpGet("sales")]
        public async Task<ActionResult<List<SalesDataPoint>>> GetSalesData([FromQuery] string period = "month")
        {
            try
            {
                if (!new[] { "week", "month", "year" }.Contains(period.ToLower()))
                {
                    return BadRequest(new { message = "Period 'week', 'month' veya 'year' olmalıdır." });
                }

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                    throw new BusinessException("Kullanıcı bulunamadı");

                var salesData = await _dashboardService.GetSalesDataAsync(period);
                return Ok(salesData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Satış verileri alınırken bir hata oluştu.", error = ex.Message });
            }
        }

        /// <summary>
        /// Hızlı istatistikleri getirir
        /// GET /api/dashboard/quick-stats
        /// </summary>
        [HttpGet("quick-stats")]
        public async Task<ActionResult<QuickStats>> GetQuickStats()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                    throw new BusinessException("Kullanıcı bulunamadı");

                var quickStats = await _dashboardService.GetQuickStatsAsync();
                return Ok(quickStats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Hızlı istatistikler alınırken bir hata oluştu.", error = ex.Message });
            }
        }

        /// <summary>
        /// Bildirim sayısını getirir
        /// GET /api/dashboard/notifications/count
        /// </summary>
        [HttpGet("notifications/count")]
        public async Task<ActionResult<NotificationCountResponse>> GetNotificationCount()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                    throw new BusinessException("Kullanıcı bulunamadı");

                var count = await _dashboardService.GetNotificationCountAsync();
                return Ok(new NotificationCountResponse { Count = count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Bildirim sayısı alınırken bir hata oluştu.", error = ex.Message });
            }
        }

        /// <summary>
        /// Düşük stok uyarılarını getirir
        /// GET /api/dashboard/alerts/low-stock
        /// </summary>
        [HttpGet("alerts/low-stock")]
        public async Task<ActionResult> GetLowStockAlerts()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                    throw new BusinessException("Kullanıcı bulunamadı");

                var alerts = await _dashboardService.GetLowStockAlertsAsync();
                return Ok(alerts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Stok uyarıları alınırken bir hata oluştu.", error = ex.Message });
            }
        }
    }
}
