
namespace AzureManageLib.ConsoleHost
{
    using Microsoft.Azure;
    using Microsoft.WindowsAzure.Management.Compute;
    using Microsoft.WindowsAzure.Management.Compute.Models;
    using Microsoft.WindowsAzure.Management.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    internal class CloudServiceBuilder
    {
        private readonly SubscriptionCloudCredentials credentials;

        internal CloudServiceBuilder(SubscriptionCloudCredentials credentials)
        {
            this.credentials = credentials;
        }

        internal async Task<StepResult> CreateCloudServiceAsync(string serviceName)
        {
            try
            {
                using (var computeClient = new ComputeManagementClient(this.credentials))
                {
                    var free = await computeClient.HostedServices.CheckNameAvailabilityAsync(serviceName).ConfigureAwait(false);
                    if (free.IsAvailable)
                    {
                        await computeClient.HostedServices.CreateAsync(
                          new HostedServiceCreateParameters
                          {
                              Label = "azurelibservice",
                              Location = LocationNames.WestEurope,
                              ServiceName = serviceName
                          }).ConfigureAwait(false);
                        return new StepResult() { Succed = true, Message = "Creado el servicio" };
                    }

                    Console.WriteLine("Servicio ya existente. ¿Proseguir con este?");
                    var result = Console.ReadLine() == "y";
                    return new StepResult() { Succed = result, Message = "Nombre ya utilizado" };
                }
            }
            catch (Exception ex)
            {
                return new StepResult() { Succed = false, Message = $"Servicio no creado: {ex.Message}" };
            }
        }

        internal async Task<string> DeleteCloudServiceAsync(string serviceName)
        {
            try
            {
                using (var computeClient = new ComputeManagementClient(this.credentials))
                {
                    await computeClient.HostedServices.DeleteAsync(serviceName).ConfigureAwait(false);
                }

                return "Eliminado el servicio";
            }
            catch (Exception ex)
            {
                return $"Servicio no elinimado: {ex.Message}";
            }
        }
    }
}
