using PastirmaApi.Core.Entities;

namespace PastirmaApi.Application.DTOs.DashboardDTOs
{
    public class UpdateOrderStatusRequest
    {
        public OrderStatus Status { get; set; }
    }
}
