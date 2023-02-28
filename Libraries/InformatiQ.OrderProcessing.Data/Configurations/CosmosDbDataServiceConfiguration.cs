namespace InformatiQ.OrderProcessing.Data.Configurations
{
    public class CosmosDbDataServiceConfiguration : ICosmosDbDataServiceConfiguration
    {
        public const string CosmosDbSettings = "COSMOSDB_CONNECTIONSETTINGS";
        public string ConnectionString { get; set; }
        public List<Database> Databases { get; set; } = new();
        public string MaxRetryAttempts { get; set; }
        public string MaxRetryWaitTime { get; set; }
    }

    public class CosmosContainer
    {
        public string Name { get; set; }
        public string PartitionKeyPath { get; set; }
        public string TimeToLive { get; set; }
        public bool? IsProvisioned { get; set; }
        public bool? IsAutoscaled { get; set; }
        public string RequestUnits { get; set; }
        public string MaxThroughPut { get; set; }
    }

    public class Database
    {
        public string Name { get; set; }
        public bool? IsProvisioned { get; set; }
        public bool? IsAutoscaled { get; set; }
        public string RequestUnits { get; set; }
        public string MaxThroughPut { get; set; }
        public List<CosmosContainer> Containers { get; set; } = new();
    }

}
