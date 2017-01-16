
namespace AzureManagmentLibPre.ConsoleHost
{
    using Microsoft.Azure.Management.Compute;
    using Microsoft.Azure.Management.Compute.Models;
    using Microsoft.Rest;
    using Microsoft.WindowsAzure.Management.Models;
    using System;
    using System.Threading.Tasks;

    internal class CloudServiceBuilder
    {
        private readonly TokenCredentials credentials;
        private readonly string subscriptionId;

        internal CloudServiceBuilder(TokenCredentials credentials, string subscriptionId)
        {
            this.credentials = credentials;
            this.subscriptionId = subscriptionId;
        }

        internal async Task<StepResult> CreateAvailabilityAsync(string avsetName, string groupName)
        {
            try
            {
                using (var computeClient = new ComputeManagementClient(this.credentials))
                {
                    Console.WriteLine("Creando el availability set...");
                    var computeManagementClient = new ComputeManagementClient(this.credentials) { SubscriptionId = subscriptionId };
                    var res =  await computeManagementClient.AvailabilitySets.CreateOrUpdateAsync(
                      groupName,
                      avsetName,
                      new AvailabilitySet()
                      {
                          Location = LocationNames.WestEurope
                      }
                    ).ConfigureAwait(false);
                }

                return new StepResult() { Succed = true, Message = "Availability set creado" };
            }
            catch (Exception ex)
            {
                return new StepResult() { Succed = false, Message = $"Availability set no creado: {ex.Message}" };
            }
        }
    }
}
