using Dapr.Workflow;
using InformatiQ.OrderProcessing.Core.Models;
using InformatiQ.OrderProcessing.Service;
using InformatiQ.OrderProcessing.Service.Customers;
using InformatiQ.OrderProcessing.Workflow.Mappers;
using InformatiQ.OrderProcessing.Workflow.Models;
using System.Text.Json;

namespace InformatiQ.OrderProcessing.Workflow.Activities
{
    class OrderProcessingStartActivity : WorkflowActivity<OrderModel, (OrderModel orderModel, string message)>
    {
        private readonly ICustomerService _customerService;
        private readonly IOrderService _orderService;

        public OrderProcessingStartActivity(ICustomerService customerService, IOrderService orderService)
        {
            _customerService = customerService;
            _orderService = orderService;
        }

        public override async Task<(OrderModel orderModel, string message)> RunAsync(WorkflowActivityContext context, OrderModel processingStartModel)
        {
            await Console.Out.WriteLineAsync("OrderProcessingStartActivity running");

            processingStartModel.OrderStatus = Core.Infrastructure.Enums.OrderStatus.Processing;

            var order = CustomMapper.Mapper.Map<Order>(processingStartModel);
            await Console.Out.WriteLineAsync("ORDER MODEL");
            await Console.Out.WriteLineAsync(JsonSerializer.Serialize(order));

            var createdOrder = await _orderService.CreateOrder(order);
            var customer = await Task.FromResult(_customerService.GetCustomer(processingStartModel.CustomerId));
            processingStartModel.CustomerName = customer.Name;

            string message = $"Dear {customer.Name}, Your Order# {createdOrder.Id} has being started to process!";

            return (processingStartModel, message);
        }
    }
}
