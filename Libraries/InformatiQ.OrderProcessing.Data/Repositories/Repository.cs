using InformatiQ.OrderProcessing.Core.Entities;
using InformatiQ.OrderProcessing.Data.Configurations;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace InformatiQ.OrderProcessing.Data.Repositories
{
    public partial class Repository<T> : IRepository<T> where T : IContainerEntity
    {
        private readonly Container _container;
        private readonly ILogger _logger;

        public Repository(CosmosClient client, ICosmosDbDataServiceConfiguration dataServiceConfiguration, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Repository<T>>();

            foreach (var database in dataServiceConfiguration.Databases)
            {
                if (database.Containers.Any(x => x.Name == typeof(T).Name))
                {
                    _container = client.GetContainer(database.Name, typeof(T).Name);
                    break;
                }
            }

            if (_container is null)
            {
                string errorMessage = $"Could not create container for {typeof(T).Name}";
                _logger.LogCritical("errorMessage");
                throw new NullReferenceException(errorMessage);
            }
        }

        public Container Container
        {
            get
            {
                return _container;
            }
        }

        private enum Operation
        {
            Create,
            Upsert,
            Delete
        }
    }
}
