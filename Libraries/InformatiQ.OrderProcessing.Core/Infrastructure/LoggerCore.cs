using Microsoft.Extensions.Logging;

namespace InformatiQ.OrderProcessing.Core.Infrastructure
{
    public class LoggerCore
    {
        public static ILogger GetLogger(string categoryName)
        {
            var loggerFactory = CreateFactory();

            ILogger logger = loggerFactory.CreateLogger(categoryName);

            return logger;
        }

        public static ILogger<T> GetLogger<T>() where T : class
        {
            var loggerFactory = CreateFactory();

            ILogger<T> logger = loggerFactory.CreateLogger<T>();

            return logger;
        }

        private static ILoggerFactory CreateFactory()
        {
            return LoggerFactory.Create(builder =>
            {
                builder.AddFilter("Microsoft", LogLevel.Warning)
                       .AddFilter("System", LogLevel.Warning)
                       .AddFilter("InformatiQ.OrderProcessing.Api", LogLevel.Debug);
            });
        }
    }
}
