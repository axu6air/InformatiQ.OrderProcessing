using AutoMapper;
using InformatiQ.OrderProcessing.Core.Models;
using InformatiQ.OrderProcessing.Workflow.Models;

namespace InformatiQ.OrderProcessing.Workflow.Mappers
{
    public class OrderProcessingMapper : Profile
    {
        public OrderProcessingMapper()
        {
            CreateMap<OrderModel, Order>().ReverseMap();
            CreateMap<OrderItemModel, OrderItem>().ReverseMap();
        }
    }

    public static class CustomMapper
    {
        private static readonly Lazy<IMapper> Lazy = new(() =>
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<OrderProcessingMapper>();
            });
            return config.CreateMapper();
        });

        public static IMapper Mapper => Lazy.Value;
    }
}
