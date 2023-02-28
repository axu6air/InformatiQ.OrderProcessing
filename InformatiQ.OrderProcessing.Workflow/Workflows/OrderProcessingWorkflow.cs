using Dapr.Workflow;
using InformatiQ.OrderProcessing.Workflow.Activities;
using InformatiQ.OrderProcessing.Workflow.Helpers;
using InformatiQ.OrderProcessing.Workflow.Models;
using System.Text.Json;

namespace InformatiQ.OrderProcessing.Workflow.Workflows
{
    class OrderProcessingWorkflow : Workflow<OrderModel, OrderModel>
    {
        public override async Task<OrderModel> RunAsync(WorkflowContext context, OrderModel model)
        {
            var request = model.ValidateRequestAsync();

            if (!request.IsValid)
                return null;

            await Console.Out.WriteLineAsync("OrderProcessingWorkflow started");
            await Console.Out.WriteLineAsync(JsonSerializer.Serialize(model));

            var (orderProcessingModel, message) =
                await context.CallActivityAsync<(OrderModel orderModel, string message)>(nameof(OrderProcessingStartActivity), model);

            var isUserNotified = await context.CallActivityAsync<bool>(nameof(NotifyUserActivity), message);

            var isAddressOkay = await context.CallActivityAsync<bool>(nameof(VerfyAddressActivity), orderProcessingModel);

            var isPaymentOkay = await context.CallActivityAsync<bool>(nameof(VerifyPaymentActivity), orderProcessingModel);

            if (isAddressOkay && isPaymentOkay)
            {
                (orderProcessingModel, message) =
                    await context.CallActivityAsync<(OrderModel orderModel, string message)>(nameof(UpdateOrderActivity), model);
                isUserNotified = await context.CallActivityAsync<bool>(nameof(NotifyUserActivity), message);
            }

            await Console.Out.WriteLineAsync("OrderProcessingWorkflow ended");

            return orderProcessingModel;
        }
    }
}
