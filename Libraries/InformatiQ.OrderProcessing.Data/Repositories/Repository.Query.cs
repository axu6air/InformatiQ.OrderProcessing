using InformatiQ.OrderProcessing.Core.Entities;
using InformatiQ.OrderProcessing.Core.Infrastructure;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using System.Net;

namespace InformatiQ.OrderProcessing.Data.Repositories
{
    public partial class Repository<T> : IRepository<T> where T : IContainerEntity
    {
        public async ValueTask<(List<T> results, HttpStatusCode statusCode)> GetAsync(
           List<string> ids,
           CancellationToken cancellationToken = default, bool exceptsDeleted = false)
        {
            List<(string, PartitionKey)> items = new();

            foreach (var id in ids)
            {
                items.Add((id, new PartitionKey(id)));
            }

            IReadOnlyList<(string, PartitionKey)> values = items;

            return await GetManyAsync(values, cancellationToken, exceptsDeleted);
        }

        public async ValueTask<(List<T> results, HttpStatusCode statusCode)> GetAsync(
            IReadOnlyList<(string, PartitionKey)> ids,
            CancellationToken cancellationToken = default, bool exceptsDeleted = false) => await GetManyAsync(ids, cancellationToken, exceptsDeleted);

        private async ValueTask<(List<T> results, HttpStatusCode statusCode)> GetManyAsync(
            IReadOnlyList<(string, PartitionKey)> ids,
            CancellationToken cancellationToken,
            bool exceptsDeleted = false
            )
        {
            var response = await _container.ReadManyItemsAsync<T>(items: ids, cancellationToken: cancellationToken)
                          .ConfigureAwait(false);

            return (response.ToList(), response.StatusCode);
        }

        public async ValueTask<(T result, HttpStatusCode statusCode)> GetAsync(
            string id,
            string partitionKeyValue = null,
            CancellationToken cancellationToken = default, bool exceptsDeleted = false) =>
            await GetAsync(id, new PartitionKey(partitionKeyValue ?? id), cancellationToken, exceptsDeleted);

        public async ValueTask<(T result, HttpStatusCode statusCode)> GetAsync(
             string id,
             PartitionKey partitionKey,
             CancellationToken cancellationToken = default,
             bool exceptsDeleted = false) => await GetAsync(id, partitionKey, throwErrorIfNotFound: true, cancellationToken, exceptsDeleted);

