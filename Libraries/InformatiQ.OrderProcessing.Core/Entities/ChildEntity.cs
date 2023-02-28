namespace InformatiQ.OrderProcessing.Core.Entities
{
    public class ChildEntity : IChildEntity
    {
        public virtual string Id { get; set; } = Guid.NewGuid().ToString();

        protected virtual string GetPartitionKeyValue() => Id;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }

    }
}