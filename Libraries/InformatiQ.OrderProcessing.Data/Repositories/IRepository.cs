using InformatiQ.OrderProcessing.Core.Entities;
using Microsoft.Azure.Cosmos;
using System.Linq.Expressions;
using System.Net;

namespace InformatiQ.OrderProcessing.Data.Repositories
{
    public interface IRepository<T> where T : IContainerEntity
    {
        Container Container { get; }

        ValueTask<(List<T> results, HttpStatusCode statusCode)> GetAsync(
            List<string> ids,
            CancellationToken cancellationToken = default, bool exceptsDeleted = false);

        ValueTask<(List<T> results, HttpStatusCode statusCode)> GetAsync(
            IReadOnlyList<(string, PartitionKey)> ids,
            CancellationToken cancellationToken = default, bool exceptsDeleted = false);

        ValueTask<(T result, HttpStatusCode statusCode)> GetAsync(
               string id,
               string partitionKeyValue = null,
               CancellationToken cancellationToken = default, bool exceptsDeleted = false);

        ValueTask<(T result, HttpStatusCode statusCode)> GetAsync(
            string id,
            PartitionKey partitionKey,
            CancellationToken cancellationToken = default, bool exceptsDeleted = false);

        ValueTask<(T result, HttpStatusCode statusCode)> GetAsync(
             string id,
             PartitionKey partitionKey,
             bool throwErrorIfNotFound,
             CancellationToken cancellationToken = default, bool exceptsDeleted = false);

        ValueTask<(List<T> results, HttpStatusCode statusCode)> GetAsync(
                   Expression<Func<T, bool>> predicate,
                   PartitionKey partitionKey = default,
                   CancellationToken cancellationToken = default,
                   bool exceptsDeleted = false,
                   Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
                   bool returnFirstOrDefault = false);

        ValueTask<(T result, HttpStatusCode statusCode)> GetCacheAsync(
             string id,
             PartitionKey partitionKey = default,
             bool throwErrorIfNotFound = false,
             int cacheminutes = 30,
             CancellationToken cancellationToken = default);

        ValueTask<(List<T> results, HttpStatusCode statusCode)> GetByQueryAsync(
            string query,
            QueryRequestOptions requestOptions = null,
            CancellationToken cancellationToken = default
            );

        ValueTask<(List<T> results, HttpStatusCode statusCode)> GetByQueryAsync(
            QueryDefinition queryDefinition,
            QueryRequestOptions requestOptions = null,
            CancellationToken cancellationToken = default);

        ValueTask<(T result, HttpStatusCode statusCode)> CreateAsync(
            T value,
            CancellationToken cancellationToken = default);

        ValueTask<List<(T result, HttpStatusCode statusCode)>> CreateAsync(
            List<T> values,
            CancellationToken cancellationToken = default);

        ValueTask<(T result, HttpStatusCode statusCode)> PatchAsync(
            string id,
            IReadOnlyList<PatchOperation> patchOperations,
            PatchItemRequestOptions patchItemRequestOptions = default,
            PartitionKey partitionKey = default,
            CancellationToken cancellationToken = default,
            bool ignoreEtag = false);

        ValueTask<(T result, HttpStatusCode statusCode)> UpdateAsync(
            T value,
            CancellationToken cancellationToken = default,
            bool isUpdate = true,
            bool ignoreEtag = false);

        ValueTask<List<(T result, HttpStatusCode statusCode)>> UpdateAsync(
             List<T> values,
             CancellationToken cancellationToken = default,
             bool ignoreEtag = false);

        ValueTask<HttpStatusCode> DeleteAsync(
            T value,
            CancellationToken cancellationToken = default);

        ValueTask<HttpStatusCode> DeleteAsync(
            string id,
            string partitionKeyValue = null,
            CancellationToken cancellationToken = default);

        ValueTask<HttpStatusCode> DeleteAsync(
            string id,
            PartitionKey partitionKey,
            CancellationToken cancellationToken = default);


        ValueTask<(bool exist, HttpStatusCode statusCode)> ExistsAsync(string id, string partitionKeyValue = null,
            CancellationToken cancellationToken = default);

        ValueTask<(bool exist, HttpStatusCode statusCode)> ExistsAsync(string id, PartitionKey partitionKey,
            CancellationToken cancellationToken = default);

        ValueTask<(bool exist, HttpStatusCode statusCode)> Exists(Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default);

        ValueTask<(int count, HttpStatusCode statusCode)> CountAsync(CancellationToken cancellationToken = default);

        ValueTask<(int count, HttpStatusCode statusCode)> CountAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default);

        ValueTask<(List<T> item, HttpStatusCode httpStatusCode)> GetAsync(
            Expression<Func<T, bool>> predicate,
            int returnItems,
            PartitionKey partitionKey = default,
            CancellationToken cancellationToken = default,
            bool exceptsDeleted = false,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null);
    }
}
