using InformatiQ.OrderProcessing.Core.Models;

namespace InformatiQ.OrderProcessing.Service.Customers
{
    public class CustomerService : ICustomerService
    {
        public Customer GetCustomer(string id)
        {
            var customerNumber = new Random().Next();

            return new Customer()
            {
                Id = id,
                Name = $"Test Customer {customerNumber}",
                Email = $"test.customer{customerNumber}@email.com",
                CreatedAtUtc = DateTime.UtcNow,
            };
        }
    }
}