        public async ValueTask<(T result, HttpStatusCode statusCode)> GetAsync(
             string id,
             PartitionKey partitionKey,
             bool throwErrorIfNotFound,
             CancellationToken cancellationToken = default, bool exceptsDeleted = false)
        {
            try
            {
                if (partitionKey == default)
                    partitionKey = new PartitionKey(id);

                ItemResponse<T> response = await _container
                    .ReadItemAsync<T>(id, partitionKey, cancellationToken: cancellationToken);

                T item = response.Resource;

                return (item, response.StatusCode);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                if (throwErrorIfNotFound)
                    throw new NullEntityException($"{typeof(T).Name} for {id} is not found");
                else
                    return (default, HttpStatusCode.NotFound);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
            {
                _logger.LogCritical(ex, ex.Message, "Cosmos threw a BadRequest response");
                return (default, HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex.Message);
                throw;
            }
        }

        public async ValueTask<(T result, HttpStatusCode statusCode)> GetCacheAsync(
             string id,
             PartitionKey partitionKey = default,
             bool throwErrorIfNotFound = false,
             int cacheminutes = 30,
             CancellationToken cancellationToken = default)
        {
            try
            {
                if (partitionKey == default)
                    partitionKey = new PartitionKey(id);

                var itemRequestOptions = new ItemRequestOptions
                {
                    ConsistencyLevel = ConsistencyLevel.Eventual,
                    DedicatedGatewayRequestOptions = new DedicatedGatewayRequestOptions
                    {
                        MaxIntegratedCacheStaleness = TimeSpan.FromMinutes(cacheminutes),
                    },
                };

                ItemResponse<T> response = await _container
                    .ReadItemAsync<T>(id, partitionKey, cancellationToken: cancellationToken, requestOptions: itemRequestOptions);

                T item = response.Resource;

                return (item, response.StatusCode);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                if (throwErrorIfNotFound)
                    throw new NullEntityException($"{typeof(T).Name} for {id} is not found");
                else
                    return (default, HttpStatusCode.NotFound);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
            {
                _logger.LogCritical(ex, ex.Message, "Cosmos threw a BadRequest response");
                return (default, HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex.Message);
                throw;
            }
        }

        // TODO: Need rework when returnFirstOrDefault is true 
        public async ValueTask<(List<T> results, HttpStatusCode statusCode)> GetAsync(
            Expression<Func<T, bool>> predicate,
            PartitionKey partitionKey = default,
            CancellationToken cancellationToken = default,
            bool exceptsDeleted = false,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            bool returnFirstOrDefault = false)
        {
            try
            {
                if (partitionKey == default)
                    partitionKey = new PartitionKey("/id");

                var results = new List<T>();

                var query = _container.GetItemLinqQueryable<T>(
                              requestOptions: new QueryRequestOptions
                              {
                                  PartitionKey = partitionKey,
                                  MaxItemCount = 100000
                              }
                          )
                          .Where(predicate);

                if (orderBy != null)
                    query = orderBy(query);

                var feedIterator = _container.GetItemQueryIterator<T>(query.ToQueryDefinition());

                while (feedIterator.HasMoreResults)
                {
                    var data = await feedIterator.ReadNextAsync(cancellationToken);
                    var requstCharge = data.RequestCharge;

                    if (returnFirstOrDefault && data.Count > 0)
                    {
                        results.Add(data.Take(1).First());
                        break;
                    }

                    results.AddRange(data);
                }

                return (results, HttpStatusCode.OK);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return (new List<T>(), HttpStatusCode.NotFound);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
            {
                _logger.LogCritical(ex, ex.Message, "Cosmos threw a BadRequest response");
                return (new List<T>(), HttpStatusCode.NotFound);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async ValueTask<(List<T> item, HttpStatusCode httpStatusCode)> GetAsync(
            Expression<Func<T, bool>> predicate,
            int returnItems,
            PartitionKey partitionKey = default,
            CancellationToken cancellationToken = default,
            bool exceptsDeleted = false,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null)
        {
            try
            {
                if (partitionKey == default)
                    partitionKey = new PartitionKey("/id");

                if (returnItems == 0)
                    returnItems = 1000;

                var results = new List<T>();

                var query = _container.GetItemLinqQueryable<T>(
                              requestOptions: new QueryRequestOptions
                              {
                                  PartitionKey = partitionKey,
                                  MaxItemCount = 100000
                              }
                          )
                          .Where(predicate);

                if (orderBy != null)
                    query = orderBy(query);

                query = query.Skip(0).Take(returnItems);

                var feedIterator = _container.GetItemQueryIterator<T>(query.ToQueryDefinition());

                while (feedIterator.HasMoreResults)
                {
                    var data = await feedIterator.ReadNextAsync(cancellationToken);
                    results.AddRange(data);
                }

                return (results, HttpStatusCode.OK);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return (new List<T>(), HttpStatusCode.NotFound);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
            {
                _logger.LogCritical(ex, ex.Message, "Cosmos threw a BadRequest response");
                return (new List<T>(), HttpStatusCode.NotFound);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async ValueTask<(List<T> results, HttpStatusCode statusCode)> GetByQueryAsync(
            string query,
            QueryRequestOptions requestOptions = null,
            CancellationToken cancellationToken = default
            ) =>
            await GetByQueryAsync(new QueryDefinition(query), requestOptions, cancellationToken);


        public async ValueTask<(List<T> results, HttpStatusCode statusCode)> GetByQueryAsync(
            QueryDefinition queryDefinition,
            QueryRequestOptions requestOptions = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var queryResultSetIterator = _container.GetItemQueryIterator<T>(queryDefinition, requestOptions: requestOptions);

                List<T> entities = new();

                while (queryResultSetIterator.HasMoreResults)
                {
                    var response = await queryResultSetIterator.ReadNextAsync(cancellationToken);
                    foreach (var item in response)
                        entities.Add(item);
                }

                return (entities, HttpStatusCode.OK);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return (new List<T>(), HttpStatusCode.NotFound);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
            {
                _logger.LogCritical(ex, ex.Message, "Cosmos threw a BadRequest response");
                return (new List<T>(), HttpStatusCode.NotFound);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async ValueTask<(bool exist, HttpStatusCode statusCode)> Exists(Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            IQueryable<T> query =
                _container.GetItemLinqQueryable<T>()
                    .Where(predicate);

            var response = await query.CountAsync(cancellationToken);

            return response.Resource > 0 ? (true, response.StatusCode) : (false, response.StatusCode);
        }

        public async ValueTask<(int count, HttpStatusCode statusCode)> CountAsync(CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = _container.GetItemLinqQueryable<T>();

            var response = await query.CountAsync(cancellationToken: cancellationToken);

            return (response.Resource, response.StatusCode);
        }

        public async ValueTask<(int count, HttpStatusCode statusCode)> CountAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            IQueryable<T> query =
                _container.GetItemLinqQueryable<T>()
                    .Where(predicate);

            var response = await query.CountAsync(cancellationToken);

            return (response.Resource, response.StatusCode);
        }
    }
}
