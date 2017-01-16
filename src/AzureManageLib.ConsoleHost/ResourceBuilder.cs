
namespace AzureManageLib.ConsoleHost
{
    using Microsoft.Azure;
    using Microsoft.Azure.Management.Resources;
    using Microsoft.Azure.Management.Resources.Models;
    using Microsoft.WindowsAzure.Management.Models;
    using System;
    using System.Configuration;

    internal static class ResourceBuilder
    {
        private static ResourceManagementClient ResourceManager;

        internal static void GetReosurceClient(TokenCloudCredentials credential)
        {
            var resourceManagementClient = new ResourceManagementClient(credential);
            Console.WriteLine("Registrando los providers...");
            var rpResult = resourceManagementClient.Providers.Register("Microsoft.Storage");
            Console.WriteLine($"Microsoft.Storage: {rpResult.StatusCode}");
            rpResult = resourceManagementClient.Providers.Register("Microsoft.Network");
            Console.WriteLine($"Microsoft.Network: { rpResult.StatusCode}");
            rpResult = resourceManagementClient.Providers.Register("Microsoft.Compute");
            Console.WriteLine($"Microsoft.Compute: {rpResult.StatusCode}");
            ResourceBuilder.ResourceManager = resourceManagementClient;
        }

        internal static StepResult CreateResourceGroup()
        {
            try
            {
                Console.WriteLine("Creando el resource group...");
                var name = ConfigurationManager.AppSettings["groupName"];
                var free = ResourceBuilder.ResourceManager.ResourceGroups.CheckExistence(name);
                if (!free.Exists)
                {
                    var resourceGroup = new ResourceGroup { Location = LocationNames.WestEurope };
                    var result = ResourceBuilder.ResourceManager.ResourceGroups.CreateOrUpdateAsync(name, resourceGroup).Result;
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
    }
}
