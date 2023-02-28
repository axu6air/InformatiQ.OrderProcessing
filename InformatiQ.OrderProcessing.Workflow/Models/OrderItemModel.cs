namespace InformatiQ.OrderProcessing.Workflow.Models
{
    public class OrderItemModel : BaseModel
    {
        public int ProductId { get; set; } = default!;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}
