using Dapr.Workflow;
using Humanizer;
using InformatiQ.OrderProcessing.Core.Infrastructure.Enums;
using InformatiQ.OrderProcessing.Core.Models;
using InformatiQ.OrderProcessing.Service;
using InformatiQ.OrderProcessing.Workflow.Mappers;
using InformatiQ.OrderProcessing.Workflow.Models;
using Microsoft.Azure.Cosmos;

namespace InformatiQ.OrderProcessing.Workflow.Activities
{
    class UpdateOrderActivity : WorkflowActivity<OrderModel, (OrderModel orderModel, string message)>
    {
        private readonly IOrderService _orderService;

        public UpdateOrderActivity(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public override async Task<(OrderModel orderModel, string message)> RunAsync(WorkflowActivityContext context, OrderModel input)
        {
            try
            {
                await Console.Out.WriteLineAsync($"{nameof(UpdateOrderActivity)} running");

                var order = CustomMapper.Mapper.Map<Order>(input);
                //await Console.Out.WriteLineAsync("OrderModel");
                ////await Console.Out.WriteLineAsync(JsonSerializer.Serialize<OrderModel>(input));
                //await Console.Out.WriteLineAsync("Order");
                //await Console.Out.WriteLineAsync(JsonSerializer.Serialize<Order>(order));

                var updatedOrder = await _orderService.UpdateOrderStatusAsync(order.Id, new[] {
                    PatchOperation.Replace($"/{nameof(Order.OrderStatus).Camelize()}", OrderStatus.Placed)
                });

                string message = $"Dear {input.CustomerName}, Your Order# {input.Id} has been placed!";

                return (CustomMapper.Mapper.Map<OrderModel>(updatedOrder), message);
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.Message);
                return (null, string.Empty);
            }
        }
    }
}
