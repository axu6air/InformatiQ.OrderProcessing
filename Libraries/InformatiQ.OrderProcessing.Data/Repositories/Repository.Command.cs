using InformatiQ.OrderProcessing.Core.Entities;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace InformatiQ.OrderProcessing.Data.Repositories
{
    public partial class Repository<T> : IRepository<T> where T : IContainerEntity
    {
        public async ValueTask<(T result, HttpStatusCode statusCode)> CreateAsync(
               T value,
               CancellationToken cancellationToken = default)
        {
            HttpStatusCode statusCode = HttpStatusCode.Created;
            try
            {
                value = GenerateCommonBaseEntity(value, operation: Operation.Create);
                var response = await _container.CreateItemAsync(value, new PartitionKey(value.PartitionKey),
                             cancellationToken: cancellationToken);

                statusCode = response.StatusCode;
                return (response.Resource, response.StatusCode);
            }
            catch (CosmosException ex)
            {
                statusCode = ex.StatusCode;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                GenerateLog(statusCode, _logger, value, Operation.Create);
            }

            return (default(T), statusCode);
        }

        public async ValueTask<List<(T result, HttpStatusCode statusCode)>> CreateAsync(
            List<T> values,
            CancellationToken cancellationToken = default)
        {
            List<(T item, HttpStatusCode statusCode)> result = new();

            foreach (var value in values)
            {
                result.Add(await CreateAsync(value, cancellationToken));
            }

            return result;
        }

        public async ValueTask<(T result, HttpStatusCode statusCode)> ReplaceAsync(
            T value,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var response =
                     await _container.ReplaceItemAsync<T>(value, value.Id, new PartitionKey(value.PartitionKey),
                             cancellationToken: cancellationToken);

                return (response.Resource, response.StatusCode);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return (default, ex.StatusCode);
            }
            catch (CosmosException)
            {
                throw;
            }
        }

        public async ValueTask<(T result, HttpStatusCode statusCode)> PatchAsync(
            string id,
            IReadOnlyList<PatchOperation> patchOperations,
            PatchItemRequestOptions patchItemRequestOptions = default,
            PartitionKey partitionKey = default,
            CancellationToken cancellationToken = default,
            bool ignoreEtag = false)
        {
            try
            {
                if (partitionKey == default)
                    partitionKey = new PartitionKey(id);

                ItemResponse<T> response =
                    await _container.PatchItemAsync<T>(id, partitionKey, patchOperations, patchItemRequestOptions, cancellationToken)
                    .ConfigureAwait(false);

                return (response.Resource, response.StatusCode);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return (default, ex.StatusCode);
            }
            catch (CosmosException)
            {
                throw;
            }
        }

        public async ValueTask<(T result, HttpStatusCode statusCode)> UpdateAsync(
            T value,
            CancellationToken cancellationToken = default,
            bool isUpdate = true,
            bool ignoreEtag = false)
        {
            HttpStatusCode statusCode = HttpStatusCode.OK;
            try
            {
                value = GenerateCommonBaseEntity(value, operation: isUpdate ? Operation.Upsert : Operation.Delete);

                ItemResponse<T> response =
                    await _container.UpsertItemAsync<T>(value, new PartitionKey(value.PartitionKey), null,
                            cancellationToken)
                        .ConfigureAwait(false);

                statusCode = response.StatusCode;
                return (response.Resource, statusCode);
            }
            catch (CosmosException ex)
            {
                statusCode = ex.StatusCode;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                GenerateLog(statusCode, _logger, value, Operation.Upsert);
            }

            return (default(T), statusCode);
        }

        public async ValueTask<List<(T result, HttpStatusCode statusCode)>> UpdateAsync(
            List<T> values,
            CancellationToken cancellationToken = default,
            bool ignoreEtag = false)
        {

            var updateTasks = new List<(T item, HttpStatusCode statusCode)>();

            foreach (var value in values)
                updateTasks.Add(await UpdateAsync(value, cancellationToken));

            return updateTasks;
        }

        public ValueTask<HttpStatusCode> DeleteAsync(
            T value,
            CancellationToken cancellationToken = default) =>
            DeleteAsync(value.Id, value.PartitionKey, cancellationToken);

        public async ValueTask<HttpStatusCode> DeleteAsync(
            string id,
            string partitionKeyValue = null,
            CancellationToken cancellationToken = default) =>
            await DeleteAsync(id, new PartitionKey(partitionKeyValue ?? id), cancellationToken);

        public async ValueTask<HttpStatusCode> DeleteAsync(
            string id,
            PartitionKey partitionKey,
            CancellationToken cancellationToken = default)
        {

            if (partitionKey == default)
                partitionKey = new PartitionKey(id);

            var response = await _container.DeleteItemAsync<T>(id, partitionKey, null, cancellationToken)
                .ConfigureAwait(false);

            return (response.StatusCode);

        }

        public ValueTask<(bool exist, HttpStatusCode statusCode)> ExistsAsync(string id, string partitionKeyValue = null,
            CancellationToken cancellationToken = default)
            => ExistsAsync(id, new PartitionKey(partitionKeyValue ?? id), cancellationToken);

        public async ValueTask<(bool exist, HttpStatusCode statusCode)> ExistsAsync(string id, PartitionKey partitionKey,
            CancellationToken cancellationToken = default)
        {
            if (partitionKey == default)
                partitionKey = new PartitionKey(id);

            try
            {
                var response = await _container.ReadItemAsync<T>(id, partitionKey, cancellationToken: cancellationToken)
                                   .ConfigureAwait(false);
            }
            catch (CosmosException e) when (e.StatusCode == HttpStatusCode.NotFound)
            {
                return (false, e.StatusCode);
            }

            return (true, HttpStatusCode.OK);
        }

        private static T GenerateCommonBaseEntity(T value, Operation operation)
        {
            switch (operation)
            {
                case Operation.Create:
                    value.CreatedAtUtc = DateTime.UtcNow;
                    break;
                case Operation.Upsert:
                    value.UpdatedAtUtc = DateTime.UtcNow;
                    break;
                case Operation.Delete:
                    break;
                default:
                    break;
            }

            return value;
        }

        private static void GenerateLog(HttpStatusCode httpStatusCode, ILogger logger, T value, Operation operation)
        {
            if (httpStatusCode == HttpStatusCode.Created || httpStatusCode == HttpStatusCode.OK)
                return;
            else
                logger.LogCritical($"Operation failed for {operation.ToString()} {typeof(T).Name}: {JsonSerializer.Serialize(value)}");
        }
    }
}
