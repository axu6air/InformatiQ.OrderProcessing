namespace InformatiQ.OrderProcessing.Core.Entities
{
    public interface IEntity
    {
        string Id { get; set; }
        DateTime CreatedAtUtc { get; set; }
        DateTime? UpdatedAtUtc { get; set; }
    }
}