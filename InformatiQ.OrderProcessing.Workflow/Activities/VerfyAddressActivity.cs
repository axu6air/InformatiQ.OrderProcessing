using Dapr.Workflow;
using InformatiQ.OrderProcessing.Workflow.Models;

namespace InformatiQ.OrderProcessing.Workflow.Activities
{
    internal class VerfyAddressActivity : WorkflowActivity<OrderModel, bool>
    {
        public override async Task<bool> RunAsync(WorkflowActivityContext context, OrderModel input)
        {
            await Console.Out.WriteLineAsync("VerfyAddressActivity running");

            return await Task.Run(() =>
            {
                if (input.ShippingAddressId > -1 && input.BillingAddressId > -1)
                    return true;

                return false;
            });
        }
    }
}
