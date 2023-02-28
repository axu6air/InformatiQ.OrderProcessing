using InformatiQ.OrderProcessing.Core.Models;

namespace InformatiQ.OrderProcessing.Service.Customers
{
    public interface ICustomerService
    {
        public Customer GetCustomer(string id);
    }
}
