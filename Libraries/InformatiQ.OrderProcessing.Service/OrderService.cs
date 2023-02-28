using Humanizer;
using InformatiQ.OrderProcessing.Core.Models;
using InformatiQ.OrderProcessing.Data.Repositories;
using Microsoft.Azure.Cosmos;

namespace InformatiQ.OrderProcessing.Service
{
    public class OrderService : IOrderService
    {
        private readonly IRepository<Order> _orderRepository;

        public OrderService(IRepository<Order> orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<Order> GetOrderById(string id)
        {
            var (result, _) = await _orderRepository.GetAsync(id);
            return result!;
        }

        public async Task<IEnumerable<Order>> GetAllOrder()
        {
            var (results, _) = await _orderRepository.GetAsync(x => x.Id != null);
            return results.ToList();
        }

        public async Task<Order> CreateOrder(Order order)
        {
            var (result, _) = await _orderRepository.CreateAsync(order);
            return result;
        }

        public async Task<Order> UpdateOrder(Order order)
        {
            var (result, _) = await _orderRepository.UpdateAsync(order);
            return result;
        }

        public async Task<Order> UpdateOrderStatusAsync(string id, IReadOnlyList<PatchOperation> patchOperations)
        {
            var (result, _) = await _orderRepository.PatchAsync(id, patchOperations);
            return result;
        }

        public async Task<bool> DeleteOrder(Order order)
        {
            var result = await _orderRepository.DeleteAsync(order);
            return result == System.Net.HttpStatusCode.NoContent ? true : false;
        }

        public async Task<IEnumerable<OrderItem>> GetOrderItemsByOrderId(string orderId)
        {
            var (order, _) = await _orderRepository.GetAsync(orderId);
            return order?.OrderItems;
        }

        public async Task<IEnumerable<OrderItem>> CreateOrderItem(string orderId, OrderItem orderItem)
        {
            var (order, _) = await _orderRepository.GetAsync(orderId);
            order.OrderItems.Add(orderItem);

            var patchOperations = new[] {
                PatchOperation.Replace($"/{nameof(Order.OrderItems).Camelize()}", order.OrderItems)
            };
            var (result, _) = await _orderRepository.PatchAsync(orderId, patchOperations);

            return result?.OrderItems;
        }
    }
}
