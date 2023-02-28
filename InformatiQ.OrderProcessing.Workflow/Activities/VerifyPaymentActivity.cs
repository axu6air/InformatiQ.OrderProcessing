using Dapr.Workflow;
using InformatiQ.OrderProcessing.Workflow.Models;

namespace InformatiQ.OrderProcessing.Workflow.Activities
{
    internal class VerifyPaymentActivity : WorkflowActivity<OrderModel, bool>
    {
        public override async Task<bool> RunAsync(WorkflowActivityContext context, OrderModel input)
        {
            await Console.Out.WriteLineAsync("VerifyPaymentActivity running");

            return await Task.Run(() =>
            {
                if (input.PaymentId > -1)
                    return true;

                return false;
            });
        }
    }
}
