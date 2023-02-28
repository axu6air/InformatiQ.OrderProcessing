using InformatiQ.OrderProcessing.Core.Infrastructure;
using InformatiQ.OrderProcessing.Data.Configurations;
using InformatiQ.OrderProcessing.Data.Repositories;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CosmosDatabase = InformatiQ.OrderProcessing.Data.Configurations.Database;

namespace InformatiQ.OrderProcessing.Data
{

    public static class DataServiceExtension
    {
        public static IServiceCollection AddDataConfigurationService(this IServiceCollection service, string cosmosConfiguration)
        {
            var logger = LoggerCore.GetLogger("DataServiceExtension");

            logger.LogInformation("Cosmos db initiating...");

            var cosmosConfig = EnvironmentVariable<CosmosDbDataServiceConfiguration>.GetServiceConfiguration(cosmosConfiguration);

            #region Dependency Injections 

            logger.LogInformation($"Cosmos db services trying to inject...");
            service.AddTransient(typeof(IRepository<>), typeof(Repository<>));

            logger.LogInformation($"Cosmos db services injected...");
            #endregion

            try
            {
                var cosmoDbConfiguration = PrepareCosmosClient(service, cosmosConfig, out CosmosClient cosmosClient);
                logger.LogInformation("Cosmos client prepared...");


                logger.LogInformation("Cosmos db trying to create if not exist...");
                foreach (var database in cosmoDbConfiguration.Databases)
                {
                    try
                    {
                        var (databaseResponse, isProvisioned) = CreateDatabase(database, cosmosClient);

                        if (databaseResponse != null)
                        {
                            CreateContainers(databaseResponse, database.Containers, isProvisioned, logger);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError($"{database.Name} could not be initiated");
                        logger.LogInformation($"{database.Name} informations");

                        foreach (var container in database.Containers)
                        {
                            logger.LogInformation($"Container name is {container.Name}");
                            logger.LogInformation($"Container partition key is {container.PartitionKeyPath}");
                            logger.LogInformation($"Container time to live is {container.TimeToLive}");
                            logger.LogInformation($"Container Max RU is {container.MaxThroughPut}");
                            logger.LogInformation($"Container RU is {container.RequestUnits}");
                        }

                        logger.LogInformation(ex, ex.Message);
                    }

                    logger.LogInformation("Cosmos db created or found...");

                }

                service.AddSingleton(cosmosClient);

                logger.LogInformation("Cosmos db successfully initiated");
            }
            catch (Exception ex)
            {
                logger.LogError("Cosmos db could not be initiated");
                logger.LogError("Cosmos db informations: ");
                logger.LogInformation($"ConnectionString: {cosmosConfig?.ConnectionString}");
                logger.LogInformation($"MaxRetryAttempts: {cosmosConfig?.MaxRetryAttempts}");
                logger.LogInformation($"MaxRetryWaitTime: {cosmosConfig?.MaxRetryWaitTime}");

                logger.LogError(ex, message: ex.Message);
            }

            return service;
        }

        //public static IServiceCollection AddDataCacheConfigurationService(this IServiceCollection service, string key)
        //{
        //    service.AddCosmosCache((CosmosCacheOptions cacheOptions) =>
        //    {
        //        cacheOptions.ContainerName = "myCacheContainer";
        //        cacheOptions.DatabaseName = "myCacheDatabase";
        //        cacheOptions.ClientBuilder = new CosmosClientBuilder(configuration["CosmosDBConnectionString"]);
        //        cacheOptions.CreateIfNotExists = true;
        //    });

        //    return service;
        //}

        private static ICosmosDbDataServiceConfiguration PrepareCosmosClient(IServiceCollection service, CosmosDbDataServiceConfiguration cosmosConfig, out CosmosClient cosmosClient)
        {
            service.Configure<CosmosDbDataServiceConfiguration>(x =>
            {
                x.ConnectionString = cosmosConfig.ConnectionString;
                x.Databases = cosmosConfig.Databases;
                x.MaxRetryAttempts = cosmosConfig.MaxRetryAttempts;
                x.MaxRetryWaitTime = cosmosConfig.MaxRetryWaitTime;
            });

            #region Dependency Injections 

            //Validations and Dependency Injection
            service.AddSingleton<IValidateOptions<CosmosDbDataServiceConfiguration>, CosmosDbDataServiceConfigurationValidation>();
            var cosmosDbDataServiceConfiguration = service.BuildServiceProvider().GetRequiredService<IOptions<CosmosDbDataServiceConfiguration>>().Value;
            service.AddSingleton<ICosmosDbDataServiceConfiguration>(cosmosDbDataServiceConfiguration);

            #endregion

            var serviceProvider = service.BuildServiceProvider();
            var cosmoDbConfiguration = serviceProvider.GetRequiredService<ICosmosDbDataServiceConfiguration>();

            CosmosClientOptions cosmosClientOptions = new()
            {
                SerializerOptions = new CosmosSerializationOptions()
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                },
                ConnectionMode = ConnectionMode.Gateway,

                //Retry policy for 429 StatusCode
                MaxRetryAttemptsOnRateLimitedRequests = int.TryParse(cosmoDbConfiguration.MaxRetryAttempts, out int maxRetryAttempt) ? maxRetryAttempt : 9,
                MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(double.TryParse(cosmoDbConfiguration.MaxRetryWaitTime, out double maxRetryWait) ? maxRetryWait : 30)
            };

            //Creating cosmos db if not exits
            cosmosClient = new(cosmoDbConfiguration.ConnectionString, cosmosClientOptions);
            return cosmoDbConfiguration;
        }

