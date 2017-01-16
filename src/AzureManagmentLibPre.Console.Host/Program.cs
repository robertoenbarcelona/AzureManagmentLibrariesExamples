
namespace AzureManagmentLibPre.ConsoleHost
{
    using Microsoft.Azure;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;
    using Microsoft.Rest;
    using System;
    using System.Configuration;

    public class Program
    {
        private static ResourceBuilder ResourceManager;

        static void Main(string[] args)
        {
            var token = GetAuthorizationHeader();
            var subscripcionId = ConfigurationManager.AppSettings["subscriptionId"];
            var proceed = CreateResourceGroup(new TokenCloudCredentials(subscripcionId, token));
            Console.WriteLine("¿Proseguir?");
            Console.ReadLine();
            if (!proceed) { return; }
            var credential = new TokenCredentials(token);
            ExecuteArmAPI(credential, subscripcionId);
            Console.WriteLine("¿Cierre?");
            Console.ReadLine();
        }

        private static void ExecuteArmAPI(TokenCredentials credential, string subscripcionId)
        {

            var proceed = CreateStorage(credential, subscripcionId);
            var deleting = true;
            Console.WriteLine("¿Proseguir?");
            Console.ReadLine();
            if (proceed)
            {
                proceed = CreateNetwork(credential, subscripcionId);
                Console.WriteLine("¿Proseguir?");
                Console.ReadLine();
            }

            if (proceed)
            {
                proceed = CreateCloudService(credential, subscripcionId);
                Console.WriteLine("¿Proseguir?");
                Console.ReadLine();
            }

            if (proceed)
            {
                Console.WriteLine("¿Proseguir?");
                CreateVirtuaMachine(credential, subscripcionId);
            }

            Console.WriteLine("¿Eliminar la VM?");
            deleting = Console.ReadLine() == "y";
        }

        private static bool CreateResourceGroup(SubscriptionCloudCredentials credential)
        {
            Program.ResourceManager = new ResourceBuilder(credential);
            var result = Program.ResourceManager.CreateResourceGroup(ConfigurationManager.AppSettings["groupName"]);
            Console.WriteLine(result.Message);
            return result.Succed;
        }

        private static bool DeleteResourceGroup()
        {
            var result = Program.ResourceManager.DeleteResourceGroupAsync(ConfigurationManager.AppSettings["groupName"]).Result;
            Console.WriteLine(result.Message);
            return result.Succed;
        }

        private static bool CreateStorage(TokenCredentials credential, string subscripcionId)
        {
            Console.WriteLine("Creando storage...");
            var storageBuilder = new StorageBuilder(credential, subscripcionId);
            var result = storageBuilder.CreateStorageAccountAsync(ConfigurationManager.AppSettings["storageName"], ConfigurationManager.AppSettings["groupName"]).Result;
            Console.WriteLine(result.Message);
            return result.Succed;
        }

        private static bool CreateNetwork(TokenCredentials credential, string subscripcionId)
        {
            Console.WriteLine("Creando network...");
            var networkBuilder = new VirtualNetworkBuilder(credential, subscripcionId);
            var result = networkBuilder.CreatePublicIPAddressAsync(ConfigurationManager.AppSettings["ipName"], ConfigurationManager.AppSettings["groupName"]).Result;
            Console.WriteLine(result.Message);
            if (!result.Succed) { return result.Succed; }
            result = networkBuilder.CreateVirtualNetworkAsync(ConfigurationManager.AppSettings["vnetName"], ConfigurationManager.AppSettings["subnetName"], ConfigurationManager.AppSettings["groupName"]).Result;
            Console.WriteLine(result.Message);
            if (!result.Succed) { return result.Succed; }
            result = networkBuilder.CreateNetworkInterfaceAsync(
                        ConfigurationManager.AppSettings["vnetName"],
                        ConfigurationManager.AppSettings["subnetName"],
                        ConfigurationManager.AppSettings["ipName"],
                        ConfigurationManager.AppSettings["nicName"],
                        ConfigurationManager.AppSettings["groupName"])
                    .Result;
            Console.WriteLine(result.Message);
            return result.Succed;
        }

        private static bool CreateCloudService(TokenCredentials credential, string subscripcionId)
        {
            Console.WriteLine("Creando el availability set...");
            var serviceBuilder = new CloudServiceBuilder(credential, subscripcionId);
            var result = serviceBuilder.CreateAvailabilityAsync(ConfigurationManager.AppSettings["availabilityName"], ConfigurationManager.AppSettings["groupName"]).Result;
            Console.WriteLine(result.Message);
            return result.Succed;
        }

        private static void CreateVirtuaMachine(TokenCredentials credential, string subscripcionId)
        {
            Console.WriteLine("Creando la virtual machine...");
            var vmBuilder = new VirtualMachineBuilder(credential, subscripcionId);
            var result = vmBuilder.CreateVirtualMachineAsync(
                    ConfigurationManager.AppSettings["adminName"],
                    ConfigurationManager.AppSettings["adminPassword"],
                    ConfigurationManager.AppSettings["vmName"],
                    ConfigurationManager.AppSettings["storageName"],
                    ConfigurationManager.AppSettings["nicName"],
                    ConfigurationManager.AppSettings["avsetName"],
                    ConfigurationManager.AppSettings["groupName"])
                    .Result;
            Console.WriteLine(result);
        }

        private static string GetAuthorizationHeader()
        {
            AuthenticationResult result = null;

            var context = new AuthenticationContext(string.Format(
              ConfigurationManager.AppSettings["login"],
              ConfigurationManager.AppSettings["tenantId"]));

            result = context.AcquireTokenAsync(
              ConfigurationManager.AppSettings["apiEndpoint"],
              ConfigurationManager.AppSettings["appId"],
              new Uri(ConfigurationManager.AppSettings["redirectUri"]),
              new PlatformParameters(PromptBehavior.Auto, null)).Result;

            return result.AccessToken;
        }
    }
}
