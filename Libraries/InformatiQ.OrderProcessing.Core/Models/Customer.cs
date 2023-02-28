using InformatiQ.OrderProcessing.Core.Entities;

namespace InformatiQ.OrderProcessing.Core.Models
{
    public class Customer : ContainerEntity
    {
        public string Name { get; set; }

        public string Email { get; set; }
    }
}
