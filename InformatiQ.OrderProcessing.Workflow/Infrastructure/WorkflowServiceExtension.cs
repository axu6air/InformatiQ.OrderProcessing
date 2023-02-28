using Dapr.Workflow;
using InformatiQ.OrderProcessing.Workflow.Activities;
using InformatiQ.OrderProcessing.Workflow.Workflows;
using Microsoft.Extensions.DependencyInjection;

namespace InformatiQ.OrderProcessing.Workflow.Infrastructure
{
    public static class WorkflowServiceExtension
    {
        public static IServiceCollection AddOrderProcessingWorkflow(this IServiceCollection services)
        {
            return services.AddDaprWorkflow(options =>
            {
                options.RegisterWorkflow<OrderProcessingWorkflow>();

                options.RegisterActivity<FetchOrderActivity>();
                options.RegisterActivity<OrderProcessingStartActivity>();
                options.RegisterActivity<VerfyAddressActivity>();
                options.RegisterActivity<VerifyPaymentActivity>();
                options.RegisterActivity<ChangeOrderStatus>();
                options.RegisterActivity<UpdateOrderActivity>();
                options.RegisterActivity<NotifyUserActivity>();
            });
        }
    }
}
