using InformatiQ.OrderProcessing.Core.Models;
using Microsoft.Azure.Cosmos;

namespace InformatiQ.OrderProcessing.Service
{
    public interface IOrderService
    {
        Task<Order> GetOrderById(string id);
        Task<IEnumerable<Order>> GetAllOrder();
        Task<Order> CreateOrder(Order order);
        Task<Order> UpdateOrder(Order order);
        Task<Order> UpdateOrderStatusAsync(string id, IReadOnlyList<PatchOperation> patchOperations);
        Task<bool> DeleteOrder(Order order);

        Task<IEnumerable<OrderItem>> GetOrderItemsByOrderId(string orderId);
        Task<IEnumerable<OrderItem>> CreateOrderItem(string orderId, OrderItem orderItem);
    }
}
