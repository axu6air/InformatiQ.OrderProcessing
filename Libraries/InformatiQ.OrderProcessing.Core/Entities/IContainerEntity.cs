namespace InformatiQ.OrderProcessing.Core.Entities
{
    public interface IContainerEntity : IEntity
    {
        string PartitionKey { get; }
    }
}
