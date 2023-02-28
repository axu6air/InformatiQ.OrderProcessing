using InformatiQ.OrderProcessing.Core.Entities;

namespace InformatiQ.OrderProcessing.Core.Models
{
    public class OrderItem : ChildEntity
    {
        public int ProductId { get; set; } = default!;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}
