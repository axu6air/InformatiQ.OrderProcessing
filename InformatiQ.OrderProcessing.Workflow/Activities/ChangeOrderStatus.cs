using Dapr.Workflow;
using InformatiQ.OrderProcessing.Service.Customers;
using InformatiQ.OrderProcessing.Workflow.Models;
using Microsoft.Extensions.Logging;

namespace InformatiQ.OrderProcessing.Workflow.Activities
{
    internal class ChangeOrderStatus : WorkflowActivity<OrderModel, OrderModel>
    {
        private readonly ILogger _logger;
        private readonly ICustomerService _customerService;

        public ChangeOrderStatus(ILoggerFactory loggerFactory, ICustomerService customerService)
        {
            _logger = loggerFactory.CreateLogger<OrderProcessingStartActivity>();
            _customerService = customerService;
        }

        public override async Task<OrderModel> RunAsync(WorkflowActivityContext context, OrderModel input)
        {
            await Console.Out.WriteLineAsync("ChangeOrderStatus running");

            var customer = await Task.FromResult(_customerService.GetCustomer(input.CustomerId));

            _logger.LogInformation($"Dear {customer.Name}, Your Order# {input.Id} has been placed successfully!");
            //input.OrderStatus = Core.Infrastructure.Enums.OrderStatus.Placed;
            return input;
        }
    }
}
