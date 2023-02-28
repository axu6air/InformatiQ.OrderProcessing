using InformatiQ.OrderProcessing.Core.Entities;
using InformatiQ.OrderProcessing.Core.Infrastructure.Enums;
using System.Text.Json.Serialization;

namespace InformatiQ.OrderProcessing.Core.Models
{
    public class Order : ContainerEntity
    {
        public string CustomerId { get; set; } = default!;
        public int PaymentId { get; set; } = default!;
        public int BillingAddressId { get; set; } = default!;
        public int ShippingAddressId { get; set; } = default!;
        public decimal DeliveryCharge { get; set; }
        public decimal OrderTotal { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public OrderStatus OrderStatus { get; set; }
        public bool IsUserNotified { get; set; }
        public bool IsSuccess { get; set; }

        public virtual List<OrderItem> OrderItems { get; set; } = new();
    }
}
