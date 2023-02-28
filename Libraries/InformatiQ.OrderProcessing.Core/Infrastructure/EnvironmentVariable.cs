using Newtonsoft.Json;

namespace InformatiQ.OrderProcessing.Core.Infrastructure
{
    public class EnvironmentVariable<T>
    {
        public static T GetServiceConfiguration(string configuration)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(configuration);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
