using Microsoft.Extensions.Options;

namespace InformatiQ.OrderProcessing.Data.Configurations
{
    public class CosmosDbDataServiceConfigurationValidation : IValidateOptions<CosmosDbDataServiceConfiguration>
    {
        public ValidateOptionsResult Validate(string name, CosmosDbDataServiceConfiguration options)
        {

            if (string.IsNullOrEmpty(options.ConnectionString))
                return ValidateOptionsResult.Fail($"{nameof(options.ConnectionString)} configuration parameter for the Azure Cosmos Db connection String is required");

            if (options.Databases.Count == 0)
                return ValidateOptionsResult.Fail("Configuration parameter for at least one database is required");

            foreach (var database in options.Databases)
            {
                if (database.Containers.Count == 0)
                    return ValidateOptionsResult.Fail($"At least one container is required for {database.Name}");


                if (database.IsProvisioned == null)
                {
                    return ValidateOptionsResult.Fail($"IsProvisioned property configuration parameter for {database.Name} is required");
                }
                else
                {
                    if (database.IsProvisioned.Value == true)
                    {
                        if (database.IsAutoscaled == null)
                            return ValidateOptionsResult.Fail($"IsAutoscaled property configuration parameter for {database.Name} is required because IsProvisioned property value is true");
                        else
                        {
                            if (database.IsAutoscaled.Value == true)
                            {
                                if (database.MaxThroughPut == null)
                                    return ValidateOptionsResult.Fail($"MaxThroughPut property configuration parameter for {database.Name} is required because IsAutoscaled property value is true");
                                else if (int.TryParse(database.MaxThroughPut, out int maxRu) && maxRu < 1000)
                                    return ValidateOptionsResult.Fail($"MaxThroughPut property configuration parameter for {database.Name} is greater or equal to 1000");
                            }
                            else if (database.IsAutoscaled == false)
                            {
                                if (database.RequestUnits == null)
                                {
                                    return ValidateOptionsResult.Fail($"RequestUnits property configuration parameter for {database.Name} is required because IsProvisioned property value is true and IsAutoScaled property value is false");

                                }
                            }
                        }

                    }

                    else if (database.IsProvisioned.Value == false)
                    {
                        foreach (var container in database.Containers)
                        {
                            if (container.IsAutoscaled != null && container.IsAutoscaled == true)
                            {
                                if (container.MaxThroughPut == null)
                                    return ValidateOptionsResult.Fail($"MaxThroughPut property configuration parameter for {container.Name} is required because IsAutoscaled property value is true for the container");
                                if (int.TryParse(container.MaxThroughPut, out int maxRu) && maxRu < 1000)
                                    return ValidateOptionsResult.Fail($"MaxThroughPut property configuration parameter for {container.Name} must be greater or egual to 1000");
                            }
                            else if (container.IsAutoscaled.Value == false || container.IsAutoscaled == null)
                            {
                                if (container.RequestUnits == null)
                                    return ValidateOptionsResult.Fail($"RequestUnits property configuration parameter for {container.Name} is required because IsAutoscaled property is null or false for the container");
                            }

                            if (!ValidateTimeToLiveInput(container))
                                return ValidateOptionsResult.Fail("TimeToLive property configuration parameter for each container is required or invalid (integers > 0 or -1)");
                        }
                    }
                }



                if (!ValidatePartitionKeyInput(database))
                    return ValidateOptionsResult.Fail("Configuration parameter for all container's partition key path is required");

            }

            if (string.IsNullOrEmpty(options.MaxRetryAttempts))
                return ValidateOptionsResult.Fail($"{nameof(options.MaxRetryAttempts)} property configuration parameter for number of retries upon reaching rate limit is required");
            if (string.IsNullOrEmpty(options.MaxRetryWaitTime))
                return ValidateOptionsResult.Fail($"{nameof(options.MaxRetryWaitTime)} property configuration parameter for retry wait time upon reaching rate limit is required");

            return ValidateOptionsResult.Success;
        }

        private bool ValidateRequestUnitsInput(Database database)
        {
            if (string.IsNullOrEmpty(database.RequestUnits) || !int.TryParse(database.RequestUnits, out int _))
            {
                foreach (var container in database.Containers)
                {
                    if (string.IsNullOrEmpty(container.RequestUnits) || !int.TryParse(container.RequestUnits, out int _))
                        return false;
                }
            }

            return true;
        }

        private bool ValidateTimeToLiveInput(CosmosContainer container)
        {
            if (string.IsNullOrEmpty(container.TimeToLive) || !int.TryParse(container.TimeToLive, out int containerTimeToLive))
                return false;
            if (containerTimeToLive < -1 || containerTimeToLive == 0) return false;

            return true;
        }

        private bool ValidateRequestUnitsForDatabase(Database database)
        {
            if (string.IsNullOrEmpty(database.RequestUnits) || !int.TryParse(database.RequestUnits, out int _))
                return false;

            return true;
        }

        private bool ValidatePartitionKeyInput(Database database)
        {
            return database.Containers.Any(x => !string.IsNullOrEmpty(x.PartitionKeyPath));
        }

        private bool HasIsAutoScaledForContainer(Database database)
        {
            return database.Containers.Any(x => x.IsAutoscaled != null);
        }
    }
}
