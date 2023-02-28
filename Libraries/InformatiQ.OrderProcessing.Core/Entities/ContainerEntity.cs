namespace InformatiQ.OrderProcessing.Core.Entities
{
    public abstract class ContainerEntity : IContainerEntity
    {
        public virtual string Id { get; set; } = Guid.NewGuid().ToString();

        string IContainerEntity.PartitionKey => GetPartitionKeyValue();

        protected virtual string GetPartitionKeyValue() => Id;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }
    }
}
