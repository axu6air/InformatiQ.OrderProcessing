using InformatiQ.OrderProcessing.Core.Infrastructure.Enums;
using System.Text.Json.Serialization;

namespace InformatiQ.OrderProcessing.Workflow.Models
{
    public class OrderModel : BaseModel
    {
        public string CustomerId { get; set; } = Guid.NewGuid().ToString();
        public string CustomerName { get; set; } = default!;
        public int PaymentId { get; set; } = default!;
        public int BillingAddressId { get; set; } = default!;
        public int ShippingAddressId { get; set; } = default!;
        public decimal DeliveryCharge { get; set; }
        public decimal OrderTotal { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public OrderStatus OrderStatus { get; set; } = OrderStatus.Placed;
        public bool IsUserNotified { get; set; }
        public bool IsSuccess { get; set; }

        public virtual List<OrderItemModel> OrderItems { get; set; } = new();
    }
}
