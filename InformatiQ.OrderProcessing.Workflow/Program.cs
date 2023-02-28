// See https://aka.ms/new-console-template for more information
using Dapr.Client;
using Dapr.Workflow;
using InformatiQ.OrderProcessing.Workflow.Helpers;
using InformatiQ.OrderProcessing.Workflow.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

#region Application Startup 

var builder = Host.CreateDefaultBuilder(args)
                  .ConfigureHostConfiguration((config) =>
                  {
                      config.SetBasePath(Directory.GetCurrentDirectory())
                              .AddJsonFile("appsettings.json")
                              .AddEnvironmentVariables();
                  })
                  .ConfigureServices(services =>
                  {
                      services.AddOrderProcessingWorkflow();
                  });

builder.AddApplicationServices();

using var host = builder.Build();


host.Start();

#endregion

using var daprClient = new DaprClientBuilder().Build();
WorkflowEngineClient workflowClient = host.Services.GetRequiredService<WorkflowEngineClient>();

await OrderHelper.CreateSampleOrder(workflowClient);


while (true)
{
    Thread.Sleep(TimeSpan.FromSeconds(1));
    Console.WriteLine("Application running");
}