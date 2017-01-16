
namespace AzureManagmentLibPre.ConsoleHost
{
    using Microsoft.Azure;
    using Microsoft.Azure.Management.Resources;
    using Microsoft.Azure.Management.Resources.Models;
    using Microsoft.WindowsAzure.Management.Models;
    using System;
    using System.Threading.Tasks;

    internal class ResourceBuilder
    {
        private readonly ResourceManagementClient resourceManager;

        public ResourceBuilder(SubscriptionCloudCredentials credential)
        {
            var resourceManagementClient = new ResourceManagementClient(credential) ; 
            Console.WriteLine("Registrando los providers...");
            var rpResult = resourceManagementClient.Providers.Register("Microsoft.Storage");
            Console.WriteLine($"Microsoft.Storage: {rpResult.StatusCode}");
            rpResult = resourceManagementClient.Providers.Register("Microsoft.Network");
            Console.WriteLine($"Microsoft.Network: { rpResult.StatusCode}");
            rpResult = resourceManagementClient.Providers.Register("Microsoft.Compute");
            Console.WriteLine($"Microsoft.Compute: {rpResult.StatusCode}");
            this.resourceManager = resourceManagementClient;
        }

        internal StepResult CreateResourceGroup(string resourceName)
        {
            try
            {
                Console.WriteLine("Creando el resource group...");
                var free = this.resourceManager.ResourceGroups.CheckExistence(resourceName);
                if (!free.Exists)
                {
                    var resourceGroup = new ResourceGroup { Location = LocationNames.WestEurope };
                    var result = this.resourceManager.ResourceGroups.CreateOrUpdateAsync(resourceName, resourceGroup).Result;
                    Console.WriteLine($"Creado el resoruce group: {result.StatusCode}");
                    return new StepResult() { Succed = true, Message = "ResourceGroup creado" };
                }
                else
                {
                    Console.WriteLine("ResourceGroup ya existente. ¿Proseguir con este?");
                    var result = Console.ReadLine() == "y";
                    return new StepResult() { Succed = result, Message = "Nombre ya utilizado" };
                }
            }
            catch (Exception ex)
            {
                return new StepResult() { Succed = false, Message = $"ResourceGroup non creado: {ex.Message}" };
            }
        }

        internal async Task<StepResult> DeleteResourceGroupAsync(string groupName)
        {
            try
            {
                Console.WriteLine("Eliminando el resource group...");
                var res = await this.resourceManager.ResourceGroups.DeleteAsync(groupName).ConfigureAwait(false);
                return new StepResult() { Succed = true, Message = "Elimimado el resource group" };
            }
            catch (Exception ex)
            {
                return new StepResult() { Succed = false, Message = $"Resource group no eliminado: {ex.Message}" };
            }
        }
    }
}
