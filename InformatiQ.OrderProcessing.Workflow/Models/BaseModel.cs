namespace InformatiQ.OrderProcessing.Workflow.Models
{
    public class BaseModel
    {
        public virtual string Id { get; set; } = Guid.NewGuid().ToString();
    }
}
