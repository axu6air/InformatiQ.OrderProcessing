namespace InformatiQ.OrderProcessing.Data.Configurations
{

    public interface ICosmosDbDataServiceConfiguration
    {
        string ConnectionString { get; set; }
        List<Database> Databases { get; set; }
        //string Budget { get; set; }
        string MaxRetryAttempts { get; set; }
        string MaxRetryWaitTime { get; set; }
    }
}
