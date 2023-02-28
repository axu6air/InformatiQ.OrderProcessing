using Microsoft.Extensions.Logging;

namespace InformatiQ.OrderProcessing.Core.Infrastructure
{
    [Serializable]
    public class NullEntityException : Exception
    {
        private readonly ILogger _logger;

        public NullEntityException(string message) : base(message)
        {
            _logger = LoggerCore.GetLogger<NullEntityException>();
            _logger.LogError(message);
        }
    }
}
