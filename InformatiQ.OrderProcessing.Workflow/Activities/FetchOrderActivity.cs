using Dapr.Workflow;
using InformatiQ.OrderProcessing.Core.Models;
using InformatiQ.OrderProcessing.Service;
using InformatiQ.OrderProcessing.Workflow.Models;

namespace InformatiQ.OrderProcessing.Workflow.Activities
{
    internal class FetchOrderActivity : WorkflowActivity<OrderModel, Order>
    {
        private readonly IOrderService _orderService;

        public FetchOrderActivity(IOrderService orderService) =>
            _orderService = orderService;

        public override async Task<Order> RunAsync(WorkflowActivityContext context, OrderModel input)
        {
            await Console.Out.WriteLineAsync($"{nameof(FetchOrderActivity)} running");

            await Console.Out.WriteLineAsync("OrderProcessingWorkflow running");
            var order = await _orderService.GetOrderById(input.Id);
            await Console.Out.WriteLineAsync($"FetchOrderActivity: {order?.Id}");
            return order;
        }
    }
}
