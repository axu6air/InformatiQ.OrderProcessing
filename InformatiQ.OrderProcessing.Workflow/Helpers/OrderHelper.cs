using Dapr.Workflow;
using InformatiQ.OrderProcessing.Workflow.Models;
using InformatiQ.OrderProcessing.Workflow.Workflows;

namespace InformatiQ.OrderProcessing.Workflow.Helpers
{
    internal static class OrderHelper
    {
        internal static async Task CreateSampleOrder(WorkflowEngineClient workflowClient)
        {
            var orderModel = GenerateOrderModel();

            var instance = Guid.NewGuid().ToString();

            await workflowClient.ScheduleNewWorkflowAsync(
                    name: nameof(OrderProcessingWorkflow),
                    instanceId: instance,
                    input: orderModel);

            WorkflowState state = await workflowClient.GetWorkflowStateAsync(
                    instanceId: instance,
                    getInputsAndOutputs: true);


            while (!state.IsWorkflowCompleted)
            {

                state = await workflowClient.GetWorkflowStateAsync(
                    instanceId: instance,
                    getInputsAndOutputs: true);
            }
        }

        internal static OrderModel GenerateOrderModel()
        {
            return new OrderModel()
            {
                BillingAddressId = 1,
                ShippingAddressId = 2,
                DeliveryCharge = 10,
                OrderTotal = 100,
                IsSuccess = true,
                IsUserNotified = true,
                OrderItems = new List<OrderItemModel>
                {
                    new() {  Price = 50, ProductId = 10, Quantity = 1 },
                    new() {  Price = 20, ProductId = 20, Quantity = 2 },
                },
                PaymentId = 4564562
            };
        }


    }
}
