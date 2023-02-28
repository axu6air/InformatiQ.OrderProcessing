using InformatiQ.OrderProcessing.Data;
using InformatiQ.OrderProcessing.Data.Configurations;
using InformatiQ.OrderProcessing.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace InformatiQ.OrderProcessing.Workflow.Infrastructure
{
    public static class ConfigureServicesExtension
    {
        public static IHostBuilder AddApplicationServices(this IHostBuilder builder)
        {
            var configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile($"appsettings.json").AddEnvironmentVariables();

            var config = configuration.Build();
            var cosmosSettings = config.GetSection(CosmosDbDataServiceConfiguration.CosmosDbSettings);

            return builder.ConfigureServices((services) =>
            {
                services.AddDataConfigurationService(cosmosSettings.Value)
                         .AddDomainConfigurationService();
            });
        }
    }
}
