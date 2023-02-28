using Dapr.Workflow;
using InformatiQ.OrderProcessing.Core.Infrastructure;
using Microsoft.Extensions.Logging;

namespace InformatiQ.OrderProcessing.Workflow.Activities
{
    class NotifyUserActivity : WorkflowActivity<string, bool>
    {
        public override async Task<bool> RunAsync(WorkflowActivityContext context, string message)
        {
            try
            {
                await Console.Out.WriteLineAsync("NotifyUserActivity running");

                if (!string.IsNullOrEmpty(message))
                {
                    var logger = LoggerCore.GetLogger<NotifyUserActivity>();
                    logger.LogInformation(message);
                    await Console.Out.WriteLineAsync(message);
                    return await Task.FromResult(true);
                }

                return false;
            }
            catch
            {
                return false;
            }

        }
    }
}