        private static (DatabaseResponse, bool isProvisioned) CreateDatabase(CosmosDatabase database, CosmosClient cosmosClient, bool isProvisioned = false)
        {
            DatabaseResponse databaseResponse = null;

            if (database.IsProvisioned != null && database.IsProvisioned == true)
            {
                isProvisioned = true;
                if (database.IsAutoscaled != null && database.IsAutoscaled == true)
                {
                    int maxThroughPut = int.TryParse(database.MaxThroughPut, out int contMaxThroughPut) ? contMaxThroughPut : 1000;
                    ThroughputProperties autoScaledThroughputPropertiesForDatabase;
                    autoScaledThroughputPropertiesForDatabase = ThroughputProperties.CreateAutoscaleThroughput(maxThroughPut);

                    databaseResponse = cosmosClient.CreateDatabaseIfNotExistsAsync(database.Name, autoScaledThroughputPropertiesForDatabase).GetAwaiter().GetResult();
                }
                else
                {
                    int? databaseRequestUnits = int.TryParse(database.RequestUnits, out int dbBudget) ? dbBudget : null;

                    databaseResponse = cosmosClient.CreateDatabaseIfNotExistsAsync(database.Name, databaseRequestUnits).GetAwaiter().GetResult();
                }
            }
            else
                databaseResponse = cosmosClient.CreateDatabaseIfNotExistsAsync(database.Name).GetAwaiter().GetResult();

            return (databaseResponse, isProvisioned);
        }

        private static void CreateContainers(DatabaseResponse databaseResponse, List<CosmosContainer> containers, bool isProvisionedDatabase, ILogger logger)
        {
            foreach (var container in containers)
            {
                int? defaultTimeToLive = int.TryParse(container.TimeToLive, out int contTimeToLive) ? contTimeToLive : -1;

                ContainerProperties containerProperties = new()
                {
                    Id = container.Name,
                    PartitionKeyPath = container.PartitionKeyPath,
                    DefaultTimeToLive = defaultTimeToLive
                };

                if (isProvisionedDatabase == true)
                {
                    _ = databaseResponse?.Database.CreateContainerIfNotExistsAsync(containerProperties).GetAwaiter().GetResult();
                }
                else
                {
                    if (container.IsAutoscaled != null && container.IsAutoscaled == true)
                    {
                        int maxThroughtPut = int.TryParse(container.MaxThroughPut, out int contMaxThroughPut) ? contMaxThroughPut : 1000;
                        ThroughputProperties autoscaleThroughputProperties = ThroughputProperties.CreateAutoscaleThroughput(maxThroughtPut);

                        _ = databaseResponse?.Database.CreateContainerIfNotExistsAsync(containerProperties, autoscaleThroughputProperties).GetAwaiter().GetResult();
                    }
                    else
                    {
                        int? containerRequestUnits = int.TryParse(container.RequestUnits, out int contBudget) ? contBudget : null;
                        _ = databaseResponse?.Database.CreateContainerIfNotExistsAsync(containerProperties, containerRequestUnits).GetAwaiter().GetResult();
                    }
                }

                logger.LogInformation($"Cosmos db container {container.Name} created or found...");
            }
        }
    }
}
