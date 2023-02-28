using InformatiQ.OrderProcessing.Service.Customers;
using Microsoft.Extensions.DependencyInjection;

namespace InformatiQ.OrderProcessing.Service
{
    public static class DomainServiceExtension
    {
        public static IServiceCollection AddDomainConfigurationService(this IServiceCollection services)
        {
            services.AddTransient<IOrderService, OrderService>();
            services.AddTransient<ICustomerService, CustomerService>();
            return services;
        }
    }
}
